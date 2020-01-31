using AdvancedDataGridView;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using JdSuite.Common.Module;
using System.Text;
using System.Xml.XPath;

namespace XMLConverter
{
    public partial class frmMain : Form
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger("xmlModulefrmMain");

        private List<EncodingInfo> encodings = Encoding.GetEncodings().OrderBy(x => x.DisplayName).ToList();
        private List<string> dataTypes = new List<string>() { "Array", "String", "Int16", "Int32", "Int64", "Boolean", "Date/Time", "Double", "Single" };

        public bool isCancel;
        private Dictionary<string, string> encodingDict;
        public ModuleState State { get; set; }

        public string LastFileName = "";

        public frmMain()
        {
            InitializeComponent();
             
            InitVariables(null);
        }

        public frmMain(ModuleState state)
        {
            this.State = state;

            InitializeComponent();
             

            InitVariables(state);
            this.gridNodes.DataError += GridNodes_DataError;
            this.gridNodes.CellValueChanged += GridNodes_CellValueChanged;
        }

        private void GridNodes_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                string cellValue = (string)this.gridNodes.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                if (cellValue == "Zero or more" || cellValue == "One or more")
                {
                    
                    this.gridNodes.Rows[e.RowIndex].Cells[5].Value = "Array";
                }
            }
        }

        private void GridNodes_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void InitVariables(ModuleState state)
        {
            if (state == null)
            {
                state = new ModuleState();
            }
            this.State = state;
            var height = Screen.PrimaryScreen.WorkingArea.Height - 20;
            var width = Screen.PrimaryScreen.WorkingArea.Width;
            this.Height = height;

            if (this.Height < 700)
            {
                this.Height = height - 60;

            }

            int x = (width - this.Width) / 2;
            int y = (height - this.Height) / 2;
            if (y < 10)
                y = 10;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(x, y);



            isCancel = false;
            encodingDict = new Dictionary<string, string>();
            encodingDict.Add("Unicode", "utf-16");
            encodingDict.Add("Unicode (Big endian)", "unicodeFFFE");
            encodingDict.Add("Western European (Windows)", "Windows-1252");
            encodingDict.Add("Unicode (UTF-32)", "utf-32");
            encodingDict.Add("Unicode (UTF-32 Big endian)", "utf-32BE");
            encodingDict.Add("US-ASCII", "us-ascii");
            encodingDict.Add("Western European (ISO)", "iso-8859-1");
            encodingDict.Add("Unicode (UTF-7)", "utf-7");
            encodingDict.Add("Unicode (UTF-8)", "utf-8");


            this.inputFileDialog.Filter = "XML files (*.xml)|*.xml|JSON files (*.json, *.js)|*.json;*.js|All files (*.*)|*.*";
            //on “.JSON”, “.js”and “*.*” files

            //comboTextEncoding.DataSource = encodings;
            // comboTextEncoding.DisplayMember = "DisplayName";
            //comboTextEncoding.ValueMember=encodings[0].


            foreach (var enc in encodings)
            {
                if (!encodingDict.ContainsKey(enc.DisplayName))
                    encodingDict.Add(enc.DisplayName, enc.Name);
            }

            foreach (String field in encodingDict.Keys)
            {
                comboTextEncoding.Items.Add(field);
            }

            if (!string.IsNullOrEmpty(state.TextEncoding))
            {
                SetTextEncoding(state.TextEncoding);
            }

            if (State.MatchState(ModuleState.VerifySchemaTagOrderVar, "True"))
            {
                chkCheckTagOrder.Checked = true;
            }

            if (this.State.DataFilePath.Count() > 0)
            {
                SetInputFilePath(this.State.DataFilePath);
            }

            if (this.State.Schema != null)
            {
                SetTreeFromSchema(this.State.Schema);
            }
        }

        private void SetInputFilePath(string path)
        {
            txtInputFile.Text = path;
            this.State.DataFilePath = path;
        }

        private void OpeningChildren(XmlNodeList parent, TreeGridNode parentN)
        {
            try
            {
                int count = 0;
                foreach (XmlNode child in parent)
                {
                    if (child.Name == "xml" || child.Name == "Root")
                        OpeningChildren(child.ChildNodes, parentN);
                    else
                    {
                        TreeGridNode node = parentN.Nodes.Add(child.Name);
                        if (child.Name != "Root")
                        {
                            parentN.Nodes[count].Cells[1].Value = child.Attributes[0].Value;
                            parentN.Nodes[count].Cells[2].Value = child.Attributes[1].Value;
                            parentN.Nodes[count].Cells[3].Value = child.Attributes[2].Value;
                            parentN.Nodes[count].Cells[4].Value = child.Attributes[3].Value;
                            parentN.Nodes[count].Cells[5].Value = child.Attributes[4].Value;
                            count++;
                        }

                        if (child.HasChildNodes)
                            OpeningChildren(child.ChildNodes, parentN.Nodes[parentN.Nodes.IndexOf(node)]);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                SetInputFilePath("");
                this.State.Schema = null;
                MessageBox.Show("Could not load previous session!", "XML Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                gridNodes.Nodes.Clear();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (this.State != null)
            {
                if (!string.IsNullOrEmpty(this.State.TextEncoding))
                {
                    SetTextEncoding(this.State.TextEncoding);
                }
                else
                {
                    comboTextEncoding.SelectedIndex = 0;
                }



                if (this.State.Schema != null)
                {
                    ExpandTree();
                }
            }
            this.BringToFront();

        }

        private void btnInputBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result;
            logger.Info("Opening OpenFile dialog");

            // OpenFileDialog dlg = new OpenFileDialog();
            // result= dlg.ShowDialog(this);

            result = inputFileDialog.ShowDialog(this);

            if (result == DialogResult.OK)
            {
                if (System.IO.Path.GetExtension(inputFileDialog.FileName).ToLower() == ".xml")
                {

                    SetInputFilePath(inputFileDialog.FileName);


                    //SetTreeFromInputFile();//old code
                }
                else
                {
                    MessageBox.Show("Selected file is not an XML file", "XML Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Loads visual tree nodes from Schema node
        /// </summary>
        /// <param name="schema"></param>
        private void SetTreeFromSchema(Field schema)
        {
            gridNodes.Nodes.Clear();

            logger.Trace("Loading_tree_nodes from schema");

            TreeGridNode treeRootNode = gridNodes.Nodes.Add("Root");
            treeRootNode.Cells[1].ReadOnly = true;
            treeRootNode.Cells[2].ReadOnly = true;
            treeRootNode.Cells[3].ReadOnly = true;
            treeRootNode.Cells[4].ReadOnly = true;
            treeRootNode.Cells[5].ReadOnly = true;

            CreateTreeChildNodes(schema, treeRootNode);
            treeRootNode.Expand();
            ExpandChildren(treeRootNode);
        }




        /// <summary>
        /// Loads tree nodes from Xml data Input File
        /// </summary>
        private void SetTreeFromInputFile()
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(txtInputFile.Text);

            gridNodes.Nodes.Clear();

            TreeGridNode treeRootNode = gridNodes.Nodes.Add("Root");//Name
            treeRootNode.Cells[1].ReadOnly = true;                  //Type [Element, Attribute, PCData]
            treeRootNode.Cells[2].ReadOnly = true;                  //Optionality [One, Zero or one, Zero or more, One or more]
            treeRootNode.Cells[3].ReadOnly = true;                  //Change [None, Ignore, Flatten]
            treeRootNode.Cells[4].ReadOnly = true;                  //XML Name
            treeRootNode.Cells[5].ReadOnly = true;                  //Data Type

            treeRootNode.Tag = "";

            ExtractSchema(doc.ChildNodes, treeRootNode);

            var treeParentNode = treeRootNode.Nodes[0];

            List<TreeGridNode> gridNodeList = new List<TreeGridNode>();

            GetGridNodeList(treeParentNode, gridNodeList);

            foreach (var node in gridNodeList)
            {
                logger.Info("Node_Path {0}", (string)node.Tag);
            }

            XDocument xdoc = XDocument.Load(txtInputFile.Text);

            SetNodeAttributes(xdoc, gridNodeList);

            foreach (var node in gridNodeList)
            {
                logger.Info("Node_Path [{0}] Type [{1}] Optionality [{2}]", (string)node.Tag, node.Cells[1].Value, node.Cells[2].Value);
            }


            treeRootNode.Expand();
            ExpandChildren(treeRootNode);
        }

        /// <summary>
        /// Sets optionality and node type like Array or Element
        /// </summary>
        /// <param name="xdoc"></param>
        /// <param name="nodeList"></param>
        private void SetNodeAttributes(XDocument xdoc, List<TreeGridNode> nodeList)
        {
            var pathList = nodeList.Select(x => x.Tag.ToString()).ToList();

            var longestPath = pathList.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);

            var parts = longestPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var treeNode in nodeList)
            {
                try
                {


                    string fullPath = (string)treeNode.Tag;
                    var lastIndex = fullPath.LastIndexOf('/');

                    var nodeParts = fullPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string nodeParent = "";
                    if (nodeParts.Length >= 2)
                    {
                        nodeParent = nodeParts[nodeParts.Length - 2];
                    }

                    var nodeName = (string)treeNode.Cells[0].Value;


                    SetOptionality(xdoc, treeNode, nodeParent, nodeName, fullPath);



                    //0-Name
                    //1-Type [Element, Attribute, PCData]
                    //2-Optionality [One, Zero or one, Zero or more, One or more]
                    //3-Change [None, Ignore, Flatten]
                    //4-XML Name
                    //5-Data Type
                                       
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Optionality_setting_error for node {0}", treeNode.Tag);
                }
            }
        }

        /// <summary>
        /// Makes list of TreeGridNode from hiearchical treenodes
        /// </summary>
        /// <param name="treeNode"></param>
        /// <param name="nodeList"></param>
        private void GetGridNodeList(TreeGridNode treeNode, List<TreeGridNode> nodeList)
        {
            nodeList.Add(treeNode);

            foreach (var childNode in treeNode.Nodes)
            {
                GetGridNodeList(childNode, nodeList);
            }
        }


        private void SetOptionality(XDocument xdoc, TreeGridNode treeNode, string ParentName, string NodeName, string NodeFullPath)
        {
            logger.Trace("Getting_Optionality_For XmlNode [{0}]", NodeFullPath);

            treeNode.Cells[1].Value = "Element";

            string optionality = JdSuite.Common.Optionality.One;

            if (ParentName == "")
            {
                treeNode.Cells[2].Value = optionality;
                return;
            }


            bool[] options = new bool[] { false, false, false };

            foreach (var parentNode in xdoc.Descendants(ParentName))//xdoc.Elements
            {
                var nodeCount = parentNode.Elements(NodeName).Count();

                if (nodeCount == 0)
                {
                    options[0] = true; //Zero or one || Zero or more
                }
                else if (nodeCount == 1)
                {
                    options[1] = true;  //One || Zero or One
                }
                else if (nodeCount > 1)
                {
                    options[2] = true;

                    //One or more || Zero or more
                }


                if (options[0] && options[1] && options[2])
                {
                    break;
                }
            }

            optionality = JdSuite.Common.Optionality.Zero_OR_One;

            if (options[0] && options[2])
            {
                optionality = JdSuite.Common.Optionality.Zero_OR_More;
            }
            else if (options[0] && options[1] && !options[2])
            {
                optionality = JdSuite.Common.Optionality.Zero_OR_One;
            }
            else if (!options[0] && options[1] && options[2])
            {
                optionality = JdSuite.Common.Optionality.One_Or_More;
            }
            else if (!options[0] && options[1] && !options[2])
            {
                optionality = JdSuite.Common.Optionality.One;
            }
            else if (!options[0] && !options[1] && options[2])
            {
                optionality = JdSuite.Common.Optionality.One_Or_More;
            }

            if (options[2])
            {
                //If a node is repeated in its parent then it is an array
                treeNode.Cells[5].Value = "Array";
            }

            logger.Trace("Node_Optionality {0}={1}", NodeFullPath, optionality);

            treeNode.Cells[2].Value = optionality;

        }

        /// <summary>
        /// Extracts schema from Xml Nodes
        /// </summary>
        /// <param name="xmlNodeList"></param>
        /// <param name="parentTreeNode"></param>
        private void ExtractSchema(XmlNodeList xmlNodeList, TreeGridNode parentTreeNode)
        {


            foreach (XmlNode childXMLNode in xmlNodeList)
            {
                if (childXMLNode.NodeType != XmlNodeType.Element)
                    continue;


                TreeGridNode childTreeNode = parentTreeNode.Nodes.FirstOrDefault(trnode => (string)trnode.Cells[0].Value == childXMLNode.Name);

                if (childXMLNode.Name != "#text" && childXMLNode.Name != "xml" && childTreeNode == null)
                {
                    childTreeNode = parentTreeNode.Nodes.Add(childXMLNode.Name);//TagName
                    childTreeNode.Cells[1].Value = "Element";//Tag Type
                    childTreeNode.Cells[2].Value = "One";//Optionality
                    childTreeNode.Cells[3].Value = "Flatten";//Change
                    childTreeNode.Cells[4].Value = childXMLNode.Name;//XML Name
                    childTreeNode.Cells[5].Value = "String";//Data Type
                    childTreeNode.Tag = (string)parentTreeNode.Tag + "/" + childXMLNode.Name;

                    foreach (XmlAttribute attr in childXMLNode.Attributes)
                    {
                        TreeGridNode attrTreeNode = childTreeNode.Nodes.Add(attr.Name);
                        attrTreeNode.Cells[1].Value = "Attribute";
                        attrTreeNode.Cells[2].Value = "One";
                        attrTreeNode.Cells[3].Value = "Flatten";
                        attrTreeNode.Cells[4].Value = attr.Name;
                        attrTreeNode.Cells[5].Value = "String";
                        attrTreeNode.Tag = childTreeNode.Tag.ToString() + "[" + attr.Name + "]";
                    }
                }

                if (childXMLNode.HasChildNodes)
                {
                    if (childXMLNode.ChildNodes.OfType<XmlNode>().Any(x => x.NodeType == XmlNodeType.Element))
                    {
                        ExtractSchema(childXMLNode.ChildNodes, childTreeNode);
                    }
                }
            }
        }



        private void ExportFile(string filename)
        {
            this.State.Schema = GenerateSchema();
            this.State.Schema.Save(filename);
            MessageBox.Show("Schema file saved successfully!", "XML Converter", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportXML_Click(object sender, EventArgs e)
        {
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ExportFile(saveFileDialog.FileName);
            }
        }

        private void ExpandChildren(TreeGridNode parent)
        {
            parent.Expand();
            foreach (TreeGridNode node in parent.Nodes)
            {
                ExpandChildren(node);
            }
        }

        private void btnGenReadXML_Click(object sender, EventArgs e)
        {
            logger.Trace("Entered");

            SetTreeFromInputFile();

            /*******Below from here is old code*****/
            //ExpandTree();
        }

        private void ExpandTree()
        {
            ExpandChildren(gridNodes.Nodes[0]);
        }

         

        public void SetTextEncoding(string encoding)
        {
            for (int i = 0; i < comboTextEncoding.Items.Count; i++)
            {
                string text = comboTextEncoding.Items[i] as string;
                if (text == encoding)
                {
                    comboTextEncoding.SelectedIndex = i;
                    break;
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isCancel)
            {
                logger.Info("Setting_schema_on_module_output_state");
                SetStateSchema();
            }
            else
            {
                logger.Warn("Due_to_cancel_not_updating_schema_on_module_output_state");
            }
        }

        private void SetStateSchema()
        {
            if (gridNodes.Nodes.Count <= 0)
            {
                return;
            }
            logger.Trace("Setting_schema_on_state");
            this.State.Schema = GenerateSchema();
            this.State.TextEncoding = comboTextEncoding.Text;
            this.State.SetStateVar(ModuleState.VerifySchemaTagOrderVar, chkCheckTagOrder.Checked.ToString());
        }

        private void gridNodes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                                      Color.Black, 0, ButtonBorderStyle.Inset,
                                      Color.Black, 1, ButtonBorderStyle.Solid,
                                      Color.Black, 0, ButtonBorderStyle.Inset,
                                      Color.Black, 0, ButtonBorderStyle.Inset);
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

        private void AddNewNode(bool isParent)
        {
            frmNodeName nameForm = new frmNodeName();
            if (nameForm.ShowDialog() == DialogResult.OK)
            {
                TreeGridNode node;
                if (isParent)
                {
                    node = gridNodes.CurrentNode.Parent.Nodes.Add(nameForm.NodeName);
                    AddChildren(node, gridNodes.CurrentNode);
                    MoveNode(node, gridNodes.CurrentNode);
                }
                else
                {
                    node = gridNodes.CurrentNode.Nodes.Add(nameForm.NodeName);
                }

                node.Cells[1].Value = "Element";
                node.Cells[2].Value = "One";
                node.Cells[3].Value = "Flatten";
                node.Cells[4].Value = nameForm.NodeName;
                node.Cells[5].Value = "String";
                ExpandTree();
            }
        }

        private void addChildMenuItem_Click(object sender, EventArgs e)
        {
            AddNewNode(false);
        }

        private void addParentMenuItem_Click(object sender, EventArgs e)
        {
            if (gridNodes.CurrentNode.Parent.Cells[0].Value == null)
                MessageBox.Show("Cannot add parent to Root", "XML Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                AddNewNode(true);
        }

        private void removeMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this node and all its children?", "XML Converter", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                gridNodes.CurrentNode.Parent.Nodes.Remove(gridNodes.CurrentNode);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            logger.Trace("Clicked_cancel");
            isCancel = true;
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            logger.Info("Closing window");
            this.Close();
        }

        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        private void btnReadXML_Click(object sender, EventArgs e)
        {
            DialogResult result = inputFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (System.IO.Path.GetExtension(inputFileDialog.FileName).ToLower() == ".xml")
                {
                    logger.Info("Loading_schema_from_file {0}", inputFileDialog.FileName);

                    SetInputFilePath(inputFileDialog.FileName);
                    Field field = Field.Parse(inputFileDialog.FileName);
                    SetTreeFromSchema(field);
                    this.State.InputIsSchema = true;
                    ExpandTree();
                }
                else
                {
                    MessageBox.Show("Selected file is not an XML file", "XML Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        #region NodeProcessing

        /// <summary>
        /// Creates tree nodes from schema nodes
        /// </summary>
        /// <param name="schemaNode"></param>
        /// <param name="parentTreeNode"></param>
        private void CreateTreeChildNodes(Field schemaNode, TreeGridNode parentTreeNode)
        {
            if (parentTreeNode.Cells[0].Value.ToString() == "Root")
            {
                logger.Trace("Creating childNodes");
            }
            int count = 0;
            string name = schemaNode.Name;


            //0-Name
            //1-Type [Element, Attribute, PCData]
            //2-Optionality [One, Zero or one, Zero or more, One or more]
            //3-Change [None, Ignore, Flatten]
            //4-XML Name
            //5-Data Type


            TreeGridNode treeNode = parentTreeNode.Nodes.Add(name);
            treeNode.Cells[0].Value = schemaNode.Name;
            treeNode.Cells[1].Value = schemaNode.Type;
            treeNode.Cells[2].Value = schemaNode.Optionality;
            treeNode.Cells[3].Value = schemaNode.Change;
            treeNode.Cells[4].Value = schemaNode.Alias;
            treeNode.Cells[5].Value = FirstLetterToUpper(schemaNode.DataType);
            count++;

            if (schemaNode.ChildNodes.Count > 0)
            {
                foreach (var childSchemaNode in schemaNode.ChildNodes)
                {
                    CreateTreeChildNodes(childSchemaNode, treeNode);
                }
            }
        }

        /// <summary>
        /// Copies nodes from second to first
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private void AddChildren(TreeGridNode first, TreeGridNode second)
        {
            //TreeGridNode node = parent.Nodes.Add("Hello");
            TreeGridNode node = first.Nodes.Add(second.Cells[0].Value.ToString());
            node.Cells[1].Value = second.Cells[1].Value;
            node.Cells[2].Value = second.Cells[2].Value;
            node.Cells[3].Value = second.Cells[3].Value;
            node.Cells[4].Value = second.Cells[4].Value;
            node.Cells[5].Value = second.Cells[5].Value;
            foreach (TreeGridNode sub in second.Nodes)
            {
                AddChildren(node, sub);
            }
        }

        /// <summary>
        /// Creates schema from visual tree
        /// </summary>
        /// <returns></returns>
        private Field GenerateSchema()
        {
            logger.Trace("Generating_schema_from_node_tree");
            Field _schema = null;
            foreach (TreeGridNode node in gridNodes.Nodes[0].Nodes)
            {
                CreateDefinition(node, ref _schema);
            }
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
            field.Type = treeGridNode.Cells[1].Value.ToString();
            field.Optionality = treeGridNode.Cells[2].Value.ToString();
            field.Change = treeGridNode.Cells[3].Value.ToString();
            field.Alias = treeGridNode.Cells[4].Value.ToString();
            field.DataType = treeGridNode.Cells[5].Value.ToString();

            if (element == null)
                element = field;
            else
                element.ChildNodes.Add(field);

            logger.Trace("Created_field_from_tree_node {0}", field.ToString());

            foreach (TreeGridNode subNode in treeGridNode.Nodes)
            {
                CreateDefinition(subNode, ref field);
            }
        }

        #endregion NodeProcessing
    }
}