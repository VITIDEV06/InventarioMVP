using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Configuración: datos personales y contraseña (todos los usuarios) y parámetros del sistema (solo administrador).</summary>
    public class UcConfiguracion : VistaBase, IConfiguracionView
    {
        private readonly TextBox _txtNombre = Estilos.Caja(100);
        private readonly TextBox _txtCorreo = Estilos.Caja(120);
        private readonly Label _lblInfo = new() { Dock = DockStyle.Top, Height = 64, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 6, 0, 0) };
        private readonly CampoPassword _txtActual = new();
        private readonly CampoPassword _txtNueva = new();
        private readonly CampoPassword _txtConfirmar = new();
        private readonly NumericUpDown _numStockMinimo = new() { Maximum = 100000, Width = 120 };
        private readonly Panel _panelAdmin;

        public event EventHandler? CargarSolicitado;
        public event EventHandler? PerfilGuardado;
        public event EventHandler? PasswordCambiada;
        public event EventHandler? ParametrosGuardados;

        public string NombreCompleto => _txtNombre.Text;
        public string Correo => _txtCorreo.Text;
        public string PasswordActual => _txtActual.Valor;
        public string PasswordNueva => _txtNueva.Valor;
        public string PasswordConfirmacion => _txtConfirmar.Valor;
        public int StockMinimo => (int)_numStockMinimo.Value;

        public UcConfiguracion()
        {
            // --- Mi cuenta ---
            var tablaPerfil = Estilos.TablaFormulario(140);
            Estilos.Fila(tablaPerfil, "Nombre completo", _txtNombre);
            Estilos.Fila(tablaPerfil, "Correo", _txtCorreo);
            var btnPerfil = Estilos.Boton("Guardar cambios", Estilos.Primario, 170, "save");
            var contenidoPerfil = new Panel { Padding = new Padding(0, 4, 0, 0) };
            contenidoPerfil.Controls.Add(Pie(btnPerfil));
            contenidoPerfil.Controls.Add(_lblInfo);
            contenidoPerfil.Controls.Add(tablaPerfil);

            // --- Cambiar contraseña ---
            var tablaPass = Estilos.TablaFormulario(150);
            Estilos.Fila(tablaPass, "Contraseña actual", _txtActual);
            Estilos.Fila(tablaPass, "Nueva contraseña", _txtNueva);
            Estilos.Fila(tablaPass, "Confirmar contraseña", _txtConfirmar);
            var btnPass = Estilos.Boton("Cambiar contraseña", Estilos.Primario, 200, "key-round");
            var contenidoPass = new Panel { Padding = new Padding(0, 4, 0, 0) };
            contenidoPass.Controls.Add(Pie(btnPass));
            contenidoPass.Controls.Add(new Label
            {
                Text = "Mínimo 8 caracteres, con mayúsculas, minúsculas y números.", Dock = DockStyle.Top, Height = 34,
                ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 8, 0, 0)
            });
            contenidoPass.Controls.Add(tablaPass);

            var superior = new TableLayoutPanel { Dock = DockStyle.Top, Height = 290, ColumnCount = 2 };
            superior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            superior.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            superior.Controls.Add(Estilos.Titulado("Mi cuenta", contenidoPerfil), 0, 0);
            superior.Controls.Add(Estilos.Titulado("Cambiar contraseña", contenidoPass), 1, 0);

            // --- Parámetros administrativos ---
            var tablaAdmin = Estilos.TablaFormulario(220);
            Estilos.Fila(tablaAdmin, "Stock mínimo por defecto", _numStockMinimo);
            _numStockMinimo.Dock = DockStyle.None;
            var btnAdmin = Estilos.Boton("Guardar parámetros", Estilos.Primario, 190, "save");
            var contenidoAdmin = new Panel { Padding = new Padding(0, 4, 0, 0) };
            contenidoAdmin.Controls.Add(Pie(btnAdmin));
            contenidoAdmin.Controls.Add(new Label
            {
                Text = "Valor sugerido como stock mínimo al registrar un producto nuevo (se usa para detectar el stock bajo).",
                Dock = DockStyle.Top, Height = 34, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 8, 0, 0)
            });
            contenidoAdmin.Controls.Add(tablaAdmin);
            _panelAdmin = new Panel { Dock = DockStyle.Fill, Visible = false };
            _panelAdmin.Controls.Add(Estilos.Titulado("Parámetros del sistema (solo administrador)", contenidoAdmin));

            Controls.Add(_panelAdmin);
            Controls.Add(superior);
            Controls.Add(Encabezado("Configuración", "Administre su cuenta y las preferencias del sistema"));

            Load += (_, _) => Disparar(CargarSolicitado);
            btnPerfil.Click += (_, _) => Disparar(PerfilGuardado);
            btnPass.Click += (_, _) => Disparar(PasswordCambiada);
            btnAdmin.Click += (_, _) => Disparar(ParametrosGuardados);
        }

        private static Panel Pie(Button boton)
        {
            var p = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(0, 8, 0, 0) };
            p.Controls.Add(boton);
            return p;
        }

        public void MostrarPerfil(Usuario u)
        {
            _txtNombre.Text = u.NombreCompleto;
            _txtCorreo.Text = u.Correo;
            _lblInfo.Text = $"Usuario: {u.NombreUsuario}   ·   Rol: {u.RolTexto}\n" +
                            $"Miembro desde: {Formato.Fecha(u.FechaCreacion)}   ·   Último acceso: " +
                            (u.UltimoAcceso == null ? "—" : Formato.FechaHora(u.UltimoAcceso.Value));
        }

        public void MostrarParametros(int stockMinimoDefecto, bool esAdministrador)
        {
            _numStockMinimo.Value = Math.Min(stockMinimoDefecto, _numStockMinimo.Maximum);
            _panelAdmin.Visible = esAdministrador;
        }

        public void LimpiarPasswords()
        {
            _txtActual.Limpiar();
            _txtNueva.Limpiar();
            _txtConfirmar.Limpiar();
        }
    }
}
