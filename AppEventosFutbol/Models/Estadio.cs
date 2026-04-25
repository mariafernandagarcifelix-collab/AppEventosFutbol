using System;
using System.Collections.Generic;
using System.Text;
using Postgrest.Attributes;
using Postgrest.Models;

namespace AppEventosFutbol.Models
{
    [Table("estadios")]
    public class Estadio : BaseModel
    {
        [PrimaryKey("id", false)] // false significa que Supabase genera el ID automáticamente (Autoincrementable o UUID)
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("latitud")]
        public double Latitud { get; set; }

        [Column("longitud")]
        public double Longitud { get; set; }
    }
}
