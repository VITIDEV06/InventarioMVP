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
        public int RolId { get; set; } = 2;
        public string Rol { get; set; } = "USUARIO";
        public bool Activo { get; set; } = true;
        public DateTime? UltimoAcceso { get; set; }

        public bool EsAdministrador => RolId == 1;
        public string EstadoTexto => Activo ? "Activo" : "Desactivado";
        public string RolTexto => EsAdministrador ? "Administrador" : "Usuario";
    }

    /// <summary>Permiso que el administrador puede conceder a un usuario individual.</summary>
    public class Permiso
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
