using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public class FrmLogin : Form, ILoginView
    {
        private readonly TextBox _txtIdentificador = new() { Dock = DockStyle.Top, Font = Estilos.Fuente, MaxLength = 120 };
        private readonly TextBox _txtPassword = new() { Dock = DockStyle.Top, Font = Estilos.Fuente, UseSystemPasswordChar = true, MaxLength = 72 };
        private readonly Button _btnIngresar = Estilos.Boton("Iniciar sesión", Estilos.Primario, 320);
        private readonly Label _lblError = new()
        {
            Dock = DockStyle.Top,
            Height = 40,
            ForeColor = Estilos.Peligro,
            Font = Estilos.Fuente
        };

        public Usuario? UsuarioAutenticado { get; private set; }

        public event EventHandler? IniciarSesionSolicitado;

        public string Identificador => _txtIdentificador.Text;
        public string Password => _txtPassword.Text;

        public FrmLogin()
        {
            Text = "Inventario - Iniciar sesión";
            Icon = Imagenes.Icono;
            ClientSize = new Size(380, 480);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = Estilos.Fuente;

            var titulo = new Label
            {
                Text = "Inventario",
                Font = new Font("Segoe UI Semibold", 22F),
                ForeColor = Estilos.Menu,
                Dock = DockStyle.Top,
                Height = 46,
                TextAlign = ContentAlignment.MiddleCenter
            };
            var logo = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(0, 14, 0, 0),
                Image = Imagenes.Logo(128),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            var subtitulo = new Label
            {
                Text = "Ingrese con su usuario o correo",
                ForeColor = Estilos.Neutro,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var contenido = new Panel { Location = new Point(30, 205), Size = new Size(320, 255) };
            // Los controles Dock=Top se apilan en orden inverso al de inserción.
            contenido.Controls.Add(_btnIngresar);
            _btnIngresar.Dock = DockStyle.Top;
            _btnIngresar.Height = 40;
            contenido.Controls.Add(_lblError);
            contenido.Controls.Add(_txtPassword);
            contenido.Controls.Add(new Label { Text = "Contraseña", Dock = DockStyle.Top, Height = 24 });
            contenido.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 });
            contenido.Controls.Add(_txtIdentificador);
            contenido.Controls.Add(new Label { Text = "Usuario o correo", Dock = DockStyle.Top, Height = 24 });

            Controls.Add(contenido);
            Controls.Add(subtitulo);
            Controls.Add(titulo);
            Controls.Add(logo);

            AcceptButton = _btnIngresar;
            _btnIngresar.Click += (_, _) => IniciarSesionSolicitado?.Invoke(this, EventArgs.Empty);
        }

        public void MostrarError(string mensaje) => _lblError.Text = mensaje;

        public void LimpiarPassword()
        {
            _txtPassword.Clear();
            _txtPassword.Focus();
        }

        public void Ocupado(bool ocupado)
        {
            _btnIngresar.Enabled = !ocupado;
            Cursor = ocupado ? Cursors.WaitCursor : Cursors.Default;
            if (ocupado) _lblError.Text = string.Empty;
        }

        private void InitializeComponent()
        {

        }

        public void CompletarInicioSesion(Usuario usuario)
        {
            UsuarioAutenticado = usuario;
            DialogResult = DialogResult.OK;
        }
    }
}
