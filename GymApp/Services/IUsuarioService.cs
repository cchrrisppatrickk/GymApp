using GymApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GymApp.ViewModels;
using GymApp.ViewModels.ApiAgent;

namespace GymApp.Services
{
    public interface IUsuarioService
    {
        Task<PagedResult<UsuarioViewModel>> ObtenerUsuariosPaginadosAsync(string? buscar, int pagina, int? mes = null, int? anio = null, int tamanoPagina = 20);
        // CRUD Básico
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task<Usuario> ObtenerPorIdAsync(int id);

        /// <summary>Devuelve solo usuarios con rol Empleado o Admin (personal del gimnasio).</summary>
        Task<List<Usuario>> ObtenerPersonalAsync();

        /// <summary>Devuelve solo usuarios con rol Cliente (socios del gimnasio).</summary>
        Task<PagedResult<UsuarioViewModel>> ObtenerSociosPaginadosAsync(string? buscar, int pagina, int? mes = null, int? anio = null, int tamanoPagina = 20);
        
        // Lógica de Negocio Compleja
        Task<Usuario> CrearUsuarioAsync(Usuario usuario, string? passwordRaw, IFormFile? fotoArchivo = null, string? fotoBase64 = null);
        Task ActualizarUsuarioAsync(Usuario usuario, IFormFile? fotoArchivo = null, string? fotoBase64 = null);
        Task<bool> EliminarUsuarioAsync(int id);

        // Seguridad
        Task<Usuario> ValidarLoginAsync(string dni, string password);
        Task<List<string>> ObtenerPermisosUsuarioAsync(int userId);
        Task ActualizarPermisosUsuarioAsync(int userId, string[] permisos);

        // Funciones Extra
        byte[] GenerarImagenQR(Guid codigoQR); // Devuelve la imagen en bytes

        // ── Dominio de Usuarios — Consultas granulares para el Agente IA ──────
        /// <summary>Busca usuarios por nombre (parcial) o DNI (exacto).</summary>
        Task<IEnumerable<UsuarioAgenteDTO>> BuscarParaAgenteAsync(string termino);

        /// <summary>Devuelve usuarios registrados en los últimos N días.</summary>
        Task<IEnumerable<UsuarioAgenteDTO>> ObtenerRecientesParaAgenteAsync(int dias);

        /// <summary>Devuelve usuarios cuya fecha de registro coincide con el día indicado.</summary>
        Task<IEnumerable<UsuarioAgenteDTO>> ObtenerPorFechaExactaParaAgenteAsync(DateTime fecha);
    }
}