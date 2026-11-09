namespace InventarioMVP.Entidades
{
    /// <summary>Usuario del sistema. PasswordHash contiene siempre un hash BCrypt, nunca texto plano.</summary>
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
