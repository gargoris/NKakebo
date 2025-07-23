#!/bin/bash
# build-cross-platform.sh - Script para compilar Kakebo para múltiples plataformas

set -e

echo "🚀 Compilando Kakebo para múltiples plataformas..."

PROJECT_DIR="KakeboApp"
OUTPUT_DIR="releases"
VERSION="1.0.0"

# Crear directorio de releases
mkdir -p $OUTPUT_DIR

# Limpiar builds anteriores
echo "🧹 Limpiando builds anteriores..."
dotnet clean

# Restaurar dependencias
echo "📦 Restaurando dependencias..."
dotnet restore

# Función para compilar para una plataforma específica
build_platform() {
    local runtime=$1
    local platform_name=$2
    
    echo "🔨 Compilando para $platform_name ($runtime)..."
    
    dotnet publish $PROJECT_DIR \
        -c Release \
        -r $runtime \
        --self-contained true \
        -p:PublishSingleFile=false \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -o $OUTPUT_DIR/$platform_name
    
    echo "✅ $platform_name compilado exitosamente"
}

# Compilar para diferentes plataformas
echo "🎯 Compilando versiones de escritorio..."

# Windows x64
build_platform "win-x64" "Windows-x64"

# Windows ARM64 (Surface, etc.)
build_platform "win-arm64" "Windows-ARM64"

# Linux x64
build_platform "linux-x64" "Linux-x64"

# Linux ARM64 (Raspberry Pi, etc.)
build_platform "linux-arm64" "Linux-ARM64"

# macOS Intel
build_platform "osx-x64" "macOS-Intel"

# macOS Apple Silicon
build_platform "osx-arm64" "macOS-AppleSilicon"

echo ""
echo "🎉 ¡Compilación multiplataforma completada!"
echo "📁 Los binarios están en: $OUTPUT_DIR/"
echo ""
echo "Plataformas disponibles:"
echo "  - Windows x64 (Intel/AMD)"
echo "  - Windows ARM64 (Surface, Qualcomm)"
echo "  - Linux x64 (Ubuntu, Fedora, etc.)"
echo "  - Linux ARM64 (Raspberry Pi, etc.)"
echo "  - macOS Intel"
echo "  - macOS Apple Silicon (M1/M2/M3)"
echo ""
echo "Para ejecutar:"
echo "  Windows: ./KakeboApp.exe"
echo "  Linux/macOS: ./KakeboApp"
