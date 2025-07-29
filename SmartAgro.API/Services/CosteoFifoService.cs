using Microsoft.EntityFrameworkCore;
using SmartAgro.Data;
using SmartAgro.Models.Entities;

namespace SmartAgro.API.Services
{
    public interface ICosteoFifoService
    {
        Task<decimal> ObtenerCostoSalidaFifoAsync(int materiaPrimaId, decimal cantidadSolicitada);
    }

    public class CosteoFifoService : ICosteoFifoService
    {
        private readonly SmartAgroDbContext _context;

        public CosteoFifoService(SmartAgroDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> ObtenerCostoSalidaFifoAsync(int materiaPrimaId, decimal cantidadSolicitada)
        {
            var entradas = await _context.MovimientosStock
                .Where(m => m.MateriaPrimaId == materiaPrimaId && m.Tipo == "Entrada")
                .OrderBy(m => m.Fecha)
                .ToListAsync();

            decimal costoTotal = 0;
            decimal restante = cantidadSolicitada;

            foreach (var entrada in entradas)
            {
                if (restante <= 0) break;

                var disponible = entrada.Cantidad;
                var aUsar = Math.Min(restante, disponible);

                costoTotal += aUsar * entrada.CostoUnitario;
                restante -= aUsar;
            }

            if (restante > 0)
                throw new Exception("No hay suficiente inventario para cubrir la cantidad solicitada.");

            return costoTotal;
        }
    }
}