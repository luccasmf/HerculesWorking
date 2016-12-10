namespace ControleDocumentosLibrary
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DocumentosModel : DbContext
    {
        public DocumentosModel()
            : base("name=DocumentosModel")
        {
        }

        public virtual DbSet<Aluno> Aluno { get; set; }
        public virtual DbSet<AlunoCurso> AlunoCurso { get; set; }
        public virtual DbSet<AlunoEvento> AlunoEvento { get; set; }
        public virtual DbSet<Curso> Curso { get; set; }
        public virtual DbSet<Documento> Documento { get; set; }
        public virtual DbSet<Evento> Evento { get; set; }
        public virtual DbSet<Funcionario> Funcionario { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }
        public virtual DbSet<SolicitacaoDocumento> SolicitacaoDocumento { get; set; }
        public virtual DbSet<TipoDocumento> TipoDocumento { get; set; }
        public virtual DbSet<Usuario> Usuario { get; set; }
        public virtual DbSet<Presenca> Presenca { get; set; }
        public virtual DbSet<Chamada> Chamada { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Aluno>()
                .Property(e => e.IdUsuario)
                .IsUnicode(false);

            modelBuilder.Entity<Aluno>()
                .HasMany(e => e.AlunoEvento)
                .WithRequired(e => e.Aluno)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Aluno>()
                .HasMany(e => e.AlunoCurso)
                .WithRequired(e => e.Aluno)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AlunoEvento>()
                .HasMany(e => e.Presenca)
                .WithRequired(e => e.AlunoEvento)
                .HasForeignKey(e => new { e.IdAluno, e.IdEvento })
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Curso>()
                .Property(e => e.Nome)
                .IsUnicode(false);

            modelBuilder.Entity<Curso>()
                .HasMany(e => e.AlunoCurso)
                .WithRequired(e => e.Curso)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Curso>()
                .HasMany(e => e.Evento)
                .WithMany(e => e.Curso)
                .Map(m => m.ToTable("CursoEvento").MapLeftKey("IdCurso").MapRightKey("IdEvento"));

            modelBuilder.Entity<Documento>()
                .Property(e => e.NomeDocumento)
                .IsUnicode(false);

            modelBuilder.Entity<Documento>()
                .Property(e => e.CaminhoDocumento)
                .IsUnicode(false);

            modelBuilder.Entity<Evento>()
                .Property(e => e.NomeEvento)
                .IsUnicode(false);

            modelBuilder.Entity<Evento>()
                .Property(e => e.Local)
                .IsUnicode(false);

            modelBuilder.Entity<Evento>()
                .Property(e => e.Observacao)
                .IsUnicode(false);

            modelBuilder.Entity<Evento>()
                .HasMany(e => e.AlunoEvento)
                .WithRequired(e => e.Evento)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Evento>()
                .HasMany(e => e.Chamada)
                .WithRequired(e => e.Evento)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Funcionario>()
                .Property(e => e.IdUsuario)
                .IsUnicode(false);

            modelBuilder.Entity<Funcionario>()
                .HasMany(e => e.Curso)
                .WithRequired(e => e.Funcionario)
                .HasForeignKey(e => e.IdCoordenador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Funcionario>()
                .HasMany(e => e.Evento)
                .WithOptional(e => e.Funcionario)
                .HasForeignKey(e => e.IdFuncionarioCriador);

            modelBuilder.Entity<Funcionario>()
                .HasMany(e => e.Presenca)
                .WithRequired(e => e.Funcionario)
                .HasForeignKey(e => e.IdProfessor)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Logs>()
                .Property(e => e.IdUsuario)
                .IsUnicode(false);

            modelBuilder.Entity<Logs>()
                .Property(e => e.EstadoAnterior)
                .IsUnicode(false);

            modelBuilder.Entity<SolicitacaoDocumento>()
                .Property(e => e.Observacao)
                .IsUnicode(false);

            modelBuilder.Entity<TipoDocumento>()
                .Property(e => e.TipoDocumento1)
                .IsUnicode(false);

            modelBuilder.Entity<TipoDocumento>()
                .HasMany(e => e.Documento)
                .WithRequired(e => e.TipoDocumento)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.IdUsuario)
                .IsUnicode(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.Nome)
                .IsUnicode(false);

            modelBuilder.Entity<Usuario>()
                .Property(e => e.E_mail)
                .IsUnicode(false);

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.Aluno)
                .WithRequired(e => e.Usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Usuario>()
                .HasMany(e => e.Funcionario)
                .WithRequired(e => e.Usuario)
                .WillCascadeOnDelete(false);
        }
    }
}

