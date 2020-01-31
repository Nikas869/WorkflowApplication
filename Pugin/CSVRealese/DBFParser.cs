using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using dBASE.NET;

namespace CSVInput
{

    public class DBFParser
    {
        NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();
        public string SourceFileName { get; set; }
        public string XMLRootName { get; set; } = "Root";

        public XElement RootNode { get; set; }
        public List<string> Headers { get; private set; } = new List<string>();

        public Encoding FileEncoding { get; set; }

        public int RecordCount { get; private set; }


        public DBFParser()
        {
            FileEncoding = Encoding.UTF8;
        }



        public void ParseHeader(Dbf dbfreader)
        {
            logger.Info("Entered");
            this.Headers.Clear();

            for (int i = 0; i < dbfreader.Fields.Count; i++)
            {
                Headers.Add(dbfreader.Fields[i].Name);
            }
            CleanHeaders();
        }

        private void CleanHeaders()
        {

            char[] prohibited_chars = new char[] { (char)0x20 };

            for (int i = 0; i < Headers.Count; i++)
            {
                Headers[i] = Headers[i].Trim(prohibited_chars);
                Headers[i] = Headers[i].Replace(" ", "_");
            }

            int idx = 0;
            do
            {

                idx = Headers.FindIndex(x => x.IndexOfAny(prohibited_chars) >= 0);
                if (idx >= 0)
                {
                    Headers[idx] = Headers[idx].Trim(prohibited_chars);
                    Headers[idx] = Headers[idx].Replace(" ", "_");
                }

            } while (idx >= 0);
        }


        private void LogHeaders()
        {
            var headers = string.Join("###", this.Headers);
            logger.Info("Headers[{0}]", headers);

        }

        public void Parse()
        {
            logger.Info("Parsing file {0}", SourceFileName);

            if (RootNode == null)
                RootNode = new XElement(XMLRootName);


            Dbf dbf = new Dbf();

            try
            {
                dbf.Read(this.SourceFileName);
                ParseHeader(dbf);

                logger.Trace("Parsing dbf records");

                for (int row = 0; row < dbf.Records.Count; row++)
                {
                    RecordCount = row;

                    XElement node = new XElement("Item");
                    RootNode.Add(node);

                    for (int column = 0; column < dbf.Records[row].Data.Count; column++)
                    {
                        string cell = null;
                        if (dbf.Records[row].Data[column] == null)
                        {
                            cell = "null";
                        }
                        else
                        {
                            cell = dbf.Records[row].Data[column].ToString();
                        }

                        node.Add(new XElement(Headers[column], cell));

                    }

                }

                logger.Trace("DBF file is parsed successfully RecordCount {0}",RecordCount);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DBF file parsing error {SourceFileName}");
                throw new Exception($"DBF file parsing error {SourceFileName} Error:" + ex.Message);
            }
        }

        public void SaveXML(string fileName)
        {
            RootNode.Save(fileName);
        }
    }

}
