using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymApp.Models;
using GymApp.Repositories;
using GymApp.ViewModels;

namespace GymApp.Services;

public class PaseDiarioService : IPaseDiarioService
{
    private readonly IPaseDiarioRepository _paseDiarioRepository;

    public PaseDiarioService(IPaseDiarioRepository paseDiarioRepository)
    {
        _paseDiarioRepository = paseDiarioRepository;
    }

    public async Task RegistrarPaseAsync(PaseDiarioCreateDTO dto, int empleadoId)
    {
        var pase = new PaseDiario
        {
            UserId = dto.UserId,
            TurnoId = dto.TurnoId,
            UsuarioEmpleadoId = empleadoId,
            Monto = dto.Monto,
            MetodoPago = dto.MetodoPago,
            Observacion = dto.Observacion,
            FechaCreacion = DateTime.Now
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
            Observacion = p.Observacion
        }).OrderByDescending(p => p.Fecha).ToList();
    }
}
