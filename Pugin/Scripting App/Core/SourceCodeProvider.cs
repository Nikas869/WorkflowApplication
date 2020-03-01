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

        internal static StringBuilder GetInitializationCode(List<DynamicClassProperties> inputDCObjects)
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

        private static void InitProp(List<DynamicClassProperties> inputDCObjects, StringBuilder result, DynamicClassProperties rootObject)
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

        internal static StringBuilder GetInitializationCodeUsingData(List<DynamicClassProperties> inputDCObjects, XElement data)
        {
            StringBuilder result = new StringBuilder();
            var rootObject = inputDCObjects[0];

            if (string.Equals(rootObject.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
            {
                result.AppendLine($"{rootObject.ClassName} = new List<{rootObject.GetFullClassName()}> {{");
                result.AppendLine($"new {rootObject.GetFullClassName()} {{");
                InitPropWithData(inputDCObjects, result, rootObject, data);
                result.AppendLine($"}},");
                result.AppendLine($"}};");
            }
            else
            {
                result.AppendLine($"{rootObject.ClassName} = new {rootObject.GetFullClassName()} {{");
                InitPropWithData(inputDCObjects, result, rootObject, data);
                result.AppendLine($"}};");
            }

            return result;
        }

        private static void InitPropWithData(List<DynamicClassProperties> inputDCObjects, StringBuilder result, DynamicClassProperties rootObject, XElement data)
        {
            foreach (var child in inputDCObjects.Where(o => o.ParentNode == rootObject))
            {
                if (child.IsParent)
                {
                    if (string.Equals(child.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                    {
                        result.AppendLine($"{child.ClassName} = new List<{child.GetFullClassName()}> {{");
                        InitPropWithData(inputDCObjects, result, child, data);
                        result.AppendLine($"}}");
                    }
                    else
                    {
                        result.AppendLine($"{child.ClassName} = new {child.GetFullClassName()} {{");
                        InitPropWithData(inputDCObjects, result, child, data);
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
