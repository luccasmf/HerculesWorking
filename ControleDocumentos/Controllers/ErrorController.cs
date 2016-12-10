using ControleDocumentos.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControleDocumentos.Controllers
{
    //[AuthorizeAD(Groups = "G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW, G_FACULDADE_COORDENADOR_R, G_FACULDADE_COORDENADOR_RW, G_FACULDADE_SECRETARIA_R, G_FACULDADE_SECRETARIA_RW")]
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View("Error");
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 200;
            return View("NotFound");
        }

        public ActionResult InternalServer()

        {
            Response.StatusCode = 200;
            return View("InternalServer");
        }

        public ActionResult Unauthorized()
        {
            Response.StatusCode = 200;
            return View("Unauthorized");
        }

        public ActionResult UnauthorizedPartial()
        {
            Response.StatusCode = 200;
            return PartialView("_Unauthorized");
        }
    }
}