using ControleDocumentos.Models;
using ControleDocumentos.Repository;
using ControleDocumentos.Util.Extension;
using ControleDocumentosLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace ControleDocumentos.Util
{
    public static class Utilidades
    {
       // public static Usuario UsuarioLogado = new Usuario();

        private static LogsRepository logsRepository = new LogsRepository();

        private static UsuarioRepository usuarioRepository = new UsuarioRepository();

        /// <summary>
        /// Compara propriedades dos objetos passados e caso haja mudanças, substitui os valores e retorna o "atualizado"
        /// </summary>
        /// <typeparam name="T">tipo do objeto</typeparam>
        /// <param name="old">versão persistida do objeto</param>
        /// <param name="to">nova versão do objeto</param>
        /// <param name="alterar">nome dos campos a serem alterados (string[])</param>
        /// <returns>retorna o objeto com as informações atualizadas</returns>
        public static T ComparaValores<T>(T old, T to, params string[] alterar) where T : class
        {
            bool alterado = false;
            if (old != null && to != null)
            {
                Type type = typeof(T);
                List<string> alterarList = new List<string>(alterar);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (alterar.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(old, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue == null && toValue == null)
                        {
                            continue;
                        }
                        else if (selfValue == null && toValue != null)
                        {
                            pi.SetValue(old, toValue);
                            alterado = true;
                        }
                        else if (pi.Name.ToUpperInvariant().Contains("STATUS") && toValue.ToString() == "0")
                        {
                            continue;
                        }
                        else if (selfValue.ToString() != toValue.ToString())
                        {
                            pi.SetValue(old, toValue);
                            alterado = true;
                        }
                    }
                }
            }
            if (alterado)
                return old;
            else
                return null;
        }

        public static Usuario GetSession(LoginModel lm)
        {
            Usuario user;
            if (lm.UserName == "admin")
            {
                user = new Usuario();
                user.Nome = "Admin";
                user.E_mail = "teste@teste";
                user.Permissao = EnumPermissaoUsuario.admin;
                //user.Permissao = EnumPermissaoUsuario.coordenador;
                //user.IdUsuario = "160220040209";

                //user.Permissao = EnumPermissaoUsuario.aluno;
                //user.IdUsuario = "10077401272";

            }
            else
            {
                user = usuarioRepository.GetUsuarioById(lm.UserName);
            }



            return user;
        }

        public static bool SalvaLog<T>(Usuario usuario, EnumAcao e, T objeto, int? idObjeto)
        {
            Logs log = new Logs();

            log.IdUsuario = usuario.IdUsuario;
            log.Data = DateTime.Now;
            log.Acao = e;
            log.IdObjeto = idObjeto;
            log.TipoObjeto = EnumExtensions.GetValueFromDescription<EnumTipoObjeto>(typeof(T).Name.ToString());
            // log.EstadoAnterior = new JavaScriptSerializer().Serialize(objeto);

            return (logsRepository.SalvarLog(log));
        }

        public static object BuscarCoords()
        {
            var context = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings["Dominio"], "9077401526", "12qw!@QW"); //usuario com direitos q nao entendi...
            GroupPrincipal gro = GroupPrincipal.FindByIdentity(context, "G_FACULDADE_COORDENADOR_R");

            List<Usuario> usuarios = new List<Usuario>();
            List<Funcionario> funcionarios = new List<Funcionario>();
            foreach (UserPrincipal userPrincipal in gro.Members)
            {
                Usuario user = usuarioRepository.GetUsuarioById(userPrincipal.SamAccountName);
                if (user != null)
                {
                    if (user.Permissao == EnumPermissaoUsuario.professor || user.Permissao == EnumPermissaoUsuario.secretaria)
                    {
                        user.Permissao = EnumPermissaoUsuario.coordenador;
                        user.Funcionario.FirstOrDefault().Permissao = EnumPermissaoUsuario.coordenador;

                    }
                    else if (user.Permissao == EnumPermissaoUsuario.aluno)
                    {
                        user.Permissao = EnumPermissaoUsuario.coordenador;
                        Funcionario f = new Funcionario();

                        f.Permissao = EnumPermissaoUsuario.coordenador;
                        f.IdUsuario = user.IdUsuario;


                        funcionarios.Add(f);

                    }
                    else if (user.Permissao == EnumPermissaoUsuario.coordenador)
                    {
                        continue;
                    }

                    usuarios.Add(user);
                }
                else
                {

                    if (usuarioRepository.GetUsuarioById(userPrincipal.SamAccountName) == null)
                    {
                        Usuario ususario = new Usuario();
                        Funcionario f = new Funcionario();

                        ususario.IdUsuario = userPrincipal.SamAccountName;
                        ususario.Nome = userPrincipal.Name;
                        ususario.Permissao = EnumPermissaoUsuario.coordenador;

                        f.IdUsuario = ususario.IdUsuario;
                        f.Permissao = EnumPermissaoUsuario.coordenador;

                        funcionarios.Add(f);
                        usuarios.Add(ususario);
                    }
                }

            }

            switch (usuarioRepository.PersisteUsuario(usuarios.ToArray()))
            {
                case "Cadastrado":
                    usuarioRepository.PersisteFuncionario(funcionarios.ToArray());
                    return true;
                default:
                    return false;
            }

        }
    }
}