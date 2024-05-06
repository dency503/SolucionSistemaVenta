using System.Security.Cryptography;
using System.Text;
using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    public class UtilidadesService : IUtilidadesService
    {
        public string ConvertirSHA256(string texto)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding encoding = Encoding.UTF8;
                byte[] result = hash.ComputeHash(encoding.GetBytes(texto));
                foreach (byte b in result)
                {
                    stringBuilder.Append(b.ToString("x2"));

                }
                return stringBuilder.ToString();
            }


        }
        public string GenerarClave()
        {
            string clave = Guid.NewGuid().ToString("N").Substring(0, 6);
            return clave;
        }
    }
}
