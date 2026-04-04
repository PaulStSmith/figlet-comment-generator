"""FIGLetRenderer — renders text using a FIGFont."""

from __future__ import annotations

import os
import re
from typing import Dict, List, Optional

from .fig_font import FIGFont
from .layout_mode import LayoutMode
from .smushing_rules import SmushingRules

_HIERARCHY_CHARS = r"|/\[]{}()<>"
_OPPOSITE_PAIRS: Dict[str, str] = {
    "[": "]", "]": "[",
    "{": "}", "}": "{",
    "(": ")", ")": "(",
    "<": ">", ">": "<",
}
_ANSI_COLOR_RESET = "\x1b[0m"

_RE_LAST_NON_WS = re.compile(r"\S(?=\s*$)")
_RE_FIRST_NON_WS = re.compile(r"\S")


class FIGLetRenderer:
    """Renders strings to multi-line ASCII art using a FIGFont."""

    def __init__(
        self,
        font: Optional[FIGFont] = None,
        mode: LayoutMode = LayoutMode.Smushing,
        line_separator: Optional[str] = None,
        use_ansi_colors: bool = False,
        paragraph_mode: bool = True,
    ) -> None:
        self.font: FIGFont = font if font is not None else FIGFont.default
        self.layout_mode: LayoutMode = mode
        self.line_separator: str = line_separator if line_separator is not None else os.linesep
        self.use_ansi_colors: bool = use_ansi_colors
        self.paragraph_mode: bool = paragraph_mode

    # ------------------------------------------------------------------ #
    # Static convenience method
    # ------------------------------------------------------------------ #
    @staticmethod
    def render(
        text: str,
        font: Optional[FIGFont] = None,
        mode: LayoutMode = LayoutMode.Smushing,
        line_separator: Optional[str] = None,
        use_ansi_colors: bool = False,
        paragraph_mode: bool = True,
    ) -> str:
        """Render *text* and return the ASCII-art string."""
        if not text:
            return ""
        renderer = FIGLetRenderer(font, mode, line_separator, use_ansi_colors, paragraph_mode)
        return renderer.render_text(text)

    # ------------------------------------------------------------------ #
    # Instance render
    # ------------------------------------------------------------------ #
    def render_text(self, text: str) -> str:
        """Render *text* using the current font and settings."""
        if not text:
            return ""

        sep = self.line_separator

        if self.paragraph_mode:
            empty_line = sep * self.font.height
            paragraphs = text.replace("\r\n", "\n").split("\n")
            parts: List[str] = []
            for para in paragraphs:
                if not para.strip():
                    parts.append(empty_line)
                else:
                    parts.append(self._render_line(para))
            return "".join(parts)

        clean = text.replace("\r", "").replace("\n", " ")
        return self._render_line(clean)

    # ------------------------------------------------------------------ #
    # Core line renderer
    # ------------------------------------------------------------------ #
    def _render_line(self, text: str) -> str:
        font = self.font
        mode = self.layout_mode
        sep = self.line_separator
        use_colors = self.use_ansi_colors

        output_lines: List[List[str]] = [[] for _ in range(font.height)]

        # First pass: strip ANSI, build plain text + color map
        plain_chars: List[str] = []
        color_dict: Dict[int, str] = {}
        ansi = _ANSIProcessor(preserve_colors=use_colors)

        for ch in text:
            is_ansi = ansi.process_char(ch)
            if is_ansi:
                continue
            if use_colors and ansi.current_color_sequence:
                color_dict[len(plain_chars)] = ansi.current_color_sequence
                ansi.reset_color_state()
            if ord(ch) in font.characters:
                plain_chars.append(ch)

        plain_text = "".join(plain_chars)

        # RTL: reverse text and color map
        if font.print_direction == 1:
            plain_text = plain_text[::-1]
            reversed_colors: Dict[int, str] = {}
            for pos, seq in color_dict.items():
                reversed_colors[len(plain_text) - pos - 1] = seq
            color_dict = reversed_colors

        # Second pass: composite characters
        char_index = 0
        i = 0
        while i < len(plain_text):
            cp = ord(plain_text[i])
            i += 1

            char_lines = font.characters[cp]
            color_code = color_dict.get(char_index, "")
            char_index += 1

            if not output_lines[0]:
                # First character
                for li in range(font.height):
                    row: List[str] = []
                    if use_colors:
                        row.append(color_code)
                    row.append(char_lines[li])
                    output_lines[li] = row
            else:
                # Calculate minimum overlap across all rows
                overlap = None
                for li in range(font.height):
                    existing = "".join(output_lines[li])
                    ov = self._calculate_overlap(existing, char_lines[li], mode)
                    overlap = ov if overlap is None else min(overlap, ov)

                assert overlap is not None

                # Apply smushing
                for li in range(font.height):
                    existing = "".join(output_lines[li])
                    self._smush(output_lines[li], existing, char_lines[li], overlap, mode, color_code)

        # Build final string
        rows: List[str] = []
        for li in range(font.height):
            row = "".join(output_lines[li]).replace(font.hard_blank, " ")
            if use_colors:
                row += _ANSI_COLOR_RESET
            rows.append(row)

        return sep.join(rows) + sep

    # ------------------------------------------------------------------ #
    # Smushing helpers
    # ------------------------------------------------------------------ #
    def _smush(
        self,
        line_parts: List[str],
        existing: str,
        character: str,
        overlap: int,
        mode: LayoutMode,
        color_code: str,
    ) -> None:
        """Mutate *line_parts* in-place to smush *character* into the existing line."""
        # Rebuild without trailing overlap
        trimmed = existing[: len(existing) - overlap]
        line_end = existing[len(existing) - overlap :]

        line_parts.clear()

        if mode == LayoutMode.Kerning:
            line_parts.append(trimmed)
            if self.use_ansi_colors:
                line_parts.append(color_code)
            line_parts.append(character)
            return

        line_parts.append(trimmed)
        for idx in range(overlap):
            c1 = line_end[idx] if idx < len(line_end) else " "
            c2 = character[idx] if idx < len(character) else " "
            line_parts.append(
                self._smush_characters(c1, c2, self.font.hard_blank, mode, self.font.smushing_rules)
            )
        if self.use_ansi_colors:
            line_parts.append(color_code)
        line_parts.append(character[overlap:])

    def _calculate_overlap(self, line: str, character: str, mode: LayoutMode) -> int:
        if mode == LayoutMode.FullSize:
            return 0

        eol = line if len(line) <= len(character) else line[len(line) - len(character):]
        m1 = _RE_LAST_NON_WS.search(eol)
        m2 = _RE_FIRST_NON_WS.search(character)

        if not m1 or not m2:
            return len(character)

        can = self._can_smush(m1.group()[0], m2.group()[0], self.font.hard_blank, mode, self.font.smushing_rules)
        overlap = max(len(eol) - m1.start(), m2.start()) + 1 if can else 0
        overlap = min(overlap, len(character))

        # Special case: opposing slashes reduce overlap by 1
        if can and (
            (m1.group()[0] == "/" and m2.group()[0] == "\\")
            or (m1.group()[0] == "\\" and m2.group()[0] == "/")
        ):
            overlap = max(overlap - 1, 0)

        return overlap

    @staticmethod
    def _can_smush(
        c1: str, c2: str, hard_blank: str, mode: LayoutMode, rules: SmushingRules
    ) -> bool:
        if mode == LayoutMode.Kerning:
            return c1 == c2 == " "
        if mode == LayoutMode.FullSize:
            return False
        if c1 == hard_blank or c2 == hard_blank:
            return rules.has_rule(SmushingRules.HardBlank)
        if c1 == " " and c2 == " ":
            return True
        if c1 == " " or c2 == " ":
            return True
        if rules.has_rule(SmushingRules.EqualCharacter) and c1 == c2:
            return True
        if rules.has_rule(SmushingRules.Underscore):
            if (c1 == "_" and c2 in _HIERARCHY_CHARS) or (c2 == "_" and c1 in _HIERARCHY_CHARS):
                return True
        if rules.has_rule(SmushingRules.Hierarchy):
            if c1 in _HIERARCHY_CHARS and c2 in _HIERARCHY_CHARS:
                return True
        if rules.has_rule(SmushingRules.OppositePair):
            if _OPPOSITE_PAIRS.get(c1) == c2:
                return True
        if rules.has_rule(SmushingRules.BigX):
            if (c1 == ">" and c2 == "<") or (c1 == "/" and c2 == "\\") or (c1 == "\\" and c2 == "/"):
                return True
        return False

    @staticmethod
    def _smush_characters(
        c1: str, c2: str, hard_blank: str, mode: LayoutMode, rules: SmushingRules
    ) -> str:
        if mode == LayoutMode.Kerning:
            return c1
        if c1 == " " and c2 == " ":
            return " "
        if c1 == " ":
            return c2
        if c2 == " ":
            return c1
        if c1 == hard_blank or c2 == hard_blank:
            if rules.has_rule(SmushingRules.HardBlank):
                return hard_blank
            return c1
        if rules.has_rule(SmushingRules.EqualCharacter) and c1 == c2:
            return c1
        if rules.has_rule(SmushingRules.Underscore):
            if c1 == "_" and c2 in _HIERARCHY_CHARS:
                return c2
            if c2 == "_" and c1 in _HIERARCHY_CHARS:
                return c1
        if rules.has_rule(SmushingRules.Hierarchy):
            rank1 = _HIERARCHY_CHARS.index(c1) if c1 in _HIERARCHY_CHARS else -1
            rank2 = _HIERARCHY_CHARS.index(c2) if c2 in _HIERARCHY_CHARS else -1
            if rank1 >= 0 and rank2 >= 0:
                return _HIERARCHY_CHARS[max(rank1, rank2)]
        if rules.has_rule(SmushingRules.OppositePair):
            if _OPPOSITE_PAIRS.get(c1) == c2:
                return "|"
        if rules.has_rule(SmushingRules.BigX):
            if c1 == "/" and c2 == "\\":
                return "|"
            if c1 == "\\" and c2 == "/":
                return "Y"
            if c1 == ">" and c2 == "<":
                return "X"
        if rules.has_rule(SmushingRules.HardBlank):
            if c1 == hard_blank and c2 == hard_blank:
                return hard_blank
        return c1


# --------------------------------------------------------------------------- #
# Internal ANSI escape sequence processor
# --------------------------------------------------------------------------- #
class _ANSIProcessor:
    _NON_COLOR_TERMINATORS = set("ABCDEFGHfJKSTsunhlitrP@XLM")

    def __init__(self, preserve_colors: bool = False) -> None:
        self._preserve = preserve_colors
        self._in_esc = False
        self._buf: List[str] = []
        self.current_color_sequence: str = ""

    def process_char(self, ch: str) -> bool:
        if ch == "\x1b":
            self._in_esc = True
            self._buf = [ch]
            return True
        if self._in_esc:
            self._buf.append(ch)
            if len(self._buf) == 2 and ch != "[":
                self._in_esc = False
                return True
            if len(self._buf) >= 3 and (("\x40" <= ch <= "\x7e") or ch == "m"):
                self._in_esc = False
                seq = "".join(self._buf)
                self._buf = []
                if ch == "m" and self._preserve:
                    self.current_color_sequence += seq
                return True
            return True
        return False

    def reset_color_state(self) -> None:
        self.current_color_sequence = ""
