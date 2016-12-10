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
    public class HoraComplementarController : BaseController
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        SolicitacaoDocumentoRepository solicitacaoRepository = new SolicitacaoDocumentoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();
        UsuarioRepository usuarioRepository = new UsuarioRepository();

        // GET: HoraComplementar
        public ActionResult Index()
        {            
            PopularDropDowns();

            AlunoCurso al = cursoRepository.GetAlunoCurso(User.Identity.Name);

            ViewBag.HrsComputadas = al.HoraCompleta.HasValue && al.HoraCompleta.Value > 0 ? al.HoraCompleta.Value : 0;
            ViewBag.HrsNecessarias = al.HoraNecessaria;

            List<SolicitacaoDocumento> retorno = solicitacaoRepository.GetMinhaSolicitacao(User.Identity.Name).ToList();
            return View(retorno);
        }

        public ActionResult CadastrarSolicitacao(int? idSol)
        {
            SolicitacaoDocumento sol = new SolicitacaoDocumento();

            if (idSol.HasValue)
            {
                sol = solicitacaoRepository.GetSolicitacaoById((int)idSol);
            }
            return PartialView("_CadastroSolicitacao", sol);
        }

        public ActionResult List(Models.SolicitacaoDocumentoFilter filter)
        {
            var list = solicitacaoRepository.GetByFilterAluno(filter, User.Identity.Name).Where(x => x.TipoSolicitacao == EnumTipoSolicitacao.aluno);
            return PartialView("_List", list.ToList());
        }

        public ActionResult CarregaModalExclusao(int idSol)
        {
            SolicitacaoDocumento sol = solicitacaoRepository.GetSolicitacaoById(idSol);
            return PartialView("_ExclusaoSolicitacao", sol);
        }

        public ActionResult CarregaModalConfirmacao(EnumStatusSolicitacao novoStatus, int idSol)
        {
            SolicitacaoDocumento sol = new SolicitacaoDocumento { IdSolicitacao = idSol, Status = novoStatus };
            return PartialView("_CancelamentoSolicitacao", sol);
        }

        public object SalvarSolicitacao(SolicitacaoDocumento sol, HttpPostedFileBase uploadFile)
        {
            Usuario user = GetSessionUser();

            string msg = "Erro";
            if (uploadFile == null)
                return Json(new { Status = false, Type = "error", Message = "Selecione um documento" }, JsonRequestBehavior.AllowGet);

            try
            {
                var edit = true;
                sol.Status = sol.IdSolicitacao > 0 ? sol.Status : EnumStatusSolicitacao.pendente;
                sol.DataAbertura = DateTime.Now;
                AlunoCurso al = new AlunoCurso();

                if (sol.IdSolicitacao == 0)
                {
                    al = cursoRepository.GetAlunoCurso(User.Identity.Name);

                    sol.IdAlunoCurso = al.IdAlunoCurso;
                    sol.TipoSolicitacao = EnumTipoSolicitacao.aluno;

                    edit = false;

                    sol.Documento = new Documento();
                    sol.Documento.arquivo = DirDoc.converterFileToArray(uploadFile);
                    sol.Documento.NomeDocumento = uploadFile.FileName;
                    sol.Documento.IdAlunoCurso = sol.IdAlunoCurso;

                    sol.Documento.IdTipoDoc = tipoDocumentoRepository.GetTipoDoc("certificado").IdTipoDoc;

                    string msgDoc = DirDoc.SalvaArquivo(sol.Documento);

                    sol.DataLimite = sol.DataAbertura.AddDays(7);
                    msg = solicitacaoRepository.PersisteSolicitacao(sol);

                }
                else
                {
                    sol.Documento = new Documento();
                    sol.Documento.arquivo = DirDoc.converterFileToArray(uploadFile);
                    sol.Documento.NomeDocumento = uploadFile.FileName;

                    msg = solicitacaoRepository.AlteraDocumento(sol);
                }


                if (msg != "Erro")
                {
                    if (!edit)
                    {
                        try
                        {
                            sol.AlunoCurso = al;
                            var solicitacaoEmail = solicitacaoRepository.ConverToEmailModel(sol, Url.Action("Login", "Account", null, Request.Url.Scheme));

                            var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/NovaSolicitacaoHoras.cshtml");
                            string viewCode = System.IO.File.ReadAllText(url);

                            var html = RazorEngine.Razor.Parse(viewCode, solicitacaoEmail);

                            var toEmail = new List<Usuario>();
                            var coord = cursoRepository.GetCoordenadorByCurso(al.IdCurso);
                            toEmail = usuarioRepository.GetUsuariosSecretaria();
                            if (coord != null && coord.Usuario != null)
                            {
                                toEmail.Add(coord.Usuario);
                            }
                            if (toEmail.Any(x => !string.IsNullOrEmpty(x.E_mail)))
                            {
                                var to = toEmail.Where(x => !string.IsNullOrEmpty(x.E_mail)).Select(x => x.E_mail).ToArray();
                                var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                                Email.EnviarEmail(from, to, "Nova solicitação de horas complementares", html);
                            }
                        }
                        catch (Exception e)
                        {
                        }
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


        public object CancelarSolicitacao(SolicitacaoDocumento solic)
        {
            Usuario user = GetSessionUser();
            try
            {
                var sol = solicitacaoRepository.GetSolicitacaoById(solic.IdSolicitacao);
                sol.Status = EnumStatusSolicitacao.cancelado;
                if (sol.Status == EnumStatusSolicitacao.cancelado && !string.IsNullOrEmpty(sol.Documento.CaminhoDocumento))
                {
                    DirDoc.DeletaArquivo(sol.Documento.CaminhoDocumento);
                    sol.Documento.CaminhoDocumento = null;
                }
                string msg = solicitacaoRepository.PersisteSolicitacao(sol);

                if (msg != "Erro")
                {
                    Utilidades.SalvaLog(user, EnumAcao.Cancelar, sol, sol.IdSolicitacao);

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

        private void PopularDropDowns()
        {
            var listStatus = Enum.GetValues(typeof(EnumStatusSolicitacao)).
                Cast<EnumStatusSolicitacao>().Select(v => new SelectListItem
                {
                    Text = EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString(),
                }).ToList();
            ViewBag.Status = new SelectList(listStatus, "Value", "Text");
        }

    }
}