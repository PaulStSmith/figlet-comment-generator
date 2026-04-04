"""LayoutMode tests — mirrors C# LayoutModeTests."""
from __future__ import annotations

import pytest

from byteforge_figlet import FIGFont, FIGLetRenderer, LayoutMode
from .test_utilities import load_test_font


class TestLayoutModeValues:
    def test_full_size_is_minus_one(self):
        assert int(LayoutMode.FullSize) == -1

    def test_kerning_is_zero(self):
        assert int(LayoutMode.Kerning) == 0

    def test_smushing_is_one(self):
        assert int(LayoutMode.Smushing) == 1

    def test_default_equals_smushing(self):
        assert LayoutMode.Default == LayoutMode.Smushing
        assert int(LayoutMode.Default) == int(LayoutMode.Smushing)

    def test_all_distinct_integer_values(self):
        distinct = {int(LayoutMode.FullSize), int(LayoutMode.Kerning), int(LayoutMode.Smushing)}
        assert len(distinct) == 3

    def test_ordering(self):
        assert LayoutMode.FullSize < LayoutMode.Kerning
        assert LayoutMode.Kerning < LayoutMode.Smushing


@pytest.mark.parametrize("mode,expected", [
    (LayoutMode.FullSize, -1),
    (LayoutMode.Kerning,  0),
    (LayoutMode.Smushing, 1),
    (LayoutMode.Default,  1),
])
def test_mode_values_match_expected(mode, expected):
    assert int(mode) == expected


class TestLayoutModeNames:
    def test_full_size_name(self):
        assert LayoutMode.FullSize.name == "FullSize"

    def test_kerning_name(self):
        assert LayoutMode.Kerning.name == "Kerning"

    def test_smushing_name(self):
        assert LayoutMode.Smushing.name == "Smushing"

    def test_default_name_equals_smushing(self):
        # Default is an alias for Smushing
        assert LayoutMode.Default.name == "Smushing"


class TestLayoutModeWithRenderer:
    def test_all_modes_render_without_error(self):
        font = load_test_font("mini-fixed")
        text = "AB"
        for mode in [LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing]:
            result = FIGLetRenderer.render(text, font=font, mode=mode, line_separator="\n")
            assert result is not None

    def test_default_equals_smushing_output(self):
        font = load_test_font("mini-fixed")
        text = "AB"
        smush_result = FIGLetRenderer.render(text, font=font, mode=LayoutMode.Smushing, line_separator="\n")
        default_result = FIGLetRenderer.render(text, font=font, mode=LayoutMode.Default, line_separator="\n")
        assert smush_result == default_result

    def test_widths_decrease_full_to_smush(self):
        font = load_test_font("mini-fixed")
        text = "ABCD"
        def width(mode):
            r = FIGLetRenderer.render(text, font=font, mode=mode, line_separator="\n")
            return len([l for l in r.split("\n") if l][0])
        assert width(LayoutMode.FullSize) >= width(LayoutMode.Kerning)
        assert width(LayoutMode.Kerning) >= width(LayoutMode.Smushing)

    def test_all_modes_produce_correct_height(self):
        font = load_test_font("mini-fixed")
        for mode in [LayoutMode.FullSize, LayoutMode.Kerning, LayoutMode.Smushing, LayoutMode.Default]:
            result = FIGLetRenderer.render("AB", font=font, mode=mode, line_separator="\n")
            lines = [l for l in result.split("\n") if l]
            assert len(lines) == font.height
