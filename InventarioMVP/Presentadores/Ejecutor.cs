using InventarioMVP.Negocio.Servicios;

namespace InventarioMVP.Presentadores
{
    /// <summary>Manejo uniforme de errores en los presentadores: los mensajes de negocio se muestran tal cual.</summary>
    internal static class Ejecutor
    {
        public static void Ejecutar(Action accion, Action<string> mostrarError)
        {
            var error = Intentar(accion);
            if (error != null) mostrarError(error);
        }

        /// <summary>Ejecuta la acción y devuelve el mensaje de error, o null si salió bien.</summary>
        public static string? Intentar(Action accion)
        {
            try
            {
                accion();
                return null;
            }
            catch (ValidacionException ex) { return ex.Message; }
            catch (Exception ex) { return "No se pudo completar la operación: " + ex.Message; }
        }
    }
}
