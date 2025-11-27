using GymApp.Models;
using GymApp.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymApp.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly ITurnoRepository _turnoRepository;

        public TurnoService(ITurnoRepository turnoRepository)
        {
            _turnoRepository = turnoRepository;
        }

        public async Task<IEnumerable<Turno>> ObtenerTodosAsync()
        {
            return await _turnoRepository.GetAllAsync();
        }

        public async Task<Turno> ObtenerPorIdAsync(int id)
        {
            return await _turnoRepository.GetByIdAsync(id);
        }

        public async Task CrearTurnoAsync(Turno turno)
        {
            // VALIDACIÓN DE NEGOCIO: La hora fin no puede ser menor a la hora inicio
            if (turno.HoraFin <= turno.HoraInicio)
            {
                throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");
            }

            await _turnoRepository.InsertAsync(turno);
            await _turnoRepository.SaveAsync();
        }

        public async Task ActualizarTurnoAsync(Turno turno)
        {
            // VALIDACIÓN DE NEGOCIO
            if (turno.HoraFin <= turno.HoraInicio)
            {
                throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio.");
            }

            await _turnoRepository.UpdateAsync(turno);
            await _turnoRepository.SaveAsync();
        }

        public async Task EliminarTurnoAsync(int id)
        {
            // Aquí podrías validar si el turno está siendo usado en una Membresía antes de borrar
            await _turnoRepository.DeleteAsync(id);
            await _turnoRepository.SaveAsync();
        }
    }
}