from __future__ import annotations

from dataclasses import dataclass

from core.lfsr import LFSR


@dataclass
class SequenceStats:
    period: int
    zeros: int
    ones: int
    chi_square: float
    balanced_delta: int
    cyclic_shifts_unique: bool
    max_abs_autocorrelation: float


def _chi_square_for_bits(bits: list[int]) -> float:
    n = len(bits)
    if n == 0:
        return 0.0
    ones = sum(bits)
    zeros = n - ones
    expected = n / 2
    return ((zeros - expected) ** 2) / expected + ((ones - expected) ** 2) / expected


def _max_abs_autocorrelation(bits: list[int], max_lag: int | None = None) -> float:
    n = len(bits)
    if n < 2:
        return 0.0
    bipolar = [1 if b else -1 for b in bits]
    if max_lag is None:
        max_lag = min(n - 1, 128)
    max_corr = 0.0
    for lag in range(1, max_lag + 1):
        total = 0
        for i in range(n):
            total += bipolar[i] * bipolar[(i + lag) % n]
        corr = total / n
        max_corr = max(max_corr, abs(corr))
    return max_corr


def _cyclic_shifts_unique(bits: list[int]) -> bool:
    n = len(bits)
    if n == 0:
        return False
    rotations = set()
    for shift in range(n):
        key = tuple(bits[shift:] + bits[:shift])
        rotations.add(key)
    return len(rotations) == n


def analyze_lfsr(polynomial: str, seed: int, sample_bits: int = 2048) -> SequenceStats:
    lfsr = LFSR(polynomial, seed)
    period = lfsr.period(max_steps=(1 << lfsr.degree))

    seq_lfsr = LFSR(polynomial, seed)
    bits = seq_lfsr.generate_bits(max(sample_bits, period))

    ones = sum(bits)
    zeros = len(bits) - ones
    chi_square = _chi_square_for_bits(bits)
    delta = abs(ones - zeros)

    period_bits = bits[:period] if period > 0 else bits
    cyclic_unique = _cyclic_shifts_unique(period_bits)
    max_corr = _max_abs_autocorrelation(period_bits)

    return SequenceStats(
        period=period,
        zeros=zeros,
        ones=ones,
        chi_square=chi_square,
        balanced_delta=delta,
        cyclic_shifts_unique=cyclic_unique,
        max_abs_autocorrelation=max_corr,
    )

