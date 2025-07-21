# Desarrollo Multiplataforma - KakeboApp

## Configuración del Entorno

### IDEs Recomendados
- **Visual Studio 2022** (Windows)
- **JetBrains Rider** (Windows, Linux, macOS)
- **Visual Studio Code** con extensión C# (todas las plataformas)

### Extensiones Útiles para VS Code
- C# for Visual Studio Code
- .NET Install Tool
- Avalonia for VS Code
- NuGet Package Manager

## Debugging por Plataforma

### Windows
```bash
dotnet run --project KakeboApp
```

### Linux
```bash
# Instalar dependencias adicionales si es necesario
sudo apt install libx11-6 libice6 libsm6 libfontconfig1

dotnet run --project KakeboApp
```

### macOS
```bash
# Podría requerir permisos adicionales
xattr -dr com.apple.quarantine ./KakeboApp

dotnet run --project KakeboApp
```

## Variables de Entorno Útiles

```bash
# Habilitar logging detallado de Avalonia
export AVALONIA_LOG_LEVEL=Verbose

# Forzar renderizador específico
export AVALONIA_RENDERER=skia

# Para Linux con problemas de fuentes
export FONTCONFIG_PATH=/etc/fonts
```

## Troubleshooting

### Linux: Problemas con librerías nativas
```bash
# Ubuntu/Debian
sudo apt install libc6-dev libgdiplus libx11-dev

# CentOS/RHEL/Fedora  
sudo yum install glibc-devel libgdiplus libX11-devel
```

### macOS: Problemas de permisos
```bash
sudo spctl --master-disable  # Solo para desarrollo
```

### Todos: Limpiar cache de NuGet
```bash
dotnet nuget locals all --clear
```
