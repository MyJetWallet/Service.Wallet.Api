using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Service.Wallet.Api.Services;

namespace Service.Wallet.Api.Hubs
{
#pragma warning disable

    public class SignalRIncomingRequest : Attribute
    {
    }

    public class SignalrOutcomming : Attribute
    {

        public SignalrOutcomming(string topicName)
        {
            TopicName = topicName;
        }


        public string TopicName { get; }


    }

    [UsedImplicitly]
    public static class SignalrDocumentationGenerator
    {

        private static bool HasDocumentationAttr(this IEnumerable<CustomAttributeData> attrs)
        {
            return attrs.Any(itm => itm.AttributeType == typeof(SignalRIncomingRequest));
        }

        private static void GenerateIncomingMethods(this StringBuilder sb, Type typeOfHub)
        {
            sb.Append("<hr/>");
            sb.Append("<h1>Client to Server</h1>");
            sb.Append("<hr/>");

            foreach (var method in typeOfHub.GetMethods().Where(method => method.CustomAttributes.HasDocumentationAttr()))
            {

                try
                {
                    var parameters = method.GetParameters();


                    sb.Append("<h3>" + method.Name + "(");

                    if (parameters.Length == 1)
                    {
                        sb.Append(parameters[0].ParameterType.RenderModelHtml());
                    }

                    sb.Append(")</h3>");
                }
                finally
                {
                    sb.Append("<hr/>");
                }

            }

        }


        public static void RenderTopics(this StringBuilder sb, Type typeOfHub)
        {
            sb.Append("<hr/>");
            sb.Append("<h1>Server to Client</h1>");
            sb.Append("<hr/>");
            var assembly = typeOfHub.Assembly;


            foreach (var type in assembly.GetTypes())
            {

                var attr = type.GetCustomAttribute<SignalrOutcomming>();

                if (attr == null)
                    continue;

                sb.Append("<h3>Method: " + attr.TopicName + "</h3>");

                sb.Append("<pre><code>");


                if (type.GetProperties().Length > 0)
                {
                    sb.Append(type.RenderHtmlClass(10));
                }
                sb.Append("</code></pre>");

                sb.Append("<hr/>");
            }
        }


        public static string GenerateDocumentation(Type typeOfHub)
        {
            var sb = new StringBuilder();
            sb.GenerateIncomingMethods(typeOfHub);
            sb.RenderTopics(typeOfHub);
            return sb.ToString();
        }

    }
}