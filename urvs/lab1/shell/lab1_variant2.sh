#!/bin/sh
set -eu

usage() {
    echo "Usage: $0 <directory>" >&2
}

if [ "$#" -ne 1 ]; then
    usage
    exit 1
fi

if [ ! -d "$1" ]; then
    echo "error: '$1' is not a directory" >&2
    exit 2
fi

current=$(cd "$1" && pwd -P)
first=1

while :; do
    if [ "$first" -eq 1 ]; then
        echo "каталог $current начальный каталог"
    else
        echo "каталог $current родительский каталог"
    fi

    # Печатаем только подкаталоги текущего уровня (без "." и ".."), отсортированные.
    find "$current" -mindepth 1 -maxdepth 1 -type d -printf '%f\n' | LC_ALL=C sort \
    | while IFS= read -r name; do
        [ -n "$name" ] || continue
        echo "  каталог $name"
    done

    parent=$(dirname "$current")
    if [ "$parent" = "$current" ]; then
        break
    fi

    current=$parent
    first=0
done
