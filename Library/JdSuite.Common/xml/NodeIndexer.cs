using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using JdSuite.Common.Module;
using System.Linq.Expressions;
using System.Xml.Schema;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace JdSuite.Common
{
    /// <summary>
    /// This class is used to load data from xml data file into DataItem tree structure
    /// </summary>
    public class NodeIndexer : IDisposable
    {
        private NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(NodeIndexer));
        public List<string> ErrorList { get; private set; } = new List<string>();

        public Field RootSchemaNode { get; private set; }

        // public XElement RootXmlNode { get; private set; }
        //private DataItem RootDataNode { get; set; }


        private readonly Dictionary<string, SortedList<long, NodeInfo>> m_nodeMap = new Dictionary<string, SortedList<long, NodeInfo>>();

        public List<string> GetFirstLevelNodes()
        {
           return m_nodeMap.Keys.ToList();
        }

        //private readonly List<XElement> NodeList = new List<XElement>();

        public float ProgressPercent { get; private set; }

        public bool StopIndexing { get; set; } = false;

        public EventHandler<NodeIndexEventArgs> OnNodeParsed { get; set; }

        public EventHandler<float> OnProgressChange { get; set; }

        public EventHandler OnIndexingCompleted { get; set; }

        public Action<string> OnError { get; set; }

        public void ClearEvents()
        {
            OnNodeParsed = null;
            OnProgressChange = null;
            OnIndexingCompleted = null;
            OnError = null;

        }

        private FileStream fileStream { get; set; } = null;

        private string DataFile { get; set; } = "";

        public int Maxlevel { get; private set; }

        public int GetNodeCount(string NodeType)
        {
            if (m_nodeMap.ContainsKey(NodeType))
            {
                return m_nodeMap[NodeType].Count;
            }

            return 0;
        }

        public int GetTotalNodeCount()
        {
            int sum = 0;
            foreach (var item in m_nodeMap.Values)
            {
                sum += item.Count;
            }

            return sum;
        }

        public int ItemCount { get { return GetTotalNodeCount(); } }

        

        /// <summary>
        /// This is recomended method to load items
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public DataItem GetDataItemNode(int Id,string NodeName)
        {
            JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage($"Output Module: Loading Record {Id}");
            XElement xnode = GetXMLNode(Id,NodeName);
            if (xnode == null)
                return null;

            var item = new DataItem();
            item.Id = Id;
            int maxlevel = 0;
            //item.Load(xnode, this.RootSchemaNode, ref maxlevel);TODO:Revert

            XElement topElement = new XElement(this.RootSchemaNode.Name, xnode);

            item.Load(topElement, this.RootSchemaNode, ref maxlevel);
            item.IsProperty = false;
            item.Value = "";

            if (this.Maxlevel < maxlevel)
                this.Maxlevel = maxlevel;

            logger.Trace("Loaded_node {0}", item.ToString());

            JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage($"Output Module: Loaded Record {Id}");
            return item;
        }

        public XElement GetXMLNode(long Id, string NodeName, bool keepFileStreamOpen = false)
        {
            if (!m_nodeMap.ContainsKey(NodeName))
            {
                // JdSuite.Common.MessageService.ShowError("Data Missing Error", $"Record Id {Id} does not exist in DataFactory");
                return null;
            }


            if(!m_nodeMap[NodeName].ContainsKey(Id))
            {
                return null;
            }
            var info = m_nodeMap[NodeName][Id];

            logger.Trace("Loading_node_from_file_offset {0} {1}",NodeName, info.ToString());


            if (this.fileStream == null)
            {
                fileStream = File.OpenRead(DataFile);
            }

            fileStream.Seek(info.Offset, SeekOrigin.Begin);

            XElement xnode = null;

            try
            {
                using (XmlReader reader = XmlReader.Create(fileStream))
                {
                    var b = reader.Read();
                    while (reader.Name != NodeName)
                        reader.Read();

                    xnode = XElement.ReadFrom(reader) as XElement;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex,$"Node loading error {NodeName} Id:{Id} ");
            }
            finally
            {
                if (keepFileStreamOpen == false)
                {
                    fileStream.Close();
                    fileStream = null;
                }
            }

            return xnode;
        }

        public void LoadData(string dataFile, string FieldFile)
        {
            DataFile = dataFile;
            DataFile = @"C:\Application_Testing\shakeel\Customer_data_XML_1.xml"; //for testing only
            FieldFile = @"C:\Application_Testing\shakeel\xml_schema.xml";


            RootSchemaNode = Field.Parse(FieldFile);

            LoadData(DataFile, RootSchemaNode);

        }

        /// <summary>
        /// Loads data from DataFile asynchronously (xml data file)
        /// </summary>
        /// <param name="DataFile">xml data file</param>
        /// <param name="rootSchemaField">root schema node</param>
        public async Task LoadDataAsync(string dataFile, Field rootSchemaField)
        {
            this.DataFile = dataFile;
            logger.Info("Loading file [{0}]", DataFile);


            RootSchemaNode = rootSchemaField;

            await System.Threading.Tasks.Task.Factory.StartNew(CreateNodeIndex);
        }

        public void LoadData(string dataFile, Field rootSchemaField)
        {
            this.DataFile = dataFile;
            logger.Info("Loading file [{0}]", DataFile);


            RootSchemaNode = rootSchemaField;

            CreateNodeIndex();
        }

        public void CreateNodeIndex()
        {

            logger.Info("Creating_node_index");

            if (RootSchemaNode.ChildNodes.Count < 1)
            {
                logger.Error("RootSchemaNode does not have child nodes");
                return;
            }

            //  string data = "<" + RootSchemaNode.ChildNodes[0].Name + ">";
           

            m_nodeMap.Clear();

            try
            {
                logger.Info("Starting_node_indexing file {0}", DataFile);

                foreach (var ff in RootSchemaNode.ChildNodes)
                {
                    logger.Info("Creating_nodeIndex for node {0}", ff.ToString());

                    string indexNodeName = ff.Name;
                    SortedList<long, NodeInfo> nodeMap = new SortedList<long, NodeInfo>();
                    m_nodeMap.Add(indexNodeName, nodeMap);

                    CreateNodeIndex(DataFile, indexNodeName, nodeMap);
                    
                }

                logger.Info("Indexing_completed_successfully file {0}", DataFile);
            }
            catch (Exception ex)
            {

                string errorStr = "IndexingProcessError: " + ex.Message;
                ErrorList.Add(errorStr);
                logger.Error(ex, "XMLFile_node_indexing_error {0}", DataFile);
                OnError?.Invoke(errorStr);
            }
            finally
            {
                OnIndexingCompleted?.Invoke(this, new EventArgs());
                if (!StopIndexing)
                {
                    // NodeParsed?.Invoke(this, new NodeIndexEventArgs() { NodeIndexer = this, ItemCount = NodeMap.Count });
                    OnProgressChange?.Invoke(this, ProgressPercent);
                }
            }

            logger.Trace("Leaving_method");
        }

        private void CreateNodeIndex(string xmlFile, string indexNodeName, SortedList<long, NodeInfo> nodeMap)
        {
            logger.Info("Creating_node_index");
            ProgressPercent = 0;
            indexNodeName = "<" + indexNodeName;


            int dataLen = indexNodeName.Length;

            int MB10 = 1024 * 1024 * 10;
            int recordId = 0;
            long offset = 0;

            nodeMap.Clear();

            NodeInfo last = null;
            long streamLength = 0;
            using (var localFileStream = File.OpenRead(xmlFile))
            {
                streamLength = localFileStream.Length;
                float lastReported = 0;
                do
                {


                    offset = FileUtils.Seek(localFileStream, indexNodeName, MB10);
                    if (offset == -1 || localFileStream.Position >= localFileStream.Length)
                    {
                        break;
                    }

                    if (last != null)
                    {
                        last.Length = (int)(offset - last.Offset);
                    }

                    NodeInfo info = new NodeInfo();
                    info.Id = ++recordId;
                    info.Offset = offset;
                    last = info;
                    nodeMap.Add(info.Id, info);

                    logger.Trace("Found_node {0}=>{1}", indexNodeName, info.ToString());

                    OnNodeParsed?.Invoke(this, new NodeIndexEventArgs(indexNodeName) { NodeIndexer = this, ItemCount = nodeMap.Count, CurrentNode = info });
                    if (StopIndexing)
                        goto lbl_end;
                    ProgressPercent = 100 * offset / streamLength;

                    if (recordId % 5 == 0 && (ProgressPercent - lastReported >= 2))
                    {

                        OnProgressChange?.Invoke(this, ProgressPercent);
                        lastReported = ProgressPercent;
                    }

                    localFileStream.Seek(dataLen * 2, SeekOrigin.Current);

                } while (offset > 0);

                localFileStream.Close();
            }

        lbl_end:
            logger.Trace("Leaving_method");

        }//end of CreateNodeIndex

        /*
        private void CreateNodeList()
        {
            logger.Info("Creating node list");

            try
            {
                XmlReaderSettings s = new XmlReaderSettings();
                s.CheckCharacters = false;
                s.CloseInput = false;
                s.IgnoreWhitespace = true;

                using (var fs = File.OpenRead(DataFile))
                {

                    using (StreamReader sreader = new StreamReader(fs))
                    {
                        using (XmlReader mReader = XmlReader.Create(sreader, s))
                        {
                            IXmlLineInfo lineInfo = mReader as IXmlLineInfo;
                            long p1 = mReader.GetPosition(sreader);
                            int id = 0;

                            while (!mReader.ReadToFollowing(RootSchemaNode.ChildNodes[0].Name))
                            {

                            }

                            int nodeNameLength = RootSchemaNode.ChildNodes[0].Name.Length;
                            long p2 = 0;
                            while (!mReader.EOF)
                            {
                                try
                                {
                                    if (mReader.NodeType == XmlNodeType.EndElement && mReader.Name.Equals(RootSchemaNode.Name, StringComparison.OrdinalIgnoreCase))
                                        break;

                                    p2 = mReader.GetPosition(sreader);
                                    p2 = p2 - nodeNameLength - 2;
                                    XElement currentElement = XNode.ReadFrom(mReader) as XElement;


                                    NodeList.Add(currentElement);
                                    long p3 = mReader.GetPosition(sreader);

                                    NodeInfo pos = new NodeInfo();
                                    pos.Id = ++id;
                                    pos.Offset = p2;
                                    pos.Length = (int)(p3 - p2);
                                    NodeMap.Add(pos.Id, pos);



                                    logger.Trace($"Parsed_node_from_xml_file {currentElement.Name} {pos.ToString()} Next_Node_Offset:{pos.Offset + pos.Length}");
                                    if (id % 5 == 0)
                                    {
                                        NodeParsed?.Invoke(this, new DataEventArgs() { Factory = this, ItemCount = NodeMap.Count });
                                    }

                                    //logger.Trace("Node text:[{0}]", ReadFromStream(fs, p2, 256));

                                    // if (!mReader.ReadToNextSibling(RootSchemaNode.ChildNodes[0].Name))
                                    //    break;
                                }
                                catch (System.Xml.XmlException ex)
                                {
                                    logger.Error(ex, "Xml file loading error [{0}]", fileStream.Name);
                                    if (ex.InnerException != null)
                                        logger.Info(ex.InnerException);
                                    OnError("xml file loading error " + ex.Message);
                                }
                            }//end of while !EOF
                        }//end of using reader

                        NodeParsed?.Invoke(this, new DataEventArgs() { Factory = this, ItemCount = NodeMap.Count });
                    }//end of using streamReader

                }//end of using fileStream
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Xml file loading error [{0}]", fileStream.Name);
                OnError("xml file loading error " + ex.Message);
            }
        }
        */



        private string ReadFromStream(Stream ss, long offset, int dataLen)
        {
            long oldPos = ss.Position;

            ss.Seek(offset, SeekOrigin.Begin);

            byte[] buffer = new byte[dataLen];
            ss.Read(buffer, 0, dataLen);
            ss.Seek(oldPos, SeekOrigin.Begin);

            return Encoding.UTF8.GetString(buffer);
        }

        public void Dispose()
        {
            CloseFileStream();
        }

        public void CloseFileStream()
        {
            if (this.fileStream != null)
            {
                this.fileStream.Close();
                this.fileStream.Dispose();
            }
        }


        /*
        public void AddNode(DataItem dataItem, XElement xElement, Field schema, ref int maxLevel)
        {
            try
            {
                if (xElement == null)
                {
                    logger.Error("XElement is null");
                    return;
                }



                var nodeName = xElement.Name.LocalName;

                if (schema == null)
                {
                    logger.Error($"Schema node for xml data node {nodeName} is null");
                    return;
                }

                var nextSchemaNode = schema;
                if (nextSchemaNode.Name != nodeName)
                {
                    nextSchemaNode = schema.ChildNodes.FirstOrDefault(x => x.Name == nodeName);
                }

                if (nextSchemaNode == null)
                {
                    logger.Error($"Schema node for xml data node {nodeName} is null");
                    return;
                }


                var targetType = nextSchemaNode.DataType;
                targetType = targetType.Replace("xs:", "");

                string nodeValue = xElement.Value;
                if (xElement.HasElements)
                    nodeValue = "Array";

                //Validate xml element value
                if (nodeValue != "Array")
                    ValidateNodeDataType(nodeName, targetType, nodeValue);


                IXmlLineInfo lineInfo = xElement as IXmlLineInfo;
                if (lineInfo != null)
                {
                    dataItem.LineNo = lineInfo.LineNumber;
                    dataItem.Position = lineInfo.LinePosition;
                }
                dataItem.Name = nodeName;
                dataItem.Type = targetType;
                dataItem.Value = "Array";// element.Value;


                foreach (var childXelement in xElement.Elements())
                {
                    if (childXelement.HasElements)
                    {
                        var childNode = dataItem.AddChild(new DataItem());

                        AddNode(childNode, childXelement, nextSchemaNode, ref maxLevel);

                        lineInfo = childXelement as IXmlLineInfo;
                        if (lineInfo != null)
                        {
                            childNode.LineNo = lineInfo.LineNumber;
                            childNode.Position = lineInfo.LinePosition;
                        }

                        if (maxLevel < childNode.Level)
                        {
                            maxLevel = childNode.Level;
                        }
                    }
                    else
                    {
                        AddProp(dataItem, childXelement, nextSchemaNode);
                    }
                }
                if (dataItem.ReverseLevel < maxLevel)
                {
                    dataItem.ReverseLevel = maxLevel;
                }

                // Debug.WriteLine("ReverseLevel = " + dataItem.ReverseLevel);

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"XElement_processing_error {xElement.Name.LocalName} Err:{ex.Message}");
            }
        }

        private void AddProp(DataItem dataItem, XElement element, Field schema)
        {
            if (element == null)
            {
                logger.Error($"XElement is null");
                return;
            }

            var nodeName = element.Name.LocalName;

            var schemaNode = schema;
            if (schemaNode.Name != nodeName)
            {
                schemaNode = schema.ChildNodes.FirstOrDefault(x => x.Name == nodeName);
            }


            if (schemaNode == null)
            {
                logger.Error($"Schema node for XElement {nodeName} is null");
                return;
            }

            var targetType = schemaNode.DataType;
            targetType = targetType.Replace("xs:", "");


            string nodeValue = element.Value;

            if (element.HasElements)
                nodeValue = "Array";

            if (nodeValue != "Array")
                ValidateNodeDataType(nodeName, targetType, nodeValue);

            var propItem = new DataItem(nodeName, targetType, nodeValue);
            var lineInfo = element as IXmlLineInfo;
            if (lineInfo != null)
            {
                propItem.LineNo = lineInfo.LineNumber;
                propItem.Position = lineInfo.LinePosition;
            }
            dataItem.AddProp(propItem);
        }


        public bool ValidateNodeDataType(string nodeName, string targetType, string nodeValue)
        {


            //Validate xml element value
            bool isValid = true;

            if (targetType == "Int16")
            {
                if (!Int16.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Int32")
            {
                if (!int.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }

            else if (targetType == "Int64")
            {
                if (!Int64.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Boolean" || targetType == "bool")
            {
                if (!bool.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Double")
            {
                if (!double.TryParse(nodeValue, out var intVal))
                {
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Single")
            {
                if (!Single.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Date/Time")
            {
                if (!DateTime.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }

            return isValid;
        }


        */
    }

    public class NodeInfo
    {

        public int Id { get; set; }
        public long Offset { get; set; }
        public int Length { get; set; }

        public override string ToString()
        {
            return $"NodeInfo[Id:{Id}, Offset:{Offset}, Length:{Length}]";
        }
    }

    public class NodeIndexEventArgs : EventArgs
    {
        public NodeIndexEventArgs(string nodeName)
        {
            NodeName = nodeName;
        }

        public string NodeName { get; set; }

        public int ItemCount { get; set; }
        public NodeIndexer NodeIndexer { get; set; }

        public NodeInfo CurrentNode { get; set; }


        //public XmlNode CurrentNodeXML { get; set; }
    }

}
