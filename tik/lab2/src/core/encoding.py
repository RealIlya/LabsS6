from __future__ import annotations


def bytes_to_text(data: bytes) -> str:
    """Lossless one-byte to one-char mapping for reversible symbolic view."""
    return data.decode("latin-1")


def text_to_bytes(value: str) -> bytes:
    return value.encode("latin-1")


def bytes_to_hex(data: bytes) -> str:
    return " ".join(f"{b:02X}" for b in data)


def hex_to_bytes(value: str) -> bytes:
    cleaned = value.replace(" ", "").replace("\n", "").replace("\t", "").replace("\r", "")
    if not cleaned:
        return b""
    if len(cleaned) % 2 != 0:
        raise ValueError("HEX-строка должна содержать чётное число символов.")
    try:
        return bytes.fromhex(cleaned)
    except ValueError as exc:
        raise ValueError("Некорректный HEX-формат.") from exc


def bytes_to_bin(data: bytes) -> str:
    return " ".join(f"{b:08b}" for b in data)


def bin_to_bytes(value: str) -> bytes:
    cleaned = value.replace(" ", "").replace("\n", "").replace("\t", "").replace("\r", "")
    if not cleaned:
        return b""
    if any(ch not in "01" for ch in cleaned):
        raise ValueError("BIN-строка может содержать только 0 и 1.")
    if len(cleaned) % 8 != 0:
        raise ValueError("BIN-строка должна содержать число бит, кратное 8.")
    out = bytearray()
    for idx in range(0, len(cleaned), 8):
        out.append(int(cleaned[idx : idx + 8], 2))
    return bytes(out)

