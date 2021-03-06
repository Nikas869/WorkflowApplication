﻿using JdSuite.Common.Module;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptingApp.Core
{
    internal static class CompilerService
    {
        public static (CompilerResults CompilerResult, int CodeLine) GenerateCodeAndCompile(
            string sourceFile,
            Field inputSchema,
            Field outputSchema,
            string code,
            Dictionary<string, string> inputFiles = null,
            Dictionary<string, string> outputFiles = null)
        {
            List<DynamicClass> initializeObjects = new List<DynamicClass>();

            ////TreeView Input Class Object
            var (InpuntClassObject, inputDCObjects) = CreateDynamicClassObjects(inputSchema, initializeObjects);

            ////TreeView Output Class Object
            var (OutputClassObject, outputDCObjects) = CreateDynamicClassObjects(outputSchema, initializeObjects);

            //Get Initialize Objects into String
            StringBuilder inObject = new StringBuilder();
            foreach (var item in initializeObjects)
            {
                inObject.AppendLine($"{item.GetFullClassName()} {item.GetFullClassName()} = new {item.GetFullClassName()}();");
            }

            StringBuilder dataInitialization = new StringBuilder();

            if (inputFiles != null)
            {
                dataInitialization
                    .Append(SourceCodeProvider.GetInitializationCodeUsingData(inputDCObjects, inputFiles))
                    .Append(Environment.NewLine)
                    .Append(SourceCodeProvider.GetInitializationCode(outputDCObjects));
            }
            else
            {
                dataInitialization
                    .Append(SourceCodeProvider.GetInitializationCode(inputDCObjects))
                    .Append(Environment.NewLine)
                    .Append(SourceCodeProvider.GetInitializationCode(outputDCObjects));
            }

            SourceCodeProvider.WriteSourceCode(
                sourceFile,
                InpuntClassObject,
                OutputClassObject,
                inObject.ToString(),
                dataInitialization,
                code,
                outputFiles);

            return Compile(sourceFile);
        }

        /// <summary>
        /// Just compiles source code and returns compilation result
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static (CompilerResults CompilerResult, int CodeLine) Compile(string filePath)
        {
            CSharpCodeProvider cProv = new CSharpCodeProvider();
            CompilerParameters cParams = new CompilerParameters();
            cParams.ReferencedAssemblies.Add("mscorlib.dll");
            cParams.ReferencedAssemblies.Add("System.dll");
            cParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            cParams.ReferencedAssemblies.Add("System.Xml.dll");
            cParams.GenerateExecutable = false;
            cParams.GenerateInMemory = true;
            cParams.IncludeDebugInformation = true;

            var lines = File.ReadAllLines(filePath).ToList();
            var index = lines.FindIndex(line => line.Contains("public void UpdateText()"));

            return (cProv.CompileAssemblyFromFile(cParams, filePath), index + 2);
        }

        private static (string, List<DynamicClass>) CreateDynamicClassObjects(Field schema, List<DynamicClass> initializeObjects)
        {
            var classes = new List<DynamicClass>();

            foreach (Field input in schema.ChildNodes)
            {
                ExtractClassInfo(input, null, classes);
            }

            initializeObjects.AddRange(classes.Where(cl => cl.ParentNode == null));

            var classesCode = CreateDynamicClasses(classes).ToString();

            return (classesCode, classes);
        }

        private static void ExtractClassInfo(Field node, DynamicClass parent, List<DynamicClass> prop)
        {
            DynamicClass newProp = null;
            if (node != null)
            {
                newProp = new DynamicClass
                {
                    PropertyName = node.Name,
                    PropertyType = node.DataType,
                    ParentNode = parent,
                    ClassName = node.Alias,
                    IsParent = node.HasChildNodes(),
                    XmlType = node.Type != null ? (XMLType)Enum.Parse(typeof(XMLType), node.Type) : XMLType.Element
                };
                prop.Add(newProp);
            }
            foreach (var subNode in node.ChildNodes)
            {
                ExtractClassInfo(subNode, newProp, prop);
            }
        }

        private static StringBuilder CreateDynamicClasses(List<DynamicClass> DCObject)
        {
            StringBuilder builder = new StringBuilder();

            string classNameType = DCObject[0].PropertyName;
            foreach (var c in DCObject)
            {
                string className = c.GetFullClassName();

                // Create Class Object
                builder.AppendLine($"[XmlRoot(ElementName = \"{c.ClassName}\")]");
                builder.AppendFormat("public class ").Append(className).Append(" { ");
                // Get Parent and Child
                var Parent_Child = DCObject.Where(x => x.ParentNode == c).ToList();

                foreach (var cp in Parent_Child)
                {
                    if (cp.IsParent)
                    {
                        if (string.Equals(cp.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                        {
                            builder.AppendLine(GetXmlAttribute(cp));
                            builder.AppendLine($"public List<{cp.GetFullClassName()}> {cp.PropertyName} {{ get; set; }} = new List<{cp.GetFullClassName()}>();");
                        }
                        else
                        {
                            builder.AppendLine(GetXmlAttribute(cp));
                            builder.AppendFormat("public ").Append(cp.GetFullClassName()).Append($"{(string.Equals(cp.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase) ? "[] " : " ")}").Append(cp.PropertyName).Append(" { get; set; }").Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        builder.AppendLine(GetXmlAttribute(cp));
                        builder.AppendFormat("public ").Append(cp.PropertyType + " ").Append(cp.PropertyName).Append(" { get; set; }").Append(Environment.NewLine);
                    }
                }
                builder.Append(" } ");
                builder.Append(Environment.NewLine);
            }

            return builder;
        }

        private static string GetXmlAttribute(DynamicClass prop)
        {
            switch (prop.XmlType)
            {
                case XMLType.Element:
                    return $@"[XmlElement(""{prop.ClassName}"")]";
                case XMLType.Attribute:
                    return $@"[XmlAttribute(""{prop.ClassName}"")]";
                case XMLType.PCData:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
