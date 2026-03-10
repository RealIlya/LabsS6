from __future__ import annotations

from pathlib import Path


def read_bytes(path: str | Path) -> bytes:
    # Файловый ввод-вывод вынесен отдельно, чтобы не смешивать его с core и UI
    return Path(path).read_bytes()


def write_bytes(path: str | Path, data: bytes) -> None:
    Path(path).write_bytes(data)

