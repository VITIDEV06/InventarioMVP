using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos
{
    /// <summary>
    /// Fábrica de conexiones MySQL. Usa la cadena de conexión del usuario de
    /// aplicación (privilegios mínimos), recibida desde appsettings.json.
    /// </summary>
    public class ConexionMySql
    {
        private readonly string _cadenaConexion;

        public ConexionMySql(string cadenaConexion)
        {
            if (string.IsNullOrWhiteSpace(cadenaConexion))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(cadenaConexion));
            _cadenaConexion = cadenaConexion;
        }

        /// <summary>Crea y abre una conexión nueva. El llamador debe liberarla (using).</summary>
        public MySqlConnection Abrir()
        {
            var conexion = new MySqlConnection(_cadenaConexion);
            conexion.Open();
            return conexion;
        }
    }
}
