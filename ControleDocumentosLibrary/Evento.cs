namespace ControleDocumentosLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Evento")]
    public partial class Evento
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Evento()
        {
            AlunoEvento = new HashSet<AlunoEvento>();
            Documento = new HashSet<Documento>();
            Curso = new HashSet<Curso>();
        }

        [Key]
        public int IdEvento { get; set; }

        [Required]
        [StringLength(30)]
        public string NomeEvento { get; set; }

        public int Vagas { get; set; }

        public int VagasPreenchidas { get; set; }

        public int CargaHoraria { get; set; }

        public int? PresencaNecessaria { get; set; }

        public int? IdFuncionarioCriador { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public EnumStatusEvento Status { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string Local { get; set; }

        [Column(TypeName = "text")]
        public string Observacao { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AlunoEvento> AlunoEvento { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Chamada> Chamada { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Documento> Documento { get; set; }

        public virtual Funcionario Funcionario { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Curso> Curso { get; set; }

        [NotMapped]
        public List<int> SelectedCursos { get; set; }
    }
}
