namespace ControleDocumentosLibrary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Chamada")]
    public partial class Chamada
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Chamada()
        {
            Presenca = new HashSet<Presenca>();
        }

        [Key]
        public int IdChamada { get; set; }

        public int IdEvento { get; set; }

        public DateTime Data { get; set; }

        public virtual Evento Evento { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Presenca> Presenca { get; set; }
    }
}
