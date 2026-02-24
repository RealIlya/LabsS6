FROM debian:bookworm-slim

ENV DEBIAN_FRONTEND=noninteractive
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        pandoc \
        wkhtmltopdf \
        chromium \
        libreoffice-writer \
        texlive-xetex \
        texlive-lang-cyrillic \
        texlive-fonts-recommended \
        lmodern \
        fontconfig \
        python3 \
        python3-pip \
        fonts-dejavu-core \
        fonts-liberation \
        fonts-crosextra-carlito \
        fonts-crosextra-caladea \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /opt/reporting

COPY requirements.txt /opt/reporting/requirements.txt
RUN pip3 install --break-system-packages --no-cache-dir -r /opt/reporting/requirements.txt

COPY scripts /opt/reporting/scripts

WORKDIR /work

ENTRYPOINT ["python3", "/opt/reporting/scripts/build_full_report.py"]
