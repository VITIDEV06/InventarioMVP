using System.Globalization;
using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Vistas
{
    /// <summary>Opción del menú lateral. La lista ya viene filtrada según los permisos del usuario.</summary>
    public record ItemMenu(string Titulo, string Icono, Func<Control> Crear);

    /// <summary>Ventana principal: menú lateral dinámico, hora actual y usuario autenticado.</summary>
    public class FrmDashboard : Form
    {
        private readonly Usuario _usuario;
        private readonly Panel _contenido = new() { Dock = DockStyle.Fill, BackColor = Estilos.Fondo };
        private readonly Label _lblTitulo = new() { Dock = DockStyle.Left, AutoSize = false, Width = 400, TextAlign = ContentAlignment.MiddleLeft, Font = Estilos.FuenteTitulo, ForeColor = Estilos.Menu };
        private readonly Label _lblReloj = new() { Dock = DockStyle.Right, AutoSize = false, Width = 270, TextAlign = ContentAlignment.MiddleRight, Font = Estilos.Fuente, ForeColor = Estilos.Neutro };
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
        private readonly List<(Button Boton, ItemMenu Item)> _botonesMenu = new();

        /// <summary>True si el usuario eligió "Cerrar sesión" (volver al login) y no cerrar la aplicación.</summary>
        public bool CerrarSesionSolicitado { get; private set; }

        public FrmDashboard(Usuario usuario, IList<ItemMenu> items)
        {
            _usuario = usuario;

            Text = "Inventario MVP";
            Icon = Imagenes.Icono;
            ClientSize = new Size(1200, 680);
            MinimumSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Estilos.Fuente;

            // --- Menú lateral ---
            var menu = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = Estilos.Menu };
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

            var btnSalir = BotonMenu("Cerrar sesión", "log-out", CerrarSesion);

            // Sesión del usuario en la parte inferior del menú
            var sesion = new Panel { Dock = DockStyle.Bottom, Height = 84, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(14, 10, 8, 8) };
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
            foreach (var item in items.Reverse())
            {
                var boton = BotonMenu(item.Titulo, item.Icono, () => { });
                boton.Click += (_, _) => Mostrar(item, boton);
                _botonesMenu.Add((boton, item));
                menu.Controls.Add(boton);
            }
            menu.Controls.Add(marca);

            // --- Barra superior: título | reloj | perfil ---
            var superior = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(16, 0, 16, 0) };
            var icoReloj = new PictureBox
            {
                Dock = DockStyle.Right, Width = 30, SizeMode = PictureBoxSizeMode.CenterImage,
                Image = Iconos.Obtener("clock", Iconos.Sutil)
            };
            var separador = new Panel { Dock = DockStyle.Right, Width = 24 };
            var perfil = new PerfilUsuarioControl(_usuario.NombreCompleto, Roles.Nombre(_usuario.RolId)) { Dock = DockStyle.Right };
            // Con Dock = Right, el último control agregado queda más pegado al borde derecho.
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

            if (items.Count > 0)
            {
                var primero = _botonesMenu.First(b => b.Item == items[0]);
                Mostrar(primero.Item, primero.Boton);
            }
        }

        private Button BotonMenu(string texto, string icono, Action alHacerClic)
        {
            var b = new Button
            {
                Text = " " + texto, Dock = DockStyle.Top, Height = 46, Tag = icono,
                FlatStyle = FlatStyle.Flat, ForeColor = Color.White, BackColor = Estilos.Menu,
                Image = Iconos.Obtener(icono, Iconos.Normal),
                ImageAlign = ContentAlignment.MiddleLeft, TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(20, 0, 0, 0),
                Font = new Font("Segoe UI", 11F), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Estilos.MenuHover;
            b.Click += (_, _) => alHacerClic();
            return b;
        }

        private void Mostrar(ItemMenu item, Button activo)
        {
            foreach (var (boton, _) in _botonesMenu)
            {
                bool esActivo = boton == activo;
                boton.BackColor = esActivo ? Estilos.MenuActivo : Estilos.Menu;
                boton.Image = Iconos.Obtener((string)boton.Tag!, esActivo ? Iconos.Blanco : Iconos.Normal);
            }

            _lblTitulo.Text = item.Titulo;
            foreach (Control c in _contenido.Controls) c.Dispose();
            _contenido.Controls.Clear();
            var vista = item.Crear();
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
