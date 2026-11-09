using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Seguridad;

namespace InventarioMVP.Vistas
{
    /// <summary>Ventana modal (solo administrador) para crear o editar una cuenta, su rol y sus permisos.</summary>
    public class FrmUsuarioEditor : DialogoBase
    {
        private readonly Func<DatosUsuario, string?> _guardar;
        private readonly bool _esNuevo;
        private readonly TextBox _txtNombre = Estilos.Caja(100);
        private readonly TextBox _txtCorreo = Estilos.Caja(120);
        private readonly TextBox _txtUsuario = Estilos.Caja(50);
        private readonly CampoPassword _txtPassword = new();
        private readonly ComboBox _cmbRol = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Estilos.Fuente };
        private readonly CheckBox _chkActivo = new() { Text = "Cuenta activa", Checked = true, AutoSize = true, Font = Estilos.Fuente };
        private readonly CheckedListBox _lstPermisos = new() { CheckOnClick = true, Height = 76, BorderStyle = BorderStyle.FixedSingle, Font = Estilos.Fuente };
        private readonly IList<Permiso> _delegables;

        public FrmUsuarioEditor(Usuario? existente, IList<Permiso> delegables, IList<string> concedidos, Func<DatosUsuario, string?> guardar)
            : base(existente == null ? "Nuevo usuario" : "Editar usuario",
                   existente == null ? "Cree una cuenta y asigne su rol" : "Cuenta: " + existente.NombreUsuario,
                   new Size(540, existente == null ? 600 : 560))
        {
            _guardar = guardar;
            _esNuevo = existente == null;
            _delegables = delegables;

            _cmbRol.Items.Add("Usuario");
            _cmbRol.Items.Add("Administrador");
            _cmbRol.SelectedIndex = existente?.EsAdministrador == true ? 1 : 0;
            foreach (var p in delegables) _lstPermisos.Items.Add(p.Descripcion, concedidos.Contains(p.Codigo));

            var tabla = Estilos.TablaFormulario(160);
            Estilos.Fila(tabla, "Nombre completo", _txtNombre);
            Estilos.Fila(tabla, "Correo", _txtCorreo);
            Estilos.Fila(tabla, "Nombre de usuario", _txtUsuario);
            if (_esNuevo) Estilos.Fila(tabla, "Contraseña", _txtPassword);
            Estilos.Fila(tabla, "Rol", _cmbRol);
            if (!_esNuevo) Estilos.Fila(tabla, "Estado", _chkActivo);
            Estilos.Fila(tabla, "Permisos adicionales", _lstPermisos);
            Cuerpo.Controls.Add(new Label
            {
                Text = "Los administradores tienen acceso completo. Un usuario puede recibir permisos adicionales; " +
                       "por defecto solo consulta información y sus propias operaciones.",
                Dock = DockStyle.Top, Height = 56, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 10, 0, 0)
            });
            Cuerpo.Controls.SetChildIndex(Cuerpo.Controls[^1], 0);
            Cuerpo.Controls.Add(tabla);
            Cuerpo.Controls.SetChildIndex(tabla, 1);

            if (existente != null)
            {
                _txtNombre.Text = existente.NombreCompleto;
                _txtCorreo.Text = existente.Correo;
                _txtUsuario.Text = existente.NombreUsuario;
                _txtUsuario.Enabled = false;
                _chkActivo.Checked = existente.Activo;
            }
            _cmbRol.SelectedIndexChanged += (_, _) => _lstPermisos.Enabled = _cmbRol.SelectedIndex == 0;
            _lstPermisos.Enabled = _cmbRol.SelectedIndex == 0;
        }

        protected override string? Aceptar()
        {
            var permisos = Enumerable.Range(0, _delegables.Count)
                .Where(i => _lstPermisos.GetItemChecked(i)).Select(i => _delegables[i].Codigo).ToList();
            return _guardar(new DatosUsuario(_txtNombre.Text, _txtCorreo.Text, _txtUsuario.Text, _txtPassword.Valor,
                _cmbRol.SelectedIndex == 1 ? Roles.Admin : Roles.Usuario, _chkActivo.Checked, permisos));
        }
    }

    /// <summary>Ventana modal para que el administrador establezca una nueva contraseña a otro usuario.</summary>
    public class FrmRestablecerPassword : DialogoBase
    {
        private readonly Func<string, string?> _guardar;
        private readonly CampoPassword _txtNueva = new();
        private readonly CampoPassword _txtConfirmar = new();

        public FrmRestablecerPassword(Usuario usuario, Func<string, string?> guardar)
            : base("Restablecer contraseña", "Cuenta: " + usuario.NombreUsuario, new Size(480, 330))
        {
            _guardar = guardar;
            BtnAceptar.Text = "Restablecer";
            BtnAceptar.Image = Iconos.Obtener("key-round", Iconos.Blanco);
            var tabla = Estilos.TablaFormulario(150);
            Estilos.Fila(tabla, "Nueva contraseña", _txtNueva);
            Estilos.Fila(tabla, "Confirmar contraseña", _txtConfirmar);
            Cuerpo.Controls.Add(new Label
            {
                Text = "Mínimo 8 caracteres, con mayúsculas, minúsculas y números.",
                Dock = DockStyle.Top, Height = 34, ForeColor = Estilos.Neutro, Font = new Font("Segoe UI", 9F), Padding = new Padding(0, 10, 0, 0)
            });
            Cuerpo.Controls.SetChildIndex(Cuerpo.Controls[^1], 0);
            Cuerpo.Controls.Add(tabla);
            Cuerpo.Controls.SetChildIndex(tabla, 1);
        }

        protected override string? Aceptar() =>
            _txtNueva.Valor != _txtConfirmar.Valor ? "Las contraseñas no coinciden." : _guardar(_txtNueva.Valor);
    }
}
