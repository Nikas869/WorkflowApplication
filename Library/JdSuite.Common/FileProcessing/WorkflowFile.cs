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

        public WorkflowFile(string filePath, XDocument fileContent)
        {
            FilePath = filePath;
            xDocument = fileContent;
        }

        public void SaveAsXml(string fileName)
        {
            xDocument.Save(fileName);
        }
    }
}
