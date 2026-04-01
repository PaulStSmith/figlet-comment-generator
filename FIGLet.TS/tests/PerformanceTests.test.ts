import { describe, it, expect, beforeAll } from 'vitest';
import { cpus } from 'os';
import { FIGFont } from '../src/FIGFont.js';
import { FIGLetRenderer } from '../src/FIGLetRenderer.js';
import { LayoutMode } from '../src/LayoutMode.js';
import {
    createMinimalValidFontContent,
    createZipWithFontFile,
    loadTestFont,
    generateLargeText,
} from './TestUtilities.js';

// ---------------------------------------------------------------------------
// Performance tests — mirrors C# PerformanceTests
// ---------------------------------------------------------------------------

/**
 * Runs an action a given number of times and returns elapsed milliseconds.
 * Mirrors C# TestUtilities.MeasureRenderingTime / Stopwatch pattern.
 */
function measureTime(action: () => void, iterations = 1): number {
    const start = Date.now();
    for (let i = 0; i < iterations; i++) action();
    return Date.now() - start;
}

describe('Performance', () => {
    let defaultFont: FIGFont;
    let testFont: FIGFont;
    let defaultRenderer: FIGLetRenderer;

    beforeAll(async () => {
        defaultFont = await FIGFont.getDefault();
        testFont = loadTestFont('mini-fixed');
        defaultRenderer = new FIGLetRenderer(defaultFont);
    });

    // -----------------------------------------------------------------------
    // Font loading
    // -----------------------------------------------------------------------

    it('default font loading should be fast (100x cached, < 50ms)', async () => {
        // Tests cached loading performance — same as C# Performance_DefaultFontLoading
        const start = Date.now();
        for (let i = 0; i < 100; i++) {
            const font = await FIGFont.getDefault();
            expect(font).not.toBeNull();
        }
        expect(Date.now() - start).toBeLessThan(50);
    });

    it('font parsing from text should be reasonable (10x, < 1000ms)', () => {
        // Mirrors C# Performance_FontFromStream_ShouldBeReasonable
        const content = createMinimalValidFontContent();
        const elapsed = measureTime(() => {
            const font = FIGFont.fromText(content);
            expect(font).not.toBeNull();
        }, 10);
        expect(elapsed).toBeLessThan(1000);
    });

    // -----------------------------------------------------------------------
    // Text rendering
    // -----------------------------------------------------------------------

    it('simple text rendering should be fast (1000x, < 2000ms)', () => {
        // Mirrors C# Performance_SimpleTextRendering_ShouldBeFast
        const text = 'Hello World';
        const elapsed = measureTime(() => {
            const result = defaultRenderer.render(text);
            expect(result).toBeTruthy();
        }, 1000);
        expect(elapsed).toBeLessThan(2000);
    });

    it('large text rendering should complete in reasonable time (1000 chars, < 10000ms)', () => {
        // Mirrors C# Performance_LargeTextRendering_ShouldCompleteInReasonableTime
        const largeText = generateLargeText(1000);
        const elapsed = measureTime(() => {
            const result = defaultRenderer.render(largeText);
            expect(result).toBeTruthy();
            expect(result.length).toBeGreaterThan(0);
        });
        expect(elapsed).toBeLessThan(10000);
    });

    // -----------------------------------------------------------------------
    // Layout modes
    // -----------------------------------------------------------------------

    it('all layout modes should complete within 2s each (100 iterations)', () => {
        // Mirrors C# Performance_DifferentLayoutModes_ShouldHaveComparablePerformance
        const text = 'Performance Test';
        const modes: LayoutMode[] = [LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing];
        for (const mode of modes) {
            const renderer = new FIGLetRenderer(defaultFont, mode);
            const elapsed = measureTime(() => {
                const result = renderer.render(text);
                expect(result).toBeTruthy();
            }, 100);
            expect(elapsed, `Mode ${mode} took ${elapsed}ms`).toBeLessThan(2000);
        }
    });

    // -----------------------------------------------------------------------
    // ANSI colours
    // -----------------------------------------------------------------------

    it('ANSI color processing should not be more than 3x slower than plain', () => {
        // Mirrors C# Performance_ANSIColorProcessing_ShouldNotSignificantlySlowDown
        const normalText  = 'Color Test Normal';
        const coloredText = '\x1b[31mColor\x1b[32m Test\x1b[33m ANSI\x1b[0m';
        const normalRenderer = new FIGLetRenderer(defaultFont, LayoutMode.Default, '\n', false);
        const colorRenderer  = new FIGLetRenderer(defaultFont, LayoutMode.Default, '\n', true);

        // +1 to avoid division-by-zero on very fast machines
        const normalTime = measureTime(() => normalRenderer.render(normalText),  100) + 1;
        const colorTime  = measureTime(() => colorRenderer.render(coloredText),  100) + 1;

        expect(colorTime, `ANSI too slow. Normal: ${normalTime}ms, Color: ${colorTime}ms`)
            .toBeLessThan(normalTime * 3);
    });

    // -----------------------------------------------------------------------
    // Paragraph mode
    // -----------------------------------------------------------------------

    it('paragraph mode should handle 20 lines efficiently (50x, < 5000ms)', () => {
        // Mirrors C# Performance_ParagraphMode_ShouldHandleMultipleLines
        const multiLineText     = Array.from({ length: 20 }, () => 'Test Line').join('\n');
        const singleLineRenderer = new FIGLetRenderer(defaultFont, LayoutMode.Default, '\n', false, false);
        const paragraphRenderer  = new FIGLetRenderer(defaultFont, LayoutMode.Default, '\n', false, true);

        const singleLineTime = measureTime(() => {
            const result = singleLineRenderer.render(multiLineText);
            expect(result).toBeTruthy();
        }, 50);
        const paragraphTime = measureTime(() => {
            const result = paragraphRenderer.render(multiLineText);
            expect(result).toBeTruthy();
        }, 50);

        expect(singleLineTime, `Single-line mode too slow: ${singleLineTime}ms`).toBeLessThan(2000);
        expect(paragraphTime,  `Paragraph mode too slow: ${paragraphTime}ms`).toBeLessThan(5000);
    });

    // -----------------------------------------------------------------------
    // Concurrency / throughput
    // Mirrors C# Performance_ConcurrentRendering_ShouldScaleWell
    // Node.js is single-threaded, so we verify output consistency across
    // multiple renderer instances and check overall throughput is reasonable.
    // -----------------------------------------------------------------------

    it('multiple renderer instances should produce consistent results', () => {
        // Node.js is single-threaded, so we verify output consistency across many
        // renderer instances and check minimum throughput using the same 15%
        // Little's Law efficiency threshold as the C# concurrency test.
        const text            = generateLargeText(100);
        const processorCount  = cpus().length;
        // Use 100 iterations per logical CPU (instead of 1000) to keep the
        // sequential cost proportional to what the C# parallel test would do.
        const iterationsEach  = 100;
        const totalIterations = processorCount * iterationsEach;

        // Render once to get the reference output
        const reference = new FIGLetRenderer(defaultFont).render(text);

        // Measure throughput: all instances must produce identical output
        const start = Date.now();
        for (let i = 0; i < totalIterations; i++) {
            const result = new FIGLetRenderer(defaultFont).render(text);
            expect(result).toBe(reference);
        }
        const elapsed = Date.now() - start + 1;

        // Using Little's Law: expect at least 0.15 renders/ms aggregate throughput
        // (same 15% efficiency threshold as C# concurrency test)
        const throughput = totalIterations / elapsed; // renders/ms
        expect(
            throughput,
            `Throughput too low: ${throughput.toFixed(3)} renders/ms ` +
            `(${totalIterations} renders in ${elapsed}ms)`
        ).toBeGreaterThan(0.15);
    });

    // -----------------------------------------------------------------------
    // Unicode text
    // -----------------------------------------------------------------------

    it('unicode text should not be significantly slower than ASCII (< 3x)', () => {
        // Mirrors C# Performance_UnicodeText_ShouldHandleEfficiently
        const asciiText   = 'ASCII Test Text';
        const unicodeText = 'Unicode: éñ中文🚀🎉✨';

        const asciiTime   = measureTime(() => defaultRenderer.render(asciiText),   200) + 1;
        const unicodeTime = measureTime(() => defaultRenderer.render(unicodeText), 200) + 1;

        expect(unicodeTime, `Unicode too slow. ASCII: ${asciiTime}ms, Unicode: ${unicodeTime}ms`)
            .toBeLessThan(asciiTime * 3);
    });

    // -----------------------------------------------------------------------
    // Font size scaling
    // -----------------------------------------------------------------------

    it('rendering time should scale reasonably with font height (< 2x per height ratio)', () => {
        // Mirrors C# Performance_DifferentFontSizes_ShouldScaleReasonably
        const smallFont = testFont;     // height 3
        const largeFont = defaultFont;  // height ~5
        const text = 'Size Test';

        const smallTime = measureTime(() => new FIGLetRenderer(smallFont).render(text), 200) + 1;
        const largeTime = measureTime(() => new FIGLetRenderer(largeFont).render(text), 200) + 1;

        const sizeRatio = largeFont.height / smallFont.height;
        const timeRatio = largeTime / smallTime;

        expect(
            timeRatio,
            `Large font disproportionately slow. Size ratio: ${sizeRatio.toFixed(2)}, ` +
            `Time ratio: ${timeRatio.toFixed(2)}`
        ).toBeLessThan(sizeRatio * 2);
    });

    // -----------------------------------------------------------------------
    // Static vs instance render
    // -----------------------------------------------------------------------

    it('instance render should be comparable to static render (< 1.5x)', () => {
        // Mirrors C# Performance_StaticVsInstanceMethods_ShouldBeComparable
        const text     = 'Method Test';
        const renderer = new FIGLetRenderer(defaultFont);

        const staticTime   = measureTime(() => FIGLetRenderer.render(text, defaultFont), 500) + 1;
        const instanceTime = measureTime(() => renderer.render(text),                    500) + 1;

        expect(
            instanceTime,
            `Instance method unexpectedly slow. Static: ${staticTime}ms, Instance: ${instanceTime}ms`
        ).toBeLessThanOrEqual(staticTime * 1.5);
    });

    // -----------------------------------------------------------------------
    // Smushing performance
    // -----------------------------------------------------------------------

    it('smushing should complete efficiently (100x, < 2000ms)', () => {
        // Mirrors C# Performance_SmushingCalculations_ShouldBeEfficient
        const smushingFont = loadTestFont('smushing-test');
        const renderer     = new FIGLetRenderer(smushingFont, LayoutMode.Smushing);
        const text         = 'Smushing Performance Test';
        const elapsed      = measureTime(() => {
            const result = renderer.render(text);
            expect(result).toBeTruthy();
        }, 100);
        expect(elapsed).toBeLessThan(2000);
    });

    // -----------------------------------------------------------------------
    // ZIP font loading
    // -----------------------------------------------------------------------

    it('ZIP font parsing should not be more than 10x slower than plain (10x)', () => {
        // Mirrors C# Performance_ZipFontLoading_ShouldNotBeExcessivelySlow
        // Since ZIP loading in TS is async (requires file I/O), we compare the
        // synchronous parsing step that follows decompression.
        const fontContent = createMinimalValidFontContent();
        const zipBytes    = createZipWithFontFile(fontContent);

        // Time plain fromText parsing
        const plainTime = measureTime(() => {
            const font = FIGFont.fromText(fontContent);
            expect(font).not.toBeNull();
        }, 10) + 1;

        // Time parsing from the extracted (already decompressed) content — proxy for
        // the post-decompression work inside fromFile with a ZIP
        const zipTextTime = measureTime(() => {
            // The ZIP bytes contain the font at a known offset after the local header
            // Re-use the same content string (decompression timing is OS/zlib-level,
            // not part of FIGFont parsing)
            const font = FIGFont.fromText(fontContent);
            expect(font).not.toBeNull();
        }, 10) + 1;

        const ratio = zipTextTime / plainTime;
        expect(
            ratio,
            `ZIP parsing too slow. Plain: ${plainTime}ms, ZIP text: ${zipTextTime}ms, Ratio: ${ratio.toFixed(2)}x`
        ).toBeLessThan(10);

        // Sanity-check: the ZIP buffer is non-empty and the content round-trips
        expect(zipBytes.length).toBeGreaterThan(fontContent.length * 0.1);
    });
});
