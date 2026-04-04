"""Shared test utilities — mirrors C# TestUtilities and TS TestUtilities."""
from __future__ import annotations

import io
import random
import re
import zipfile
from pathlib import Path
from typing import Dict, Optional

from byteforge_figlet import FIGFont, SmushingRules


_TEST_FONTS_DIR = Path(__file__).parent / "TestFonts"


def create_minimal_valid_font_content(
    height: int = 5, hard_blank: str = "$", print_direction: int = 0
) -> str:
    """Return a string containing a minimal valid .flf font for testing."""
    dir_suffix = f" {print_direction}" if print_direction != 0 else ""
    lines = [f"flf2a{hard_blank} {height} 4 10 15 0{dir_suffix}"]
    for char_code in range(32, 127):
        for line_idx in range(height):
            if char_code == 32:  # space — use hard_blank
                lines.append(f"{hard_blank}@@" if line_idx == height - 1 else f"{hard_blank}@")
            elif line_idx == height - 1:
                lines.append(f"{chr(char_code)}@@")
            else:
                lines.append(f"{chr(char_code)}@")
    return "\n".join(lines) + "\n"


def create_stream_from_string(content: str) -> io.BytesIO:
    """Return a BytesIO stream from a string (UTF-8 encoded)."""
    return io.BytesIO(content.encode("utf-8"))


def create_zip_with_font_file(
    font_content: str,
    filename: str = "test.flf",
    store: bool = False,
) -> bytes:
    """Return a ZIP archive (bytes) containing the given font content."""
    buf = io.BytesIO()
    compression = zipfile.ZIP_STORED if store else zipfile.ZIP_DEFLATED
    with zipfile.ZipFile(buf, "w", compression=compression) as zf:
        zf.writestr(filename, font_content.encode("utf-8"))
    return buf.getvalue()


def load_test_font(font_name: str) -> FIGFont:
    """Load a test font from tests/TestFonts/<font_name>.flf."""
    path = _TEST_FONTS_DIR / f"{font_name}.flf"
    font = FIGFont.from_file(str(path))
    if font is None:
        if font_name != "mini-fixed":
            font = FIGFont.from_file(str(_TEST_FONTS_DIR / "mini-fixed.flf"))
        if font is None:
            raise RuntimeError(f"Failed to load test font: {font_name}")
    return font


def assert_smushing_rule(font: FIGFont, rule: SmushingRules, should_have: bool = True) -> None:
    has = font.has_smushing_rule(rule)
    if should_have:
        assert has, f"Font should have smushing rule: {rule}"
    else:
        assert not has, f"Font should NOT have smushing rule: {rule}"


def assert_multi_line_equal(expected: str, actual: str, msg: str = "") -> None:
    exp_lines = expected.replace("\r", "").split("\n")
    act_lines = actual.replace("\r", "").split("\n")
    assert len(exp_lines) == len(act_lines), (
        f"{msg}Line count mismatch. Expected {len(exp_lines)}, got {len(act_lines)}"
    )
    for i, (e, a) in enumerate(zip(exp_lines, act_lines)):
        assert e == a, f"{msg}Line {i + 1} mismatch.\n  Expected: {e!r}\n  Actual:   {a!r}"


def strip_ansi_colors(text: str) -> str:
    return re.sub(r"\x1B\[[0-?]*[ -/]*[@-~]", "", text)


def generate_large_text(length: int) -> str:
    chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 "
    rng = random.Random(42)
    return "".join(rng.choice(chars) for _ in range(length))


# ---------------------------------------------------------------------------
# Static test vectors (using mini-fixed font, smushing mode, '\n' separator)
# ---------------------------------------------------------------------------
TEST_TEXTS: Dict[str, str] = {
    "Hello":                "Helo\nHelo\nHelo\n",
    "World":                "World\nWorld\nWorld\n",
    "Test":                 "Test\nTest\nTest\n",
    "123":                  "123\n123\n123\n",
    "ABC":                  "ABC\nABC\nABC\n",
    "!@#$%^&*()":           "! %^&*)\n! %^&*)\n  %^&*)\n",
    "The quick brown fox":  "The quick brown fox\nThe quick brown fox\nThe quick brown fox\n",
    "a":                    "a\na\na\n",
}

ANSI_COLORED_TEXTS: Dict[str, str] = {
    "\x1b[31mRed\x1b[0m":
        "\x1b[31mRed\x1b[0m\n\x1b[31mRed\x1b[0m\n\x1b[31mRed\x1b[0m\n",
    "\x1b[32mGreen\x1b[33mYellow\x1b[0m":
        "\x1b[32mGren\x1b[33mYelow\x1b[0m\n\x1b[32mGren\x1b[33mYelow\x1b[0m\n\x1b[32mGren\x1b[33mYelow\x1b[0m\n",
    "\x1b[1;4;31mBold Underline Red\x1b[0m":
        "\x1b[1;4;31mBold Underline Red\x1b[0m\n\x1b[1;4;31mBold Underline Red\x1b[0m\n\x1b[1;4;31mBold Underline Red\x1b[0m\n",
    "Normal\x1b[96mCyan\x1b[0mBack":
        "Normal\x1b[96mCyan\x1b[0mBack\x1b[0m\nNormal\x1b[96mCyan\x1b[0mBack\x1b[0m\nNormal\x1b[96mCyan\x1b[0mBack\x1b[0m\n",
    "\x1b[38;5;196mRGB Red\x1b[0m":
        "\x1b[38;5;196mRGB Red\x1b[0m\n\x1b[38;5;196mRGB Red\x1b[0m\n\x1b[38;5;196mRGB Red\x1b[0m\n",
    "\x1b[48;2;255;0;0mTrue Color BG\x1b[0m":
        "\x1b[48;2;255;0;0mTrue Color BG\x1b[0m\n\x1b[48;2;255;0;0mTrue Color BG\x1b[0m\n\x1b[48;2;255;0;0mTrue Color BG\x1b[0m\n",
}

# Real-world banner scenarios (default small font, '\n' separator)
BANNER_SCENARIOS = [
    ("ERROR",   "  ___ ___ ___  ___  ___ \n | __| _ \\ _ \\/ _ \\| _ \\\n | _||   /   / (_) |   /\n |___|_|_\\_|_\\\\___/|_|_\\\n                        \n"),
    ("SUCCESS", "  ___ _   _  ___ ___ ___ ___ ___ \n / __| | | |/ __/ __| __/ __/ __|\n \\__ \\ |_| | (_| (__| _|\\__ \\__ \\\n |___/\\___/ \\___\\___|___|___/___/\n                                 \n"),
    ("WARNING", " __      __ _   ___ _  _ ___ _  _  ___ \n \\ \\    / //_\\ | _ \\ \\| |_ _| \\| |/ __|\n  \\ \\/\\/ // _ \\|   / .` || || .` | (_ |\n   \\_/\\_//_/ \\_\\_|_\\_|\\_|___|_|\\_|\\___|\n                                       \n"),
    ("DEBUG",   "  ___  ___ ___ _   _  ___ \n |   \\| __| _ ) | | |/ __|\n | |) | _|| _ \\ |_| | (_ |\n |___/|___|___/\\___/ \\___|\n                          \n"),
    ("TODO",    "  _____ ___  ___   ___  \n |_   _/ _ \\|   \\ / _ \\ \n   | || (_) | |) | (_) |\n   |_| \\___/|___/ \\___/ \n                        \n"),
    ("FIX ME",  "  ___ _____  __    __  __ ___ \n | __|_ _\\ \\/ /   |  \\/  | __|\n | _| | | >  <    | |\\/| | _| \n |_| |___/_/\\_\\   |_|  |_|___|\n                              \n"),
]
