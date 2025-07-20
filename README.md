# NKakebo - Aplicación de Gestión Financiera Japonesa

Una aplicación multiplataforma que implementa el método japonés Kakebo para la gestión consciente de las finanzas personales, desarrollada con C# y Avalonia UI.

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
- 📱 **Interfaz responsive** para escritorio y móvil

## 🛠️ Tecnologías

- **Framework**: .NET 9.0
- **UI**: Avalonia UI 11.0
- **Arquitectura**: MVVM con ReactiveUI
- **Base de Datos**: LiteDB (embebida y encriptada)
- **Plataformas**: Windows Desktop y Android
- **Patrones**: Clean Architecture, Result Pattern, DI

## 🏗️ Arquitectura

```
KakeboApp/
├── KakeboApp.Core/              # Lógica de negocio
│   ├── Models/                  # Entidades y DTOs
│   ├── Interfaces/              # Contratos de servicios
│   ├── Services/                # Servicios de negocio
│   ├── Data/                    # Acceso a datos
│   └── Utils/                   # Utilidades y helpers
├── KakeboApp/                   # Presentación
│   ├── ViewModels/              # ViewModels MVVM
│   ├── Views/                   # Vistas XAML
│   ├── Converters/              # Convertidores de datos
│   ├── Services/                # Servicios de plataforma
│   └── Platforms/Android/       # Configuración Android
└── README.md
```

### Principios de Diseño

- **Inmutabilidad**: Records para entidades
- **Functional Error Handling**: Result<T> pattern
- **Reactive Programming**: ReactiveUI para UI responsiva
- **Dependency Injection**: Microsoft.Extensions.DI
- **Clean Separation**: Core business logic separada de UI

## 🚀 Instalación y Configuración

### Prerrequisitos

- .NET 9.0 SDK
- Visual Studio 2022 / JetBrains Rider / VS Code
- Para Android: Workload de .NET MAUI

### Clonar y Ejecutar

```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/kakebo-app.git
cd kakebo-app

# Restaurar dependencias
dotnet restore

# Ejecutar en Windows
dotnet run --project KakeboApp --framework net9.0-windows

# Compilar para Android
dotnet build --framework net9.0-android
```

### Configuración de Base de Datos

1. **Primera ejecución**: La app te pedirá seleccionar o crear una base de datos
2. **Encriptación opcional**: Puedes proteger tu BD con contraseña
3. **Ubicación**: 
   - Windows: `%AppData%/KakeboApp/`
   - Android: Directorio interno de la aplicación

## 📱 Uso de la Aplicación

### 1. Gestión de Transacciones
- **Agregar**: Botón "Nueva Transacción" con formulario completo
- **Editar**: Doble clic en cualquier transacción
- **Filtrar**: Por texto, categoría o tipo
- **Sugerencias**: Subcategorías automáticas según categoría principal

### 2. Presupuesto Kakebo
- **Navegación mensual**: Anterior/Siguiente mes
- **4 categorías**: Establece presupuesto para cada tipo de gasto
- **Alertas visuales**: Indicadores cuando excedes presupuesto
- **Balance automático**: Cálculo de tasa de ahorro

### 3. Reportes y Análisis
- **Vista agregada**: Gastos totales por categoría principal
- **Vista detallada**: Desglose por subcategorías
- **Porcentajes**: Distribución automática del gasto
- **Navegación temporal**: Análisis mes a mes

## 🎨 Capturas de Pantalla

### Escritorio
- Sidebar navegable con páginas principales
- Panel lateral deslizable para edición
- DataGrids con filtros avanzados
- Formularios con validación reactiva

### Android
- Interfaz adaptativa táctil
- Navegación optimizada para móvil
- Controles nativos Android
- Teclado virtual inteligente

## 🔧 Desarrollo

### Estructura del Código

#### Models (Core)
```csharp
public record Transaction
{
    public int? Id { get; init; }
    public required string Description { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime Date { get; init; }
    public required TransactionType Type { get; init; }
    public required Category Category { get; init; }
    // ...
}
```

#### Services con Result Pattern
```csharp
public async Task<Result<Transaction>> AddTransactionAsync(Transaction transaction)
{
    try
    {
        // Lógica de negocio
        return new Result<Transaction>.Success(savedTransaction);
    }
    catch (Exception ex)
    {
        return new Result<Transaction>.Error($"Error: {ex.Message}");
    }
}
```

#### ViewModels Reactivos
```csharp
public class TransactionsViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> AddTransactionCommand { get; }
    
    private async Task LoadTransactions()
    {
        var transactions = await _transactionService.GetAllTransactionsAsync();
        // Actualizar UI automáticamente
    }
}
```

### Extending la Aplicación

#### Agregar Nueva Categoría
1. Añadir enum value en `Category`
2. Actualizar `CategoryUtils.GetKakeboCategory()`
3. Agregar display name en `GetCategoryDisplayName()`
4. Definir subcategorías en `GetCommonSubcategories()`

#### Nueva Funcionalidad de Reporte
1. Crear query en `TransactionService`
2. Añadir método en `ITransactionService`
3. Implementar en ViewModel
4. Crear vista XAML con binding

## 🧪 Testing

```bash
# Ejecutar tests unitarios
dotnet test

# Coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Áreas de Testing
- **Servicios**: Lógica de negocio con mocks
- **Utils**: Funciones puras de categorización
- **ViewModels**: Comandos y propiedades reactivas
- **Database**: Operaciones CRUD con BD en memoria

## 📦 Deployment

### Windows Desktop
```bash
# Publicación autocontenida
dotnet publish -c Release -r win-x64 --self-contained
```

### Android APK
```bash
# Generar APK
dotnet publish -c Release -f net9.0-android
```

## 🤝 Contribución

### Guidelines
1. **Fork** el repositorio
2. **Branch** para nueva feature: `git checkout -b feature/nueva-funcionalidad`
3. **Commit** cambios: `git commit -m 'Add: nueva funcionalidad'`
4. **Push** branch: `git push origin feature/nueva-funcionalidad`
5. **Pull Request** con descripción detallada

### Convenciones de Código
- **C# Coding Standards**: Seguir Microsoft guidelines
- **Naming**: PascalCase para público, camelCase para privado
- **Async/Await**: Siempre para operaciones I/O
- **Immutability**: Preferir records y readonly
- **Error Handling**: Result<T> pattern, no excepciones para flujo

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](LICENSE) para más detalles.

## 🎯 Roadmap

### Próximas Funcionalidades
- [ ] **Exportación**: PDF y Excel de reportes
- [ ] **Gráficos**: Charts interactivos con tendencias
- [ ] **Metas**: Objetivos de ahorro a largo plazo
- [ ] **Categorías personalizadas**: Definidas por usuario
- [ ] **Multi-moneda**: Soporte para diferentes divisas
- [ ] **Cloud sync**: Sincronización entre dispositivos
- [ ] **Notificaciones**: Recordatorios y alertas
- [ ] **Temas**: Dark mode y personalización

### Mejoras Técnicas
- [ ] **Unit Tests**: Cobertura completa
- [ ] **Performance**: Optimización de queries
- [ ] **Accessibility**: Soporte completo WCAG
- [ ] **Localization**: Múltiples idiomas
- [ ] **CI/CD**: Pipeline automatizado

## 📞 Soporte

- **Issues**: [GitHub Issues](https://github.com/tu-usuario/kakebo-app/issues)
- **Documentación**: [Wiki del proyecto](https://github.com/tu-usuario/kakebo-app/wiki)
- **Discusiones**: [GitHub Discussions](https://github.com/tu-usuario/kakebo-app/discussions)

---

**Kakebo** - _Transformando la gestión financiera con sabiduría japonesa y tecnología moderna_ 🏮💰
