using System;
using System.Collections.Generic;
using System.Text;
using AppEventosFutbol.Models;
using AppEventosFutbol.Services;

namespace AppEventosFutbol.Controllers
{
    public class AgregarEstadioController
    {
        public async Task<(bool EsValido, string Mensaje)> ValidarYGuardarEstadio(string nombre, double? latitud, double? longitud)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre del estadio no puede estar vacío.");

            if (latitud == null || longitud == null)
                return (false, "Debes seleccionar una ubicación en el mapa haciendo clic para colocar el Pin.");

            try
            {
                // 1. Creamos el objeto con los datos
                var nuevoEstadio = new Estadio
                {
                    Nombre = nombre,
                    Latitud = latitud.Value,
                    Longitud = longitud.Value
                };

                // 2. MAGIA: Insertamos en Supabase
                await SupabaseConfig.Cliente.From<Estadio>().Insert(nuevoEstadio);

                return (true, "Estadio guardado con éxito en la nube.");
            }
            catch (Exception ex)
            {
                // Si falla el internet o la base de datos
                return (false, $"Error al guardar en la base de datos: {ex.Message}");
            }
        }
    }
}
