using JdSuite.Common.Logging;
using JdSuite.Common.Module;
using JdSuite.Common.Module.MefMetadata;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Forms;
using JdSuite.Common;
using System.Xml.Linq;
using System;
using JdSuite.Common.Logging.Enums;
using System.Drawing;
using XMLConverter.Properties;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using JdSuite.Common.FileProcessing;

namespace XMLConverter
{
    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.DATA_INPUTS)]
    [ExportMetadata("ModuleType", typeof(XmlInput))]
    public class XmlInput : BaseModule
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger("XMlModule");


        private OutputNode OutputNode
        {
            get { return (OutputNode)OutputNodes.FirstOrDefault(); }
        }



        protected override Bitmap Icon { get { return Resources.Icon; } }

        protected override string ToolTip { get { return "Provides an XML Schema for other modules."; } }

        protected override string ModuleName { get { return "XML_Converter"; } }

        public XmlInput()
        {
            Logger.AppName = ModuleName;
            var op = new OutputNode(this, "Schema", "DataLocation");

            AddOutputNode(op);
            DisplayName = "XML Input";

            this.ModuleCategory = IModuleCategory.DATA_INPUTS;
        }

        public override void OpenWindow(Workflow workflow)
        {
            var msg = "";
            try
            {
                Program.Main(this.OutputNode.State);
            }
            catch (Exception e)
            {
                msg = $"Critical failure in XML Converter: {e.Message}";

                JdSuite.Common.MessageService.ShowError("XML Module Error", msg);
            }
        }

        public override bool Run(WorkInfo workInfo)
        {
            NodeIndexer indexer = null;
            bool bStatus = false;
            try
            {
                logger.Info("Running_workflow");

                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Running Module");

                if (this.OutputNode.State == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, "XML Module OutputNode.State is null");
                    return false;
                }

                if (this.OutputNode.State.Schema == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, "OutputNode.State.Schema is null");
                    return false;
                }


                if (this.OutputNode.State.DataFilePath == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, "XML Input module OutputNode.State.DataFilePath is null");
                    return false;
                }

                // if input file doesn't exists or differ from loaded we need to load it
                if (this.OutputNode.State.DataFile == null || this.OutputNode.State.DataFile.FilePath != this.OutputNode.State.DataFilePath)
                {
                    this.OutputNode.State.DataFile = WorkflowFileFactory.LoadFromXmlFile(this.OutputNode.State.DataFilePath);
                }

                Field.SetParent(this.OutputNode.State.Schema);
                Field.ComputeXPath(this.OutputNode.State.Schema);

                workInfo.Schema = this.OutputNode.State.Schema;

                bool isValid = false;
                indexer = new NodeIndexer();

                /*
                indexer.OnProgressChange += (object sender, float p) =>
                {
                    workInfo.UpdateProgress(this, p);
                };

                indexer.OnIndexingCompleted += (object sender, EventArgs e) =>
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Node indexing completed");
                };

                indexer.OnError = (string err) =>
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, err);
                };
                */

                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Starting indexing process, please wait....");

                indexer.LoadData(this.OutputNode.State.DataFilePath, this.OutputNode.State.Schema);
                workInfo.Indexer = indexer;

                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Indexing process completed");

                //string fieldXPath= FirstLevelSchemaNode.GetXPath(FirstLevelSchemaNode.Parent);

                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Starting optionality validation process, please wait....");
                foreach (var nodeName in indexer.GetFirstLevelNodes())
                {
                    int Count = indexer.GetNodeCount(nodeName);
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"[{nodeName}].Count = {Count} Nodes");
                }

                this.OutputNode.State.DataFile.OnValidationProgressChange += (s, e) => { workInfo.UpdateProgress(this, e); };
                var validationErrors = new List<string>();

                isValid = this.OutputNode.State.DataFile.ValidateUsingSchema(this.OutputNode.State.Schema, indexer.GetTotalNodeCount(), out validationErrors);
                this.OutputNode.State.DataFile.OnValidationProgressChange = null;

                foreach (var message in validationErrors)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, message);
                }

                /*
                indexer.NodeParsed += (object sender, NodeIndexEventArgs e) =>
                {
                    isValid = validator.Validate(RootSchemaNode, indexer.GetXMLNode(e.CurrentNode.Id));
                    if (isValid == false)
                    {
                        foreach (var message in validator.ErrorList)
                        {
                            workInfo.Log(this.DisplayName, NLog.LogLevel.Info, message);
                        }
                        indexer.StopIndexing = true;
                    }
                };
                */

                /*
                foreach (var node in indexer.NodeMap)
                {
                    isValid = validator.Validate(this.OutputNode.State.Schema.ChildNodes[0], indexer.GetXMLNode(node.Key, true));
                    if (isValid == false)
                    {
                        foreach (var message in validator.ErrorList)
                        {
                            workInfo.Log(this.DisplayName, NLog.LogLevel.Info, message);
                        }
                        break;
                    }
                }
                */
                //isValid = true;

                if (isValid == false)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting workflow execution due to xml validation error");
                    return false;
                }

                if (indexer.ItemCount <= 0)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting workflow execution as there are no xml nodes in xml file");
                    return false;
                }

                bStatus = true;

            }
            catch (Exception ex)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Fatal error occured in {this.DisplayName}");
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, ex.Message);
                logger.Error(ex, "Workflow_excution_error");
                bStatus = false;
            }
            finally
            {
                if (indexer != null)
                {
                    indexer.ClearEvents();
                    indexer.CloseFileStream();
                }
            }
            workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"Module completed processing with Status: {bStatus}");
            logger.Info($"{this.DisplayName} module completed processing with status:{bStatus}");
            return bStatus;
        }

        public override object Execute(Workflow workflow)
        {
            logger.Trace("Executing xmlinput module");

            Logger.Log(Severity.DEBUG, LogCategory.MODULE, "Test");
            if (this.OutputNode.State == null)
                this.OutputNode.State = new ModuleState();

            var msg = "";
            bool failed = false;

            try
            {
                Program.Main(this.OutputNode.State);
            }
            catch (Exception e)
            {
                msg = $"Critical failure in XML Converter: {e.Message}";
            }

            if (Program.form == null)
            {
                return null;
            }

            if (Program.form.isCancel)
            {
                return null;
            }

            if (msg.Length > 0)
            {
                failed = true;
            }
            else if (this.OutputNode.State.Schema == null || this.OutputNode.State.DataFilePath == null)
            {
                msg += "Procedure not completed.";
                if (this.OutputNode.State.Schema == null)
                {
                    msg += " Schema not available. Please generate a schema before closing the window.";
                }
                if (this.OutputNode.State.DataFilePath == null)
                {
                    msg += " Input path not available. Input file was not added before closing the window.";
                }
                failed = true;
            }
            else
            {
                if (!File.Exists(this.OutputNode.State.DataFilePath))
                {

                    msg += string.Format("Input file does not exist at path: {0}.", this.OutputNode.State.DataFilePath);
                    failed = true;
                }
            }

            if (failed)
            {
                if (OutputNode.IsConnected())
                {
                    msg = "Disconnecting module: " + msg;
                    OutputNode.Disconnect();
                }
                Logger.Log(Severity.WARN, LogCategory.MODULE, msg);
                System.Windows.Forms.MessageBox.Show(msg);
                this.OutputNode.State.DataFilePath = "";
                return null;
            }

            logger.Info("Calling   RequestStateUpdate?.Invoke(workflow)");
            RequestStateUpdate?.Invoke(workflow);
            return null;
        }

        public override int GetMajorVersion()
        {
            return 1;
        }

        public override int GetMinorVersion()
        {
            return 0;
        }

        public override int GetPatchVersion()
        {
            return 0;
        }
    }
}