# Reporting + Parser Containers

Два отдельных образа:

1. `labs-reporting` - сборка итогового отчёта.
2. `labs-parser` - OCR/парсинг PDF/DOCX (вынесен отдельно, чтобы не утяжелять отчётный образ).

## 1) Reporting (итоговый отчёт)

### Что внутри

- `pandoc` (md -> docx)
- `xelatex` (`md -> pdf`) - базовый рендер основного отчёта
- `wkhtmltopdf` (title html+css -> title.pdf)
- `chromium` (альтернативный рендер титульника для совместимости с HeadlessChrome/Edge)
- `libreoffice-writer` (docx -> pdf)
- шрифты: `fonts-liberation`, `fonts-crosextra-carlito`, `fonts-crosextra-caladea`, `fonts-dejavu-core`
- Python: `pypdf`, `reportlab` (склейка + нумерация)

### Сборка образа

```bash
docker build -t labs-reporting:latest -f tools/reporting-container/Dockerfile tools/reporting-container
```

### Как запустить (базово)

Пример для `po/lab1`:

```bash
docker run --rm \
  -v "$PWD:/work" \
  labs-reporting:latest \
  --subject-dir po \
  --lab-dir po/lab1 \
  --report-md po/lab1/report/lab1-report.md \
  --discipline "Технологии разработки программного обеспечения" \
  --lab-number 1 \
  --lab-title "Конвертер p1_p2"
```

Результат: только `po/lab1/report/lab1-report.pdf`.

### Рендер титульника через Chromium

Для титульника можно переключить движок рендера:

- по умолчанию: `--title-renderer wkhtmltopdf`
- Chromium: `--title-renderer chromium`
- если бинарник не стандартный: `--chromium-bin /path/to/chromium`

Пример с Chromium:

```bash
docker run --rm \
  -v "$PWD:/work" \
  labs-reporting:latest \
  --subject-dir po \
  --lab-dir po/lab1 \
  --report-docx po/lab1/report/lab1-report.docx \
  --discipline "Технологии разработки программного обеспечения" \
  --lab-number 1 \
  --lab-title "Конвертер p1_p2" \
  --title-renderer chromium
```

### Как получить вёрстку “как на Windows”

Для максимально точного совпадения использовать уже готовый основной PDF, который сформирован в Windows (например Word):

```bash
docker run --rm \
  -v "$PWD:/work" \
  labs-reporting:latest \
  --subject-dir po \
  --lab-dir po/lab1 \
  --main-report-pdf po/lab1/report/main-win.pdf \
  --discipline "Технологии разработки программного обеспечения" \
  --lab-number 1 \
  --lab-title "Конвертер p1_p2"
```

Почему: Linux LibreOffice не даёт 100% идентичный рендер Word на Windows.

### Источники основного отчёта (приоритет)

1. `--main-report-pdf` (лучший путь для полного совпадения с Windows)
2. `--report-md` + `--main-renderer xelatex` (базовый режим)
3. `--report-docx` + `--main-renderer libreoffice` (альтернативный режим)
4. `--report-md` + `--main-renderer libreoffice` (md -> docx -> pdf)

Нужно указать хотя бы один из этих флагов.

### Рендер основного отчёта (базовый и альтернативный)

- По умолчанию: `--main-renderer xelatex` (базовый режим).
- Альтернатива: `--main-renderer libreoffice`.

Пример с `libreoffice`:

```bash
docker run --rm \
  -v "$PWD:/work" \
  labs-reporting:latest \
  --subject-dir po \
  --lab-dir po/lab1 \
  --report-docx po/lab1/report/lab1-report.docx \
  --discipline "Технологии разработки программного обеспечения" \
  --lab-number 1 \
  --lab-title "Конвертер p1_p2" \
  --main-renderer libreoffice
```

### Промежуточные файлы

- По умолчанию удаляются.
- С `--debug` сохраняются:
  - `__labN-title.pdf`
  - `__labN-report.docx` (если генерировался из md)
  - `__labN-report-main.pdf` (если собирался из docx)
  - `__title.filled.html`

### Платформенные команды запуска

PowerShell:

```powershell
docker run --rm `
  -v "${PWD}:/work" `
  labs-reporting:latest `
  --subject-dir po `
  --lab-dir po/lab1 `
  --report-md po/lab1/report/lab1-report.md `
  --discipline "Технологии разработки программного обеспечения" `
  --lab-number 1 `
  --lab-title "Конвертер p1_p2" `
  --title-renderer chromium
```

cmd.exe:

```bat
docker run --rm ^
  -v "%cd%:/work" ^
  labs-reporting:latest ^
  --subject-dir po ^
  --lab-dir po/lab1 ^
  --report-md po/lab1/report/lab1-report.md ^
  --discipline "Технологии разработки программного обеспечения" ^
  --lab-number 1 ^
  --lab-title "Конвертер p1_p2" ^
  --title-renderer chromium
```

## 2) Parser (OCR/извлечение)

### Что внутри

- `poppler-utils` (`pdftotext`, `pdftoppm`)
- `tesseract-ocr` + `rus/eng`

### Сборка образа

```bash
docker build -t labs-parser:latest -f tools/reporting-container/Dockerfile.parser tools/reporting-container
```

### Запуск интерактивно

```bash
docker run --rm -it -v "$PWD:/work" labs-parser:latest
```

После старта можно выполнять:

```bash
pdftotext input.pdf output.txt
pdftoppm -png input.pdf /work/out/page
tesseract /work/out/page-1.png stdout -l rus+eng
```
