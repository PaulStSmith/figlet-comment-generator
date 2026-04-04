"""Smushing rules flags for FIGLet rendering."""

from enum import IntFlag


class SmushingRules(IntFlag):
    """Flags that control which smushing rules are applied during rendering."""

    NONE = 0
    """No smushing rules apply."""

    EqualCharacter = 1
    """Rule 1: Two identical characters smush into one."""

    Underscore = 2
    """Rule 2: An underscore is replaced by any character in the hierarchy."""

    Hierarchy = 4
    """Rule 3: A character of higher hierarchy replaces one of lower hierarchy."""

    OppositePair = 8
    """Rule 4: Opposing brackets/braces/parens smush into a vertical bar."""

    BigX = 16
    """Rule 5: /+\\ becomes |, \\+/ becomes Y, >+< becomes X."""

    HardBlank = 32
    """Rule 6: Two hardblanks smush into one hardblank."""

    def has_rule(self, rule: "SmushingRules") -> bool:
        """Return True if this set of rules includes the given rule."""
        return bool(self & rule)
