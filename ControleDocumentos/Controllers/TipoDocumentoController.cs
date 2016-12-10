using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Repository;
using ControleDocumentosLibrary;
using ControleDocumentos.Util;
using ControleDocumentos.Filter;

namespace ControleDocumentos.Controllers
{
    [AuthorizeAD(Groups = "G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class TipoDocumentoController : BaseController
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();


        // GET: TipoDocumento
        public ActionResult Index()
        {
            return View(tipoDocumentoRepository.listaTipos());
        }

        public ActionResult CarregaModalCadastro(int? idTipo)
        {
            //instancia model
            TipoDocumento tipoDoc = new TipoDocumento();

            if (idTipo.HasValue)
            {
                //pega model pelo id
                tipoDoc = tipoDocumentoRepository.GetTipoDocById(idTipo);
            }
            //retorna model
            return PartialView("_CadastroTipoDocumento", tipoDoc);
        }

        public ActionResult CarregaModalExclusao(int idTipo)
        {
            //get no tipo
            TipoDocumento tipoDoc = tipoDocumentoRepository.GetTipoDocById(idTipo);

            //retorna o tipo na partial
            return PartialView("_ExclusaoTipoDocumento", tipoDoc);
        }

        public object SalvarTipoDocumento(TipoDocumento model)
        {
            Usuario user = GetSessionUser();

            try
            {
                if(tipoDocumentoRepository.TipoDocExists(model))
                    return Json(new { Status = false, Type = "error", Message = "Tipo de documento já existente" }, JsonRequestBehavior.AllowGet);
                
                if (tipoDocumentoRepository.CadastraTipoDoc(model))
                {
                    Utilidades.SalvaLog(user, EnumAcao.Persistir, model, (model.IdTipoDoc > 0? (int?)model.IdTipoDoc:null));
                    return Json(new { Status = true, Type = "success", Message = "Tipo de documento salvo com sucesso.", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);

                }
                else
                    throw new Exception("erro ao cadastrar tipo");
            }
            catch (Exception ex)
            {
                return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult List()
        {
            return PartialView("_List", tipoDocumentoRepository.listaTipos());
        }

        public object ExcluirTipoDocumento(TipoDocumento tipoDoc)
        {
            Usuario user = GetSessionUser();

            tipoDoc = tipoDocumentoRepository.GetTipoDocById(tipoDoc.IdTipoDoc);
            if (tipoDocumentoRepository.DeletaTipoDoc(tipoDoc.IdTipoDoc))
            {
                Utilidades.SalvaLog(user, EnumAcao.Excluir, tipoDoc, tipoDoc.IdTipoDoc);
                return Json(new { Status = true, Type = "success", Message = "Tipo de documento deletado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);

        }
    }
}