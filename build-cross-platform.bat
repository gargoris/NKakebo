@echo off
REM build-cross-platform.bat - Script de Windows para compilar Kakebo multiplataforma

echo 🚀 Compilando Kakebo para múltiples plataformas...

set PROJECT_DIR=KakeboApp
set OUTPUT_DIR=releases
set VERSION=1.0.0

REM Crear directorio de releases
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%

REM Limpiar builds anteriores
echo 🧹 Limpiando builds anteriores...
dotnet clean

REM Restaurar dependencias
echo 📦 Restaurando dependencias...
dotnet restore

REM Función para compilar (usando call)
echo 🎯 Compilando versiones de escritorio...

REM Windows x64
echo 🔨 Compilando para Windows x64...
dotnet publish %PROJECT_DIR% -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o %OUTPUT_DIR%\Windows-x64
echo ✅ Windows x64 compilado exitosamente

REM Windows ARM64
echo 🔨 Compilando para Windows ARM64...
dotnet publish %PROJECT_DIR% -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o %OUTPUT_DIR%\Windows-ARM64
echo ✅ Windows ARM64 compilado exitosamente

REM Linux x64
echo 🔨 Compilando para Linux x64...
dotnet publish %PROJECT_DIR% -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o %OUTPUT_DIR%\Linux-x64
echo ✅ Linux x64 compilado exitosamente

REM Linux ARM64
echo 🔨 Compilando para Linux ARM64...
dotnet publish %PROJECT_DIR% -c Release -r linux-arm64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o %OUTPUT_DIR%\Linux-ARM64
echo ✅ Linux ARM64 compilado exitosamente

REM macOS Intel
echo 🔨 Compilando para macOS Intel...
dotnet publish %PROJECT_DIR% -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o %OUTPUT_DIR%\macOS-Intel
echo ✅ macOS Intel compilado exitosamente

REM macOS Apple Silicon
echo 🔨 Compilando para macOS Apple Silicon...
dotnet publish %PROJECT_DIR% -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true -o %OUTPUT_DIR%\macOS-AppleSilicon
echo ✅ macOS Apple Silicon compilado exitosamente

echo.
echo 🎉 ¡Compilación multiplataforma completada!
echo 📁 Los binarios están en: %OUTPUT_DIR%\
echo.
echo Plataformas disponibles:
echo   - Windows x64 (Intel/AMD)
echo   - Windows ARM64 (Surface, Qualcomm)
echo   - Linux x64 (Ubuntu, Fedora, etc.)
echo   - Linux ARM64 (Raspberry Pi, etc.)
echo   - macOS Intel
echo   - macOS Apple Silicon (M1/M2/M3)
echo.
echo Para ejecutar:
echo   Windows: KakeboApp.exe
echo   Linux/macOS: ./KakeboApp

pause
