using AdvancedDataGridView;
using JdSuite.Common.FileProcessing;
using JdSuite.Common.Module;
using ScriptingApp.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ScriptingApp
{
    public partial class frmMain : Form
    {
        public IList<BaseInputNode> InputNodes { get; set; }
        public IList<Field> InputSchemas => InputNodes.Select(node => GetSchemaOrNull(node)).ToList();

        public IList<BaseOutputNode> OutputNodes { get; set; }
        public IList<Field> OutputSchemas => OutputNodes.Select(node => node.State?.Schema).ToList();

        public delegate void NodesCountHandler(int newValue);
        public event NodesCountHandler OnInputNodesValueChanged;
        public event NodesCountHandler OnOutputNodesValueChanged;

        public frmMain(IList<BaseInputNode> inputNodes, IList<BaseOutputNode> outputNodes)
        {
            InitializeComponent();

            tabControl1.SelectedTab = tabScript;
            txtdatainputcount.Value = inputNodes.Count;
            txtdataoutputcount.Value = outputNodes.Count;

            InputNodes = inputNodes;
            OutputNodes = outputNodes;
        }

        #region Node Function 
        private void AddNewNode(bool isParent)
        {
            frmAddNode addnode = new frmAddNode();
            if (addnode.ShowDialog() == DialogResult.OK)
            {
                TreeGridNode node;
                if (isParent)
                {
                    node = grOutput.CurrentNode.Parent.Nodes.Add(addnode.NodeName);
                    AddChildren(node, grOutput.CurrentNode);
                    MoveNode(node, grOutput.CurrentNode);
                }
                else
                    node = grOutput.CurrentNode.Nodes.Add(addnode.NodeName);

                node.Cells[1].Value = addnode.NodeName;
                node.Cells[2].Value = addnode.DataType;
            }
        }

        private void AddChildren(TreeGridNode parent, TreeGridNode child)
        {
            //TreeGridNode node = parent.Nodes.Add("Hello");
            TreeGridNode node = parent.Nodes.Add(child.Cells[0].Value.ToString());
            node.Cells[1].Value = child.Cells[1].Value;
            foreach (TreeGridNode sub in child.Nodes)
            {
                AddChildren(node, sub);
            }
        }

        private void MoveNode(TreeGridNode node, TreeGridNode prevNode)
        {
            List<TreeGridNode> allNode = new List<TreeGridNode>();
            foreach (TreeGridNode sub in node.Parent.Nodes)
            {
                allNode.Add(sub);
            }
            List<TreeGridNode> removed = new List<TreeGridNode>();
            try
            {
                bool startDelete = false;
                foreach (TreeGridNode sub in allNode)
                {
                    if (sub == prevNode)
                        startDelete = true;

                    if (sub == node)
                        startDelete = false;

                    if (startDelete)
                    {
                        if (sub != prevNode)
                            removed.Add(sub);
                        node.Parent.Nodes.Remove(sub);
                    }
                }

                foreach (TreeGridNode sub in removed)
                {
                    node.Parent.Nodes.Add(sub);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        #endregion

        #region ExpandNodes
        private void ExpandChildren(TreeGridNode parent)
        {
            parent.Expand();
            foreach (TreeGridNode node in parent.Nodes)
            {
                ExpandChildren(node);
            }
        }

        #endregion

        #region Events
        // Form Events
        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeTreeViewsWithRoot();
            FillTreeViewsUsingSchemas(InputSchemas, OutputSchemas);
            SetSampleCode();

            // How to get modified schema
            //var outputSchema = GenerateSchema(grInput);
        }

        private void InitializeTreeViewsWithRoot()
        {
            grInput.Nodes.Clear();
            grInput.Nodes.Add("Root");

            grOutput.Nodes.Clear();
            grOutput.Nodes.Add("Root");
        }

        private void FillTreeViewsUsingSchemas(IEnumerable<Field> inputSchemas, IEnumerable<Field> outputSchemas)
        {
            var inputRoot = grInput.Nodes[0];
            var outputRoot = grOutput.Nodes[0];

            int index = 0;
            foreach (var schema in inputSchemas)
            {
                // Bind Input TreeView
                SetTreeFromSchema(inputRoot, schema, $"Input_{index}");

                index++;
            }
            index = 0;
            foreach (var schema in outputSchemas)
            {
                // Bind OutPut Treeview
                SetTreeFromSchema(outputRoot, schema, $"Output_{index}");

                index++;
            }
        }

        /// <summary>
        /// Creates schema from visual tree
        /// </summary>
        /// <returns></returns>
        private Field GenerateSchema(TreeGridView gridView)
        {
            Field _schema = null;
            foreach (TreeGridNode node in gridView.Nodes)
            {
                CreateDefinition(node, ref _schema);
            }
            Field.SetParent(_schema);
            return _schema;
        }

        /// <summary>
        /// Creates schema nodes from tree nodes
        /// </summary>
        /// <param name="treeGridNode"></param>
        /// <param name="element"></param>
        private void CreateDefinition(TreeGridNode treeGridNode, ref Field element)
        {
            Field field = new Field();
            field.Name = treeGridNode.Cells[0].Value.ToString();
            field.Alias = treeGridNode.Cells[0].Value?.ToString();
            field.DataType = treeGridNode.Cells[2].Value?.ToString();
            field.Type = treeGridNode.Cells[3].Value?.ToString();
            field.Optionality = treeGridNode.Cells[4].Value?.ToString();
            field.Change = treeGridNode.Cells[5].Value?.ToString();

            if (element == null)
                element = field;
            else
                element.ChildNodes.Add(field);

            foreach (TreeGridNode subNode in treeGridNode.Nodes)
            {
                CreateDefinition(subNode, ref field);
            }
        }

        /// <summary>
        /// Loads visual tree nodes from Schema node
        /// </summary>
        /// <param name="schema"></param>
        private void SetTreeFromSchema(TreeGridNode gridNode, Field schema, string rootChildName)
        {
            var rootChildNode = gridNode.Nodes.Add(new[] { rootChildName, rootChildName, "Array" });

            if (schema != null)
            {
                CreateTreeChildNodes(schema, rootChildNode);
            }

            gridNode.Expand();
            ExpandChildren(gridNode);
        }

        /// <summary>
        /// Creates tree nodes from schema nodes
        /// </summary>
        /// <param name="schemaNode"></param>
        /// <param name="parentTreeNode"></param>
        private void CreateTreeChildNodes(Field schemaNode, TreeGridNode parentTreeNode)
        {
            string name = schemaNode.Name;

            //0-Name
            //1-Data Type
            //2-Xml Name
            //3-Type [Element, Attribute, PCData]
            //4-Optionality [One, Zero or one, Zero or more, One or more]
            //5-Change [None, Ignore, Flatten]

            TreeGridNode treeNode = parentTreeNode.Nodes.Add(name);
            treeNode.Cells[0].Value = schemaNode.Alias;
            treeNode.Cells[1].Value = schemaNode.Alias;
            treeNode.Cells[2].Value = schemaNode.DataType;
            treeNode.Cells[3].Value = schemaNode.Type;
            treeNode.Cells[4].Value = schemaNode.Optionality;
            treeNode.Cells[5].Value = schemaNode.Change;

            if (schemaNode.ChildNodes.Count > 0)
            {
                foreach (var childSchemaNode in schemaNode.ChildNodes)
                {
                    CreateTreeChildNodes(childSchemaNode, treeNode);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddNewNode(false);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this node and all its children?", "XML Converter", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                grOutput.CurrentNode.Parent.Nodes.Remove(grOutput.CurrentNode);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string filename = DateTime.Now.ToString("ddMMyyyyhhmmss") + ".xml";
            var outputSchema = GenerateSchema(grOutput);
            outputSchema.Save(filename);

            MessageBox.Show("Schema file generated successfully!", "XML Converter", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            TreeGridNode currentNode = grOutput.CurrentNode;
            int NextRowIndex = currentNode.Index + 1;
            int TotalRowCount = currentNode.Parent.Nodes.Count;

            if (NextRowIndex != TotalRowCount)
            {
                var selectedNode = grOutput.CurrentNode;
                var index = selectedNode.Index;
                var parent = selectedNode.Parent;

                parent.Nodes.Remove(selectedNode);
                parent.Nodes.Insert(index + 1, selectedNode);
                var newSchema = GenerateSchema(grOutput);
                SetTreeFromSchema(grOutput.Nodes[0], newSchema.ChildNodes[0].ChildNodes[0], "Output_0");
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            TreeGridNode currentNode = grOutput.CurrentNode;
            int NextRowIndex = currentNode.Index - 1;

            if (NextRowIndex != -1)
            {
                var selectedNode = grOutput.CurrentNode;
                var index = selectedNode.Index;
                var parent = selectedNode.Parent;

                parent.Nodes.Remove(selectedNode);
                parent.Nodes.Insert(index - 1, selectedNode);
                var newSchema = GenerateSchema(grOutput);
                SetTreeFromSchema(grOutput.Nodes[0], newSchema.ChildNodes[0].ChildNodes[0], "Output_0");
            }
        }

        private void btnFindError_Click(object sender, EventArgs e)
        {
            txtCompileStatus.Text = "Compiling...";
            var inputSchema = GenerateSchema(grInput);
            var outputSchema = GenerateSchema(grOutput);
            var result = CompilerService.GenerateCodeAndCompile(inputSchema, outputSchema, txtScript.Text);
            txtCompileStatus.Text = string.Empty;
            // Check for errors  
            if (result.Errors.Count > 0)
            {
                foreach (CompilerError CompErr in result.Errors)
                {
                    //lblCompileStatus.ForeColor = Color.Red;
                    txtCompileStatus.ForeColor = Color.Red;
                    txtCompileStatus.Text = txtCompileStatus.Text +
                                //"Line number " + CompErr.Line +
                                " Error Number: " + CompErr.ErrorNumber +
                                ", '" + CompErr.ErrorText + ";" +
                                Environment.NewLine;
                }
                return;
            }
            else
            {
                //Successful Compile
                //lblCompileStatus.ForeColor = Color.Green;
                txtCompileStatus.ForeColor = Color.Green;
                txtCompileStatus.Text = "Success!";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();

            var data = WorkflowFileFactory.LoadFromXmlFile(GetInputXml());
            var inputSchema = GenerateSchema(grInput);
            var outputSchema = GenerateSchema(grOutput);
            var result = CompilerService.GenerateCodeAndCompile(inputSchema, outputSchema, txtScript.Text, data);

            try
            {
                Assembly loAssembly = result.CompiledAssembly;
                // Retrieve an obj ref - generic type only
                object loObject =
                       loAssembly.CreateInstance("WinFormCodeCompile.Transform");
                if (loObject == null)
                {
                    MessageBox.Show("Couldn't load class.");
                    return;
                }
                try
                {
                    var type = loObject.GetType();
                    var method = type.GetMethod("UpdateText");
                    var invokationResult = method.Invoke(loObject, null);
                }
                catch (Exception loError)
                {
                    MessageBox.Show(loError.Message, "Compiler");
                }
            }
            catch { }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtSample.SelectedText);
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            txtScript.Text += Clipboard.GetText();
        }

        private void SetSampleCode()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "SampleCode.txt";

            if (File.Exists(filePath))
            {
                txtSample.Text = File.ReadAllText(filePath);
            }
            else
            {
                txtSample.Text = "Please, provide sample code in SampleCode.txt file";
            }
        }

        private string GetInputXml()
        {
            return ConfigurationManager.AppSettings["InputFilePath"];
        }
        #endregion

        private void txtdatainputcount_ValueChanged(object sender, EventArgs e)
        {
            OnInputNodesValueChanged?.Invoke((int)txtdatainputcount.Value);
        }

        private void txtdataoutputcount_ValueChanged(object sender, EventArgs e)
        {
            OnOutputNodesValueChanged?.Invoke((int)txtdataoutputcount.Value);
        }
        private Field GetSchemaOrNull(BaseInputNode node)
        {
            if (node == null || node.Connector == null)
            {
                return null;
            }

            return ((OutputNode)node.Connector).State?.Schema;
        }
    }
}
