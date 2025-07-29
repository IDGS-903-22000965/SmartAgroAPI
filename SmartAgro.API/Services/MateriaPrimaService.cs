// SmartAgro.API/Services/MateriaPrimaService.cs
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class MateriaPrimaService : IMateriaPrimaService
    {
        private readonly SmartAgroDbContext _context;

        public MateriaPrimaService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<List<MateriaPrima>> ObtenerMateriasPrimasAsync()
        {
            try
            {
                return await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Activo)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener materias primas: {ex.Message}");
            }
        }

        public async Task<MateriaPrima?> ObtenerMateriaPrimaPorIdAsync(int id)
        {
            try
            {
                return await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Include(m => m.ProductoMateriasPrimas)
                        .ThenInclude(pm => pm.Producto)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener materia prima: {ex.Message}");
            }
        }

        public async Task<MateriaPrima> CrearMateriaPrimaAsync(MateriaPrima materiaPrima)
        {
            try
            {
                // Verificar que el proveedor existe
                var proveedor = await _context.Proveedores.FindAsync(materiaPrima.ProveedorId);
                if (proveedor == null)
                {
                    throw new Exception("Proveedor no encontrado");
                }

                // Verificar que no existe otra materia prima con el mismo nombre del mismo proveedor
                var existeNombre = await _context.MateriasPrimas
                    .AnyAsync(m => m.Nombre.ToLower() == materiaPrima.Nombre.ToLower()
                                && m.ProveedorId == materiaPrima.ProveedorId
                                && m.Activo);

                if (existeNombre)
                {
                    throw new Exception("Ya existe una materia prima con ese nombre para este proveedor");
                }

                materiaPrima.Activo = true;
                _context.MateriasPrimas.Add(materiaPrima);
                await _context.SaveChangesAsync();

                return materiaPrima;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear materia prima: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarMateriaPrimaAsync(MateriaPrima materiaPrima)
        {
            try
            {
                var materiaPrimaExistente = await _context.MateriasPrimas.FindAsync(materiaPrima.Id);
                if (materiaPrimaExistente == null)
                {
                    return false;
                }

                // Verificar que el proveedor existe
                var proveedor = await _context.Proveedores.FindAsync(materiaPrima.ProveedorId);
                if (proveedor == null)
                {
                    throw new Exception("Proveedor no encontrado");
                }

                // Verificar que no existe otra materia prima con el mismo nombre del mismo proveedor
                var existeNombre = await _context.MateriasPrimas
                    .AnyAsync(m => m.Nombre.ToLower() == materiaPrima.Nombre.ToLower()
                                && m.ProveedorId == materiaPrima.ProveedorId
                                && m.Id != materiaPrima.Id
                                && m.Activo);

                if (existeNombre)
                {
                    throw new Exception("Ya existe una materia prima con ese nombre para este proveedor");
                }

                // Actualizar propiedades
                materiaPrimaExistente.Nombre = materiaPrima.Nombre;
                materiaPrimaExistente.Descripcion = materiaPrima.Descripcion;
                materiaPrimaExistente.UnidadMedida = materiaPrima.UnidadMedida;
                materiaPrimaExistente.CostoUnitario = materiaPrima.CostoUnitario;
                materiaPrimaExistente.Stock = materiaPrima.Stock;
                materiaPrimaExistente.StockMinimo = materiaPrima.StockMinimo;
                materiaPrimaExistente.ProveedorId = materiaPrima.ProveedorId;
                materiaPrimaExistente.Activo = materiaPrima.Activo;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar materia prima: {ex.Message}");
            }
        }

        public async Task<bool> EliminarMateriaPrimaAsync(int id)
        {
            try
            {
                var materiaPrima = await _context.MateriasPrimas.FindAsync(id);
                if (materiaPrima == null)
                {
                    return false;
                }

                // Verificar si está siendo utilizada en productos
                var estaEnUso = await _context.ProductoMateriasPrimas
                    .AnyAsync(pm => pm.MateriaPrimaId == id);

                if (estaEnUso)
                {
                    // Soft delete - solo marcar como inactivo
                    materiaPrima.Activo = false;
                }
                else
                {
                    // Hard delete si no está en uso
                    _context.MateriasPrimas.Remove(materiaPrima);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar materia prima: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarStockAsync(int id, int nuevoStock)
        {
            try
            {
                var materiaPrima = await _context.MateriasPrimas.FindAsync(id);
                if (materiaPrima == null)
                {
                    return false;
                }

                materiaPrima.Stock = nuevoStock;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar stock: {ex.Message}");
            }
        }

        public async Task<List<MateriaPrima>> ObtenerPorProveedorAsync(int proveedorId)
        {
            try
            {
                return await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.ProveedorId == proveedorId && m.Activo)
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener materias primas por proveedor: {ex.Message}");
            }
        }

        public async Task<List<MateriaPrima>> ObtenerBajoStockAsync()
        {
            try
            {
                return await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Activo && m.Stock <= m.StockMinimo)
                    .OrderBy(m => m.Stock)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener materias primas bajo stock: {ex.Message}");
            }
        }

        public async Task<List<MateriaPrima>> BuscarMateriasPrimasAsync(string termino)
        {
            try
            {
                return await _context.MateriasPrimas
                    .Include(m => m.Proveedor)
                    .Where(m => m.Activo &&
                               (m.Nombre.Contains(termino) ||
                                m.Descripcion!.Contains(termino) ||
                                m.Proveedor.Nombre.Contains(termino)))
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar materias primas: {ex.Message}");
            }
        }
    }
}