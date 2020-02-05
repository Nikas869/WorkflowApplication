using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using DataOutput.Properties;
using JdSuite.Common;
using JdSuite.Common.Logging;
using JdSuite.Common.Module;
using JdSuite.Common.Module.MefMetadata;
using NLog;

namespace DataOutput
{

    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.OUTPUTS)]
    [ExportMetadata("ModuleType", typeof(DataOutputModule))]
    public class DataOutputModule : BaseModule, IOutPutModule
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(DataOutputModule));

        MainWindowClass mainWindowClass = null;

        private static List<DataOutputModule> Instances = new List<DataOutputModule>();
        private readonly string ActiveKey = "CommandId_1";

        public Task ExecutionTask { get; set; }
        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        #region Properties
        private InputNode InputNode
        {
            get { return (InputNode)InputNodes.FirstOrDefault(); }
            set
            {

                if (InputNodes.Count >= 1)
                    InputNodes[0] = value;
                else
                    InputNodes.Add(value);
            }
        }

        public int MarkedMenu { get; private set; } = 99999;

        public Workflow WorkFlow { get; private set; }

        public DataOutputModule()
        {
            DisplayName = "DataOutput";

            logger.Info("creating object");
            JdSuite.Common.Logging.Logger.AppName = ModuleName;

            InputNode = new InputNode(this, "Schema", "DataLocation");

            Instances.Add(this);

            this.ModuleType = ModuleType.DataOutput;
        }

        protected override string ModuleName { get { return "DataOutputRelease"; } }



        protected override Bitmap Icon { get { return Resources.icon_20; } }

        protected override string ToolTip { get { return "Provides DataOutput for other modules."; } }

        #endregion Properties


        public override void SetContextMenuItems(ContextMenu ctxMenu)
        {
            base.SetContextMenuItems(ctxMenu);

            MenuItem item = new MenuItem();
            item.Click += F6_Click;
            item.Header = "Proof Data (F6)";

            ctxMenu.Items.Add(item);

            item = new MenuItem();
            item.Click += F5_Click;
            item.Header = "Production (F5)";

            ctxMenu.Items.Add(item);

        }

        private void F5_Click(object sender, RoutedEventArgs e)
        {
            SetKey((int)Key.F5);

        }

        private void F6_Click(object sender, RoutedEventArgs e)
        {
            SetKey((int)Key.F6);
        }

        public void SetKey(int iKey)
        {
            StateInfo[ActiveKey] = iKey.ToString();

            logger.Info($"{DisplayName} Entered");
            if (iKey == (int)Key.F5)
            {
                MarkedMenu = iKey;
                base.ActiveKey = (int)Key.F5;
                if (ModulePageBorder == null)
                    return;

                ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Blue;

                var obj = Instances.FirstOrDefault(x => x.MarkedMenu == iKey);


                if (obj != null && obj != this)
                {
                    obj.SetKey((int)Key.F6);
                }


            }
            else if (iKey == (int)Key.F6)
            {
                MarkedMenu = iKey;
                base.ActiveKey = (int)Key.F6;
                if (ModulePageBorder == null)
                    return;
                ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Yellow;

                var obj = Instances.FirstOrDefault(x => x.MarkedMenu == iKey);

                if (obj != null && obj != this)
                {
                    obj.SetKey((int)Key.F5);
                }


            }
        }


        private List<BaseModule> GetModuleChain()
        {
            List<BaseModule> modules = new List<BaseModule>();
            modules.Add(this);
            InputModule.Register(this, modules);
            return modules;
        }

        public void StartRunChain()
        {
            if (!CanProcess())
                return;

            var modules = GetModuleChain();
            //var sourceModule = modules.LastOrDefault();

            WorkInfo winfo = new WorkInfo(modules);
            winfo.Command = this.MarkedMenu;
            winfo.OutPutModule = this;



            Thread.Sleep(1000 * 2);

            BaseModule currentModule = winfo.ModuleList.LastOrDefault();
            bool bStatus = false;
            try
            {
            run_cycle:
                winfo.Log(this.DisplayName, LogLevel.Info, $"Starting execution of {currentModule.DisplayName} module");
                bStatus = currentModule.Run(winfo);
                logger.Info($"{currentModule.DisplayName}.Run() returned status:{bStatus}");
                if (bStatus)
                {
                    var nextModule = winfo.NextModule(currentModule);
                    if (nextModule != null)
                    {
                        currentModule = nextModule;
                        goto run_cycle;
                    }
                }
                else
                {
                    winfo.Log(currentModule.DisplayName, LogLevel.Error, $"Halted execution as Module status is not Successful");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Halting execution as Module {currentModule.DisplayName} encountered an error");
                winfo.Log(currentModule.DisplayName, LogLevel.Error, $"Halting execution as due to error: {ex.Message}");
            }
        }

        public override void OpenWindow(Workflow workflow)
        {
            ShowWindow();
        }



        public override bool Run(WorkInfo workInfo)
        {
            logger.Info("Entered_method");
            bool bStatus = false;
            try
            {
                var connectorState = ((OutputNode)this.InputNode.Connector).State;
                InputNode.State.DataFilePath = connectorState.DataFilePath;
                InputNode.State.Schema = connectorState.Schema;

                if (!File.Exists(InputNode.State.DataFilePath))
                {
                    workInfo.Log(this.DisplayName, LogLevel.Error, $"Output DataFile from previous file {InputNode.State.DataFilePath} does not exist");
                    return false;
                }

                if (InputNode.State.Schema==null)
                {
                    workInfo.Log(this.DisplayName, LogLevel.Error, $"Schema is not passed to OutputModule from previous module");
                    return false;
                }

                mainWindowClass.LoadData(InputNode.State.DataFilePath, InputNode.State.Schema);
                bStatus = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DataOutputModule encountered an exception");
                bStatus = false;
            }
            return bStatus;
        }

        public override object Execute(Workflow workflow)
        {
            logger.Info($"Entered {DisplayName}");

            if (workflow.Command == (int)MarkedMenu && (workflow.Command == (int)Key.F5 || workflow.Command == (int)Key.F6))
            {
                if (!CanProcess())
                    return null;

                this.WorkFlow = workflow;

                logger.Trace("Calling StartChain");
                this.ExecutionTask = Task.Factory.StartNew(new Action(StartRunChain), CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
                logger.Trace("Calling ShowWindow");
                ShowWindow();

            }

            else if (workflow.Command == (int)Commands.DoubleClick)
            {
                ShowWindow();
            }



            return null;
        }



        /// <summary>
        /// View Data menu workflow
        /// </summary>
        private void ShowWindow()
        {
            if (!CanProcess())
                return;


            // ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Blue;

            ModuleState state = null;
            try
            {
                logger.Info($"Calling {InputModule.DisplayName}.Run() on input module");


                // JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage($"Output Module: Sending run command to Module [{InputModule.DisplayName}]");

                /*
                bool status = await System.Threading.Tasks.Task.Factory.StartNew(() =>
                 {
                     return InputModule.Run(this.MarkedMenu);
                 });
                 */


                /*
                bool status = await InputModule.Run(this.MarkedMenu);


               logger.Info($"{InputModule.DisplayName}.Run() Status={status}");

                var workFlow = InputModule.GetState(InputModule);
                state = ((OutputNode)this.InputNode.Connector).State;

                string tempmsg = status ? "Successful" : "Failed";

                JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage($"Output Module: [{InputModule.DisplayName}] execution status {tempmsg}");

                if (!status)
                {
                    MessageService.ShowError("Module Execution Error", $"{InputModule.DisplayName} execution error");
                    return;
                }


                if (state.Schema == null)
                {
                    MessageService.ShowError("Data Connection Error", $"In Module {InputModule.DisplayName} Sate has invalid Schema");
                    return;
                }

                if (!File.Exists(state.DataFilePath))
                {
                    MessageService.ShowError("Data Connection Error", $"In Module {InputModule.DisplayName} output file [{state.DataFilePath}] does not exists");
                    return;
                }
                */
                // ModulePageBorder.BorderBrush = System.Windows.Media.Brushes.Green;

                if (mainWindowClass != null)

                    mainWindowClass.Close();
                mainWindowClass = null;

                mainWindowClass = new MainWindowClass();
                mainWindowClass.Schema = InputNode.State.Schema;
                mainWindowClass.DataFile = InputNode.State.DataFilePath;
                mainWindowClass.TextBlockModuleTitle.Text = "Output Module [" + DisplayName + "]";

                logger.Info("Loading data from xml file [{0}]", InputNode.State.DataFilePath);

                JdSuite.Common.ApplicationWindowUtil.ShowStatusBarMessage($"Ready...");

                mainWindowClass.ShowDialog();


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

                MessageService.ShowError("DataOutput Module Execution Error", ex.Message);
            }
        }



        private bool CanProcess()
        {
            if (!IsConnected())
            {
                MessageService.ShowError("Data Connection Error", "Data Output module input node is not connected with any other module");
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



            return true;
        }

        #region HouseKeeping

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



        public override void WriteParameter(XmlWriter writer)
        {

        }

        public override void ReadParameter(XElement reader)
        {
            if (StateInfo.ContainsKey(ActiveKey))
            {
                int itt;
                if (int.TryParse(StateInfo[ActiveKey], out itt))
                {
                    SetKey(itt);
                }
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



        void IOutPutModule.Log(string Source, LogEventInfo logEvent)
        {
            mainWindowClass?.loggerControl?.Log(new Controls.LogEventViewModel(logEvent));
        }

        void IOutPutModule.Log(string Source, LogLevel level, string message)
        {
            mainWindowClass?.Log(Source, level, message);
        }

        void IOutPutModule.UpdateProgress(BaseModule Module, float ProgressPercent)
        {
            if (mainWindowClass != null)
            {
                string Source = Module.DisplayName;
                string message = $"Progress {ProgressPercent.ToString("00.00")} %";

                mainWindowClass?.Log(Source, NLog.LogLevel.Info, message);
            }
        }

        #endregion HouseKeeping
    }
}
