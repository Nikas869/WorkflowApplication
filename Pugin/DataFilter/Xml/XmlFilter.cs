using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using JdSuite.Common.Module;
using JdSuite.DataFilter.Models;
using JdSuite.DataFilter.ViewModels;


namespace JdSuite.DataFilter.Xml
{

    //Following are important links which provide details about XPath queries, syntax and details
    //https://weblogs.asp.net/dixin/linq-to-xml-1-modeling-xml
    //https://weblogs.asp.net/dixin/linq-to-xml-2-query-methods
    //https://csharp.hotexamples.com/examples/System.Xml.Linq/XElement/XPathSelectElements/php-xelement-xpathselectelements-method-examples.html
    //https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ms256086(v=vs.100)?redirectedfrom=MSDN
    //https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ms256236%28v%3dvs.100%29
    //http://www.pearsonitcertification.com/articles/article.aspx?p=101369&seqNum=3
    //https://github.com/code4libtoronto/2016-07-28-librarycarpentrylessons/blob/master/xpath-xquery/lesson.md good link
    //https://orderofcode.com/2016/05/31/reading-big-xmls-in-net/ xml streaming
    //https://weblogs.asp.net/dixin/linq-to-xml-1-modeling-xml Dixin blog


    /// <summary>
    /// This class implements main xml filtering logic
    /// </summary>
    public class XmlFilter:IDisposable
    {
        private bool elseOutputDone = false;
        private NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(XmlFilter));

        public Action<float> OnProgressUpdate { get; set; }

        public int InputNodeCount { get; set; }

        public float Progress { get; set; }

        public int CurrentNodeCount { get; set; }

        Queue<XmlWriter> WriterQue = new Queue<XmlWriter>();
        /// <summary>
        /// Xml data input file, it is mandatory parameter
        /// </summary>
        public string InputFileName { get; set; }

        /// <summary>
        /// Xml Output file, it is mandatory parameter
        /// </summary>
        public string OutputFileName { get; set; }


        public string OutputFileName2 { get; set; }

        /// <summary>
        /// Valid folder path where temporary xml files shall be written
        /// </summary>
        public string DataDir { get; set; }


        /// <summary>
        /// Xml Filter Fields
        /// </summary>
        public List<FilterField> Filters { get; set; }





        /// <summary>
        /// For example "/Invoices/Invoice/items/item[ItemNumber>=5 and QTY>2]"
        /// </summary>
        public List<string> XPathFilter { get; set; }


        public XmlFilter()
        {
            Filters = new List<FilterField>();
            XPathFilter = new List<string>();

        }

        public void CreateXPathExpressions()
        {
            XPathFilter.Clear();
            // var groups = Filters.GroupBy(x => x.XPath).Select(x => new { key = x.Key, items = x.ToList() });
            // //(1,and);(2,or)
            logger.Info("Creating XPathFilter expression FieldCount {0}", Filters.Count);
            if (Filters.Count == 1)
            {
                var ff = Filters[0];
                ff.SetXPathCondition();
                logger.Info("{0} XPathCondition={1}", ff.FieldName, ff.XPathCondition);
                XPathFilter.Add(ff.XPathCondition);
                return;
            }

            if (Filters.Count >= 2)
            {
                if (Filters[0].ParentXPath == Filters[1].ParentXPath)
                {
                    var ff1 = Filters[0];
                    var ff2 = Filters[1];
                    var filter = ff1.ParentXPath + "[" + ff1.ConditionExpr2() + " " + ff2.JoinType + " " + ff2.ConditionExpr2() + "]";

                    logger.Info($"Joine XPathCondition {ff1.FieldName} {ff2.FieldName} = {filter}");
                    XPathFilter.Add(filter);
                }
                else
                {
                    foreach (var ff in Filters)
                    {
                        logger.Info("{0} XPathCondition={1}", ff.FieldName, ff.XPathCondition);
                        ff.SetXPathCondition();
                        XPathFilter.Add(ff.XPathCondition);
                    }
                }
            }

        }

        /// <summary>
        /// This function is only used in direct testing
        /// </summary>
        public void LoadData()
        {
            // InputFileName = @"E:\Canvas2\Dropbox\BlockApp\Data\Customer_data_XML_1.xml";
            // OutputFileName = @"E:\Canvas2\Dropbox\BlockApp\Data\Customer_data_XML_5_filter_out.xml";
            // DataDir = @"E:\Canvas2\Dropbox\BlockApp\Data\";

            // XPathFilter = " / Invoices/Invoice/items/item/props[Prop1>=200]";

            // XPathFilter= "/Invoices/Invoice/items/item[ItemNumber>=100]";
            //XPathFilter = "/Invoices/Invoice/items/item[ItemNumber>=5 and QTY>2]";
            //XPathFilter = "/Invoices/Invoice/items/item[./Description[contains(text(),'23')]]";
            //[contains(text(), 'About us')

            // logger.Info("Loading xml data file {0}", InputFileName);
        }



        /// <summary>
        /// Creates XmlWriter object
        /// </summary>
        /// <param name="outputFileName">Name of file to which data shall be written</param>
        /// <returns></returns>
        private XmlWriter CreateWriter(string outputFileName, bool AddToQue)
        {
            logger.Trace("Initializing xmlWriter with {0}", outputFileName);
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.Indent = true;
            setting.IndentChars = "\t";
            var writer = XmlWriter.Create(outputFileName, setting);
            if (AddToQue)
                WriterQue.Enqueue(writer);
            return writer;
        }

        /// <summary>
        /// Creates XmlReader object which can be used to read xml from file
        /// </summary>
        /// <param name="inputFile">Name of file from which xml shall be read</param>
        /// <returns></returns>
        private XmlTextReader CreateReader(string inputFile)
        {
            var reader = new XmlTextReader(inputFile);
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            reader.MoveToContent();

            return reader;
        }

        /// <summary>
        /// Use this as it manages to run multiple passes to process all xpath expressions
        /// </summary>
        public void Filter()
        {
            logger.Trace("Starting main_filtering_process");

            logger.Info("InputFile [{0}]", InputFileName);
            logger.Info("OutputFile [{0}]", OutputFileName);
            if (!string.IsNullOrEmpty(OutputFileName2))
                logger.Info("OutputFile2 [{0}]", OutputFileName2);

            WriterQue.Clear();
            string filter = "";
            try
            {
                if (XPathFilter.Count <= 0)
                {
                    CreateXPathExpressions();
                }

                if (XPathFilter.Count == 1)
                {
                    filter = XPathFilter[0];
                    Filter(InputFileName, OutputFileName, filter);
                    return;
                }
                Random rand = new Random();

                if (File.Exists(OutputFileName))
                {
                    File.Delete(OutputFileName);

                }

                string inputFileName = InputFileName;
                for (int i = 0; i < XPathFilter.Count - 1; i++)
                {
                    filter = XPathFilter[i];
                    string outputFile = DataDir + "DataFilter_" + DateTime.Now.ToString("yyMMddHHmmssfff") + "_" + rand.Next(1900).ToString() + ".xml";
                    Filter(inputFileName, outputFile, filter);
                    inputFileName = outputFile;
                }

                filter = XPathFilter[XPathFilter.Count - 1];
                Filter(inputFileName, OutputFileName, filter);

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"XmlFilter_Error Input [{InputFileName}] XPath[{filter}]");
                throw;
            }
            finally
            {
                while (WriterQue.Count > 0)
                {
                    var writer = WriterQue.Dequeue();

                    try { writer.Close(); } catch { }
                }
            }
        }


        /// <summary>
        /// Executes actual xml filtering logic, reads xml nodes, filters them and then writes to output file
        /// </summary>
        public void Filter(string inputFile, string outputFile, string XPathFilter)
        {
            logger.Info("Starting filtering_step with following parameters");

            logger.Info("InputFile [{0}]", inputFile);
            logger.Info("OutputFile [{0}]", outputFile);
            logger.Info("XPathFilter [{0}]", XPathFilter);

            //  var xpath = "/Invoices/Invoice/items/item[ItemNumber>=5 and QTY>2]";

            char[] sep = new char[] { '/' };

            string xpathLocal = XPathFilter.Substring(0, XPathFilter.IndexOf('['));
            var parts = xpathLocal.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            var lastPart = "descendant-or-self::" + parts[parts.Length - 1] + XPathFilter.Substring(XPathFilter.IndexOf('['));


            using (var reader = CreateReader(inputFile))
            {
                CurrentNodeCount++;

                if (reader.Name != parts[0])
                {
                    string message = $"Source Xml.Root [{reader.Name}] is_not_equal_to XPath [{XPathFilter}]";
                    logger.Error(message);

                    throw new Exception(message);
                }

                using (var writer = CreateWriter(outputFile, false))
                {
                    XmlWriter writer2 = null;
                    if (elseOutputDone == false && !string.IsNullOrEmpty(OutputFileName2))
                    {
                        writer2 = CreateWriter(OutputFileName2, true);
                        elseOutputDone = true;
                    }

                    writer.WriteStartDocument();
                    writer.WriteStartElement(reader.Name);
                    if (writer2 != null)
                    {
                        writer2.WriteStartDocument();
                        writer2.WriteStartElement(reader.Name);
                    }


                    List<XElement> removelist = new List<XElement>();

                    if (parts.Length == 2)
                    {
                        reader.Read();
                        CurrentNodeCount++;

                        while (!reader.EOF)
                        {
                            logger.Trace("Processing LineNumber {0}", reader.LineNumber);

                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                XElement rootNode = XElement.ReadFrom(reader) as XElement;
                                

                                bool done = false;
                                foreach (var xnode in rootNode.XPathSelectElements(lastPart))
                                {
                                    xnode.WriteTo(writer);
                                    done = true;
                                }

                                if (!done)
                                {
                                    if (writer2 != null)
                                        rootNode.WriteTo(writer2);
                                }
                            }

                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                reader.Read();
                            }
                            else
                            {
                                reader.Skip();
                            }

                            CurrentNodeCount++;
                            UpdateProgress();
                        }
                    }
                    else
                    {
                        reader.Read();
                        CurrentNodeCount++;
                        do
                        {
                            logger.Trace("Processing LineNumber {0}", reader.LineNumber);
                            if (reader.Name == parts[1])
                            {
                                FilterSubTree(reader, writer, writer2, parts, lastPart, removelist);
                            }

                            if (reader.NodeType == XmlNodeType.EndElement)
                            {
                                reader.Read();
                            }
                            else
                            {
                                reader.Skip();
                            }

                            CurrentNodeCount++;
                            UpdateProgress();

                        } while (!reader.EOF);
                    }

                    if (writer2 != null)
                    {
                        writer2.WriteEndElement();
                        writer2.Flush();
                        writer2.Close();
                    }

                    writer.WriteEndElement();
                    writer.Flush();
                    writer.Close();
                }//End of using writer
            }//end of using reader
        }

        private void UpdateProgress()
        {
            if (OnProgressUpdate != null)
            {
                Progress = ((float) CurrentNodeCount * 100 )/ (InputNodeCount + 1);
                OnProgressUpdate(Progress);
            }

        }

        private void FilterSubTree(XmlTextReader reader, XmlWriter writer, XmlWriter writer2, string[] parts, string lastPart, List<XElement> removelist)
        {
            string l1data = reader.ReadOuterXml();
            removelist.Clear();

            List<XElement> passlist = new List<XElement>();

            var xelement = XElement.Parse(l1data);


            string filterStr = ".//" + parts[parts.Length - 2] + "/" + lastPart;
            string allPartsStr = ".//" + parts[parts.Length - 2] + "/" + parts[parts.Length - 1];

            var choosenNodes = xelement.XPathSelectElements(filterStr).ToList();
            var allNodes = xelement.XPathSelectElements(allPartsStr);

            int pass_count = 0;
            XElement removeParent = null;

            foreach (XElement cnode in allNodes)
            {
                if (choosenNodes.Contains(cnode))
                {
                    pass_count++;
                }
                else
                {
                    removelist.Add(cnode);
                }
            }

            if (removelist.Count > 0)
            {
                removeParent = removelist.FirstOrDefault().Parent;
            }



            removelist.ForEach(x => x.Remove());

            if (pass_count == 0 && removelist.Count > 0)
            {
                // removeParent.Parent.Add(new XElement(removeParent.Name, string.Empty));
            }

            logger.Info("Selected_xml_node \n{0}", xelement.Name);

            xelement.WriteTo(writer);
            writer.Flush();

            if (writer2 != null)
            {
                if (removelist.Count > 0)
                {
                    foreach (var node in removelist)
                        removeParent.Add(node);
                }

                choosenNodes.ForEach(x => x.Remove());

                xelement.WriteTo(writer2);
                writer2.Flush();

            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    while (WriterQue.Count > 0)
                    {
                        try
                        {
                            var writer = WriterQue.Dequeue();
                            writer.Flush();
                            writer.Close();
                        }
                        catch { }
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~XmlFilter()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
