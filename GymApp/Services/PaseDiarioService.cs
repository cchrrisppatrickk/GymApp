using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Hosting;

namespace GymApp.Services;

public class PaseDiarioService : IPaseDiarioService
{
    private readonly IPaseDiarioRepository _paseDiarioRepository;
    private readonly IWebHostEnvironment _env;

    public PaseDiarioService(IPaseDiarioRepository paseDiarioRepository, IWebHostEnvironment env)
    {
        _paseDiarioRepository = paseDiarioRepository;
        _env = env;
    }

    public async Task RegistrarPaseAsync(PaseDiarioCreateDTO dto, int empleadoId)
    {
        string? rutaComprobante = await ProcesarComprobanteAsync(dto, empleadoId);

        var pase = new PaseDiario
        {
            UserId = dto.UserId,
            TurnoId = dto.TurnoId,
            UsuarioEmpleadoId = empleadoId,
            Monto = dto.Monto,
            MetodoPago = dto.MetodoPago,
            Observacion = dto.Observacion,
            FechaCreacion = DateTime.Now,
            ComprobanteRuta = rutaComprobante
        };

        await _paseDiarioRepository.InsertAsync(pase);
        await _paseDiarioRepository.SaveAsync();
    }

    public async Task<IEnumerable<PaseDiarioListDTO>> ListarPasesAsync()
    {
        var pases = await _paseDiarioRepository.ObtenerTodosConDetallesAsync();

        return pases.Select(p => new PaseDiarioListDTO
        {
            PaseDiarioId = p.PaseDiarioId,
            NombreCliente = p.User != null ? (p.User.NombreCompleto ?? p.User.NombreUsuario) : "Público General",
            NombreTurno = p.Turno.Nombre,
            Monto = p.Monto,
            MetodoPago = p.MetodoPago,
            NombreEmpleado = p.UsuarioEmpleado.NombreCompleto ?? p.UsuarioEmpleado.NombreUsuario,
            Fecha = p.FechaCreacion,
            Observacion = p.Observacion,
            ComprobanteRuta = p.ComprobanteRuta
        }).OrderByDescending(p => p.Fecha).ToList();
    }

    public async Task EliminarFisicamenteAsync(int id)
    {
        var pase = await _paseDiarioRepository.GetByIdAsync(id);
        if (pase != null)
        {
            await _paseDiarioRepository.DeleteAsync(id);
            await _paseDiarioRepository.SaveAsync();
        }
    }

    public async Task<PaseDiarioListDTO?> ObtenerDetallesPaseAsync(int id)
    {
        var p = await _paseDiarioRepository.ObtenerPorIdConDetallesAsync(id);
        if (p == null) return null;

        return new PaseDiarioListDTO
        {
            PaseDiarioId = p.PaseDiarioId,
            NombreCliente = p.User != null ? (p.User.NombreCompleto ?? p.User.NombreUsuario) : "Público General",
            NombreTurno = p.Turno.Nombre,
            Monto = p.Monto,
            MetodoPago = p.MetodoPago,
            NombreEmpleado = p.UsuarioEmpleado.NombreCompleto ?? p.UsuarioEmpleado.NombreUsuario,
            Fecha = p.FechaCreacion,
            Observacion = p.Observacion,
            ComprobanteRuta = p.ComprobanteRuta
        };
    }

    private async Task<string?> ProcesarComprobanteAsync(PaseDiarioCreateDTO dto, int empleadoId)
    {
        // Sin contenido → no hay comprobante que guardar
        bool tieneBase64 = !string.IsNullOrWhiteSpace(dto.ComprobanteBase64);
        bool tieneArchivo = dto.ComprobanteArchivo != null && dto.ComprobanteArchivo.Length > 0;

        if (!tieneBase64 && !tieneArchivo) return null;

        // Directorio destino: wwwroot/uploads/comprobantediario
        var dirFisico = Path.Combine(_env.WebRootPath, "uploads", "comprobantediario");
        Directory.CreateDirectory(dirFisico); // No-op si ya existe

        var nombreArchivo = $"pasediario_{empleadoId}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        var rutaFisica = Path.Combine(dirFisico, nombreArchivo);

        if (tieneBase64)
        {
            // Eliminar prefijo "data:image/jpeg;base64," u otros
            var base64Limpio = Regex.Replace(dto.ComprobanteBase64!, @"^data:image\/[a-zA-Z]+;base64,", string.Empty);
            var bytes = Convert.FromBase64String(base64Limpio);
            await File.WriteAllBytesAsync(rutaFisica, bytes);
        }
        else if (tieneArchivo)
        {
            using var stream = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write);
            await dto.ComprobanteArchivo!.CopyToAsync(stream);
        }

        return "/uploads/comprobantediario/" + nombreArchivo;
    }
}
