from core.analysis import analyze_lfsr
from core.lfsr import LFSR, parse_polynomial, scrambler_xor


def test_parse_polynomial() -> None:
    degree, taps = parse_polynomial("x9 + x3 + 1")
    assert degree == 9
    assert taps == [0, 3]


def test_lfsr_deterministic() -> None:
    a = LFSR("x9 + x3 + 1", seed=0b101011001)
    b = LFSR("x9 + x3 + 1", seed=0b101011001)
    assert a.generate_bits(64) == b.generate_bits(64)


def test_scrambler_xor_reversible() -> None:
    source = b"example data for variant2"
    seed = 0b101101001
    poly = "x9 + x4 + 1"
    encrypted = scrambler_xor(source, poly, seed)
    decrypted = scrambler_xor(encrypted, poly, seed)
    assert decrypted == source


def test_analysis_returns_period() -> None:
    stats = analyze_lfsr("x9 + x3 + 1", seed=0b100111001, sample_bits=256)
    assert stats.period > 0
    assert stats.chi_square >= 0

