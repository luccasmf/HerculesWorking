using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Models
{
    public class SolicitacaoDocumentoFilter
    {
        public int? IdCurso { get; set; }
        public int? IdStatus { get; set; }
        public bool ApenasPendentes { get; set; }
    }
}