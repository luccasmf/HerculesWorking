namespace ControleDocumentosLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SolicitacaoDocumento")]
    public partial class SolicitacaoDocumento
    {
        [Key]
        public int IdSolicitacao { get; set; }

        public EnumTipoSolicitacao TipoSolicitacao { get; set; }

        public DateTime DataAbertura { get; set; }

        public DateTime DataLimite { get; set; }

        public DateTime? DataAtendimento { get; set; }

        public EnumStatusSolicitacao Status { get; set; }

        public int? IdDocumento { get; set; }

        public int? IdFuncionario { get; set; }

        public int? IdAlunoCurso { get; set; }

        public int? TipoDocumento { get; set; }

        [Column(TypeName = "text")]
        public string Observacao { get; set; }

        public virtual AlunoCurso AlunoCurso { get; set; }

        public virtual Documento Documento { get; set; }

        public virtual Funcionario Funcionario { get; set; }
    }
}
