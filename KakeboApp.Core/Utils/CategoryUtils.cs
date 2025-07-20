using KakeboApp.Core.Models;

namespace KakeboApp.Core.Utils;

public static class CategoryUtils
{
    // Pattern matching para clasificación automática Kakebo
    public static KakeboCategory GetKakeboCategory(Category category) => category switch
    {
        // Supervivencia
        Category.Housing or 
        Category.Food or 
        Category.Transportation or 
        Category.Utilities or 
        Category.Healthcare or 
        Category.Insurance => KakeboCategory.Survival,
        
        // Opcional
        Category.Entertainment or 
        Category.Dining or 
        Category.Shopping or 
        Category.Hobbies or 
        Category.Travel or 
        Category.Sports => KakeboCategory.Optional,
        
        // Cultura
        Category.Books or 
        Category.Education or 
        Category.Courses or 
        Category.Subscriptions => KakeboCategory.Culture,
        
        // Inesperado
        Category.Emergency or 
        Category.Repairs or 
        Category.Medical or 
        Category.Legal or 
        Category.Other => KakeboCategory.Unexpected,
        
        // Ingresos no se clasifican en Kakebo
        Category.Salary or 
        Category.Investment or 
        Category.Freelance or 
        Category.Business or 
        Category.Gifts => throw new ArgumentException($"Income categories cannot be classified in Kakebo system: {category}")
    };
    
    // Nombres amigables en español
    public static string GetCategoryDisplayName(Category category) => category switch
    {
        // Ingresos
        Category.Salary => "Salario",
        Category.Investment => "Inversiones",
        Category.Freelance => "Freelance",
        Category.Business => "Negocio",
        Category.Gifts => "Regalos",
        
        // Supervivencia
        Category.Housing => "Vivienda",
        Category.Food => "Comida",
        Category.Transportation => "Transporte",
        Category.Utilities => "Servicios",
        Category.Healthcare => "Salud",
        Category.Insurance => "Seguros",
        
        // Opcional
        Category.Entertainment => "Entretenimiento",
        Category.Dining => "Restaurantes",
        Category.Shopping => "Compras",
        Category.Hobbies => "Aficiones",
        Category.Travel => "Viajes",
        Category.Sports => "Deportes",
        
        // Cultura
        Category.Books => "Libros",
        Category.Education => "Educación",
        Category.Courses => "Cursos",
        Category.Subscriptions => "Suscripciones",
        
        // Inesperado
        Category.Emergency => "Emergencia",
        Category.Repairs => "Reparaciones",
        Category.Medical => "Médico",
        Category.Legal => "Legal",
        Category.Other => "Otros",
        
        _ => category.ToString()
    };
    
    // Nombre completo con subcategoría opcional
    public static string GetFullCategoryName(Category category, string? subcategory)
    {
        var categoryName = GetCategoryDisplayName(category);
        return string.IsNullOrWhiteSpace(subcategory) 
            ? categoryName 
            : $"{categoryName} - {subcategory}";
    }
    
    // Sugerencias contextuales por categoría
    public static IReadOnlyList<string> GetCommonSubcategories(Category category) => category switch
    {
        Category.Housing => new[] { "Alquiler", "Hipoteca", "Mantenimiento", "Muebles", "Decoración" },
        Category.Food => new[] { "Supermercado", "Mercado", "Carnicería", "Panadería", "Verdulería" },
        Category.Transportation => new[] { "Gasolina", "Transporte público", "Taxi", "Uber", "Parking", "Mantenimiento vehículo" },
        Category.Utilities => new[] { "Electricidad", "Agua", "Gas", "Internet", "Teléfono", "Basura" },
        Category.Healthcare => new[] { "Médico", "Dentista", "Farmacia", "Análisis", "Especialista", "Emergencia" },
        Category.Insurance => new[] { "Salud", "Vida", "Hogar", "Vehículo", "Responsabilidad civil" },
        
        Category.Entertainment => new[] { "Cine", "Teatro", "Conciertos", "Videojuegos", "Streaming", "Parques" },
        Category.Dining => new[] { "Desayuno", "Almuerzo", "Cena", "Cafetería", "Bar", "Comida rápida" },
        Category.Shopping => new[] { "Ropa", "Calzado", "Accesorios", "Electrónicos", "Hogar", "Regalos" },
        Category.Hobbies => new[] { "Deportes", "Arte", "Música", "Fotografía", "Jardinería", "Colecciones" },
        Category.Travel => new[] { "Vuelos", "Hotel", "Alquiler coche", "Excursiones", "Comidas", "Souvenirs" },
        Category.Sports => new[] { "Gimnasio", "Clases", "Equipamiento", "Competiciones", "Nutrición" },
        
        Category.Books => new[] { "Ficción", "No ficción", "Técnicos", "Académicos", "Revistas", "Audiolibros" },
        Category.Education => new[] { "Matrícula", "Libros", "Material", "Transporte", "Alojamiento" },
        Category.Courses => new[] { "Online", "Presencial", "Certificaciones", "Talleres", "Seminarios" },
        Category.Subscriptions => new[] { "Netflix", "Spotify", "Revistas", "Software", "Noticias", "Fitness" },
        
        Category.Emergency => new[] { "Médica", "Hogar", "Vehículo", "Familia", "Trabajo", "Legal" },
        Category.Repairs => new[] { "Hogar", "Vehículo", "Electrónicos", "Ropa", "Muebles" },
        Category.Medical => new[] { "Urgencias", "Medicamentos", "Tratamientos", "Cirugía", "Terapia" },
        Category.Legal => new[] { "Abogado", "Notario", "Multas", "Trámites", "Documentos" },
        
        Category.Salary => new[] { "Salario base", "Extras", "Comisiones", "Bonus", "Aguinaldo" },
        Category.Investment => new[] { "Dividendos", "Intereses", "Ganancias capital", "Alquiler", "Royalties" },
        Category.Freelance => new[] { "Consultoría", "Diseño", "Programación", "Redacción", "Traducción" },
        Category.Business => new[] { "Ventas", "Servicios", "Productos", "Comisiones", "Licencias" },
        Category.Gifts => new[] { "Familia", "Amigos", "Premios", "Herencias", "Devoluciones" },
        Category.Other => new[] { "Varios", "Misceláneos", "Sin clasificar" },
        
        _ => Array.Empty<string>()
    };
    
    // Categorías de ingresos
    public static IReadOnlyList<Category> GetIncomeCategories() => new[]
    {
        Category.Salary,
        Category.Investment,
        Category.Freelance,
        Category.Business,
        Category.Gifts
    };
    
    // Categorías de gastos
    public static IReadOnlyList<Category> GetExpenseCategories() => new[]
    {
        // Supervivencia
        Category.Housing,
        Category.Food,
        Category.Transportation,
        Category.Utilities,
        Category.Healthcare,
        Category.Insurance,
        
        // Opcional
        Category.Entertainment,
        Category.Dining,
        Category.Shopping,
        Category.Hobbies,
        Category.Travel,
        Category.Sports,
        
        // Cultura
        Category.Books,
        Category.Education,
        Category.Courses,
        Category.Subscriptions,
        
        // Inesperado
        Category.Emergency,
        Category.Repairs,
        Category.Medical,
        Category.Legal,
        Category.Other
    };
    
    // Obtener categorías por tipo Kakebo
    public static IReadOnlyList<Category> GetCategoriesByKakeboType(KakeboCategory kakeboCategory) => 
        GetExpenseCategories().Where(c => GetKakeboCategory(c) == kakeboCategory).ToList();
    
    // Validar si una categoría es válida para un tipo de transacción
    public static bool IsValidCategoryForType(Category category, TransactionType type) => type switch
    {
        TransactionType.Income => GetIncomeCategories().Contains(category),
        TransactionType.Expense => GetExpenseCategories().Contains(category),
        _ => false
    };
    
    // Obtener la primera categoría válida para un tipo
    public static Category GetDefaultCategoryForType(TransactionType type) => type switch
    {
        TransactionType.Income => Category.Salary,
        TransactionType.Expense => Category.Food,
        _ => throw new ArgumentException($"Unknown transaction type: {type}")
    };
}