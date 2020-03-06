﻿using JdSuite.Common.FileProcessing;
using JdSuite.Common.Module;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ScriptingApp.Core
{
    internal static class CompilerService
    {
        public static CompilerResults GenerateCodeAndCompile(Field inputSchema, Field outputSchema, string code, WorkflowFile file = null)
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
                if (string.Equals(item.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase))
                {
                    inObject.AppendLine($"List<{item.GetFullClassName()}> {item.GetFullClassName()} = new List<{item.GetFullClassName()}>();");
                }
                else
                {
                    inObject.AppendLine($"{item.GetFullClassName()} {item.GetFullClassName()} = new {item.GetFullClassName()}();");
                }
            }

            StringBuilder dataInitialization = new StringBuilder();

            if (file != null)
            {
                dataInitialization
                    .Append(SourceCodeProvider.GetInitializationCodeUsingData(inputDCObjects, file.RootNode))
                    .Append(Environment.NewLine)
                    .Append(SourceCodeProvider.GetInitializationCodeUsingData(outputDCObjects, file.RootNode));
            }
            else
            {
                dataInitialization
                    .Append(SourceCodeProvider.GetInitializationCode(inputDCObjects))
                    .Append(Environment.NewLine)
                    .Append(SourceCodeProvider.GetInitializationCode(outputDCObjects));
            }

            var sourceFile = ConfigurationManager.AppSettings["SourceCodeTempFile"];
            SourceCodeProvider.WriteSourceCode(
                sourceFile,
                InpuntClassObject,
                OutputClassObject,
                inObject.ToString(),
                dataInitialization,
                code,
                saveFilePath: file?.FilePath);

            return Compile(sourceFile);
        }

        /// <summary>
        /// Just compiles source code and returns compilation result
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static CompilerResults Compile(string filePath)
        {
            CSharpCodeProvider cProv = new CSharpCodeProvider();
            CompilerParameters cParams = new CompilerParameters();
            cParams.ReferencedAssemblies.Add("mscorlib.dll");
            cParams.ReferencedAssemblies.Add("System.dll");
            cParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            cParams.ReferencedAssemblies.Add("System.Xml.dll");
            cParams.GenerateExecutable = false;
            cParams.GenerateInMemory = true;

            return cProv.CompileAssemblyFromFile(cParams, filePath);
        }

        private static (string, List<DynamicClass>) CreateDynamicClassObjects(Field schema, List<DynamicClass> initializeObjects)
        {
            var classes = new List<DynamicClass>();

            foreach (Field input in schema.ChildNodes)
            {
                ExtractClassInfo(input, null, classes);
            }

            var classesCode = CreateDynamicClasses(classes, initializeObjects).ToString();

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

        private static StringBuilder CreateDynamicClasses(List<DynamicClass> DCObject, List<DynamicClass> initializeObjects)
        {
            StringBuilder builder = new StringBuilder();
            // Get All Classess
            var classes = DCObject.Where(x => x.IsParent == true).ToList();
            string classNameType = classes[0].PropertyName;
            initializeObjects.Add(classes[0]);
            foreach (var c in classes)
            {
                string className = c.GetFullClassName();

                // Create Class Object
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
