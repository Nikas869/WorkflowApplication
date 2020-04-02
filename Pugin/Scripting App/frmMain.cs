using AdvancedDataGridView;
using JdSuite.Common.Module;
using ScriptingApp.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ScriptingApp
{
    public partial class frmMain : Form
    {
        public IList<BaseInputNode> InputNodes { get; set; }
        public IList<Field> InputSchemas => InputNodes.Select(node => GetSchemaOrNull(node)).ToList();

        public IList<BaseOutputNode> OutputNodes { get; set; }
        public IList<Field> OutputSchemas => OutputNodes.Select(node => node.State?.Schema).ToList();

        public Field OutputSchema { get; set; }

        public delegate void NodesCountHandler(int newValue);
        public event NodesCountHandler OnInputNodesValueChanged;
        public event NodesCountHandler OnOutputNodesValueChanged;

        public frmMain(
            IList<BaseInputNode> inputNodes,
            IList<BaseOutputNode> outputNodes,
            string code = "",
            Field outputSchema = null)
        {
            InitializeComponent();

            tabControl1.SelectedTab = tabScript;
            txtdatainputcount.Value = inputNodes.Count;
            txtdataoutputcount.Value = outputNodes.Count;
            txtScript.Text = code;

            InputNodes = inputNodes;
            OutputNodes = outputNodes;

            OutputSchema = outputSchema;
        }

        public string GetCode()
        {
            return txtScript.Text;
        }

        public Field GetFullInputSchema()
        {
            return GenerateSchema(grInput);
        }

        public Field GetFullOutputSchema()
        {
            return GenerateSchema(grOutput);
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

        private void ExpandChildren(TreeGridNode parent)
        {
            parent.Expand();
            foreach (TreeGridNode node in parent.Nodes)
            {
                ExpandChildren(node);
            }
        }

        #region Events
        // Form Events
        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeTreeViewsWithRoot();
            FillTreeViewsUsingSchemas(grInput, InputSchemas, "Input");

            if (OutputSchema != null)
            {
                // Root alredy exist, so we fill only his children
                grOutput.Nodes.Clear();
                var root = grOutput.Nodes.Add(OutputSchema.Name);
                foreach (var child in OutputSchema.ChildNodes)
                {
                    CreateTreeChildNodes(child, root);
                }
                ExpandChildren(grOutput.Nodes[0]);
            }
            else
            {
                FillTreeViewsUsingSchemas(grOutput, OutputSchemas, "Output");
            }

            SetSampleCode();
        }

        private void InitializeTreeViewsWithRoot()
        {
            grInput.Nodes.Clear();
            grInput.Nodes.Add("Root");

            grOutput.Nodes.Clear();
            grOutput.Nodes.Add("Root");
        }

        private void FillTreeViewsUsingSchemas(TreeGridView tree, IEnumerable<Field> schemas, string namePrefix)
        {
            var root = tree.Nodes[0];

            int index = 0;
            foreach (var schema in schemas)
            {
                SetTreeFromSchema(root, schema, $"{namePrefix}_{index}");

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
            var rootChildNode = gridNode.Nodes.Add(new[] { rootChildName, rootChildName, "String" });

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

            var tempFile = Path.GetTempFileName();
            var result = CompilerService.GenerateCodeAndCompile(tempFile, inputSchema, outputSchema, txtScript.Text);
            File.Delete(tempFile);

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
        #endregion

        private void txtdatainputcount_ValueChanged(object sender, EventArgs e)
        {
            if (InputNodes == null)
            {
                return;
            }

            var nodesBefore = InputNodes.Count;

            OnInputNodesValueChanged?.Invoke((int)txtdatainputcount.Value);

            if (nodesBefore < (int)txtdatainputcount.Value)
            {
                grInput.Nodes[0].Nodes.Add(new[] { $"Input_{InputNodes.Count - 1}", $"Input_{InputNodes.Count - 1}", "String" });
            }
            else if (nodesBefore > (int)txtdatainputcount.Value)
            {
                grInput.Nodes[0].Nodes.RemoveAt(InputNodes.Count);
            }
        }

        private void txtdataoutputcount_ValueChanged(object sender, EventArgs e)
        {
            if (OutputNodes == null)
            {
                return;
            }

            var nodesBefore = OutputNodes.Count;

            OnOutputNodesValueChanged?.Invoke((int)txtdataoutputcount.Value);

            if (nodesBefore < (int)txtdataoutputcount.Value)
            {
                grOutput.Nodes[0].Nodes.Add(new[] { $"Output_{OutputNodes.Count - 1}", $"Output_{OutputNodes.Count - 1}", "String" });
            }
            else if (nodesBefore > (int)txtdataoutputcount.Value)
            {
                grOutput.Nodes[0].Nodes.RemoveAt(OutputNodes.Count);
            }
        }

        private Field GetSchemaOrNull(BaseInputNode node)
        {
            if (node == null || node.Connector == null)
            {
                return null;
            }

            return ((OutputNode)node.Connector).State?.Schema;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
