# Минимальная требуемая версия CMake
cmake_minimum_required(VERSION 3.16)

# Название проекта
project(IaCT3 VERSION 1.0 LANGUAGES CXX)

set(TARGET iact3)

# Устанавливаем стандарт C++
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Включаем "магию" Qt для автоматической обработки MOC, UIC, и RCC файлов
set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

set(QT_QML_GENERATE_QMLLS_INI ON)

# Говорим CMake добавлять пути к библиотекам, с которыми мы линкуемся, в RPATH.
# Это позволит исполняемому файлу находить .so файлы Qt без LD_LIBRARY_PATH.
set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)

# Ищем установленный Qt6
# Мы будем использовать модули Core (основа), Gui (для окна) и Quick (для QML)
find_package(Qt6 COMPONENTS Core Gui Widgets Quick QuickControls2 QuickDialogs2 Charts REQUIRED)

# Создаем исполняемый файл (наше приложение)
qt_add_executable(${TARGET}
    main.cpp
    back/HashBackend.hpp
    back/HashBackend.cpp
    front/resources.qrc
)

set_target_properties(${TARGET} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../install"
)

# Связываем (линкуем) наше приложение с библиотеками Qt, которые мы нашли
target_link_libraries(${TARGET} PRIVATE
    Qt6::Core
    Qt6::Gui
    Qt6::Widgets
    Qt6::Quick
    Qt6::QuickControls2
    Qt6::QuickDialogs2
    Qt6::Charts
)