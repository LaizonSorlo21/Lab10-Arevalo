using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController(HabitacionRepositorio repositorio) : Controller
{
    public async Task<IActionResult> Index(int? idTipoHabitacion)
    {
        ViewBag.IdTipoHabitacion = new SelectList(
            await repositorio.ObtenerTiposActivosAsync(),
            nameof(TipoHabitacion.IdTipoHabitacion),
            nameof(TipoHabitacion.Nombre),
            idTipoHabitacion);
        ViewBag.FiltroTipo = idTipoHabitacion;
        return View(await repositorio.ObtenerActivasAsync(idTipoHabitacion));
    }

    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await repositorio.ObtenerPorIdAsync(id);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion { Activo = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        QuitarNavegacionesDelModelState();

        if (!string.IsNullOrWhiteSpace(habitacion.Numero) &&
            await repositorio.NumeroExisteAsync(habitacion.Numero))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número de habitación ya existe.");
        }

        if (ModelState.IsValid)
        {
            await repositorio.CrearAsync(habitacion);
            TempData["Exito"] = $"La habitación {habitacion.Numero} se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await repositorio.ObtenerPorIdAsync(id);
        if (habitacion is null)
        {
            return NotFound();
        }

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion)
        {
            return BadRequest();
        }

        QuitarNavegacionesDelModelState();

        if (!string.IsNullOrWhiteSpace(habitacion.Numero) &&
            await repositorio.NumeroExisteAsync(habitacion.Numero, habitacion.IdHabitacion))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número de habitación ya existe.");
        }

        if (ModelState.IsValid)
        {
            if (!await repositorio.ActualizarAsync(habitacion))
            {
                return NotFound();
            }

            TempData["Exito"] = $"La habitación {habitacion.Numero} se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        await CargarTiposAsync(habitacion.IdTipoHabitacion);
        return View(habitacion);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await repositorio.ObtenerPorIdAsync(id);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var habitacion = await repositorio.ObtenerPorIdAsync(id);
        if (habitacion is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Delete", habitacion);
        }

        if (!await repositorio.EliminarLogicoAsync(id))
        {
            return NotFound();
        }

        TempData["Exito"] = $"La habitación {habitacion.Numero} fue desactivada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync(int? seleccionado = null)
    {
        ViewBag.IdTipoHabitacion = new SelectList(
            await repositorio.ObtenerTiposActivosAsync(),
            nameof(TipoHabitacion.IdTipoHabitacion),
            nameof(TipoHabitacion.Nombre),
            seleccionado);
    }

    private void QuitarNavegacionesDelModelState()
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        ModelState.Remove(nameof(Habitacion.Reservas));
    }
}
