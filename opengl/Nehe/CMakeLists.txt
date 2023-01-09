cmake_minimum_required(VERSION 3.24)
project(Nehe)

set(CMAKE_CXX_STANDARD 14)

include_directories(.)
include_directories(gl)

add_custom_target(glutdlllib COMMAND ${CMAKE_COMMAND} -E copy ${PROJECT_SOURCE_DIR}/gl/glut32.dll ${CMAKE_BINARY_DIR})

set(OpenGlLibs glu32 opengl32)

add_executable(Nehe
        framework.h
        Nehe.cpp
        Nehe.h
        Resource.h
        targetver.h)

target_link_libraries(Nehe ${OpenGlLibs})

add_dependencies(Nehe glutdlllib)