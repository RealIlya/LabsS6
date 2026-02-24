from __future__ import annotations

VARIANT_SCRAMBLERS: dict[int, tuple[str, str]] = {
    1: ("x8 + x7 + x6 + x3 + x2 + 1", "x8 + x5 + x3 + x2 + 1"),
    2: ("x9 + x3 + 1", "x9 + x4 + 1"),
    3: ("x10 + x5 + x4 + x2 + 1", "x10 + x7 + 1"),
    4: ("x5 + x4 + x2 + 1", "x5 + x2 + 1"),
    5: ("x11 + x5 + x2 + 1", "x11 + x2 + 1"),
    6: ("x7 + x5 + x2 + 1", "x7 + x + 1"),
    7: ("x12 + x7 + x3 + x + 1", "x12 + x6 + x4 + x + 1"),
    8: ("x8 + x6 + x2 + 1", "x8 + x4 + x3 + x2 + 1"),
    9: ("x11 + x3 + x2 + 1", "x11 + x10 + x9 + x2 + 1"),
    10: ("x6 + x5 + x + 1", "x6 + x + 1"),
}

