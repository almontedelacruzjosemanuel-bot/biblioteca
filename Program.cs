using Biblioteca.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar CORS para evitar bloqueos en peticiones web o Swagger remoto
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

// 2. CORRECCIÓN CLAVE: Registrar DbConnection como Singleton o transitorio seguro
builder.Services.AddSingleton<DbConnection>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Habilitar Swagger siempre en desarrollo y producción para pruebas
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API v1");
    c.RoutePrefix = string.Empty; // Hace que Swagger abra directamente en la raíz de la URL
});

app.UseHttpsRedirection();

// 3. Activar CORS antes de los controladores
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();