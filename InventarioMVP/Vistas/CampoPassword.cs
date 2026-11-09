namespace InventarioMVP.Vistas
{
    /// <summary>Campo de contraseña con botón de ojo para mostrar u ocultar el texto.</summary>
    public class CampoPassword : Panel
    {
        private readonly TextBox _texto = new()
        {
            Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, UseSystemPasswordChar = true, MaxLength = 72, Font = Estilos.Fuente
        };
        private readonly Button _ojo = new()
        {
            Dock = DockStyle.Right, Width = 36, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, TabStop = false, BackColor = Color.White
        };

        public CampoPassword()
        {
            Height = 30;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Padding = new Padding(4, 4, 0, 0);
            _ojo.FlatAppearance.BorderSize = 0;
            _ojo.FlatAppearance.MouseOverBackColor = Estilos.Fondo;
            ActualizarIcono();
            _ojo.Click += (_, _) =>
            {
                _texto.UseSystemPasswordChar = !_texto.UseSystemPasswordChar;
                ActualizarIcono();
                _texto.Focus();
            };
            Controls.Add(_texto);
            Controls.Add(_ojo);
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string Valor
        {
            get => _texto.Text;
            set => _texto.Text = value;
        }

        public void Limpiar()
        {
            _texto.Clear();
            _texto.UseSystemPasswordChar = true;
            ActualizarIcono();
        }

        public new void Focus() => _texto.Focus();

        private void ActualizarIcono() =>
            _ojo.Image = Iconos.Obtener(_texto.UseSystemPasswordChar ? "eye" : "eye-off", Iconos.Sutil);
    }
}
