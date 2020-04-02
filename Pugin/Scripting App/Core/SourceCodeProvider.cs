using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptingApp.Core
{
    internal static class SourceCodeProvider
    {
        internal static void WriteSourceCode(
            string tempFilePath,
            string inputClassObject,
            string outputClassObject,
            string inObject,
            StringBuilder dataInitialization,
            string code,
            Dictionary<string, string> outputFiles = null)
        {
            var logMethod = LogMethod();
            var saveToFile = SaveToFile();
            var saves = Saves(outputFiles);
            using (StreamWriter sw = new StreamWriter(tempFilePath))
            {
                sw.Write(
$@"using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WinFormCodeCompile
{{
    #region
    {inputClassObject}
    {outputClassObject}
    #endregion

    public class Transform
    {{
        {inObject}

        public Transform()
        {{
");
                sw.Write(dataInitialization?.ToString());
                sw.Write(@$"
        }}

        {logMethod}

        {saveToFile}

        public static T DeserializeXMLFileToObject<T>(string XmlFilename)
        {{
            T returnObject = default(T);
            if (string.IsNullOrEmpty(XmlFilename)) return default(T);

            try
            {{
                using (var xmlStream = new StreamReader(XmlFilename))
                {{
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    returnObject = (T)serializer.Deserialize(xmlStream);
                }}
            }}
            catch (Exception ex)
            {{
                Log(ex.Message);
            }}
            return returnObject;
        }}

        public void  UpdateText()
        {{
            {code}
            {saves}
        }}
    }}
}}");
            }
        }

        private static string Saves(Dictionary<string, string> outputFiles)
        {
            if (outputFiles == null || outputFiles.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var output in outputFiles)
            {
                sb.AppendLine($"Save({output.Key}, @\"{output.Value}\");");
            }

            return sb.ToString();
        }

        internal static StringBuilder GetInitializationCode(List<DynamicClass> inputDCObjects)
        {
            StringBuilder result = new StringBuilder();

            foreach (var rootObject in inputDCObjects.Where(o => o.ParentNode == null))
            {
                if (string.Equals(rootObject.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                {
                    result.AppendLine($"{rootObject.ClassName} = new List<{rootObject.GetFullClassName()}> {{");
                    result.AppendLine($"}};");
                }
                else
                {
                    result.AppendLine($"{rootObject.ClassName} = new {rootObject.GetFullClassName()} {{");
                    InitProp(inputDCObjects, result, rootObject);
                    result.AppendLine($"}};");
                }
            }

            return result;
        }

        private static void InitProp(List<DynamicClass> inputDCObjects, StringBuilder result, DynamicClass rootObject)
        {
            foreach (var child in inputDCObjects.Where(o => o.ParentNode == rootObject))
            {
                if (child.IsParent)
                {
                    if (string.Equals(child.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                    {
                        result.AppendLine($"{child.ClassName} = new List<{(child.IsParent ? child.GetFullClassName() : child.PropertyType)}> {{");
                        result.AppendLine($"}},");
                    }
                    else
                    {
                        result.AppendLine($"{child.ClassName} = new {child.GetFullClassName()} {{");
                        InitProp(inputDCObjects, result, child);
                        result.AppendLine($"}},");
                    }
                }
                else
                {
                    result.AppendLine($"{child.ClassName} = default({child.PropertyType}),");
                }
            }
        }

        internal static StringBuilder GetInitializationCodeUsingData(List<DynamicClass> inputDCObjects, Dictionary<string, string> inputFiles)
        {
            StringBuilder result = new StringBuilder();
            foreach (var rootObject in inputDCObjects.Where(o => o.ParentNode == null))
            {
                var childRootObject = inputDCObjects.FirstOrDefault(x => x.ParentNode == rootObject);

                if (childRootObject == null)
                {
                    continue;
                }

                if (string.Equals(rootObject.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                {
                    result.AppendLine($"{rootObject.ClassName} = new List<{rootObject.GetFullClassName()}> {{");
                    result.AppendLine($"new {rootObject.GetFullClassName()} {{");
                    result.AppendLine($"{childRootObject.PropertyName} = DeserializeXMLFileToObject<{childRootObject.GetFullClassName()}>(\"{inputFiles[rootObject.ClassName].Replace(@"\", @"\\")}\")");
                    result.AppendLine($"}},");
                    result.AppendLine($"}};");
                }
                else
                {
                    result.AppendLine($"{rootObject.ClassName} = new {rootObject.GetFullClassName()} {{");
                    result.AppendLine($"{childRootObject.PropertyName} = DeserializeXMLFileToObject<{childRootObject.GetFullClassName()}>(\"{inputFiles[rootObject.ClassName].Replace(@"\", @"\\")}\")");
                    result.AppendLine($"}};");
                }
            }

            return result;
        }

        private static string SaveToFile()
        {
            return
$@"public static void Save(object result, string path)
{{
    try
    {{
        using (var sw = new StreamWriter($@""{{path}}""))
        {{
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("""", """");
            var serializer = new XmlSerializer(result.GetType());
            serializer.Serialize(sw, result, ns);
            sw.Flush();
        }}
    }}
    catch (Exception ex) {{ Log(ex.Message); }}
}}";
        }

        private static string LogMethod()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"Log.txt";
            return
$@"public static void Log(object message)
{{
    try
    {{
        using (var sw = new StreamWriter(@""{path}"", true))
        {{
            sw.WriteLine(message);
        }}
    }}
    catch {{ }}
}}";
        }
    }
}
