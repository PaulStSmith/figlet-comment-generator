"""FIGFont tests — mirrors C# FIGFontTests and TS FIGFontTests."""
from __future__ import annotations

import io
import os
import tempfile
import zipfile

import pytest

from byteforge_figlet import FIGFont, SmushingRules
from .test_utilities import (
    assert_smushing_rule,
    create_minimal_valid_font_content,
    create_stream_from_string,
    create_zip_with_font_file,
    load_test_font,
)


# ---------------------------------------------------------------------------
# Default font
# ---------------------------------------------------------------------------

class TestDefaultFont:
    def test_loads_successfully(self):
        font = FIGFont.default
        assert font is not None
        assert font.signature[:5] == "flf2a"
        assert font.height > 0
        assert len(font.characters) >= 95

    def test_is_cached(self):
        font1 = FIGFont.default
        font2 = FIGFont.default
        assert font1.signature == font2.signature
        assert font1.height == font2.height
        assert font1.hard_blank == font2.hard_blank


# ---------------------------------------------------------------------------
# from_file
# ---------------------------------------------------------------------------

class TestFromFile:
    def test_returns_none_for_none_path(self):
        assert FIGFont.from_file(None) is None

    def test_returns_none_for_empty_path(self):
        assert FIGFont.from_file("") is None

    def test_raises_for_nonexistent_file(self):
        with pytest.raises((FileNotFoundError, OSError)):
            FIGFont.from_file("nonexistent_totally_missing.flf")

    def test_loads_valid_font_file(self):
        content = create_minimal_valid_font_content(3, "#")
        with tempfile.NamedTemporaryFile(suffix=".flf", mode="w", delete=False, encoding="utf-8") as f:
            f.write(content)
            tmp = f.name
        try:
            font = FIGFont.from_file(tmp)
            assert font is not None
            assert font.hard_blank == "#"
            assert font.height == 3
        finally:
            os.unlink(tmp)


# ---------------------------------------------------------------------------
# from_stream
# ---------------------------------------------------------------------------

class TestFromStream:
    def test_returns_none_for_none_stream(self):
        assert FIGFont.from_stream(None) is None

    def test_parses_valid_stream(self):
        content = create_minimal_valid_font_content(3, "#")
        stream = create_stream_from_string(content)
        font = FIGFont.from_stream(stream)
        assert font is not None
        assert font.hard_blank == "#"
        assert font.height == 3

    def test_loads_mini_fixed(self):
        font = load_test_font("mini-fixed")
        assert font is not None
        assert font.signature == "flf2a$"
        assert font.height == 3
        assert len(font.characters) == 95


# ---------------------------------------------------------------------------
# from_text / from_lines
# ---------------------------------------------------------------------------

class TestFromLines:
    def test_returns_none_for_none(self):
        assert FIGFont.from_lines(None) is None
        assert FIGFont.from_text(None) is None

    def test_returns_none_for_empty(self):
        assert FIGFont.from_lines([]) is None
        assert FIGFont.from_text("") is None

    def test_raises_for_invalid_signature(self):
        with pytest.raises(ValueError, match="Invalid FIGfont format"):
            FIGFont.from_lines(["invalid_signature 5 4 10 15 0"])

    def test_parses_minimal_valid_font(self):
        content = create_minimal_valid_font_content()
        lines = content.split("\n")
        font = FIGFont.from_lines(lines)
        assert font is not None
        assert font.signature == "flf2a$"
        assert font.hard_blank == "$"
        assert font.height == 5
        assert font.baseline == 4
        assert font.max_length == 10
        assert font.old_layout == 15
        assert len(font.characters) == 95  # ASCII 32-126

    def test_round_trip_from_text(self):
        content = create_minimal_valid_font_content(3, "#")
        font = FIGFont.from_text(content)
        assert font is not None
        assert font.hard_blank == "#"
        assert font.height == 3


# ---------------------------------------------------------------------------
# Smushing rules
# ---------------------------------------------------------------------------

class TestSmushingRules:
    def test_smushing_test_font_has_all_rules(self):
        font = load_test_font("smushing-test")
        assert_smushing_rule(font, SmushingRules.EqualCharacter, True)
        assert_smushing_rule(font, SmushingRules.Underscore, True)
        assert_smushing_rule(font, SmushingRules.Hierarchy, True)
        assert_smushing_rule(font, SmushingRules.OppositePair, True)
        assert_smushing_rule(font, SmushingRules.BigX, True)
        assert_smushing_rule(font, SmushingRules.HardBlank, True)


# ---------------------------------------------------------------------------
# ParseInt variants (accessed via public static method)
# ---------------------------------------------------------------------------

@pytest.mark.parametrize("text,expected", [
    ("0", 0),
    ("128", 128),
    ("0x1A", 26),
    ("0xFF", 255),
    ("0b1010", 10),
    ("0777", 511),
    ("  42  ", 42),
])
def test_parse_int_valid_inputs(text, expected):
    result = FIGFont._parse_int(text)
    assert result == expected


# ---------------------------------------------------------------------------
# Layout parameter parsing
# ---------------------------------------------------------------------------

class TestLayoutParameters:
    def test_old_layout_minus_one_gives_no_rules(self):
        content = create_minimal_valid_font_content(2, "$")
        lines = content.split("\n")
        lines[0] = "flf2a$ 2 1 5 -1 0"
        font = FIGFont.from_lines(lines)
        assert font is not None
        assert font.smushing_rules == SmushingRules.NONE

    def test_old_layout_zero_gives_no_rules(self):
        content = create_minimal_valid_font_content(2, "$")
        lines = content.split("\n")
        lines[0] = "flf2a$ 2 1 5 0 0"
        font = FIGFont.from_lines(lines)
        assert font is not None
        assert font.smushing_rules == SmushingRules.NONE

    def test_full_layout_takes_precedence(self):
        content = create_minimal_valid_font_content(2, "$")
        lines = content.split("\n")
        lines[0] = "flf2a$ 2 1 5 99 0 0 7"  # old=99, full=7
        font = FIGFont.from_lines(lines)
        assert font is not None
        assert font.full_layout == 7
        # full=7 -> bit0=1 (smushing on), bits1-2=3 -> EqualCharacter + Underscore
        assert_smushing_rule(font, SmushingRules.EqualCharacter, True)
        assert_smushing_rule(font, SmushingRules.Underscore, True)
        assert_smushing_rule(font, SmushingRules.Hierarchy, False)

    def test_full_layout_bit0_clear_disables_all_rules(self):
        content = create_minimal_valid_font_content(2, "$")
        lines = content.split("\n")
        lines[0] = "flf2a$ 2 1 5 0 0 0 126"  # full=126, bit0=0
        font = FIGFont.from_lines(lines)
        assert font is not None
        assert font.smushing_rules == SmushingRules.NONE


# ---------------------------------------------------------------------------
# Character map completeness
# ---------------------------------------------------------------------------

class TestCharacterMap:
    def test_default_font_contains_all_printable_ascii(self):
        font = FIGFont.default
        for i in range(32, 127):
            assert i in font.characters, f"Font missing ASCII {i} ('{chr(i)}')"
            assert len(font.characters[i]) == font.height, f"Wrong height for char {i}"


# ---------------------------------------------------------------------------
# Font parsing edge cases
# ---------------------------------------------------------------------------

class TestFontParsing:
    def test_comments_extracted_correctly(self):
        content = create_minimal_valid_font_content(2, "$")
        lines = content.split("\n")
        lines[0] = "flf2a$ 2 1 5 0 2"  # 2 comment lines
        lines.insert(1, "Comment Line 1")
        lines.insert(2, "Comment Line 2")
        font = FIGFont.from_lines(lines)
        assert font is not None
        assert font.comments == "Comment Line 1\nComment Line 2"

    def test_print_direction_parsed(self):
        content = create_minimal_valid_font_content(2, "$", print_direction=1)
        font = FIGFont.from_text(content)
        assert font is not None
        assert font.print_direction == 1

    def test_extended_unicode_character_parsed(self):
        base = create_minimal_valid_font_content(2)
        extended = base + "8364\nE@\nu@@\n"  # Euro U+20AC = 8364
        font = FIGFont.from_lines(extended.split("\n"))
        assert font is not None
        assert 8364 in font.characters
        assert len(font.characters[8364]) == 2

    def test_malformed_extended_character_stops_parsing(self):
        base = create_minimal_valid_font_content(2)
        extended = base + "invalid_code\nE@\nu@@\n"
        font = FIGFont.from_lines(extended.split("\n"))
        assert font is not None
        assert len(font.characters) == 95  # only standard ASCII

    def test_custom_hard_blank_character(self):
        content = create_minimal_valid_font_content(2, "@")
        font = FIGFont.from_text(content)
        assert font is not None
        assert font.hard_blank == "@"


# ---------------------------------------------------------------------------
# ZIP loading
# ---------------------------------------------------------------------------

class TestZIPLoading:
    def test_deflate_zip_loads_correctly(self):
        content = create_minimal_valid_font_content(2)
        zip_bytes = create_zip_with_font_file(content, "test.flf", store=False)
        font = FIGFont.from_stream(io.BytesIO(zip_bytes))
        assert font is not None
        assert font.height == 2
        assert len(font.characters) == 95

    def test_store_zip_loads_correctly(self):
        content = create_minimal_valid_font_content(3, "@")
        zip_bytes = create_zip_with_font_file(content, "test.flf", store=True)
        font = FIGFont.from_stream(io.BytesIO(zip_bytes))
        assert font is not None
        assert font.hard_blank == "@"
        assert font.height == 3

    def test_invalid_zip_bytes_returns_none(self):
        # "PK" magic header but invalid local-file record
        fake_zip = b"PK" + create_minimal_valid_font_content(2).encode("utf-8")
        font = FIGFont.from_stream(io.BytesIO(fake_zip))
        assert font is None

    def test_zip_file_via_from_file(self):
        content = create_minimal_valid_font_content(3, "@")
        zip_bytes = create_zip_with_font_file(content, "test.flf", store=False)
        with tempfile.NamedTemporaryFile(suffix=".zip", delete=False) as f:
            f.write(zip_bytes)
            tmp = f.name
        try:
            font = FIGFont.from_file(tmp)
            assert font is not None
            assert font.hard_blank == "@"
            assert font.height == 3
        finally:
            os.unlink(tmp)
