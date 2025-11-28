using GymApp.Data;
using GymApp.Repositories;
using GymApp.Services;
using Microsoft.EntityFrameworkCore;
// 1. AGREGAR ESTE NAMESPACE
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configuración de BD (Ya la tienes)
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyección de Dependencias (Tus repositorios y servicios actuales)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPlaneRepository, PlaneRepository>();
builder.Services.AddScoped<IPlaneService, PlaneService>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();
builder.Services.AddScoped<ITurnoService, TurnoService>();
builder.Services.AddScoped<IMembresiaRepository, MembresiaRepository>();
builder.Services.AddScoped<IMembresiaService, MembresiaService>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
// Agrega el de Usuarios si falta
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IVentaService, VentaService>();

// ============================================================
// 2. CONFIGURAR AUTENTICACIÓN (COOKIES)
// ============================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Si el usuario no tiene acceso, lo mandamos aquí
        options.LoginPath = "/Auth/Login";
        // Si el usuario intenta entrar a algo que su rol no permite
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        // Tiempo de vida de la cookie
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });
// ============================================================

var app = builder.Build();

// ============================================================
// INICIO DEL DATA SEEDING (SEMBRADO DE DATOS)
// ============================================================
// Esto se ejecuta una sola vez al levantar la app. 
// Verifica si la BD está vacía y la llena.
await DbSeeder.Seed(app);
// ============================================================


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ============================================================
// 3. ACTIVAR EL MIDDLEWARE (¡IMPORTANTE EL ORDEN!)
// ============================================================
app.UseAuthentication(); // <--- Debe ir ANTES de Authorization
app.UseAuthorization();
// ============================================================

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rutas
app.MapControllerRoute(
    name: "default",
    // Cambiamos el default para que vaya a la Presentación o Login
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();