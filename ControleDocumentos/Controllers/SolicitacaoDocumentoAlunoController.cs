using ControleDocumentos.Filter;
using ControleDocumentos.Repository;
using ControleDocumentos.Util;
using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS")]
    public class SolicitacaoDocumentoAlunoController : BaseController
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        SolicitacaoDocumentoRepository solicitacaoRepository = new SolicitacaoDocumentoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();

        // GET: SolicitacaoDocumentoAluno
        public ActionResult Index()
        {
            Aluno aluno = alunoRepository.GetAlunoByIdUsuario(User.Identity.Name);
            if (aluno == null) {
                return RedirectToAction("Unauthorized", "Error");
            }
            var id = aluno.IdAluno;
            return View(solicitacaoRepository.GetAguardandoAtendimentoAluno(id).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.secretaria).ToList());
        }

        public ActionResult CarregarSolicitacao(int? idSol)
        {
            SolicitacaoDocumento sol = new SolicitacaoDocumento();

            if (idSol.HasValue)
            {
                sol = solicitacaoRepository.GetSolicitacaoById((int)idSol);

                if (User.Identity.Name == sol.AlunoCurso.Aluno.IdUsuario)
                {
                    // marca a solicitação como visualizada
                    if (sol.Status == EnumStatusSolicitacao.pendente)
                    {
                        sol.Status = EnumStatusSolicitacao.visualizado;
                        solicitacaoRepository.PersisteSolicitacao(sol);
                    }
                }
                else
                    return null;
            }

            return PartialView("_VisualizacaoSolicitacao", sol);
        }

        public ActionResult List(bool apenasPendentes)
        {
            Aluno aluno = alunoRepository.GetAlunoByIdUsuario(User.Identity.Name);
            var id = aluno.IdAluno;

            if (!apenasPendentes)
            {
                return PartialView("_List", solicitacaoRepository.GetTodosAluno(id).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.secretaria).ToList());
            }
            else {
                return PartialView("_List", solicitacaoRepository.GetAguardandoAtendimentoAluno(id).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.secretaria).ToList());
            }
        }

        #region Métodos auxiliares

        public object EnviarDocumento(SolicitacaoDocumento sol, HttpPostedFileBase uploadFile)
        {
            Usuario user = GetSessionUser();
            try
            {
                if (uploadFile == null)
                    return Json(new { Status = false, Type = "error", Message = "Selecione um documento" }, JsonRequestBehavior.AllowGet);


                var solicitacao = solicitacaoRepository.GetSolicitacaoById(sol.IdSolicitacao);

                if (solicitacao.AlunoCurso.Aluno.Usuario.IdUsuario == User.Identity.Name)
                {
                    solicitacao.Status = EnumStatusSolicitacao.processando;
                    solicitacao.DataAtendimento = DateTime.Now;

                    solicitacao.Documento.arquivo = DirDoc.converterFileToArray(uploadFile);
                    solicitacao.Documento.NomeDocumento = uploadFile.FileName;

                    string msgDoc = DirDoc.SalvaArquivo(solicitacao.Documento);
                    switch (msgDoc)
                    {
                        case "Falha ao persistir":
                            return Json(new { Status = false, Type = "error", Message = msgDoc }, JsonRequestBehavior.AllowGet);
                        case "Falha ao criptografar":
                            return Json(new { Status = false, Type = "error", Message = msgDoc }, JsonRequestBehavior.AllowGet);
                        default:
                            string msg = solicitacaoRepository.PersisteSolicitacao(solicitacao);

                            if (msg != "Erro")
                            {
                                try
                                {
                                    var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/SolicitacaoDocumentoAtendida.cshtml");
                                    string viewCode = System.IO.File.ReadAllText(url);

                                    var solicitacaoEmail = solicitacaoRepository.ConverToEmailModel(sol, Url.Action("Login", "Account", null, Request.Url.Scheme));
                                    var html = RazorEngine.Razor.Parse(viewCode, solicitacaoEmail);

                                    if (!string.IsNullOrEmpty(solicitacaoEmail.EmailFuncionario))
                                    {
                                        var to = new[] { solicitacaoEmail.EmailFuncionario };
                                        var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();

                                        Email.EnviarEmail(from, to, string.Format("Solicitação de documento atendida - {0} - {1}", solicitacaoEmail.NomeTipoDocumento, solicitacaoEmail.NomeAluno), html);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                Utilidades.SalvaLog(user, EnumAcao.Persistir, solicitacao, solicitacao.IdSolicitacao);
                                return Json(new { Status = true, Type = "success", Message = "Solicitação salva com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                            }
                            else
                                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                    return Json(new { Status = false, Type = "error", Message = "Não autorizado!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}