"""FIGLetRenderer tests — mirrors C# FIGLetRendererTests and TS FIGLetRendererTests."""
from __future__ import annotations

import pytest

from byteforge_figlet import FIGFont, FIGLetRenderer, LayoutMode, SmushingRules
from .test_utilities import (
    assert_multi_line_equal,
    create_minimal_valid_font_content,
    load_test_font,
    strip_ansi_colors,
    generate_large_text,
    TEST_TEXTS,
    ANSI_COLORED_TEXTS,
)


@pytest.fixture(scope="module")
def test_font():
    return load_test_font("mini-fixed")


@pytest.fixture(scope="module")
def smushing_font():
    return load_test_font("smushing-test")


# ---------------------------------------------------------------------------
# Constructor
# ---------------------------------------------------------------------------

class TestConstructor:
    def test_default_parameters(self, test_font):
        renderer = FIGLetRenderer(font=test_font)
        assert renderer.font is test_font
        assert renderer.layout_mode == LayoutMode.Smushing
        assert renderer.use_ansi_colors is False
        assert renderer.paragraph_mode is True

    def test_custom_parameters(self, test_font):
        renderer = FIGLetRenderer(test_font, LayoutMode.Kerning, "|", True, False)
        assert renderer.font is test_font
        assert renderer.layout_mode == LayoutMode.Kerning
        assert renderer.line_separator == "|"
        assert renderer.use_ansi_colors is True
        assert renderer.paragraph_mode is False

    def test_null_font_falls_back_to_default(self):
        renderer = FIGLetRenderer()
        assert renderer.font is not None


# ---------------------------------------------------------------------------
# Static render
# ---------------------------------------------------------------------------

class TestStaticRender:
    def test_empty_string_returns_empty(self, test_font):
        assert FIGLetRenderer.render("", font=test_font) == ""

    def test_renders_text_correctly(self, test_font):
        result = FIGLetRenderer.render("Hi", font=test_font, line_separator="\n")
        assert result is not None
        assert_multi_line_equal("Hi\nHi\nHi\n", result)
        lines = [l for l in result.split("\n") if l]
        assert len(lines) == test_font.height


# ---------------------------------------------------------------------------
# Instance render — basic
# ---------------------------------------------------------------------------

class TestRender:
    def test_empty_string_returns_empty(self, test_font):
        renderer = FIGLetRenderer(test_font)
        assert renderer.render_text("") == ""

    def test_single_character(self, test_font):
        renderer = FIGLetRenderer(test_font)
        result = renderer.render_text("A")
        lines = [l for l in result.split("\n") if l]
        assert len(lines) == test_font.height
        assert "A" in result.replace(test_font.hard_blank, " ")

    def test_two_chars_wider_than_one(self, test_font):
        renderer = FIGLetRenderer(test_font, line_separator="\n")
        two_lines = [l for l in renderer.render_text("AB").split("\n") if l]
        one_lines = [l for l in renderer.render_text("A").split("\n") if l]
        assert len(two_lines[0]) > len(one_lines[0])

    # --- Layout modes ---

    @pytest.mark.parametrize("mode", [LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing])
    def test_all_modes_produce_correct_height(self, test_font, mode):
        renderer = FIGLetRenderer(test_font, mode, "\n")
        lines = [l for l in renderer.render_text("AB").split("\n") if l]
        assert len(lines) == test_font.height

    def test_full_size_no_overlap(self, test_font):
        def lines(s):
            return [l for l in FIGLetRenderer.render(s, font=test_font, mode=LayoutMode.FullSize, line_separator="\n").split("\n") if l]
        assert len(lines("AB")[0]) == len(lines("A")[0]) + len(lines("B")[0])

    def test_widths_decrease_full_to_smush(self, test_font):
        def width(mode):
            r = FIGLetRenderer.render("AB", font=test_font, mode=mode, line_separator="\n")
            return len([l for l in r.split("\n") if l][0])
        assert width(LayoutMode.FullSize) >= width(LayoutMode.Kerning)
        assert width(LayoutMode.Kerning) >= width(LayoutMode.Smushing)

    # --- Paragraph mode ---

    def test_paragraph_mode_true_expands_newlines(self, test_font):
        renderer = FIGLetRenderer(test_font, LayoutMode.Default, "\n", False, True)
        result = renderer.render_text("A\nB")
        lines = [l for l in result.split("\n") if l]
        assert len(lines) >= test_font.height * 2

    def test_paragraph_mode_false_collapses_newlines(self, test_font):
        renderer = FIGLetRenderer(test_font, LayoutMode.Default, "\n", False, False)
        result = renderer.render_text("A\nB")
        lines = [l for l in result.split("\n") if l]
        assert len(lines) == test_font.height

    def test_blank_line_adds_exactly_height_extra_newlines(self, test_font):
        renderer = FIGLetRenderer(test_font, LayoutMode.Default, "\n", False, True)
        consecutive = renderer.render_text("A\nB")
        separated = renderer.render_text("A\n\nB")
        stripped = consecutive.replace(test_font.hard_blank, " ")
        assert "A" in stripped
        assert "B" in stripped
        n_consec = consecutive.count("\n")
        n_sep = separated.count("\n")
        assert n_sep - n_consec == test_font.height

    # --- Custom line separator ---

    def test_custom_line_separator(self, test_font):
        renderer = FIGLetRenderer(test_font, line_separator="|")
        result = renderer.render_text("A")
        assert "|" in result
        assert "\n" not in result

    # --- ANSI colours ---

    def test_ansi_colors_preserved_when_enabled(self, test_font):
        renderer = FIGLetRenderer(test_font, use_ansi_colors=True, line_separator="\n")
        result = renderer.render_text("\x1b[31mR\x1b[32mG\x1b[0m")
        assert "\x1b[31m" in result
        assert "\x1b[32m" in result
        assert "\x1b[0m" in result

    def test_ansi_stripped_when_disabled(self, test_font):
        renderer = FIGLetRenderer(test_font, use_ansi_colors=False, line_separator="\n")
        result = renderer.render_text("\x1b[31mRed\x1b[0m")
        assert "\x1b[" not in result
        assert "Red" in result

    def test_complex_ansi_sequences_preserved(self, test_font):
        renderer = FIGLetRenderer(test_font, use_ansi_colors=True, line_separator="\n")
        inp = "\x1b[38;5;196m\x1b[48;2;255;255;0m\x1b[1m\x1b[4mTest\x1b[0m"
        result = renderer.render_text(inp)
        assert "\x1b[38;5;196m" in result
        assert "\x1b[48;2;255;255;0m" in result
        assert "\x1b[1m" in result
        assert "\x1b[4m" in result
        assert "\x1b[0m" in result

    # --- RTL ---

    def test_rtl_font_has_print_direction_one(self):
        content = create_minimal_valid_font_content(2, "$", print_direction=1)
        rtl_font = FIGFont.from_text(content)
        assert rtl_font is not None
        assert rtl_font.print_direction == 1

    def test_rtl_render_ab_stripped_equals_ltr_ba(self):
        ltr_font = FIGFont.from_text(create_minimal_valid_font_content(2, "$", print_direction=0))
        rtl_font = FIGFont.from_text(create_minimal_valid_font_content(2, "$", print_direction=1))
        ltr_renderer = FIGLetRenderer(ltr_font, line_separator="\n")
        rtl_renderer = FIGLetRenderer(rtl_font, line_separator="\n", use_ansi_colors=True)
        rtl_result = rtl_renderer.render_text("\x1b[31mAB\x1b[0m")
        assert "\x1b[31m" in rtl_result
        assert "\x1b[0m" in rtl_result
        stripped = strip_ansi_colors(rtl_result)
        ltr_ba = ltr_renderer.render_text("BA")
        assert stripped == ltr_ba

    # --- Unicode / missing characters ---

    def test_unknown_characters_skipped_gracefully(self, test_font):
        renderer = FIGLetRenderer(test_font)
        result = renderer.render_text("A\u2603B")  # snowman not in font
        processed = result.replace(test_font.hard_blank, " ")
        assert "A" in processed
        assert "B" in processed

    def test_emoji_skipped_gracefully(self, test_font):
        renderer = FIGLetRenderer(test_font)
        result = renderer.render_text("A\U0001F680B")  # rocket emoji
        processed = result.replace(test_font.hard_blank, " ")
        assert "A" in processed
        assert "B" in processed

    # --- Batch test vectors ---

    def test_all_test_texts(self, test_font):
        renderer = FIGLetRenderer(test_font, LayoutMode.Default, "\n")
        for input_text, expected in TEST_TEXTS.items():
            result = renderer.render_text(input_text)
            assert expected in result, f"Failed for: {input_text!r}"

    def test_all_ansi_colored_texts(self, test_font):
        renderer = FIGLetRenderer(test_font, LayoutMode.Default, "\n", use_ansi_colors=True)
        for input_text, expected in ANSI_COLORED_TEXTS.items():
            result = renderer.render_text(input_text)
            assert expected in result, f"Failed for: {input_text!r}"

    # --- Performance ---

    def test_large_text_renders_in_reasonable_time(self, test_font):
        import time
        renderer = FIGLetRenderer(test_font)
        text = generate_large_text(100)
        start = time.time()
        result = renderer.render_text(text)
        elapsed = time.time() - start
        assert result
        assert elapsed < 5.0


# ---------------------------------------------------------------------------
# _smush_characters
# ---------------------------------------------------------------------------

class TestSmushCharacters:
    def test_equal_character_smushing(self, smushing_font):
        result = FIGLetRenderer._smush_characters("|", "|", "$", LayoutMode.Smushing, SmushingRules.EqualCharacter)
        assert result == "|"

    def test_underscore_yields_to_hierarchy(self, smushing_font):
        result = FIGLetRenderer._smush_characters("_", "|", "$", LayoutMode.Smushing, SmushingRules.Underscore)
        assert result == "|"

    def test_hierarchy_higher_wins(self, smushing_font):
        result = FIGLetRenderer._smush_characters("|", "/", "$", LayoutMode.Smushing, SmushingRules.Hierarchy)
        assert result == "/"

    def test_opposite_pair_brackets_give_pipe(self, smushing_font):
        result = FIGLetRenderer._smush_characters("[", "]", "$", LayoutMode.Smushing, SmushingRules.OppositePair)
        assert result == "|"

    def test_bigx_slash_backslash_gives_pipe(self, smushing_font):
        result = FIGLetRenderer._smush_characters("/", "\\", "$", LayoutMode.Smushing, SmushingRules.BigX)
        assert result == "|"

    def test_bigx_backslash_slash_gives_y(self, smushing_font):
        result = FIGLetRenderer._smush_characters("\\", "/", "$", LayoutMode.Smushing, SmushingRules.BigX)
        assert result == "Y"

    def test_bigx_arrows_give_x(self, smushing_font):
        result = FIGLetRenderer._smush_characters(">", "<", "$", LayoutMode.Smushing, SmushingRules.BigX)
        assert result == "X"

    def test_hard_blank_smushing(self, smushing_font):
        hb = smushing_font.hard_blank
        result = FIGLetRenderer._smush_characters(hb, hb, hb, LayoutMode.Smushing, SmushingRules.HardBlank)
        assert result == hb


# ---------------------------------------------------------------------------
# _can_smush
# ---------------------------------------------------------------------------

class TestCanSmush:
    def test_spaces_always_smushable(self, test_font):
        assert FIGLetRenderer._can_smush(" ", " ", "$", LayoutMode.Smushing, SmushingRules.NONE) is True

    def test_kerning_only_spaces(self, test_font):
        assert FIGLetRenderer._can_smush(" ", " ", "$", LayoutMode.Kerning, SmushingRules.NONE) is True
        assert FIGLetRenderer._can_smush("A", "A", "$", LayoutMode.Kerning, SmushingRules.NONE) is False
        assert FIGLetRenderer._can_smush("A", "B", "$", LayoutMode.Kerning, SmushingRules.NONE) is False

    def test_full_size_cannot_smush(self, test_font):
        assert FIGLetRenderer._can_smush("A", "A", "$", LayoutMode.FullSize, SmushingRules.EqualCharacter) is False
