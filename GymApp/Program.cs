using GymApp.Data;
using GymApp.Repositories;
using GymApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


// Agregar el servicio de DbContext
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// 2. Registro del Repositorio Genérico (Sintaxis para Tipos Genéricos Abiertos <>)
// Esto permite inyectar IGenericRepository<CualquierCosa> sin registrar uno por uno.
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 3. Registro de Repositorios Específicos
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Registro de Servicios de Negocio
builder.Services.AddScoped<IUsuarioService, UsuarioService>();


////////////////////////////////////////////////////////////////////////////////////////

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
