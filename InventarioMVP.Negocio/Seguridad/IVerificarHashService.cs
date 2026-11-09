namespace InventarioMVP.Negocio.Seguridad
{
    /// <summary>Contrato con una única responsabilidad: verificar un texto plano contra un hash almacenado.</summary>
    public interface IVerificarHashService
    {
        bool Verificar(string textoPlano, string hashAlmacenado);
    }
}
