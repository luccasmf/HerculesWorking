using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Filter;
using ControleDocumentosLibrary;
using ControleDocumentos.Util.Extension;
using ControleDocumentos.Repository;
using ControleDocumentos.Models;
using ControleDocumentos.Util;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class HomeController : BaseController
    {
        UsuarioRepository usuarioRepository = new UsuarioRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        // GET: Home
        public ActionResult Index()
        {
            // PUBLICADO
            //Utilidades.UsuarioLogado = GetSessionUser();

            // LOCAL
            Usuario user = GetSessionUser();

            if (user.E_mail == null)
                return RedirectToAction("DadosCadastrais");
            if (user.Permissao == EnumPermissaoUsuario.aluno)
                return RedirectToAction("Index", "SolicitacaoDocumentoAluno");
            else if (user.Permissao == EnumPermissaoUsuario.coordenador)
                return RedirectToAction("Index", "SolicitacaoHoraComplementar");
            else if (user.Permissao == EnumPermissaoUsuario.professor)
                return RedirectToAction("Index", "Evento");
            else if (user.Permissao == EnumPermissaoUsuario.secretaria)
                return RedirectToAction("Index", "Documento");
            else if (user.Permissao == EnumPermissaoUsuario.admin)
                return RedirectToAction("Index", "Log");

            return View(user);
        }

        public ActionResult DadosCadastrais()
        {
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.aluno)
            {
                var aluno = alunoRepository.GetAlunoByIdUsuario(user.IdUsuario);
                var idCurso = aluno.AlunoCurso != null ? aluno.AlunoCurso.Select(x => x.IdCurso).FirstOrDefault() : 0;

                var listCursosSelectList = cursoRepository.GetCursos().Select(item => new SelectListItem
                {
                    Value = item.IdCurso.ToString(),
                    Text = item.Nome.ToString(),
                });
                ViewBag.Cursos = new SelectList(listCursosSelectList, "Value", "Text", idCurso.ToString());
            }
           // GetSessionUser();

            var usuario = usuarioRepository.GetUsuarioById(user.IdUsuario);
            return View(usuario);
        }

        public object SalvarDadosCadastrais(Usuario usuario, int? IdCurso)
        {
            Usuario user = GetSessionUser();

            usuario.Permissao = user.Permissao;
            try
            {
                bool flag = false;
                if (user.Permissao == EnumPermissaoUsuario.aluno)
                {
                    flag = alunoRepository.PersisteAluno(usuario.IdUsuario, (int)IdCurso);
                }

                string msg = usuarioRepository.PersisteUsuario(new Usuario[] { usuario });

                if (msg != "Erro")
                {
                    flag = true;
                }


                if (flag)
                {
                    Utilidades.SalvaLog(user, EnumAcao.Persistir, usuario, null);
                    return Json(new { Status = true, Type = "success", Message = "Salvo com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação." }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}