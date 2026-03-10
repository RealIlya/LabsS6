from __future__ import annotations

from dataclasses import dataclass

from core.lfsr import LFSR


@dataclass
class SequenceStats:
    degree: int
    effective_seed: int
    analyzed_bits: int
    period: int
    zeros: int
    ones: int
    chi_square: float
    balanced_delta: int
    cyclic_shifts_unique: bool
    max_abs_autocorrelation: float
    preview_bits: str


def _chi_square_for_bits(bits: list[int]) -> float:
    # Здесь сравниваем число нулей и единиц с ожидаемым распределением 50/50
    n = len(bits)
    if n == 0:
        return 0.0
    ones = sum(bits)
    zeros = n - ones
    expected = n / 2
    return ((zeros - expected) ** 2) / expected + ((ones - expected) ** 2) / expected


def _max_abs_autocorrelation(bits: list[int], max_lag: int | None = None) -> float:
    # Нулевой сдвиг не учитываем, ищем наибольшее сходство со сдвинутыми копиями
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


def analyze_lfsr(polynomial: str, seed: int) -> SequenceStats:
    # Метрики считаются на одном полном периоде, чтобы честно сравнивать разные фазы
    lfsr = LFSR(polynomial, seed)
    period = lfsr.period(max_steps=(1 << lfsr.degree))

    seq_lfsr = LFSR(polynomial, seed)
    analyzed_bits = max(period, 1)
    bits = seq_lfsr.generate_bits(analyzed_bits)
    preview_bits = "".join(str(bit) for bit in bits[: min(32, len(bits))])

    ones = sum(bits)
    zeros = len(bits) - ones
    chi_square = _chi_square_for_bits(bits)
    delta = abs(ones - zeros)

    period_bits = bits[:period] if period > 0 else bits
    cyclic_unique = _cyclic_shifts_unique(period_bits)
    max_corr = _max_abs_autocorrelation(period_bits)

    return SequenceStats(
        degree=lfsr.degree,
        effective_seed=lfsr.initial_state,
        analyzed_bits=len(bits),
        period=period,
        zeros=zeros,
        ones=ones,
        chi_square=chi_square,
        balanced_delta=delta,
        cyclic_shifts_unique=cyclic_unique,
        max_abs_autocorrelation=max_corr,
        preview_bits=preview_bits,
    )


def build_verdict(stats: SequenceStats) -> str:
    # Пороговые значения эвристические и нужны только для читаемого вывода
    max_period = (1 << stats.degree) - 1
    period_ratio = stats.period / max_period if max_period > 0 else 0.0
    balance_ratio = stats.balanced_delta / stats.analyzed_bits if stats.analyzed_bits > 0 else 1.0
    chi_ok = stats.chi_square <= 3.84
    corr = stats.max_abs_autocorrelation

    findings: list[tuple[str, str]] = []
    score = 0

    if period_ratio >= 0.95:
        findings.append(("Период", f"хороший: {stats.period} из {max_period}"))
        score += 2
    elif period_ratio >= 0.5:
        findings.append(("Период", f"средний: {stats.period} из {max_period}"))
        score += 1
    else:
        findings.append(("Период", f"слабый: {stats.period} из {max_period}"))

    if balance_ratio <= 0.05:
        findings.append(("Баланс 0/1", f"хороший: разница {stats.balanced_delta} бит"))
        score += 2
    elif balance_ratio <= 0.15:
        findings.append(("Баланс 0/1", f"приемлемый: разница {stats.balanced_delta} бит"))
        score += 1
    else:
        findings.append(("Баланс 0/1", f"слабый: разница {stats.balanced_delta} бит"))

    if chi_ok:
        findings.append(("Равномерность", f"chi^2 = {stats.chi_square:.4f}, явного перекоса не видно"))
        score += 2
    else:
        findings.append(("Равномерность", f"chi^2 = {stats.chi_square:.4f}, распределение заметно перекошено"))

    if stats.cyclic_shifts_unique:
        findings.append(("Цикличность", "сдвиги уникальны, цикл ведет себя корректно"))
        score += 2
    else:
        findings.append(("Цикличность", "есть повторы сдвигов, это плохой признак"))

    if corr <= 0.2:
        findings.append(("Автокорреляция", f"низкая: {corr:.4f}"))
        score += 2
    elif corr <= 0.35:
        findings.append(("Автокорреляция", f"умеренная: {corr:.4f}"))
        score += 1
    else:
        findings.append(("Автокорреляция", f"высокая: {corr:.4f}"))

    if score >= 9:
        overall = "Итог: последовательность хорошего качества для учебного скремблера."
    elif score >= 6:
        overall = "Итог: последовательность приемлемая, но показатели не идеальны."
    else:
        overall = "Итог: последовательность слабая, полином или seed стоит заменить."

    lines = ["Вердикт:"]
    for name, text in findings:
        lines.append(f"- {name}: {text}.")
    lines.append(overall)
    return "\n".join(lines)
