using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Models
{
    public class EventoFilter
    {
        public string NomeEvento { get; set; }
        public int? IdStatus { get; set; }
        public bool ApenasInscritos { get; set; }
    }
}