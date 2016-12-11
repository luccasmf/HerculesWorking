namespace ControleDocumentosLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Documento")]
    public partial class Documento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Documento()
        {
            SolicitacaoDocumento = new HashSet<SolicitacaoDocumento>();
        }

        [Key]
        public int IdDocumento { get; set; }

        public int IdTipoDoc { get; set; }

        [StringLength(200)]
        public string NomeDocumento { get; set; }

        public DateTime Data { get; set; }

        [Column(TypeName = "text")]
        public string CaminhoDocumento { get; set; }

        public int? IdAlunoCurso { get; set; }

        public int? IdEvento { get; set; }

        public virtual AlunoCurso AlunoCurso { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SolicitacaoDocumento> SolicitacaoDocumento { get; set; }

        public virtual Evento Evento { get; set; }

        public virtual TipoDocumento TipoDocumento { get; set; }

        [NotMapped]
        public byte[] arquivo { get; set; }
    }
}
