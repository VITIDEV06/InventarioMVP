namespace InventarioMVP.Vistas
{
    /// <summary>Notificaciones visuales (toast) de éxito, error e información en la esquina inferior derecha.</summary>
    internal static class Notificador
    {
        public static void Exito(Control origen, string mensaje) => Mostrar(origen, mensaje, Estilos.Exito, "circle-check");
        public static void Error(Control origen, string mensaje) => Mostrar(origen, mensaje, Estilos.Peligro, "circle-x");
        public static void Info(Control origen, string mensaje) => Mostrar(origen, mensaje, Estilos.Primario, "info");

        private static void Mostrar(Control origen, string mensaje, Color color, string icono)
        {
            var formulario = origen.FindForm();
            if (formulario == null || formulario.IsDisposed) return;
            var toast = formulario.Controls.OfType<ToastPanel>().FirstOrDefault();
            if (toast == null)
            {
                toast = new ToastPanel();
                formulario.Controls.Add(toast);
            }
            toast.Mostrar(formulario, mensaje, color, icono);
        }

        private sealed class ToastPanel : Panel
        {
            private readonly Label _texto = new() { Dock = DockStyle.Fill, ForeColor = Color.White, Font = Estilos.Fuente, TextAlign = ContentAlignment.MiddleLeft };
            private readonly PictureBox _icono = new() { Dock = DockStyle.Left, Width = 40, SizeMode = PictureBoxSizeMode.CenterImage };
            private readonly System.Windows.Forms.Timer _temporizador = new() { Interval = 4000 };

            public ToastPanel()
            {
                Visible = false;
                Size = new Size(380, 60);
                Padding = new Padding(6, 6, 10, 6);
                Controls.Add(_texto);
                Controls.Add(_icono);
                _temporizador.Tick += (_, _) => { _temporizador.Stop(); Visible = false; };
            }

            public void Mostrar(Form formulario, string mensaje, Color color, string icono)
            {
                BackColor = color;
                _texto.Text = mensaje;
                _icono.Image = Iconos.Obtener(icono, Iconos.Blanco, 22);
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                Location = new Point(formulario.ClientSize.Width - Width - 20, formulario.ClientSize.Height - Height - 20);
                Visible = true;
                BringToFront();
                _temporizador.Stop();
                _temporizador.Start();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) _temporizador.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
