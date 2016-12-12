using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentosLibrary;
using System.IO;
using ControleDocumentos.Repository;
using ControleDocumentos.Filter;
using ControleDocumentos.Util.Extension;
using ControleDocumentos.Util;

namespace ControleDocumentos.Controllers
{
    // autorizar coordenador, aluno e secretaria
    [AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class DocumentoController : BaseController
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();

        // GET: Documento
        public ActionResult Index()
        {
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                List<Documento> retorno = documentoRepository.GetDocsByCoordenador(user.IdUsuario);
                return View(retorno);
            }

            if (user.Permissao == EnumPermissaoUsuario.aluno)
            {
                List<Documento> retorno = documentoRepository.GetDocsByAluno(user.IdUsuario);
                return View(retorno);
            }
            
            return View(documentoRepository.GetAllDocs().Where(x=>x.NomeDocumento != "").ToList());
        }

        [AuthorizeAD(Groups = "G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
        public ActionResult CadastrarDocumento(int? idDoc)
        {
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.aluno)
                return RedirectToAction("UnauthorizedPartial", "Error");

            PopularDropDowns();
            Documento doc = new Documento();

            if (idDoc.HasValue)
            {
                doc = documentoRepository.GetDocumentoById((int)idDoc);
                PopularDropDownAlunos(doc.AlunoCurso.Curso.IdCurso);
            }
            else
            {
                ViewBag.Alunos = new SelectList(new List<SelectListItem>() { new SelectListItem() {
                    Text ="Selecione um curso",
                    Value =""}
                }, "Value", "Text");
            }
            return PartialView("_CadastroDocumento", doc);
        }

        public ActionResult List()
        {
            Usuario user = GetSessionUser();

            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                List<Documento> retorno = documentoRepository.GetDocsByCoordenador(user.IdUsuario);
                return PartialView("_List", retorno.Where(x => x.NomeDocumento != ""));
            }
            // helenira: replicar validação da view
            return PartialView("_List", documentoRepository.GetAllDocs().Where(x => x.NomeDocumento != ""));

        }

        [AuthorizeAD(Groups = "G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
        public ActionResult CarregaModalExclusao(int idDoc)
        {
            Documento doc = documentoRepository.GetDocumentoById(idDoc);
            return PartialView("_ExclusaoDocumento", doc);
        }

        #region Métodos auxiliares

        private void PopularDropDowns()
        {
            Usuario user = GetSessionUser();

            List<Curso> lstCursos = new List<Curso>();
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                lstCursos = cursoRepository.GetCursoByCoordenador(user.IdUsuario);
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
            ViewBag.TiposDoc = new SelectList(listTiposDoc, "Value", "Text");

        }

        private void PopularDropDownAlunos(int idCurso)
        {
            Usuario user = GetSessionUser();

            var ok = false;
            List<SelectListItem> listAlunos = new List<SelectListItem>();

            // se é coordenador, valida se usuario é coordenador do curso passado no parametro
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                var cursos = cursoRepository.GetCursoByCoordenador(user.IdUsuario);
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

        [AuthorizeAD(Groups = "G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
        public object SalvarDocumento(Documento doc, HttpPostedFileBase uploadFile)
        {
            Usuario user = GetSessionUser();

            if (ModelState.IsValid)
            {
                try
                {
                    // valida se o aluno está matriculado no curso que coordena
                    if (user.Permissao == EnumPermissaoUsuario.coordenador)
                    {
                        var cursos = cursoRepository.GetCursoByCoordenador(user.IdUsuario);
                        if (!cursos.Any(x => x.AlunoCurso.Any(y => y.IdAluno == doc.AlunoCurso.IdAluno)))
                            return Json(new { Status = false, Type = "error", Message = "Não autorizado!" }, JsonRequestBehavior.AllowGet);
                    }

                    if (uploadFile == null)
                        return Json(new { Status = false, Type = "error", Message = "Selecione um documento" }, JsonRequestBehavior.AllowGet);

                    if (!ValidaArquivo(uploadFile.FileName))
                    {
                        return Json(new { Status = false, Type = "error", Message = "Formato inválido" }, JsonRequestBehavior.AllowGet);

                    }

                    doc.arquivo = DirDoc.converterFileToArray(uploadFile);
                    doc.NomeDocumento = uploadFile.FileName;
                    string mensagem = DirDoc.SalvaArquivo(doc);

                    switch (mensagem)
                    {
                        case "Arquivo existente":
                            return Json(new { Status = true, Type = "success", Message = "Documento salvo com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                        case "Sucesso":
                            doc.arquivo = null;
                            Utilidades.SalvaLog(user, EnumAcao.Persistir, doc, (doc.IdDocumento > 0 ? (int?)doc.IdDocumento : null));
                            return Json(new { Status = true, Type = "success", Message = "Documento salvo com sucesso", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                        case "Falha ao persistir":
                            return Json(new { Status = false, Type = "error", Message = mensagem }, JsonRequestBehavior.AllowGet);
                        case "Falha ao criptografar":
                            return Json(new { Status = false, Type = "error", Message = mensagem }, JsonRequestBehavior.AllowGet);
                        default:
                            return null;
                    }
                }
                catch (Exception e)
                {
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);
                }
            }
            else {
                return Json(new { Status = false, Type = "error", Message = "Campos inválidos" }, JsonRequestBehavior.AllowGet);
            }
        }

        [AuthorizeAD(Groups = "G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
        public object ExcluirDocumento(Documento doc)
        {
            Usuario user = GetSessionUser();

            // valida se o aluno está matriculado no curso que coordena
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                List<Curso> cursos = cursoRepository.GetCursoByCoordenador(user.IdUsuario);

                List<int> alunos = new List<int>();
                doc = documentoRepository.GetDocumentoById(doc.IdDocumento);

                foreach (Curso c in cursos)
                {
                    alunos.AddRange(c.AlunoCurso.Select(x => x.IdAluno));
                }

                if (!alunos.Contains(doc.AlunoCurso.IdAluno))
                    return Json(new { Status = false, Type = "error", Message = "Não autorizado!" }, JsonRequestBehavior.AllowGet);
            }

            if (documentoRepository.DeletaArquivo(doc,true))
            {
                
                Utilidades.SalvaLog(user, EnumAcao.Excluir, doc, (int?)doc.IdDocumento);
                return Json(new { Status = true, Type = "success", Message = "Documento deletado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação, verifique se existe uma solicitação vinculada a este documento." }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Baixa arquivo
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>retorna o arquivo pra download</returns>
        /// 
        public ActionResult Download(string nomeDoc)
        {
            Usuario user = GetSessionUser();

            Documento doc = documentoRepository.GetDocumentoByNome(nomeDoc);
            if (user.Permissao == EnumPermissaoUsuario.professor)
            {
                return RedirectToAction("Unauthorized", "Error");
            }

            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                List<Documento> retorno = documentoRepository.GetDocsByCoordenador(user.IdUsuario);
                if (!retorno.Any(x => x.IdDocumento == doc.IdDocumento))
                    return RedirectToAction("Unauthorized", "Error");
            }

            string nomeArquivo = doc.NomeDocumento;
            string extensao = Path.GetExtension(nomeArquivo);

            string contentType = "application/" + extensao.Substring(1);

            byte[] bytes = DirDoc.BaixaArquivo(doc, user);

            if (bytes == null)
                return RedirectToAction("Unauthorized", "Error");

            return File(bytes, contentType, nomeArquivo);
        }

        public JsonResult GetAlunosByIdCurso(int idCurso)
        {
            Usuario user = GetSessionUser();

            // se é coordenador, valida se é coodenador do curso passado por parametro
            if (user.Permissao == EnumPermissaoUsuario.coordenador)
            {
                var cursos = cursoRepository.GetCursoByCoordenador(user.IdUsuario);
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

        #endregion
    }
}