"""Integration tests — mirrors C# IntegrationTests and TS IntegrationTests."""
from __future__ import annotations

import io
import os
import tempfile

import pytest

from byteforge_figlet import FIGFont, FIGLetRenderer, LayoutMode
from .test_utilities import (
    assert_multi_line_equal,
    create_minimal_valid_font_content,
    create_zip_with_font_file,
    load_test_font,
    strip_ansi_colors,
    BANNER_SCENARIOS,
)


class TestEndToEnd:
    def test_default_font_and_renderer(self):
        result = FIGLetRenderer.render("Hello", line_separator="\n")
        assert result
        lines = [l for l in result.split("\n") if l]
        assert len(lines) > 0
        assert all(len(l) > 0 for l in lines)

    def test_custom_font_from_file(self):
        content = create_minimal_valid_font_content(4, "#")
        with tempfile.NamedTemporaryFile(suffix=".flf", mode="w", delete=False, encoding="utf-8") as f:
            f.write(content)
            tmp = f.name
        try:
            font = FIGFont.from_file(tmp)
            result = FIGLetRenderer.render("Test", font=font, line_separator="\n")
            assert font is not None
            assert font.height == 4
            assert result
            lines = [l for l in result.split("\n") if l]
            assert len(lines) == 4
        finally:
            os.unlink(tmp)

    def test_deflate_zip_font_end_to_end(self):
        content = create_minimal_valid_font_content(3, "@")
        zip_bytes = create_zip_with_font_file(content, "test.flf", store=False)
        with tempfile.NamedTemporaryFile(suffix=".zip", delete=False) as f:
            f.write(zip_bytes)
            tmp = f.name
        try:
            font = FIGFont.from_file(tmp)
            result = FIGLetRenderer.render("Hi", font=font, line_separator="\n")
            assert font is not None
            assert font.hard_blank == "@"
            assert font.height == 3
            lines = [l for l in result.split("\n") if l]
            assert len(lines) == 3
        finally:
            os.unlink(tmp)

    def test_store_zip_font_end_to_end(self):
        content = create_minimal_valid_font_content(4, "%")
        zip_bytes = create_zip_with_font_file(content, "test.flf", store=True)
        with tempfile.NamedTemporaryFile(suffix=".zip", delete=False) as f:
            f.write(zip_bytes)
            tmp = f.name
        try:
            font = FIGFont.from_file(tmp)
            result = FIGLetRenderer.render("Hi", font=font, line_separator="\n")
            assert font is not None
            assert font.hard_blank == "%"
            assert font.height == 4
            lines = [l for l in result.split("\n") if l]
            assert len(lines) == 4
        finally:
            os.unlink(tmp)

    def test_all_layout_modes_different_widths(self):
        font = load_test_font("mini-fixed")
        text = "ABCD"
        full = FIGLetRenderer.render(text, font=font, mode=LayoutMode.FullSize,  line_separator="\n")
        kern = FIGLetRenderer.render(text, font=font, mode=LayoutMode.Kerning,   line_separator="\n")
        smsh = FIGLetRenderer.render(text, font=font, mode=LayoutMode.Smushing,  line_separator="\n")
        full_lines = [l for l in full.split("\n") if l]
        kern_lines = [l for l in kern.split("\n") if l]
        smsh_lines = [l for l in smsh.split("\n") if l]
        assert len(full_lines) == font.height
        assert len(kern_lines) == font.height
        assert len(smsh_lines) == font.height
        assert len(full_lines[0]) >= len(kern_lines[0])
        assert len(kern_lines[0]) >= len(smsh_lines[0])

    def test_complex_text_ansi_and_paragraphs(self):
        font = load_test_font("smushing-test")
        renderer = FIGLetRenderer(font, LayoutMode.Smushing, "\n", True, True)
        text = "\x1b[31mHello\x1b[0m\n\x1b[32mWorld\x1b[0m!"
        result = renderer.render_text(text)
        assert "\x1b[31m" in result
        assert "\x1b[32m" in result
        assert "\x1b[0m" in result
        assert "\n" in result
        lines = [l for l in result.split("\n") if l]
        assert len(lines) >= font.height * 2

    def test_different_line_separators(self):
        font = load_test_font("mini-fixed")
        for sep in ["\n", "\r\n", "|", "|||"]:
            renderer = FIGLetRenderer(font, line_separator=sep)
            result = renderer.render_text("Test")
            assert result is not None
            assert sep in result
            parts = [p for p in result.split(sep) if p]
            assert len(parts) == font.height

    def test_real_world_banners(self):
        font = FIGFont.default
        for text, expected in BANNER_SCENARIOS:
            result = FIGLetRenderer.render(text, font=font, mode=LayoutMode.Smushing, line_separator="\n")
            assert result is not None
            assert len(result) > len(text)
            lines = [l for l in result.split("\n") if l]
            assert len(lines) > 1
            assert_multi_line_equal(expected, result, f"Banner mismatch for {text!r}: ")

    def test_error_recovery_unknown_chars(self):
        font = load_test_font("mini-fixed")
        renderer = FIGLetRenderer(font)
        result = renderer.render_text("A\u2603B")  # snowman not in font
        processed = result.replace(font.hard_blank, " ")
        assert "A" in processed
        assert "B" in processed

    def test_error_recovery_empty_input(self):
        font = load_test_font("mini-fixed")
        renderer = FIGLetRenderer(font)
        assert renderer.render_text("") == ""

    def test_very_long_input_does_not_raise(self):
        font = load_test_font("mini-fixed")
        renderer = FIGLetRenderer(font)
        result = renderer.render_text("A" * 1000)
        assert result
        assert len(result) > 0

    def test_all_smushing_scenarios_narrower_than_full_size(self):
        font = load_test_font("smushing-test")
        renderer = FIGLetRenderer(font, LayoutMode.Smushing, "\n")
        cases = [
            ("AA", "Equal character smushing"),
            ("_|", "Underscore smushing"),
            ("||", "Hierarchy smushing"),
            ("[]", "Opposite pair smushing"),
            ("/\\", "Big X (slashes)"),
            ("><", "Big X (arrows)"),
        ]
        for text, desc in cases:
            smush = renderer.render_text(text)
            full  = FIGLetRenderer.render(text, font=font, mode=LayoutMode.FullSize, line_separator="\n")
            smush_lines = [l for l in smush.split("\n") if l]
            full_lines  = [l for l in full.split("\n") if l]
            assert len(smush_lines[0]) <= len(full_lines[0]), desc

    def test_comment_style_formatting(self):
        font = FIGFont.default
        renderer = FIGLetRenderer(font, line_separator="\n")
        banner = renderer.render_text("API")
        for lang, prefix in [("C#", "//"), ("Python", "#"), ("SQL", "--"), ("HTML", "<!-- ")]:
            lines = [l for l in banner.split("\n") if l]
            commented = "\n".join(f"{prefix} {l}" for l in lines)
            for line in commented.split("\n"):
                if line:
                    assert line.startswith(prefix), f"Failed for {lang}"

    def test_complex_ansi_sequences_preserved(self):
        renderer = FIGLetRenderer(FIGFont.default, use_ansi_colors=True, line_separator="\n")
        inp = "".join([
            "\x1b[38;5;196m",
            "\x1b[48;2;0;255;0m",
            "\x1b[1m",
            "\x1b[3m",
            "\x1b[4m",
            "Complex",
            "\x1b[0m",
        ])
        result = renderer.render_text(inp)
        assert "\x1b[38;5;196m" in result
        assert "\x1b[48;2;0;255;0m" in result
        assert "\x1b[1m" in result
        assert "\x1b[3m" in result
        assert "\x1b[4m" in result
        assert "\x1b[0m" in result

    def test_concurrent_rendering_is_thread_safe(self):
        import concurrent.futures
        font = FIGFont.default
        texts = ["Test1", "Test2", "Test3", "Test4", "Test5"]
        results = [None] * len(texts)
        errors = [None] * len(texts)
        def render_one(i):
            try:
                results[i] = FIGLetRenderer.render(texts[i], font=font)
            except Exception as e:
                errors[i] = e
        with concurrent.futures.ThreadPoolExecutor(max_workers=5) as executor:
            futs = [executor.submit(render_one, i) for i in range(len(texts))]
            for f in futs:
                f.result()
        for i in range(len(texts)):
            assert errors[i] is None, f"Exception in thread {i}: {errors[i]}"
            assert results[i] is not None
            assert len(results[i]) > 0
