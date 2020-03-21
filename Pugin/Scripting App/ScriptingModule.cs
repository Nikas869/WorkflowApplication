using JdSuite.Common;
using JdSuite.Common.Logging;
using JdSuite.Common.Module;
using JdSuite.Common.Module.MefMetadata;
using ScriptingApp.Properties;
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;

namespace ScriptingApp
{
    [Export(typeof(IModule)), PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportMetadata("ModuleCategory", IModuleCategory.DATA_MANIPULATION)]
    [ExportMetadata("ModuleType", typeof(ScriptingModule))]
    public class ScriptingModule : BaseModule
    {
        protected override string ModuleName => "Scripting";
        protected override Bitmap Icon => Resources.Icon;
        protected override string ToolTip => "Provides an Scripting support.";

        public ScriptingModule()
        {
            Logger.AppName = ModuleName;
            DisplayName = "Script";

            var inNode = new InputNode(this, "Schema", "DataLocation");
            AddInputNode(inNode);

            var outNode = new OutputNode(this, "Schema", "DataLocation");
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
            }

            return null;
        }

        private void ShowWindow()
        {
            try
            {
                var form = new frmMain(InputNodes, OutputNodes);
                BindNodesCountToForm(form);

                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageService.ShowError("Critical", ex.Message);
            }
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
    }
}
