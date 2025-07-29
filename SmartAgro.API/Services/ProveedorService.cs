// SmartAgro.API/Services/ProveedorService.cs
using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly SmartAgroDbContext _context;

        public ProveedorService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<List<Proveedor>> ObtenerProveedoresAsync()
        {
            try
            {
                return await _context.Proveedores
                    .Include(p => p.MateriasPrimas.Where(m => m.Activo))
                    .Include(p => p.Compras)
                    .Where(p => p.Activo)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener proveedores: {ex.Message}");
            }
        }

        public async Task<Proveedor?> ObtenerProveedorPorIdAsync(int id)
        {
            try
            {
                return await _context.Proveedores
                    .Include(p => p.MateriasPrimas.Where(m => m.Activo))
                    .Include(p => p.Compras)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener proveedor: {ex.Message}");
            }
        }

        public async Task<Proveedor> CrearProveedorAsync(Proveedor proveedor)
        {
            try
            {
                // Verificar que no existe otro proveedor con el mismo RFC (si se proporciona)
                if (!string.IsNullOrEmpty(proveedor.RFC))
                {
                    var existeRFC = await _context.Proveedores
                        .AnyAsync(p => p.RFC == proveedor.RFC && p.Activo);

                    if (existeRFC)
                    {
                        throw new Exception("Ya existe un proveedor con ese RFC");
                    }
                }

                // Verificar que no existe otro proveedor con la misma razón social
                var existeRazonSocial = await _context.Proveedores
                    .AnyAsync(p => p.RazonSocial.ToLower() == proveedor.RazonSocial.ToLower() && p.Activo);

                if (existeRazonSocial)
                {
                    throw new Exception("Ya existe un proveedor con esa razón social");
                }

                proveedor.Activo = true;
                proveedor.FechaRegistro = DateTime.Now;

                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                return proveedor;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear proveedor: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarProveedorAsync(Proveedor proveedor)
        {
            try
            {
                var proveedorExistente = await _context.Proveedores.FindAsync(proveedor.Id);
                if (proveedorExistente == null)
                {
                    return false;
                }

                // Verificar que no existe otro proveedor con el mismo RFC (si se proporciona)
                if (!string.IsNullOrEmpty(proveedor.RFC))
                {
                    var existeRFC = await _context.Proveedores
                        .AnyAsync(p => p.RFC == proveedor.RFC && p.Id != proveedor.Id && p.Activo);

                    if (existeRFC)
                    {
                        throw new Exception("Ya existe un proveedor con ese RFC");
                    }
                }

                // Verificar que no existe otro proveedor con la misma razón social
                var existeRazonSocial = await _context.Proveedores
                    .AnyAsync(p => p.RazonSocial.ToLower() == proveedor.RazonSocial.ToLower()
                                && p.Id != proveedor.Id && p.Activo);

                if (existeRazonSocial)
                {
                    throw new Exception("Ya existe un proveedor con esa razón social");
                }

                // Actualizar propiedades
                proveedorExistente.Nombre = proveedor.Nombre;
                proveedorExistente.RazonSocial = proveedor.RazonSocial;
                proveedorExistente.RFC = proveedor.RFC;
                proveedorExistente.Direccion = proveedor.Direccion;
                proveedorExistente.Telefono = proveedor.Telefono;
                proveedorExistente.Email = proveedor.Email;
                proveedorExistente.ContactoPrincipal = proveedor.ContactoPrincipal;
                proveedorExistente.Activo = proveedor.Activo;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar proveedor: {ex.Message}");
            }
        }

        public async Task<bool> EliminarProveedorAsync(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null)
                {
                    return false;
                }

                // Verificar si tiene compras o materias primas asociadas
                var tieneCompras = await _context.ComprasProveedores
                    .AnyAsync(c => c.ProveedorId == id);

                var tieneMateriasPrimas = await _context.MateriasPrimas
                    .AnyAsync(m => m.ProveedorId == id);

                if (tieneCompras || tieneMateriasPrimas)
                {
                    // Soft delete - solo marcar como inactivo
                    proveedor.Activo = false;
                }
                else
                {
                    // Hard delete si no tiene relaciones
                    _context.Proveedores.Remove(proveedor);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar proveedor: {ex.Message}");
            }
        }

        public async Task<List<Proveedor>> BuscarProveedoresAsync(string termino)
        {
            try
            {
                return await _context.Proveedores
                    .Include(p => p.MateriasPrimas.Where(m => m.Activo))
                    .Where(p => p.Activo &&
                               (p.Nombre.Contains(termino) ||
                                p.RazonSocial.Contains(termino) ||
                                (p.RFC != null && p.RFC.Contains(termino)) ||
                                (p.ContactoPrincipal != null && p.ContactoPrincipal.Contains(termino))))
                    .OrderBy(p => p.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar proveedores: {ex.Message}");
            }
        }
    }
}