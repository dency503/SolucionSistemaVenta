namespace SistemaVentas.AplicacionWeb.Models.ViewModels
{
    public class VMDashBoard
    {
        public int TotalVentas { get; set; }
        public string? TotalIngreso { get; set; }
        public int TotalProductos { get; set; }
        public string? TotalCategorias { get; set; }
        public List<VMVentasSemana> VentasUltimaSemana { get; set; }
        public List<VMVentasSemana> ProductosTopUltimaSemana { get; set; }

    }
}
