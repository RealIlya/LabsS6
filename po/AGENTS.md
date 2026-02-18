# Repository Guidelines

## Project Structure & Module Organization
- `lab1/` contains the Lab 1 assignment materials.
- `lab1/Проект 1. Конвертор р1_р2.docx` is the canonical specification and should be treated as the source of truth.
- `lab1/_extracted_lab1/document.md` and `lab1/_extracted_lab1/media/*` are generated artifacts used for searchable text and image review.
- If implementation code is added, place it in `lab1/src/` and tests in `lab1/tests/` to keep code separate from source documents.

## Build, Test, and Development Commands
- `pandoc "lab1/Проект 1. Конвертор р1_р2.docx" --extract-media="lab1/_extracted_lab1" -t markdown -o "lab1/_extracted_lab1/document.md"`  
  Regenerates Markdown and extracted images from the `.docx`.
- `rg --files lab1`  
  Lists project files quickly.
- `rg -n "keyword" lab1/_extracted_lab1/document.md`  
  Finds requirements by section or term.
- `dotnet build` / `dotnet test`  
  Use these once SDK-based C# projects are added.

## Coding Style & Naming Conventions
- Working language for collaboration is Russian.
- Write all reports and deliverables in Russian by default (allow English only for code identifiers, commands, and framework terms).
- Use UTF-8 encoding and preserve domain terms from the assignment (including Russian identifiers where required by the spec).
- Keep Markdown concise with clear headings and short, task-focused paragraphs.
- For C# code: 4-space indentation, `PascalCase` for types/methods, `camelCase` for local variables.
- Keep required specification names when applicable (for example `Conver_10_P`, `Conver_P_10`, `Control_`).
- Prefer one class per file and match filename to class name.

## Testing Guidelines
- Add tests for each public method in the assignment model.
- Cover happy paths and boundaries: bases `2..16`, sign handling, fractional precision, and invalid input.
- Use test names like `MethodName_Scenario_ExpectedResult`.
- Keep tests deterministic and independent.

## Reporting Rules
- Prepare all reports in Russian.
- For this lab, include only the required report parts: assignment text, program code, and test datasets/results.
- Do not include answers to control questions in the report unless the instructor explicitly requests them.
- Default page margins for reports: left `3 cm`, top `2 cm`, bottom `2 cm`, right `1.5 cm`.
- Default first-line paragraph indent: `1 cm`.
- Default alignment for report body text: justified (full width).
- Table captions: place at the top-right, format `Таблица N`, with continuous numbering through the full report; for table splits use top-right `Продолжение таблицы N`.
- Figure captions: place centered below the image, format `Рис. N`; use references in text as `на рис. N`.

## Commit & Pull Request Guidelines
- No Git history is available in this workspace yet, so no existing commit style can be inferred.
- Use Conventional Commits (`feat:`, `fix:`, `docs:`, `test:`) with focused scope.
- PRs should include: summary of changes, rationale, links to requirement sections in `lab1/_extracted_lab1/document.md`, and screenshots for UI-related updates.
