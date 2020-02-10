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
using System.Drawing;
using JdSuite.DataSorting.Properties;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Xml;
using System.Xml.XPath;

namespace JdSuite.DataSorting
{

    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.DATA_MANIPULATION)]
    [ExportMetadata("ModuleType", typeof(DataSorterModule))]
    public class DataSorterModule : BaseModule
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(DataSorterModule));


        private List<SortingField> SortingFields = new List<SortingField>();

        public Workflow WorkFlow { get; private set; }
        public InputNode InputNode
        {
            get { return (InputNode)InputNodes.FirstOrDefault(); }
        }

        public OutputNode OutputNode
        {
            get { return (OutputNode)OutputNodes.FirstOrDefault(); }
        }


        public DataSorterModule()
        {
            DisplayName = "DataSorter";
            logger.Info("creating object");
            Logger.AppName = ModuleName;

            var ip = new InputNode(this, "Schema", "DataLocation");
            AddInputNode(ip);

            var op = new OutputNode(this, "Schema", "DataLocation");
            op.State = new ModuleState();
            AddOutputNode(op);

            // AddOutputNode(OutputNode); Not required
        }

        protected override string ModuleName { get { return "DataSorter"; } }



        protected override Bitmap Icon { get { return Resources.sorting_icon_9; } }

        protected override string ToolTip { get { return "Provides Sorting for other modules."; } }


        public override void OpenWindow(Workflow workflow)
        {
            logger.Info("Entered");

            if (!ValidateSchemaPass())
                return;

            this.WorkFlow = workflow;

            ShowParameterWindow();
        }

        public override object Execute(Workflow workflow)
        {
            logger.Info("Entered");

            if (!ValidateSchemaPass())
                return null;

            string msg = "";
            bool failed = false;
            this.WorkFlow = workflow;

            if (workflow.Command == (int)JdSuite.Common.Module.Commands.DoubleClick)
            {
                ShowParameterWindow();
            }
            else if (workflow.Command == (int)Key.F6)
            {
               // Run((int)workflow.Command);
            }

            var element = workflow.GetAppState(ModuleName, Guid);
            if (element != null)
            {
                //ReadProgramCSV(element.Element("Root"));
            }



            if (this.OutputNode.State.Schema == null)
            {
                msg += "Process is incomplete as Output Schema is not set.";
                failed = true;
            }

            if (this.OutputNode.State.DataFilePath == null)
            {

                msg += " Output node data file path is not set. ";
                failed = true;
            }



            if (failed)
            {
                logger.Error(msg);
                MessageService.ShowError("Output node validation error", msg);
            }

            if (failed)
            {
                if (OutputNode.IsConnected())
                {
                    msg = "Disconnecting module due to errors: " + msg;
                    OutputNode.Disconnect();
                }

                logger.Warn(msg);

                this.OutputNode.State.DataFilePath = "";
                return null;
            }


            logger.Info("Calling   RequestStateUpdate?.Invoke(workflow)");
            RequestStateUpdate?.Invoke(workflow);

            return null;
        }



        public override void SetContextMenuItems(ContextMenu ctxMenu)
        {
            MenuItem item = new MenuItem();
            item.Click += MenuItemViewData_Click;
            item.Header = "View Data(F5)";
            //item.Icon = new System.Windows.Controls.Image { Source = new BitmapImage(new Uri("pack://application:,,,/Images/cancel.png", UriKind.Absolute)) };

            ctxMenu.Items.Add(item);

            item = new MenuItem();
            item.Click += MenuItemRunData_Click;
            item.Header = "Run Data(F6)";

            ctxMenu.Items.Add(item);
        }

        private void MenuItemViewData_Click(object sender, RoutedEventArgs e)
        {
            ShowParameterWindow();

        }

        private void MenuItemRunData_Click(object sender, RoutedEventArgs e)
        {
          //  Run((int)Key.F6);
        }

        /// <summary>
        /// View data menu workflow
        /// </summary>
        private void ShowParameterWindow()
        {
            if (!ValidateSchemaPass())
                return;

            ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Blue;

            ModuleState state = null;
            try
            {
                var workFlow = InputModule.GetState(InputModule);
                state = ((OutputNode)this.InputNode.Connector).State;
                var schema = ((OutputNode)this.InputNode.Connector).GetSchema();


                MainWindow mainWindowClass = new MainWindow();
                //1. Load Schema
                mainWindowClass.LoadSchema(state.Schema);

                logger.Info("Loading data from xml file {0}", state.DataFilePath);
                //2. Set Input Data file
                mainWindowClass.WindowViewModel.InputDataFile = state.DataFilePath;

                var outputFile = DataDir + "DataSorter_" + DisplayName + "_" + DateTime.Now.ToString("MMddHHmmssfff") + ".xml";

                //3.
                mainWindowClass.WindowViewModel.OutputDataFile = outputFile;

                //4.Load Sorting Fields
                foreach (var item in SortingFields)
                    mainWindowClass.WindowViewModel.SortingFields.Add(item);

                mainWindowClass.ShowDialog();

                //Sync back sorting fields
                this.SortingFields.Clear();
                this.SortingFields.AddRange(mainWindowClass.WindowViewModel.SortingFields.ToList());
                ParameterCount = this.SortingFields.Count;


                this.InputNode.State = state;

                this.OutputNode.State.DataFilePath = mainWindowClass.WindowViewModel.OutputDataFile;
                this.OutputNode.State.Schema = state.Schema;
                this.OutputNode.State.TextEncoding = state.TextEncoding;
                this.OutputNode.State.InputIsSchema = state.InputIsSchema;

            }
            catch (Exception ex)
            {
                if (state == null)
                {
                    logger.Error(ex, $"Data loading error in module class");
                }
                else
                {
                    logger.Error(ex, $"XML Loading Error {state.DataFilePath}");
                }
            }
        }


        private bool CanRun(WorkInfo workInfo)
        {
            if (InputNode.IsConnected()==false)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"{this.DisplayName} input node is not connected with any other module");
                return false;
            }

           

            if (this.InputNode.State == null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State is null");
                return false;
            }

            this.InputNode.State.DataFilePath = ((OutputNode)this.InputNode.Connector).State.DataFilePath;
            this.InputNode.State.DataFile = ((OutputNode)this.InputNode.Connector).State.DataFile;
            this.InputNode.State.Schema = ((OutputNode)this.InputNode.Connector).State.Schema;

            if (this.InputNode.State.DataFilePath == null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State.DataFilePath is null");
                return false;
            }
            if (this.InputNode.State.DataFile == null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State.DataFile is null");
                return false;
            }

            if (!File.Exists(this.InputNode.State.DataFilePath))
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as {InputNode.State.DataFilePath} does not exist ");
                return false;
            }

            string prevModuleName = workInfo.NextModuleRTL(this).DisplayName;
            if (InputNode.State.Schema == null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as Previous Module {prevModuleName} dis not pass Schema");
                return false;
            }

            if (OutputNode.IsConnected()==false)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as OutputNode is not connected");
                return false;
            }

            if (OutputNode.State==null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as OutputNode.State is null");
                return false;
            }

            if (string.IsNullOrEmpty( OutputNode.State.DataFilePath))
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as OutputNode.DataFilePath is not set");
                return false;
            }
            this.OutputNode.State.Schema = this.InputNode.State.Schema;
            this.OutputNode.State.DataFile = this.InputNode.State.DataFile;

            string msg = "";
            if (SortingFields.Count <= 0)
            {
                msg = string.Format("Halting execution as as sorting fields are not provided");
                logger.Warn(msg);

                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                return false;
            }


            return true;
        }


        /// <summary>
        /// Run Data menu workflow
        /// </summary>
        public override bool Run(WorkInfo workInfo)
        {
            bool bStatus = false;
            try
            {

                logger.Info($"{this.DisplayName} Running_workflow");
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Running workflow");

                if (!CanRun(workInfo))
                {
                    return false;
                }

                 
               // ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Green;

                logger.Trace("Creating sorter object");

                this.OutputNode.State.Schema = this.InputNode.State.Schema;

                XMLSorter sorter = new XMLSorter(this.OutputNode.State.DataFile, this.SortingFields);
                logger.Info("Calling Sorter.Sort()");
                sorter.Sort();

                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Data sorting completed, now saving sorted data");
                this.OutputNode.State.DataFile.SaveAsXml(this.OutputNode.State.DataFilePath);

                // JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage($"Data Sorter Module: sorting process completed");
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "DataSorter module completed sorting process");

                var nextModule = workInfo.NextModule(this);
                if (nextModule == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"No module is connected with {this.DisplayName}");
                    return false;
                }
                bStatus = true;
               

            }
            catch (Exception ex)
            {
                var msg = $"Critical failure in {this.DisplayName} module: {ex.Message} ";
                logger.Error(msg);
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                bStatus = false;
            }

            workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"Module completed processing with Status: {bStatus}");
            logger.Info($"{this.DisplayName} module completed processing with status:{bStatus}");

            return bStatus;
        }

        private bool ValidateDataPass()
        {
            if (InputNode.IsConnected()==false)
            {
                MessageService.ShowError("Data Connection Error", "Data Sorter module input node is not connected with any other module");
                return false;
            }

            string prevModName = InputModule.DisplayName;
            var workFlow = InputModule.GetState(InputModule);
            var state = ((OutputNode)this.InputNode.Connector).State;
            var schema = ((OutputNode)this.InputNode.Connector).GetSchema();

            if (state == null)
            {
                MessageService.ShowError("Data Connection Error", $"In Module {prevModName} passed invalid state object");
                return false;
            }

            if (!File.Exists(state.DataFilePath))
            {
                MessageService.ShowError("Data Connection Error", $"In Module {prevModName} Sate has invalid input path");
                return false;
            }

            if (state.Schema == null)
            {
                MessageService.ShowError("Data Connection Error", $"In Module {prevModName} Sate has invalid Schema");
                return false;
            }


            return true;
        }


        private bool ValidateSchemaPass()
        {
            if (InputNode.IsConnected()==false)
            {
                MessageService.ShowError("DataSorter Data Connection Error", "Data Sorter module input node is not connected with any other module");
                return false;
            }

            string prevModName = InputModule.DisplayName;
            var workFlow = InputModule.GetState(InputModule);
            var state = ((OutputNode)this.InputNode.Connector).State;
            var schema = ((OutputNode)this.InputNode.Connector).GetSchema();

            if (state == null)
            {
                MessageService.ShowError("DataSorter Data Connection Error", $"In Module {prevModName} passed invalid state object");
                return false;
            }



            if (state.Schema == null)
            {
                MessageService.ShowError("DataSorter Data Connection Error", $"In Module {prevModName} Sate has invalid Schema");
                return false;
            }


            return true;
        }


        

        private BaseModule InputModule
        {
            get
            {

                if (this.InputNode.Connector == null) return null;

                return this.InputNode.Connector.GetModule();
            }
        }




        #region HouseKeeping

        public override void WriteParameter(XmlWriter writer)
        {
            if (SortingFields.Count > 0)
            {
                writer.WriteStartElement("Parameters");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SortingField>));

                xmlSerializer.Serialize(writer, SortingFields);

                writer.WriteEndElement();
            }
        }


        public override void ReadParameter(XElement BaseModuleNode)
        {
            var parameterNode = BaseModuleNode.XPathSelectElement("//Parameters");

            if (parameterNode != null && parameterNode.HasElements)
            {
                logger.Info("Loading parameter");

                var reader = parameterNode.CreateReader();
                reader.MoveToContent();
                reader.Read();
                reader.MoveToContent();


                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SortingField>));
                SortingFields = (List<SortingField>)xmlSerializer.Deserialize(reader);
            }
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

        #endregion HouseKeeping
    }
}
