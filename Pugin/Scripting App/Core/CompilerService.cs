using JdSuite.Common.Module;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ScriptingApp.Core
{
    internal static class CompilerService
    {
        public static CompilerResults GenerateCodeAndCompile(Field inputSchema, Field outputSchema, string code, XElement data = null)
        {
            List<string> initializeObjects = new List<string>();

            ////TreeView Input Class Object
            var (InpuntClassObject, inputDCObjects) = CreateDynamicClassObjects(inputSchema, initializeObjects);

            ////TreeView Output Class Object
            var (OutputClassObject, outputDCObjects) = CreateDynamicClassObjects(outputSchema, initializeObjects);

            //Get Initialize Objects into String
            string inObject = "";
            foreach (var item in initializeObjects)
            {
                inObject = inObject + item + " " + item + " = " + " new " + item + "();" + Environment.NewLine;
            }

            StringBuilder dataInitialization = null;

            if (data != null)
            {
                dataInitialization = SourceCodeProvider.GetInitializationDataCode(inputDCObjects, data);
            }

            var sourceFile = ConfigurationManager.AppSettings["SourceCodeTempFile"];
            SourceCodeProvider.WriteSourceCode(sourceFile, InpuntClassObject, OutputClassObject, inObject, dataInitialization, code);

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
            cParams.GenerateExecutable = false;
            cParams.GenerateInMemory = true;

            return cProv.CompileAssemblyFromFile(cParams, filePath);
        }

        private static (string, List<DynamicClassProperties>) CreateDynamicClassObjects(Field schema, List<string> initializeObjects)
        {
            schema.Parent = null;
            var DCObject = new List<DynamicClassProperties>();
            foreach (Field node in schema.ChildNodes)
            {
                CreateDefinitionDynamicClassProperties(node, null, DCObject);
            }

            var classesCode = CreateDynamicClasses(DCObject, initializeObjects).ToString();

            return (classesCode, DCObject);
        }

        private static void CreateDefinitionDynamicClassProperties(Field node, DynamicClassProperties parent, List<DynamicClassProperties> prop)
        {
            DynamicClassProperties newProp = null;
            if (node != null)
            {
                newProp = new DynamicClassProperties
                {
                    PropertyName = node.Alias ?? node.Name,
                    PropertyType = node.DataType,
                    ParentNode = parent,
                    ClassName = node.Alias ?? node.Name,
                    IsParent = node.HasChildNodes()
                };
                prop.Add(newProp);
            }
            foreach (var subNode in node.ChildNodes)
            {
                CreateDefinitionDynamicClassProperties(subNode, newProp, prop);
            }
        }

        private static StringBuilder CreateDynamicClasses(List<DynamicClassProperties> DCObject, List<string> initializeObjects)
        {
            StringBuilder builder = new StringBuilder();
            // Get All Classess
            var classes = DCObject.Where(x => x.IsParent == true).ToList();
            string classNameType = classes[0].PropertyName;
            initializeObjects.Add(classNameType);
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
                            builder.Append(GetListProperty(cp));
                        }
                        else
                        {
                            builder.AppendFormat("public ").Append(cp.GetFullClassName()).Append($"{(string.Equals(cp.PropertyType.ToString(), "Array", StringComparison.OrdinalIgnoreCase) ? "[] " : " ")}").Append(cp.PropertyName).Append(" { get; set; }").Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        builder.AppendFormat("public ").Append(cp.PropertyType + " ").Append(cp.PropertyName).Append(" { get; set; }").Append(Environment.NewLine);
                    }
                }
                builder.Append(" } ");
                builder.Append(Environment.NewLine);
            }

            return builder;
        }

        private static string GetListProperty(DynamicClassProperties cp)
        {
            return $"public List<{cp.GetFullClassName()}> {cp.PropertyName} {{ get; set; }} = new List<{cp.GetFullClassName()}>();{Environment.NewLine}";
        }
    }
}
