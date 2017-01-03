using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleDocumentosLibrary
{
    public enum EnumPermissaoUsuario : int
    {
        [Description("Deslogado")]
        deslogado=0,
        [Description("Aluno")]
        aluno=1,
        [Description("Professor")]
        professor,
        [Description("Coordenador")]
        coordenador,
        [Description("Secretaria")]
        secretaria,
        [Description("Admin")]
        admin,
    }

    public enum EnumStatusSolicitacao : int
    {
        [Description("Pendente")]
        pendente = 1,
        [Description("Visualizado")]
        visualizado,
        [Description("Processando")]
        processando,
        [Description("Concluído")]
        concluido,
        [Description("Cancelado")]
        cancelado,
        [Description("Reprovado")]
        reprovado,
        [Description("Documento Excluído")]
        excluido,
    }

    public enum EnumStatusEvento : int
    {
        [Description("Ativo")]
        ativo=1,
        [Description("Cancelado")]
        cancelado=2,
        [Description("Concluído")]
        concluido=3,
        [Description("Certificados gerados")]
        certificados=4,

    }

    public enum EnumSession : int
    {
        Usuario
    }

    public enum EnumTipoSolicitacao: int
    {
        [Description("Solicitação do aluno")]
        aluno=1,

        [Description("Solicitação da secretaria")]
        secretaria
    }

    public enum EnumNivelCurso : int {
        [Description("Técnico")]
        tecnico = 1,

        [Description("Tecnólogo")]
        tecnologo,

        [Description("Graduação")]
        graduacao,

        [Description("Pós-Graduação")]
        pos_graduacao
    }
}
