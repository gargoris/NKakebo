# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NKakebo is a cross-platform personal finance management application implementing the Japanese Kakebo methodology. Built with C# and Avalonia UI, it runs natively on Windows, Linux, and macOS.

### Core Technology Stack
- **.NET 9.0** - Cross-platform runtime
- **Avalonia UI 11.3.2** - Cross-platform UI framework (XAML-based)
- **ReactiveUI** - MVVM framework with reactive programming
- **ReactiveUI.Fody** - Automatic property notification generation with `[Reactive]` attributes
- **LiteDB 5.0.21** - Embedded NoSQL database
- **Serilog** - Structured logging
- **Fody** - Code weaving framework for compile-time enhancements

## Development Commands

### Building and Running
```bash
# Restore dependencies
dotnet restore

# Run application (development)
dotnet run --project KakeboApp

# Clean build artifacts
dotnet clean

# Build for current platform
dotnet build

# Run tests (if available)
dotnet test
```

### Cross-Platform Building
```bash
# Use platform-specific scripts for full cross-platform builds
# Windows:
.\build-cross-platform.bat

# Linux/macOS:
chmod +x build-cross-platform.sh
./build-cross-platform.sh
```

### Manual Platform-Specific Builds
```bash
# Windows x64
dotnet publish KakeboApp -c Release -r win-x64 --self-contained true -o releases/Windows-x64

# Linux x64
dotnet publish KakeboApp -c Release -r linux-x64 --self-contained true -o releases/Linux-x64

# macOS Intel
dotnet publish KakeboApp -c Release -r osx-x64 --self-contained true -o releases/macOS-Intel

# macOS Apple Silicon
dotnet publish KakeboApp -c Release -r osx-arm64 --self-contained true -o releases/macOS-AppleSilicon
```

## Code Architecture

### Project Structure
- **KakeboApp.Core/** - Business logic and data access (platform-agnostic)
  - `Models/` - Domain models, DTOs, and Result types
  - `Services/` - Business services (Transaction, Database)
  - `Data/` - Database implementation (LiteDB)
  - `Interfaces/` - Service contracts
  - `Utils/` - Utility classes and helpers

- **KakeboApp/** - Presentation layer (Avalonia UI)
  - `ViewModels/` - MVVM ViewModels with ReactiveUI and Fody attributes
  - `Views/` - XAML Views and code-behind
  - `Services/` - Platform-specific services
  - `Converters/` - Value converters for data binding
  - `Collections/` - Thread-safe collections
  - `Commands/` - Async command implementations
  - `Navigation/` - Navigation service
  - `Utils/` - UI thread helpers and utilities

### Key Design Patterns

#### MVVM with ReactiveUI
- All ViewModels inherit from `ViewModelBase` (ReactiveObject)
- Commands use `ReactiveCommand` for async operations with `RxApp.MainThreadScheduler`
- Properties use `[Reactive]` attributes from ReactiveUI.Fody for automatic change notification
- Thread-safe UI updates via `UIThreadHelper.InvokeOnUIThread`
- Fody weaving enabled for automatic property notification generation

#### Repository Pattern
- `IKakeboDatabase` interface abstracts data access
- `LiteDbKakeboDatabase` implements database operations
- All operations are async and return `Result<T>` for error handling

#### Result Pattern
- Custom `Result<T>` type replaces exceptions for business logic
- Success/Error states with pattern matching support
- Extension methods for functional composition (`Map`, `MapAsync`)

#### Service Layer
- `ITransactionService` - Business logic for transactions
- `IBudgetService` - Budget management and validation
- `IDatabaseService` - Database lifecycle management
- `INavigationService` - View navigation management

### Kakebo Domain Model

#### Core Entities
- **Transaction** - Income/Expense with Kakebo categorization
- **MonthlyBudget** - Budget allocation by Kakebo categories
- **Category** - Detailed expense/income categories
- **KakeboCategory** - Japanese methodology: Survival, Optional, Culture, Unexpected

#### Database Schema
- Uses LiteDB collections: `transactions`, `budgets`
- Indexed by Date, Category, Type for performance
- Validation via Data Annotations

### Cross-Platform Considerations

#### Platform Services
- `IPlatformService` - Platform-specific implementations
- `DesktopPlatformService` - Desktop-specific features
- `WindowsPlatformService` - Windows-specific implementation
- `CrossPlatformService` - Generic implementation

#### UI Responsiveness
- `LayoutManager` - Responsive design with breakpoints
- `ThreadSafeObservableCollection<T>` - UI thread-safe collections
- Platform-specific preprocessing directives (WINDOWS, LINUX, MACOS)

#### Data Storage Paths
- Windows: `%APPDATA%\KakeboApp`
- Linux: `~/.local/share/KakeboApp`
- macOS: `~/Library/Application Support/KakeboApp`

## Important Implementation Notes

### Threading
- All UI operations must be on UI thread (`Dispatcher.UIThread.CheckAccess()`)
- `UIThreadHelper` utility class provides comprehensive UI thread management:
  - `InvokeOnUIThread()` - Async UI thread execution
  - `InvokeOnUIThreadSync()` - Synchronous UI thread execution
  - `InvokeOnUIThreadAsync()` - Async task execution on UI thread
  - `InvokeOnUIThreadWithResult<T>()` - Function execution with return value
  - `IsOnUIThread()` - Thread checking utility
- ViewModelBase provides `ExecuteSafelyAsync` for thread-safe operations
- ReactiveCommand operations use `RxApp.MainThreadScheduler` for UI thread safety
- Database operations run on background threads with UI marshalling

### Error Handling
- Business logic uses `Result<T>` pattern instead of exceptions
- UI layer handles errors via `ViewModelBase.HandleException`
- Logging via Serilog with structured logging

### Validation
- Domain models use Data Annotations for validation
- Custom validation extensions in `BudgetValidationExtensions`
- Server-side validation before database operations

### Performance
- Compiled bindings enabled by default (`AvaloniaUseCompiledBindingsByDefault`)
- Lazy loading patterns for large data sets
- Background database operations with UI updates

## Development Guidelines

### Adding New Features
1. Start with domain model in `KakeboApp.Core/Models`
2. Define service interface in `KakeboApp.Core/Interfaces`
3. Implement service in `KakeboApp.Core/Services`
4. Create ViewModel in `KakeboApp/ViewModels` using `[Reactive]` attributes
5. Create View in `KakeboApp/Views`
6. Register services in `Program.cs`
7. Ensure commands use `RxApp.MainThreadScheduler` for UI thread safety

### Testing
- No specific test framework currently configured
- ViewModels are testable in isolation
- Services use dependency injection for mocking
- Consider adding xUnit and Moq for testing

### Debugging Platform-Specific Issues
Use environment variables for detailed logging:
```bash
export AVALONIA_LOG_LEVEL=Verbose
export AVALONIA_RENDERER=skia
```

## Dependencies Management
- Package references in `.csproj` files
- Cross-platform compatible packages only
- Self-contained deployments include all dependencies
- Runtime identifiers: `win-x64`, `linux-x64`, `osx-x64`, `win-arm64`, `linux-arm64`, `osx-arm64`
- Fody weaving configured via `FodyWeavers.xml` for ReactiveUI property generation

## Recent Architectural Improvements (July 2025)

### Reactive UI Enhancements
- **Fody Integration**: Added ReactiveUI.Fody for automatic property notification
  - ViewModels now use `[Reactive]` attributes instead of manual `RaiseAndSetIfChanged`
  - Cleaner, more maintainable ViewModel code
  - Compile-time property weaving via Fody framework

### Thread Safety Improvements
- **UIThreadHelper Utility**: Comprehensive UI thread management (`KakeboApp/Utils/UIThreadHelper.cs`)
  - Centralized thread safety operations
  - Multiple execution patterns (sync, async, with results)
  - Error handling and logging integration
  - Replaces manual `Dispatcher.UIThread` checks in ViewModels
  - **Event Thread Safety**: All events now use `UIThreadHelper.InvokeOnUIThread()` for safe UI updates
    - `LayoutManager.LayoutChanged` event
    - `NavigationService.NavigatedTo/NavigatingFrom` events
    - `DatabaseService.DatabaseConnected/DatabaseDisconnected` events
    - Observable notifications in ViewModels

### Command Execution Enhancements
- **MainThreadScheduler**: All ReactiveCommands now use `RxApp.MainThreadScheduler`
  - Ensures commands execute on UI thread
  - Prevents cross-thread operation exceptions
  - Improved application responsiveness

### ViewModelBase Simplification
- Removed manual thread checking logic
- Simplified using `UIThreadHelper` methods
- Maintained `ExecuteSafelyAsync` patterns for consistent error handling
- Enhanced with automatic property notification via Fody attributes

### Database Creation Fix
- **DatabaseConnectionViewModel**: Fixed "Crear Nueva" button to properly create and connect to database
  - Now calls `_databaseService.CreateDatabaseAsync()` instead of just file selection
  - Automatic connection after database creation
  - Thread-safe event notification using `Dispatcher.UIThread.Post()`
- **DatabaseService**: Updated to use `UIThreadHelper` instead of obsolete `ThreadingHelper`
  - All database events now use proper UI thread marshalling