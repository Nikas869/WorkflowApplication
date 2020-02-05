using JdSuite.Common.Module;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel.Composition;
using JdSuite.Common.Module.MefMetadata;
using System.Xml.Linq;
using JdSuite.Common.Logging;
using System.Windows;
using JdSuite.Common;
using CSVInput.Properties;
using System.Drawing;
using System.Xml;
using System.Xml.XPath;
using System.Windows.Controls;

namespace CSVInput
{

    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.DATA_INPUTS)]
    [ExportMetadata("ModuleType", typeof(CSVModule))]
    public class CSVModule : BaseModule
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(CSVModule));

        internal DataFileInfo DataFileInfo { get; set; } = new DataFileInfo();


        protected override string ModuleName { get { return "CSVModule"; } }


        protected override Bitmap Icon { get { return Resources.Icon; } }

        protected override string ToolTip { get { return "CSV Input module."; } }

        public OutputNode OutputNode
        {
            get { return (OutputNode)OutputNodes.FirstOrDefault(); }
        }


        public CSVModule()
        {
            DisplayName = "CSV";
            logger.Info("creating object");
            Logger.AppName = ModuleName;

            var op = new OutputNode(this, "MainOutput", "DataLocation");
            op.State = new ModuleState();


            AddOutputNode(op);

            this.DataFileInfo.Delimiter = ",";
            this.DataFileInfo.Encoding = Encoding.UTF8.EncodingName;
            this.DataFileInfo.FileType = "CSV";
            this.DataFileInfo.FirstRowHasHeader = true;
            this.DataFileInfo.RootArrayName = "Root";
            this.DataFileInfo.ShowRecordCount = "2000";
            this.DataFileInfo.TextQualifier = "\"";

        }

        public override bool Run(WorkInfo workInfo)
        {
            bool bStatus = false;
            string msg = "";
            try
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Executing workflow");
                // JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage("CSV Module: Running Command");

              
                logger.Info($"Running {DisplayName}");

                if (!File.Exists(this.DataFileInfo.FilePath))
                {
                    msg = $"{DisplayName} is not configured, Input File does not exist";
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, msg);
                    logger.Error(msg);

                    return false;
                }


                if (this.OutputNode.State.Schema == null)
                {
                    msg = $"{DisplayName} schema is not set, {DisplayName} is not configured";

                    logger.Error(msg);
                    //MessageService.ShowError($"{DisplayName} schema is not set", $"{DisplayName} is not configured");
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                    return false;
                }

                if (this.OutputNode.State.DataFilePath == null)
                {
                    msg = $"Halting execution as {DisplayName} xml output file is not set";
                    // MessageService.ShowError($"{DisplayName} xml output file is not set", $"{DisplayName} is not configured");
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                    logger.Error(msg);
                    return false;
                }

                if (this.DataFileInfo.FileType.ToLower() == "csv")
                {
                    logger.Info("Creating xml file from csv {0}", this.DataFileInfo.FilePath);
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"Creating xml file from csv {this.DataFileInfo.FilePath}");

                    CSVParser parser = new CSVParser();
                    parser.Delimiter = this.DataFileInfo.Delimiter;
                    parser.FileEncoding = Encoding.GetEncoding(this.DataFileInfo.Encoding);
                    parser.FirstLineHasHeaders = this.DataFileInfo.FirstRowHasHeader;
                    parser.SkipFirstLine = false;
                    parser.SourceFileName = this.DataFileInfo.FilePath;
                    parser.XMLRootName = this.DataFileInfo.RootArrayName;

                    //JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage("CSV Module: Starting to parse CSV File");
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"CSV Module: Starting to parse CSV File");

                    parser.Parse();
                    logger.Trace("Writing_to_output_file {0}", this.OutputNode.State.DataFilePath);

                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"Writing to output file {this.OutputNode.State.DataFilePath}");

                    //JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage("CSV Module: Saving CSV file xml");
                    parser.SaveXML(this.OutputNode.State.DataFilePath);

                    logger.Info($"Created xml file from csv {this.OutputNode.State.DataFilePath}");
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"Created xml file from csv {this.OutputNode.State.DataFilePath}");
                }
                else if (this.DataFileInfo.FileType.ToLower() == "dbf")
                {
                    logger.Info(msg = $"Creating xml file from dbf {this.DataFileInfo.FilePath}");

                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, msg);

                    DBFParser parser = new DBFParser();

                    parser.FileEncoding = Encoding.GetEncoding(this.DataFileInfo.Encoding);
                    parser.SourceFileName = this.DataFileInfo.FilePath;
                    parser.XMLRootName = this.DataFileInfo.RootArrayName;

                    msg = "CSV Module: Parsing DBF File";
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, msg);

                    // JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage("CSV Module: Parsing DBF File");
                    parser.Parse();

                    // JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage("CSV Module: Saving DBF file xml");

                    msg = "CSV Module: Saving DBF file xml";
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, msg);
                    parser.SaveXML(this.OutputNode.State.DataFilePath);

                    logger.Info("Created xml file from dbf {0}", this.OutputNode.State.DataFilePath);

                    msg = $"CSV Module: Transformed DBF to xml {this.OutputNode.State.DataFilePath}";

                    workInfo.Log(this.DisplayName, NLog.LogLevel.Info, msg);

                }

                msg = "CSV Module: Executed its part successfully";
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, msg);
                //JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage("CSV Module: Executed its part successfully");
                bStatus = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"An error occured in {this.DisplayName}");

                msg = $"An error occured in {this.DisplayName} Error:{ex.Message}";

                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);

                bStatus = false;
            }

           
            return bStatus;
        }


        public override object Execute(Workflow workflow)
        {
            if (this.OutputNode.State == null)
                this.OutputNode.State = new ModuleState();

            var element = workflow.GetAppState(ModuleName, Guid);
            if (element != null)
            {
                //ReadProgramCSV(element.Element("Root"));
            }


            ShowWindow();


            RequestStateUpdate?.Invoke(workflow);
            return null;
        }

        private void ShowWindow()
        {

            try
            {
                MainWindowClass mainWindowClass = new MainWindowClass();
                mainWindowClass.moduleReference = this;
                mainWindowClass.SetPropsModuleToWindow(this.DataFileInfo);
                var result = mainWindowClass.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    this.OutputNode.State.Schema = mainWindowClass.RootSchema;

                    if (string.IsNullOrEmpty(this.OutputNode.State.DataFilePath))
                    {
                        string outputFile = DataDir + "CSVOutput_" + DateTime.Now.ToString("yyMMddHHmmssfff") + ".xml";

                        this.OutputNode.State.DataFilePath = outputFile;
                        logger.Trace("OutputNode.State.InputPath={0}", outputFile);
                    }
                }
            }
            catch (Exception e)
            {
                var msg = $"Critical failure in CSV Converter: {e.Message} ";
                logger.Error(msg);
                throw new Exception(msg);
            }
        }




        public override void SetContextMenuItems(ContextMenu ctxMenu)
        {
            /*
            MenuItem item = new MenuItem();
            item.Click += MenuItemViewData_Click;
            item.Header = "View Data(F5)";
            //item.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("pack://application:,,,/Images/cancel.png", UriKind.Absolute)) };

            ctxMenu.Items.Add(item);

            item = new MenuItem();
            item.Click += MenuItemRunData_Click;
            item.Header = "Run Data(F6)";

            ctxMenu.Items.Add(item);
            */
        }


        public override void WriteParameter(XmlWriter writer)
        {

            writer.WriteStartElement("Parameters");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataFileInfo));

            xmlSerializer.Serialize(writer, this.DataFileInfo);

            writer.WriteEndElement();

        }

        public override void ReadParameter(XElement BaseModuleNode)
        {
            var parameterNode = BaseModuleNode.XPathSelectElement("//Parameters");

            if (parameterNode != null && parameterNode.HasElements)
            {
                var reader = parameterNode.CreateReader();
                reader.MoveToContent();
                reader.Read();
                reader.MoveToContent();

                logger.Info("Loading parameter");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataFileInfo));
                this.DataFileInfo = (DataFileInfo)xmlSerializer.Deserialize(reader);

            }
        }
    }
}
