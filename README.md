# NKakebo - Aplicación Multiplataforma de Gestión Financiera Japonesa

Una aplicación de escritorio **multiplataforma** que implementa el método japonés Kakebo para la gestión consciente de las finanzas personales, desarrollada con C# y Avalonia UI.

## 🌍 Soporte Multiplataforma

### Plataformas Soportadas
- ✅ **Windows x64** (Intel/AMD de 64 bits)
- ✅ **Windows ARM64** (Surface Pro X, PCs con Qualcomm)
- ✅ **Linux x64** (Ubuntu, Fedora, Debian, openSUSE, etc.)
- ✅ **Linux ARM64** (Raspberry Pi 4+, servidores ARM)
- ✅ **macOS Intel** (Mac Intel de 64 bits)
- ✅ **macOS Apple Silicon** (Mac M1/M2/M3)

### Tecnología Multiplataforma
- **UI Framework**: Avalonia UI 11.3.2 (nativa multiplataforma)
- **Runtime**: .NET 9.0 (sin dependencias de Windows)
- **Base de Datos**: LiteDB (embebida, funciona en cualquier OS)
- **Gráficos**: Skia (renderizado nativo multiplataforma)

## 🚀 Instalación y Ejecución

### Prerrequisitos
**Ninguno** - Los binarios incluyen todo lo necesario (self-contained)

### Descargar y Ejecutar

#### Windows# Descargar versión Windows
# Ejecutar:
KakeboApp.exe
#### Linux# Descargar versión Linux
# Dar permisos de ejecución:
chmod +x KakeboApp

# Ejecutar:
./KakeboApp
#### macOS# Descargar versión macOS
# Dar permisos de ejecución:
chmod +x KakeboApp

# Ejecutar:
./KakeboApp
## 🔧 Compilación desde Código Fuente

### Para Desarrolladores

#### Prerrequisitos de Desarrollo
- .NET 9.0 SDK
- Visual Studio 2022 / JetBrains Rider / VS Code

#### Compilación Simple# Clonar repositorio
git clone https://github.com/tu-usuario/nkakebo.git
cd nkakebo

# Restaurar dependencias
dotnet restore

# Compilar y ejecutar
dotnet run --project KakeboApp
#### Compilación Multiplataforma# En Windows: usar script de PowerShell
.\build-cross-platform.bat

# En Linux/macOS: usar script de Bash
chmod +x build-cross-platform.sh
./build-cross-platform.sh
#### Comandos de Compilación Manual# Windows x64
dotnet publish KakeboApp -c Release -r win-x64 --self-contained true -o releases/Windows-x64

# Linux x64
dotnet publish KakeboApp -c Release -r linux-x64 --self-contained true -o releases/Linux-x64

# macOS x64
dotnet publish KakeboApp -c Release -r osx-x64 --self-contained true -o releases/macOS-x64

# macOS Apple Silicon
dotnet publish KakeboApp -c Release -r osx-arm64 --self-contained true -o releases/macOS-ARM64
## 🏗️ Arquitectura Multiplataforma

### Separación por CapasKakeboApp/
├── KakeboApp.Core/              # Lógica de negocio (multiplataforma)
│   ├── Models/                  # DTOs y entidades
│   ├── Services/                # Servicios de negocio
│   ├── Data/                    # Acceso a datos LiteDB
│   └── Interfaces/              # Contratos
├── KakeboApp/                   # Presentación (multiplataforma)
│   ├── ViewModels/              # MVVM ViewModels
│   ├── Views/                   # XAML Views (Avalonia)
│   ├── Services/                # Servicios específicos de plataforma
│   └── Converters/              # Value converters
└── Scripts/                     # Scripts de compilación
### Servicios Específicos de Plataforma
- **File Dialogs**: Avalonia Storage Provider (funciona en todos los OS)
- **Rutas de Datos**: 
  - Windows: `%APPDATA%\KakeboApp`
  - Linux: `~/.local/share/KakeboApp`
  - macOS: `~/Library/Application Support/KakeboApp`

## 🌟 Características

### Metodología Kakebo Original
- **4 Categorías Kakebo**: Supervivencia, Opcional, Cultura e Inesperado
- **Clasificación automática** de gastos según la metodología japonesa
- **Presupuesto mensual** con seguimiento y alertas
- **Análisis de patrones** de gasto y ahorro

### Funcionalidades Avanzadas
- ✅ **CRUD completo** de transacciones con validación
- 📊 **Reportes detallados** por categoría y subcategoría
- 🔍 **Filtros avanzados** por fecha, tipo y categoría
- 💡 **Sugerencias inteligentes** de subcategorías
- 🔒 **Base de datos encriptada** con LiteDB
- 🖥️ **Interfaz nativa** en cada plataforma

### UI Responsiva
- **Desktop**: Sidebar navegable, DataGrids, formularios completos
- **Temas**: Fluent Design (se adapta al tema del sistema)
- **Fuentes**: Inter font incluida para consistencia visual

## 📦 Distribución

### Binarios Autocontenidos
Cada versión incluye:
- Runtime .NET 9.0 embebido
- Todas las librerías necesarias
- No requiere instalación de .NET en el sistema
- Tamaño aproximado: ~80MB por plataforma

### Estructura de Archivosreleases/
├── Windows-x64/
│   ├── KakeboApp.exe        # Ejecutable principal
│   ├── *.dll                # Librerías .NET y Avalonia
│   └── runtimes/            # Librerías nativas
├── Linux-x64/
│   ├── KakeboApp            # Ejecutable principal
│   ├── *.dll                # Librerías .NET y Avalonia
│   └── runtimes/            # Librerías nativas Linux
└── macOS-x64/
    ├── KakeboApp            # Ejecutable principal
    ├── *.dll                # Librerías .NET y Avalonia
    └── runtimes/            # Librerías nativas macOS
## 🧪 Testing Multiplataforma
# Ejecutar tests en la plataforma actual
dotnet test

# Tests específicos de plataforma
dotnet test --logger "console;verbosity=detailed"
## 🎯 Ventajas de la Arquitectura Multiplataforma

### Para Usuarios
- **Una sola aplicación**: Funciona igual en Windows, Linux y macOS
- **Sin instaladores**: Binarios autocontenidos
- **Rendimiento nativo**: No es web app, es nativa de escritorio
- **Consistencia**: UI idéntica en todas las plataformas

### Para Desarrolladores
- **Un solo código fuente**: Mantenimiento simplificado
- **C# y .NET**: Tecnologías maduras y bien documentadas
- **Avalonia UI**: XAML familiar para desarrolladores WPF
- **Testing simplificado**: Misma lógica en todas las plataformas

## 🔒 Seguridad y Privacidad

- **Base de datos local**: Todos los datos permanecen en tu máquina
- **Encriptación opcional**: Protege tu BD con contraseña
- **Sin telemetría**: La aplicación no envía datos a internet
- **Código abierto**: Auditable y transparente

## 📄 Licencia

MIT License - Uso libre para proyectos comerciales y personales.

---

## 🎉 Estado del Proyecto: LISTO PARA PRODUCCIÓN

### ✅ Compilación Exitosa
- ✅ Windows, Linux, macOS
- ✅ Arquitecturas x64 y ARM64
- ✅ Binarios autocontenidos
- ✅ Sin errores de compilación

### ✅ Funcionalidades Completas
- ✅ Gestión completa de transacciones
- ✅ Presupuestos Kakebo
- ✅ Reportes y análisis
- ✅ Base de datos encriptada

**Kakebo** - _Gestión financiera japonesa, ahora multiplataforma_ 🌍💰
