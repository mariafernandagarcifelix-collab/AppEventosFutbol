using AppEventosFutbol.Models;
using AppEventosFutbol.Services;
using System;
using System.Threading.Tasks;

namespace AppEventosFutbol.Controllers
{
    public class AgregarEquipoController
    {
        public async Task<(bool Exito, string Mensaje)> GuardarEquipoAsync(string nombreEquipo)
        {
            if (string.IsNullOrWhiteSpace(nombreEquipo))
                return (false, "El nombre del equipo no puede estar vacío.");

            try
            {
                var nuevoEquipo = new Equipo
                {
                    Nombre = nombreEquipo
                };

                await SupabaseConfig.Cliente.From<Equipo>().Insert(nuevoEquipo);
                return (true, "Equipo guardado correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al guardar el equipo: {ex.Message}");
            }
        }
    }
}
