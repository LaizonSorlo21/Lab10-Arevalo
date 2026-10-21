using Hotel.Web.Models;
using Hotel.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio(HotelContext context)
{
    public async Task<List<Reserva>> ObtenerTodasAsync()
    {
        return await context.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaIngreso)
            .ThenBy(r => r.IdHabitacionNavigation.Numero)
            .ToListAsync();
    }

    public async Task<List<HabitacionDisponibleViewModel>> ObtenerDisponiblesAsync(
        DateOnly ingreso,
        DateOnly salida)
    {
        var noches = salida.DayNumber - ingreso.DayNumber;

        return await context.Habitacions
            .AsNoTracking()
            .Where(h => h.Activo && !h.Reservas.Any(r =>
                r.Estado != "Cancelada" &&
                r.FechaIngreso < salida &&
                r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .Select(h => new HabitacionDisponibleViewModel
            {
                IdHabitacion = h.IdHabitacion,
                Numero = h.Numero,
                Piso = h.Piso,
                Tipo = h.IdTipoHabitacionNavigation.Nombre,
                Capacidad = h.IdTipoHabitacionNavigation.Capacidad,
                PrecioNoche = h.IdTipoHabitacionNavigation.PrecioNoche,
                Noches = noches
            })
            .ToListAsync();
    }

    public async Task<List<Huesped>> ObtenerHuespedesActivosAsync()
    {
        return await context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos)
            .ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<List<Habitacion>> ObtenerHabitacionesActivasAsync()
    {
        return await context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task<bool> HuespedActivoExisteAsync(int idHuesped)
    {
        return await context.Huespeds.AsNoTracking()
            .AnyAsync(h => h.IdHuesped == idHuesped && h.Activo);
    }

    public async Task<bool> HabitacionActivaExisteAsync(int idHabitacion)
    {
        return await context.Habitacions.AsNoTracking()
            .AnyAsync(h => h.IdHabitacion == idHabitacion && h.Activo);
    }

    public async Task<bool> ExisteCruceAsync(
        int idHabitacion,
        DateOnly ingreso,
        DateOnly salida)
    {
        return await context.Reservas.AsNoTracking().AnyAsync(r =>
            r.IdHabitacion == idHabitacion &&
            r.Estado != "Cancelada" &&
            r.FechaIngreso < salida &&
            r.FechaSalida > ingreso);
    }

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        reserva.FechaRegistro = DateTime.Now;
        reserva.Observaciones = string.IsNullOrWhiteSpace(reserva.Observaciones)
            ? null
            : reserva.Observaciones.Trim();
        context.Reservas.Add(reserva);
        await context.SaveChangesAsync();
    }

    public async Task<string> CancelarAsync(int id)
    {
        var reserva = await context.Reservas.FindAsync(id);
        if (reserva is null)
        {
            return "No encontrada";
        }

        if (reserva.Estado is not ("Pendiente" or "Confirmada"))
        {
            return "No cancelable";
        }

        reserva.Estado = "Cancelada";
        await context.SaveChangesAsync();
        return "Cancelada";
    }
}
