using JdSuite.Common;
using JdSuite.Common.Logging;
using JdSuite.Common.Module;
using JdSuite.Common.Module.MefMetadata;
using ScriptingApp.Core;
using ScriptingApp.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace ScriptingApp
{
    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.DATA_MANIPULATION)]
    [ExportMetadata("ModuleType", typeof(ScriptingModule))]
    public class ScriptingModule : BaseModule
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(ScriptingModule));
        protected override string ModuleName => "Scripting";
        protected override Bitmap Icon => Resources.Icon;
        protected override string ToolTip => "Provides an Scripting support.";

        /// <summary>
        /// C# code
        /// </summary>
        public string CodeText { get; set; }

        public Field FullInputSchema { get; set; }
        public Field FullOutputSchema { get; set; }

        public ScriptingModule()
        {
            Logger.AppName = ModuleName;
            DisplayName = "Script";

            var inNode = new InputNode(this, "Schema", "DataLocation");
            AddInputNode(inNode);

            var outNode = new OutputNode(this, "Schema", "DataLocation");
            outNode.State = new ModuleState();
            AddOutputNode(outNode);
        }

        public override void OpenWindow(Workflow workflow)
        {
            ShowWindow();
        }

        public override object Execute(Workflow workflow)
        {
            if (workflow.Command == (int)Commands.DoubleClick)
            {
                ShowWindow();
                return null;
            }

            if (!ValidateInputSchemas())
                return null;

            var tempFile = Path.GetTempFileName();
            var result = CompilerService.GenerateCodeAndCompile(tempFile, FullInputSchema, FullOutputSchema, CodeText);
            File.Delete(tempFile);

            try
            {
                Assembly loAssembly = result.CompiledAssembly;
                // Retrieve an obj ref - generic type only
                object loObject =
                       loAssembly.CreateInstance("WinFormCodeCompile.Transform");
                if (loObject == null)
                {
                    MessageService.ShowError("Critical", "Couldn't load class.");
                    return loObject;
                }
                try
                {
                    var type = loObject.GetType();
                    var method = type.GetMethod("UpdateText");
                    var invokationResult = method.Invoke(loObject, null);
                }
                catch (Exception loError)
                {
                    MessageService.ShowError("Critical", loError.Message);
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Run Data menu workflow
        /// </summary>
        public override bool Run(WorkInfo workInfo)
        {
            bool bStatus;
            try
            {
                logger.Info($"{this.DisplayName} Running_workflow");
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Running workflow");

                if (!CanRun(workInfo))
                {
                    return false;
                }

                logger.Trace($"Creating [{DisplayName}] Scripting object");
                workInfo.Log(this.DisplayName, NLog.LogLevel.Info, "Starting Scripting process");

                var inputFiles = new Dictionary<string, string>();
                var outputFiles = new Dictionary<string, string>();

                for (int i = 0; i < InputNodes.Count; i++)
                {
                    inputFiles.Add($"Input_{i}", InputNodes[i].State.DataFilePath);
                }

                for (int i = 0; i < OutputNodes.Count; i++)
                {
                    outputFiles.Add($"Output_{i}", ((InputNode)OutputNodes[i].Connector).State.DataFilePath);
                }

                var tempFile = Path.GetTempFileName();
                var result = CompilerService.GenerateCodeAndCompile(tempFile, FullInputSchema, FullOutputSchema, CodeText, inputFiles, outputFiles);
                File.Delete(tempFile);

                try
                {
                    Assembly loAssembly = result.CompiledAssembly;
                    object loObject = loAssembly.CreateInstance("WinFormCodeCompile.Transform");
                    if (loObject == null)
                    {
                        return false;
                    }
                    try
                    {
                        var type = loObject.GetType();
                        var method = type.GetMethod("UpdateText");
                        var invokationResult = method.Invoke(loObject, null);
                    }
                    catch
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
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

        private bool CanRun(WorkInfo workInfo)
        {
            if (!InputNodes.All(node => node.IsConnected()))
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

            foreach (var node in InputNodes)
            {
                if (node.State == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State is null");
                    return false;
                }

                node.State.DataFilePath = ((OutputNode)node.Connector).State.DataFilePath;
                node.State.Schema = ((OutputNode)node.Connector).State.Schema;
                node.State.DataFile = ((OutputNode)node.Connector).State.DataFile;

                if (node.State.DataFilePath == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as InputNode.State.DataFilePath is null");
                    return false;
                }

                if (!File.Exists(node.State.DataFilePath))
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as {node.State.DataFilePath} does not exist ");
                    return false;
                }

                if (node.State.DataFile == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as DataFile is null");
                    return false;
                }
            }

            for (int i = 0; i < OutputNodes.Count; i++)
            {
                if (OutputNodes[i].Connector.IsConnected() == false)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as OutputNode is not connected");
                    return false;
                }

                if (((InputNode)OutputNodes[i].Connector).State == null)
                {
                    workInfo.Log(this.DisplayName, NLog.LogLevel.Error, $"Halting execution as OutputNode.State is null");
                    return false;
                }

                ((InputNode)OutputNodes[i].Connector).State.Schema = GetOutputSchema(i);
                ((InputNode)OutputNodes[i].Connector).State.DataFile = null;
                var outputFile = DataDir + "Scripting_" + DisplayName + "_" + DateTime.Now.ToString("yyMMddHHmmssfff") + i.ToString() + ".xml";
                ((InputNode)OutputNodes[i].Connector).State.DataFilePath = outputFile;
            }

            return true;
        }

        private void ShowWindow()
        {
            try
            {
                var form = new frmMain(InputNodes, OutputNodes, CodeText, FullOutputSchema);
                BindNodesCountToForm(form);

                form.ShowDialog();
                CodeText = form.GetCode();
                FullInputSchema = form.GetFullInputSchema();
                FullOutputSchema = form.GetFullOutputSchema();
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Critical", ex.Message);
            }
        }

        private bool ValidateInputSchemas()
        {
            if (!InputNodes.All(node => node.IsConnected()))
            {
                MessageService.ShowError("Data Connection Error", "Not all input nodes are connected to scripting module");
                return false;
            }

            foreach (var node in InputNodes)
            {
                string prevModName = node.DisplayName;
                var state = ((OutputNode)node.Connector).State;

                if (state == null)
                {
                    MessageService.ShowError("Data Connection Error", $"In Module {prevModName} passed invalid state object");
                    return false;
                }
                if (state.Schema == null)
                {
                    MessageService.ShowError("Data Connection Error", $"In Module {prevModName} Sate has invalid Schema");
                    return false;
                }
            }

            return true;
        }

        private void BindNodesCountToForm(frmMain form)
        {
            form.OnInputNodesValueChanged += (newValue) =>
            {
                if (newValue > InputNodes.Count)
                {
                    for (int i = newValue - InputNodes.Count; i > 0; i--)
                    {
                        var inNode = new InputNode(this, "Schema", "DataLocation");
                        AddInputNode(inNode);
                    }
                }
                else if (newValue < InputNodes.Count)
                {
                    for (int i = InputNodes.Count - newValue; i > 0; i--)
                    {
                        RemoveNode(InputNodes.Last());
                    }
                }
            };

            form.OnOutputNodesValueChanged += (newValue) =>
            {
                if (newValue > OutputNodes.Count)
                {
                    for (int i = newValue - OutputNodes.Count; i > 0; i--)
                    {
                        var outNode = new OutputNode(this, "Schema", "DataLocation");
                        outNode.State = new ModuleState();
                        AddOutputNode(outNode);
                    }
                }
                else if (newValue < OutputNodes.Count)
                {
                    for (int i = OutputNodes.Count - newValue; i > 0; i--)
                    {
                        RemoveNode(OutputNodes.Last());
                    }
                }
            };
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

                var xmlSerializer = new XmlSerializer(typeof(string));
                CodeText = (string)xmlSerializer.Deserialize(reader);
                xmlSerializer = new XmlSerializer(typeof(Field));
                FullInputSchema = (Field)xmlSerializer.Deserialize(reader);
                FullOutputSchema = (Field)xmlSerializer.Deserialize(reader);
            }
        }

        public override void WriteParameter(XmlWriter writer)
        {
            writer.WriteStartElement("Parameters");

            var xmlSerializer = new XmlSerializer(typeof(string));
            xmlSerializer.Serialize(writer, CodeText);

            xmlSerializer = new XmlSerializer(typeof(Field));
            xmlSerializer.Serialize(writer, FullInputSchema);
            xmlSerializer.Serialize(writer, FullOutputSchema);

            writer.WriteEndElement();
        }

        public Field GetOutputSchema(int index)
        {
            return FullOutputSchema.ChildNodes.SingleOrDefault(node => node.Name == $"Output_{index}");
        }
    }
}
