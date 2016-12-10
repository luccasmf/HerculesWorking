namespace ControleDocumentosLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AlunoCurso")]
    public partial class AlunoCurso
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AlunoCurso()
        {
            Documento = new HashSet<Documento>();
            SolicitacaoDocumento = new HashSet<SolicitacaoDocumento>();
        }

        [Key]
        public int IdAlunoCurso { get; set; }

        [Required]
        public int IdAluno { get; set; }

        [Required]
        public int IdCurso { get; set; }

        public int HoraNecessaria { get; set; }

        public int? HoraCompleta { get; set; }

        public virtual Aluno Aluno { get; set; }

        public virtual Curso Curso { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Documento> Documento { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SolicitacaoDocumento> SolicitacaoDocumento { get; set; }
    }
}
