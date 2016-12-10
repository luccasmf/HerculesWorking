using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Models
{
    public class LogFilter
    {
        public string Usuario { get; set; }
        public EnumAcao? Acao { get; set; }
        public EnumTipoObjeto? TipoObjeto { get; set; }
        public int? IdObjeto { get; set; }
    }
}