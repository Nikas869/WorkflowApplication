using JdSuite.Common;
using JdSuite.Common.Module;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace DataOutput.Controls
{
    /// <summary>
    /// Interaction logic for TreeGrid.xaml
    /// </summary>
    public partial class TreeGrid : UserControl
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(TreeGrid));

        // DataItemFactory DataFactory { get; set; }

        public TreeGrid()
        {
            InitializeComponent();
            rootNode.HideNavigationButtons();
        }



        public void LoadData(string xmlDataFile, string schemaFile)
        {
            // this.DataFactory = new DataItemFactory();
            //  this.DataFactory.LoadData(xmlDataFile, schemaFile);
            // LoadData(this.DataFactory);
        }


       

        public void LoadData(DataItem dataItem, int recordNo)
        {
            try
            {
                logger.Trace("Loading_DataItem RecordNo {0}", recordNo);

                rootNode.RootNode = rootNode;

                //TreeNodeX.LoadData(dataItem, this.rootNode);

                rootNode.ShowDataItem(dataItem, recordNo);

                rootNode.textNodeHeader.Text = dataItem.Name;
                rootNode.grid.ColumnDefinitions[0].MinWidth = this.rootNode.DataItem.ReverseLevel * 30;


                rootNode.HideNavigationButtons();
                TreeNodeX.AdjustColumnWidth(rootNode, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DataItem_Loading_Error RecordNo {0}", recordNo);
            }
        }

        private void TreeGridCtrl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // logger.Info("TreeNodeX.AdjustColumnWidth(rootNode, true);");//start from here

            TreeNodeX.AdjustColumnWidth(rootNode, true);
        }
    }
}
