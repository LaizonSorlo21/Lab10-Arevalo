namespace Hotel.Web.ViewModels;

public class HabitacionDisponibleViewModel
{
    public int IdHabitacion { get; init; }
    public string Numero { get; init; } = string.Empty;
    public int Piso { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public int Capacidad { get; init; }
    public decimal PrecioNoche { get; init; }
    public int Noches { get; init; }
    public decimal TotalEstimado => PrecioNoche * Noches;
}
