# Repository Guidelines

## Project Structure & Module Organization
This repository is organized by lab: `lab1` through `lab6`. Each lab folder currently stores the assignment PDF (for example, `lab3/Семестр 2_ Лабораторная работа 3.pdf`).

Keep contributions scoped to one lab at a time. Use this layout inside each `labN` folder:
- `src/` for implementation code.
- `tests/` for automated checks.
- `report.tex` as the main XeLaTeX report source.
- `assets/` for images, tables, and generated figures.

## Build, Test, and Development Commands
There is no single global build script yet; run commands per lab.
- `rg --files` lists all tracked files quickly.
- `git status` shows local changes before commit.
- `xelatex -interaction=nonstopmode -output-directory labN/build labN/report.tex` builds a PDF report.
- `pandoc labN/report.md -o labN/report.docx --reference-doc=reference.docx` generates DOCX when needed.
- `pytest labN/tests -q` runs Python tests (if the lab uses Python).

## Coding Style & Naming Conventions
Use readable, consistent naming:
- Lab folders: `lab1`, `lab2`, ..., `lab6`.
- Scripts: `task_01.py`, `task_02.py`.
- Assets: `figure_01.png`, `table_01.csv`.

Formatting defaults:
- Python: 4 spaces, no tabs, PEP 8.
- JS/TS (if added): 2 spaces, semicolon-consistent style.
- Keep files small and single-purpose; document non-obvious logic briefly.

## Testing Guidelines
Put tests next to each lab in `labN/tests/`. Prefer deterministic tests and avoid hidden external dependencies.
- Python test files: `test_*.py`.
- Add at least one smoke test for every executable script.
- Run the relevant test command before opening a PR.

## Commit & Pull Request Guidelines
Current history uses short informal commits (`init`, `po: lab1: almost done`). Standardize moving forward:
- Commit format: `labN: imperative summary` (example: `lab4: add matrix inversion tests`).
- One logical change per commit.
- PRs must include: purpose, changed paths, verification steps, and screenshots for report layout changes.

## Report Formatting Rules
Default report source is XeLaTeX (`*.tex`), with DOCX as an allowed fallback when required.
- Margins: left `3 cm`, top `2 cm`, bottom `2 cm`, right `1.5 cm`.
- First-line indent: `1 cm`; body text justified.
- Table caption: top-right `Таблица N`; continued table: `Продолжение таблицы N`.
- Figure caption: bottom-center `Рис. N`; in-text reference: `на рис. N`.
