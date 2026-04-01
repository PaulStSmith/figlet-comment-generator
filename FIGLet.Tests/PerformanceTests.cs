using ByteForge.FIGLet;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace FIGLet.Tests;

[TestClass]
public class PerformanceTests
{
    private FIGFont _defaultFont;
    private FIGFont _testFont;
    private FIGLetRenderer _defaultRenderer;

    [TestInitialize]
    public void Initialize()
    {
        _defaultFont = FIGFont.Default;
        _testFont = TestUtilities.LoadTestFont("mini-fixed");
        _defaultRenderer = new FIGLetRenderer(_defaultFont);
    }

    [TestMethod]
    public void Performance_DefaultFontLoading_ShouldBeFast()
    {
        // This tests cached loading performance
        TestUtilities.AssertPerformance(() =>
        {
            for (var i = 0; i < 100; i++)
            {
                var font = FIGFont.Default;
                Assert.IsNotNull(font);
            }
        }, TimeSpan.FromMilliseconds(50), "Default font loading (100x)");
    }

    [TestMethod]
    public void Performance_FontFromStream_ShouldBeReasonable()
    {
        // Arrange
        var content = TestUtilities.CreateMinimalValidFontContent();
        
        TestUtilities.AssertPerformance(() =>
        {
            for (var i = 0; i < 10; i++)
            {
                using var stream = TestUtilities.CreateStreamFromString(content);
                var font = FIGFont.FromStream(stream);
                Assert.IsNotNull(font);
            }
        }, TimeSpan.FromSeconds(1), "Font loading from stream (10x)");
    }

    [TestMethod]
    public void Performance_SimpleTextRendering_ShouldBeFast()
    {
        // Arrange
        var text = "Hello World";
        
        TestUtilities.AssertPerformance(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                var result = _defaultRenderer.Render(text);
                Assert.IsNotNull(result);
            }
        }, TimeSpan.FromSeconds(2), "Simple text rendering (1000x)");
    }

    [TestMethod]
    public void Performance_LargeTextRendering_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var largeText = TestUtilities.GenerateLargeText(1000);
        
        TestUtilities.AssertPerformance(() =>
        {
            var result = _defaultRenderer.Render(largeText);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }, TimeSpan.FromSeconds(10), "Large text rendering (1000 chars)");
    }

    [TestMethod]
    public void Performance_DifferentLayoutModes_ShouldHaveComparablePerformance()
    {
        // Arrange
        var text = "Performance Test";
        var modes = new[] { LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing };
        var results = new Dictionary<LayoutMode, TimeSpan>();
        
        // Act & Measure
        foreach (var mode in modes)
        {
            var renderer = new FIGLetRenderer(_defaultFont) { LayoutMode = mode };
            var stopwatch = Stopwatch.StartNew();
            
            for (var i = 0; i < 100; i++)
            {
                var result = renderer.Render(text);
                Assert.IsNotNull(result);
            }
            
            stopwatch.Stop();
            results[mode] = stopwatch.Elapsed;
        }
        
        // Assert - All modes should complete within reasonable time
        foreach (var (mode, time) in results)
        {
            Assert.IsTrue(time < TimeSpan.FromSeconds(2), 
                $"{mode} took too long: {time}s");
        }
        
        // Log performance for comparison (informational)
        Console.WriteLine("Layout Mode Performance (100 iterations):");
        foreach (var (mode, time) in results.OrderBy(kvp => kvp.Value))
        {
            Console.WriteLine($"  {mode}: {time.TotalMilliseconds:F1}ms");
        }
    }

    [TestMethod]
    public void Performance_ANSIColorProcessing_ShouldNotSignificantlySlowDown()
    {
        // Arrange
        var normalText = "Color Test Normal";
        var coloredText = "\x1b[31mColor\x1b[32m Test\x1b[33m ANSI\x1b[0m";
        
        var normalRenderer = new FIGLetRenderer(_defaultFont) { UseANSIColors = false };
        var colorRenderer = new FIGLetRenderer(_defaultFont) { UseANSIColors = true };
        
        // Measure normal rendering
        var normalTime = MeasureRenderingTime(normalRenderer, normalText, 100);
        
        // Measure ANSI color rendering
        var colorTime = MeasureRenderingTime(colorRenderer, coloredText, 100);
        
        // Assert - Color processing shouldn't be more than 3x slower
        Assert.IsTrue(colorTime < normalTime * 3, 
            $"ANSI color processing too slow. Normal: {normalTime}, Color: {colorTime}");
    }

    [TestMethod]
    public void Performance_ParagraphMode_ShouldHandleMultipleLines()
    {
        // Arrange
        var multiLineText = string.Join("\n", Enumerable.Repeat("Test Line", 20));
        var singleLineRenderer = new FIGLetRenderer(_defaultFont) { ParagraphMode = false };
        var paragraphRenderer = new FIGLetRenderer(_defaultFont) { ParagraphMode = true };
        
        // Measure both modes
        var singleLineTime = MeasureRenderingTime(singleLineRenderer, multiLineText, 50);
        var paragraphTime = MeasureRenderingTime(paragraphRenderer, multiLineText, 50);
        
        // Assert - Both should complete in reasonable time
        Assert.IsTrue(singleLineTime < TimeSpan.FromSeconds(2), 
            $"Single line mode too slow: {singleLineTime}s");
        Assert.IsTrue(paragraphTime < TimeSpan.FromSeconds(5), 
            $"Paragraph mode too slow: {paragraphTime}s");
    }

    [TestMethod]
    public void Performance_SmushingCalculations_ShouldBeEfficient()
    {
        // Arrange
        var smushingFont = TestUtilities.LoadTestFont("smushing-test");
        var smushingRenderer = new FIGLetRenderer(smushingFont) { LayoutMode = LayoutMode.Smushing };
        var text = "Smushing Performance Test";
        
        // Measure both modes
        var smushingTime = MeasureRenderingTime(smushingRenderer, text, 100);
        
        // Assert - Smushing should complete in reasonable time (upper bound)
        Assert.IsTrue(smushingTime.TotalMilliseconds < 5000,
            $"Smushing too slow. Time: {smushingTime.TotalMilliseconds}ms");
    }

    [TestMethod]
    public void Performance_MemoryUsage_ShouldNotLeak()
    {
        // Arrange
        var text = "Memory Test";
        var initialMemory = GC.GetTotalMemory(true);
        
        // Act - Render many times to check for memory leaks
        for (var i = 0; i < 1000; i++)
        {
            var result = _defaultRenderer.Render(text);
            Assert.IsNotNull(result);
            
            // Force garbage collection every 100 iterations
            if (i % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        
        // Final cleanup
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryIncrease = finalMemory - initialMemory;
        
        // Assert - Memory increase should be reasonable (< 10MB for this test)
        Assert.IsTrue(memoryIncrease < 10 * 1024 * 1024, 
            $"Possible memory leak detected. Memory increased by {memoryIncrease:N0} bytes");
        
        Console.WriteLine($"Memory Usage Test (1000 iterations):");
        Console.WriteLine($"  Initial:  {initialMemory:N0} bytes");
        Console.WriteLine($"  Final:    {finalMemory:N0} bytes");
        Console.WriteLine($"  Increase: {memoryIncrease:N0} bytes");
    }

    [TestMethod]
    [DoNotParallelize]
    public void Performance_ConcurrentRendering_ShouldScaleWell()
    {
        // Arrange
        var text = TestUtilities.GenerateLargeText(100);
        var processorCount = Environment.ProcessorCount;
        var iterationsPerThread = 1000;
        
        // Measure sequential performance
        var sequentialTime = MeasureRenderingTime(_defaultRenderer, text, processorCount * iterationsPerThread);
        
        // Measure concurrent performance
        var concurrentStopwatch = Stopwatch.StartNew();
        Parallel.For(0, processorCount, t =>
        {
            var renderer = new FIGLetRenderer(_defaultFont);
            for (var i = 0; i < iterationsPerThread; i++)
            {
                var result = renderer.Render(text);
                Assert.IsNotNull(result);
            }
        });
        concurrentStopwatch.Stop();
        var concurrentTime = concurrentStopwatch.Elapsed;
        
        // Assert - Concurrent should be faster than sequential (with some tolerance)
        // Using Little's Law (L = λ * W) to model the system as an M/M/c queue:
        // - Service rate μ = total_renders / sequential_time (renders per ms)
        // - Max throughput λ_max = c * μ (c = processorCount)
        // - Theoretical concurrent time = total_renders / λ_max = sequential_time / c
        // - Theoretical speedup = c, so theoretical efficiency = 1
        // - Expect at least 15% of the theoretical efficiency (0.15) to account for overhead and thread pool limitations
        var efficiencyRatio = sequentialTime.TotalMilliseconds / concurrentTime.TotalMilliseconds;
        var efficiency = efficiencyRatio / processorCount;
        Assert.IsTrue(efficiency > 0.15, 
            $"Concurrent rendering not efficient. Sequential: {sequentialTime}s, Concurrent: {concurrentTime}s, Efficiency: {efficiency:F2} (speedup per processor), Efficiency ratio: {efficiencyRatio:F2} (total speedup), Thread count: {processorCount}");
    }

    [TestMethod]
    public void Performance_UnicodeText_ShouldHandleEfficiently()
    {
        // Arrange
        var asciiText = "ASCII Test Text";
        var unicodeText = "Unicode: éñ中文🚀🎉✨";
        
        // Measure ASCII rendering
        var asciiTime = MeasureRenderingTime(_defaultRenderer, asciiText, 200);
        
        // Measure Unicode rendering
        var unicodeTime = MeasureRenderingTime(_defaultRenderer, unicodeText, 200);
        
        // Assert - Unicode shouldn't be significantly slower
        Assert.IsTrue(unicodeTime < asciiTime * 3, 
            $"Unicode rendering too slow. ASCII: {asciiTime}s, Unicode: {unicodeTime}s");
    }

    [TestMethod]
    public void Performance_DifferentFontSizes_ShouldScaleReasonably()
    {
        // Arrange
        var smallFont = _testFont; // Height 3
        var largeFont = _defaultFont; // Height ~5
        var text = "Size Test";
        
        var smallRenderer = new FIGLetRenderer(smallFont);
        var largeRenderer = new FIGLetRenderer(largeFont);
        
        // Measure performance with different font sizes
        var smallTime = MeasureRenderingTime(smallRenderer, text, 200);
        var largeTime = MeasureRenderingTime(largeRenderer, text, 200);
        
        // Assert - Larger font shouldn't be excessively slower
        var sizeRatio = (double)largeFont.Height / smallFont.Height;
        var timeRatio = largeTime.TotalMilliseconds / smallTime.TotalMilliseconds;
        
        Assert.IsTrue(timeRatio < sizeRatio * 2, 
            $"Large font disproportionately slow. Size ratio: {sizeRatio}, Time ratio: {timeRatio}");
    }

    [TestMethod]
    public void Performance_StaticVsInstanceMethods_ShouldBeComparable()
    {
        // Arrange
        var text = "Method Test";
        var font = _defaultFont;
        var renderer = new FIGLetRenderer(font);
        
        // Measure static method performance
        var staticTime = Stopwatch.StartNew();
        for (var i = 0; i < 500; i++)
        {
            var result = FIGLetRenderer.Render(text, font);
            Assert.IsNotNull(result);
        }
        staticTime.Stop();
        
        // Measure instance method performance
        var instanceTime = Stopwatch.StartNew();
        for (var i = 0; i < 500; i++)
        {
            var result = renderer.Render(text);
            Assert.IsNotNull(result);
        }
        instanceTime.Stop();
        
        // Assert - Instance method should be faster or comparable (reuses renderer)
        Assert.IsTrue(instanceTime.Elapsed <= staticTime.Elapsed * 1.5, 
            $"Instance method unexpectedly slow. Static: {staticTime.Elapsed}s, Instance: {instanceTime.Elapsed}s");
    }

    [TestMethod]
    public void Performance_ZipFontLoading_ShouldNotBeExcessivelySlow()
    {
        // Arrange
        var fontContent = TestUtilities.CreateMinimalValidFontContent();
        var zipBytes = TestUtilities.CreateZipWithFontFile(fontContent);
        
        // Measure regular font loading
        var regularTime = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++)
        {
            using var stream = TestUtilities.CreateStreamFromString(fontContent);
            var font = FIGFont.FromStream(stream);
            Assert.IsNotNull(font);
        }
        regularTime.Stop();
        
        // Measure ZIP font loading
        var zipTime = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++)
        {
            using var stream = new MemoryStream(zipBytes);
            var font = FIGFont.FromStream(stream);
            Assert.IsNotNull(font);
        }
        zipTime.Stop();
        
        // Assert - ZIP loading shouldn't be more than 10x slower
        var ratio = zipTime.Elapsed.TotalMilliseconds / regularTime.Elapsed.TotalMilliseconds;
        Assert.IsTrue(ratio < 10, 
            $"ZIP font loading too slow. Regular: {regularTime.Elapsed}s, ZIP: {zipTime.Elapsed}s, Ratio: {ratio:F2}x");
        
        Console.WriteLine($"ZIP vs Regular Font Loading (10 iterations):");
        Console.WriteLine($"  Regular: {regularTime.Elapsed.TotalMilliseconds:F1}ms");
        Console.WriteLine($"  ZIP:     {zipTime.Elapsed.TotalMilliseconds:F1}ms");
        Console.WriteLine($"  Ratio:   {ratio:F2}x");
    }

    private static TimeSpan MeasureRenderingTime(FIGLetRenderer renderer, string text, int iterations)
    {
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            var result = renderer.Render(text);
            Assert.IsNotNull(result);
        }
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}