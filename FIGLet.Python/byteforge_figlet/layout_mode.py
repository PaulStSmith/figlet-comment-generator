"""Layout mode enumeration for FIGLet rendering."""

from enum import IntEnum


class LayoutMode(IntEnum):
    """Controls how FIGLet characters are horizontally arranged."""

    FullSize = -1
    """Characters are placed side by side with no overlap."""

    Kerning = 0
    """Characters are moved as close together as possible without overlapping."""

    Smushing = 1
    """Characters may overlap and be merged according to smushing rules."""

    Default = 1  # alias for Smushing
