namespace ControleDocumentosLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Presenca")]
    public partial class Presenca
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdAluno { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdEvento { get; set; }

        [Key]
        [Column(Order = 2)]
        public DateTime Data { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IdProfessor { get; set; }

        public int? IdChamada { get; set; }

        public virtual AlunoEvento AlunoEvento { get; set; }

        public virtual Chamada Chamada { get; set; }

        public virtual Funcionario Funcionario { get; set; }
    }
}
