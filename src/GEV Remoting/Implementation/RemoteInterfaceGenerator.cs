using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GEV.Remoting.Implementation
{
    /// <summary>
    /// Generates a local proxy-class that communicates with the remtoe service.
    /// </summary>
    internal class RemoteInterfaceGenerator
    {
        internal static T GenerateService<T>(string host, int port) where T : IRemoteService
        {
            MethodInfo[] methodInfos = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance);

            StringBuilder sourceCode = new StringBuilder();

            foreach (MethodInfo method in methodInfos)
            {
                if(!method.IsSpecialName)
                {
                    sourceCode.Append(GenerateSourceForMethod(method));
                }
            }

            string typeName = String.Format("GeneratedRemoteServiceClass_{0}", new Random().Next());

            string envelope = "using System;\n" +
                              "using GEV.Remoting;\n" +
                              "using System.Collections.Generic;\n\n" +
                              "public class {0} : {1}\n" +
                              "{{\n" +
                              "    private ServiceSubscriber m_Proxy = new ServiceSubscriber(\"{3}\", {4});\n\n" +
                              "{2}\n" +
                              "}}\n" +
                              "\n" +
                              "return new {0}();";

            string completeSource =  String.Format(envelope, typeName, typeof(T).FullName, sourceCode.ToString(), host, port);

            ScriptOptions options = ScriptOptions.Default;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            options = options.AddReferences(assemblies);
            options = options.WithEmitDebugInformation(true);

            Script script = CSharpScript.Create<T>(completeSource, options);
            Task<ScriptState> run = script.RunAsync();
            run.Wait();
            return (T)run.Result.ReturnValue;
        }

        private static string GenerateSourceForMethod(MethodInfo info)
        {
            //TODO: [KG] No exception handling :(
            string result = "";
            if (info.ReturnType != typeof(void))
            {
                result = "public {0} {1}({2})\n" +
                         "{{\n" +
                         "   object o =  this.m_Proxy.CallMethod(\"{1}\", new Dictionary<string, object>()\n" +
                         "   {{\n" +
                         "{3}\n" +
                         "   }});\n" +
                         "   if(o is {0})\n" +
                         "   {{\n" +
                         "       return ({0})o;\n" +
                         "   }}\n" +
                         "   else\n" +
                         "   {{\n" +
                         "       throw new Exception(\"TODO!\");\n" +
                         "   }}\n" +
                         "}}\n" +
                         "\n";
            }
            else
            {
                result = "public {0} {1}({2})\n" +
                         "{{\n" +
                         "   this.m_Proxy.CallMethod(\"{1}\", new Dictionary<string, object>()\n" +
                         "   {{\n" +
                         "{3}\n" +
                         "   }});\n" +
                         "}}\n" +
                         "\n";
            }
            List<string> parameters = new List<string>();
            List<string> callingParameters = new List<string>();

            foreach (var param in info.GetParameters())
            {
                parameters.Add(String.Format("{0} {1}", param.ParameterType.FullName, param.Name));
                callingParameters.Add(String.Format("       {{ \"{0}\", {0} }},", param.Name));
            }

            string returnType = GetTypeString(info.ReturnParameter.ParameterType);
            if (returnType.ToLower() == "system.void")
            {
                returnType = "void";
            }

            result = String.Format(result, returnType, info.Name, String.Join(", ", parameters), String.Join("\n", callingParameters));

            return result;
        }

        private static string GenerateSourceForProperty(PropertyInfo info)
        {
            string result = "public {0} {1}\n" +
                            "{{\n" +
                            "   {2}\n" +
                            "   {3}\n" +
                            "}}\n" +
                            "\n";

            string getter = "";
            string setter = "";

            if(info.CanRead)
            {
                getter = "get\n" +
                         "{\n" +
                         "  throw new Exception(\"TODO!\");\n" +
                         "}\n";
            }

            if(info.CanWrite)
            {
                setter = "set\n" +
                         "{\n" +
                         "  throw new Exception(\"TODO!\");\n" +
                         "}\n";
            }


            result = String.Format(result, GetTypeString(info.PropertyType), info.Name, getter, setter);

            return result;
        }

        public static string GetTypeString(Type t)
        {
            if(t.GenericTypeArguments.Length == 0)
            {
                return t.Namespace + "." + t.Name;
            }
            else
            {
                List<string> generics = new List<string>();
                foreach(Type generic in t.GenericTypeArguments)
                {
                    generics.Add(GetTypeString(generic));
                }

                return String.Format("{0}.{1}<{2}>", t.Namespace, t.Name.Split('`')[0], String.Join(", ", generics));
            }
        }
    }
}
