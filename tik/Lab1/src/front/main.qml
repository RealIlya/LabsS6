// Импортируем необходимые модули QML
import QtQuick
import QtQuick.Controls
import QtQuick.Dialogs
import QtQuick.Window

import MyComponents 1.0

// Импортируем наш C++ класс
import MathBackend 1.0

// Создаем главный объект - окно
ApplicationWindow {
    id: root
    width: 800
    height: 300
    maximumHeight: 800
    //minimumWidth: rootLayout.implicitWidth// + 30
    //minimumHeight: rootLayout.implicitHeight
    visible: true
    Material.theme: Material.Light
    title: "Факторизация числа - Метод (p-1) Полларда"
    // Создаем экземпляр нашего C++ объекта.
    Factorizer {
        id: factorizer
    }

    // Диалоги для выбора файлов
    FileDialog {
        id: openDialog
        title: "Выберите файл с числом для факторизации"
        onAccepted: {
            if (selectedFile) {
                // Преобразуем строку URL в локальный путь
                var pathString = selectedFile.toString();
                if (pathString.startsWith("file://")) {
                    pathString = pathString.substring(7); // Удаляем "file://"
                }
                openDialog.sourceFileInput.filePath = pathString;
                if (fileContentTA) {
                    fileContentTA.text = factorizer.readFileContent(pathString);
                    fileContentTA = null;
                }
            }
        }
        // Пользовательское свойство, чтобы диалог "знал", какое поле обновлять
        property FileInput sourceFileInput
        property StyledTextArea fileContentTA: null
    }

    Item {
        anchors.fill: parent
        FileInput {
            id: generatorInputPath
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.right: parent.horizontalCenter
            anchors.margins: 15
            labelText: "Входной файл:"
            placeholderText: "Файл с числом..."
            onButtonClicked: {
                openDialog.sourceFileInput = generatorInputPath;
                openDialog.fileContentTA = generatorInputText;
                openDialog.open();
            }
        }
        Button {
            id: generateButton
            anchors.top: parent.top
            anchors.left: parent.horizontalCenter
            anchors.right: parent.right
            anchors.margins: 15
            text: "Факторизовать число"
            onClicked: {
                generatorOutputText.text = factorizer.factorize(generatorInputText.text);
            }
        }
        StyledTextArea {
            id: generatorInputText
            anchors.top: generatorInputPath.bottom
            anchors.left: parent.left
            anchors.right: parent.horizontalCenter
            anchors.bottom: parent.bottom
            anchors.margins: 15
            readOnly: false
            placeholderText: "Число для факторизации..."
        }
        StyledTextArea {
            id: generatorOutputText
            anchors.top: generatorInputPath.bottom
            anchors.left: parent.horizontalCenter
            anchors.right: parent.right
            anchors.bottom: parent.bottom
            anchors.margins: 15
            readOnly: true
            placeholderText: "Результаты факторизации (множители, время, итерации)..."
        }
    }
}
