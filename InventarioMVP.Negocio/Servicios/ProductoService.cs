using InventarioMVP.Datos.Repositorios;
using InventarioMVP.Entidades;

namespace InventarioMVP.Negocio.Servicios
{
    public class ProductoService
    {
        private readonly IProductoRepositorio _productos;

        public ProductoService(IProductoRepositorio productos) => _productos = productos;

        public IList<Producto> Listar() => _productos.ObtenerTodos();

        public IList<Producto> Buscar(string texto)
        {
            texto = (texto ?? "").Trim();
            return texto.Length == 0 ? _productos.ObtenerTodos() : _productos.Buscar(texto);
        }

        /// <summary>Inserta si Id == 0, de lo contrario actualiza.</summary>
        public void Guardar(Producto p)
        {
            p.Codigo = (p.Codigo ?? "").Trim();
            p.Nombre = (p.Nombre ?? "").Trim();
            p.Descripcion = (p.Descripcion ?? "").Trim();
            p.Categoria = (p.Categoria ?? "").Trim();

            if (p.Codigo.Length == 0 || p.Codigo.Length > 30)
                throw new ValidacionException("El código es obligatorio (máximo 30 caracteres).");
            if (p.Nombre.Length == 0 || p.Nombre.Length > 100)
                throw new ValidacionException("El nombre es obligatorio (máximo 100 caracteres).");
            if (p.Descripcion.Length > 255)
                throw new ValidacionException("La descripción no puede superar 255 caracteres.");
            if (p.Categoria.Length == 0 || p.Categoria.Length > 60)
                throw new ValidacionException("La categoría es obligatoria (máximo 60 caracteres).");
            if (p.Precio < 0 || p.Precio > 99999999.99m)
                throw new ValidacionException("El precio debe estar entre 0 y 99,999,999.99.");
            if (p.Stock < 0)
                throw new ValidacionException("El stock no puede ser negativo.");
            if (_productos.ExisteCodigo(p.Codigo, p.Id))
                throw new ValidacionException("Ya existe un producto con ese código.");

            if (p.Id == 0)
                p.Id = _productos.Insertar(p);
            else if (!_productos.Actualizar(p))
                throw new ValidacionException("El producto ya no existe.");
        }

        public void Eliminar(int id)
        {
            if (id <= 0) throw new ValidacionException("Seleccione un producto.");
            if (!_productos.Eliminar(id))
                throw new ValidacionException("El producto ya no existe.");
        }
    }
}
