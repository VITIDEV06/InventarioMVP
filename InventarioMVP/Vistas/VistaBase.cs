namespace InventarioMVP.Vistas
{
    /// <summary>Base de las pantallas del Dashboard: estado de carga, notificaciones y confirmaciones.</summary>
    public abstract class VistaBase : UserControl
    {
        protected VistaBase()
        {
            Dock = DockStyle.Fill;
            Font = Estilos.Fuente;
            BackColor = Estilos.Fondo;
            Padding = new Padding(20, 8, 20, 14);
        }

        /// <summary>Dispara un evento hacia el presentador mostrando el cursor de espera mientras se ejecuta.</summary>
        protected void Disparar(EventHandler? manejador)
        {
            if (manejador == null) return;
            UseWaitCursor = true;
            Cursor = Cursors.WaitCursor;
            try { manejador(this, EventArgs.Empty); }
            finally
            {
                UseWaitCursor = false;
                Cursor = Cursors.Default;
            }
        }

        public void MostrarError(string mensaje) => Notificador.Error(this, mensaje);

        public void MostrarInfo(string mensaje) => Notificador.Exito(this, mensaje);

        public bool Confirmar(string mensaje) =>
            MessageBox.Show(FindForm(), mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        /// <summary>Título de la pantalla con una línea descriptiva.</summary>
        protected static Panel Encabezado(string titulo, string descripcion)
        {
            var p = new Panel { Dock = DockStyle.Top, Height = 62 };
            // El control Fill se agrega primero: se acomoda al final, con el espacio que dejan los demás.
            p.Controls.Add(new Label
            {
                Text = titulo, Dock = DockStyle.Fill, Font = Estilos.FuenteTitulo, ForeColor = Estilos.Menu, TextAlign = ContentAlignment.BottomLeft
            });
            p.Controls.Add(new Label
            {
                Text = descripcion, Dock = DockStyle.Bottom, Height = 24, ForeColor = Estilos.Neutro, Font = Estilos.Fuente
            });
            return p;
        }
    }
}
