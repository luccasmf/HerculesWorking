namespace ControleDocumentosLibrary
{ 
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Logs")]
    public partial class Logs
    {
        [Key]
        public int IdLog { get; set; }

        [StringLength(20)]
        public string IdUsuario { get; set; }

        public DateTime Data { get; set; }

        public EnumAcao Acao { get; set; }

        public EnumTipoObjeto TipoObjeto { get; set; }

        public int? IdObjeto { get; set; }

        [Column(TypeName = "text")]
        public string EstadoAnterior { get; set; }

        public virtual Usuario Usuario { get; set; }
    }
}
