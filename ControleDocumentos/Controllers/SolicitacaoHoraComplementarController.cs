using ControleDocumentos.Filter;
using ControleDocumentos.Repository;
using ControleDocumentos.Util;
using ControleDocumentos.Util.Extension;
using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class SolicitacaoHoraComplementarController : BaseController
    {
        // GET: SolicitacaoHoraComplementar
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        SolicitacaoDocumentoRepository solicitacaoRepository = new SolicitacaoDocumentoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();

        public ActionResult Index()
        {
            Usuario user = GetSessionUser();

            PopularDropDowns();
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                List<SolicitacaoDocumento> retorno = solicitacaoRepository.GetSolicitacaoByCoordenador(
                    User.Identity.Name).Where(x =>
                    (x.Status == EnumStatusSolicitacao.pendente ||
                    x.Status == EnumStatusSolicitacao.visualizado) &&
                    x.TipoSolicitacao == EnumTipoSolicitacao.aluno).ToList();

                return View(retorno);
            }
            return View(solicitacaoRepository.GetByFilter(new Models.SolicitacaoDocumentoFilter())
                .Where(x => (x.Status == EnumStatusSolicitacao.pendente || x.Status == EnumStatusSolicitacao.visualizado) 
                && x.TipoSolicitacao == EnumTipoSolicitacao.aluno).ToList());
        }

        public ActionResult CadastrarSolicitacao(int? idSol)
        {
            Usuario user = GetSessionUser();

            SolicitacaoDocumento sol = new SolicitacaoDocumento();

            if (idSol.HasValue)
            {
                if (user.Permissao == EnumPermissaoUsuario.coordenador)
                {
                    var retorno = solicitacaoRepository.GetSolicitacaoByCoordenador(User.Identity.Name).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.aluno).ToList();
                    if (!retorno.Any(x => x.IdSolicitacao == idSol))
                        return PartialView("_UnauthorizedPartial", "Error");
                }

                sol = solicitacaoRepository.GetSolicitacaoById((int)idSol);

                // marca a solicitação como visualizada
                if (sol.Status == EnumStatusSolicitacao.pendente)
                {
                    sol.Status = EnumStatusSolicitacao.visualizado;
                    solicitacaoRepository.PersisteSolicitacao(sol);
                }
            }

            //retorna model
            return PartialView("_CadastroSolicitacao", sol);
        }

        public ActionResult List(Models.SolicitacaoDocumentoFilter filter)
        {
            Usuario user = GetSessionUser();

            List<SolicitacaoDocumento> retorno = new List<SolicitacaoDocumento>();
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                retorno = solicitacaoRepository.GetByFilterCoordenador(filter, User.Identity.Name).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.aluno).ToList();
            }
            else {
                retorno = solicitacaoRepository.GetByFilter(filter).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.aluno).ToList(); ;
            }

            if (filter.ApenasPendentes)
                retorno = retorno.Where(x => x.Status == EnumStatusSolicitacao.pendente || x.Status == EnumStatusSolicitacao.visualizado).ToList();

            return PartialView("_List", retorno);
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
            ViewBag.Status = new SelectList(listStatus, "Value", "Text");
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

        public object AlterarStatus(SolicitacaoDocumento solic, int? Horas)
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

                if (solic.Status == EnumStatusSolicitacao.concluido && !Horas.HasValue)
                {
                    return Json(new { Status = false, Type = "error", Message = "Informe a quantidade de horas." }, JsonRequestBehavior.AllowGet);
                }
                else if (solic.Status == EnumStatusSolicitacao.reprovado && string.IsNullOrEmpty(solic.Observacao))
                {
                    return Json(new { Status = false, Type = "error", Message = "Informe a observação sobre reprovação." }, JsonRequestBehavior.AllowGet);
                }
                var ok = true;

                var sol = solicitacaoRepository.GetSolicitacaoById(solic.IdSolicitacao);
                sol.Status = solic.Status;
                if (sol.Status == EnumStatusSolicitacao.reprovado && !string.IsNullOrEmpty(sol.Documento.CaminhoDocumento))
                {
                    DirDoc.DeletaArquivo(sol.Documento.CaminhoDocumento);
                    sol.Documento.CaminhoDocumento = null;
                }
                if (sol.Status == EnumStatusSolicitacao.reprovado)
                {
                    sol.Observacao = solic.Observacao;
                }
                else if (sol.Status == EnumStatusSolicitacao.concluido)
                {
                    ok = alunoRepository.AdicionaHoras(Horas.Value, sol.IdSolicitacao);
                }

                var msg = "";
                if (ok)
                    msg = solicitacaoRepository.PersisteSolicitacao(sol);
                else
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);

                if (msg != "Erro")
                {
                    try
                    {
                        var acao = sol.Status == EnumStatusSolicitacao.concluido ? "aprovada" :
                            sol.Status == EnumStatusSolicitacao.reprovado ? "reprovada" : "";

                        var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/AlteracaoStatusSolicitacaoHoras.cshtml");
                        string viewCode = System.IO.File.ReadAllText(url);
                        var solicitacaoEmail = solicitacaoRepository.ConverToEmailModel(sol, Url.Action("Login", "Account", null, Request.Url.Scheme));

                        var html = RazorEngine.Razor.Parse(viewCode, solicitacaoEmail);
                        var to = new[] { solicitacaoEmail.EmailAluno };
                        var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                        Email.EnviarEmail(from, to, "Solicitação de horas complementares " + acao, html);
                    }
                    catch (Exception e)
                    {
                    }

                    if (sol.Status == EnumStatusSolicitacao.concluido)
                    {
                        Utilidades.SalvaLog(user, EnumAcao.Aprovar, sol, sol.IdSolicitacao);
                    }
                    else if (sol.Status == EnumStatusSolicitacao.reprovado)
                    {
                        Utilidades.SalvaLog(user, EnumAcao.Reprovar, sol, sol.IdSolicitacao);
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