using GymApp.Data;
using GymApp.Repositories;
using GymApp.Services;
using GymApp.Configuration;
using GymApp.Constants;
// 1. AGREGAR ESTE NAMESPACE
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
builder.Services.AddScoped<ICongelamientoRepository, CongelamientoRepository>();
builder.Services.AddScoped<ICongelamientoService, CongelamientoService>();
builder.Services.AddScoped<IMembresiaRepository, MembresiaRepository>();
builder.Services.AddScoped<IMembresiaService, MembresiaService>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
// Agrega el de Usuarios si falta
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRestriccionService, RestriccionService>();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddScoped<IVentaRepository, VentaRepository>();
builder.Services.AddScoped<IVentaService, VentaService>();

builder.Services.AddScoped<IPaseDiarioRepository, PaseDiarioRepository>();
builder.Services.AddScoped<IPaseDiarioService, PaseDiarioService>();

// En Program.cs
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<IConfiguracionAlertaRepository, ConfiguracionAlertaRepository>();

// Jobs
builder.Services.AddScoped<NotificacionProgramadaJob>();
builder.Services.AddScoped<AlertaVencimientoJob>();

// ============================================================
// HANGFIRE
// ============================================================
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Webhook / n8n
builder.Services.AddHttpClient();
builder.Services.Configure<N8nSettings>(builder.Configuration.GetSection("N8nConfig"));
builder.Services.AddScoped<IWebhookService, WebhookService>();
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
// 4. CONFIGURAR AUTORIZACIÓN (POLÍTICAS Y CLAIMS)
// ============================================================
builder.Services.AddAuthorization(options => {
   // --- PAGOS ---
   options.AddPolicy(AppPoliticas.RequiereVerPagos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PagosVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereCrearPagos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PagosCrear) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereEditarPagos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PagosEditar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereAnularPagos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PagosAnular) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   // --- MEMBRESÍAS ---
   options.AddPolicy(AppPoliticas.RequiereVerMembresias, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.MembresiasVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereCrearMembresias, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.MembresiasCrear) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereEliminarMembresias, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.MembresiasEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereEditarMembresias, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.MembresiasEditar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereCongelarMembresias, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.MembresiasCongelar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereRenovarMembresias, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.MembresiasRenovar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- VENTAS ---
   options.AddPolicy(AppPoliticas.RequiereVerVentas, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.VentasVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarVentas, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.VentasAnular) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.VentasEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- PRODUCTOS ---
   options.AddPolicy(AppPoliticas.RequiereVerProductos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.ProductosVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarProductos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.ProductosEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- PLANES ---
   options.AddPolicy(AppPoliticas.RequiereVerPlanes, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PlanesVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarPlanes, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PlanesEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- TURNOS ---
   options.AddPolicy(AppPoliticas.RequiereVerTurnos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.TurnosVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarTurnos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.TurnosEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.TurnosAnular) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- CONGELAMIENTOS ---
   options.AddPolicy(AppPoliticas.RequiereVerCongelamientos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.CongelamientosVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereCrearCongelamientos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.CongelamientosCrear) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereEliminarCongelamientos, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.CongelamientosEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.CongelamientosAnular) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- ACCESO ---
   options.AddPolicy(AppPoliticas.RequiereVerAcceso, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AccesoVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarAcceso, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AccesoEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- PASES DIARIOS ---
   options.AddPolicy(AppPoliticas.RequiereVerPasesDiarios, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PasesDiariosVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarPasesDiarios, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PasesDiariosEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.PasesDiariosAnular) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- USUARIOS ---
   options.AddPolicy(AppPoliticas.RequiereVerUsuarios, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.UsuariosVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereCrearUsuarios, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.UsuariosCrear) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereEditarUsuarios, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.UsuariosEditar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   options.AddPolicy(AppPoliticas.RequiereEliminarUsuarios, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.UsuariosEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- ROLES ---
   options.AddPolicy(AppPoliticas.RequiereVerRoles, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.RolesVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
       
   options.AddPolicy(AppPoliticas.RequiereEliminarRoles, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.RolesEliminar) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));

   // --- DASHBOARD ---
   options.AddPolicy(AppPoliticas.RequiereVerDashboard, policy => 
       policy.RequireAssertion(context => context.User.HasClaim(TipoClaim.Permiso, AppPermisos.DashboardVer) || context.User.HasClaim(TipoClaim.Permiso, AppPermisos.AdminAccesoTotal) || context.User.IsInRole(AppRoles.Admin)));
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
// HANGFIRE DASHBOARD Y RECURRING JOB
// ============================================================
app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<NotificacionProgramadaJob>(
    "notificacion-programada",
    job => job.EjecutarRevisionAsync(),
    Cron.Minutely);

RecurringJob.AddOrUpdate<AlertaVencimientoJob>(
    "AvisoVencimientos",
    job => job.EjecutarAlertasAsync(),
    Cron.Hourly);

// ============================================================
// 3. ACTIVAR EL MIDDLEWARE (¡IMPORTANTE EL ORDEN!)
// ============================================================
app.UseAuthentication(); // <--- Debe ir ANTES de Authorization
app.UseAuthorization();
// ============================================================

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rutas convencionales (MVC)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Rutas de Atributos (Web API)
app.MapControllers();

app.Run();



//Scaffold - DbContext "Server=DESKTOP-6U3IQMJ\SQLEXPRESS;Database=GymDB;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer - OutputDir Models - ContextDir Data - Context GymDbContext - Force