using GymApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GymApp.ViewModels;

namespace GymApp.Services
{
    public interface IUsuarioService
    {
        Task<PagedResult<UsuarioViewModel>> ObtenerUsuariosPaginadosAsync(string? buscar, int pagina, int tamanoPagina = 10);
        // CRUD Básico
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<Usuario> ObtenerPorIdAsync(int id);
        
        // Lógica de Negocio Compleja
        Task<Usuario> CrearUsuarioAsync(Usuario usuario, string? passwordRaw, IFormFile? fotoArchivo = null);
        Task ActualizarUsuarioAsync(Usuario usuario, IFormFile? fotoArchivo = null);
        Task<bool> EliminarUsuarioAsync(int id);


        // Seguridad
        Task<Usuario> ValidarLoginAsync(string dni, string password);

        // Funciones Extra
        byte[] GenerarImagenQR(Guid codigoQR); // Devuelve la imagen en bytes
    }
}