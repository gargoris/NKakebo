---
description: Repository Information Overview
alwaysApply: true
---

# NKakebo Information

## Summary
NKakebo is a cross-platform financial management application implementing the Japanese Kakebo method for conscious personal finance management. Developed with C# and Avalonia UI, it supports Windows, Linux, and macOS on both x64 and ARM64 architectures.

## Structure
- **KakeboApp.Core/**: Business logic layer with models, services, data access, and interfaces
- **KakeboApp/**: Presentation layer with ViewModels, Views (Avalonia UI), and platform-specific services
- **build-cross-platform.bat/sh**: Scripts for building the application for multiple platforms

## Language & Runtime
**Language**: C# (.NET)
**Version**: .NET 9.0
**Build System**: MSBuild (dotnet CLI)
**Package Manager**: NuGet

## Dependencies
**Main Dependencies**:
- Avalonia UI 11.3.2 (cross-platform UI framework)
- ReactiveUI 20.4.1 (MVVM framework)
- LiteDB 5.0.21 (embedded NoSQL database)
- Microsoft.Extensions.Hosting/DependencyInjection 9.0.0 (DI container)
- Serilog 4.3.0 (logging framework)

**Development Dependencies**:
- Fody 6.8.1 (IL weaving)
- ReactiveUI.Fody 19.5.41 (ReactiveUI IL weaving)

## Build & Installation
```bash
# Restore dependencies
dotnet restore

# Build and run
dotnet run --project KakeboApp

# Cross-platform build (Windows)
.\build-cross-platform.bat

# Cross-platform build (Linux/macOS)
chmod +x build-cross-platform.sh
./build-cross-platform.sh

# Manual build for specific platform
dotnet publish KakeboApp -c Release -r win-x64 --self-contained true -o releases/Windows-x64
```

## Testing
**Run Command**:
```bash
dotnet test
```

## Project Components

### KakeboApp.Core
**Purpose**: Business logic layer independent of UI
**Key Features**:
- Models for financial data (transactions, budgets)
- Data access services using LiteDB
- Business logic services for financial operations
- Interfaces defining core functionality contracts

### KakeboApp
**Purpose**: UI and presentation layer
**Key Features**:
- MVVM architecture with ReactiveUI
- Avalonia UI for cross-platform UI rendering
- Platform-specific services for file dialogs and storage
- Responsive UI supporting desktop layouts
- Support for multiple platforms (Windows, Linux, macOS)

## Platform Support
- Windows x64/ARM64
- Linux x64/ARM64
- macOS Intel/Apple Silicon

The application is built as self-contained, including the .NET runtime, making it deployable without requiring .NET installation on the target system.