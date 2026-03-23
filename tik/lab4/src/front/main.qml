import QtQuick
import QtQuick.Controls
import QtQuick.Layouts
import QtQuick.Dialogs

import MyComponents 1.0
import Crypto 1.0

ApplicationWindow {
    id: root
    width: 900
    height: 650
    minimumWidth: 800
    minimumHeight: 600
    visible: true
    Material.theme: Material.Light
    title: "Лабораторная работа №4: Псевдослучайная генерация (ANSI X9.17)"

    GeneratorANSI {
        id: crypto
    }

    // Диалог для сохранения результатов
    FileDialog {
        id: saveDialog
        title: "Сохранить результат в файл"
        fileMode: FileDialog.SaveFile
        onAccepted: {
            var path = selectedFile.toString().replace("file://", "");
            crypto.saveFileContent(path, activeTextArea.text);
        }
        property StyledTextArea activeTextArea: null
    }

    // Диалог для загрузки входных параметров
    FileDialog {
        id: openParamsDialog
        title: "Выберите файл с параметрами (K1, K2, s0, m)"
        onAccepted: {
            var path = selectedFile.toString().replace("file://", "");
            var content = crypto.readFileContent(path);

            // Разбиваем текст по пробелам или переносам строк
            var params = content.trim().split(/\s+/);

            if (params.length >= 4) {
                k1Field.text = params[0];
                k2Field.text = params[1];
                s0Field.text = params[2];
                mField.value = parseInt(params[3]);
                outputSeqText.text = "Параметры успешно загружены из файла!\nНажмите 'Сгенерировать последовательность'.";
            } else {
                outputSeqText.text = "Ошибка: Файл должен содержать 4 параметра, разделенных пробелами или переносами строк:\nK1 K2 s0 m";
            }
        }
    }

    ColumnLayout {
        anchors.fill: parent
        spacing: 0

        TabBar {
            id: tabBar
            Layout.fillWidth: true
            TabButton {
                text: "Генерация ПСП (ANSI X9.17)"
            }
            TabButton {
                text: "Статистические тесты NIST"
            }
        }

        StackLayout {
            currentIndex: tabBar.currentIndex
            Layout.fillWidth: true
            Layout.fillHeight: true

            // --- Вкладка 1: Генерация ---
            Item {
                ColumnLayout {
                    anchors.fill: parent
                    anchors.margins: 15

                    Button {
                        Layout.alignment: Qt.AlignHCenter
                        text: "Загрузить параметры из файла..."
                        onClicked: openParamsDialog.open()
                    }

                    GridLayout {
                        columns: 2
                        Layout.fillWidth: true

                        Label {
                            text: "Ключ K1 (16 hex-символов):"
                        }
                        TextField {
                            id: k1Field
                            Layout.fillWidth: true
                            text: "0123456789ABCDEF"
                            maximumLength: 16
                        }

                        Label {
                            text: "Ключ K2 (16 hex-символов):"
                        }
                        TextField {
                            id: k2Field
                            Layout.fillWidth: true
                            text: "FEDCBA9876543210"
                            maximumLength: 16
                        }

                        Label {
                            text: "Начальное значение s0 (16 hex-символов):"
                        }
                        TextField {
                            id: s0Field
                            Layout.fillWidth: true
                            text: "1A2B3C4D5E6F7A8B"
                            maximumLength: 16
                        }

                        Label {
                            text: "Количество 64-битных блоков m:"
                        }
                        SpinBox {
                            id: mField
                            from: 1
                            to: 100000
                            value: 200 // 200 блоков = 12800 бит
                            editable: true
                        }
                    }

                    Button {
                        Layout.alignment: Qt.AlignHCenter
                        text: "Сгенерировать последовательность"
                        onClicked: {
                            outputSeqText.text = crypto.generateANSI(k1Field.text, k2Field.text, s0Field.text, mField.value);
                        }
                    }

                    StyledTextArea {
                        id: outputSeqText
                        Layout.fillWidth: true
                        Layout.fillHeight: true
                        readOnly: true
                        placeholderText: "Сгенерированная битовая последовательность..."
                    }

                    Button {
                        text: "Сохранить последовательность..."
                        onClicked: {
                            saveDialog.activeTextArea = outputSeqText;
                            saveDialog.open();
                        }
                    }
                }
            }

            // --- Вкладка 2: Тестирование ---
            Item {
                ColumnLayout {
                    anchors.fill: parent
                    anchors.margins: 15

                    Label {
                        text: "Тестируемая последовательность берется из результатов вкладки 'Генерация'."
                        font.bold: true
                    }

                    Button {
                        Layout.alignment: Qt.AlignHCenter
                        text: "Запустить тесты"
                        onClicked: {
                            if (outputSeqText.text === "" || outputSeqText.text.includes("Параметры успешно")) {
                                testLogsText.text = "Сначала сгенерируйте последовательность во вкладке 'Генерация'.";
                            } else {
                                testLogsText.text = crypto.runTests(outputSeqText.text);
                            }
                        }
                    }

                    StyledTextArea {
                        id: testLogsText
                        Layout.fillWidth: true
                        Layout.fillHeight: true
                        readOnly: true
                        placeholderText: "Здесь будут отображены подробные результаты всех тестов..."
                        // Моноширинный шрифт для ровного отображения таблиц
                        font.family: "Monospace"
                    }

                    Button {
                        text: "Сохранить лог тестов..."
                        onClicked: {
                            saveDialog.activeTextArea = testLogsText;
                            saveDialog.open();
                        }
                    }
                }
            }
        }
    }
}
