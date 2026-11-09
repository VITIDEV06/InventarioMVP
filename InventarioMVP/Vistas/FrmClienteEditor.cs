using InventarioMVP.Entidades;
using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Vistas
{
    /// <summary>Ventana modal para registrar o editar un cliente.</summary>
    public class FrmClienteEditor : DialogoBase
    {
        private readonly Cliente? _existente;
        private readonly Func<Cliente, string?> _guardar;
        private readonly TextBox _txtNombre = Estilos.Caja(120);
        private readonly TextBox _txtTelefono = Estilos.Caja(30);
        private readonly TextBox _txtCorreo = Estilos.Caja(120);
        private readonly TextBox _txtDireccion = Estilos.Caja(255);
        private readonly TextBox _txtObservaciones = new() { MaxLength = 500, Multiline = true, Height = 76, ScrollBars = ScrollBars.Vertical };

        public FrmClienteEditor(Cliente? existente, Func<Cliente, string?> guardar)
            : base(existente == null ? "Nuevo cliente" : "Editar cliente",
                   existente == null ? "Complete los datos del cliente" : "Registrado el " + Formato.Fecha(existente.FechaRegistro),
                   new Size(500, 490))
        {
            _existente = existente;
            _guardar = guardar;

            var tabla = Estilos.TablaFormulario(120);
            Estilos.Fila(tabla, "Nombre completo", _txtNombre);
            Estilos.Fila(tabla, "Teléfono", _txtTelefono);
            Estilos.Fila(tabla, "Correo", _txtCorreo);
            Estilos.Fila(tabla, "Dirección", _txtDireccion);
            Estilos.Fila(tabla, "Observaciones", _txtObservaciones);
            Cuerpo.Controls.Add(tabla);

            if (existente != null)
            {
                _txtNombre.Text = existente.NombreCompleto;
                _txtTelefono.Text = existente.Telefono;
                _txtCorreo.Text = existente.Correo;
                _txtDireccion.Text = existente.Direccion;
                _txtObservaciones.Text = existente.Observaciones;
            }
        }

        protected override string? Aceptar() => _guardar(new Cliente
        {
            Id = _existente?.Id ?? 0,
            NombreCompleto = _txtNombre.Text,
            Telefono = _txtTelefono.Text,
            Correo = _txtCorreo.Text,
            Direccion = _txtDireccion.Text,
            Observaciones = _txtObservaciones.Text
        });
    }
}
