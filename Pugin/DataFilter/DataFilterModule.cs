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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Xml;
using JdSuite.DataFilter.Properties;
using JdSuite.DataFilter.Models;
using JdSuite.DataFilter.ViewModels;
using System.Xml.XPath;

namespace JdSuite.DataFilter
{

    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.DATA_MANIPULATION)]
    [ExportMetadata("ModuleType", typeof(DataFilterModule))]
    public class DataFilterModule : BaseModule
    {

        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(DataFilterModule));


        private List<FilterField> FilterFields { get; set; } = new List<FilterField>();


        public Workflow WorkFlow { get; private set; }


        public InputNode InputNode
        {
            get { return (InputNode)InputNodes.FirstOrDefault(); }
        }

        public OutputNode OutputNode
        {
            get { return (OutputNode)OutputNodes.FirstOrDefault(); }
        }

        public OutputNode OutputNode2
        {
            get
            {

                if (OutputNodes.Count >= 2)
                {
                    return (OutputNode)OutputNodes[1];
                }
                else
                    return null;
            }
        }


        public DataFilterModule()
        {
            DisplayName = "DataFilter";
            logger.Info("creating object");
            Logger.AppName = ModuleName;

            var ip = new InputNode(this, "Input", "DataLocation");
            AddInputNode(ip);

            var op = new OutputNode(this, "MainOutput", "DataLocation");
            op.State = new ModuleState();

            AddOutputNode(op);

            ModuleCategory = IModuleCategory.DATA_MANIPULATION;
            ModuleType = ModuleType.DataFilter;

        }

        public void CreateOutPutNode2()
        {
            if (OutputNode2 != null)
                return;
            var op = new OutputNode(this, "ElseOutput", "DataLocation");

            op.State = new ModuleState();
            AddOutputNode(op);
        }

        public void RemoveOutPutNode2()
        {

            if (OutputNodes.Count >= 2)
                RemoveNode(OutputNode2);

        }

        protected override string ModuleName => "DataFilter";



        protected override Bitmap Icon { get { return Resources.icon_20; } }

        protected override string ToolTip { get { return "Provides data filtering."; } }


        public override void OpenWindow(Workflow workflow)
        {
            logger.Info("Entered");

            if (!ValidateInputSchema())
                return;

            this.WorkFlow = workflow;

            ShowParameterWindow();
        }

        public override object Execute(Workflow workflow)
        {
            logger.Info("Entered Command:{0}", workflow.Command);

            if (!ValidateInputSchema())
                return null;

            string msg = "";
            bool failed = false;
            this.WorkFlow = workflow;

            if (workflow.Command == (int)JdSuite.Common.Module.Commands.DoubleClick)
            {
                ShowParameterWindow();
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
            logger.Error("Invalid menu item click");
            // Run((int)Key.F6);
        }

        /// <summary>
        /// View data menu workflow
        /// </summary>
        private void ShowParameterWindow()
        {
            if (!ValidateInputSchema())
                return;

            ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Blue;

            ModuleState state1 = null;
            try
            {
                var workFlow = InputModule.GetState(InputModule);
                state1 = ((OutputNode)this.InputNode.Connector).State;
                var schema = ((OutputNode)this.InputNode.Connector).GetSchema();


                MainWindow mainWindowClass = new MainWindow();
                mainWindowClass.DFModule = this;
                //1. Load Schema
                mainWindowClass.LoadSchema(state1.Schema);


                //2.Load Filter Fields
                foreach (var item in FilterFields)
                    mainWindowClass.AddFilterField(item);

                //3. Show dialog
                var result = mainWindowClass.ShowDialog();

                //4. Sync back dialog changes
                if (result.HasValue && result.Value)
                {
                    //Sync back Filter fields
                    this.FilterFields.Clear();
                    this.FilterFields.AddRange(mainWindowClass.FilterFields);
                    ParameterCount = this.FilterFields.Count;
                }

                this.InputNode.State = state1;

                var outputFile = DataDir + "DataFilter_" + DisplayName + "_" + DateTime.Now.ToString("yyMMddHHmmssfff") + ".xml";
                this.OutputNode.State.DataFilePath = outputFile;
                this.OutputNode.State.Schema = state1.Schema;
                this.OutputNode.State.TextEncoding = state1.TextEncoding;
                this.OutputNode.State.InputIsSchema = state1.InputIsSchema;

                if (this.OutputNode2 != null)
                {
                    var outputFile2 = DataDir + "DataFilter_" + DisplayName + "_" + DateTime.Now.ToString("yyMMddHHmmssff") + ".xml";
                    this.OutputNode2.State.DataFilePath = outputFile2;
                    this.OutputNode2.State.Schema = state1.Schema;
                    this.OutputNode2.State.TextEncoding = state1.TextEncoding;
                    this.OutputNode2.State.InputIsSchema = state1.InputIsSchema;

                }

            }
            catch (Exception ex)
            {
                if (state1 == null)
                {
                    logger.Error(ex, $"Data loading error in module class");
                }
                else
                {
                    logger.Error(ex, $"XML Loading Error {state1.DataFilePath}");
                }
            }
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

                //var workFlow = InputModule.GetState(InputModule);
                //var state = ((OutputNode)this.InputNode.Connector).State;

                // string msg = "";



                // ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Green;

                logger.Trace($"Creating [{DisplayName}] XmlFilter object");
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Starting XmlFilter process");

                using (Xml.XmlFilter filter = new Xml.XmlFilter())
                {
                    filter.InputFileName = InputNode.State.DataFilePath;
                    filter.OutputFileName = OutputNode.State.DataFilePath;

                    if (OutputNode2 != null)
                    {
                        filter.OutputFileName2 = OutputNode2.State.DataFilePath;
                    }

                    filter.DataDir = this.DataDir;

                    foreach (var ff in FilterFields)
                    {
                        filter.Filters.Add(ff);
                    }

                    filter.InputNodeCount = workInfo.Indexer.ItemCount;
                    Action<float> OnProgress = (p) =>
                      {
                          workInfo.UpdateProgress(this, p);
                      };

                    filter.OnProgressUpdate = OnProgress;

                    filter.Filter();
                }
               
                bStatus = true;

               
            }
            catch (XPathException ex)
            {
                var msg = $"Critical failure in [{DisplayName}] module error: {ex.Message} ";
                logger.Error(msg);
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                bStatus = false;
            }
            catch (Exception ex)
            {
                var msg = $"Critical failure in [{DisplayName}] module error: {ex.Message} ";
                logger.Error(msg);
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                bStatus = false;
            }

            workInfo.Log(this.DisplayName, NLog.LogLevel.Info, $"Module completed processing with Status: {bStatus}");
            logger.Info($"{this.DisplayName} module completed processing with status:{bStatus}");
            return bStatus;
        }


        /// <summary>
        /// Call it from ShowWindows
        /// </summary>
        /// <returns></returns>
        private bool ValidateInputSchema()
        {
            if (!IsConnected())
            {
                MessageService.ShowError("Data Connection Error", "Data Filter module input node is not connected with any other module");
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
            /*
            if (!File.Exists(state.InputPath))
            {
                MessageService.ShowError("Data Connection Error", $"In Module {prevModName} Sate has invalid input path");
                return false;
            }
            */
            if (state.Schema == null)
            {
                MessageService.ShowError("Data Connection Error", $"In Module {prevModName} Sate has invalid Schema");
                return false;
            }


            return true;
        }


        private bool CanRun(WorkInfo workInfo)
        {
            if (!IsConnected())
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"{this.DisplayName} input node is not connected with any other module");
                return false;
            }

            string prevModuleName = workInfo.NextModuleRTL(this).DisplayName;
            if (workInfo.Schema == null)
            {

                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as Previous Module {prevModuleName} dis not pass Schema");
                return false;
            }




            if (this.InputNode.State == null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State is null");
                return false;
            }

            this.InputNode.State.DataFilePath = ((OutputNode)this.InputNode.Connector).State.DataFilePath;
            this.InputNode.State.Schema = ((OutputNode)this.InputNode.Connector).State.Schema;

            if (this.InputNode.State.DataFilePath == null)
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State.DataFilePath is null");
                return false;
            }

            if (!File.Exists(this.InputNode.State.DataFilePath))
            {
                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as {InputNode.State.DataFilePath} does not exist ");
                return false;
            }

            string msg = "";
            if (FilterFields.Count <= 0)
            {
                msg = string.Format("Halting execution as as filter fields are not provided");
                logger.Warn(msg);

                workInfo.Log(this.DisplayName, NLog.LogLevel.Error, msg);
                return false;
            }

            /*
            string prevNodeName = InputModule.DisplayName;
            var workFlow = InputModule.GetState(InputModule);
            var state = ((OutputNode)this.InputNode.Connector).State;
            var schema = ((OutputNode)this.InputNode.Connector).GetSchema();
            */

            /*
            if (!File.Exists(state.InputPath))
            {
                MessageService.ShowError("Data Connection Error", $"In Module {prevModName} Sate has invalid input path");
                return false;
            }
            */

            return true;
        }

        private bool IsConnected()
        {
            if (!this.InputNode.IsConnected())
                return false;

            return !(this.InputNode.Connector == null);
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
            if (FilterFields.Count > 0)
            {
                writer.WriteStartElement("Parameters");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<FilterField>));

                xmlSerializer.Serialize(writer, FilterFields);

                writer.WriteEndElement();
            }
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

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<FilterField>));
                FilterFields = (List<FilterField>)xmlSerializer.Deserialize(reader);

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
