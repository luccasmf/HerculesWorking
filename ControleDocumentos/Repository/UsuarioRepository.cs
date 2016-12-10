using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;
using System.Reflection;
using ControleDocumentos.Util;

namespace ControleDocumentos.Repository
{
    public class UsuarioRepository
    {
        DocumentosModel db = new DocumentosModel();

        public List<Usuario> GetUsuarios()
        {
            return db.Usuario.ToList();
        }

        public Usuario GetUsuarioByALuno(Aluno al)
        {
            return db.Usuario.Find(al.IdUsuario);
        }

        public Funcionario GetFuncionarioByUsuario(string idUsuario)
        {
            return db.Funcionario.Where(u => u.IdUsuario == idUsuario).FirstOrDefault();
        }

        public Usuario GetUsuarioByFuncionario(Funcionario func)
        {
            return db.Usuario.Find(func.IdUsuario);
        }

        public Usuario GetUsuarioById(string idUsuario)
        {
            return db.Usuario.Find(idUsuario);
        }

        public string PersisteUsuario(Usuario[] users)
        {
            if (users.Length == 1)
            {
                Usuario user = users[0];
                if (!string.IsNullOrEmpty(user.IdUsuario))
                {
                    return ComparaInfos(user);
                }
                else
                {
                    db.Usuario.Add(user);

                }
                
            }
            else
            {
                db.Usuario.AddRange(users);
            }
            if (db.SaveChanges() > 0)
            {
                return "Cadastrado";
            }
            else
            {
                return "Erro";
            }

        }

        private string ComparaInfos(Usuario user)
        {
            DocumentosModel db2 = new DocumentosModel();
            Usuario userld = db2.Usuario.Find(user.IdUsuario);

            if(user.Funcionario.Count > 0)
            {
                PersisteFuncionario(user.Funcionario.ToArray());
            }

            userld = Utilidades.ComparaValores(userld, user, new string[] {"E_mail","Permissao"});

            
            if (userld == null)
            {
                return "Mantido";
            }
            if (db2.SaveChanges() > 0)
            {
                return "Alterado";
            }
            else
            {
                return "Erro";
            }
        }

        public string PersisteFuncionario(Funcionario[] users)
        {
            if (users.Length == 1)
            {
                Funcionario user = users[0];
                
                    db.Funcionario.Add(user);
                
            }
            else
            {
                db.Funcionario.AddRange(users);
            }
            if (db.SaveChanges() > 0)
            {
                return "Cadastrado";
            }
            else
            {
                return "Erro";
            }

        }

        public List<Funcionario> GetCoordenadores()
        {
            List<Funcionario> funcs = db.Funcionario.Where(x => x.Permissao == EnumPermissaoUsuario.coordenador).ToList();

            return funcs;
        }

        public List<Usuario> GetUsuariosSecretaria()
        {
            return db.Usuario.Where(x => x.Permissao == EnumPermissaoUsuario.secretaria).ToList();
        }
    }
}