using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace AppEventosFutbol.Models
{
    [Table("equipos")]
    public class Equipo : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }
    }
}
