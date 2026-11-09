using System.Globalization;
using InventarioMVP.Entidades;

namespace InventarioMVP.Vistas
{
    /// <summary>Pantalla de inicio: bienvenida con fecha y hora actualizadas cada segundo.</summary>
    public class UcInicio : UserControl
    {
        private readonly Label _lblHora = new()
        {
            Dock = DockStyle.Top, Height = 110, TextAlign = ContentAlignment.BottomCenter,
            Font = new Font("Segoe UI Light", 56F), ForeColor = Estilos.Menu
        };
        private readonly Label _lblFecha = new()
        {
            Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.TopCenter,
            Font = new Font("Segoe UI", 16F), ForeColor = Estilos.Primario
        };
        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };

        public UcInicio(Usuario usuario)
        {
            Dock = DockStyle.Fill;
            BackColor = Estilos.Fondo;

            var bienvenida = new Label
            {
                Text = "Bienvenido, " + usuario.NombreCompleto,
                Dock = DockStyle.Top, Height = 80, TextAlign = ContentAlignment.BottomCenter,
                Font = Estilos.FuenteTitulo, ForeColor = Estilos.Menu
            };

            var logo = new PictureBox
            {
                Dock = DockStyle.Top, Height = 190, SizeMode = PictureBoxSizeMode.CenterImage,
                Image = Imagenes.Logo(150)
            };

            // Orden inverso de apilado: el último agregado queda arriba.
            Controls.Add(_lblFecha);
            Controls.Add(_lblHora);
            Controls.Add(bienvenida);
            Controls.Add(logo);

            Actualizar();
            _timer.Tick += (_, _) => Actualizar();
            _timer.Start();
        }

        private void Actualizar()
        {
            var ahora = DateTime.Now;
            _lblHora.Text = ahora.ToString("HH:mm:ss");
            var fecha = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-PA"));
            _lblFecha.Text = char.ToUpper(fecha[0]) + fecha[1..];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _timer.Dispose();
            base.Dispose(disposing);
        }
    }
}
