from __future__ import annotations

from pathlib import Path


def read_bytes(path: str | Path) -> bytes:
    return Path(path).read_bytes()


def write_bytes(path: str | Path, data: bytes) -> None:
    Path(path).write_bytes(data)

