namespace InventarioMVP.Vistas
{
    /// <summary>Pantalla pública de creación de cuenta. Siempre crea cuentas de tipo Usuario.</summary>
    public class FrmRegistro : DialogoBase, IRegistroView
    {
        private readonly TextBox _txtNombre = Estilos.Caja(100);
        private readonly TextBox _txtCorreo = Estilos.Caja(120);
        private readonly TextBox _txtUsuario = Estilos.Caja(50);
        private readonly CampoPassword _txtPassword = new();
        private readonly CampoPassword _txtConfirmar = new();
        private string? _error;

        public event EventHandler? RegistrarSolicitado;

        public string NombreCompleto => _txtNombre.Text;
        public string Correo => _txtCorreo.Text;
        public string NombreUsuario => _txtUsuario.Text;
        public string Password => _txtPassword.Valor;
        public string Confirmacion => _txtConfirmar.Valor;

        public FrmRegistro() : base("Crear cuenta", "Regístrese para acceder al sistema", new Size(460, 560))
        {
            BtnAceptar.Text = "Crear cuenta";
            BtnAceptar.Image = Iconos.Obtener("user-plus", Iconos.Blanco);

            var tabla = Estilos.TablaFormulario(150);
            Estilos.Fila(tabla, "Nombre completo", _txtNombre);
            Estilos.Fila(tabla, "Correo electrónico", _txtCorreo);
            Estilos.Fila(tabla, "Nombre de usuario", _txtUsuario);
            Estilos.Fila(tabla, "Contraseña", _txtPassword);
            Estilos.Fila(tabla, "Confirmar contraseña", _txtConfirmar);
            Cuerpo.Controls.Add(new Label
            {
                Text = "La contraseña debe tener al menos 8 caracteres, con mayúsculas, minúsculas y números. " +
                       "El usuario admite letras, números, punto, guion y guion bajo.",
                Dock = DockStyle.Top, Height = 64, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 8, 0, 0)
            });
            Cuerpo.Controls.Add(tabla);
        }

        protected override string? Aceptar()
        {
            _error = null;
            RegistrarSolicitado?.Invoke(this, EventArgs.Empty);
            return _error;
        }

        public void MostrarError(string mensaje) => _error = mensaje;
    }
}
