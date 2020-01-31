using System;
using System.Windows;
using System.Windows.Input;
using DataOutput.ViewModel;
using System.Collections.ObjectModel;
using JdSuite.Common.Module;
using System.Xml.Linq;
using JdSuite.Common;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataOutput.Controls;

namespace DataOutput
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowClass : Window
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(MainWindowClass));

        private Point LastPos = new Point(0.0, 0.0);

        private ObservableCollection<OutputViewModel> Items { get; set; }

        private NodeIndexer DataFactory { get; set; }

        // private XMLFieldValidator xmlFieldValidator { get; set; } = new XMLFieldValidator();

        public MainWindowClass()
        {
            Items = new ObservableCollection<OutputViewModel>();
            InitializeComponent();
            treeGrid.rootNode.FetchRecordNo = FetchRecordNo;
            this.Closing += MainWindowClass_Closing;
        }

        private void MainWindowClass_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.DataFactory != null)
            {
                logger.Trace("Calling DataFactory.CloseFileStream()");
                this.DataFactory.CloseFileStream();
            }
        }

        public void AddItem(OutputViewModel item)
        {
            Items.Add(item);
        }

        Field _schema;
        public Field Schema
        {
            get { return _schema; }
            set { _schema = value; }
        }



        public string DataFile
        {
            get; set;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            logger.Info("Loading Output Module Main Window");
        }

        private void OnError(string error)
        {

        }

        public void LoadData(string dataFile, Field schema)
        {
            this.treeGrid.rootNode.RootSchemaNode = schema;

            Dispatcher.BeginInvoke(new Action(() => LoadDataInternal(dataFile, schema)));
        }

        public void LoadDataInternal(string dataFile, Field schema)
        {
            try
            {
                logger.Info("Loading DataFile: {0}", dataFile);
                //Now module will push items to it as soon as it receives items
                //Make changes for it


                if (DataFactory != null)
                    DataFactory.Dispose();

                DataFactory = new NodeIndexer();
                DataFactory.OnNodeParsed += DataFactory_NodeParsed;
                DataFactory.OnIndexingCompleted += DataFactory_NodeIndexingCompleted;
                DataFactory.OnError = OnError;

                // string dataFile = @"C:\Application_Testing\shakeel\Customer_data_XML_1.xml";
                //string schemaFile = @"C:\Application_Testing\shakeel\xml_schema.xml";

                // Schema = Field.Parse(schemaFile);
                // DataFile = dataFile;

                DataFactory.LoadData(dataFile, schema);


                //xmlOutputControl.RefreshData();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessageService.ShowError("Data Loading Error", ex.Message);
            }
        }

        private DataItem FetchRecordNo(int RecordId, Field Schema)
        {
            logger.Info("Fetching_record_id from DataFactory {0}", RecordId);
            return DataFactory.GetDataItemNode(RecordId, Schema.Name);
        }

        private void DataFactory_NodeIndexingCompleted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var rootSchema = treeGrid.rootNode.RootSchemaNode;
                foreach (var firstLevelSchema in rootSchema.ChildNodes)
                {

                   

                    var oldDataItem = treeGrid.rootNode.DataItems.FirstOrDefault(x => x.Name == firstLevelSchema.Name && x.Schema.XPath==firstLevelSchema.XPath);

                    if (oldDataItem != null)
                    {
                        oldDataItem.TotalRecordCount = DataFactory.GetNodeCount(firstLevelSchema.Name);

                        if (oldDataItem.IsDataNodeMissing)
                        {
                            var newDataItem = DataFactory.GetDataItemNode(1, oldDataItem.Name);

                            var childItem = newDataItem.Children.FirstOrDefault(x => x.Name == firstLevelSchema.Name);

                            if (childItem != null)
                            {
                                if (childItem.IsProperty)
                                {
                                    oldDataItem.ValueBlock.Text = childItem.Value;
                                    oldDataItem.Value = childItem.Value;
                                }
                                else
                                {

                                    TreeNodeX treeNodeX = oldDataItem.VisualTreeNode as TreeNodeX;
                                    treeNodeX.LoadData(childItem);

                                    treeGrid.rootNode.RemoveDataItem(oldDataItem);
                                    oldDataItem.VisualTreeNode = null;
                                }
                            }
                        }
                    }
                    
                }

            });
        }

        private void DataFactory_NodeParsed(object sender, NodeIndexEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {

                /*
                XMLFieldValidator validator = new XMLFieldValidator();
                validator.Validate(dataItem, this.DataFactory.RootSchemaNode);
                validator.LogEachError();
                */


               // treeGrid.rootNode.DataItem.TotalRecordCount = this.DataFactory.ItemCount;
                

                if (treeGrid.rootNode.DataContext == null)
                {
                    treeGrid.rootNode.RootSchemaNode = DataFactory.RootSchemaNode;

                    foreach (var ff in treeGrid.rootNode.RootSchemaNode.ChildNodes)
                    {
                        var dataItem = DataFactory.GetDataItemNode(1, ff.Name);
                        if (dataItem != null)
                        {
                            treeGrid.LoadData(dataItem, 1);
                        }
                    }
                }
            });
        }


        private void Splitter_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void Splitter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                LastPos = Mouse.GetPosition(dockPanelParent);
            }
        }

        public void Log(string Source, NLog.LogLevel level, string message)
        {
            loggerControl?.Log(Source, level, message);
        }
    }


}
