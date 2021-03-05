using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BalsamicSolutions.ApiCommon.Extensions
{
    public static class Extensions
    {
        /// <summary>
        ///  Expand an exception into a readable string
        /// </summary>
        /// <param name="thisException"></param>
        /// <returns></returns>
        public static string ExceptionText(this Exception thisException)
        {
            return GetExceptionText(thisException, true, true, false);
        }

        /// <summary>
        ///  Expand an exception into a readable string
        /// </summary>
        /// <param name="thisException"></param>
        /// <param name="withStackTrace"></param>
        /// <param name="includeInnerException"></param>
        /// <returns></returns>
        public static string ExceptionText(this Exception thisException, bool withStackTrace, bool includeInnerException)
        {
            return GetExceptionText(thisException, withStackTrace, includeInnerException, false);
        }

        /// <summary>
        ///  Expand an exception into a readable string
        /// </summary>
        /// <param name="thisException"></param>
        /// <param name="withStackTrace"></param>
        /// <param name="includeInnerException"></param>
        /// <returns></returns>
        public static string ExceptionText(this Exception thisException, bool withStackTrace, bool includeInnerException, bool webFormated)
        {
            return GetExceptionText(thisException, withStackTrace, includeInnerException, webFormated);
        }

        /// <summary>
        /// Deserializes json text to a typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonEncodedObject"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string thisStr)
        {
            return thisStr.FromJson<T>(false);
        }

        /// <summary>
        /// Deserializes json text to a typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="thisStr"></param>
        /// <param name="includeTypes"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string thisStr, bool includeTypes)
        {
            if (thisStr.IsNullOrWhiteSpace())
            {
                return default(T);
            }
            else
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
                jsonSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                if (includeTypes) jsonSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(thisStr, jsonSerializerSettings);
            }
        }

        /// <summary>
        /// Deserializes json text to a dynamic JObject
        /// </summary>
        /// <param name="thisStr"></param>
        /// <returns></returns>
        public static object FromJson(this string thisStr)
        {
            Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            jsonSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            return Newtonsoft.Json.JsonConvert.DeserializeObject(thisStr, jsonSerializerSettings);
        }

        /// <summary>
        /// Expand an exception into a readable string
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="withStackTrace"></param>
        /// <param name="includeInnerException"></param>
        /// <returns></returns>
        public static string GetExceptionText(Exception ex, bool withStackTrace, bool includeInnerException, bool webFormated)
        {
            StringBuilder returnValue = new StringBuilder();
            Exception innerException = ex.InnerException;

            returnValue.AppendLine(ex.Message);
            if (withStackTrace)
            {
                returnValue.AppendLine(ex.StackTrace);
                if (webFormated)
                {
                    returnValue.AppendLine("<b/>");
                }
            }

            while (innerException != null && includeInnerException)
            {
                returnValue.AppendLine(innerException.Message);
                {
                    returnValue.AppendLine("<b/>");
                }
                if (withStackTrace)
                {
                    returnValue.AppendLine(innerException.StackTrace);
                    {
                        returnValue.AppendLine("<b/>");
                    }
                }

                innerException = innerException.InnerException;
            }

            return returnValue.ToString();
        }

        /// <summary>
        /// null or ""
        /// </summary>
        /// <param name="thisStr"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this String thisStr)
        {
            return string.IsNullOrEmpty(thisStr);
        }

        /// <summary>
        /// null or only white space
        /// </summary>
        /// <param name="thisStr"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this String thisStr)
        {
            return string.IsNullOrWhiteSpace(thisStr);
        }

        /// <summary>
        ///returns the last element of a string split
        /// </summary>
        /// <param name="thisStr"></param>
        /// <returns></returns>
        public static string LastSplitElement(this string thisStr, char delimiter)
        {
            if (null == thisStr) return null;
            string[] strParts = thisStr.Split(new char[] { delimiter });
            return strParts[strParts.Length - 1];
        }

        /// <summary>
        /// Serialize an object to json text
        /// </summary>
        /// <param name="thisStr"></param>
        /// <returns></returns>
        public static string ToJson(this object thisObj)
        {
            return ToJson(thisObj, true, false);
        }

        /// <summary>
        /// Serialize an object to json text
        /// </summary>
        /// <param name="thisStr"></param>
        /// <returns></returns>
        public static string ToJson(this object thisObj, bool ignoreNulls)
        {
            return ToJson(thisObj, ignoreNulls, false);
        }

        /// <summary>
		/// Serialize an object to json text
		/// </summary>
		/// <param name="thisStr"></param>
		/// <returns></returns>
		public static string ToJson(this object thisObj, bool ignoreNulls, bool includeTypes)
        {
            Newtonsoft.Json.JsonSerializerSettings jsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            jsonSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            if (ignoreNulls) jsonSerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            if (includeTypes) jsonSerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
            return Newtonsoft.Json.JsonConvert.SerializeObject(thisObj, Newtonsoft.Json.Formatting.Indented, jsonSerializerSettings);
        }

        /// <summary>
        /// shorten a string to a provided maximum  length
        /// </summary>
        /// <param name="thisStr"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string TrimTo(this string thisStr, int maxLength, bool addEllipsis = false)
        {
            if (string.IsNullOrEmpty(thisStr))
            {
                return string.Empty;
            }
            if (addEllipsis)
            {
                maxLength -= 3;
            }
            if (thisStr.Length <= maxLength)
            {
                return thisStr;
            }
            string returnValue = thisStr.Substring(0, maxLength);
            if (addEllipsis)
            {
                returnValue += "...";
            }
            return returnValue;
        }

        

        /// <summary>
		/// just what it says, case insensitive equality check
		/// </summary>
		/// <param name="thisStr"></param>
		/// <param name="compareTo"></param>
		/// <returns></returns>
		public static bool CaseInsensitiveEquals(this String thisStr, string compareTo)
        {
            if (null == thisStr && null == compareTo)
            {
                return true;
            }
            if (null == thisStr || null == compareTo)
            {
                return false;
            }
            return thisStr.Equals(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }

       
    }
}
