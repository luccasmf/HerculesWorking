using ControleDocumentos.Repository;
using ControleDocumentos.Util.Extension;
using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Util;
using ControleDocumentos.Filter;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class SolicitacaoDocumentoController : BaseController
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        SolicitacaoDocumentoRepository solicitacaoRepository = new SolicitacaoDocumentoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();

        // GET: SolicitacaoDocumento
        public ActionResult Index()
        {
            Usuario user = GetSessionUser();

            PopularDropDowns();
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                List<SolicitacaoDocumento> retorno = solicitacaoRepository.GetSolicitacaoByCoordenador(User.Identity.Name).Where(x => x.Status == EnumStatusSolicitacao.processando && x.TipoSolicitacao == EnumTipoSolicitacao.secretaria).ToList();
                return View(retorno);
            }
            return View(solicitacaoRepository.GetByFilter(new Models.SolicitacaoDocumentoFilter { IdStatus = (int)EnumStatusSolicitacao.processando }));
        }

        public ActionResult CadastrarSolicitacao(int? idSol)
        {
            PopularDropDownsCadastro();
            SolicitacaoDocumento sol = new SolicitacaoDocumento();
            Usuario user = GetSessionUser();

            if (idSol.HasValue)
            {
                if (user.Permissao == EnumPermissaoUsuario.coordenador)
                {
                    var retorno = solicitacaoRepository.GetSolicitacaoByCoordenador(User.Identity.Name).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.secretaria).ToList();
                    if (!retorno.Any(x => x.IdSolicitacao == idSol))
                        return PartialView("_UnauthorizedPartial", "Error");
                }

                sol = solicitacaoRepository.GetSolicitacaoById((int)idSol);
                PopularDropDownAlunos(sol.AlunoCurso.Curso.IdCurso);
            }
            else
            {
                ViewBag.Alunos = new SelectList(new List<SelectListItem>() { new SelectListItem() {
                    Text ="Selecione um curso",
                    Value =""}
                }, "Value", "Text");
            }

            //retorna model
            return PartialView("_CadastroSolicitacao", sol);
        }

        public ActionResult List(Models.SolicitacaoDocumentoFilter filter)
        {
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
               return PartialView("_List", solicitacaoRepository.GetByFilterCoordenador(filter, User.Identity.Name).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.secretaria).ToList());
            }
            return PartialView("_List", solicitacaoRepository.GetByFilter(filter));
        }

        public ActionResult CarregaModalExclusao(int idSol)
        {
            SolicitacaoDocumento sol = solicitacaoRepository.GetSolicitacaoById(idSol);
            return PartialView("_ExclusaoSolicitacao", sol);
        }

        public ActionResult CarregaModalConfirmacao(EnumStatusSolicitacao novoStatus, int idSol)
        {
            SolicitacaoDocumento sol = new SolicitacaoDocumento { IdSolicitacao = idSol, Status = novoStatus };
            return PartialView("_AlteracaoStatus", sol);
        }

        #region Métodos auxiliares

        private void PopularDropDowns()
        {
            Usuario user = GetSessionUser();

            List<Curso> lstCursos = new List<Curso>();
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                lstCursos = cursoRepository.GetCursoByCoordenador(User.Identity.Name);
            }
            else {
                lstCursos = cursoRepository.GetCursos();
            }

            var listCursosSelectList = lstCursos.Select(item => new SelectListItem
            {
                Value = item.IdCurso.ToString(),
                Text = item.Nome.ToString(),
            });
            ViewBag.Cursos = new SelectList(listCursosSelectList, "Value", "Text");


            var listStatus = Enum.GetValues(typeof(EnumStatusSolicitacao)).
                Cast<EnumStatusSolicitacao>().Select(v => new SelectListItem
                {
                    Text = EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString(),
                }).ToList();
            ViewBag.Status = new SelectList(listStatus, "Value", "Text", ((int)EnumStatusSolicitacao.processando).ToString());
        }

        private void PopularDropDownsCadastro()
        {
            Usuario user = GetSessionUser();

            List<Curso> lstCursos = new List<Curso>();
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                lstCursos = cursoRepository.GetCursoByCoordenador(User.Identity.Name);
            }
            else {
                lstCursos = cursoRepository.GetCursos();
            }

            var listCursosSelectList = lstCursos.Select(item => new SelectListItem
            {
                Value = item.IdCurso.ToString(),
                Text = item.Nome.ToString(),
            });
            ViewBag.Cursos = new SelectList(listCursosSelectList, "Value", "Text");

            var listTiposDoc = tipoDocumentoRepository.listaTipos().Select(item => new SelectListItem
            {
                Value = item.IdTipoDoc.ToString(),
                Text = item.TipoDocumento1.ToString(),
            });
            ViewBag.TiposDocumento = new SelectList(listTiposDoc, "Value", "Text");
        }

        private void PopularDropDownAlunos(int idCurso)
        {
            Usuario user = GetSessionUser();

            var ok = false;
            List<SelectListItem> listAlunos = new List<SelectListItem>();

            // se é coordenador, valida se usuario é coordenador do curso passado no parametro
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                var cursos = cursoRepository.GetCursoByCoordenador(User.Identity.Name);
                if (cursos.Any(x => x.IdCurso == idCurso))
                    ok = true;
            }

            if (ok || user.Permissao != EnumPermissaoUsuario.coordenador)
            {
                listAlunos = alunoRepository.GetAlunoByIdCurso(idCurso).Select(item => new SelectListItem
                {
                    Value = item.IdAluno.ToString(),
                    Text = item.Usuario.Nome.ToString(),
                }).ToList();
            }

            ViewBag.Alunos = new SelectList(listAlunos, "Value", "Text");
        }

        public JsonResult GetAlunosByIdCurso(int idCurso)
        {
            Usuario user = GetSessionUser();

            // se é coordenador, valida se é coodenador do curso passado por parametro
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                var cursos = cursoRepository.GetCursoByCoordenador(User.Identity.Name);
                if (!cursos.Any(x => x.IdCurso == idCurso))
                    return Json(null);
            }

            if (idCurso > 0)
            {
                var lstAlunos = alunoRepository.GetAlunoByIdCurso(idCurso);
                return Json(lstAlunos.Select(x => new { Value = x.IdAluno, Text = x.Usuario.Nome }));
            }
            return Json(null);
        }

        public object SalvarSolicitacao(SolicitacaoDocumento sol)
        {
            Usuario user = GetSessionUser();

            var edit = true;
            sol.Status = sol.IdSolicitacao > 0 ? sol.Status : EnumStatusSolicitacao.pendente;
            sol.DataAbertura = DateTime.Now;
            if (sol.IdSolicitacao == 0)
                sol.IdAlunoCurso = cursoRepository.GetAlunoCurso(sol.AlunoCurso.IdAluno, sol.AlunoCurso.IdCurso).IdAlunoCurso;
            sol.AlunoCurso = null;
            sol.TipoSolicitacao = EnumTipoSolicitacao.secretaria;
            if (sol.IdSolicitacao == 0)
            {
                edit = false;
                Documento d = new Documento();
                d.IdTipoDoc = (int)sol.TipoDocumento;
                d.IdAlunoCurso = sol.IdAlunoCurso;
                d.Data = sol.DataAbertura;
                d.NomeDocumento = "";

                sol.Documento = d;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // valida se o aluno está matriculado no curso que coordena
                    if (user.Permissao == EnumPermissaoUsuario.coordenador)
                    {
                        var cursos = cursoRepository.GetCursoByCoordenador(User.Identity.Name);
                        AlunoCurso ac = cursoRepository.GetAlunoCurso(sol.IdAlunoCurso);
                        if (!cursos.Contains(ac.Curso))
                            return Json(new { Status = false, Type = "error", Message = "Não autorizado!" }, JsonRequestBehavior.AllowGet);
                    }

                    string msg = solicitacaoRepository.PersisteSolicitacao(sol);

                    if (msg != "Erro")
                    {
                        try
                        {
                            DocumentosModel db2 = new DocumentosModel();
                            var solicitacao = db2.SolicitacaoDocumento.Find(sol.IdSolicitacao);
                            var solicitacaoEmail = solicitacaoRepository.ConverToEmailModel(solicitacao, Url.Action("Login", "Account", null, Request.Url.Scheme));

                            if (edit)
                            {
                                var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/AlteracaoSolicitacaoDocumento.cshtml");
                                string viewCode = System.IO.File.ReadAllText(url);

                                var html = RazorEngine.Razor.Parse(viewCode, solicitacaoEmail);
                                if (!string.IsNullOrEmpty(solicitacaoEmail.EmailAluno))
                                {
                                    var to = new[] { solicitacaoEmail.EmailAluno };
                                    var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                                    Email.EnviarEmail(from, to, "Alteração em solicitação de documento - " + solicitacaoEmail.NomeTipoDocumento, html);
                                }
                            }
                            else {
                                var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/NovaSolicitacaoDocumento.cshtml");
                                string viewCode = System.IO.File.ReadAllText(url);

                                var html = RazorEngine.Razor.Parse(viewCode, solicitacaoEmail);
                                if (!string.IsNullOrEmpty(solicitacaoEmail.EmailAluno))
                                {
                                    var to = new[] { solicitacaoEmail.EmailAluno };
                                    var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                                    Email.EnviarEmail(from, to, "Nova solicitação de documento - " + solicitacaoEmail.NomeTipoDocumento, html);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                        }

                        Utilidades.SalvaLog(user, EnumAcao.Persistir, sol, (sol.IdSolicitacao > 0 ? (int?)sol.IdSolicitacao : null));
                        return Json(new { Status = true, Type = "success", Message = "Solicitação salva com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                    }
                    else
                        return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                }
            }
            else {
                return Json(new { Status = false, Type = "error", Message = "Campos inválidos" }, JsonRequestBehavior.AllowGet);
            }
        }

        public object ExcluirSolicitacao(SolicitacaoDocumento sol)
        {
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                var retorno = solicitacaoRepository.GetSolicitacaoByCoordenador(User.Identity.Name);
                if (!retorno.Any(x => x.IdSolicitacao == sol.IdSolicitacao))
                    return Json(new { Status = false, Type = "error", Message = "Não autorizado!" }, JsonRequestBehavior.AllowGet);
            }

            var s = solicitacaoRepository.GetSolicitacaoById(sol.IdSolicitacao);
            if (s.Status == EnumStatusSolicitacao.pendente) //regra q soh deleta se status for pendente
            {
                if (solicitacaoRepository.DeletaArquivo(s))
                {
                    Utilidades.SalvaLog(user, EnumAcao.Excluir, s, s.IdSolicitacao);
                    return Json(new { Status = true, Type = "success", Message = "Solicitação deletada com sucesso!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = false, Type = "error", Message = "Só é possível realizar exclusão de solicitações pendentes." }, JsonRequestBehavior.AllowGet);
        }

        public object AlterarStatus(SolicitacaoDocumento solic)
        {
            Usuario user = GetSessionUser();

            try
            {
                if (user.Permissao == EnumPermissaoUsuario.coordenador)
                {
                    var retorno = solicitacaoRepository.GetSolicitacaoByCoordenador(User.Identity.Name);
                    if (!retorno.Any(x => x.IdSolicitacao == solic.IdSolicitacao))
                        return Json(new { Status = false, Type = "error", Message = "Não autorizado!" }, JsonRequestBehavior.AllowGet);
                }

                var sol = solicitacaoRepository.GetSolicitacaoById(solic.IdSolicitacao);
                sol.Status = solic.Status;
                if (sol.Status == EnumStatusSolicitacao.pendente && !string.IsNullOrEmpty(sol.Documento.CaminhoDocumento))
                {
                    DirDoc.DeletaArquivo(sol.Documento.CaminhoDocumento);
                    sol.Documento.CaminhoDocumento = null;
                }
                string msg = solicitacaoRepository.PersisteSolicitacao(sol);

                if (msg != "Erro")
                {
                    try
                    {
                        var acao = sol.Status == EnumStatusSolicitacao.cancelado ? "cancelada" :
                            sol.Status == EnumStatusSolicitacao.concluido ? "aprovada" :
                            sol.Status == EnumStatusSolicitacao.pendente ? "reprovada" : "";
                        var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/AlteracaoStatusSolicitacaoDocumento.cshtml");
                        string viewCode = System.IO.File.ReadAllText(url);
                        var solicitacaoEmail = solicitacaoRepository.ConverToEmailModel(sol, Url.Action("Login", "Account", null, Request.Url.Scheme));

                        var html = RazorEngine.Razor.Parse(viewCode, solicitacaoEmail);
                        if (!string.IsNullOrEmpty(solicitacaoEmail.EmailFuncionario))
                        {
                            var to = new[] { solicitacaoEmail.EmailFuncionario };
                            var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                            Email.EnviarEmail(from, to, "Solicitação de documento " + acao, html);
                        }
                    }
                    catch (Exception e)
                    {
                    }

                    if (sol.Status == EnumStatusSolicitacao.concluido)
                    {
                        Utilidades.SalvaLog(user, EnumAcao.Aprovar, sol, sol.IdSolicitacao);
                    }
                    else if (sol.Status == EnumStatusSolicitacao.pendente)
                    {
                        Utilidades.SalvaLog(user, EnumAcao.Reprovar, sol, sol.IdSolicitacao);
                    }
                    else if (sol.Status == EnumStatusSolicitacao.cancelado)
                    {
                        Utilidades.SalvaLog(user, EnumAcao.Cancelar, sol, sol.IdSolicitacao);
                    }

                    return Json(new { Status = true, Type = "success", Message = "Solicitação salva com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}