using ControleDocumentos.Filter;
using ControleDocumentos.Repository;
using ControleDocumentos.Util.Extension;
using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControleDocumentos.Controllers
{
    //[AuthorizeAD]
    public class LogController : Controller
    {
        LogsRepository logRepository = new LogsRepository();
        // GET: Log
        public ActionResult Index()
        {
            PopularDropDowns();
            // lucciros implementa o get by filter pfvr, criei o metodo na repo ja
            return View(logRepository.GetByFilter(new Models.LogFilter()));
        }

        public ActionResult List(Models.LogFilter filter)
        {
            return PartialView("_List", logRepository.GetByFilter(filter));
        }

        #region Métodos auxiliares

        private void PopularDropDowns()
        {
            var listAcoes = Enum.GetValues(typeof(EnumAcao)).
                Cast<EnumAcao>().Select(v => new SelectListItem
                {
                    Text = EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString(),
                }).ToList();
            ViewBag.Acao = new SelectList(listAcoes, "Value", "Text");

            var listTiposObj = Enum.GetValues(typeof(EnumTipoObjeto)).
                Cast<EnumTipoObjeto>().Select(v => new SelectListItem
                {
                    Text = EnumExtensions.GetEnumDescription(v),
                    Value = ((int)v).ToString(),
                }).ToList();
            ViewBag.TipoObjeto = new SelectList(listTiposObj, "Value", "Text");
        }

        #endregion
    }
}