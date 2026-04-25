using System;
using System.Collections.Generic;
using System.Text;
using AppEventosFutbol.Models;
using AppEventosFutbol.Services;
using System.Threading.Tasks;

namespace AppEventosFutbol.Controllers
{
    public class ReportesController
    {
        public async Task<List<Evento>> ObtenerEventosProximosMapaAsync()
        {
            try
            {
                // 1. Traer todos los eventos de la nube
                var res = await SupabaseConfig.Cliente.From<Evento>().Get();
                var todos = res.Models;

                // 2. Lógica de Negocio: Filtrar y agrupar
                return todos
                    .Where(e => e.FechaHora >= DateTime.Now) // Solo futuros
                    .GroupBy(e => e.EstadioNombre)           // Agrupar por sede
                    .Select(g => g.OrderBy(e => e.FechaHora).First()) // El más próximo de cada grupo
                    .ToList();
            }
            catch { return new List<Evento>(); }
        }
    }
}
