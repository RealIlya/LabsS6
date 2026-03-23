#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include <QQuickStyle>

#include "back/GeneratorANSI.hpp"

int main(int argc, char* argv[]) {
  QQuickStyle::setStyle("Material");
  QGuiApplication app(argc, argv);

  qmlRegisterType<GeneratorANSI>("Crypto", 1, 0, "GeneratorANSI");

  QQmlApplicationEngine engine;
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