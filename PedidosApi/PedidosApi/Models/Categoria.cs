using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PedidosApi.Models
{
    [Table("categoria")]
    public class Categoria
    {
        [Key]
        [Column("idcategoria")]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string Descripcion { get; set; }

        [JsonIgnore]
        public ICollection<Articulo> Articulos { get; set; }
    }
}
