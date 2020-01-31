
using JdSuite.Common;
using JdSuite.Common.Module;
using JdSuite.DataFilter.FilterStrategies;
using JdSuite.DataFilter.Models.Filters;

using JdSuite.DataFilter.ViewModels;
using JdSuite.DataFilter.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JdSuite.DataFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger("DataFilter.MainWindow");

        DataFilterModule _module;
        public DataFilterModule DFModule
        {
            get { return _module; }
            set
            {
                _module = value;

                if(_module.OutputNode2!=null)
                {
                    this.chkCreateElseOutput.IsEnabled = true;
                    this.chkCreateElseOutput.IsChecked = true;
                }
            }
        }

        public ViewModel WindowViewModel { get; private set; }

        public List<FilterField> FilterFields
        {
            get { return filterFieldCtrl.Filters; }
        }

        public MainWindow()
        {
            InitializeComponent();

            WindowViewModel = new ViewModel()
            {
                FieldNodes = new ObservableCollection<Field>()
            };

            // WindowViewModel.DataDirectory = @"E:\Canvas\Dropbox\BlockApp\Data";

            //   string schemaFile = @"E:\Canvas2\Dropbox\BlockApp\Data\field_schema.xml";
            //  LoadSchema(schemaFile);

            DataContext = WindowViewModel;
            
        }

        /// <summary>
        /// Adds filter field to display control and displays values from FF on screen
        /// </summary>
        /// <param name="ff"></param>
        public void AddFilterField(FilterField ff)
        {
            this.filterFieldCtrl.AddField(ff);
            chkCreateElseOutput.IsEnabled = true;
        }

        public void LoadSchema(string FileName)
        {
            logger.Trace("Loading Schema from file:{0}", FileName);
            JdSuite.Common.Module.Field rootField = JdSuite.Common.Module.Field.Parse(FileName);

            LoadSchema(rootField);
        }



        public void LoadSchema(Field schemaNode)
        {
            logger.Trace("Loading schema from field");
            WindowViewModel.FieldNodes.Add(schemaNode);

            foreach (object item in this.treeView.Items)
            {
                TreeViewItem treeItem = this.treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeItem != null)
                    ExpandAll(treeItem, true);
                treeItem.IsExpanded = true;
            }

        }

        private void ButtonAddField_Click(object sender, RoutedEventArgs e)
        {
            var selectedField = treeView.SelectedItem as Field;

            if (selectedField.ChildNodes != null)

                UpdateRightPart(selectedField);


        }

        private void ButtonRemoveField_Click(object sender, RoutedEventArgs e)
        {
            var selectedField = treeView.SelectedItem as Field;

            // selectedField.Filter = null;

            UpdateAddFieldButtonEnabled();

            UpdateRightPart(selectedField);


        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedField = treeView.SelectedItem as Field;
            if (selectedField != null)
                WindowViewModel.IsAddFieldButtonEnabled = true;

            //SelectField(selectedField);//1
        }




        private void SelectField(Field selectedField)
        {
            if (selectedField == null)
                return;
            var model = (ViewModel)DataContext;

            foreach (var field in model.FieldNodes)
            {
                DeselectField(field);//2
            }

            //selectedField.IsSelected = true;//It is already done by binding

            //UpdateAddFieldButtonEnabled();
            //UpdateDeleteFieldButtonEnabled();
            //UpdateRightPart(selectedField);
        }

        private void DeselectField(Field field)
        {
            field.IsSelected = false;

            foreach (var subField in field.ChildNodes)
            {
                DeselectField(subField);//3, 4
            }
        }

        private void UpdateAddFieldButtonEnabled()
        {
            var selectedField = treeView.SelectedItem as Field;
        }



        private void UpdateRightPart(Field selectedField)
        {
            if (selectedField.HasChildNodes())
                return;

            if (filterFieldCtrl.Filters.Count >= 2)
            {
                MessageService.ShowError("Invalid Operation", "Condition grid has already two conditions, remove one of them to set another condition");
                return;
            }


            Field root = this.WindowViewModel.FieldNodes[0];

            FilterField ff = new FilterField();

            ff.FieldName = selectedField.Name;
            ff.FieldType = selectedField.DataType;
            ff.Id = -1;
            ff.XPath = selectedField.GetXPath(root);
            ff.JoinType = "or";
            logger.Info("field_xpath {0}={1}", selectedField.Name, ff.XPath);

            filterFieldCtrl.AddField(ff);

            chkCreateElseOutput.IsEnabled = filterFieldCtrl.Filters.Count > 0;

            //DFModule.Store[]
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
             

            if(chkCreateElseOutput.IsEnabled && chkCreateElseOutput.IsChecked.HasValue && chkCreateElseOutput.IsChecked.Value)
            {

                if(this.DFModule!=null)
                {
                    this.DFModule.CreateOutPutNode2();
                }

            }else
            {
                if (this.DFModule != null)
                {
                    this.DFModule.RemoveOutPutNode2();
                }
            }


            this.DialogResult = true;
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void ExpandAll(ItemsControl items, bool expand)
        {
            foreach (object obj in items.Items)
            {
                ItemsControl childControl = items.ItemContainerGenerator.ContainerFromItem(obj) as ItemsControl;
                if (childControl != null)
                {
                    ExpandAll(childControl, expand);
                }
                TreeViewItem item = childControl as TreeViewItem;
                if (item != null)
                    item.IsExpanded = true;
            }
        }
    }


}
