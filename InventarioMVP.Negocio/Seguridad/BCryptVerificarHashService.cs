namespace InventarioMVP.Negocio.Seguridad
{
    /// <summary>Verifica contraseñas contra hashes BCrypt.</summary>
    public class BCryptVerificarHashService : IVerificarHashService
    {
        public bool Verificar(string textoPlano, string hashAlmacenado)
        {
            if (string.IsNullOrEmpty(textoPlano) || string.IsNullOrEmpty(hashAlmacenado))
                return false;
            try
            {
                return BCrypt.Net.BCrypt.Verify(textoPlano, hashAlmacenado);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false; // hash almacenado con formato inválido
            }
        }
    }
}
