using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Repository;
using ControleDocumentosLibrary;
using ControleDocumentos.Util;
using ControleDocumentos.Filter;
using ControleDocumentos.Util.Extension;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class CursoController : BaseController
    {
        CursoRepository cursoRepository = new CursoRepository();
        UsuarioRepository usuarioRepository = new UsuarioRepository();
        // GET: Curso
        public ActionResult Index()
        {
            return View(cursoRepository.GetCursos());
        }

        public ActionResult CarregaModalCadastro(int? idCurso)
        {
            PopularDropDowns();

            //instancia model
            Curso curso = new Curso();

            if (idCurso.HasValue)
            {
                //pega model pelo id
                curso = cursoRepository.GetCursoById(idCurso.Value);
            }
            PopularDropDowns();
            //retorna model
            return PartialView("_CadastroCurso", curso);
        }

        public ActionResult CarregaModalExclusao(int idCurso)
        {
            
            Curso curso = cursoRepository.GetCursoById(idCurso);
            PopularDropDowns();
            return PartialView("_ExclusaoCurso", curso);
        }

        public object SalvarCurso(Curso curso) //serve pra cadastrar e editar
        {
            Usuario user = GetSessionUser();

            switch (cursoRepository.PersisteCurso(curso))
            {
                case "Cadastrado":
                    Utilidades.SalvaLog(user, EnumAcao.Persistir, curso, null);
                    return Json(new { Status = true, Type = "success", Message = "Curso cadastrado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                case "Alterado":
                    Utilidades.SalvaLog(user, EnumAcao.Persistir, curso, curso.IdCurso);
                    return Json(new { Status = true, Type = "success", Message = "Curso alterado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                case "Erro":
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new { Status = false, Type = "error", Message = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult List()
        {
           
            return PartialView("_List", cursoRepository.GetCursos());
        }

        public object ExcluirCurso(Curso curso)
        {
           Usuario user = GetSessionUser();

            curso = cursoRepository.GetCursoById(curso.IdCurso);
            string msg = cursoRepository.DeletaCurso(curso.IdCurso);
            switch (msg)
            {
                case "Excluido":
                    Utilidades.SalvaLog(user, EnumAcao.Excluir, curso, curso.IdCurso);
                    return Json(new { Status = true, Type = "success", Message = "Curso deletado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                case "Alunos":
                    return Json(new { Status = false, Type = "error", Message = "Não é possível excluir cursos com alunos cadastrados" }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);

            }

        }

        public JsonResult AtualizarCoordenadores()
        {
            GetSessionUser();

            Utilidades.BuscarCoords();
            List<Funcionario> lstCoordenadores = usuarioRepository.GetCoordenadores();
            return Json(lstCoordenadores.Select(x => new { Value = x.IdUsuario, Text = x.Usuario.Nome }));
        }

        #region Métodos auxiliares

        private void PopularDropDowns()
        {
            var listCoordenadores = usuarioRepository.GetCoordenadores().Select(item => new SelectListItem
            {
                Value = item.IdFuncionario.ToString(),
                Text = item.Usuario.Nome.ToString(),
            });
            ViewBag.Coordenadores = new SelectList(listCoordenadores, "Value", "Text");

            var listNivel = Enum.GetValues(typeof(EnumNivelCurso)).
                Cast<EnumNivelCurso>().Select(v => new SelectListItem
                {
                    Text = EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString(),
                }).ToList();
            ViewBag.Niveis = new SelectList(listNivel, "Value", "Text");

        }

        #endregion
    }
}