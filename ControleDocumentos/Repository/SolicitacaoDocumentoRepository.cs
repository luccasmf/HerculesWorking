using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;
using ControleDocumentos.Util;
using System.IO;
using System.Web.Script.Serialization;
using ControleDocumentos.Models;

namespace ControleDocumentos.Repository
{
    public class SolicitacaoDocumentoRepository
    {
        DocumentosModel db = new DocumentosModel();
        LogsRepository logsRepository = new LogsRepository();

        public SolicitacaoDocumento GetSolicitacaoById(int id)
        {
            return db.SolicitacaoDocumento.Find(id);
        }

        public List<SolicitacaoDocumento> GetAll()
        {
            return db.SolicitacaoDocumento.ToList();
        }

        public string PersisteSolicitacao(SolicitacaoDocumento sol)
        {
            if (sol.IdSolicitacao > 0)
            {
                return ComparaInfos(sol);
            }
            else
            {
                db.SolicitacaoDocumento.Add(sol);

            }
            try
            {
                if (db.SaveChanges() > 0)
                {
                    return "Cadastrado";
                }
                else
                {
                    return "Erro";
                }
            }
            catch (Exception e)
            {
                return "Erro";
            }
            
        }

        public List<SolicitacaoDocumento> GetMinhaSolicitacao(string idUsuario)
        {
            Aluno aluno = db.Aluno.Where(x => x.IdUsuario == idUsuario).FirstOrDefault();


            return GetSolicitacaoByAluno(aluno).Where(x=>x.TipoSolicitacao == EnumTipoSolicitacao.aluno).ToList();
        }

        public List<SolicitacaoDocumento> GetSolicitacaoByAluno(Aluno aluno)
        {
            List<SolicitacaoDocumento> solicitacoes = db.SolicitacaoDocumento.Where(x => x.AlunoCurso.IdAluno == aluno.IdAluno).ToList();

            return solicitacoes;
        }

        public List<SolicitacaoDocumento> GetAguardandoAtendimentoAluno(int idUsuario)
        {
            List<SolicitacaoDocumento> solicitacoesDocumento = new List<SolicitacaoDocumento>();

            solicitacoesDocumento = db.SolicitacaoDocumento.Where(x => x.AlunoCurso.Aluno.IdAluno == idUsuario &&
           (x.Status == EnumStatusSolicitacao.pendente ||
           x.Status == EnumStatusSolicitacao.visualizado)).ToList();

            return solicitacoesDocumento;
        }

        public List<SolicitacaoDocumento> GetByFilterAluno(SolicitacaoDocumentoFilter filter, string idUsuario)
        {
            Aluno al = db.Aluno.Where(x => x.IdUsuario == idUsuario).FirstOrDefault();

            List<SolicitacaoDocumento> solicitacoes = GetByFilter(filter).Where(x => x.AlunoCurso.IdAluno == al.IdAluno).ToList();

            return solicitacoes;
        }

        public List<SolicitacaoDocumento> GetTodosAluno(int idAluno)
        {
            List<SolicitacaoDocumento> solicitacoesDocumento = new List<SolicitacaoDocumento>();
            solicitacoesDocumento = db.SolicitacaoDocumento.Where(x => x.AlunoCurso.IdAluno == idAluno).ToList();

            return solicitacoesDocumento;
        }

        public List<SolicitacaoDocumento> GetByFilter(SolicitacaoDocumentoFilter filter)
        {
            List<SolicitacaoDocumento> solicitacosDocumento = db.SolicitacaoDocumento.ToList();

            if (filter.IdCurso != null && filter.IdStatus != null) //se nenhum for nulo faz esse
            {
                solicitacosDocumento = solicitacosDocumento.Where(x => x.AlunoCurso.IdCurso == filter.IdCurso && x.Status == (EnumStatusSolicitacao)filter.IdStatus).ToList();

            }
            else if (filter.IdCurso != null) //se status for nulo faz esse
            {
                solicitacosDocumento = solicitacosDocumento.Where(x => x.AlunoCurso.IdCurso == filter.IdCurso).ToList();
            }
            else if (filter.IdStatus != null) //se curso for nulo faz esse
            {
                solicitacosDocumento = solicitacosDocumento.Where(x => x.Status == (EnumStatusSolicitacao)filter.IdStatus).ToList();
            }            

            return solicitacosDocumento;
        }

        public string AlteraDocumento(SolicitacaoDocumento sol)
        {
            SolicitacaoDocumento solic = db.SolicitacaoDocumento.Find(sol.IdSolicitacao);
            try
            {
                File.Delete(solic.Documento.CaminhoDocumento);

            }
            catch
            {

            }
            solic.Documento.arquivo = sol.Documento.arquivo;
            solic.Documento.NomeDocumento = sol.Documento.NomeDocumento;
            solic.DataAbertura = DateTime.Now;
            solic.DataLimite = solic.DataAbertura.AddDays(7);
            string msgDoc = DirDoc.SalvaArquivo(solic.Documento);

            if(db.SaveChanges()>0)
            {
                return "Alterado";
            }
            return "Erro";
        }

        public bool DeletaArquivo(SolicitacaoDocumento sol)
        {
            if (sol.Documento != null && sol.Documento.CaminhoDocumento != null)
            {
                File.Delete(sol.Documento.CaminhoDocumento);
                sol.Documento.CaminhoDocumento = null;
            }
            db.SolicitacaoDocumento.Remove(sol);
            return db.SaveChanges() > 0;
        }

        #region alterarStatus: método pra caso de erro do outro
        //public string AlteraStatus(SolicitacaoDocumento sol, EnumStatusSolicitacao e)
        //{
        //    SolicitacaoDocumento soli = db.SolicitacaoDocumento.Find(sol.IdSolicitacao);
        //    soli.Status = e;

        //    if(db.SaveChanges()>0)
        //    {
        //        return "Sucesso";
        //    }
        //    else
        //    {
        //        return "Erro";
        //    }
        //}
        #endregion

        private string ComparaInfos(SolicitacaoDocumento sol)
        {
            DocumentosModel db2 = new DocumentosModel();
            SolicitacaoDocumento solicitacaoOld = (from sd in db2.SolicitacaoDocumento where sd.IdSolicitacao == sol.IdSolicitacao select sd).FirstOrDefault();
            //db.Entry(solicitacaoOld).Reload();
            solicitacaoOld = Utilidades.ComparaValores(solicitacaoOld, sol, new string[] { "DataLimite", "DataAtendimento", "Status", "IdFuncionario", "Observacao" });

            if (solicitacaoOld.Status == EnumStatusSolicitacao.cancelado)
            {
                db2.Documento.Remove(solicitacaoOld.Documento);
            }

            if (solicitacaoOld == null)
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

        public Models.SolicitacaoDocumentoEmail ConverToEmailModel(SolicitacaoDocumento sol, string url)
        {
            return new Models.SolicitacaoDocumentoEmail
            {
                UrlSistema = url,
                DataAtendimento = sol.DataAtendimento,
                DataLimite = sol.DataLimite,
                NomeAluno = sol.AlunoCurso != null && sol.AlunoCurso.Aluno != null && sol.AlunoCurso.Aluno.Usuario != null ?
                    sol.AlunoCurso.Aluno.Usuario.Nome : "",
                EmailAluno = sol.AlunoCurso != null && sol.AlunoCurso.Aluno != null && sol.AlunoCurso.Aluno.Usuario != null ?
                    sol.AlunoCurso.Aluno.Usuario.E_mail : "",
                EmailFuncionario = sol.Funcionario != null && sol.Funcionario.Usuario != null ? sol.Funcionario.Usuario.E_mail : "",
                NomeCurso = sol.AlunoCurso != null && sol.AlunoCurso.Curso != null ? sol.AlunoCurso.Curso.Nome : "",
                NomeTipoDocumento = sol.Documento != null && sol.Documento.TipoDocumento != null ? sol.Documento.TipoDocumento.TipoDocumento1 : "",
                Observacao = sol.Observacao,
                Status = sol.Status
            };
        }

        public List<SolicitacaoDocumento> GetSolicitacaoByCursoId(int idCurso)
        {
            List<SolicitacaoDocumento> solics = (from so in db.SolicitacaoDocumento
                                                 join ac in db.AlunoCurso on so.IdAlunoCurso equals ac.IdAlunoCurso
                                                 join cu in db.Curso on ac.IdCurso equals cu.IdCurso
                                                 where cu.IdCurso == idCurso
                                                 select so).ToList();


            //db.SolicitacaoDocumento.Where(x => x.AlunoCurso.Curso.IdCurso == idCurso).ToList();

            return solics;
        }

        public List<SolicitacaoDocumento> GetSolicitacaoByCoordenador(string idCoord)
        {
            List<SolicitacaoDocumento> solics = (from so in db.SolicitacaoDocumento
                                                 join ac in db.AlunoCurso on so.IdAlunoCurso equals ac.IdAlunoCurso
                                                 join cu in db.Curso on ac.IdCurso equals cu.IdCurso
                                                 join fu in db.Funcionario on cu.IdCoordenador equals fu.IdFuncionario
                                                 where fu.IdUsuario == idCoord
                                                 select so).ToList();

            return solics;
        }

        public List<SolicitacaoDocumento> GetByFilterCoordenador(SolicitacaoDocumentoFilter filter, string idCoord)
        {
            //List<SolicitacaoDocumento> solics = (from so in db.SolicitacaoDocumento
            //                                     join ac in db.AlunoCurso on so.IdAlunoCurso equals ac.IdAlunoCurso
            //                                     join cu in db.Curso on ac.IdCurso equals cu.IdCurso
            //                                     join fu in db.Funcionario on cu.IdCoordenador equals fu.IdFuncionario
            //                                     where fu.IdUsuario == idCoord
            //                                     select so).Where(x => x.Status == (EnumStatusSolicitacao)filter.IdStatus).ToList();

            Funcionario f = db.Funcionario.Where(x => x.IdUsuario == idCoord).FirstOrDefault();
            List<int> cursos = new List<int>();

            foreach(Curso c in f.Curso)
            {
                cursos.Add(c.IdCurso);
            }
            List<SolicitacaoDocumento> solics = GetByFilter(filter).Where(x => cursos.Contains(x.AlunoCurso.IdCurso)).ToList();

            return solics;
        }
    }
}