using MySql.Data.MySqlClient;

namespace InventarioMVP.Datos
{
    /// <summary>Error de datos con mensaje apto para el usuario (duplicados, registros en uso, restricciones).</summary>
    public class ConflictoDatosException : Exception
    {
        public ConflictoDatosException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Fábrica de conexiones MySQL. Usa la cadena de conexión del usuario de
    /// aplicación (privilegios mínimos), recibida desde appsettings.json.
    /// Todas las consultas se ejecutan con parámetros.
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

        /// <summary>Ejecuta el trabajo con una conexión propia.</summary>
        public T Usar<T>(Func<Operacion, T> trabajo)
        {
            try
            {
                using var cn = Abrir();
                return trabajo(new Operacion(cn, null));
            }
            catch (MySqlException ex) { throw Traducir(ex); }
        }

        /// <summary>Ejecuta el trabajo dentro de una transacción: se confirma al terminar o se revierte si falla.</summary>
        public T EnTransaccion<T>(Func<Operacion, T> trabajo)
        {
            try
            {
                using var cn = Abrir();
                using var tx = cn.BeginTransaction();
                try
                {
                    var resultado = trabajo(new Operacion(cn, tx));
                    tx.Commit();
                    return resultado;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch (MySqlException ex) { throw Traducir(ex); }
        }

        public void EnTransaccion(Action<Operacion> trabajo) =>
            EnTransaccion<object?>(op => { trabajo(op); return null; });

        // Atajos de una sola operación
        public List<T> Consultar<T>(string sql, Func<MySqlDataReader, T> mapear, params (string, object?)[] p) =>
            Usar(op => op.Consultar(sql, mapear, p));

        public object? Escalar(string sql, params (string, object?)[] p) => Usar(op => op.Escalar(sql, p));

        public int Ejecutar(string sql, params (string, object?)[] p) => Usar(op => op.Ejecutar(sql, p));

        public int Insertar(string sql, params (string, object?)[] p) => Usar(op => op.Insertar(sql, p));

        private static Exception Traducir(MySqlException ex) => ex.Number switch
        {
            1062 => new ConflictoDatosException("Ya existe un registro con esos datos."),
            1451 or 1452 => new ConflictoDatosException("El registro está relacionado con otros datos y no se puede modificar así."),
            3819 or 4025 => new ConflictoDatosException("Los datos no cumplen las restricciones de la base de datos."),
            _ => ex
        };
    }

    /// <summary>Operaciones parametrizadas sobre una conexión (y, opcionalmente, una transacción).</summary>
    public sealed class Operacion
    {
        private readonly MySqlConnection _cn;
        private readonly MySqlTransaction? _tx;

        internal Operacion(MySqlConnection cn, MySqlTransaction? tx)
        {
            _cn = cn;
            _tx = tx;
        }

        private MySqlCommand Crear(string sql, (string, object?)[] parametros)
        {
            var cmd = new MySqlCommand(sql, _cn, _tx);
            foreach (var (nombre, valor) in parametros)
                cmd.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
            return cmd;
        }

        public List<T> Consultar<T>(string sql, Func<MySqlDataReader, T> mapear, params (string, object?)[] p)
        {
            var lista = new List<T>();
            using var cmd = Crear(sql, p);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) lista.Add(mapear(rd));
            return lista;
        }

        /// <summary>Ejecuta una consulta y devuelve sus columnas (alias del SELECT) y filas, sin mapear a una entidad.</summary>
        public (List<string> Columnas, List<object?[]> Filas) Tabla(string sql, params (string, object?)[] p)
        {
            var columnas = new List<string>();
            var filas = new List<object?[]>();
            using var cmd = Crear(sql, p);
            using var rd = cmd.ExecuteReader();
            for (int i = 0; i < rd.FieldCount; i++) columnas.Add(rd.GetName(i));
            while (rd.Read())
            {
                var fila = new object?[rd.FieldCount];
                for (int i = 0; i < fila.Length; i++) fila[i] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                filas.Add(fila);
            }
            return (columnas, filas);
        }

        public T? Primero<T>(string sql, Func<MySqlDataReader, T> mapear, params (string, object?)[] p) where T : class =>
            Consultar(sql, mapear, p).FirstOrDefault();

        public object? Escalar(string sql, params (string, object?)[] p)
        {
            using var cmd = Crear(sql, p);
            var valor = cmd.ExecuteScalar();
            return valor == DBNull.Value ? null : valor;
        }

        public int Ejecutar(string sql, params (string, object?)[] p)
        {
            using var cmd = Crear(sql, p);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>Ejecuta un INSERT y devuelve el id generado.</summary>
        public int Insertar(string sql, params (string, object?)[] p)
        {
            using var cmd = Crear(sql, p);
            cmd.ExecuteNonQuery();
            return (int)cmd.LastInsertedId;
        }
    }

    /// <summary>Lectura segura de columnas que pueden ser NULL.</summary>
    internal static class Lector
    {
        public static string Texto(this MySqlDataReader rd, string col)
        {
            int i = rd.GetOrdinal(col);
            return rd.IsDBNull(i) ? string.Empty : rd.GetString(i);
        }

        public static DateTime? FechaNula(this MySqlDataReader rd, string col)
        {
            int i = rd.GetOrdinal(col);
            return rd.IsDBNull(i) ? null : rd.GetDateTime(i);
        }

        public static int Entero(this MySqlDataReader rd, string col)
        {
            int i = rd.GetOrdinal(col);
            return rd.IsDBNull(i) ? 0 : Convert.ToInt32(rd.GetValue(i));
        }

        public static decimal Decimal(this MySqlDataReader rd, string col)
        {
            int i = rd.GetOrdinal(col);
            return rd.IsDBNull(i) ? 0m : Convert.ToDecimal(rd.GetValue(i));
        }
    }
}
