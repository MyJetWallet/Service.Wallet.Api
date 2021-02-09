using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;

namespace Service.Wallet.Api.Services
{
#pragma warning disable
    [UsedImplicitly]
    public static class ModelRenderer
    {

        private static readonly Dictionary<Type, string> TypescriptTypes = new Dictionary<Type, string>
        {
            [typeof(byte)] = "number",

            [typeof(short)] = "number",
            [typeof(ushort)] = "number",

            [typeof(int)] = "number",
            [typeof(uint)] = "number",

            [typeof(long)] = "number",
            [typeof(ulong)] = "number",

            [typeof(double)] = "number",
            [typeof(float)] = "number",
            [typeof(decimal)] = "number",

            [typeof(bool)] = "boolean",
            [typeof(string)] = "string",
            [typeof(DateTime)] = "date",
        };

        private static string GetTypescriptType(this Type tp)
        {
            if (TypescriptTypes.ContainsKey(tp))
                return TypescriptTypes[tp];


            if (tp == typeof(string))
                return "string";

            if (tp.IsClass)
            {
                return tp.RenderHtmlClass(10);
            }

            if (tp.IsGenericType && tp.GetInterfaces().Any(itm => itm == typeof(IEnumerable)))
            {

                var genericType = tp.GetGenericArguments()[0];

                return "[" + genericType.RenderHtmlClass(10) + "]";

            }

            if (tp.IsGenericType &&
                tp.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var gen = tp.GetGenericArguments()[0];
                return "?" + gen.RenderHtmlClass(10);
            }

            return "string";
        }

        private static string LowerCaseFirstString(this string str)
        {
            return ("" + str[0]).ToLowerInvariant() + str.Substring(1, str.Length - 1);
        }

        public static string RenderHtmlClass(this Type type, int padding)
        {
            if (TypescriptTypes.ContainsKey(type))
                return TypescriptTypes[type];

            var result = new StringBuilder();

            var paddingStr = "class=\"" + padding + "px\"";

            result.Append("<div " + paddingStr + ">{</div>");

            foreach (var pi in type.GetProperties())
            {
                result.Append("<div style=\"padding-left:" + (padding + 10) + "px;\">" + pi.Name.LowerCaseFirstString() + ": " + pi.PropertyType.GetTypescriptType() + "</div>");
            }

            result.Append("<div " + paddingStr + ">}</div>");

            return result.ToString();

        }

        public static IHtmlContent RenderModelHtml(this Type type)
        {
            var result = new StringBuilder();

            if (TypescriptTypes.ContainsKey(type))
            {
                result.Append(TypescriptTypes[type]);

            }
            else
            {
                result.Append("{");
                foreach (var pi in type.GetProperties())
                {
                    result.Append("<div style=\"padding-left:10px;\">" + pi.Name.LowerCaseFirstString() + ": " + pi.PropertyType.GetTypescriptType() + "</div>");
                }

                result.Append("}");
            }

            return new HtmlString(result.ToString());
        }

    }
}