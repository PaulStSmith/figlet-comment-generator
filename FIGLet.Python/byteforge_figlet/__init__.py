"""ByteForge FIGLet — Python FIGLet rendering library.

Quickstart::

    from byteforge_figlet import FIGFont, FIGLetRenderer, LayoutMode

    print(FIGLetRenderer.render("Hello!"))

Or use the bundled CLI::

    python -m byteforge_figlet "Hello World"
    figprint "Hello World" --font small
"""

from .fig_font import FIGFont
from .fig_let_renderer import FIGLetRenderer
from .layout_mode import LayoutMode
from .smushing_rules import SmushingRules

__version__ = "2.0.0"
__all__ = ["FIGFont", "FIGLetRenderer", "LayoutMode", "SmushingRules"]
