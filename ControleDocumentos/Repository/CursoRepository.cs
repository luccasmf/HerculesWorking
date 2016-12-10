using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;
using System.Reflection;
using ControleDocumentos.Util;

namespace ControleDocumentos.Repository
{
    public class CursoRepository
    {
        DocumentosModel db = new DocumentosModel();

        public List<Curso> GetCursos()
        {
            return db.Curso.ToList();
        }

        public List<Curso> GetCursoByCoordenador(string idCoordenador)
        {
            List<Curso> crs = (from cr in db.Curso
                         join fu in db.Funcionario on cr.IdCoordenador equals fu.IdFuncionario
                         where fu.IdUsuario == idCoordenador
                         select cr).ToList();

            return crs;
        }

        public Funcionario GetCoordenadorByCurso(int idCurso)
        {
            Funcionario func = (from fu in db.Funcionario
                               join cr in db.Curso on fu.IdFuncionario equals cr.IdCoordenador
                               where cr.IdCurso == idCurso
                               select fu).FirstOrDefault();

            return func;
        }

        public List<Curso> GetCursoByArrayId(int[] ids)
        {
            return db.Curso.Where(x => ids.Contains(x.IdCurso)).ToList();
        }

        public Curso GetCursoById(int idCurso)
        {
            return db.Curso.Find(idCurso);
        }

        public List<Curso> GetCursosByAlunoId(int idAluno)
        {
            List<Curso> cursos = (from c in db.Curso
                                  join ac in db.AlunoCurso on c.IdCurso equals ac.IdCurso
                                  where ac.IdAluno == idAluno
                                  select c).ToList();

            return cursos;
        }

        public string PersisteCurso(Curso curso)
        {
            if (curso.IdCurso > 0)
            {
                return ComparaInfos(curso);
            }
            else
            {
                db.Curso.Add(curso);

            }
            if (db.SaveChanges() > 0)
            {
                return "Cadastrado";
            }
            else
            {
                return "Erro";
            }
        }

        public List<Curso> GetCursosByEventoId(int idEvento)
        {
            List<Curso> cursos = (from evento in db.Evento
                                  join curso in db.Curso on (evento.Curso.Select(x => x.IdCurso).FirstOrDefault()) equals curso.IdCurso
                                  where evento.IdEvento == idEvento
                                  select curso).ToList();

            return cursos;
        }

        internal AlunoCurso GetAlunoCurso(int? idAlunoCurso)
        {
            return db.AlunoCurso.Find(idAlunoCurso);
        }

        public bool MatriculaAluno(int idCurso, int idAluno)
        {
            AlunoCurso matricula = new AlunoCurso();
            Curso c = db.Curso.Find(idCurso);

            matricula.IdAluno = idAluno;
            matricula.IdCurso = idCurso;
            matricula.HoraNecessaria = c.HoraComplementar;
            matricula.HoraCompleta = 0;

            db.AlunoCurso.Add(matricula);

            return db.SaveChanges() > 0;
        }        

        private string ComparaInfos(Curso c)
        {
            DocumentosModel db2 = new DocumentosModel();
            Curso cursoOld = db2.Curso.Find(c.IdCurso);

            cursoOld = Utilidades.ComparaValores(cursoOld, c, new string[] { "Nome", "Nível", "HoraComplementar", "IdCoordenador" });

            if (cursoOld == null)
            {
                return "Mantido";
            }
            if (db2.SaveChanges() > 0)
            {
                return "Alterado";
            }
            else
            {
                return "Erro";
            }
        }

        public string DeletaCurso(int idCurso)
        {
            Curso curso = db.Curso.Find(idCurso);

            if(curso.AlunoCurso.Count > 0)
            {
                return "Alunos";
            }
            else
            {
                try
                {
                    db.Curso.Remove(curso);
                    db.SaveChanges();
                    return "Excluido";
                }
                catch (Exception e)
                {
                    return "Erro";
                }
                
            }
        }

        public AlunoCurso GetAlunoCurso(int idAluno, int idCurso)
        {
            return db.AlunoCurso.Where(x => x.IdAluno == idAluno && x.IdCurso == idCurso).FirstOrDefault();
        }

        public AlunoCurso GetAlunoCurso(string idUsuario)
        {
            Aluno al = db.Aluno.Where(x => x.IdUsuario == idUsuario).FirstOrDefault();
            return db.AlunoCurso.Where(x => x.IdAluno == al.IdAluno).FirstOrDefault();
        }
    }

}