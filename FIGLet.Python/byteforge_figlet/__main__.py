"""CLI entry point — ``python -m byteforge_figlet``."""

from __future__ import annotations

import argparse
import sys

from .fig_font import FIGFont
from .fig_let_renderer import FIGLetRenderer
from .layout_mode import LayoutMode


def _parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        prog="figprint",
        description="Render text as ASCII art using FIGLet fonts.",
    )
    parser.add_argument("text", nargs="?", help="Text to render (reads stdin if omitted)")
    parser.add_argument(
        "-f", "--font",
        metavar="PATH",
        default=None,
        help="Path to a .flf font file (default: built-in 'small')",
    )
    parser.add_argument(
        "-m", "--mode",
        choices=["full", "kerning", "smush"],
        default="smush",
        help="Layout mode (default: smush)",
    )
    parser.add_argument(
        "--width",
        type=int,
        default=0,
        help="Target output width in characters (0 = unlimited)",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = _parse_args(argv)

    text = args.text
    if not text:
        if sys.stdin.isatty():
            print("Usage: figprint <text> [--font PATH] [--mode full|kerning|smush]", file=sys.stderr)
            return 1
        text = sys.stdin.read().rstrip("\n")

    font = FIGFont.from_file(args.font) if args.font else None

    mode_map = {"full": LayoutMode.FullSize, "kerning": LayoutMode.Kerning, "smush": LayoutMode.Smushing}
    mode = mode_map[args.mode]

    result = FIGLetRenderer.render(text, font=font, mode=mode, line_separator="\n")
    print(result, end="")
    return 0


if __name__ == "__main__":
    sys.exit(main())
