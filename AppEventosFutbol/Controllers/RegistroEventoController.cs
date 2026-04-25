using System;
using System.Collections.Generic;
using System.Text;
using AppEventosFutbol.Models;
using AppEventosFutbol.Services;
using System.Threading.Tasks;

namespace AppEventosFutbol.Controllers
{
    public class RegistroEventoController
    {
        // 1. DESCARGAR ESTADIOS REALES DESDE SUPABASE
        public async Task<List<Estadio>> ObtenerEstadiosAsync()
        {
            try
            {
                // Descargamos todos los registros de la tabla 'estadios'
                var respuesta = await SupabaseConfig.Cliente.From<Estadio>().Get();
                return respuesta.Models; // Devolvemos la lista real
            }
            catch
            {
                // Si no hay internet, devolvemos una lista vacía para que la app no explote
                return new List<Estadio>();
            }
        }

        // 2. VALIDAR Y GUARDAR EVENTO EN SUPABASE
        public async Task<(bool EsValido, string MensajeError)> ValidarYGuardarEventoAsync(string equipoLocal, string equipoVisitante, string boletosText, string precioText, DateTime fecha, TimeSpan hora, Estadio estadioSeleccionado)
        {
            // Validaciones (Igual que antes)
            if (string.IsNullOrWhiteSpace(equipoLocal) || string.IsNullOrWhiteSpace(equipoVisitante))
                return (false, "Los nombres de los equipos no pueden estar vacíos.");

            if (!int.TryParse(boletosText, out int boletos) || boletos <= 0)
                return (false, "El número de boletos debe ser mayor a 0.");

            if (!decimal.TryParse(precioText, out decimal precio) || precio <= 0)
                return (false, "El precio debe ser mayor a 0.");

            DateTime fechaHoraEvento = fecha.Date.Add(hora);
            if (fechaHoraEvento < DateTime.Now)
                return (false, "La fecha y hora no pueden estar en el pasado.");

            if (estadioSeleccionado == null)
                return (false, "Debes seleccionar un estadio anfitrión.");

            try
            {
                // Si pasa las validaciones, armamos el "Paquete" (Modelo)
                var nuevoEvento = new Evento
                {
                    EquipoLocal = equipoLocal,
                    EquipoVisitante = equipoVisitante,
                    BoletosTotales = boletos,
                    NumeroBoletos = boletos, // Al inicio, los disponibles son iguales a los totales
                    Precio = precio,
                    FechaHora = fechaHoraEvento,
                    EstadioId = estadioSeleccionado.Id,
                    EstadioNombre = estadioSeleccionado.Nombre, // Guardamos el nombre para búsquedas rápidas
                    Latitud = estadioSeleccionado.Latitud,      // Guardamos coordenadas para el mapa
                    Longitud = estadioSeleccionado.Longitud
                };

                // Insertamos en la nube
                await SupabaseConfig.Cliente.From<Evento>().Insert(nuevoEvento);

                return (true, "Evento registrado correctamente en la base de datos.");
            }
            catch (Exception ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
        }
    }
}
