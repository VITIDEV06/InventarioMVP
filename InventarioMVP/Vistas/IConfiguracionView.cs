using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public interface IConfiguracionView
    {
        event EventHandler CargarSolicitado;
        event EventHandler PerfilGuardado;
        event EventHandler PasswordCambiada;
        event EventHandler ParametrosGuardados;

        string NombreCompleto { get; }
        string Correo { get; }
        string PasswordActual { get; }
        string PasswordNueva { get; }
        string PasswordConfirmacion { get; }
        int StockMinimo { get; }

        void MostrarPerfil(Usuario usuario);
        void MostrarParametros(int stockMinimoDefecto, bool esAdministrador);
        void LimpiarPasswords();
        void MostrarError(string mensaje);
        void MostrarInfo(string mensaje);
    }
}
