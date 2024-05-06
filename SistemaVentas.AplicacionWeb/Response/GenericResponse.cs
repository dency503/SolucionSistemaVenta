namespace SistemaVentas.AplicacionWeb.Response
{
    public class GenericResponse<TOBject>
    {
        public bool Estado { get; set; }
        public string? Mensaje { get; set; }
        public TOBject? Objecto { get; set; }
        public List<TOBject>? ListaObjecto { get; set; }
    }
}
