// Импортируем необходимые модули QML
import QtQuick
import QtQuick.Controls
import QtQuick.Dialogs
import QtQuick.Window
import QtQuick.Layouts
import QtCharts
import MyComponents 1.0

// Импортируем наш C++ класс
import Crypto 1.0

// Создаем главный объект - окно
ApplicationWindow {
    id: root
    width: 800
    height: 500
    maximumHeight: 800
    //minimumWidth: rootLayout.implicitWidth// + 30
    //minimumHeight: rootLayout.implicitHeight
    visible: true
    Material.theme: Material.Light
    title: "Хеш функции - RIPEMD-320"
    // Создаем экземпляр нашего C++ объекта.
    HashBackend {
        id: crypto
    }

    // Диалоги для выбора файлов
    FileDialog {
        id: openDialog
        title: "Выберите файл"
        onAccepted: {
            if (selectedFile) {
                // Преобразуем строку URL в локальный путь
                var pathString = selectedFile.toString();
                if (pathString.startsWith("file://")) {
                    pathString = pathString.substring(7); // Удаляем "file://"
                }
                openDialog.sourceFileInput.filePath = pathString;
                if (fileContentTA) {
                    fileContentTA.text = crypto.readFileContent(pathString);
                    fileContentTA = null;
                }
            }
        }
        // Пользовательское свойство, чтобы диалог "знал", какое поле обновлять
        property FileInput sourceFileInput
        property StyledTextArea fileContentTA: null
    }

    FileDialog {
        id: saveDialog
        title: "Сохранить хэш в файл"
        fileMode: FileDialog.SaveFile
        onAccepted: {
            var path = selectedFile.toString().replace("file://", "");
            crypto.saveFileContent(path, activeTextArea.text);
        }
        property StyledTextArea activeTextArea: null
    }

    Item {
        anchors.fill: parent
        ColumnLayout {
            anchors.fill: parent
            spacing: 0

            TabBar {
                id: tabBar
                Layout.fillWidth: true
                TabButton {
                    text: "Вычисление хеша"
                }
                TabButton {
                    text: "Лавинный эффект"
                }
            }

            SwipeView {
                id: swipe
                currentIndex: tabBar.currentIndex
                Layout.fillHeight: true
                Layout.fillWidth: true
                Item {
                    ColumnLayout {
                        anchors.fill: parent
                        anchors.margins: 15

                        RowLayout {
                            Button {
                                text: "Загрузить текст из файла..."
                                onClicked: {
                                    openDialog.activeTextArea = inputText;
                                    openDialog.open();
                                }
                            }
                        }

                        StyledTextArea {
                            id: inputText
                            Layout.fillWidth: true
                            Layout.fillHeight: true
                            placeholderText: "Введите текст для хеширования (RIPEMD-320)..."
                        }

                        Button {
                            Layout.alignment: Qt.AlignHCenter
                            text: "Вычислить RIPEMD-320"
                            onClicked: {
                                outputText.text = crypto.calculateHash(inputText.text);
                            }
                        }

                        StyledTextArea {
                            id: outputText
                            Layout.fillWidth: true
                            Layout.preferredHeight: 100
                            readOnly: true
                            placeholderText: "Результат (hex)..."
                        }

                        RowLayout {
                            Button {
                                text: "Сохранить результат..."
                                onClicked: {
                                    saveDialog.activeTextArea = outputText;
                                    saveDialog.open();
                                }
                            }
                        }
                    }
                }
                Item {
                    ColumnLayout {
                        anchors.fill: parent
                        anchors.margins: 15

                        RowLayout {
                            Label {
                                text: "Бит для изменения (0-511):"
                            }
                            SpinBox {
                                id: bitIndexInput
                                from: 0
                                to: 511
                                value: 5 // по умолчанию 5-й бит
                            }
                            Button {
                                text: "Построить график лавинного эффекта"
                                onClicked: {
                                    // Запрашиваем данные у C++
                                    var results = crypto.analyzeAvalanche(inputText.text, bitIndexInput.value);

                                    // Очищаем старый график и рисуем новый
                                    avalancheSeries.clear();
                                    for (var i = 0; i < results.length; i++) {
                                        avalancheSeries.append(i, results[i]);
                                    }
                                }
                            }
                        }

                        // Сам график QtCharts
                        ChartView {
                            title: "Зависимость изменившихся бит в регистрах от раунда"
                            Layout.fillWidth: true
                            Layout.fillHeight: true
                            antialiasing: true
                            theme: ChartView.ChartThemeLight

                            ValueAxis {
                                id: axisX
                                min: 0
                                max: 80
                                tickCount: 9
                                labelFormat: "%.0f"
                                titleText: "Номер раунда (0-79)"
                            }

                            ValueAxis {
                                id: axisY
                                min: 0
                                max: 320 // В RIPEMD-320 всего 320 бит состояния
                                titleText: "Число изменившихся бит"
                            }

                            LineSeries {
                                id: avalancheSeries
                                name: "Лавинный эффект"
                                axisX: axisX
                                axisY: axisY
                                color: "red"
                                width: 3
                            }
                        }
                    }
                }
            }
            PageIndicator {
                id: indicator

                count: swipe.count
                currentIndex: swipe.currentIndex
                Layout.alignment: Qt.AlignHCenter
            }
        }
    }
}
