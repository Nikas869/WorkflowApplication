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
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace ScriptingApp
{
    public partial class frmMain : Form
    {
        #region Data Members Global
        XElement exportRoot;
        XNamespace exportNs;
        bool HasTopRowAdded = false;
        #endregion

        #region Constructor
        public frmMain()
        {
            InitializeComponent();
        }
        #endregion

        #region XMLRead and AddNodes Function
        private void AddNode(XmlNode inXmlNode, TreeGridNode inTreeNode, TreeGridView grid, string Type)
        {
            XmlNode xNode;
            TreeGridNode tNode;
            XmlNodeList nodeList;
            int i;

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    if (nodeList[i].Name != "xs:schema" && nodeList[i].Name != "XML_Converter" && nodeList[i].Name != "Root" && nodeList[i].Name != "Schema")
                    {

                        xNode = inXmlNode.ChildNodes[i];
                        inTreeNode.Nodes.Add(xNode.Attributes[0].Value);
                        inTreeNode.Nodes[i].Cells[0].Value = xNode.Attributes[0].Value;
                        inTreeNode.Nodes[i].Cells[1].Value = xNode.Attributes[1].Value.Replace("xs:", "");//FirstLetterToUpper(xNode.Attributes[1].Value.Replace("xs:", ""));
                        tNode = inTreeNode.Nodes[i];
                        AddNode(xNode, tNode, grid, Type);
                    }
                    else
                    {
                        if (nodeList[i].Name == "xs:schema")
                        {
                            if (!HasTopRowAdded)
                            {
                                xNode = inXmlNode.ChildNodes[i];
                                inTreeNode.Nodes.Add(Type);
                                inTreeNode.Nodes[i].Cells[0].Value = Type;
                                inTreeNode.Nodes[i].Cells[1].Value = "subtree";
                                tNode = inTreeNode.Nodes[i];
                                HasTopRowAdded = true;
                                AddNode(xNode, tNode, grid, Type);
                            }
                        }
                        else
                        {
                            xNode = inXmlNode.ChildNodes[i];
                            tNode = grid.Nodes[0];
                            AddNode(xNode, tNode, grid, Type);
                        }
                    }

                }
            }
            else
            {
                // Here you need to pull the data from the XmlNode based on the
                // type of node, whether attribute values are required, and so forth.
                //inTreeNode.Text = (inXmlNode.OuterXml).Trim();
            }
        }
        #endregion

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

                node.Cells[1].Value = addnode.DataType;
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

        #region Create XML File
        private void CreateDefinition(TreeGridNode node, XElement element)
        {
            XElement el = null;
            if (node.Cells[0].Value.ToString().Contains("Output"))
            {
                el = new XElement(exportNs + "schema", new XAttribute("name", node.Cells[0].Value.ToString()), new XAttribute("type", "xs:" + node.Cells[1].Value.ToString().Replace("/", "")), new XAttribute("Alias", node.Cells[0].Value.ToString()));
                element.Add(el);
            }
            else
            {
                el = new XElement(exportNs + "element", new XAttribute("name", node.Cells[0].Value.ToString()), new XAttribute("type", "xs:" + node.Cells[1].Value.ToString().Replace("/", "")), new XAttribute("Alias", node.Cells[0].Value.ToString()));
                element.Add(el);
            }

            foreach (TreeGridNode subNode in node.Nodes)
            {
                CreateDefinition(subNode, el);
            }
        }
        #endregion

        #region XML to Move Functions
        bool isParentNode = false;
        private void CreateDefinitionMove(TreeGridNode node, XElement element, string Move, string CurrentNodeName, string CurrentNodeType, string MoveBeforNodeName, string MoveBeforNodeType)
        {
            string NodeName = "";
            string NodeType = "";
            // XElement childNode;
            XElement el = null;
            // string ChildNodeName = "";
            bool NodeNotAdd = false;
            if (Move == "Up")
            {
                if (node.Cells[0].Value.ToString() == MoveBeforNodeName && node.Cells[1].Value.ToString() == MoveBeforNodeType)
                {
                    if (!node.HasChildren)
                    {
                        NodeName = CurrentNodeName;
                        NodeType = CurrentNodeType;
                    }
                    else
                    {
                        isParentNode = true;
                        NodeName = MoveBeforNodeName;
                        NodeType = MoveBeforNodeType;

                        //foreach (TreeGridNode subNode in node.Nodes)
                        //{
                        //    ChildNodeName = LastChildofNode(subNode);
                        //}
                    }

                }
                else if (node.Cells[0].Value.ToString() == CurrentNodeName && node.Cells[1].Value.ToString() == CurrentNodeType)
                {
                    if (!isParentNode)
                    {
                        NodeName = MoveBeforNodeName;
                        NodeType = MoveBeforNodeType;
                    }
                    else
                    {
                        NodeNotAdd = true;

                    }
                }
                else
                {
                    NodeName = node.Cells[0].Value.ToString();
                    NodeType = node.Cells[1].Value.ToString();
                }
            }
            else if (Move == "Down")
            {
                if (node.Cells[0].Value.ToString() == MoveBeforNodeName && node.Cells[1].Value.ToString() == MoveBeforNodeType)
                {
                    NodeName = CurrentNodeName;
                    NodeType = CurrentNodeType;
                }
                else if (node.Cells[0].Value.ToString() == CurrentNodeName && node.Cells[1].Value.ToString() == CurrentNodeType)
                {
                    NodeName = MoveBeforNodeName;
                    NodeType = MoveBeforNodeType;
                }
                else
                {
                    NodeName = node.Cells[0].Value.ToString();
                    NodeType = node.Cells[1].Value.ToString();
                }
            }
            if (!NodeNotAdd)
            {
                el = new XElement(exportNs + "element", new XAttribute("name", NodeName), new XAttribute("type", "xs:" + NodeType.Replace("/", "").ToLower()), new XAttribute("Alias", NodeName));
                element.Add(el);
            }
            foreach (TreeGridNode subNode in node.Nodes)
            {
                CreateDefinitionMove(subNode, el, Move, CurrentNodeName, CurrentNodeType, MoveBeforNodeName, MoveBeforNodeType);
            }
        }

        private XDocument TreeViewConvertIntoXMLWithMoveNodes(TreeGridView tgv, string Move, string CurrentNodeName, string CurrentNodeType, string MoveBeforNodeName, string MoveBeforNodeType)
        {
            XDocument exportDoc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
            exportNs = "http://www.w3.org/2001/XMLSchema";
            exportRoot = new XElement(exportNs + "schema", new XAttribute("attributeFormDefault", "unqualified"), new XAttribute("elementFormDefault", "qualified"), new XAttribute(XNamespace.Xmlns + "xs", exportNs));
            foreach (TreeGridNode node in tgv.Nodes[0].Nodes)
            {
                CreateDefinitionMove(node, exportRoot, Move, CurrentNodeName, CurrentNodeType, MoveBeforNodeName, MoveBeforNodeType);
            }
            exportDoc.Add(exportRoot);
            isParentNode = false;

            return exportDoc;
        }


        private string LastChildofNode(TreeGridNode node)
        {
            string NodeName = "";
            NodeName = node.Cells[0].Value.ToString();
            foreach (TreeGridNode subNode in node.Nodes)
            {
                LastChildofNode(subNode);
            }
            return NodeName;
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
            // Here you pass schema from previous module
            Field inputSchema = GetInputSchema();
            FillTreeViewsUsingSchema(inputSchema);
            SetSampleCode();

            // How to get modified schema
            var outputSchema = GenerateSchema(grInput);
        }

        private void FillTreeViewsUsingSchema(Field inputSchema)
        {
            // Bind Input TreeView
            SetTreeFromSchema(grInput, inputSchema, "Input_0");

            // Bind OutPut Treeview
            SetTreeFromSchema(grOutput, inputSchema, "Output_0");
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
            field.DataType = treeGridNode.Cells[1].Value?.ToString();
            field.Type = treeGridNode.Cells[2].Value?.ToString();
            field.Optionality = treeGridNode.Cells[3].Value?.ToString();
            field.Change = treeGridNode.Cells[4].Value?.ToString();
            field.Alias = treeGridNode.Cells[5].Value?.ToString();

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
        private void SetTreeFromSchema(TreeGridView gridNodes, Field schema, string rootChildName)
        {
            gridNodes.Nodes.Clear();

            var treeRootNode = gridNodes.Nodes.Add("Root");
            var rootChildNode = treeRootNode.Nodes.Add(new[] { rootChildName, "Array" });

            CreateTreeChildNodes(schema, rootChildNode);
            treeRootNode.Expand();
            ExpandChildren(treeRootNode);
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
            //2-Type [Element, Attribute, PCData]
            //3-Optionality [One, Zero or one, Zero or more, One or more]
            //4-Change [None, Ignore, Flatten]
            //5-XML Name

            TreeGridNode treeNode = parentTreeNode.Nodes.Add(name);
            treeNode.Cells[0].Value = schemaNode.Name;
            treeNode.Cells[1].Value = schemaNode.DataType;
            treeNode.Cells[2].Value = schemaNode.Type;
            treeNode.Cells[3].Value = schemaNode.Optionality;
            treeNode.Cells[4].Value = schemaNode.Change;
            treeNode.Cells[5].Value = schemaNode.Alias;

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
                SetTreeFromSchema(grOutput, newSchema.ChildNodes[0].ChildNodes[0], "Output_0");
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
                SetTreeFromSchema(grOutput, newSchema.ChildNodes[0].ChildNodes[0], "Output_0");
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
                object[] loCodeParms = new object[1];
                loCodeParms[0] = "West Wind Technologies";
                try
                {
                    object loResult = loObject.GetType().InvokeMember("UpdateText", BindingFlags.InvokeMethod, null, loObject, null);
                }
                catch (Exception loError)
                {
                    MessageBox.Show(loError.Message, "Compiler Demo");
                }
            }
            catch { }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var data = WorkflowFileFactory.LoadFromXmlFile(@"D:\Projects\WorkflowApplication\SwissProt.xml");
            var inputSchema = GenerateSchema(grInput);
            var outputSchema = GenerateSchema(grOutput);
            var result = CompilerService.GenerateCodeAndCompile(inputSchema, outputSchema, txtScript.Text, data.RootNode);

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
                object[] loCodeParms = new object[1];
                loCodeParms[0] = "West Wind Technologies";
                try
                {
                    object loResult = loObject.GetType().InvokeMember("UpdateText", BindingFlags.InvokeMethod, null, loObject, null);
                }
                catch (Exception loError)
                {
                    MessageBox.Show(loError.Message, "Compiler Demo");
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

        private Field GetInputSchema()
        {
            var filePath = ConfigurationManager.AppSettings["ReadFilePath"];

            return Field.Parse(filePath);
        }
        #endregion
    }
}
