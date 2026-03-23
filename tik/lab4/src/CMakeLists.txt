cmake_minimum_required(VERSION 3.16)

set(TARGET iact4)

project(IaCT4 VERSION 1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTORCC ON)
set(CMAKE_AUTOUIC ON)

set(QT_QML_GENERATE_QMLLS_INI ON)

set(CMAKE_BUILD_WITH_INSTALL_RPATH TRUE)
set(CMAKE_INSTALL_RPATH_USE_LINK_PATH TRUE)

find_package(Qt6 COMPONENTS Core Gui Quick QuickControls2 QuickDialogs2 REQUIRED)

qt_add_executable(${TARGET}
    main.cpp
    back/GeneratorANSI.hpp
    back/GeneratorANSI.cpp
    front/resources.qrc
)

set_target_properties(${TARGET} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_SOURCE_DIR}/../install"
)

target_link_libraries(iact4 PRIVATE
    Qt6::Core
    Qt6::Gui
    Qt6::Quick
    Qt6::QuickControls2
    Qt6::QuickDialogs2
)
