using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;

namespace ControleDocumentos.Util.Extension
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this System.Enum value, string separator = ", ")
        {
            string[] values = value.ToString().Split(new string[] { ", " }, StringSplitOptions.None);
            string result = null;

            for (int i = 0; i < values.Length; i++)
            {
                string v = values[i];
                FieldInfo fi = value.GetType().GetField(v);

                if (fi == null) continue;

                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes != null && attributes.Length > 0)
                    result += attributes[0].Description;
                else
                    result += v.ToString();

                if (i < (values.Length - 1))
                {
                    result += separator;
                }
            }

            return result;
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }
    }
}