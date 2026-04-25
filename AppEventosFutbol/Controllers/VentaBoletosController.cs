using AppEventosFutbol.Models;
using AppEventosFutbol.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppEventosFutbol.Controllers
{
    public class VentaBoletosController
    {
        // Descargar eventos reales
        public async Task<List<Evento>> ObtenerEventosAsync()
        {
            try
            {
                var res = await SupabaseConfig.Cliente.From<Evento>().Get();
                return res.Models;
            }
            catch { return new List<Evento>(); }
        }

        public (bool PermiteVenta, string MensajeEstatus, string ColorEstatus) EvaluarDisponibilidad(Evento evento)
        {
            TimeSpan tiempoFaltante = evento.FechaHora - DateTime.Now;
            if (tiempoFaltante.TotalDays < 0) return (false, "EVENTO FINALIZADO", "#555555");
            if (tiempoFaltante.TotalDays <= 3) return (false, "SOLO CONSULTA", "#CE1126");
            return (true, "VENTA ABIERTA", "#006847");
        }

        // PROCESAR COMPRA REAL EN SUPABASE
        public async Task<(bool Exito, string Mensaje)> ProcesarCompraAsync(Evento evento, string nombre, string telefono, string cantidadText)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(telefono))
                return (false, "Completa los datos del cliente.");

            if (!int.TryParse(cantidadText, out int cantidad) || cantidad <= 0)
                return (false, "Cantidad inválida.");

            if (cantidad > evento.NumeroBoletos)
                return (false, $"Solo quedan {evento.NumeroBoletos} boletos.");

            try
            {
                // 1. Actualizamos el objeto local
                evento.NumeroBoletos -= cantidad;

                // 2. Sincronizamos con Supabase (Filtramos por ID para actualizar solo esa fila)
                await SupabaseConfig.Cliente
                    .From<Evento>()
                    .Where(x => x.Id == evento.Id)
                    .Update(evento);

                return (true, $"Venta exitosa. Total: ${(cantidad * evento.Precio):N2}");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar base de datos: {ex.Message}");
            }
        }
    }
}
