using FGA.Utility;
using System;
using System.Linq;
using System.Xml.Linq;

namespace FileService.Utils
{
    public static class XmlRecordHelper
    {
        public static XElement ObtenerElementoRegistro(XDocument xDoc)
        {
            if (xDoc == null) return null;
            if (xDoc.Root != null && xDoc.Root.Name.LocalName.Equals("Registro", StringComparison.OrdinalIgnoreCase))
            {
                return xDoc.Root;
            }
            return xDoc.Descendants().FirstOrDefault(e => e.Name.LocalName.Equals("Registro", StringComparison.OrdinalIgnoreCase));
        }

        public static string GetVal(this XElement parent, string elementName, string defaultValue = "")
        {
            if (parent == null) return defaultValue;
            var el = parent.Elements().FirstOrDefault(e => e.Name.LocalName.Equals(elementName, StringComparison.OrdinalIgnoreCase));
            if (el == null || string.IsNullOrWhiteSpace(el.Value)) return defaultValue;
            return el.Value.Trim();
        }

        public static decimal GetDecimal(this XElement parent, string elementName, decimal defaultValue = 0, string errorContext = "")
        {
            string val = parent.GetVal(elementName);
            if (string.IsNullOrWhiteSpace(val)) return defaultValue;
            try
            {
                return Utilitarios.ConvertirADecimal(val, errorContext);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int GetInt(this XElement parent, string elementName, int defaultValue = 0)
        {
            string val = parent.GetVal(elementName);
            if (string.IsNullOrWhiteSpace(val)) return defaultValue;
            try
            {
                return Utilitarios.ConvertirAEntero(val);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int? GetNullableInt(this XElement parent, string elementName)
        {
            string val = parent.GetVal(elementName);
            if (string.IsNullOrWhiteSpace(val)) return null;
            try
            {
                return Utilitarios.ConvertirAEntero(val);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime GetDate(this XElement parent, string elementName, DateTime? defaultValue = null)
        {
            string val = parent.GetVal(elementName);
            if (string.IsNullOrWhiteSpace(val)) return defaultValue ?? new DateTime(1900, 1, 1);
            try
            {
                return Utilitarios.ConvertirAFecha(val);
            }
            catch
            {
                return defaultValue ?? new DateTime(1900, 1, 1);
            }
        }

        public static bool GetBool(this XElement parent, string elementName, bool defaultValue = false)
        {
            string val = parent.GetVal(elementName);
            if (string.IsNullOrWhiteSpace(val)) return defaultValue;
            if (val == "1" || val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("S", StringComparison.OrdinalIgnoreCase)) return true;
            if (val == "0" || val.Equals("false", StringComparison.OrdinalIgnoreCase) || val.Equals("N", StringComparison.OrdinalIgnoreCase)) return false;
            return defaultValue;
        }
    }
}
