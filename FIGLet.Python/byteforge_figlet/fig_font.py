"""FIGFont parser — loads and exposes .flf font files."""

from __future__ import annotations

import importlib.resources
import io
import zipfile
from typing import Dict, List, Optional

from .smushing_rules import SmushingRules


class FIGFont:
    """Represents a FIGfont loaded from a .flf file."""

    # ------------------------------------------------------------------ #
    # Class-level default font cache
    # ------------------------------------------------------------------ #
    _default: Optional["FIGFont"] = None

    @classmethod
    @property
    def default(cls) -> "FIGFont":
        """Return the built-in 'small' FIGfont (loaded once and cached)."""
        if cls._default is None:
            cls._default = cls._load_default_font()
        if cls._default is None:
            raise RuntimeError("Default FIGfont could not be loaded")
        return cls._default

    # ------------------------------------------------------------------ #
    # Instance attributes
    # ------------------------------------------------------------------ #
    def __init__(self) -> None:
        self.signature: str = "flf2a"
        self.hard_blank: str = "#"
        self.height: int = 0
        self.baseline: int = 0
        self.max_length: int = 0
        self.old_layout: int = 0
        self.print_direction: int = 0
        self.full_layout: int = 0
        self.characters: Dict[int, List[str]] = {}
        self.smushing_rules: SmushingRules = SmushingRules.NONE
        self.comments: str = ""

    # ------------------------------------------------------------------ #
    # Factory methods
    # ------------------------------------------------------------------ #
    @classmethod
    def from_file(cls, path: Optional[str]) -> Optional["FIGFont"]:
        """Load a FIGFont from a file path (supports .flf and .zip)."""
        if not path:
            return None
        with open(path, "rb") as fh:
            return cls.from_stream(fh)

    @classmethod
    def from_stream(cls, stream: Optional[io.RawIOBase]) -> Optional["FIGFont"]:
        """Load a FIGFont from a binary stream (supports .flf inside .zip)."""
        if stream is None:
            return None
        data = stream.read()
        # Detect ZIP archive
        if data[:2] == b"PK":
            try:
                zf_obj = zipfile.ZipFile(io.BytesIO(data))
            except zipfile.BadZipFile:
                return None
            with zf_obj as zf:
                flf_names = [n for n in zf.namelist() if n.lower().endswith(".flf")]
                if not flf_names:
                    return None
                with zf.open(flf_names[0]) as entry:
                    data = entry.read()
        # Decode using latin-1 (FIGlet standard)
        try:
            text = data.decode("latin-1")
        except UnicodeDecodeError:
            text = data.decode("utf-8", errors="replace")
        return cls.from_lines(text.splitlines())

    @classmethod
    def from_text(cls, text: Optional[str]) -> Optional["FIGFont"]:
        """Load a FIGFont from a string containing the full .flf content."""
        if not text:
            return None
        return cls.from_lines(text.splitlines())

    @classmethod
    def from_lines(cls, lines: Optional[List[str]]) -> Optional["FIGFont"]:
        """Load a FIGFont from a list of lines."""
        if not lines:
            return None

        font = cls()

        # --- Parse header ---
        header = lines[0].rstrip()
        if not header.startswith("flf2a"):
            raise ValueError("Invalid FIGfont format: header must start with 'flf2a'")

        try:
            parts = header.split(" ")
            font.signature = parts[0]
            font.hard_blank = parts[0][5] if len(parts[0]) > 5 else "#"
            font.height = int(parts[1])
            font.baseline = int(parts[2])
            font.max_length = int(parts[3])
            font.old_layout = int(parts[4])
            comment_lines = int(parts[5])
            if len(parts) > 6:
                try:
                    font.print_direction = int(parts[6])
                except ValueError:
                    pass
            if len(parts) > 7:
                try:
                    font.full_layout = int(parts[7])
                except ValueError:
                    pass
        except (IndexError, ValueError) as exc:
            raise ValueError(f"Error parsing FIGfont header: {exc}") from exc

        # --- Collect comments ---
        font.comments = "\n".join(lines[1 : 1 + comment_lines])

        current_line = 1 + comment_lines

        # --- Load required characters ASCII 32–126 ---
        for code_point in range(32, 127):
            char_lines = cls._read_char_lines(lines, font, current_line)
            font.characters[code_point] = char_lines
            current_line += font.height

        # --- Load optional extra characters ---
        while current_line < len(lines):
            code_line = lines[current_line].strip()
            if not code_line or not (code_line[0].isdigit() or code_line.startswith("-")):
                break
            # The code point is the first token; optional comment follows
            token = code_line.split(None, 1)[0]
            try:
                code_point = cls._parse_int(token)
            except ValueError:
                break
            current_line += 1
            if current_line + font.height > len(lines):
                break
            char_lines = cls._read_char_lines(lines, font, current_line)
            font.characters[code_point] = char_lines
            current_line += font.height

        # --- Derive smushing rules ---
        cls._parse_layout_parameters(font)

        return font

    # ------------------------------------------------------------------ #
    # Helpers
    # ------------------------------------------------------------------ #
    @staticmethod
    def _read_char_lines(lines: List[str], font: "FIGFont", start: int) -> List[str]:
        result: List[str] = []
        for i in range(font.height):
            raw = lines[start + i] if start + i < len(lines) else ""
            # Strip trailing @ markers and newline characters
            stripped = raw.rstrip("\r\n").rstrip("@")
            # Workaround: some fonts use '#' as EOL even when hardblank != '#'
            if stripped.endswith("#") and font.hard_blank != "#":
                stripped = stripped.rstrip("#")
            result.append(stripped)
        return result

    @staticmethod
    def _parse_int(text: str) -> int:
        text = text.strip()
        if not text:
            return 0
        if text.startswith("-"):
            return -FIGFont._parse_int(text[1:])
        if text.lower().startswith("0x"):
            return int(text[2:], 16)
        if text.lower().startswith("0b"):
            return int(text[2:], 2)
        if len(text) > 1 and text.startswith("0"):
            return int(text, 8)
        return int(text)

    @staticmethod
    def _parse_layout_parameters(font: "FIGFont") -> None:
        if font.full_layout > 0:
            layout_mask = font.full_layout
            horizontal_smushing_enabled = (layout_mask & 1) == 1
            if not horizontal_smushing_enabled:
                font.smushing_rules = SmushingRules.NONE
                return
            layout_mask = (layout_mask >> 1) & 0x3F
        else:
            layout_mask = font.old_layout
            if layout_mask <= 0:
                font.smushing_rules = SmushingRules.NONE
                return
            layout_mask &= 0x3F

        font.smushing_rules = SmushingRules(layout_mask)

    @classmethod
    def _load_default_font(cls) -> Optional["FIGFont"]:
        try:
            pkg = importlib.resources.files("byteforge_figlet") / "fonts" / "small.flf"
            data = pkg.read_bytes()
            return cls.from_stream(io.BytesIO(data))
        except Exception:
            return None

    # ------------------------------------------------------------------ #
    # Public helpers
    # ------------------------------------------------------------------ #
    def has_smushing_rule(self, rule: SmushingRules) -> bool:
        """Return True if the font defines the given smushing rule."""
        return self.smushing_rules.has_rule(rule)
