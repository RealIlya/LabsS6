cmake_minimum_required(VERSION 3.20)
project(lab1 LANGUAGES NONE)

set(LAB1_ROOT "${CMAKE_CURRENT_SOURCE_DIR}")
set(PO_ROOT "${LAB1_ROOT}/..")
set(DOTNET_BIN "${PO_ROOT}/.dotnet/dotnet")

if(NOT EXISTS "${DOTNET_BIN}")
  message(FATAL_ERROR "Локальный SDK не найден: ${DOTNET_BIN}")
endif()

set(DOTNET_ENV
  "PATH=${PO_ROOT}/.dotnet:/usr/bin:/bin"
  "DOTNET_CLI_HOME=${PO_ROOT}/.dotnet_cli"
  "NUGET_PACKAGES=${PO_ROOT}/.nuget/packages"
  "DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1"
  "DOTNET_CLI_TELEMETRY_OPTOUT=1"
)

set(SOLUTION_FILE "${LAB1_ROOT}/ConverterLab1.sln")
set(CORE_PROJECT "${LAB1_ROOT}/src/ConverterLab1/ConverterLab1.csproj")
set(TEST_PROJECT "${LAB1_ROOT}/tests/ConverterLab1.Tests/ConverterLab1.Tests.csproj")
set(UI_PROJECT "${LAB1_ROOT}/ui/ConverterLab1.WinForms/ConverterLab1.WinForms.csproj")

add_custom_target(dotnet_restore
  COMMAND ${CMAKE_COMMAND} -E env ${DOTNET_ENV}
          ${DOTNET_BIN} restore ${SOLUTION_FILE}
  WORKING_DIRECTORY ${PO_ROOT}
  USES_TERMINAL
  COMMENT "Restore .NET dependencies"
)

add_custom_target(dotnet_build_core
  COMMAND ${CMAKE_COMMAND} -E env ${DOTNET_ENV}
          ${DOTNET_BIN} build ${CORE_PROJECT} --no-restore -v minimal
  WORKING_DIRECTORY ${PO_ROOT}
  USES_TERMINAL
  COMMENT "Build core library"
)
add_dependencies(dotnet_build_core dotnet_restore)

add_custom_target(dotnet_test
  COMMAND ${CMAKE_COMMAND} -E env ${DOTNET_ENV}
          ${DOTNET_BIN} test ${TEST_PROJECT} --no-restore -v minimal
  WORKING_DIRECTORY ${PO_ROOT}
  USES_TERMINAL
  COMMENT "Run unit tests"
)
add_dependencies(dotnet_test dotnet_build_core)

add_custom_target(dotnet_build_ui
  COMMAND ${CMAKE_COMMAND} -E env ${DOTNET_ENV}
          ${DOTNET_BIN} build ${UI_PROJECT} --no-restore -v minimal
  WORKING_DIRECTORY ${PO_ROOT}
  USES_TERMINAL
  COMMENT "Build WinForms UI"
)
add_dependencies(dotnet_build_ui dotnet_test)

if(WIN32)
  add_custom_target(dotnet_run_ui
    COMMAND ${CMAKE_COMMAND} -E env ${DOTNET_ENV}
            ${DOTNET_BIN} run --project ${UI_PROJECT} --no-build
    WORKING_DIRECTORY ${PO_ROOT}
    USES_TERMINAL
    COMMENT "Run WinForms UI"
  )
  add_dependencies(dotnet_run_ui dotnet_build_ui)
else()
  add_custom_target(dotnet_run_ui
    COMMAND ${CMAKE_COMMAND} -E echo "Запуск WinForms поддерживается только на Windows."
    USES_TERMINAL
    COMMENT "Run WinForms UI"
  )
endif()

add_custom_target(lab1_all)
add_dependencies(lab1_all dotnet_build_ui)
