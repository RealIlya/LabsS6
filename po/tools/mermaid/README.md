# Mermaid Renderer

Канонический формат диаграмм в репозитории: `*.mmd`.

PNG для отчётов нужно получать через единый repo-рендер, а не вручную и не через одноразовые скрипты.

Порядок запуска:
- сначала `npx + mermaid-cli + Edge/Chrome`;
- если локальный браузерный путь не сработал, скрипт падает обратно на контейнерный рендер через Docker.

Предварительные условия:
- либо доступен `npx` и установлен Edge/Chrome;
- либо запущен Docker Desktop с Linux engine.

## Команда

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "tools/mermaid/render-mermaid.ps1" `
  "lab2/report/assets/class-diagram.mmd" `
  "lab2/report/assets/class-diagram.png"
```

## Правила

- исходник диаграммы хранить рядом с отчётом в `labN/report/assets/*.mmd`;
- PNG считать производным артефактом от `*.mmd`;
- в отчёт вставлять именно PNG, а не raw-блок `mermaid`;
- для повторной генерации использовать только `tools/mermaid/render-mermaid.ps1`;
- fallback-версия контейнера закреплена в `tools/mermaid/render-mermaid.ps1`.
