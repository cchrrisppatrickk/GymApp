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

                // 1. Asegurarse de que la BD exista (aplica migraciones pendientes si las hay)
                context.Database.EnsureCreated();

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
                    // Buscamos el ID del rol Admin que acabamos de crear
                    var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Admin");

                    var adminUser = new Usuario
                    {
                        NombreCompleto = "Administrador del Sistema",
                        Dni = "00000000", // DNI ficticio
                        NombreUsuario = "admin",
                        Email = "admin@gymapp.com",
                        Telefono = "000-000000",
                        RoleId = adminRole.RoleId,
                        Estado = true,
                        FechaRegistro = DateTime.Now,

                        // IMPORTANTE: Aquí debes usar tu algoritmo de Hash. 
                        // Si usas BCrypt: BCrypt.Net.BCrypt.HashPassword("admin123")
                        // Por ahora pondré el texto plano o un hash simulado, asegúrate de encriptarlo.
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
                    };

                    context.Usuarios.Add(adminUser);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}