#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include <QQuickStyle>

#include "back/Factorizer.hpp"

int main(int argc, char* argv[]) {
  // Другие варианты: "Material", "Universal", "Imagine"
  QQuickStyle::setStyle("Material");
  // Создаем экземпляр приложения
  QGuiApplication app(argc, argv);

  // Регистрируем наш C++ тип для использования в QML
  qmlRegisterType<Factorizer>("MathBackend", 1, 0, "Factorizer");

  QQmlApplicationEngine engine;
  //  engine.addImportPath("/usr/lib/x86_64-linux-gnu/qt6/qml");
  engine.addImportPath("qrc:/front/");
  // Указываем движку загрузить наш QML файл.
  // Путь "qrc:/" означает, что файл находится внутри ресурсов,
  // скомпилированных в программу.
  const QUrl url(u"qrc:/front/main.qml"_qs);

  QObject::connect(
      &engine, &QQmlApplicationEngine::objectCreated, &app,
      [url](QObject* obj, const QUrl& objUrl) {
        if (!obj && url == objUrl) QCoreApplication::exit(-1);
      },
      Qt::QueuedConnection);

  // Загружаем QML
  engine.load(url);
  return app.exec();
}