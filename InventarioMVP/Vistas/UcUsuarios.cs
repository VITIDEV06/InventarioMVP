using System.ComponentModel;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Administración de usuarios (exclusiva del administrador).</summary>
    public class UcUsuarios : VistaBase, IUsuariosView
    {
        private readonly DataGridView _grid = new();

        public event EventHandler? CargarSolicitado;
        public event EventHandler? NuevoSolicitado;
        public event EventHandler? EditarSolicitado;
        public event EventHandler? EstadoSolicitado;
        public event EventHandler? PasswordSolicitada;

        public Usuario? UsuarioSeleccionado => _grid.CurrentRow?.DataBoundItem as Usuario;

        public UcUsuarios()
        {
            Estilos.ConfigurarGrid(_grid);
            _grid.Columns.AddRange(
                Estilos.Columna(nameof(Usuario.NombreUsuario), "Usuario", 70),
                Estilos.Columna(nameof(Usuario.NombreCompleto), "Nombre completo", 110),
                Estilos.Columna(nameof(Usuario.Correo), "Correo", 110),
                Estilos.Columna(nameof(Usuario.RolTexto), "Rol", 60),
                Estilos.Columna(nameof(Usuario.EstadoTexto), "Estado", 55),
                Estilos.Columna(nameof(Usuario.UltimoAcceso), "Último acceso", 80, "dd/MM/yyyy HH:mm"),
                Estilos.Columna(nameof(Usuario.FechaCreacion), "Creado", 65, "dd/MM/yyyy"));

            var btnNuevo = Estilos.Boton("Nuevo usuario", Estilos.Primario, 150, "user-plus");
            var btnEditar = Estilos.Boton("Editar", Estilos.Neutro, 100, "pencil");
            var btnEstado = Estilos.Boton("Activar / Desactivar", Estilos.Aviso, 190, "power");
            var btnPassword = Estilos.Boton("Contraseña", Estilos.Neutro, 130, "key-round");
            var barra = Estilos.Barra(btnNuevo, btnEditar, btnEstado, btnPassword);

            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(1) };
            tarjeta.Controls.Add(_grid);
            Controls.Add(tarjeta);
            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 8 });
            Controls.Add(barra);
            Controls.Add(Encabezado("Usuarios", "Cree cuentas, asigne roles y permisos, y active o desactive usuarios"));

            Load += (_, _) => Disparar(CargarSolicitado);
            btnNuevo.Click += (_, _) => Disparar(NuevoSolicitado);
            btnEditar.Click += (_, _) => Disparar(EditarSolicitado);
            btnEstado.Click += (_, _) => Disparar(EstadoSolicitado);
            btnPassword.Click += (_, _) => Disparar(PasswordSolicitada);
            _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Disparar(EditarSolicitado); };
            _grid.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0 || _grid.Rows[e.RowIndex].DataBoundItem is not Usuario u) return;
                string col = _grid.Columns[e.ColumnIndex].DataPropertyName;
                if (col == nameof(Usuario.EstadoTexto))
                {
                    e.CellStyle!.ForeColor = u.Activo ? Estilos.Exito : Estilos.Peligro;
                    e.CellStyle.Font = Estilos.FuenteNegrita;
                }
                else if (col == nameof(Usuario.RolTexto) && u.EsAdministrador)
                {
                    e.CellStyle!.ForeColor = Estilos.Primario;
                    e.CellStyle.Font = Estilos.FuenteNegrita;
                }
            };
        }

        public void MostrarUsuarios(IList<Usuario> usuarios)
        {
            int? id = UsuarioSeleccionado?.Id;
            _grid.DataSource = new BindingList<Usuario>(usuarios.ToList());
            if (id == null) return;
            foreach (DataGridViewRow fila in _grid.Rows)
                if (fila.DataBoundItem is Usuario u && u.Id == id) { fila.Selected = true; _grid.CurrentCell = fila.Cells[0]; break; }
        }

        public bool EditarUsuario(Usuario? existente, IList<Permiso> delegables, IList<string> concedidos, Func<DatosUsuario, string?> guardar)
        {
            using var dialogo = new FrmUsuarioEditor(existente, delegables, concedidos, guardar);
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK;
        }

        public bool PedirNuevaPassword(Usuario usuario, Func<string, string?> guardar)
        {
            using var dialogo = new FrmRestablecerPassword(usuario, guardar);
            return dialogo.ShowDialog(FindForm()) == DialogResult.OK;
        }
    }
}
