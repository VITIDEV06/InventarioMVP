using System.ComponentModel;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    public class UcUsuarios : UserControl, IUsuariosView
    {
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtUsuario = new() { Dock = DockStyle.Fill, MaxLength = 50 };
        private readonly TextBox _txtCorreo = new() { Dock = DockStyle.Fill, MaxLength = 120 };
        private readonly TextBox _txtNombre = new() { Dock = DockStyle.Fill, MaxLength = 100 };
        private readonly TextBox _txtPassword = new() { Dock = DockStyle.Fill, MaxLength = 72, UseSystemPasswordChar = true };
        private readonly TextBox _txtConfirmar = new() { Dock = DockStyle.Fill, MaxLength = 72, UseSystemPasswordChar = true };

        public event EventHandler? CargarSolicitado;
        public event EventHandler? RegistrarSolicitado;

        public string NombreUsuario => _txtUsuario.Text;
        public string Correo => _txtCorreo.Text;
        public string NombreCompleto => _txtNombre.Text;
        public string Password => _txtPassword.Text;
        public string ConfirmarPassword => _txtConfirmar.Text;

        public UcUsuarios()
        {
            Dock = DockStyle.Fill;
            Font = Estilos.Fuente;
            BackColor = Estilos.Fondo;
            Padding = new Padding(16);

            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(Usuario.NombreUsuario), "Usuario", 80),
                Estilos.Columna(nameof(Usuario.Correo), "Correo", 130),
                Estilos.Columna(nameof(Usuario.NombreCompleto), "Nombre completo", 130),
                Estilos.Columna(nameof(Usuario.FechaCreacion), "Creado", 90, "g"));

            var tabla = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(12) };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            void Fila(string etiqueta, Control c)
            {
                tabla.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                tabla.Controls.Add(Estilos.Etiqueta(etiqueta));
                c.Margin = new Padding(0, 4, 0, 4);
                tabla.Controls.Add(c);
            }
            Fila("Usuario", _txtUsuario);
            Fila("Correo", _txtCorreo);
            Fila("Nombre", _txtNombre);
            Fila("Contraseña", _txtPassword);
            Fila("Confirmar", _txtConfirmar);

            var btnRegistrar = Estilos.Boton("Registrar usuario", Estilos.Primario, 180, "user-plus");
            var botones = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(12, 8, 0, 0) };
            botones.Controls.Add(btnRegistrar);

            var lateral = new Panel { Dock = DockStyle.Right, Width = 340, BackColor = Color.White };
            lateral.Controls.Add(botones);
            lateral.Controls.Add(tabla);
            lateral.Controls.Add(new Label
            {
                Text = "Nuevo usuario", Dock = DockStyle.Top, Height = 44,
                Padding = new Padding(12, 12, 0, 0), Font = new Font("Segoe UI Semibold", 11F)
            });

            var centro = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 12, 0) };
            centro.Controls.Add(_grid);

            Controls.Add(centro);
            Controls.Add(lateral);

            Load += (_, _) => CargarSolicitado?.Invoke(this, EventArgs.Empty);
            btnRegistrar.Click += (_, _) => RegistrarSolicitado?.Invoke(this, EventArgs.Empty);
        }

        public void MostrarUsuarios(IList<Usuario> usuarios) =>
            _grid.DataSource = new BindingList<Usuario>(usuarios.ToList());

        public void LimpiarFormulario()
        {
            _txtUsuario.Clear();
            _txtCorreo.Clear();
            _txtNombre.Clear();
            _txtPassword.Clear();
            _txtConfirmar.Clear();
        }

        public void MostrarError(string mensaje) =>
            MessageBox.Show(FindForm(), mensaje, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void MostrarInfo(string mensaje) =>
            MessageBox.Show(FindForm(), mensaje, "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
