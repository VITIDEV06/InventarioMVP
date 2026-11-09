using System.Globalization;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Ventana principal: menú lateral izquierdo, hora actual y usuario autenticado en la parte inferior.</summary>
    public class FrmDashboard : Form
    {
        private readonly Usuario _usuario;
        private readonly Func<UserControl> _crearInventario;
        private readonly Func<UserControl> _crearUsuarios;
        private readonly Panel _contenido = new() { Dock = DockStyle.Fill, BackColor = Estilos.Fondo };
        private readonly Label _lblTitulo = new() { Dock = DockStyle.Left, AutoSize = false, Width = 400, TextAlign = ContentAlignment.MiddleLeft, Font = Estilos.FuenteTitulo, ForeColor = Estilos.Menu };
        private readonly Label _lblReloj = new() { Dock = DockStyle.Right, AutoSize = false, Width = 270, TextAlign = ContentAlignment.MiddleRight, Font = Estilos.Fuente, ForeColor = Estilos.Neutro };
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
        private readonly List<Button> _botonesMenu = new();

        /// <summary>True si el usuario eligió "Cerrar sesión" (volver al login) y no cerrar la aplicación.</summary>
        public bool CerrarSesionSolicitado { get; private set; }

        public FrmDashboard(Usuario usuario, Func<UserControl> crearInventario, Func<UserControl> crearUsuarios)
        {
            _usuario = usuario;
            _crearInventario = crearInventario;
            _crearUsuarios = crearUsuarios;

            Text = "Inventario MVP";
            Icon = Imagenes.Icono;
            ClientSize = new Size(1180, 700);
            MinimumSize = new Size(1000, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Estilos.Fuente;

            // --- Menú lateral ---
            var menu = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Estilos.Menu };
            var marca = new Panel { Dock = DockStyle.Top, Height = 80 };
            marca.Controls.Add(new Label
            {
                Text = "INVENTARIO", Dock = DockStyle.Fill, ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 14F), TextAlign = ContentAlignment.MiddleLeft
            });
            marca.Controls.Add(new PictureBox
            {
                Dock = DockStyle.Left, Width = 72, SizeMode = PictureBoxSizeMode.CenterImage,
                Image = Imagenes.Logo(48)
            });
            var btnInicio = BotonMenu("Inicio", "layout-dashboard", () => Mostrar("Inicio", new UcInicio(_usuario)));
            var btnInventario = BotonMenu("Inventario", "package", () => Mostrar("Inventario", _crearInventario()));
            var btnUsuarios = BotonMenu("Usuarios", "users", () => Mostrar("Usuarios", _crearUsuarios()));
            var btnSalir = BotonMenu("Cerrar sesión", "log-out", CerrarSesion, esNavegacion: false);

            // Sesión del usuario en la parte inferior del menú
            var sesion = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(14, 10, 8, 8) };
            sesion.Controls.Add(new Label
            {
                Text = _usuario.NombreCompleto + Environment.NewLine + _usuario.Correo,
                Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI", 9F)
            });
            sesion.Controls.Add(new Label
            {
                Text = "Sesión iniciada como:", Dock = DockStyle.Top, Height = 18,
                ForeColor = Color.FromArgb(148, 163, 184), Font = new Font("Segoe UI", 8F)
            });

            // Orden inverso de apilado (Dock = Top): el último agregado queda arriba.
            menu.Controls.Add(sesion);
            menu.Controls.Add(btnSalir);
            menu.Controls.Add(btnUsuarios);
            menu.Controls.Add(btnInventario);
            menu.Controls.Add(btnInicio);
            menu.Controls.Add(marca);

            // --- Barra superior con título y reloj ---
            var superior = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White, Padding = new Padding(16, 0, 16, 0) };
            // Con Dock = Right, el último control agregado queda más pegado al borde derecho:
            // [título ........ reloj-icono reloj-texto | perfil]
            var icoReloj = new PictureBox
            {
                Dock = DockStyle.Right, Width = 30, SizeMode = PictureBoxSizeMode.CenterImage,
                Image = Iconos.Obtener("clock", Iconos.Sutil)
            };
            var separador = new Panel { Dock = DockStyle.Right, Width = 24 };
            var perfil = new PerfilUsuarioControl(_usuario.NombreCompleto) { Dock = DockStyle.Right };
            superior.Controls.Add(_lblTitulo);
            superior.Controls.Add(icoReloj);
            superior.Controls.Add(_lblReloj);
            superior.Controls.Add(separador);
            superior.Controls.Add(perfil);

            Controls.Add(_contenido);
            Controls.Add(superior);
            Controls.Add(menu);

            ActualizarReloj();
            _timer.Tick += (_, _) => ActualizarReloj();
            _timer.Start();

            Mostrar("Inicio", new UcInicio(_usuario));
            Resaltar(btnInicio);
        }

        private Button BotonMenu(string texto, string icono, Action alHacerClic, bool esNavegacion = true)
        {
            var b = new Button
            {
                Text = " " + texto, Dock = DockStyle.Top, Height = 48, Tag = icono,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Estilos.Menu,
                Image = Iconos.Obtener(icono, Iconos.Normal),
                ImageAlign = ContentAlignment.MiddleLeft, TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(20, 0, 0, 0),
                Font = new Font("Segoe UI", 11F), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Estilos.MenuHover;
            b.Click += (_, _) =>
            {
                if (esNavegacion) Resaltar(b);
                alHacerClic();
            };
            if (esNavegacion) _botonesMenu.Add(b);
            return b;
        }

        private void Resaltar(Button activo)
        {
            foreach (var b in _botonesMenu)
            {
                bool esActivo = b == activo;
                b.BackColor = esActivo ? Estilos.MenuActivo : Estilos.Menu;
                b.Image = Iconos.Obtener((string)b.Tag!, esActivo ? Iconos.Blanco : Iconos.Normal);
            }
        }

        private void Mostrar(string titulo, Control vista)
        {
            _lblTitulo.Text = titulo;
            foreach (Control c in _contenido.Controls) c.Dispose();
            _contenido.Controls.Clear();
            vista.Dock = DockStyle.Fill;
            _contenido.Controls.Add(vista);
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show(this, "¿Desea cerrar la sesión?", "Cerrar sesión",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            CerrarSesionSolicitado = true;
            Close();
        }

        private void ActualizarReloj() =>
            _lblReloj.Text = DateTime.Now.ToString("dddd d 'de' MMMM yyyy  HH:mm:ss", new CultureInfo("es-PA"));

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
