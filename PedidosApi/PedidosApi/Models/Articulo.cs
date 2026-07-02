using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PedidosApi.Models
{
    [Table("articulo")]
    public class Articulo
    {
        [Key]
        [Column("idarticulo")]
        public int Id { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("imagen")]
        public byte[]? Imagen { get; set; }

        [Column("idcategoria")]
        public int IdCategoria { get; set; }

        [ValidateNever]
        [ForeignKey("IdCategoria")]
        public Categoria Categoria { get; set; }

        [Column("idpresentacion")]
        public int IdPresentacion { get; set; }

        [ValidateNever]
        [ForeignKey("IdPresentacion")]
        public Presentacion Presentacion { get; set; }
    }
}
