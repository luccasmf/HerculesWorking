using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Models
{
    public class SolicitacaoDocumentoEmail
    {
        public DateTime DataLimite { get; set; }
        public DateTime? DataAtendimento { get; set; }
        public EnumStatusSolicitacao Status { get; set; }
        public string Observacao { get; set; }
        public string NomeAluno { get; set; }
        public string NomeTipoDocumento { get; set; }
        public string NomeCurso { get; set; }
        public string UrlSistema { get; set; }
        public string EmailAluno { get; set; }
        public string EmailFuncionario { get; set; }
    }
}