using JdSuite.Common.FileProcessing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CSVInput
{
    public class CSVParser
    {
        NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();
        public string SourceFileName { get; set; }
        public string XMLRootName { get; set; } = "Root";
        public string Delimiter { get; set; } = ",";
        public XElement RootNode { get; set; }

        public List<string> Headers { get; private set; } = new List<string>();
        public bool FirstLineHasHeaders { get; set; } = false;
        public Encoding FileEncoding { get; set; }

        /// <summary>
        /// If first line has headers and Headers are already set then set this property true
        /// </summary>
        public bool SkipFirstLine { get; set; } = false;


        public CSVParser()
        {
            FileEncoding = Encoding.UTF8;
        }

        private void InitializeHeaders(int Count)
        {
            logger.Info("Entered");

            Headers.Clear();
            for (int i = 0; i < Count; i++)
            {
                Headers[i] = "F" + i.ToString();

            }

        }

        public void ParseHeader()
        {
            logger.Info("Entered");

            using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(SourceFileName, FileEncoding))
            {

                parser.Delimiters = new string[] { Delimiter };
                var rawheaders = parser.ReadFields();
                var cleanedHeader = ParserBase.GetCleanHeaders(rawheaders);
                this.Headers.Clear();
                this.Headers.AddRange(cleanedHeader);
            }
        }




        private void LogHeaders()
        {
            var headers = string.Join("###", this.Headers);
            logger.Info("Headers[{0}]", headers);

        }

        public void Parse()
        {
            if (RootNode == null)
                RootNode = new XElement(XMLRootName);

            if (FirstLineHasHeaders)
            {
                ParseHeader();
                LogHeaders();
            }

            using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(SourceFileName, FileEncoding))
            {
                parser.Delimiters = new string[] { Delimiter };

                if (SkipFirstLine || FirstLineHasHeaders)
                {
                    parser.ReadFields();
                }


                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    if (Headers.Count==0)
                    {
                        InitializeHeaders(fields.Count());
                        LogHeaders();
                    }

                    XElement node = new XElement("Item");
                    RootNode.Add(node);
                    for (int i = 0; i < Headers.Count; i++)
                    {
                        node.Add(new XElement(Headers[i], fields[i]));
                    }
                }
            }
        }

        public void SaveXML(string fileName)
        {
            RootNode.Save(fileName);
        }
    }
}
