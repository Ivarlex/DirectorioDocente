using System.Security.Cryptography;
using System.Text;

namespace DirectorioDocente.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string nombreUsuario, string password)
        {
            var raw = $"{nombreUsuario.ToLower().Trim()}:{password}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(bytes).ToLower();
        }

        public static bool VerifyPassword(string nombreUsuario, string password, string storedHash)
        {
            var computed = HashPassword(nombreUsuario, password);
            return string.Equals(computed, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
