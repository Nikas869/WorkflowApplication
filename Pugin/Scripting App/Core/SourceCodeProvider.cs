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
            string logMethod = null)
        {
            logMethod ??= LogMethod();
            using (StreamWriter sw = new StreamWriter(tempFilePath))
            {
                sw.Write(
$@"using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

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

        public void  UpdateText()
        {{
            {code}
        }}
    }}
}}");
            }
        }

        internal static StringBuilder GetInitializationDataCode(List<DynamicClassProperties> inputDCObjects, XElement data)
        {
            StringBuilder result = new StringBuilder();
            var rootObject = inputDCObjects[0];

            result.AppendLine($"{rootObject.PropertyName} = new {rootObject.GetFullClassName()} {{");
            FillData(inputDCObjects, result, rootObject, data);
            result.Append($"}};");

            return result;
        }

        private static void FillData(List<DynamicClassProperties> inputDCObjects, StringBuilder result, DynamicClassProperties rootObject, XElement data)
        {
            foreach (var child in inputDCObjects.Where(o => o.ParentNode == rootObject))
            {
                if (child.IsParent)
                {
                    if (string.Equals(child.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                    {
                        result.AppendLine($"{child.ClassName} = new List<{child.GetFullClassName()}> {{");
                        FillData(inputDCObjects, result, child, data);
                        result.AppendLine($"}}");
                    }
                    else
                    {
                        result.AppendLine($"{child.ClassName} = new {child.GetFullClassName()} {{");
                        FillData(inputDCObjects, result, child, data);
                        result.AppendLine($"}}");
                    }
                }
                else
                {
                    foreach (var dataItem in data.Elements(rootObject.ClassName))
                    {
                        result.AppendLine($"new {rootObject.GetFullClassName()} {{");
                        foreach (var prop in inputDCObjects.Where(o => o.ParentNode == rootObject))
                        {
                            result.AppendLine($"{prop.ClassName} = @\"{dataItem.Element(prop.ClassName)?.Value ?? dataItem.Attribute(prop.ClassName)?.Value ?? string.Empty}\",");
                        }
                        result.AppendLine($"}},");
                    }
                }
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
