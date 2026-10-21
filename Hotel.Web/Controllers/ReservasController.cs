using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Hotel.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController(ReservaRepositorio repositorio) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await repositorio.ObtenerTodasAsync());
    }

    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso?.ToString("yyyy-MM-dd");
        ViewBag.Salida = salida?.ToString("yyyy-MM-dd");

        if (!ingreso.HasValue && !salida.HasValue)
        {
            return View(Array.Empty<HabitacionDisponibleViewModel>());
        }

        if (!ingreso.HasValue || !salida.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Debe indicar las fechas de ingreso y salida.");
            return View(Array.Empty<HabitacionDisponibleViewModel>());
        }

        if (salida.Value <= ingreso.Value)
        {
            ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior al ingreso.");
            return View(Array.Empty<HabitacionDisponibleViewModel>());
        }

        return View(await repositorio.ObtenerDisponiblesAsync(ingreso.Value, salida.Value));
    }

    public async Task<IActionResult> Create(int? idHabitacion, DateOnly? ingreso, DateOnly? salida)
    {
        var reserva = new Reserva
        {
            IdHabitacion = idHabitacion ?? 0,
            FechaIngreso = ingreso ?? DateOnly.FromDateTime(DateTime.Today),
            FechaSalida = salida ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Estado = "Pendiente"
        };

        await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion);
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        QuitarNavegacionesDelModelState();
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(nameof(Reserva.FechaSalida),
                "La fecha de salida debe ser posterior a la fecha de ingreso.");
        }

        if (reserva.FechaIngreso < hoy)
        {
            ModelState.AddModelError(nameof(Reserva.FechaIngreso),
                "La fecha de ingreso no puede ser anterior a hoy.");
        }

        if (reserva.Observaciones?.Length > 200)
        {
            ModelState.AddModelError(nameof(Reserva.Observaciones),
                "Las observaciones admiten un máximo de 200 caracteres.");
        }

        if (!await repositorio.HuespedActivoExisteAsync(reserva.IdHuesped))
        {
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "Seleccione un huésped activo.");
        }

        if (!await repositorio.HabitacionActivaExisteAsync(reserva.IdHabitacion))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "Seleccione una habitación activa.");
        }
        else if (reserva.FechaSalida > reserva.FechaIngreso &&
                 await repositorio.ExisteCruceAsync(
                     reserva.IdHabitacion,
                     reserva.FechaIngreso,
                     reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion),
                "La habitación ya está reservada en ese rango de fechas.");
        }

        if (ModelState.IsValid)
        {
            await repositorio.CrearAsync(reserva);
            TempData["Exito"] = "La reserva se registró con estado Pendiente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarListasAsync(reserva.IdHuesped, reserva.IdHabitacion);
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await repositorio.CancelarAsync(id);
        TempData[resultado == "Cancelada" ? "Exito" : "Error"] = resultado switch
        {
            "Cancelada" => "La reserva fue cancelada correctamente.",
            "No cancelable" => "Solo se pueden cancelar reservas Pendientes o Confirmadas.",
            _ => "No se encontró la reserva solicitada."
        };
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync(int? huespedSeleccionado, int? habitacionSeleccionada)
    {
        var huespedes = await repositorio.ObtenerHuespedesActivosAsync();
        var habitaciones = await repositorio.ObtenerHabitacionesActivasAsync();

        ViewBag.IdHuesped = new SelectList(
            huespedes.Select(h => new
            {
                h.IdHuesped,
                NombreCompleto = $"{h.Apellidos}, {h.Nombres} - {h.Dni}"
            }),
            "IdHuesped",
            "NombreCompleto",
            huespedSeleccionado);

        ViewBag.IdHabitacion = new SelectList(
            habitaciones.Select(h => new
            {
                h.IdHabitacion,
                Descripcion = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre} (S/ {h.IdTipoHabitacionNavigation.PrecioNoche:N2})"
            }),
            "IdHabitacion",
            "Descripcion",
            habitacionSeleccionada);
    }

    private void QuitarNavegacionesDelModelState()
    {
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.Estado));
        ModelState.Remove(nameof(Reserva.FechaRegistro));
    }
}
