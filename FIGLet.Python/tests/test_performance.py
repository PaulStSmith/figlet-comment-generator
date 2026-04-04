"""Performance tests — mirrors C# PerformanceTests."""
from __future__ import annotations

import time
import concurrent.futures

import pytest

from byteforge_figlet import FIGFont, FIGLetRenderer, LayoutMode
from .test_utilities import (
    create_minimal_valid_font_content,
    create_stream_from_string,
    create_zip_with_font_file,
    generate_large_text,
    load_test_font,
)


def _measure(fn, iterations: int) -> float:
    """Return elapsed seconds for `iterations` calls to fn()."""
    start = time.perf_counter()
    for _ in range(iterations):
        fn()
    return time.perf_counter() - start


class TestPerformance:
    def test_default_font_access_is_fast(self):
        """Accessing the cached default font 100 times should be very fast."""
        elapsed = _measure(lambda: FIGFont.default, 100)
        assert elapsed < 0.05, f"Default font access too slow: {elapsed:.3f}s for 100 calls"

    def test_font_from_stream_is_reasonable(self):
        """Loading a font from stream 10 times should complete within 1 second."""
        content = create_minimal_valid_font_content()
        def load():
            stream = create_stream_from_string(content)
            return FIGFont.from_stream(stream)
        elapsed = _measure(load, 10)
        assert elapsed < 1.0, f"Font stream loading too slow: {elapsed:.3f}s for 10 calls"

    def test_simple_text_rendering_is_fast(self):
        """Rendering 'Hello World' 1 000 times should complete within 2 seconds."""
        renderer = FIGLetRenderer(FIGFont.default)
        elapsed = _measure(lambda: renderer.render_text("Hello World"), 1000)
        assert elapsed < 2.0, f"Simple rendering too slow: {elapsed:.3f}s for 1000 calls"

    def test_large_text_rendering_completes(self):
        """Rendering 1 000 random characters should complete within 10 seconds."""
        renderer = FIGLetRenderer(FIGFont.default)
        text = generate_large_text(1000)
        start = time.perf_counter()
        result = renderer.render_text(text)
        elapsed = time.perf_counter() - start
        assert result
        assert elapsed < 10.0, f"Large text rendering too slow: {elapsed:.3f}s"

    def test_all_layout_modes_complete_in_reasonable_time(self):
        """Each layout mode should render 100 iterations in under 2 seconds."""
        renderer_base = FIGFont.default
        text = "Performance Test"
        for mode in [LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing]:
            renderer = FIGLetRenderer(renderer_base, mode)
            elapsed = _measure(lambda: renderer.render_text(text), 100)
            assert elapsed < 2.0, f"{mode.name} too slow: {elapsed:.3f}s for 100 calls"

    def test_ansi_processing_not_significantly_slower(self):
        """ANSI processing should not be more than 3x slower than plain rendering."""
        normal_renderer = FIGLetRenderer(FIGFont.default, use_ansi_colors=False)
        color_renderer = FIGLetRenderer(FIGFont.default, use_ansi_colors=True)
        normal_text = "Color Test Normal"
        colored_text = "\x1b[31mColor\x1b[32m Test\x1b[33m ANSI\x1b[0m"
        normal_time = _measure(lambda: normal_renderer.render_text(normal_text), 100)
        color_time  = _measure(lambda: color_renderer.render_text(colored_text), 100)
        assert color_time < normal_time * 3, (
            f"ANSI too slow. Normal: {normal_time:.3f}s, Color: {color_time:.3f}s"
        )

    def test_paragraph_mode_handles_multiple_lines(self):
        """Paragraph and single-line modes both complete 50 iterations in reasonable time."""
        multi = "\n".join(["Test Line"] * 20)
        single_r = FIGLetRenderer(FIGFont.default, paragraph_mode=False)
        para_r   = FIGLetRenderer(FIGFont.default, paragraph_mode=True)
        assert _measure(lambda: single_r.render_text(multi), 50) < 2.0
        assert _measure(lambda: para_r.render_text(multi), 50) < 5.0

    def test_smushing_calculations_are_efficient(self):
        """Smushing 100 iterations of a moderately long string should be fast."""
        font = load_test_font("smushing-test")
        renderer = FIGLetRenderer(font, LayoutMode.Smushing)
        elapsed = _measure(lambda: renderer.render_text("Smushing Performance Test"), 100)
        assert elapsed < 5.0, f"Smushing too slow: {elapsed:.3f}s for 100 calls"

    def test_static_vs_instance_comparable(self):
        """Instance method should not be more than 1.5x slower than creating a new renderer each time."""
        font = FIGFont.default
        renderer = FIGLetRenderer(font)
        text = "Method Test"
        static_time  = _measure(lambda: FIGLetRenderer.render(text, font=font), 500)
        instance_time = _measure(lambda: renderer.render_text(text), 500)
        assert instance_time <= static_time * 1.5, (
            f"Instance too slow. Static: {static_time:.3f}s, Instance: {instance_time:.3f}s"
        )

    def test_zip_font_loading_not_excessively_slow(self):
        """ZIP font loading should not be more than 10x slower than plain loading."""
        content = create_minimal_valid_font_content()
        zip_bytes = create_zip_with_font_file(content)

        import io
        def load_plain():
            return FIGFont.from_stream(create_stream_from_string(content))
        def load_zip():
            return FIGFont.from_stream(io.BytesIO(zip_bytes))

        plain_time = _measure(load_plain, 10)
        zip_time   = _measure(load_zip,   10)
        ratio = zip_time / max(plain_time, 0.0001)
        assert ratio < 10, f"ZIP loading too slow. Plain: {plain_time:.3f}s, ZIP: {zip_time:.3f}s, ratio: {ratio:.1f}x"

    def test_unicode_not_significantly_slower_than_ascii(self):
        """Unicode text rendering should not be more than 3x slower than ASCII."""
        renderer = FIGLetRenderer(FIGFont.default)
        ascii_text   = "ASCII Test Text"
        unicode_text = "Unicode: \u00e9\u00f1\u4e2d\u6587\U0001F680\U0001F389\u2728"
        ascii_time   = _measure(lambda: renderer.render_text(ascii_text),   200)
        unicode_time = _measure(lambda: renderer.render_text(unicode_text), 200)
        assert unicode_time < ascii_time * 3, (
            f"Unicode too slow. ASCII: {ascii_time:.3f}s, Unicode: {unicode_time:.3f}s"
        )

    def test_concurrent_rendering_scales(self):
        """Concurrent rendering on multiple threads should achieve reasonable efficiency."""
        font = FIGFont.default
        text = generate_large_text(100)
        thread_count = 4
        iterations_per_thread = 200
        def render_many():
            r = FIGLetRenderer(font)
            for _ in range(iterations_per_thread):
                r.render_text(text)

        # Sequential baseline
        seq_start = time.perf_counter()
        for _ in range(thread_count):
            render_many()
        seq_time = time.perf_counter() - seq_start

        # Concurrent
        con_start = time.perf_counter()
        with concurrent.futures.ThreadPoolExecutor(max_workers=thread_count) as ex:
            futs = [ex.submit(render_many) for _ in range(thread_count)]
            for f in futs:
                f.result()
        con_time = time.perf_counter() - con_start

        speedup = seq_time / max(con_time, 0.0001)
        efficiency = speedup / thread_count
        assert efficiency > 0.15, (
            f"Concurrent efficiency too low: {efficiency:.2f} "
            f"(seq={seq_time:.2f}s, con={con_time:.2f}s)"
        )
