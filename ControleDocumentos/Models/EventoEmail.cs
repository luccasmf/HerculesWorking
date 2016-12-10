using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Models
{
    public class EventoEmail
    {
        public string NomeEvento { get; set; }
        public string Local { get; set; }
        public int Vagas { get; set; }
        public int CargaHoraria { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Observacao { get; set; }

        public string UrlSistema { get; set; }
        public string EmailAluno { get; set; }
    }
}