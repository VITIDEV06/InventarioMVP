namespace InventarioMVP.Vistas
{
    /// <summary>
    /// Ventana modal para crear/editar registros. Las subclases agregan sus campos a <see cref="Cuerpo"/>
    /// y sobrescriben <see cref="Aceptar"/>, que devuelve null si todo salió bien o el mensaje de error
    /// (que se muestra dentro del diálogo sin cerrarlo).
    /// </summary>
    public abstract class DialogoBase : Form
    {
        protected readonly Panel Cuerpo = new() { Dock = DockStyle.Fill, Padding = new Padding(24, 12, 24, 4) };
        private readonly Label _lblError = new() { Dock = DockStyle.Bottom, Height = 34, ForeColor = Estilos.Peligro, Font = Estilos.Fuente, Padding = new Padding(24, 0, 24, 0), TextAlign = ContentAlignment.MiddleLeft };
        protected readonly Button BtnAceptar = Estilos.Boton("Guardar", Estilos.Primario, 120, "save");
        protected readonly Button BtnCancelar = Estilos.Boton("Cancelar", Estilos.Neutro, 110);

        protected DialogoBase(string titulo, string subtitulo, Size tamanoCliente)
        {
            Text = titulo;
            Icon = Imagenes.Icono;
            ClientSize = tamanoCliente;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Font = Estilos.Fuente;

            var encabezado = new Panel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(24, 10, 24, 0), BackColor = Estilos.Fondo };
            encabezado.Controls.Add(new Label { Text = titulo, Dock = DockStyle.Fill, Font = new Font("Segoe UI Semibold", 14F), ForeColor = Estilos.Menu, TextAlign = ContentAlignment.BottomLeft });
            encabezado.Controls.Add(new Label { Text = subtitulo, Dock = DockStyle.Bottom, Height = 22, ForeColor = Estilos.Neutro });

            var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(16, 12, 16, 0) };
            pie.Controls.Add(BtnCancelar);
            pie.Controls.Add(BtnAceptar);

            Controls.Add(Cuerpo);
            Controls.Add(_lblError);
            Controls.Add(pie);
            Controls.Add(encabezado);

            AcceptButton = BtnAceptar;
            CancelButton = BtnCancelar;
            BtnCancelar.DialogResult = DialogResult.Cancel;
            BtnAceptar.Click += (_, _) =>
            {
                _lblError.Text = string.Empty;
                UseWaitCursor = true;
                try
                {
                    var error = Aceptar();
                    if (error == null) DialogResult = DialogResult.OK;
                    else _lblError.Text = error;
                }
                finally { UseWaitCursor = false; }
            };
        }

        /// <summary>Valida y guarda. Devuelve null si fue exitoso o el mensaje de error a mostrar.</summary>
        protected abstract string? Aceptar();

        protected void MostrarErrorInterno(string mensaje) => _lblError.Text = mensaje;
    }
}
