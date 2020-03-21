namespace ScriptingApp
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.grInput = new AdvancedDataGridView.TreeGridView();
            this.NodeName = new AdvancedDataGridView.TreeGridColumn();
            this.NodeXMLName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NodeDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NodeType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.NodeOptionality = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.NodeChange = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.grOutput = new AdvancedDataGridView.TreeGridView();
            this.OutputNodeName = new AdvancedDataGridView.TreeGridColumn();
            this.OutputNodeXMLName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OutputNodeDataType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.OutputNodeOptionality = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.OutputNodeChange = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.txtdataoutputcount = new System.Windows.Forms.NumericUpDown();
            this.txtSheetoutputcount = new System.Windows.Forms.NumericUpDown();
            this.lblSheetoutputcount = new System.Windows.Forms.Label();
            this.tblLayoutPane1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelDataInputCount = new System.Windows.Forms.TableLayoutPanel();
            this.labelDataInputCount = new System.Windows.Forms.Label();
            this.txtdatainputcount = new System.Windows.Forms.NumericUpDown();
            this.lblSheetinputcount = new System.Windows.Forms.Label();
            this.txtSheetinputcount = new System.Windows.Forms.NumericUpDown();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanelSheetOutputCount = new System.Windows.Forms.TableLayoutPanel();
            this.labelDataOutputCount = new System.Windows.Forms.Label();
            this.flowLayoutPanelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.txtSample = new System.Windows.Forms.RichTextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFindError = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabSample = new System.Windows.Forms.TabPage();
            this.tabScript = new System.Windows.Forms.TabPage();
            this.txtScript = new System.Windows.Forms.RichTextBox();
            this.txtCompileStatus = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanelCode = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOk = new System.Windows.Forms.Button();
            this.groupBoxCompileStatus = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.grInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdataoutputcount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSheetoutputcount)).BeginInit();
            this.tblLayoutPane1.SuspendLayout();
            this.groupBoxInput.SuspendLayout();
            this.tableLayoutPanelDataInputCount.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtdatainputcount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSheetinputcount)).BeginInit();
            this.groupBoxOutput.SuspendLayout();
            this.tableLayoutPanelSheetOutputCount.SuspendLayout();
            this.flowLayoutPanelActions.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabSample.SuspendLayout();
            this.tabScript.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanelCode.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBoxCompileStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // grInput
            // 
            this.grInput.AllowUserToAddRows = false;
            this.grInput.AllowUserToDeleteRows = false;
            this.grInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grInput.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grInput.BackgroundColor = System.Drawing.Color.White;
            this.grInput.ColumnHeadersHeight = 29;
            this.grInput.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.NodeName,
            this.NodeXMLName,
            this.NodeDataType,
            this.NodeType,
            this.NodeOptionality,
            this.NodeChange});
            this.grInput.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grInput.Enabled = false;
            this.grInput.ImageList = null;
            this.grInput.Location = new System.Drawing.Point(3, 57);
            this.grInput.MultiSelect = false;
            this.grInput.Name = "grInput";
            this.grInput.ReadOnly = true;
            this.grInput.RowHeadersVisible = false;
            this.grInput.RowHeadersWidth = 51;
            this.grInput.Size = new System.Drawing.Size(321, 349);
            this.grInput.TabIndex = 1;
            // 
            // NodeName
            // 
            this.NodeName.DefaultNodeImage = null;
            this.NodeName.FillWeight = 152.7174F;
            this.NodeName.HeaderText = "Name";
            this.NodeName.MinimumWidth = 6;
            this.NodeName.Name = "NodeName";
            this.NodeName.ReadOnly = true;
            this.NodeName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.NodeName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // NodeXMLName
            // 
            this.NodeXMLName.HeaderText = "Alias";
            this.NodeXMLName.MinimumWidth = 6;
            this.NodeXMLName.Name = "NodeXMLName";
            this.NodeXMLName.ReadOnly = true;
            this.NodeXMLName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.NodeXMLName.Visible = false;
            // 
            // NodeDataType
            // 
            this.NodeDataType.FillWeight = 83.1115F;
            this.NodeDataType.HeaderText = "Data Type";
            this.NodeDataType.MinimumWidth = 6;
            this.NodeDataType.Name = "NodeDataType";
            this.NodeDataType.ReadOnly = true;
            this.NodeDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.NodeDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // NodeType
            // 
            this.NodeType.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.Nothing;
            this.NodeType.FillWeight = 64.17112F;
            this.NodeType.HeaderText = "Type";
            this.NodeType.Items.AddRange(new object[] {
            "Element",
            "Attribute",
            "PCData"});
            this.NodeType.MinimumWidth = 6;
            this.NodeType.Name = "NodeType";
            this.NodeType.ReadOnly = true;
            this.NodeType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // NodeOptionality
            // 
            this.NodeOptionality.HeaderText = "Optionality";
            this.NodeOptionality.Items.AddRange(new object[] {
            "One",
            "Zero or one",
            "Zero or more",
            "One or more"});
            this.NodeOptionality.MinimumWidth = 6;
            this.NodeOptionality.Name = "NodeOptionality";
            this.NodeOptionality.ReadOnly = true;
            this.NodeOptionality.Visible = false;
            // 
            // NodeChange
            // 
            this.NodeChange.HeaderText = "Change";
            this.NodeChange.Items.AddRange(new object[] {
            "None",
            "Ignore",
            "Flatten"});
            this.NodeChange.MinimumWidth = 6;
            this.NodeChange.Name = "NodeChange";
            this.NodeChange.ReadOnly = true;
            this.NodeChange.Visible = false;
            // 
            // grOutput
            // 
            this.grOutput.AllowDrop = true;
            this.grOutput.AllowUserToAddRows = false;
            this.grOutput.AllowUserToDeleteRows = false;
            this.grOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grOutput.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grOutput.BackgroundColor = System.Drawing.Color.White;
            this.grOutput.ColumnHeadersHeight = 29;
            this.grOutput.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OutputNodeName,
            this.OutputNodeXMLName,
            this.OutputNodeDataType,
            this.dataGridViewComboBoxColumn1,
            this.OutputNodeOptionality,
            this.OutputNodeChange});
            this.grOutput.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            this.grOutput.ImageList = null;
            this.grOutput.Location = new System.Drawing.Point(3, 60);
            this.grOutput.Name = "grOutput";
            this.grOutput.RowHeadersVisible = false;
            this.grOutput.RowHeadersWidth = 51;
            this.grOutput.Size = new System.Drawing.Size(321, 272);
            this.grOutput.TabIndex = 3;
            // 
            // OutputNodeName
            // 
            this.OutputNodeName.DefaultNodeImage = null;
            this.OutputNodeName.FillWeight = 152.7174F;
            this.OutputNodeName.HeaderText = "Name";
            this.OutputNodeName.MinimumWidth = 6;
            this.OutputNodeName.Name = "OutputNodeName";
            this.OutputNodeName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.OutputNodeName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // OutputNodeXMLName
            // 
            this.OutputNodeXMLName.HeaderText = "Alias";
            this.OutputNodeXMLName.MinimumWidth = 6;
            this.OutputNodeXMLName.Name = "OutputNodeXMLName";
            this.OutputNodeXMLName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.OutputNodeXMLName.Visible = false;
            // 
            // OutputNodeDataType
            // 
            this.OutputNodeDataType.FillWeight = 83.1115F;
            this.OutputNodeDataType.HeaderText = "Data Type";
            this.OutputNodeDataType.Items.AddRange(new object[] {
            "String",
            "Array",
            "Int16",
            "Int32",
            "Int64",
            "Boolean",
            "DateTime",
            "Double",
            "Single"});
            this.OutputNodeDataType.MinimumWidth = 6;
            this.OutputNodeDataType.Name = "OutputNodeDataType";
            this.OutputNodeDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // dataGridViewComboBoxColumn1
            // 
            this.dataGridViewComboBoxColumn1.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.dataGridViewComboBoxColumn1.DisplayStyleForCurrentCellOnly = true;
            this.dataGridViewComboBoxColumn1.FillWeight = 64.17112F;
            this.dataGridViewComboBoxColumn1.HeaderText = "Type";
            this.dataGridViewComboBoxColumn1.Items.AddRange(new object[] {
            "Element",
            "Attribute",
            "PCData"});
            this.dataGridViewComboBoxColumn1.MinimumWidth = 6;
            this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            // 
            // OutputNodeOptionality
            // 
            this.OutputNodeOptionality.HeaderText = "Optionality";
            this.OutputNodeOptionality.Items.AddRange(new object[] {
            "One",
            "Zero or one",
            "Zero or more",
            "One or more"});
            this.OutputNodeOptionality.MinimumWidth = 6;
            this.OutputNodeOptionality.Name = "OutputNodeOptionality";
            this.OutputNodeOptionality.Visible = false;
            // 
            // OutputNodeChange
            // 
            this.OutputNodeChange.HeaderText = "Change";
            this.OutputNodeChange.Items.AddRange(new object[] {
            "None",
            "Ignore",
            "Flatten"});
            this.OutputNodeChange.MinimumWidth = 6;
            this.OutputNodeChange.Name = "OutputNodeChange";
            this.OutputNodeChange.Visible = false;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.BackgroundImage = global::ScriptingApp.Properties.Resources.Save;
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnSave.Location = new System.Drawing.Point(289, 3);
            this.btnSave.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(35, 39);
            this.btnSave.TabIndex = 7;
            this.btnSave.Tag = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAdd.BackColor = System.Drawing.Color.Transparent;
            this.btnAdd.BackgroundImage = global::ScriptingApp.Properties.Resources.Add;
            this.btnAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnAdd.Location = new System.Drawing.Point(77, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(35, 39);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Tag = "Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnUp
            // 
            this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUp.BackColor = System.Drawing.Color.Transparent;
            this.btnUp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnUp.BackgroundImage")));
            this.btnUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnUp.FlatAppearance.BorderSize = 0;
            this.btnUp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUp.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnUp.Location = new System.Drawing.Point(125, 3);
            this.btnUp.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(35, 39);
            this.btnUp.TabIndex = 5;
            this.btnUp.Tag = "Up";
            this.btnUp.UseVisualStyleBackColor = false;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDown.BackColor = System.Drawing.Color.Transparent;
            this.btnDown.BackgroundImage = global::ScriptingApp.Properties.Resources.DownArrow;
            this.btnDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDown.FlatAppearance.BorderSize = 0;
            this.btnDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDown.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnDown.Location = new System.Drawing.Point(173, 3);
            this.btnDown.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(35, 39);
            this.btnDown.TabIndex = 4;
            this.btnDown.Tag = "Down";
            this.btnDown.UseVisualStyleBackColor = false;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.BackColor = System.Drawing.Color.Transparent;
            this.btnDelete.BackgroundImage = global::ScriptingApp.Properties.Resources.remove;
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnDelete.Location = new System.Drawing.Point(241, 3);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(35, 39);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Tag = "Remove";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // txtdataoutputcount
            // 
            this.txtdataoutputcount.AutoSize = true;
            this.txtdataoutputcount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtdataoutputcount.Location = new System.Drawing.Point(142, 3);
            this.txtdataoutputcount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtdataoutputcount.Name = "txtdataoutputcount";
            this.txtdataoutputcount.Size = new System.Drawing.Size(45, 22);
            this.txtdataoutputcount.TabIndex = 10;
            this.txtdataoutputcount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtdataoutputcount.ValueChanged += new System.EventHandler(this.txtdataoutputcount_ValueChanged);
            // 
            // txtSheetoutputcount
            // 
            this.txtSheetoutputcount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSheetoutputcount.AutoSize = true;
            this.txtSheetoutputcount.Enabled = false;
            this.txtSheetoutputcount.Location = new System.Drawing.Point(340, 3);
            this.txtSheetoutputcount.Name = "txtSheetoutputcount";
            this.txtSheetoutputcount.Size = new System.Drawing.Size(45, 22);
            this.txtSheetoutputcount.TabIndex = 14;
            // 
            // lblSheetoutputcount
            // 
            this.lblSheetoutputcount.Location = new System.Drawing.Point(193, 0);
            this.lblSheetoutputcount.Name = "lblSheetoutputcount";
            this.lblSheetoutputcount.Size = new System.Drawing.Size(141, 26);
            this.lblSheetoutputcount.TabIndex = 13;
            this.lblSheetoutputcount.Text = "Sheet output count:";
            this.lblSheetoutputcount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tblLayoutPane1
            // 
            this.tblLayoutPane1.ColumnCount = 1;
            this.tblLayoutPane1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblLayoutPane1.Controls.Add(this.groupBoxInput, 0, 0);
            this.tblLayoutPane1.Controls.Add(this.groupBoxOutput, 0, 1);
            this.tblLayoutPane1.Controls.Add(this.flowLayoutPanelActions, 0, 2);
            this.tblLayoutPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblLayoutPane1.Location = new System.Drawing.Point(0, 0);
            this.tblLayoutPane1.Name = "tblLayoutPane1";
            this.tblLayoutPane1.RowCount = 3;
            this.tblLayoutPane1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.99999F));
            this.tblLayoutPane1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblLayoutPane1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblLayoutPane1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblLayoutPane1.Size = new System.Drawing.Size(333, 733);
            this.tblLayoutPane1.TabIndex = 2;
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Controls.Add(this.tableLayoutPanelDataInputCount);
            this.groupBoxInput.Controls.Add(this.grInput);
            this.groupBoxInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInput.Location = new System.Drawing.Point(3, 3);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Size = new System.Drawing.Size(327, 334);
            this.groupBoxInput.TabIndex = 24;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "Data input";
            // 
            // tableLayoutPanelDataInputCount
            // 
            this.tableLayoutPanelDataInputCount.AutoSize = true;
            this.tableLayoutPanelDataInputCount.ColumnCount = 4;
            this.tableLayoutPanelDataInputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDataInputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDataInputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDataInputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelDataInputCount.Controls.Add(this.labelDataInputCount, 0, 0);
            this.tableLayoutPanelDataInputCount.Controls.Add(this.txtdatainputcount, 1, 0);
            this.tableLayoutPanelDataInputCount.Controls.Add(this.lblSheetinputcount, 2, 0);
            this.tableLayoutPanelDataInputCount.Controls.Add(this.txtSheetinputcount, 3, 0);
            this.tableLayoutPanelDataInputCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelDataInputCount.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanelDataInputCount.Name = "tableLayoutPanelDataInputCount";
            this.tableLayoutPanelDataInputCount.RowCount = 1;
            this.tableLayoutPanelDataInputCount.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelDataInputCount.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanelDataInputCount.Size = new System.Drawing.Size(321, 28);
            this.tableLayoutPanelDataInputCount.TabIndex = 24;
            // 
            // labelDataInputCount
            // 
            this.labelDataInputCount.Location = new System.Drawing.Point(3, 0);
            this.labelDataInputCount.Name = "labelDataInputCount";
            this.labelDataInputCount.Size = new System.Drawing.Size(133, 26);
            this.labelDataInputCount.TabIndex = 24;
            this.labelDataInputCount.Text = "Data input count:";
            this.labelDataInputCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtdatainputcount
            // 
            this.txtdatainputcount.AutoSize = true;
            this.txtdatainputcount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtdatainputcount.Location = new System.Drawing.Point(142, 3);
            this.txtdatainputcount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtdatainputcount.Name = "txtdatainputcount";
            this.txtdatainputcount.Size = new System.Drawing.Size(45, 22);
            this.txtdatainputcount.TabIndex = 8;
            this.txtdatainputcount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtdatainputcount.ValueChanged += new System.EventHandler(this.txtdatainputcount_ValueChanged);
            // 
            // lblSheetinputcount
            // 
            this.lblSheetinputcount.Location = new System.Drawing.Point(193, 0);
            this.lblSheetinputcount.Name = "lblSheetinputcount";
            this.lblSheetinputcount.Size = new System.Drawing.Size(141, 26);
            this.lblSheetinputcount.TabIndex = 11;
            this.lblSheetinputcount.Text = "Sheet input count:";
            this.lblSheetinputcount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSheetinputcount
            // 
            this.txtSheetinputcount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSheetinputcount.AutoSize = true;
            this.txtSheetinputcount.Enabled = false;
            this.txtSheetinputcount.Location = new System.Drawing.Point(340, 3);
            this.txtSheetinputcount.Name = "txtSheetinputcount";
            this.txtSheetinputcount.Size = new System.Drawing.Size(45, 22);
            this.txtSheetinputcount.TabIndex = 12;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Controls.Add(this.tableLayoutPanelSheetOutputCount);
            this.groupBoxOutput.Controls.Add(this.grOutput);
            this.groupBoxOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOutput.Location = new System.Drawing.Point(3, 343);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(327, 335);
            this.groupBoxOutput.TabIndex = 24;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Data output";
            // 
            // tableLayoutPanelSheetOutputCount
            // 
            this.tableLayoutPanelSheetOutputCount.AutoSize = true;
            this.tableLayoutPanelSheetOutputCount.ColumnCount = 4;
            this.tableLayoutPanelSheetOutputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSheetOutputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSheetOutputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSheetOutputCount.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSheetOutputCount.Controls.Add(this.labelDataOutputCount, 0, 0);
            this.tableLayoutPanelSheetOutputCount.Controls.Add(this.txtdataoutputcount, 1, 0);
            this.tableLayoutPanelSheetOutputCount.Controls.Add(this.lblSheetoutputcount, 2, 0);
            this.tableLayoutPanelSheetOutputCount.Controls.Add(this.txtSheetoutputcount, 3, 0);
            this.tableLayoutPanelSheetOutputCount.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanelSheetOutputCount.Location = new System.Drawing.Point(3, 18);
            this.tableLayoutPanelSheetOutputCount.Name = "tableLayoutPanelSheetOutputCount";
            this.tableLayoutPanelSheetOutputCount.RowCount = 1;
            this.tableLayoutPanelSheetOutputCount.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSheetOutputCount.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanelSheetOutputCount.Size = new System.Drawing.Size(321, 28);
            this.tableLayoutPanelSheetOutputCount.TabIndex = 26;
            // 
            // labelDataOutputCount
            // 
            this.labelDataOutputCount.Location = new System.Drawing.Point(3, 0);
            this.labelDataOutputCount.Name = "labelDataOutputCount";
            this.labelDataOutputCount.Size = new System.Drawing.Size(133, 26);
            this.labelDataOutputCount.TabIndex = 24;
            this.labelDataOutputCount.Text = "Data output count:";
            this.labelDataOutputCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowLayoutPanelActions
            // 
            this.flowLayoutPanelActions.AutoSize = true;
            this.flowLayoutPanelActions.Controls.Add(this.btnSave);
            this.flowLayoutPanelActions.Controls.Add(this.btnDelete);
            this.flowLayoutPanelActions.Controls.Add(this.btnDown);
            this.flowLayoutPanelActions.Controls.Add(this.btnUp);
            this.flowLayoutPanelActions.Controls.Add(this.btnAdd);
            this.flowLayoutPanelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanelActions.Location = new System.Drawing.Point(3, 684);
            this.flowLayoutPanelActions.Name = "flowLayoutPanelActions";
            this.flowLayoutPanelActions.Size = new System.Drawing.Size(327, 46);
            this.flowLayoutPanelActions.TabIndex = 24;
            // 
            // txtSample
            // 
            this.txtSample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSample.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSample.Location = new System.Drawing.Point(3, 3);
            this.txtSample.Name = "txtSample";
            this.txtSample.ReadOnly = true;
            this.txtSample.Size = new System.Drawing.Size(650, 565);
            this.txtSample.TabIndex = 17;
            this.txtSample.Text = ";";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.AutoSize = true;
            this.btnCancel.Location = new System.Drawing.Point(186, 88);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(127, 38);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnFindError
            // 
            this.btnFindError.Location = new System.Drawing.Point(3, 41);
            this.btnFindError.Name = "btnFindError";
            this.btnFindError.Size = new System.Drawing.Size(171, 38);
            this.btnFindError.TabIndex = 20;
            this.btnFindError.Text = "Find Error";
            this.btnFindError.UseVisualStyleBackColor = true;
            this.btnFindError.Click += new System.EventHandler(this.btnFindError_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabSample);
            this.tabControl1.Controls.Add(this.tabScript);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(664, 600);
            this.tabControl1.TabIndex = 21;
            // 
            // tabSample
            // 
            this.tabSample.Controls.Add(this.txtSample);
            this.tabSample.Location = new System.Drawing.Point(4, 25);
            this.tabSample.Name = "tabSample";
            this.tabSample.Padding = new System.Windows.Forms.Padding(3);
            this.tabSample.Size = new System.Drawing.Size(656, 571);
            this.tabSample.TabIndex = 0;
            this.tabSample.Text = "Sample";
            this.tabSample.UseVisualStyleBackColor = true;
            // 
            // tabScript
            // 
            this.tabScript.Controls.Add(this.txtScript);
            this.tabScript.Location = new System.Drawing.Point(4, 22);
            this.tabScript.Name = "tabScript";
            this.tabScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabScript.Size = new System.Drawing.Size(656, 574);
            this.tabScript.TabIndex = 1;
            this.tabScript.Text = "Script";
            this.tabScript.UseVisualStyleBackColor = true;
            // 
            // txtScript
            // 
            this.txtScript.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtScript.EnableAutoDragDrop = true;
            this.txtScript.Location = new System.Drawing.Point(3, 3);
            this.txtScript.Name = "txtScript";
            this.txtScript.Size = new System.Drawing.Size(650, 568);
            this.txtScript.TabIndex = 18;
            this.txtScript.Text = "";
            this.txtScript.WordWrap = false;
            // 
            // txtCompileStatus
            // 
            this.txtCompileStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCompileStatus.Location = new System.Drawing.Point(3, 18);
            this.txtCompileStatus.Name = "txtCompileStatus";
            this.txtCompileStatus.Size = new System.Drawing.Size(336, 102);
            this.txtCompileStatus.TabIndex = 22;
            this.txtCompileStatus.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tblLayoutPane1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(1005, 735);
            this.splitContainer1.SplitterDistance = 335;
            this.splitContainer1.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panel1);
            this.splitContainer2.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tableLayoutPanelCode);
            this.splitContainer2.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer2.Panel2MinSize = 100;
            this.splitContainer2.Size = new System.Drawing.Size(664, 733);
            this.splitContainer2.SplitterDistance = 600;
            this.splitContainer2.TabIndex = 23;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tabControl1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(664, 600);
            this.panel1.TabIndex = 23;
            // 
            // tableLayoutPanelCode
            // 
            this.tableLayoutPanelCode.ColumnCount = 3;
            this.tableLayoutPanelCode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCode.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelCode.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanelCode.Controls.Add(this.btnCancel, 1, 0);
            this.tableLayoutPanelCode.Controls.Add(this.groupBoxCompileStatus, 2, 0);
            this.tableLayoutPanelCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelCode.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelCode.Name = "tableLayoutPanelCode";
            this.tableLayoutPanelCode.RowCount = 1;
            this.tableLayoutPanelCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelCode.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 129F));
            this.tableLayoutPanelCode.Size = new System.Drawing.Size(664, 129);
            this.tableLayoutPanelCode.TabIndex = 24;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.btnOk);
            this.flowLayoutPanel1.Controls.Add(this.btnFindError);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(0, 66);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(177, 123);
            this.flowLayoutPanel1.TabIndex = 22;
            // 
            // btnOk
            // 
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnOk.Location = new System.Drawing.Point(3, 85);
            this.btnOk.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(171, 38);
            this.btnOk.TabIndex = 19;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // groupBoxCompileStatus
            // 
            this.groupBoxCompileStatus.AutoSize = true;
            this.groupBoxCompileStatus.Controls.Add(this.txtCompileStatus);
            this.groupBoxCompileStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxCompileStatus.Location = new System.Drawing.Point(319, 3);
            this.groupBoxCompileStatus.Name = "groupBoxCompileStatus";
            this.groupBoxCompileStatus.Size = new System.Drawing.Size(342, 123);
            this.groupBoxCompileStatus.TabIndex = 23;
            this.groupBoxCompileStatus.TabStop = false;
            this.groupBoxCompileStatus.Text = "Compile status";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1005, 735);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Scripting App";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtdataoutputcount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSheetoutputcount)).EndInit();
            this.tblLayoutPane1.ResumeLayout(false);
            this.tblLayoutPane1.PerformLayout();
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            this.tableLayoutPanelDataInputCount.ResumeLayout(false);
            this.tableLayoutPanelDataInputCount.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtdatainputcount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSheetinputcount)).EndInit();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.tableLayoutPanelSheetOutputCount.ResumeLayout(false);
            this.tableLayoutPanelSheetOutputCount.PerformLayout();
            this.flowLayoutPanelActions.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabSample.ResumeLayout(false);
            this.tabScript.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanelCode.ResumeLayout(false);
            this.tableLayoutPanelCode.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBoxCompileStatus.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private AdvancedDataGridView.TreeGridView grInput;
        private AdvancedDataGridView.TreeGridView grOutput;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.NumericUpDown txtdataoutputcount;
        private System.Windows.Forms.NumericUpDown txtSheetoutputcount;
        private System.Windows.Forms.Label lblSheetoutputcount;
        private System.Windows.Forms.TableLayoutPanel tblLayoutPane1;
        private System.Windows.Forms.RichTextBox txtSample;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFindError;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabSample;
        private System.Windows.Forms.TabPage tabScript;
        private System.Windows.Forms.RichTextBox txtScript;
        private System.Windows.Forms.RichTextBox txtCompileStatus;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelActions;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label labelDataOutputCount;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCode;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSheetOutputCount;
        private AdvancedDataGridView.TreeGridColumn OutputNodeName;
        private System.Windows.Forms.DataGridViewTextBoxColumn OutputNodeXMLName;
        private System.Windows.Forms.DataGridViewComboBoxColumn OutputNodeDataType;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private System.Windows.Forms.DataGridViewComboBoxColumn OutputNodeOptionality;
        private System.Windows.Forms.DataGridViewComboBoxColumn OutputNodeChange;
        private System.Windows.Forms.GroupBox groupBoxCompileStatus;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelDataInputCount;
        private System.Windows.Forms.Label labelDataInputCount;
        private System.Windows.Forms.NumericUpDown txtdatainputcount;
        private System.Windows.Forms.Label lblSheetinputcount;
        private System.Windows.Forms.NumericUpDown txtSheetinputcount;
		private AdvancedDataGridView.TreeGridColumn NodeName;
		private System.Windows.Forms.DataGridViewTextBoxColumn NodeXMLName;
		private System.Windows.Forms.DataGridViewTextBoxColumn NodeDataType;
		private System.Windows.Forms.DataGridViewComboBoxColumn NodeType;
		private System.Windows.Forms.DataGridViewComboBoxColumn NodeOptionality;
		private System.Windows.Forms.DataGridViewComboBoxColumn NodeChange;
	}
}

