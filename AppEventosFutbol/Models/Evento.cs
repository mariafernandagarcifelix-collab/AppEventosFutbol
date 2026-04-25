using System;
using System.Collections.Generic;
using System.Text;
using Postgrest.Attributes;
using Postgrest.Models;

namespace AppEventosFutbol.Models
{
    [Table("eventos")]
    public class Evento : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("equipo_local")]
        public string EquipoLocal { get; set; }

        [Column("equipo_visitante")]
        public string EquipoVisitante { get; set; }

        [Column("boletos_totales")]
        public int BoletosTotales { get; set; }

        [Column("numero_boletos")]
        public int NumeroBoletos { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("estadio_id")]
        public int EstadioId { get; set; }

        [Column("estadio_nombre")]
        public string EstadioNombre { get; set; }

        [Column("latitud")]
        public double Latitud { get; set; }

        [Column("longitud")]
        public double Longitud { get; set; }
    }
}
