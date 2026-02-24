from core.encoding import bin_to_bytes, bytes_to_bin, bytes_to_hex, hex_to_bytes


def test_hex_roundtrip() -> None:
    data = b"\x00\xA1\xFE"
    encoded = bytes_to_hex(data)
    assert hex_to_bytes(encoded) == data


def test_bin_roundtrip() -> None:
    data = b"\x01\x7F"
    encoded = bytes_to_bin(data)
    assert bin_to_bytes(encoded) == data

