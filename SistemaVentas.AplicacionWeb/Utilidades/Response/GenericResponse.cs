namespace SistemaVentas.AplicacionWeb.Utilidades.Response
{
    public class GenericResponse<TObject>
    {
        /// <summary>
        /// Indica el estado de la respuesta (true para éxito, false para error).
        /// </summary>
        public bool Estado { get; set; }

        /// <summary>
        /// Mensaje opcional que proporciona detalles sobre el estado de la respuesta.
        /// </summary>
        public string? Mensaje { get; set; }

        /// <summary>
        /// Objeto individual que se devuelve en la respuesta.
        /// </summary>
        public TObject? Objeto { get; set; }

        /// <summary>
        /// Lista de objetos que se devuelven en la respuesta.
        /// </summary>
        public List<TObject>? ListaObjeto { get; set; }
    }
}
