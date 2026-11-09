namespace InventarioMVP.Negocio.Seguridad
{
    /// <summary>Contrato con una única responsabilidad: generar el hash de una contraseña.</summary>
    public interface IHashService
    {
        string GenerarHash(string textoPlano);
    }
}
