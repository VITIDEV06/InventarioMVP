using System.Net.Mail;
using System.Text.RegularExpressions;

namespace InventarioMVP.Negocio.Servicios
{
    /// <summary>Validaciones compartidas por los servicios de Negocio.</summary>
    public static class Validaciones
    {
        private static readonly Regex PatronTelefono = new(@"^[0-9+\-\s()]{7,20}$", RegexOptions.Compiled);

        public static string Texto(string? valor, string campo, int maximo, bool obligatorio = true)
        {
            valor = (valor ?? string.Empty).Trim();
            if (obligatorio && valor.Length == 0) throw new ValidacionException($"{campo} es obligatorio.");
            if (valor.Length > maximo) throw new ValidacionException($"{campo} no puede superar {maximo} caracteres.");
            return valor;
        }

        public static string Correo(string? correo, bool obligatorio = true)
        {
            correo = (correo ?? string.Empty).Trim();
            if (correo.Length == 0)
            {
                if (obligatorio) throw new ValidacionException("El correo electrónico es obligatorio.");
                return correo;
            }
            if (correo.Length > 120) throw new ValidacionException("El correo no puede superar 120 caracteres.");
            try
            {
                var m = new MailAddress(correo);
                if (m.Address == correo && m.Host.Contains('.')) return correo;
            }
            catch (FormatException) { }
            throw new ValidacionException("El correo electrónico no es válido.");
        }

        public static string Telefono(string? telefono)
        {
            telefono = (telefono ?? string.Empty).Trim();
            if (telefono.Length > 0 && !PatronTelefono.IsMatch(telefono))
                throw new ValidacionException("El teléfono no es válido (use números, +, -, espacios o paréntesis; 7 a 20 caracteres).");
            return telefono;
        }

        public static string NombreUsuario(string? usuario)
        {
            usuario = (usuario ?? string.Empty).Trim();
            if (usuario.Length < 3 || usuario.Length > 50)
                throw new ValidacionException("El usuario debe tener entre 3 y 50 caracteres.");
            if (usuario.Any(c => !(char.IsLetterOrDigit(c) || c is '.' or '_' or '-')))
                throw new ValidacionException("El usuario solo puede contener letras, números, punto, guion y guion bajo.");
            return usuario;
        }

        /// <summary>Contraseña segura: 8+ caracteres con mayúscula, minúscula y número.</summary>
        public static void Password(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                throw new ValidacionException("La contraseña debe tener al menos 8 caracteres.");
            if (password.Length > 72)
                throw new ValidacionException("La contraseña no puede superar 72 caracteres.");
            if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
                throw new ValidacionException("La contraseña debe incluir mayúsculas, minúsculas y números.");
        }

        public static void Fechas(DateTime desde, DateTime hasta)
        {
            if (desde.Date > hasta.Date) throw new ValidacionException("La fecha inicial no puede ser posterior a la final.");
        }
    }
}
