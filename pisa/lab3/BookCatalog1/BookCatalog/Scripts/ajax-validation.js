// ============================================================================
// ajax-validation.js — Общий модуль AJAX-валидации для проекта BookCatalog
// Лабораторная работа №6: Технология AJAX
// ============================================================================
// Данный скрипт обеспечивает:
//   1. AJAX-проверки вводимых данных на стороне клиента без перезагрузки
//   2. Вызов серверных WebMethod через PageMethods
//   3. Динамическое обновление интерфейса (DOM manipulation)
// ============================================================================

(function () {
    'use strict';

    // ────────────────────────────────────────────────────────────────────────
    // Утилиты
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Debounce — задержка вызова функции для предотвращения частых AJAX-запросов.
     * Используется при вводе текста в поля (email, ISBN).
     */
    function debounce(func, delay) {
        var timer = null;
        return function () {
            var context = this;
            var args = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () {
                func.apply(context, args);
            }, delay);
        };
    }

    /**
     * XMLHttpRequest GET-запрос (для общей демонстрации AJAX).
     * В реальном проекте ASP.NET используется PageMethods.
     */
    function ajaxGet(url, callback) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true); // true = асинхронный
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {      // DONE
                if (xhr.status === 200) {
                    callback(null, xhr.responseText);
                } else {
                    callback(new Error('HTTP ' + xhr.status));
                }
            }
        };
        xhr.send(null);
    }

    /**
     * XMLHttpRequest POST-запрос с JSON-данными.
     */
    function ajaxPost(url, data, callback) {
        var xhr = new XMLHttpRequest();
        xhr.open('POST', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    try {
                        var result = JSON.parse(xhr.responseText);
                        callback(null, result);
                    } catch (e) {
                        callback(null, xhr.responseText);
                    }
                } else {
                    callback(new Error('HTTP ' + xhr.status));
                }
            }
        };
        xhr.send(JSON.stringify(data));
    }

    // ────────────────────────────────────────────────────────────────────────
    // AJAX: Проверка формата email на клиенте
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Проверяет формат email на стороне клиента (без обращения к серверу).
     * Возвращает true если формат корректен.
     */
    window.validateEmailFormat = function (email) {
        var regex = /\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*/;
        return regex.test(email);
    };

    // ────────────────────────────────────────────────────────────────────────
    // AJAX: Проверка сложности пароля
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Оценивает сложность пароля по нескольким критериям.
     * Возвращает объект { score: 0-5, label: 'слабый'|'средний'|'надёжный' }.
     */
    window.evaluatePasswordStrength = function (password) {
        var score = 0;
        if (!password || password.length === 0) {
            return { score: 0, label: '' };
        }
        if (password.length >= 6) score++;
        if (password.length >= 10) score++;
        if (/[A-ZА-ЯЁ]/.test(password)) score++;
        if (/[0-9]/.test(password)) score++;
        if (/[^A-Za-zА-Яа-яЁё0-9]/.test(password)) score++;

        var label;
        if (score <= 2) label = 'слабый';
        else if (score <= 3) label = 'средний';
        else label = 'надёжный';

        return { score: score, label: label };
    };

    // ────────────────────────────────────────────────────────────────────────
    // AJAX: Проверка ISBN через XMLHttpRequest (альтернатива PageMethods)
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Демонстрация AJAX-запроса через XMLHttpRequest.
     * В ASP.NET Web Forms для этого обычно используются PageMethods.
     */
    window.ajaxCheckISBN = function (isbn, callback) {
        if (!isbn || isbn.length < 5) {
            callback({ exists: false, count: 0 });
            return;
        }

        // В реальном проекте можно вызвать Generic Handler (.ashx):
        // ajaxGet('/Handlers/CheckISBN.ashx?isbn=' + encodeURIComponent(isbn), ...)
        // Или использовать PageMethods (предпочтительный способ в ASP.NET AJAX):
        // PageMethods.CheckISBNExists(isbn, successCallback, errorCallback);

        // Для демонстрации — используем PageMethods если доступны
        if (typeof PageMethods !== 'undefined' && PageMethods.CheckISBNExists) {
            PageMethods.CheckISBNExists(isbn,
                function (result) {
                    callback(result);
                },
                function (error) {
                    callback({ exists: false, count: 0, error: true });
                }
            );
        }
    };

    // ────────────────────────────────────────────────────────────────────────
    // AJAX: Динамическое обновление DOM (без перезагрузки страницы)
    // ────────────────────────────────────────────────────────────────────────

    /**
     * Создаёт элемент с индикатором загрузки.
     * Используется для визуальной обратной связи во время AJAX-запросов.
     */
    window.showAjaxLoader = function (containerId, message) {
        var container = document.getElementById(containerId);
        if (!container) return null;

        var loader = document.createElement('div');
        loader.className = 'ajax-loader';
        loader.innerHTML = '[~] ' + (message || 'Загрузка...');
        container.appendChild(loader);

        return loader;
    };

    /**
     * Удаляет индикатор загрузки.
     */
    window.hideAjaxLoader = function (loader) {
        if (loader && loader.parentNode) {
            loader.parentNode.removeChild(loader);
        }
    };

    /**
     * Обновляет текст элемента статуса.
     */
    window.updateStatusElement = function (elementId, text, cssClass) {
        var el = document.getElementById(elementId);
        if (!el) return;

        el.textContent = text;
        el.className = cssClass || '';
        el.style.display = text ? 'block' : 'none';
    };

    // ────────────────────────────────────────────────────────────────────────
    // Инициализация: автодетект полей и привязка событий
    // ────────────────────────────────────────────────────────────────────────

    document.addEventListener('DOMContentLoaded', function () {
        console.log('[AJAX] Модуль ajax-validation.js загружен');
        console.log('[AJAX] Технология: Asynchronous JavaScript and XML');
        console.log('[AJAX] XMLHttpRequest: поддержка ' + (typeof XMLHttpRequest !== 'undefined' ? 'да' : 'нет'));
        console.log('[AJAX] PageMethods: ' + (typeof PageMethods !== 'undefined' ? 'доступны' : 'недоступны'));
    });

})();
