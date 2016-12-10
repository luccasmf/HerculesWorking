using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Repository;
using ControleDocumentosLibrary;
using ControleDocumentos.Util;
using ControleDocumentos.Models;
using ControleDocumentos.Util.Extension;
using ControleDocumentos.Filter;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class EventoController : BaseController
    {
        EventoRepository eventoRepository = new EventoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        UsuarioRepository usuarioRepository = new UsuarioRepository();
        AlunoRepository alunoRepository = new AlunoRepository();

        #region Lado do Coordenador/Secretaria

        public ActionResult Index()
        {
            Usuario user = GetSessionUser();

            eventoRepository.AtualizaStatus();
            PopularDropDownsFiltro();
            List<Evento> eventos;
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                eventos = eventoRepository.GetByFilterCoord(user.IdUsuario, new EventoFilter());

            }
            else
            {
                eventos = eventoRepository.GetByFilter(new EventoFilter());
            }

            return View(eventos);
        }

        [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
        public ActionResult CadastrarEvento(int? idEvento)
        {
            PopularDropDownsCadastro(idEvento);
            Evento evento = new Evento();

            if (idEvento.HasValue)
            {
                evento = eventoRepository.GetEventoById((int)idEvento);
                evento.SelectedCursos = evento.Curso.Select(x => x.IdCurso).ToList();
            }
            //retorna model
            return PartialView("_CadastroEvento", evento);
        }

        public ActionResult List(EventoFilter filter)
        {
            List<Evento> eventos;
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                eventos = eventoRepository.GetByFilterCoord(user.IdUsuario, new EventoFilter());
            }
            else
            {
                eventos = eventoRepository.GetByFilter(new EventoFilter());
            }

            return PartialView("_List", eventos);
        }

        public ActionResult CarregaModalConfirmacao(EnumStatusEvento novoStatus, int idEvento)
        {
            Evento evento = new Evento { IdEvento = idEvento, Status = novoStatus };
            return PartialView("_AlteracaoStatus", evento);
        }

        public object AlterarStatus(Evento evento)
        {
            Usuario user = GetSessionUser();

            try
            {
                var ev = eventoRepository.GetEventoById(evento.IdEvento);
                ev.Status = evento.Status;

                string msg = eventoRepository.AlteraStatusEvento(evento.IdEvento, evento.Status);

                if (msg != "Erro")
                {
                    try
                    {
                        if (ev.Status == EnumStatusEvento.cancelado)
                        {
                            var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/AlteracaoEvento.cshtml");
                            string viewCode = System.IO.File.ReadAllText(url);
                            var eventoEmail = eventoRepository.ConverToEmailModel(ev, Url.Action("Login", "Account", null, Request.Url.Scheme));

                            var html = RazorEngine.Razor.Parse(viewCode, eventoEmail);

                            var alunosConfirmados = eventoRepository.GetListaChamada(ev.IdEvento);

                            if (alunosConfirmados != null && alunosConfirmados.Count > 0 && alunosConfirmados.Any(x => x.Usuario.E_mail != ""))
                            {
                                var to = alunosConfirmados.Where(x => !string.IsNullOrEmpty(x.Usuario.E_mail)).Select(x => x.Usuario.E_mail).ToArray();
                                var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                                Email.EnviarEmail(from, to, "Alteração de evento", html);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                    }

                    if (ev.Status == EnumStatusEvento.cancelado)
                    {
                        Utilidades.SalvaLog(user, EnumAcao.Cancelar, ev, ev.IdEvento);
                    }

                    return Json(new { Status = true, Type = "success", Message = "Evento salvo com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
        }


        [AuthorizeAD(Groups = "G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
        public object SalvaEvento(Evento e, int[] SelectedCursos)
        {
            Usuario user = GetSessionUser();

            if (SelectedCursos == null)
                return Json(new { Status = false, Type = "error", Message = "Selecione pelo menos um curso." }, JsonRequestBehavior.AllowGet);

            if (e.IdEvento == 0)
            {
                Funcionario f = usuarioRepository.GetFuncionarioByUsuario(user.IdUsuario);
                e.Status = EnumStatusEvento.ativo;
                e.IdFuncionarioCriador = f.IdFuncionario;
            }
            switch (eventoRepository.PersisteEvento(e, SelectedCursos))
            {
                case "Mantido":
                    return Json(new { Status = true, Type = "success", Message = "Evento salvo com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                case "Cadastrado":
                    {
                        try
                        {
                            var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/NovoEvento.cshtml");
                            string viewCode = System.IO.File.ReadAllText(url);
                            var eventoEmail = eventoRepository.ConverToEmailModel(e, Url.Action("Login", "Account", null, Request.Url.Scheme));

                            var html = RazorEngine.Razor.Parse(viewCode, eventoEmail);
                            var alunosMatriculados = new List<Aluno>();

                            foreach (var c in SelectedCursos)
                            {
                                alunosMatriculados.AddRange(alunoRepository.GetAlunoByIdCurso(c));
                            }

                            if (alunosMatriculados != null && alunosMatriculados.Count > 0 && alunosMatriculados.Any(x => x.Usuario.E_mail != ""))
                            {
                                var to = alunosMatriculados.Where(x => !string.IsNullOrEmpty(x.Usuario.E_mail)).Select(x => x.Usuario.E_mail).ToArray();
                                var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                                Email.EnviarEmail(from, to, "Novo evento", html);
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        Utilidades.SalvaLog(user, EnumAcao.Persistir, e, null);
                        return Json(new { Status = true, Type = "success", Message = "Evento cadastrado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                    }
                case "Alterado":
                    {
                        try
                        {
                            var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/AlteracaoEvento.cshtml");
                            string viewCode = System.IO.File.ReadAllText(url);
                            var eventoEmail = eventoRepository.ConverToEmailModel(e, Url.Action("Login", "Account", null, Request.Url.Scheme));

                            var html = RazorEngine.Razor.Parse(viewCode, eventoEmail);

                            var alunosConfirmados = eventoRepository.GetListaChamada(e.IdEvento);

                            if (alunosConfirmados != null && alunosConfirmados.Count > 0 && alunosConfirmados.Any(x => x.Usuario.E_mail != ""))
                            {
                                var to = alunosConfirmados.Where(x => !string.IsNullOrEmpty(x.Usuario.E_mail)).Select(x => x.Usuario.E_mail).ToArray();
                                var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                                Email.EnviarEmail(from, to, "Alteração de evento", html);
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        Utilidades.SalvaLog(user, EnumAcao.Persistir, e, e.IdEvento);
                        return Json(new { Status = true, Type = "success", Message = "Evento alterado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                    }
                case "Erro":
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthorizeAD(Groups = "G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW")]
        public object GeraCertificados(int idEvento)
        {
            bool flag = DirDoc.GeraCertificado(idEvento);

            if (flag)
            {
                try
                {
                    var evento = eventoRepository.GetEventoById(idEvento);
                    var url = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Email/CertificadoGerado.cshtml");
                    string viewCode = System.IO.File.ReadAllText(url);
                    var eventoEmail = eventoRepository.ConverToEmailModel(evento, Url.Action("Login", "Account", null, Request.Url.Scheme));

                    var html = RazorEngine.Razor.Parse(viewCode, eventoEmail);
                    List<Aluno> alunosPresentes = eventoRepository.GetAlunosPresentes(evento);

                    eventoRepository.AlteraStatusEvento(idEvento, EnumStatusEvento.certificados);

                    try
                    {
                        if (alunosPresentes != null && alunosPresentes.Count > 0 && alunosPresentes.Any(x => x.Usuario.E_mail != ""))
                        {
                            var to = alunosPresentes.Where(x => !string.IsNullOrEmpty(x.Usuario.E_mail)).Select(x => x.Usuario.E_mail).ToArray();
                            var from = System.Configuration.ConfigurationManager.AppSettings["MailFrom"].ToString();
                            Email.EnviarEmail(from, to, string.Format("Certificado do evento {0} gerado", evento.NomeEvento), html);
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    return Json(new { Status = true, Type = "success", Message = "Certificados gerados com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { Status = false, Type = "error", Message = "Houve um erro, tente novamente mais tarde!" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Status = false, Type = "error", Message = "Houve um erro, tente novamente mais tarde!" }, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeAD(Groups = "G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW")]
        public object Chamada(int idEvento)
        {
            bool chamadaFeita = eventoRepository.ChamadaFeita(idEvento, DateTime.Now.Date);
            Chamada c;
            List<Presenca> presencas;

            List<int> cursos;
            List<Aluno> alunos;

            //if (Utilidades.UsuarioLogado.Permissao == EnumPermissaoUsuario.coordenador)
            //{
            //    cursos = cursoRepository.GetCursoByCoordenador(User.Identity.Name).Select(y => y.IdCurso).ToList();
            //    alunos = eventoRepository.GetListaChamada(idEvento).Where(x => cursos.Contains(x.AlunoCurso.FirstOrDefault().IdCurso)).ToList();
            //}

            alunos = eventoRepository.GetListaChamada(idEvento);

            Evento evento = eventoRepository.GetEventoById(idEvento);

            if (evento.Chamada.Count >= (evento.DataFim - evento.DataInicio).Days)
            {
                return Json(new { Status = false, Type = "error", Message = "Todas as chamadas já foram efetuadas para este evento" }, JsonRequestBehavior.AllowGet);
            }

            if (chamadaFeita)
            {
                c = eventoRepository.GetChamada(idEvento, DateTime.Now.Date);

                return PartialView("_Chamada", new ChamadaModel
                {
                    Alunos = alunos,
                    IdEvento = idEvento,
                    NomeEvento = evento.NomeEvento,
                    Presentes = c.Presenca.ToList()
                });

            }

            return PartialView("_Chamada", new ChamadaModel
            {
                Alunos = alunos,
                IdEvento = idEvento,
                NomeEvento = evento.NomeEvento
            });

        }

        [AuthorizeAD(Groups = "G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW")]
        public object FazerChamada(int[] idAlunos, int idEvento)
        {
            Usuario user = GetSessionUser();

            bool flag = eventoRepository.AdicionaPresenca(idAlunos, idEvento, user.IdUsuario);

            if (flag)
            {
                return Json(new { Status = true, Type = "success", Message = "Chamada concluída!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false, Type = "error", Message = "Houve um erro, tente novamente mais tarde!" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Lado do aluno
        [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS")]
        public ActionResult MeusEventos()
        {
            Usuario user = GetSessionUser();

            eventoRepository.AtualizaStatus();
            List<Evento> eventos = eventoRepository.GetByFilterAluno(user.IdUsuario, new EventoFilter()).Where(x => DateTime.Now < x.DataFim).ToList();

            return View(eventos);
        }

        [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS")]
        public ActionResult ListAluno(EventoFilter filter)
        {
            Usuario user = GetSessionUser();

            return PartialView("_List", eventoRepository.GetByFilterAluno(user.IdUsuario, filter).Where(x => DateTime.Now < x.DataFim).ToList());
        }

        [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS")]
        public ActionResult CarregaModalConfirmacaoParticipacao(int idEvento, bool presente)
        {
            Evento ev = eventoRepository.GetEventoById(idEvento);
            ViewBag.Presente = presente;
            return PartialView("_ConfirmacaoPresenca", ev);
        }

        [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS")]
        public object ConfirmaCancelaPartipacao(Evento evento, bool Presente)
        {
            Usuario user = GetSessionUser();

            try
            {
                var ev = eventoRepository.GetEventoById(evento.IdEvento);
                var aluno = alunoRepository.GetAlunoByIdUsuario(user.IdUsuario);
                bool ok = false;

                if (aluno != null)
                {
                    if (Presente)
                        ok = eventoRepository.InscreveAluno(aluno.IdAluno, evento.IdEvento);
                    else
                        ok = eventoRepository.DesinscreverAluno(aluno.IdAluno, evento.IdEvento);
                }

                if (ok)
                    return Json(new { Status = true, Type = "success", Message = "Alteração realizada com sucesso!" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
        }


        #endregion

        #region Métodos auxiliares

        private void PopularDropDownsFiltro()
        {
            var listStatus = Enum.GetValues(typeof(EnumStatusEvento)).
                Cast<EnumStatusEvento>().Select(v => new SelectListItem
                {
                    Text = EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString(),
                }).ToList();

            ViewBag.Status = new SelectList(listStatus, "Value", "Text");
        }

        private void PopularDropDownsCadastro(int? idEvento)
        {
            var listCursosSelectList = cursoRepository.GetCursos().Select(item => new SelectListItem
            {
                Value = item.IdCurso.ToString(),
                Text = item.Nome.ToString(),
            });
            ViewBag.Cursos = new SelectList(listCursosSelectList, "Value", "Text");
        }

        #endregion
    }
}
