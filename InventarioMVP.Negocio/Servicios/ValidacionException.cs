namespace InventarioMVP.Negocio.Servicios
{
    /// <summary>Error de regla de negocio cuyo mensaje es apto para mostrarse al usuario.</summary>
    public class ValidacionException : Exception
    {
        public ValidacionException(string mensaje) : base(mensaje) { }
    }
}
