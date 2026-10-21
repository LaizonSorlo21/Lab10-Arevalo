using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio(HotelContext context)
{
    public async Task<List<Habitacion>> ObtenerActivasAsync(int? idTipoHabitacion)
    {
        var consulta = context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (idTipoHabitacion.HasValue)
        {
            consulta = consulta.Where(h => h.IdTipoHabitacion == idTipoHabitacion.Value);
        }

        return await consulta.OrderBy(h => h.Numero).ToListAsync();
    }

    public async Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return await context.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);
    }

    public async Task<List<TipoHabitacion>> ObtenerTiposActivosAsync()
    {
        return await context.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<bool> NumeroExisteAsync(string numero, int? idExcluir = null)
    {
        var numeroNormalizado = numero.Trim();
        return await context.Habitacions.AsNoTracking().AnyAsync(h =>
            h.Numero == numeroNormalizado &&
            (!idExcluir.HasValue || h.IdHabitacion != idExcluir.Value));
    }

    public async Task CrearAsync(Habitacion habitacion)
    {
        habitacion.Numero = habitacion.Numero.Trim();
        habitacion.Activo = true;
        context.Habitacions.Add(habitacion);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Habitacion habitacion)
    {
        var existente = await context.Habitacions.FindAsync(habitacion.IdHabitacion);
        if (existente is null)
        {
            return false;
        }

        existente.Numero = habitacion.Numero.Trim();
        existente.Piso = habitacion.Piso;
        existente.IdTipoHabitacion = habitacion.IdTipoHabitacion;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarLogicoAsync(int id)
    {
        var habitacion = await context.Habitacions.FindAsync(id);
        if (habitacion is null)
        {
            return false;
        }

        habitacion.Activo = false;
        await context.SaveChangesAsync();
        return true;
    }
}
