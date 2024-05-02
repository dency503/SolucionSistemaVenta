using Microsoft.AspNetCore.Mvc;

namespace SistemaVentas.AplicacionWeb.Controllers
{
    public class ReporteController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
