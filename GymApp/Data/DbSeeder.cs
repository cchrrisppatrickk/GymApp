using GymApp.Constants;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Data
{
    public static class DbSeeder
    {
        // Método principal que llamaremos desde Program.cs
        public static async Task Seed(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<GymDbContext>();

                // 1. Aplicar migraciones pendientes respetando el historial de EF Core
                await context.Database.MigrateAsync();

                // 2. CREAR ROLES POR DEFECTO
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(new List<Role>()
                    {
                        new Role { Nombre = "Admin", Descripcion = "Acceso total al sistema" },
                        new Role { Nombre = "Empleado", Descripcion = "Acceso a caja y asistencias" },
                        new Role { Nombre = "Cliente", Descripcion = "Acceso limitado a su perfil" }
                    });
                    await context.SaveChangesAsync();
                }

                // 3. CREAR USUARIO ADMIN POR DEFECTO
                if (!context.Usuarios.Any(u => u.NombreUsuario == "admin"))
                {
                    var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == AppRoles.Admin);

                    var adminUser = new Usuario
                    {
                        NombreCompleto = "Administrador del Sistema",
                        Dni = "00000000",
                        NombreUsuario = "admin",
                        Email = "admin@gymapp.com",
                        Telefono = "000-000000",
                        RoleId = adminRole.RoleId,
                        Estado = true,
                        FechaRegistro = DateTime.Now,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
                    };

                    context.Usuarios.Add(adminUser);
                    await context.SaveChangesAsync();
                }

                // 4. SEED DE PERMISOS DEL SISTEMA
                var permisosPorDefecto = new List<Permiso>
                {
                    // --- MÓDULO CAJA ---
                    new Permiso { PermisoId = "Caja.Ver",       Modulo = "Caja",       Descripcion = "Ver registro de pagos",    NivelPeligro = NivelPeligro.Bajo },
                    new Permiso { PermisoId = "Caja.Registrar", Modulo = "Caja",       Descripcion = "Registrar pagos",          NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Caja.Anular",    Modulo = "Caja",       Descripcion = "Anular pagos",             NivelPeligro = NivelPeligro.Alto },

                    // --- MÓDULO MEMBRESÍAS ---
                    new Permiso { PermisoId = "Membresias.Ver",      Modulo = "Membresías",  Descripcion = "Ver membresías",          NivelPeligro = NivelPeligro.Bajo },
                    new Permiso { PermisoId = "Membresias.Crear",    Modulo = "Membresías",  Descripcion = "Crear membresía",         NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Membresias.Congelar", Modulo = "Membresías",  Descripcion = "Congelar membresía",      NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Membresias.Renovar",  Modulo = "Membresías",  Descripcion = "Renovar membresía",       NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Membresias.Eliminar", Modulo = "Membresías",  Descripcion = "Eliminar membresía",      NivelPeligro = NivelPeligro.Alto },

                    // --- MÓDULO REPORTES ---
                    new Permiso { PermisoId = "Reportes.Ver",        Modulo = "Reportes",    Descripcion = "Ver estadísticas generales",     NivelPeligro = NivelPeligro.Bajo },
                    new Permiso { PermisoId = "Reportes.Financiero", Modulo = "Reportes",    Descripcion = "Ver estadísticas financieras",   NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Reportes.Exportar",   Modulo = "Reportes",    Descripcion = "Exportar datos / Reportes",      NivelPeligro = NivelPeligro.Alto },

                    // --- MÓDULO SOCIOS ---
                    new Permiso { PermisoId = "Socios.Ver",      Modulo = "Socios",      Descripcion = "Ver ficha de socio",          NivelPeligro = NivelPeligro.Bajo },
                    new Permiso { PermisoId = "Socios.Crear",    Modulo = "Socios",      Descripcion = "Registrar nuevo socio",       NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Socios.Editar",   Modulo = "Socios",      Descripcion = "Editar datos del socio",      NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Socios.Eliminar", Modulo = "Socios",      Descripcion = "Eliminar socio del sistema",  NivelPeligro = NivelPeligro.Alto },

                    // --- MÓDULO ACCESO ---
                    new Permiso { PermisoId = "Acceso.Escanear",   Modulo = "Acceso",     Descripcion = "Escanear código QR",          NivelPeligro = NivelPeligro.Bajo },
                    new Permiso { PermisoId = "Acceso.Historial",  Modulo = "Acceso",     Descripcion = "Ver historial de visitas",    NivelPeligro = NivelPeligro.Bajo },

                    // --- MÓDULO VENTAS ---
                    new Permiso { PermisoId = "Ventas.Ver",     Modulo = "Ventas",      Descripcion = "Ver ventas",                  NivelPeligro = NivelPeligro.Bajo },
                    new Permiso { PermisoId = "Ventas.Crear",   Modulo = "Ventas",      Descripcion = "Registrar nueva venta",       NivelPeligro = NivelPeligro.Medio },
                    new Permiso { PermisoId = "Ventas.Anular",  Modulo = "Ventas",      Descripcion = "Anular una venta",            NivelPeligro = NivelPeligro.Alto },
                };

                foreach (var permiso in permisosPorDefecto)
                {
                    if (!context.Permisos.Any(p => p.PermisoId == permiso.PermisoId))
                    {
                        context.Permisos.Add(permiso);
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
