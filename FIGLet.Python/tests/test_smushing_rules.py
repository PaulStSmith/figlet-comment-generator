"""SmushingRules tests — mirrors C# SmushingRulesTests."""
from __future__ import annotations

import pytest

from byteforge_figlet import FIGFont, SmushingRules
from .test_utilities import create_minimal_valid_font_content, create_stream_from_string


class TestSmushingRuleValues:
    def test_none_is_zero(self):
        assert int(SmushingRules.NONE) == 0

    def test_equal_character_is_1(self):
        assert int(SmushingRules.EqualCharacter) == 1

    def test_underscore_is_2(self):
        assert int(SmushingRules.Underscore) == 2

    def test_hierarchy_is_4(self):
        assert int(SmushingRules.Hierarchy) == 4

    def test_opposite_pair_is_8(self):
        assert int(SmushingRules.OppositePair) == 8

    def test_big_x_is_16(self):
        assert int(SmushingRules.BigX) == 16

    def test_hard_blank_is_32(self):
        assert int(SmushingRules.HardBlank) == 32

    def test_all_flags_combine_to_63(self):
        all_flags = (
            SmushingRules.EqualCharacter
            | SmushingRules.Underscore
            | SmushingRules.Hierarchy
            | SmushingRules.OppositePair
            | SmushingRules.BigX
            | SmushingRules.HardBlank
        )
        assert int(all_flags) == 63

    def test_combination_of_three(self):
        combo = SmushingRules.EqualCharacter | SmushingRules.Underscore | SmushingRules.Hierarchy
        assert int(combo) == 7  # 1 + 2 + 4

    def test_each_is_power_of_two(self):
        assert int(SmushingRules.EqualCharacter) == 1   # 2^0
        assert int(SmushingRules.Underscore)     == 2   # 2^1
        assert int(SmushingRules.Hierarchy)      == 4   # 2^2
        assert int(SmushingRules.OppositePair)   == 8   # 2^3
        assert int(SmushingRules.BigX)           == 16  # 2^4
        assert int(SmushingRules.HardBlank)      == 32  # 2^5


@pytest.mark.parametrize("rule,expected", [
    (SmushingRules.NONE,           0),
    (SmushingRules.EqualCharacter, 1),
    (SmushingRules.Underscore,     2),
    (SmushingRules.Hierarchy,      4),
    (SmushingRules.OppositePair,   8),
    (SmushingRules.BigX,           16),
    (SmushingRules.HardBlank,      32),
])
def test_individual_values_match_expected(rule, expected):
    assert int(rule) == expected


class TestSmushingRuleBitOps:
    def test_has_rule_works(self):
        rules = SmushingRules.EqualCharacter | SmushingRules.BigX
        assert rules.has_rule(SmushingRules.EqualCharacter) is True
        assert rules.has_rule(SmushingRules.BigX) is True
        assert rules.has_rule(SmushingRules.Underscore) is False
        assert rules.has_rule(SmushingRules.Hierarchy) is False
        assert rules.has_rule(SmushingRules.OppositePair) is False
        assert rules.has_rule(SmushingRules.HardBlank) is False

    def test_bitwise_union_intersection_xor(self):
        r1 = SmushingRules.EqualCharacter | SmushingRules.Underscore
        r2 = SmushingRules.Underscore | SmushingRules.Hierarchy
        assert r1 | r2 == SmushingRules.EqualCharacter | SmushingRules.Underscore | SmushingRules.Hierarchy
        assert r1 & r2 == SmushingRules.Underscore
        assert r1 ^ r2 == SmushingRules.EqualCharacter | SmushingRules.Hierarchy

    def test_individual_flags_are_distinct(self):
        flags = [
            SmushingRules.EqualCharacter,
            SmushingRules.Underscore,
            SmushingRules.Hierarchy,
            SmushingRules.OppositePair,
            SmushingRules.BigX,
            SmushingRules.HardBlank,
        ]
        for i in range(len(flags)):
            for j in range(i + 1, len(flags)):
                assert int(flags[i]) != int(flags[j])
                assert (flags[i] & flags[j]) == SmushingRules.NONE


class TestSmushingRuleWithFont:
    def test_font_has_smushing_rule_works(self):
        content = create_minimal_valid_font_content()
        content = content.replace("flf2a$ 5 4 10 15 0", "flf2a$ 5 4 10 7 0")  # old=7 -> 1+2+4
        stream = create_stream_from_string(content)
        font = FIGFont.from_stream(stream)
        assert font is not None
        assert font.has_smushing_rule(SmushingRules.EqualCharacter) is True
        assert font.has_smushing_rule(SmushingRules.Underscore) is True
        assert font.has_smushing_rule(SmushingRules.Hierarchy) is True
        assert font.has_smushing_rule(SmushingRules.OppositePair) is False
        assert font.has_smushing_rule(SmushingRules.BigX) is False
        assert font.has_smushing_rule(SmushingRules.HardBlank) is False

    def test_smushing_rules_names(self):
        assert SmushingRules.NONE.name == "NONE"
        assert SmushingRules.EqualCharacter.name == "EqualCharacter"
        assert SmushingRules.Underscore.name == "Underscore"
        assert SmushingRules.Hierarchy.name == "Hierarchy"
        assert SmushingRules.OppositePair.name == "OppositePair"
        assert SmushingRules.BigX.name == "BigX"
        assert SmushingRules.HardBlank.name == "HardBlank"
