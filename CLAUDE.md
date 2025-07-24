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

### Current Dependencies (as of July 2025)
**Avalonia UI Stack:**
- Avalonia 11.3.2 - Core UI framework
- Avalonia.Controls.DataGrid 11.3.2 - DataGrid control
- Avalonia.Desktop 11.3.2 - Desktop platform support
- Avalonia.Themes.Fluent 11.3.2 - Fluent design theme
- Avalonia.Fonts.Inter 11.3.2 - Inter font family
- Avalonia.ReactiveUI 11.3.2 - ReactiveUI integration

**ReactiveUI Stack:**
- ReactiveUI 20.4.1 - Core reactive MVVM framework
- ReactiveUI.Fody 19.5.41 - Automatic property notification

**Data & Persistence:**
- LiteDB 5.0.21 - Embedded NoSQL database

**Hosting & DI:**
- Microsoft.Extensions.Hosting 9.0.0 - Generic host
- Microsoft.Extensions.DependencyInjection 9.0.0 - Dependency injection

**Logging:**
- Serilog 4.3.0 - Structured logging framework
- Serilog.Extensions.Hosting 9.0.0 - Host integration
- Serilog.Extensions.Logging 9.0.2 - Microsoft.Extensions.Logging bridge
- Serilog.Sinks.Console 6.0.0 - Console output

**Code Generation:**
- Fody 6.8.1 - IL weaving framework for compile-time enhancements

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

### Project Structure Updates
- **Serilog Integration**: Added comprehensive structured logging with multiple sinks
- **Microsoft Extensions**: Integrated hosting and dependency injection patterns
- **Platform Detection**: Enhanced platform-specific compilation with conditional constants (WINDOWS, LINUX, MACOS)
- **Obsolete Code Cleanup**: Removed deprecated `ThreadingHelper` in favor of `UIThreadHelper`

### Latest Improvements Summary
1. **Enhanced ReactiveUI Integration**: Full Fody weaving with `[Reactive]` attributes
2. **Improved Thread Safety**: Centralized UI thread management with `UIThreadHelper`
3. **Better Error Handling**: Consistent `Result<T>` pattern across all layers
4. **Modern .NET 9.0**: Latest framework features and performance improvements
5. **Comprehensive Logging**: Structured logging with Serilog across all components

## Critical Threading Fixes (July 2025)

### ReactiveCommand Threading Issues
Fixed critical threading exceptions in transaction creation workflow:

- **Problem**: `ReactiveCommand.CanExecute` notifications were being triggered from background threads, causing `System.InvalidOperationException: Call from invalid thread` errors
- **Root Cause**: `ViewModelBase.ExecuteSafelyAsync` was changing `IsBusy` property from background threads, which triggered reactive validation chains that attempted to access UI controls from non-UI threads
- **Solution**: Added `.ObserveOn(RxApp.MainThreadScheduler)` to all reactive property validation chains in ViewModels

### Key Threading Architecture Changes

#### 1. Observable Chain Threading
```csharp
// BEFORE (causing threading issues)
var canSave = this.WhenAnyValue(x => x.IsBusy, busy => !busy);

// AFTER (thread-safe)
var canSave = this.WhenAnyValue(x => x.IsBusy, busy => !busy)
    .ObserveOn(RxApp.MainThreadScheduler);
```

#### 2. Command Execution Patterns
- **TransactionsViewModel**: UI operations wrapped in `UIThreadHelper.InvokeOnUIThread()`
- **AddEditTransactionViewModel**: Simplified async operations to avoid nested threading calls
- All `ReactiveCommand` instances now use proper schedulers for both execution and validation

#### 3. Property Update Patterns
- `[Reactive]` properties can be safely updated from any thread (Fody handles marshalling)
- Observable chains that feed into UI commands must use `RxApp.MainThreadScheduler`
- Collection modifications always wrapped in `UIThreadHelper.InvokeOnUIThread()`

### Threading Best Practices Established
1. **ReactiveCommand Creation**: Always specify `outputScheduler: RxApp.MainThreadScheduler`
2. **CanExecute Observables**: Always end with `.ObserveOn(RxApp.MainThreadScheduler)`
3. **Collection Updates**: Always use `UIThreadHelper.InvokeOnUIThread()` for ObservableCollection modifications
4. **UI Property Updates**: Simple property sets can rely on `[Reactive]` attributes, complex operations need thread marshalling
5. **Subject Notifications**: Can be called from any thread if subscribers use proper schedulers

## Final Threading Solution (July 2025)

### Comprehensive UIThreadHelper Strategy
After attempting various approaches including ReactiveObject overrides, the most reliable solution is consistent use of `UIThreadHelper.InvokeOnUIThread()` for all UI property updates from background threads.

### Key Implementation Points
1. **ViewModelBase.ExecuteSafelyAsync**: All `IsBusy` and `ErrorMessage` property updates wrapped in `UIThreadHelper`
2. **ReportsViewModel.LoadData**: Direct property updates wrapped for thread safety
3. **ReactiveCommand Configuration**: All commands use `outputScheduler: RxApp.MainThreadScheduler`
4. **CanExecute Observables**: All validation chains end with `.ObserveOn(RxApp.MainThreadScheduler)`

### Threading Architecture
```csharp
// Property updates from background threads
UIThreadHelper.InvokeOnUIThread(() => {
    IsBusy = true;
    ErrorMessage = string.Empty;
});

// ReactiveCommand with proper scheduler
SaveCommand = ReactiveCommand.CreateFromTask(
    SaveTransaction, 
    canSave.ObserveOn(RxApp.MainThreadScheduler), 
    RxApp.MainThreadScheduler
);
```

### Compilation and Runtime Success
- Fixed ReactiveObject override compilation errors
- Maintained compatibility with ReactiveUI.Fody `[Reactive]` attributes
- Eliminated `System.InvalidOperationException: Call from invalid thread` errors
- Preserved responsive UI while ensuring thread safety