using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleDocumentosLibrary
{
    public enum EnumAcao : int
    {

        [Description("Persistir")]
        Persistir = 1,
        [Description("Ativar")]
        Ativar,
        [Description("Desativar")]
        Desativar,
        [Description("Excluir")]
        Excluir,
        [Description("Aprovar")]
        Aprovar,
        [Description("Reprovar")]
        Reprovar,
        [Description("Cancelar")]
        Cancelar,
    }

    public enum EnumTipoObjeto : int
    {
        [Description("Aluno")]
        Aluno = 1,
        [Description("Curso")]
        Curso,
        [Description("Documento")]
        Documento,
        [Description("Evento")]
        Evento,
        [Description("Funcionario")]
        Funcionario,
        [Description("Usuario")]
        Usuario,
        [Description("SolicitacaoDocumento")]
        SolicitacaoDocumento,
        [Description("TipoDocumento")]
        TipoDocumento
    }
}
