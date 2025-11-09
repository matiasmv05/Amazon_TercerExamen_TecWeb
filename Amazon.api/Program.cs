using Amazon.Core.Interface;
using Amazon.Core.Services;
using Amazon.Infrastructure.Data;
using Amazon.Infrastructure.Filters;
using Amazon.Infrastructure.Mappings;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;


/// <summary>
/// Punto de entrada principal de la aplicación Amazon API
/// </summary>
/// <remarks>
/// Esta clase configura y ejecuta la API REST de Amazon que incluye:
/// - Gestión de usuarios, productos, órdenes y pagos
/// - Validación automática de datos con FluentValidation
/// - Documentación automática con Swagger/OpenAPI
/// - Configuración de base de datos SQL Server
/// - Manejo global de excepciones y validaciones
/// </remarks>
public class Program {

    /// <summary>
    /// Método principal que configura y ejecuta la aplicación web
    /// </summary>
    /// <param name="args">Argumentos de línea de comandos para la configuración</param>
    /// <remarks>
    /// El pipeline de ejecución incluye:
    /// 1. Configuración de servicios y dependencias
    /// 2. Configuración de base de datos y Entity Framework
    /// 3. Registro de servicios de aplicación
    /// 4. Configuración de controladores y serialización JSON
    /// 5. Configuración de Swagger para documentación API
    /// 6. Configuración de validadores FluentValidation
    /// 7. Middleware de autenticación y autorización
    /// 8. Mapeo de endpoints de controladores
    /// </remarks>
    public static void Main(string[] args)
{
   var builder = WebApplication.CreateBuilder(args);

        if (builder.Environment.IsDevelopment())
        {

        }

        /// <summary>
        /// Configuración de la conexión a base de datos SQL Server
        /// </summary>
        /// <remarks>
        /// La cadena de conexión se obtiene desde appsettings.json
        /// Se configura Entity Framework Core con el contexto AmazonContext
        /// </remarks>
        #region Configurar la BD SqlServer
        var connectionString = builder.Configuration.GetConnectionString("ConnectionSqlServer");
    builder.Services.AddDbContext<AmazonContext>(options => options.UseSqlServer(connectionString));
        #endregion

        /// <summary>
        /// Configuración de AutoMapper para mapeo entre entidades y DTOs
        /// </summary>
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // Inyectar las repositories y servicios de aplicación
        /// <summary>
        /// Registro de servicios de la capa de aplicación
        /// </summary>
        /// <remarks>
        /// Se registran los servicios que orquestan la lógica de negocio
        /// y coordinan entre repositories y entidades de dominio
        /// </remarks>
        builder.Services.AddTransient<IOrderService, OrderService>();
        builder.Services.AddTransient<IUserService, UserService>();
        builder.Services.AddTransient<IProductService, ProductService>();
        builder.Services.AddTransient<IPaymentService, PaymentService>();

        /// <summary>
        /// Registro de repositories genéricos y patrones de diseño
        /// </summary>
        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        builder.Services.AddScoped<IDapperContext, DapperContext>();

        //builder.Services.AddScoped<IUserRepository, UserRepository>();
        //builder.Services.AddScoped<IProductRepository, ProductRepository>();
        //builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        //builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();



        // Configuración de controladores con filtros globales
        /// <summary>
        /// Configuración del sistema de controladores ASP.NET Core
        /// </summary>
        /// <remarks>
        /// Se incluyen filtros globales para:
        /// - Manejo de excepciones (GlobalExceptionFilter)
        /// - Validación automática de modelos (ValidationFilter)
        /// - Configuración de serialización JSON para evitar referencias circulares
        /// - Supresión de validación automática del ModelState para usar FluentValidation
        /// </remarks>
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
            options.Filters.Add<ValidationFilter>();

        }).AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }).ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Configuración de Swagger para documentación API
        /// <summary>
        /// Configuración de Swagger/OpenAPI para documentación automática
        /// </summary>
        /// <remarks>
        /// Genera documentación interactiva de la API con:
        /// - Información del proyecto y contacto
        /// - Comentarios XML de los controladores
        /// - Especificación OpenAPI v3
        /// </remarks>
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Backend Amazon API",
                Version = "v1",
                Description = "Documentacion de la API de Amazon - net 9",
                Contact = new()
                {
                    Name = "Equipo de Desarrollo UCB",
                    Email = "desarrollo@ucb.edu.bo"
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        // Configuración de FluentValidation
        /// <summary>
        /// Registro de validadores FluentValidation desde el assembly
        /// </summary>
        /// <remarks>
        /// Se registran automáticamente todos los validadores que implementen
        /// AbstractValidator para los DTOs correspondientes
        /// </remarks>
        builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<ProductDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<GetByIdRequestValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<CrearOrdenRequestValidation>();
        builder.Services.AddValidatorsFromAssemblyContaining<OrderItemDtoValidator>();
        builder.Services.AddValidatorsFromAssemblyContaining<PaymentDtoValidator>();

        /// <summary>
        /// Registro del servicio de validación personalizado
        /// </summary>
        builder.Services.AddScoped<IValidationService, ValidationService>();


        var app = builder.Build();

        // Configuración del pipeline de HTTP para entorno de desarrollo
        /// <summary>
        /// Configuración de Swagger UI en entorno de desarrollo
        /// </summary>
        /// <remarks>
        /// Habilita la interfaz web de Swagger en la ruta raíz
        /// Solo disponible en entorno de desarrollo
        /// </remarks>
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Backend Social Media API v1");
                options.RoutePrefix = string.Empty;
            });
        }

        // Configuración del pipeline de middleware HTTP
        /// <summary>
        /// Configuración del pipeline de procesamiento de requests HTTP
        /// </summary>
        /// <remarks>
        /// El orden del middleware es crítico para el funcionamiento correcto
        /// </remarks>
        app.UseHttpsRedirection();
        
    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
}


