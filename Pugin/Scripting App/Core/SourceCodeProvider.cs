using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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
            string logMethod = null,
            string saveFilePath = null)
        {
            logMethod ??= LogMethod();
            var saveToFile = SaveToFile(saveFilePath);
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
        }}
    }}
}}");
            }
        }

        internal static StringBuilder GetInitializationCode(List<DynamicClass> inputDCObjects)
        {
            StringBuilder result = new StringBuilder();
            var rootObject = inputDCObjects[0];

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

        internal static StringBuilder GetInitializationCodeUsingData(List<DynamicClass> inputDCObjects, string filePath)
        {
            StringBuilder result = new StringBuilder();
            var rootObject = inputDCObjects[0];
            var childRootObject = inputDCObjects.FirstOrDefault(x => x.ParentNode == rootObject);

            if (string.Equals(rootObject.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
            {
                result.AppendLine($"{rootObject.ClassName} = new List<{rootObject.GetFullClassName()}> {{");
                result.AppendLine($"new {rootObject.GetFullClassName()} {{");
                result.AppendLine($"{childRootObject.PropertyName} = DeserializeXMLFileToObject<{childRootObject.GetFullClassName()}>(\"{filePath.Replace(@"\", @"\\")}\")");
                result.AppendLine($"}},");
                result.AppendLine($"}};");
            }
            else
            {
                result.AppendLine($"{rootObject.ClassName} = new {rootObject.GetFullClassName()} {{");
                result.AppendLine($"{childRootObject.PropertyName} = DeserializeXMLFileToObject<{childRootObject.GetFullClassName()}>(\"{filePath.Replace(@"\", @"\\")}\")");
                result.AppendLine($"}};");
            }

            return result;
        }

        private static string SaveToFile(string saveFilePath)
        {
            if (string.IsNullOrEmpty(saveFilePath))
            {
                return string.Empty;
            }
            else
            {
                return
$@"public static void Save(object result)
{{
    try
    {{
        using (var sw = new StreamWriter(@""{saveFilePath}""))
        {{
            var serializer = new XmlSerializer(result.GetType());
            serializer.Serialize(sw, result);
            sw.Flush();
        }}
    }}
    catch (Exception ex) {{ Log(ex.Message); }}
}}";
            }
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
