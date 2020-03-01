using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ScriptingApp
{
    public partial class frmAddNode : Form
    {
        public frmAddNode()
        {
            InitializeComponent();
            NodeName = "";
            DataType = "";
            this.ActiveControl = txtNodeName;
        }
        public string NodeName { get; private set; }

        public string DataType { get; private set; }

        private void frmAddNode_Load(object sender, EventArgs e)
        {
            cmbType.DataSource = new List<string> {
                "String",
                "Array",
                "Int16",
                "Int32",
                "Int64",
                "Boolean",
                "DateTime",
                "Double",
                "Single"
            }; ;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            this.NodeName = txtNodeName.Text;
            this.DataType = cmbType.SelectedValue.ToString();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtNodeName_TextChanged(object sender, EventArgs e)
        {
            btnSubmit.Enabled = txtNodeName.Text.Trim().Length > 0;
        }
    }
}


