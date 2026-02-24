#!/usr/bin/env python3
import argparse
import html
import re
import subprocess
import sys
from datetime import datetime
from io import BytesIO
from pathlib import Path

from pypdf import PdfReader, PdfWriter
from reportlab.lib.units import mm
from reportlab.pdfgen import canvas


def run(cmd: list[str], cwd: Path | None = None) -> None:
    print(">", " ".join(cmd))
    subprocess.run(cmd, cwd=str(cwd) if cwd else None, check=True)


def read_first_teacher(tutors_file: Path) -> str:
    if not tutors_file.exists():
        raise FileNotFoundError(
            f"Не найден обязательный файл преподавателей: {tutors_file}"
        )
    for line in tutors_file.read_text(encoding="utf-8").splitlines():
        value = line.strip()
        if value:
            return value
    raise ValueError(f"Файл {tutors_file} пустой.")


def patch_title_html(
    source_html: Path,
    target_html: Path,
    *,
    lab_number: str,
    discipline: str,
    lab_title: str,
    teacher: str,
    group: str,
    brigade: str,
    year: str,
) -> None:
    text = source_html.read_text(encoding="utf-8")

    replacements = {
        "ЛР2 - Титульный лист": f"ЛР{lab_number} - Титульный лист",
        "ЛАБОРАТОРНАЯ РАБОТА № N": f"ЛАБОРАТОРНАЯ РАБОТА № {html.escape(lab_number)}",
        "ПО ДИСЦИПЛИНЕ «Имя дисциплины»": f"ПО ДИСЦИПЛИНЕ «{html.escape(discipline)}»",
        "Название лабораторной работы": html.escape(lab_title),
        "Фамилия И. О.": html.escape(teacher),
        "ПМИ-32": html.escape(group),
        "Бригада №2": f"Бригада №{html.escape(brigade)}",
    }

    for src, dst in replacements.items():
        text = text.replace(src, dst)

    text = re.sub(
        r"Новосибирск,\s*\d{4}",
        f"Новосибирск, {html.escape(year)}",
        text,
        count=1,
    )

    target_html.write_text(text, encoding="utf-8")


def make_overlay(page_w: float, page_h: float, page_number: int) -> BytesIO:
    buf = BytesIO()
    pdf = canvas.Canvas(buf, pagesize=(page_w, page_h))
    pdf.setFont("Helvetica", 11)
    pdf.drawRightString(page_w - 15 * mm, 12 * mm, str(page_number))
    pdf.save()
    buf.seek(0)
    return buf


def merge_and_number(title_pdf: Path, report_pdf: Path, out_pdf: Path) -> None:
    title_reader = PdfReader(str(title_pdf))
    report_reader = PdfReader(str(report_pdf))
    writer = PdfWriter()

    for page in title_reader.pages:
        writer.add_page(page)

    page_no = 1
    for page in report_reader.pages:
        overlay = PdfReader(make_overlay(float(page.mediabox.width), float(page.mediabox.height), page_no))
        page.merge_page(overlay.pages[0])
        writer.add_page(page)
        page_no += 1

    with out_pdf.open("wb") as f:
        writer.write(f)


def detect_chromium_binary(preferred: str) -> str:
    candidates = [preferred, "chromium", "chromium-browser", "google-chrome"]
    for binary in candidates:
        if not binary:
            continue
        try:
            subprocess.run(
                [binary, "--version"],
                stdout=subprocess.DEVNULL,
                stderr=subprocess.DEVNULL,
                check=True,
            )
            return binary
        except Exception:
            continue
    raise FileNotFoundError(
        "Chromium не найден. Установите chromium или передайте --chromium-bin."
    )


def render_title_pdf(
    *,
    renderer: str,
    chromium_bin: str,
    title_html: Path,
    title_pdf: Path,
    cwd: Path,
) -> None:
    if renderer == "wkhtmltopdf":
        run(
            [
                "wkhtmltopdf",
                "--enable-local-file-access",
                title_html.name,
                str(title_pdf),
            ],
            cwd=cwd,
        )
        return

    chrome = detect_chromium_binary(chromium_bin)
    run(
        [
            chrome,
            "--headless",
            "--disable-gpu",
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--allow-file-access-from-files",
            f"--print-to-pdf={title_pdf}",
            title_html.resolve().as_uri(),
        ],
        cwd=cwd,
    )


def normalize_lab_number(raw: str) -> str:
    value = raw.strip()
    if not value:
        raise ValueError("Параметр --lab-number не должен быть пустым.")
    digits = re.sub(r"\D+", "", value)
    return digits if digits else re.sub(r"\s+", "", value)


def convert_docx_to_pdf(docx_path: Path, output_pdf: Path) -> None:
    run(
        [
            "libreoffice",
            "--headless",
            "--nologo",
            "--nodefault",
            "--nofirststartwizard",
            "--convert-to",
            "pdf:writer_pdf_Export",
            "--outdir",
            str(output_pdf.parent),
            str(docx_path),
        ]
    )
    generated = output_pdf.parent / f"{docx_path.stem}.pdf"
    if not generated.exists():
        raise FileNotFoundError(f"LibreOffice не создал PDF: {generated}")
    if generated != output_pdf:
        if output_pdf.exists():
            output_pdf.unlink()
        generated.replace(output_pdf)


def convert_md_to_pdf_xelatex(md_path: Path, output_pdf: Path) -> None:
    run(
        [
            "pandoc",
            str(md_path),
            "-f",
            "gfm",
            "-o",
            str(output_pdf),
            "--pdf-engine=xelatex",
            "-V",
            "geometry:left=3cm,right=1.5cm,top=2cm,bottom=2cm",
            "-V",
            "mainfont=DejaVu Serif",
            "-V",
            "monofont=DejaVu Sans Mono",
            "-V",
            "fontsize=14pt",
        ]
    )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Сборка полного отчета: title.pdf + основной отчет + labN-report.pdf"
    )
    parser.add_argument("--subject-dir", required=True, help="Папка предмета, например: po")
    parser.add_argument("--lab-dir", required=True, help="Папка лабы, например: po/lab1")
    parser.add_argument(
        "--report-md",
        default="",
        help="Путь к markdown-отчету.",
    )
    parser.add_argument(
        "--report-docx",
        default="",
        help="Путь к уже сверстанному docx-отчету (предпочтительно для сохранения верстки)",
    )
    parser.add_argument(
        "--main-report-pdf",
        default="",
        help="Путь к готовому PDF основного отчета (лучший вариант для полного совпадения с Windows)",
    )
    parser.add_argument("--discipline", required=True, help="Название дисциплины")
    parser.add_argument("--lab-number", required=True, help="Номер лабораторной работы")
    parser.add_argument("--lab-title", required=True, help="Название лабораторной работы")
    parser.add_argument(
        "--title-folder",
        default="$title-folder",
        help="Папка с title.html и styles.css",
    )
    parser.add_argument(
        "--output-dir",
        default="",
        help="Куда положить результаты (по умолчанию <lab-dir>/report)",
    )
    parser.add_argument("--group", default="ПМИ-32", help="Обозначение группы")
    parser.add_argument("--brigade", default="2", help="Номер бригады")
    parser.add_argument(
        "--year",
        default=str(datetime.now().year),
        help="Год на титульнике",
    )
    parser.add_argument(
        "--reference-docx",
        default="",
        help="Опционально: reference.docx для pandoc",
    )
    parser.add_argument(
        "--main-renderer",
        default="xelatex",
        choices=["xelatex", "libreoffice"],
        help=(
            "Рендер основного отчета (если не задан --main-report-pdf): "
            "xelatex (базовый) или libreoffice."
        ),
    )
    parser.add_argument(
        "--title-renderer",
        default="wkhtmltopdf",
        choices=["wkhtmltopdf", "chromium"],
        help="Движок рендера титульника: wkhtmltopdf (по умолчанию) или chromium.",
    )
    parser.add_argument(
        "--chromium-bin",
        default="chromium",
        help="Имя/путь Chromium-бинарника для --title-renderer chromium.",
    )
    parser.add_argument(
        "--debug",
        action="store_true",
        help="Сохранить промежуточные файлы сборки в output-dir",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()

    subject_dir = Path(args.subject_dir).resolve()
    lab_dir = Path(args.lab_dir).resolve()
    report_md = Path(args.report_md).resolve() if args.report_md else None
    report_docx = Path(args.report_docx).resolve() if args.report_docx else None
    main_report_pdf_arg = Path(args.main_report_pdf).resolve() if args.main_report_pdf else None
    title_folder = Path(args.title_folder).resolve()
    output_dir = (
        Path(args.output_dir).resolve() if args.output_dir else (lab_dir / "report").resolve()
    )
    lab_number = normalize_lab_number(args.lab_number)

    tutors_file = subject_dir / "tutors-list.txt"
    teacher = read_first_teacher(tutors_file)

    if not title_folder.exists():
        raise FileNotFoundError(f"Не найдена папка титульника: {title_folder}")
    if not (title_folder / "title.html").exists():
        raise FileNotFoundError(f"Не найден шаблон титульника: {title_folder / 'title.html'}")
    if main_report_pdf_arg is None and report_docx is None and report_md is None:
        raise ValueError(
            "Нужно указать один из источников основного отчета: "
            "--main-report-pdf или --report-docx или --report-md."
        )
    if report_md is not None and not report_md.exists():
        raise FileNotFoundError(f"Не найден markdown-отчет: {report_md}")
    if report_docx is not None and not report_docx.exists():
        raise FileNotFoundError(f"Не найден docx-отчет: {report_docx}")
    if main_report_pdf_arg is not None and not main_report_pdf_arg.exists():
        raise FileNotFoundError(f"Не найден PDF основного отчета: {main_report_pdf_arg}")

    output_dir.mkdir(parents=True, exist_ok=True)
    work_title_html = title_folder / "__title.filled.html"
    title_pdf = output_dir / f"__lab{lab_number}-title.pdf"
    docx_out = output_dir / f"__lab{lab_number}-report.docx"
    report_main_pdf = output_dir / f"__lab{lab_number}-report-main.pdf"
    report_pdf = output_dir / f"lab{lab_number}-report.pdf"
    intermediates: list[Path] = [title_pdf]

    if main_report_pdf_arg is not None and main_report_pdf_arg == report_pdf:
        raise ValueError(
            "Путь --main-report-pdf совпадает с итоговым labN-report.pdf. "
            "Передайте отдельный PDF основного отчета (без титульника)."
        )

    patch_title_html(
        title_folder / "title.html",
        work_title_html,
        lab_number=args.lab_number,
        discipline=args.discipline,
        lab_title=args.lab_title,
        teacher=teacher,
        group=args.group,
        brigade=args.brigade,
        year=args.year,
    )

    render_title_pdf(
        renderer=args.title_renderer,
        chromium_bin=args.chromium_bin,
        title_html=work_title_html,
        title_pdf=title_pdf,
        cwd=title_folder,
    )

    if main_report_pdf_arg is not None:
        main_source_pdf = main_report_pdf_arg
    else:
        if args.main_renderer == "xelatex":
            if report_md is None:
                if report_docx is not None:
                    docx_source = report_docx
                    convert_docx_to_pdf(docx_source, report_main_pdf)
                    intermediates.append(report_main_pdf)
                    main_source_pdf = report_main_pdf
                else:
                    raise ValueError(
                        "Для --main-renderer xelatex нужен --report-md "
                        "(или передайте --main-report-pdf)."
                    )
            else:
                convert_md_to_pdf_xelatex(report_md, report_main_pdf)
                main_source_pdf = report_main_pdf
                intermediates.append(report_main_pdf)
        else:
            if report_docx is not None:
                docx_source = report_docx
            else:
                if report_md is None:
                    raise ValueError(
                        "Для --main-renderer libreoffice нужен --report-docx "
                        "или --report-md."
                    )
                cmd_docx = [
                    "pandoc",
                    str(report_md),
                    "-f",
                    "gfm",
                    "-t",
                    "docx",
                    "-o",
                    str(docx_out),
                ]
                if args.reference_docx:
                    cmd_docx.extend(["--reference-doc", args.reference_docx])
                run(cmd_docx)
                docx_source = docx_out
                intermediates.append(docx_out)

            convert_docx_to_pdf(docx_source, report_main_pdf)
            main_source_pdf = report_main_pdf
            intermediates.append(report_main_pdf)

    merge_and_number(title_pdf, main_source_pdf, report_pdf)

    if work_title_html.exists():
        if args.debug:
            (output_dir / "__title.filled.html").write_text(
                work_title_html.read_text(encoding="utf-8"),
                encoding="utf-8",
            )
        work_title_html.unlink()

    if not args.debug:
        for path in intermediates:
            if path.exists():
                path.unlink()

    print("")
    print("Готово:")
    print(f"- {report_pdf}")
    if args.debug:
        print("Промежуточные файлы (--debug):")
        print(f"- {title_pdf}")
        if docx_out.exists():
            print(f"- {docx_out}")
        if report_main_pdf.exists():
            print(f"- {report_main_pdf}")
        print(f"- {output_dir / '__title.filled.html'}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"Ошибка: {exc}", file=sys.stderr)
        raise
