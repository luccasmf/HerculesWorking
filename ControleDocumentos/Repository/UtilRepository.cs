using ControleDocumentos.Util.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControleDocumentos.Repository
{
    public class UtilRepository
    {
        /// <summary>
        /// Pega os valores de um enumerador sendo chave o nome do enumerador e o valor sendo o valor do atributo description (se não houver o atributo pega o nome)
        /// </summary>
        public static Dictionary<object, string> GetEnumListDescriptions<T>(bool nameAsKey = false, bool addDefaultSelectOption = false, string defaultSelectOption = "Selecione")
        {
            var list = new Dictionary<object, string>();
            if (addDefaultSelectOption)
            {
                list.Add(0, defaultSelectOption);
            }

            var enumType = typeof(T);

            foreach (string name in Enum.GetNames(enumType))
            {
                object itemEnum = Enum.Parse(enumType, name);
                string description = ((Enum)itemEnum).GetEnumDescription();
                object key;
                if (nameAsKey)
                {
                    key = name;
                }
                else
                {
                    key = ((Enum)itemEnum).GetHashCode();
                }

                list.Add(key, description);
            }

            return list;
        }

        
        public static bool PublicInstancePropertiesEqual<T>(T self, T to, params string[] alterar) where T : class
        {
            if (self != null && to != null)
            {
                Type type = typeof(T);
                List<string> alterarList = new List<string>(alterar);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (alterar.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        //pi.SetValue(self)

                        if (selfValue.ToString() != toValue.ToString() && (selfValue != null && toValue != null))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return self == to;
        }
    }
}