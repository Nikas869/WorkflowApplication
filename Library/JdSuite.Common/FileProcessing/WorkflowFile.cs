using JdSuite.Common.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace JdSuite.Common.FileProcessing
{
    public class WorkflowFile
    {
        private XDocument xDocument;

        public string FilePath { get; }

        public string FileName => Path.GetFileNameWithoutExtension(FilePath);

        public string FileExtension => Path.GetExtension(FilePath);

        public XElement FileContent => xDocument.Root;

        public EventHandler<float> OnValidationProgressChange;

        public WorkflowFile(string filePath, XDocument fileContent)
        {
            FilePath = filePath;
            xDocument = fileContent;
        }

        public bool ValidateUsingSchema(Field schema, int totalNodeCount, out List<string> errors)
        {
            var validator = new XMLNodeValidator();
            var isValid = false;
            var currentNodeCount = 0;
            var lastProgress = 0.0f;
            errors = new List<string>();

            foreach (var FirstLevelSchemaNode in schema.ChildNodes)
            {
                foreach (var xmlDataNode in GetNodes(schema.Alias, FirstLevelSchemaNode.Name))
                {
                    currentNodeCount++;

                    float currentProgress = 100 * currentNodeCount / totalNodeCount;

                    if (currentProgress > lastProgress)
                    {
                        lastProgress = currentProgress;
                        OnValidationProgressChange?.Invoke(this, currentProgress);
                    }

                    isValid = validator.Validate(FirstLevelSchemaNode, xmlDataNode);

                    if (isValid == false)
                    {
                        errors.AddRange(validator.ErrorList);

                        break;
                    }
                }

                if (isValid == false)
                {
                    break;
                }
            }

            return isValid;
        }

        public void SaveAsXml(string fileName)
        {
            xDocument.Save(fileName);
        }

        private IEnumerable<XElement> GetNodes(string ParentName, string ChildNodeName)
        {
            foreach (var pnode in xDocument.Elements(ParentName))
            {
                foreach (var node in pnode.Elements(ChildNodeName))
                {
                    yield return node;
                }
            }
        }
    }
}
