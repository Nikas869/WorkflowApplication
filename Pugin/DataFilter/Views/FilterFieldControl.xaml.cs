using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using JdSuite.DataFilter.ViewModels;

namespace JdSuite.DataFilter.Views
{
    /// <summary>
    /// Interaction logic for FilterFieldControl.xaml
    /// </summary>
    public partial class FilterFieldControl : UserControl
    {
        //XPath supported functions in .net
        //https://referencesource.microsoft.com/#System.Xml/System/Xml/XPath/Internal/XPathParser.cs

        public const string FilterField = "FilterField";
        public const string ConditionField = "ConditionField";
        private int id = 0;

        NLog.ILogger logger = NLog.LogManager.GetLogger("FilterFieldCtrl");

        public readonly List<string> IntegerTypes = new List<string>() { "Integer", "Int", "Int32", "Int64", "Int16" };
        public readonly List<string> FloatTypes = new List<string>() { "Float", "Decimal" };

        public FilterControlViewModel ViewModel { get; private set; }

        public List<FilterField> Filters { get; private set; } = new List<FilterField>();

        public ObservableCollection<string> IntegerConditions = new ObservableCollection<string>()
        {
            "=","!=",">",">=","<","<=",
        };

        public ObservableCollection<string> StringConditions = new ObservableCollection<string>()
        { "=","!=", "Contains", "Starts With"};
      //    { "=","!=", "Contains", "Starts With", "Ends With","Matches" };



    public FilterFieldControl()
        {
            InitializeComponent();

            ViewModel = new FilterControlViewModel();
            ViewModel.FF = new FilterField();
            ViewModel.FF.FieldName = "";
            ViewModel.FF.FieldType = "";
            ViewModel.FF.Id = -1;
            ViewModel.FF.Condition = "";
            ViewModel.Conditions = StringConditions;

            // this.DataContext = ViewModel;
        }

        public string GetCondition(FilterField upper, FilterField lower)
        {
            string text = "";

            var lst = this.gridFilterFields.Children.OfType<FrameworkElement>().Where(x => x.Tag == lower).ToList();


            var comboJoin = lst.FirstOrDefault(x => x.Name == "comboJoin") as ComboBox;

            if (comboJoin == null)
                return text;


            text = comboJoin.Text;
            var comboRowId = Grid.GetRow(comboJoin);


            var lst2 = this.gridFilterFields.Children.OfType<FrameworkElement>().Where(x => x.Tag == upper).ToList();

            var ctrl = lst2.FirstOrDefault(x => x.Name == "lblFieldName");

            var f1RowId = Grid.GetRow(ctrl);

            if (comboRowId - f1RowId != 1)
                text = "";


            return text;
        }



        public void AddField(FilterField filterField)
        {
            logger.Info("Adding filter field {0}", filterField.FieldName);
                       
             

            if (filterField.Id <= 0)
                filterField.Id = (id++);

            if (Filters.Count > 0 )
            {
                AddConditionField(filterField);
            }//End of condition row


            /************ New Row **************/
            RowDefinition row = new RowDefinition();
            row.Height = GridLength.Auto;

            gridFilterFields.RowDefinitions.Add(row);
            int rowId = gridFilterFields.RowDefinitions.Count;

            rowId--;

            logger.Info("RowId for FieldName_and_type {0}", rowId);

            /************ Name Label **************/
            Label lbl = new Label();
            lbl.Tag = filterField;
            lbl.Name = "lblFieldName";
            lbl.HorizontalAlignment = HorizontalAlignment.Stretch;
            lbl.Content = filterField.FieldName;
            Grid.SetColumn(lbl, 0);
            Grid.SetRow(lbl, rowId);

            int index = gridFilterFields.Children.Add(lbl);

            /************ Type Label **************/
            lbl = new Label();
            lbl.Name = "lblDataType";
            lbl.Tag = filterField;
            lbl.HorizontalAlignment = HorizontalAlignment.Stretch;
            lbl.Content = filterField.FieldType;
            Grid.SetColumn(lbl, 1);
            Grid.SetRow(lbl, rowId);
            index = gridFilterFields.Children.Add(lbl);

            /************ Combobox Condition **************/
            ComboBox comboConditions = new ComboBox();
            comboConditions.Tag = filterField;
            if (filterField.FieldType.ToLower() == "string")
            {
                comboConditions.ItemsSource = StringConditions;

                if(string.IsNullOrEmpty( filterField.Condition))
                {
                    filterField.Condition = "=";
                }

                var binding1 = new Binding("Condition");

                binding1.Source = filterField;
                binding1.Mode = BindingMode.TwoWay;
                binding1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                binding1.BindsDirectlyToSource = true;

                comboConditions.SetBinding(ComboBox.SelectedItemProperty, binding1);


            }
            else
            {
                if (string.IsNullOrEmpty(filterField.Condition))
                {
                    filterField.Condition = "=";
                }

                comboConditions.ItemsSource = IntegerConditions;


                var binding1 = new Binding("Condition");

                binding1.Source = filterField;
                binding1.Mode = BindingMode.TwoWay;
                binding1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                binding1.BindsDirectlyToSource = true;

                comboConditions.SetBinding(ComboBox.SelectedItemProperty, binding1);

            }


            comboConditions.HorizontalAlignment = HorizontalAlignment.Left;
            comboConditions.HorizontalContentAlignment = HorizontalAlignment.Left;
            comboConditions.MinWidth = 100;
            comboConditions.Margin = new Thickness(5, 2, 2, 2);
            Grid.SetColumn(comboConditions, 2);
            Grid.SetRow(comboConditions, rowId);

            index = gridFilterFields.Children.Add(comboConditions);


            /************ Condition Value TextBox **************/
            TextBox text = new TextBox();
            text.Tag = filterField;
            text.MaxLength = 50;
            text.HorizontalAlignment = HorizontalAlignment.Left;
            text.HorizontalContentAlignment = HorizontalAlignment.Left;
            text.MinWidth = 120;
            text.Margin = new Thickness(5, 2, 2, 2);
            text.PreviewTextInput += TextBox_PreviewTextInput;

            var binding = new Binding("ConditionValue");

            binding.Source = filterField;
            binding.Mode = BindingMode.TwoWay;
            text.SetBinding(TextBox.TextProperty, binding);


            Grid.SetColumn(text, 3);
            Grid.SetRow(text, rowId);
            index = gridFilterFields.Children.Add(text);


            /************ Delete Link Button **************/
            Button btnDelete = new Button();
            btnDelete.Tag = filterField;
            btnDelete.Margin = new Thickness(2);
            btnDelete.HorizontalAlignment = HorizontalAlignment.Center;
            btnDelete.Background = Brushes.LightCoral;

            btnDelete.Content = Convert(Properties.Resources.if_x_circle_2561211);
            

            // Image img = new Image();
            //  var b= Properties.Resources.if_x_circle_2561211;


            // img.Source = (BitmapImage)c.ConvertFrom(Properties.Resources.if_x_circle_2561211);
            btnDelete.Content = "Remove";// Properties.Resources.if_x_circle_2561211;

            Grid.SetColumn(btnDelete, 5);
            Grid.SetRow(btnDelete, rowId);
            index = gridFilterFields.Children.Add(btnDelete);

            btnDelete.Click += BtnDelete_Click;


            /************ Horizontal Line **************/
            var line = new Line();
            line.Tag = filterField;
            line.Name = "rowline";
            line.StrokeThickness = 0.5;
            line.Stroke = Brushes.Black;

            Grid.SetColumn(line, 0);
            Grid.SetRow(line, rowId);
            Grid.SetColumnSpan(line, 5);
            line.X1 = 0;
            line.VerticalAlignment = VerticalAlignment.Bottom;
            line.HorizontalAlignment = HorizontalAlignment.Stretch;


            gridFilterFields.Children.Add(line);


            Filters.Add(filterField);

        }

        public BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private void AddConditionField(FilterField filterField)
        {
            Line line;
            #region ConditionRow

            /************ Create grid row **************/
            RowDefinition row1 = new RowDefinition();
            row1.Height = GridLength.Auto;

            gridFilterFields.RowDefinitions.Add(row1);
            int rId = gridFilterFields.RowDefinitions.Count;
            rId--;
            logger.Info("RowId for condition {0}", rId);
            /************ Create Combo **************/
            ComboBox comboJoin = new ComboBox();
            comboJoin.Tag = filterField;
            comboJoin.Name = "comboJoin";
            comboJoin.Items.Add("or");
            comboJoin.Items.Add("and");

            if(!comboJoin.Items.Contains( filterField.JoinType))
            {
                filterField.JoinType = "or";
            }

            var binding1 = new Binding("JoinType");

            binding1.Source = filterField;
            binding1.Mode = BindingMode.TwoWay;
            binding1.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            binding1.BindsDirectlyToSource = true;

            comboJoin.SetBinding(ComboBox.SelectedItemProperty, binding1);
             
            Grid.SetColumn(comboJoin, 0);
            Grid.SetRow(comboJoin, rId);

            comboJoin.Margin = new Thickness(1, 2, 2, 2);
            comboJoin.HorizontalAlignment = HorizontalAlignment.Left;
            comboJoin.HorizontalContentAlignment = HorizontalAlignment.Left;
            comboJoin.VerticalAlignment = VerticalAlignment.Center;
            comboJoin.VerticalContentAlignment = VerticalAlignment.Center;
            comboJoin.MinWidth = 80;

            gridFilterFields.Children.Add(comboJoin);


            /************ Create Line **************/
            line = new Line();
            line.Tag = filterField;
            line.Name = "rowline";
            line.StrokeThickness = 0.5;
            line.Stroke = Brushes.Black;

            Grid.SetColumn(line, 0);
            Grid.SetRow(line, rId);
            Grid.SetColumnSpan(line, 5);
            line.X1 = 0;
            line.VerticalAlignment = VerticalAlignment.Bottom;
            line.HorizontalAlignment = HorizontalAlignment.Stretch;
            //line.Y1 = 15;
            //line.Y2 = 15;

            gridFilterFields.Children.Add(line);

            #endregion ConditionRow

        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox ctrl = sender as TextBox;
            FilterField ff = ctrl.Tag as FilterField;
            if (IntegerTypes.Contains(ff.FieldType))
            {
                string data = ctrl.Text + e.Text;
                if (!int.TryParse(data, out var r))
                {
                    e.Handled = true;
                }
            }
            else if (FloatTypes.Contains(ff.FieldType))
            {
                string data = ctrl.Text + e.Text;
                if (!decimal.TryParse(data, out var r))
                {
                    e.Handled = true;
                }
            }
            else if (ff.FieldType.ToLower() == "string")
            {

            }
            else if (ff.FieldType == "DateTime")
            {

            }

        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;
            int rowNumber = Grid.GetRow(btn);// gridFilterFields.
            FilterField ff = btn.Tag as FilterField;

            int dataRowId = Grid.GetRow(btn);

            var lst = this.gridFilterFields.Children.OfType<FrameworkElement>().Where(x => x.Tag == ff).ToList();


            var comboJoin = lst.FirstOrDefault(x => x.Name == "comboJoin");
            int conditionRowId = 0;
            if (comboJoin != null)
            {
                conditionRowId = Grid.GetRow(comboJoin);
            }

            var textbox = lst.FirstOrDefault(x => x is TextBox);
            if (textbox != null)
            {
                textbox.PreviewTextInput -= TextBox_PreviewTextInput;
            }

            logger.Info("condition_row_id {0} data_row_id {1}", conditionRowId, dataRowId);

            foreach (var item in lst)
            {
                logger.Info("removing_control {0}, {1}", item.GetType().Name, item.Name);
                this.gridFilterFields.Children.Remove(item);
            }

            if (dataRowId == 1)
            {
                var comboJoin2 = gridFilterFields.Children.OfType<ComboBox>().FirstOrDefault(x => x.Name == "comboJoin");
                if (comboJoin2 != null)
                    gridFilterFields.Children.Remove(comboJoin2);
            }



            Filters.Remove(ff);

            int maxRowId = 0;
            foreach (FrameworkElement child in this.gridFilterFields.Children)
            {
                int controlRowId = Grid.GetRow(child);
                if (controlRowId > dataRowId)
                {
                    int newRowId = controlRowId - 2;
                    if (newRowId < 1)
                        newRowId = 1;

                    logger.Info("adjusting_row_for {0}|{1}|{2}=>{3}", child.GetType().Name, child.Name, controlRowId, newRowId);

                    Grid.SetRow(child, newRowId);
                    maxRowId = newRowId;
                }
            }

            gridFilterFields.RowDefinitions.RemoveAt(gridFilterFields.RowDefinitions.Count - 1);
            if (gridFilterFields.RowDefinitions.Count >= 2)
            {
                gridFilterFields.RowDefinitions.RemoveAt(gridFilterFields.RowDefinitions.Count - 1);
            }
            btn.Click -= BtnDelete_Click;
        }

        private void GridFilterFields_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int rowCount = gridFilterFields.RowDefinitions.Count;

            headerLine.X2 = gridFilterFields.ActualWidth - 1;
            headerLine.Y1 = headerLine.Y2 = tbName.ActualHeight + 2;

            c1line.X1 = c1line.X2 = gridFilterFields.ColumnDefinitions[0].ActualWidth - 1;
            c2line.X1 = c2line.X2 = gridFilterFields.ColumnDefinitions[1].ActualWidth - 1;
            c3line.X1 = c3line.X2 = gridFilterFields.ColumnDefinitions[2].ActualWidth - 1;
            c4line.X1 = c4line.X2 = gridFilterFields.ColumnDefinitions[3].ActualWidth - 1;

            c1line.Y2 = c2line.Y2 = c3line.Y2 = c4line.Y2 = gridFilterFields.ActualHeight - 1;

            Grid.SetRowSpan(c1line, rowCount);
            Grid.SetRowSpan(c2line, rowCount);
            Grid.SetRowSpan(c3line, rowCount);
            Grid.SetRowSpan(c4line, rowCount);


            foreach (var line in gridFilterFields.Children.OfType<Line>())
            {
                if (line.Name == "rowline")
                {
                    line.X2 = gridFilterFields.ActualWidth;
                }
            }


        }


    }
}
