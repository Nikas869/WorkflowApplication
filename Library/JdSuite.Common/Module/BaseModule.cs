using JdSuite.Common.Internal;
using JdSuite.Common.Module.MefMetadata;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace JdSuite.Common.Module
{
    public abstract class BaseModule : NotifyPropertyChangeBase, IModule
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger("BaseModule");

        private string _displayName = "BaseModule";

        public IModuleCategory ModuleCategory { get; set; }
        public ModuleType ModuleType { get; set; }

        public int ActiveKey { get; set; } = 0;

        /// <summary>
        /// Caller Module, tells his progress to this module
        /// </summary>
        public Action<BaseModule, int> UpdateProgress;

        /// <summary>
        /// Caller Module tells this module that error has occured and passes true to halt processing
        /// </summary>
        public Action<BaseModule, bool> OnError;

        /// <summary>
        /// Caller module tells this module that display this message or pass on to module on right
        /// </summary>
        public Action<BaseModule, string> ShowMessage;

        /// <summary>
        /// Caller Module tells this module that his part of work is completed;
        /// </summary>
        public Action<BaseModule, bool> WorkCompleted;

        public void Register(BaseModule Requester, List<BaseModule> ModuleList)
        {
            ModuleList.Add(this);
            if (this.InputNodes.Count > 0)
            {
                foreach (var input in InputNodes)
                {
                    var inputModule = input?.Connector?.Module;
                    if (inputModule != null)
                    {
                        inputModule.Register(this, ModuleList);
                    }
                }
            }
        }


        public event EventHandler<INode> OnNodeAdded;
        public event EventHandler<INode> OnNodeRemoved;

        public SerializableDictionary<Guid, ModuleState> Store { get; set; } = new SerializableDictionary<Guid, ModuleState>();

        public SerializableDictionary<string, string> StateInfo { get; private set; } = new SerializableDictionary<string, string>();

        public int ParameterCount { get; set; } = 0;

        public string DataDir { get; set; }

        protected abstract string ModuleName { get; }

        public string DisplayName
        {
            get { return _displayName; }
            set { SetPropertry(ref _displayName, value); }
        }
        protected abstract Bitmap Icon { get; }

        protected abstract string ToolTip { get; }

        public Border ModulePageBorder { get; set; }

        //Do not add/remove nodes from it directly
        public List<BaseInputNode> InputNodes { get; set; } = new List<BaseInputNode>();

        //Do not add/remove nodes from it directly
        public List<BaseOutputNode> OutputNodes { get; set; } = new List<BaseOutputNode>();

        private Guid _Guid = Guid.NewGuid();

        ~BaseModule()
        {
            if (!System.Environment.HasShutdownStarted)
            {
                Disconnect();
            }
        }

        public virtual void SetContextMenuItems(ContextMenu ctxMenu)
        {


        }



        /// <summary>
        /// When called, prompt the parent-level application to update with the
        /// updated workflow.
        /// </summary>
        public Action<Workflow> RequestStateUpdate;

        /// <summary>
        /// Get the current workflow state from the parent application
        /// </summary>
        public Func<BaseModule, Workflow> GetState { get; set; }

        public Guid Guid => _Guid;

        public void Disconnect()
        {
            foreach (var input in InputNodes)
            {
                input.Disconnect();
            }

            foreach (var output in OutputNodes)
            {
                output.Disconnect();
            }
        }

        /// <summary>
        /// Execute a representation of the data for this module.
        /// The return object should be in a human-readable file
        /// format (string), or at least a file format readable by a program capable of reading
        /// the required file extension. A module may return null if it wishes
        /// to block and provide it's own human-readable interface.
        /// </summary>
        /// <param name="workflow">The current *.flo file.</param>
        /// <returns>
        /// The representation of this module's data.
        /// If it is not readily= available, this should return null.
        /// </returns>
        public virtual object Execute(Workflow workflow) { return null; }

        public virtual void OpenWindow(Workflow workflow) { throw new NotImplementedException("Run is not implemented " + DisplayName); }


        public virtual bool Run(WorkInfo workInfo) { throw new NotImplementedException("Run is not implemented " + DisplayName); }

       // public virtual void Process(WorkInfo workInfo) { throw new NotImplementedException("Run is not implemented " + DisplayName); }

        public virtual string GetExtension()
        {
            return null;
        }

        public List<BaseInputNode> GetInputNodes()
        {
            return InputNodes;
        }

        public List<BaseOutputNode> GetOutputNodes()
        {
            return OutputNodes;
        }

        public virtual int GetMajorVersion()
        {
            return 0;
        }

        public virtual int GetMinorVersion()
        {
            return 0;
        }



        public virtual int GetPatchVersion()
        {
            return 0;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual string GetToolTipDescription()
        {
            return ToolTip;
        }

        public bool HasConnections()
        {
            foreach (var input in InputNodes)
            {
                if (input.IsConnected())
                {
                    return true;
                }
            }

            foreach (var output in OutputNodes)
            {
                if (output.IsConnected())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether this instance is ready for node communication.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReady()
        {
            foreach (var input in InputNodes)
            {
                if (!input.IsConnected())
                {
                    return false;
                }
            }
            foreach (var output in OutputNodes)
            {
                if (!output.IsConnected())
                {
                    return false;
                }
            }

            return true;
        }

        public BitmapImage RetrieveBitmapIcon()
        {
            return this.Icon.ToBitmap();
        }

        public virtual void ReadXml(XmlReader reader)
        {
            if (!reader.IsStartElement("BaseModule"))
            {
                logger.Error("Cannot load base module as <BaseModule> is missing from xml");
                throw new XmlException("Cannot load base module as <BaseModule> is missing from xml");
            }

            XElement bmnode = XElement.ReadFrom(reader) as XElement;


            int attrCount = reader.AttributeCount;
            _Guid = Guid.Parse(bmnode.Attribute("Guid").Value);
            var inputCount = Int32.Parse(bmnode.Attribute("InputCount").Value);
            var outputCount = Int32.Parse(bmnode.Attribute("OutputCount").Value);
            var stateCount = Int32.Parse(bmnode.Attribute("StateCount").Value);


            var attrib = bmnode.Attribute("ParameterCount");
            if (attrib != null)
                this.ParameterCount = Int32.Parse(attrib.Value);



            if (stateCount > 0)
            {
                var dict = bmnode.XPathSelectElement("dictionary");
                if (dict != null)
                {
                    var dictReader = dict.CreateReader();
                    XmlSerializer keySerializer = new XmlSerializer(typeof(SerializableDictionary<Guid, ModuleState>));
                    Store = (SerializableDictionary<Guid, ModuleState>)keySerializer.Deserialize(dictReader);
                }
            }

            var stateInfo = bmnode.XPathSelectElement("StateInfo");
            if (stateInfo != null)
            {
                var dict = stateInfo.XPathSelectElement("dictionary");
                if (dict != null)
                {
                    var dictReader = dict.CreateReader();
                    XmlSerializer keySerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                    StateInfo = (SerializableDictionary<string, string>)keySerializer.Deserialize(dictReader);
                }
            }

            InputNodes.Clear();
            foreach (var ioNode in bmnode.XPathSelectElements("//InputNodes/InputNode"))
            {
                var identifier = Guid.Parse(ioNode.Attribute("Identifier").Value);
                var displayName = ioNode.Attribute("DisplayName").Value;
                var ext = ioNode.Attribute("Extension").Value;

                var moduleNode = new InputNode(this, this.DisplayName, ext);
                moduleNode.Identity = identifier;

                if (Store.ContainsKey(identifier))
                {
                    moduleNode.State = Store[identifier];
                }
                else
                {
                    moduleNode.State = new ModuleState();
                }

                AddInputNode(moduleNode);
            }

            OutputNodes.Clear();
            foreach (var ioNode in bmnode.XPathSelectElements("//OutputNodes/OutputNode"))
            {
                var identifier = Guid.Parse(ioNode.Attribute("Identifier").Value);
                var displayName = ioNode.Attribute("DisplayName").Value;
                var ext = ioNode.Attribute("Extension").Value;

                var moduleNode = new OutputNode(this, this.DisplayName, ext);
                moduleNode.Identity = identifier;

                if (Store.ContainsKey(identifier))
                {
                    moduleNode.State = Store[identifier];
                }
                else
                {
                    moduleNode.State = new ModuleState();
                }
                AddOutputNode(moduleNode);
            }

            ReadParameter(bmnode);
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            try
            {
                writer.WriteAttributeString("Type", GetType().AssemblyQualifiedName);
                writer.WriteAttributeString("Guid", Guid.ToString());
                writer.WriteAttributeString("StateCount", Store.Count.ToString());
                writer.WriteAttributeString("InputCount", InputNodes.Count.ToString());
                writer.WriteAttributeString("OutputCount", OutputNodes.Count.ToString());
                writer.WriteAttributeString("ParameterCount", ParameterCount.ToString());

                if (InputNodes.Count > 0)
                {
                    writer.WriteStartElement("InputNodes");
                    foreach (var inputNode in InputNodes)
                    {
                        InputNode item = inputNode as InputNode;
                        var ext = string.Join(",", item.GetSupportedExtensions());

                        writer.WriteStartElement("InputNode");

                        writer.WriteAttributeString("Identifier", item.Identity.ToString());
                        writer.WriteAttributeString("DisplayName", item.DisplayName);
                        writer.WriteAttributeString("Extension", ext);

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                }
                if (OutputNodes.Count > 0)
                {
                    writer.WriteStartElement("OutputNodes");

                    foreach (var item in OutputNodes)
                    {
                        var ext = string.Join(",", item.GetSupportedExtensions());

                        writer.WriteStartElement("OutputNode");

                        writer.WriteAttributeString("Identifier", item.Identity.ToString());
                        writer.WriteAttributeString("DisplayName", item.DisplayName);
                        writer.WriteAttributeString("Extension", ext);

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }


                if (Store.Count > 0)
                {

                    XmlSerializer keySerializer = new XmlSerializer(typeof(SerializableDictionary<Guid, ModuleState>));
                    keySerializer.Serialize(writer, Store);
                }

                if (StateInfo.Count > 0)
                {
                    writer.WriteStartElement("StateInfo");
                    writer.WriteAttributeString("Count", StateInfo.Count.ToString());
                    XmlSerializer keySerializer = new XmlSerializer(typeof(SerializableDictionary<string, string>));
                    keySerializer.Serialize(writer, StateInfo);
                    writer.WriteEndElement();
                }

                WriteParameter(writer);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }


        public virtual void WriteParameter(XmlWriter writer)
        {

        }

        public virtual void ReadParameter(XElement BaseModuleNode)
        {

        }


        protected void AddInputNode(BaseInputNode node)
        {
            node.OnConnected += OnNodeConnected;
            node.OnDisconnect += OnNodeDisconnected;
            InputNodes.Add(node);
            OnNodeAdded?.Invoke(this, node);
        }

        protected void AddOutputNode(BaseOutputNode node)
        {
            node.OnConnected += OnNodeConnected;
            node.OnDisconnect += OnNodeDisconnected;
            OutputNodes.Add(node);
            OnNodeAdded?.Invoke(this, node);
        }

        protected void RemoveNode(BaseNode node)
        {
            node.Disconnect();
            node.OnConnected -= OnNodeConnected;
            node.OnDisconnect -= OnNodeDisconnected;
            if (node is BaseOutputNode)
            {
                OutputNodes.Remove((BaseOutputNode)node);
            }
            else
            {
                InputNodes.Remove((BaseInputNode)node);
            }

            OnNodeRemoved?.Invoke(this, node);
        }

        private void OnNodeConnected()
        {
            // if all nodes are connected, set them to ready
            if (IsReady())
            {
                foreach (var input in InputNodes)
                {
                    input.Ready = true;
                }

                foreach (var output in OutputNodes)
                {
                    output.Ready = true;
                }
            }
        }

        private void OnNodeDisconnected()
        {
            foreach (var input in InputNodes)
            {
                input.Ready = false;
            }
            foreach (var output in OutputNodes)
            {
                output.Ready = false;
            }
        }
    }


    

}
