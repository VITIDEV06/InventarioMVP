namespace InventarioMVP.Negocio.Seguridad
{
    /// <summary>Genera hashes BCrypt (con sal incluida y factor de trabajo 11).</summary>
    public class BCryptHashService : IHashService
    {
        private const int FactorTrabajo = 11;

        public string GenerarHash(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano))
                throw new ArgumentException("El texto a cifrar no puede estar vacío.", nameof(textoPlano));
            return BCrypt.Net.BCrypt.HashPassword(textoPlano, FactorTrabajo);
        }
    }
}
