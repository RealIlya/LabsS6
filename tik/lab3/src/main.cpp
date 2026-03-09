#include <QApplication>
#include <QQmlApplicationEngine>
#include <QQuickStyle>

#include "back/HashBackend.hpp"

int main(int argc, char* argv[]) {
  // Другие варианты: "Material", "Universal", "Imagine"
  QQuickStyle::setStyle("Material");
  // Создаем экземпляр приложения
  QApplication app(argc, argv);

  // Регистрируем наш C++ тип для использования в QML
  qmlRegisterType<HashBackend>("Crypto", 1, 0, "HashBackend");

  QQmlApplicationEngine engine;
  //  engine.addImportPath("/usr/lib/x86_64-linux-gnu/qt6/qml");
  engine.addImportPath("qrc:/front/");
  const QUrl url(u"qrc:/front/main.qml"_qs);

  QObject::connect(
      &engine, &QQmlApplicationEngine::objectCreated, &app,
      [url](QObject* obj, const QUrl& objUrl) {
        if (!obj && url == objUrl) QCoreApplication::exit(-1);
      },
      Qt::QueuedConnection);

  engine.load(url);
  return app.exec();
}