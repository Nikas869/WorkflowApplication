using dBASE.NET;
using JdSuite.Common.FileProcessing;
using JdSuite.Common.Module;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CSVInput
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowClass : Window
    {
        NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        public Field RootSchema { get; set; }
        internal CSVInput.CSVModule moduleReference { get; set; }

        DataFileInfo DataFileInfo { get; set; } = new DataFileInfo();

        private List<EncodingInfo> encodings = Encoding.GetEncodings().OrderBy(x => x.DisplayName).ToList();
        private string FileTypeSelected = "";
        private string FileEncoding = "";
        private bool LoadDropDown = false;
        DataTable TempTable = new DataTable();
        private int paging_PageIndex = 1;
        private int paging_NoOfRecPerPage = 200;
        private int OldPageNumber = 0;
        private enum PagingMode { Next = 2, Previous = 3, First = 1, Last = 4 };

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Message, int wParam, int lParam);

        public const int mouseDown = 0xa1;
        public const int Caption = 0x2;

        private String file_path = "";
        private String file_extension = "";
        //List<MetaDataClass> csv_metadata = new List<MetaDataClass>();
        List<MetaDataClass> Reader_metadata = new List<MetaDataClass>();

        List<GridColumnClass> Reader_metadataGrd = new List<GridColumnClass>();

        //public MetaDataClass metaData;
        //public DataTable dataTable;
        public List<MetaDataClass> metaDataArray = new List<MetaDataClass>();

        public List<GridColumnClass> metaDataArrayGrid = new List<GridColumnClass>();


        public String txtFileName = null;
        public Encoding encode;
        public MetaDataClass metaData = new MetaDataClass();
        public GridColumnClass metaDataGrd = new GridColumnClass();
        public List<MetaDataClass> metaList = new List<MetaDataClass>();
        public DataTable ParseTable = new DataTable();
        public DataTable dataTable = new DataTable();
        List<AllColumns> Columnslist = new List<AllColumns>();
        public int colNumber = 1;
        public string strHeader = null;
        public string[] strHeaders;
        public List<string[]> strRows = new List<string[]>();
        public List<string> strLines = new List<string>();
        public int[] colLengthList;
        public int[] colStartPosList;
        public string[] rowData;
        public bool bChangeFlag = false;  // change column number flag ' no other 
        public int rowLength = 0;
        bool ShowPopup = true;

        // public Dbf dbf = new Dbf();

        public MainWindowClass()
        {
            InitializeComponent();

            this.CmbFileType.SelectionChanged += CmbFileType_SelectionChanged;
        }

        public void SetPropsModuleToWindow(DataFileInfo parameters)
        {
            FileEncoding = this.DataFileInfo.Encoding;
            BindEncodingComboBox();

            parameters.CopyTo(this.DataFileInfo);

            FileTypeSelected = this.DataFileInfo.FileType;
            txtRootArrayName.Text = this.DataFileInfo.RootArrayName;

            file_path = this.DataFileInfo.FilePath;
            file_extension = System.IO.Path.GetExtension(this.DataFileInfo.FilePath);



            txtDelimiter.Text = this.DataFileInfo.Delimiter;
            txtEnclousure.Text = this.DataFileInfo.TextQualifier;

            HeadercheckBox.IsChecked = this.DataFileInfo.FirstRowHasHeader;

            int i = 0;
            foreach (ComboBoxItem item in CmbFileType.Items)
            {
                if ((string)item.Content == this.DataFileInfo.FileType)
                {
                    CmbFileType.SelectedIndex = i;
                    break;

                }
                i++;
            }

            textBoxFrom.Text = this.DataFileInfo.ShowRecordCount;
            txtInputFile.Text = this.DataFileInfo.FilePath;

        }

        public void SetPropsWindowToModule()
        {
            this.DataFileInfo.FileType = FileTypeSelected;
            this.DataFileInfo.RootArrayName = txtRootArrayName.Text;
            this.DataFileInfo.FilePath = txtInputFile.Text;
            this.DataFileInfo.Encoding = FileEncoding;
            this.DataFileInfo.Delimiter = txtDelimiter.Text;
            this.DataFileInfo.TextQualifier = txtEnclousure.Text;
            this.DataFileInfo.FirstRowHasHeader = false;

            if (HeadercheckBox.IsChecked.HasValue)
                this.DataFileInfo.FirstRowHasHeader = HeadercheckBox.IsChecked.Value;

            this.DataFileInfo.ShowRecordCount = textBoxFrom.Text;

            this.DataFileInfo.CopyTo(this.moduleReference.DataFileInfo);
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (cbxEncoding.Items.Count == 0)
                BindEncodingComboBox();

            // grid margin
            Thickness margindataGrid = dataGrid.Margin;
            margindataGrid.Top = 156;
            dataGrid.Margin = margindataGrid;
            dataGridColumns.Visibility = Visibility.Hidden;
            schemeHeaderLabel.Visibility = Visibility.Hidden;
            dataGridHidden.Visibility = Visibility.Hidden;
            btnFirst.Visibility = Visibility.Hidden;
            btnPrev.Visibility = Visibility.Hidden;
            lblPageNumber.Visibility = Visibility.Hidden;
            btnNext.Visibility = Visibility.Hidden;
            btnLast.Visibility = Visibility.Hidden;
            lblPagingInfo.Visibility = Visibility.Hidden;
            dataGrid.Visibility = Visibility.Hidden;

            // DataTable mdt = new DataTable();
            // mdt.Columns.Add("Name");
            //// mdt.Columns.Add("DataType");
            // //mdt.Columns.Add("strFormat");
            // mdt.Columns.Add("StartPos");
            // mdt.Columns.Add("Length");
            //// mdt.Columns.Add("Precision");
            // mdt.Columns.Add("TrimSpaces");
            // dataGridColumns.ItemsSource = mdt.DefaultView;
            //addCombo();
            /*
            if (File.Exists(@"HeaderData.flo"))
            {
                try
                {

                    metaDataArrayGrid.Clear();
                    DataSet ds = new DataSet();
                    ds.ReadXml(@"HeaderData.flo");
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        metaDataGrd = new GridColumnClass();
                        metaDataGrd.Name = row[0].ToString();
                        metaDataGrd.DataType = (EnumDataType)Enum.Parse(typeof(EnumDataType), row[1].ToString());
                        if (row.ItemArray.Length == 3)
                        {
                            metaDataGrd.TrimSpaces = (EnumTrimType)Enum.Parse(typeof(EnumTrimType), row[2].ToString());
                        }
                        else
                        {
                            metaDataGrd.StartPos = Convert.ToInt32(row[2]);
                            metaDataGrd.Length = Convert.ToInt32(row[3]);
                            metaDataGrd.TrimSpaces = (EnumTrimType)Enum.Parse(typeof(EnumTrimType), row[4].ToString());
                        }
                        metaDataArrayGrid.Add(metaDataGrd);
                        LoadDropDown = true;
                    }

                    dataGridColumns.ItemsSource = metaDataArrayGrid;
                    dataGridColumns.SelectedValue = 1;
                    // BindDropDown(ds);


                }
                catch (Exception ex)
                {

                }
            }
            else
            */
            {
                dataGridColumns.ItemsSource = new List<GridColumnClass>();

                dataGridColumns.Columns[2].Visibility = Visibility.Hidden;
                dataGridColumns.Columns[3].Visibility = Visibility.Hidden;
            }

            foreach (var column in dataGridColumns.Columns)
            {

                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                //column.HeaderStyle = Color.FromRgb()
            }

            if (File.Exists(this.DataFileInfo.FilePath))
                HeadercheckBox_Click(null, null);

            // LoadTempData();

        }

        private void BindEncodingComboBox()
        {
            foreach (var encoding in encodings)
            {
                cbxEncoding.Items.Add(new ComboBoxItem()
                {
                    Content = encoding.DisplayName
                });
            }

            cbxEncoding.SelectedIndex = Array.IndexOf(encodings.Select(enc => enc.CodePage).ToArray(), Encoding.ASCII.CodePage);
        }

        private void BtnFileBrower_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            switch (CmbFileType.SelectedIndex)
            {
                case 1:   // CSV
                    openFileDlg.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    break;
                case 2:   // DBF
                    openFileDlg.Filter = "DBF files (*.dbf)|*.dbf|All files (*.*)|*.*";
                    break;
                case 3:   // Text
                    openFileDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    break;
                case 4:   // ALL
                    openFileDlg.Filter = "All files (*.*)|*.*";
                    break;

            }

            //openFileDlg.Filter = "CSV files (*.csv)|*.csv|DBF files (*.dbf)|*.dbf|All files (*.*)|*.*";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();

            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                file_path = openFileDlg.FileName;
                file_extension = System.IO.Path.GetExtension(file_path);
                txtInputFile.Text = openFileDlg.FileName;
                //GetCVSData(openFileDlg.FileName);

            }
        }

        /* csv to DataTable */
        private DataTable DataTabletFromCSVFile(string csv_file_path, Encoding encode = null)
        {
            ParseTable.Clear();
            ParseTable.Columns.Clear();
            metaDataArray.Clear();
            //DataTable csvData = new DataTable();
            try
            {
                string[] FileColumns = null;
                string Delimiter = "";
                using (TextFieldParser csvReaderCount = new TextFieldParser(csv_file_path, encode))
                {
                    Delimiter = txtDelimiter.Text;
                    if (string.IsNullOrEmpty(Delimiter))
                    {
                        MessageBox.Show("Invalid Delimiter");

                        return ParseTable;
                    }
                    csvReaderCount.SetDelimiters(new string[] { Delimiter });
                    csvReaderCount.HasFieldsEnclosedInQuotes = true;
                    FileColumns = csvReaderCount.ReadFields();
                }
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path, encode))
                {
                    csvReader.SetDelimiters(new string[] { Delimiter });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    if (HeadercheckBox.IsChecked == true)
                    {
                        string[] colFields = csvReader.ReadFields();
                        //List<string> metaDataList = new List<string>();

                        foreach (string column in colFields)
                        {


                            DataColumn dataColumn = new DataColumn(column);
                            dataColumn.AllowDBNull = true;
                            ParseTable.Columns.Add(dataColumn);

                            /* Make csv file metaData to ArrayList */
                            metaData = new MetaDataClass();
                            metaData.Name = column;
                            metaData.OpenFileType = FileTypeSelected;
                            metaData.RootArrayName = txtRootArrayName.Text;
                            metaData.InputFile = txtInputFile.Text;
                            if (file_extension != ".dbf")
                            {
                                metaData.FileEncoding = FileEncoding;
                            }
                            metaDataArray.Add(metaData);
                        }
                    }
                    else
                    {
                        if (dataGridColumns != null)
                        {

                            metaDataArray.Clear();
                            foreach (GridColumnClass dr in dataGridColumns.ItemsSource)
                            {
                                string IsNotNull = dr.Name;
                                if (!string.IsNullOrEmpty(IsNotNull))
                                {
                                    DataColumn dataColumn = new DataColumn(dr.Name);
                                    dataColumn.AllowDBNull = true;
                                    ParseTable.Columns.Add(dataColumn);
                                    string DType = ((int)dr.DataType).ToString();
                                    if (!string.IsNullOrEmpty(DType))
                                    {
                                        if (DType == "1")
                                        {
                                            DType = "String";
                                        }
                                        else if (DType == "2")
                                        {
                                            DType = "Date";
                                        }
                                        else if (DType == "3")
                                        {
                                            DType = "Boolean";
                                        }
                                        else if (DType == "4")
                                        {
                                            DType = "Integer";
                                        }
                                        else if (DType == "5")
                                        {
                                            DType = "LongInteger";
                                        }
                                        //  metaData.DataTypeStr = DType;
                                    }
                                    else
                                    {
                                        DType = "String";
                                    }

                                    metaData = new MetaDataClass();
                                    metaData.Name = dr.Name;
                                    metaData.DataTypeStr = DType;
                                    metaData.OpenFileType = FileTypeSelected;
                                    metaData.RootArrayName = txtRootArrayName.Text;
                                    metaData.InputFile = txtInputFile.Text;
                                    if (file_extension != ".dbf")
                                    {
                                        metaData.FileEncoding = FileEncoding;
                                    }
                                    metaDataArray.Add(metaData);
                                }
                            }

                            while (ParseTable.Columns.Count < FileColumns.Length)
                            {
                                ParseTable.Columns.Add(new DataColumn(""));
                            }
                        }
                    }

                    Reader_metadata = metaDataArray;
                    int RowCount = 0;
                    while (!csvReader.EndOfData)
                    {
                        if (RowCount == paging_NoOfRecPerPage)
                        {
                            break;
                        }
                        string[] fieldData = csvReader.ReadFields().Take(ParseTable.Columns.Count).ToArray();
                        // string[] fieldDataStr1 = csvReader.ReadFields();
                        // string fieldDataStr = string.Join(",",fieldDataStr1);
                        //// string[] fieldData = fieldDataStr.Split(',');
                        // string[] fieldData = Regex.Split(fieldDataStr, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        ParseTable.Rows.Add(fieldData);
                        RowCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Input array is longer than the number of columns in this table.")
                {
                    MessageBox.Show("Invalid Delimiter");
                    return ParseTable;
                }
                //  MessageBox.Show(ex + "\n DataTabletFromCSVFile Exception Error!");
                MessageBox.Show(ex.Message);
            }
            return ParseTable;
        }


        /* dbf to DataTable */
        public DataTable DataTabletFromDbfFile(string dbf_file_path, Encoding encode = null)
        {

            //DataTable dbfData = new DataTable();
            ParseTable.Clear();
            ParseTable.Columns.Clear();
            metaDataArray.Clear();
            Dbf dbf = new Dbf();

            try
            {
                dbf.Read(dbf_file_path);

                for (int i = 0; i < dbf.Fields.Count; i++)
                {
                    DataColumn dataColumn = new DataColumn(dbf.Fields[i].Name);
                    dataColumn.AllowDBNull = true;
                    ParseTable.Columns.Add(dbf.Fields[i].Name);

                    metaData = new MetaDataClass();
                    metaData.Name = dbf.Fields[i].Name;
                    metaData.OpenFileType = FileTypeSelected;
                    metaData.RootArrayName = txtRootArrayName.Text;
                    metaData.InputFile = txtInputFile.Text;
                    if (file_extension != ".dbf")
                    {
                        metaData.FileEncoding = FileEncoding;
                    }
                    metaDataArray.Add(metaData);
                }

                Reader_metadata = metaDataArray;
                int RowCount = 0;
                for (int i = 0; i < dbf.Records.Count; i++)
                {
                    if (RowCount == paging_NoOfRecPerPage)
                    {
                        break;
                    }
                    string[] cellArray = new string[dbf.Records[i].Data.Count];
                    for (int j = 0; j < dbf.Records[i].Data.Count; j++)
                    {
                        string cell = null;
                        if (dbf.Records[i].Data[j] == null)
                        {
                            cell = "null";
                        }
                        else
                        {
                            cell = dbf.Records[i].Data[j].ToString();
                        }

                        cellArray[j] = cell;
                    }
                    ParseTable.Rows.Add(cellArray);
                    RowCount++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unrecognized .dbf file format, please correct! " + ex.Message);
                // MessageBox.Show(ex + "\n Exception Error !");
            }

            return ParseTable;
        }

        /* Text file to DataTable */
        private DataTable DataTabeletFromTxtFile(string txt_file_path, Encoding encode = null)
        {

            DataTable TxtData = new DataTable();
            //ParserWindow parserWin = new ParserWindow();
            //parserWin.txtFileName = txt_file_path;
            //parserWin.encode = encode;
            //var result = parserWin.ShowDialog();

            //if(result == true)
            //{               
            TxtData = TempTable;

            //dataGrid.ItemsSource = TxtData.DefaultView;
            metaDataArray.Clear();

            for (int col = 0; col < strHeaders.Count(); col++)
            {
                metaData = new MetaDataClass();
                metaData.Name = strHeaders[col];
                metaData.StartPos = colStartPosList[col];
                metaData.Length = colLengthList[col];
                metaData.OpenFileType = FileTypeSelected;
                metaData.RootArrayName = txtRootArrayName.Text;
                metaData.InputFile = txtInputFile.Text;
                if (file_extension != ".dbf")
                {
                    metaData.FileEncoding = FileEncoding;
                }
                metaDataArray.Add(metaData);
            }

            Reader_metadata = metaDataArray;
            //}

            return TxtData;
        }

        /* Get Preview Button Click Event */
        private void CVSViewBtn_Click(object sender, RoutedEventArgs e)
        {
            encode = encodings[cbxEncoding.SelectedIndex].GetEncoding();

            if (file_extension == ".dbf")
            {
                dataTable = DataTabletFromDbfFile(file_path, encode);
                ListProducts(0);
                // dataGrid.ItemsSource = dataTable.DefaultView;
            }
            else if (file_extension == ".csv")
            {
                dataTable = DataTabletFromCSVFile(file_path, encode);
                ListProducts(0);
                //dataGrid.ItemsSource = dataTable.DefaultView;
            }
            else if (file_extension == ".txt")
            {
                //dataTable.Clear();
                PreviewTextFile(file_path, encode);  // Preview File Contents
                TabItem_ContextMenuOpening(null, null);
                DataTabeletFromTxtFile(file_path, encode);
                //dataGrid.ItemsSource = dataTable.DefaultView;
            }
            else
            {
                MessageBox.Show("Please Choose the Correct file like '.csv' or '.dbf'");
            }
        }
        public void addCombo()
        {
            //ADD COLUMNS
            DataGridComboBoxColumn ss = new DataGridComboBoxColumn();
            ss.Header = "DataType";
            // ss.Name
            List<string> list = new List<string>();
            list.Add("Item 1");
            list.Add("Item 2");
            // ss.res = new Binding(list);
            ss.ItemsSource = list;
            ss.SelectedItemBinding = new Binding("test");
            ss.Width = 50;
            //ss.EditingElementStyle = style;

            //dataGridColumns.Columns.Add(ss);
            dataGridColumns.Columns.Insert(1, ss);

        }


        private void BindDropDown()
        {
            if (File.Exists(@"HeaderData.flo"))
            {
                DataSet ds = new DataSet();
                ds.ReadXml(@"HeaderData.flo");
                int i = 0;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    DataGridCell cell = GetCell(i, 1, dataGridColumns);
                    DataGridCell cell2 = GetCell(i, 4, dataGridColumns);
                    //cell.Background = new SolidColorBrush(Colors.Red);
                    string rowValue = row[1].ToString();
                    string rowTrim = "";
                    if (row.ItemArray.Length == 3)
                    {
                        rowTrim = row[2].ToString();
                    }
                    else
                    {
                        rowTrim = row[4].ToString();
                    }

                    if (!string.IsNullOrEmpty(rowValue))
                    {
                        if (rowValue == "1")
                        {
                            cell.Content = EnumDataType.String;
                        }
                        else if (rowValue == "2")
                        {
                            cell.Content = EnumDataType.Date;
                        }
                        else if (rowValue == "3")
                        {
                            cell.Content = EnumDataType.Boolean;
                        }
                        else if (rowValue == "4")
                        {
                            cell.Content = EnumDataType.Integer;
                        }
                        else if (rowValue == "5")
                        {
                            cell.Content = EnumDataType.LongNumber;
                        }
                    }

                    // for trim
                    if (!string.IsNullOrEmpty(rowTrim))
                    {
                        if (rowTrim == "0")
                        {
                            cell2.Content = EnumTrimType.None;
                        }
                        else if (rowTrim == "1")
                        {
                            cell2.Content = EnumTrimType.Left;
                        }
                        else if (rowTrim == "2")
                        {
                            cell2.Content = EnumTrimType.Right;
                        }
                        else if (rowTrim == "3")
                        {
                            cell2.Content = EnumTrimType.Both;
                        }

                    }

                    i++;
                }
            }

        }
        private void LoadTempData()
        {

            if (File.Exists(@"TableData.flo"))
            {
                DataSet ds = new DataSet();
                ds.ReadXml(@"TableData.flo");
                dataGrid.DataContext = ds.Tables[0];

            }

            if (File.Exists(@"AppDetails.flo"))
            {
                DataSet ds = new DataSet();
                ds.ReadXml(@"AppDetails.flo");
                foreach (DataRow row in ds.Tables[0].Rows)
                {

                    CmbFileType.SelectedValue = row[0].ToString();
                    txtRootArrayName.Text = row[1].ToString();
                    cbxEncoding.SelectedValue = row[3].ToString();
                    txtDelimiter.Text = row[4].ToString();
                    txtEnclousure.Text = row[5].ToString();
                    file_path = row[2].ToString();
                    file_extension = System.IO.Path.GetExtension(file_path);
                    txtInputFile.Text = row[2].ToString();
                    if (row[6].ToString() == "True")
                    {
                        HeadercheckBox.IsChecked = true;
                        dataGridColumns.IsReadOnly = true;
                    }
                    string Filetype = row[0].ToString();
                    if (Filetype == "Fixed Width")
                    {
                        dataGridColumns.IsReadOnly = false;
                    }
                }

            }
        }

        //public class AddItems
        //{
        //    public string strColumnName { get; set; }
        //    public string eDataType { get; set; }
        //    public string strFormat { get; set; }
        //    public string iStartPos { get; set; }
        //    public string iLength { get; set; }
        //    public string Precision { get; set; }
        //    public string eTrimType { get; set; }

        //}

        public void PreviewTextFile(String fileName, Encoding encode)
        {
            //DataTable TempTable = new DataTable();
            ParseTable.Clear();
            ParseTable.Columns.Clear();
            using (StreamReader txtReader = new StreamReader(fileName, encode))
            {
                string sLine = "";
                //TempTable.reset();
                //ArrayList arrText = new ArrayList();
                //sLine = txtReader.ReadLine();
                //sLine = GetHeader(fileName, encode);
                //string[] lines = File.ReadAllLines(fileName);
                //int cou = 0;
                //while (txtReader.ReadLine() != sLine)
                //{
                //    if (lines.Length <= cou)
                //    {
                //        break;
                //    }
                //    txtReader.ReadLine();
                //    cou++;
                //}
                //strHeader = sLine;
                //sLine = sLine.Replace(".", "");

                //sLine = sLine.Replace(" ", "");
                //TempTable.Columns.Add("dfas df");
                ParseTable.Columns.Add("`");
                int RowCount = 0;
                while (sLine != null)
                {
                    if (RowCount == paging_NoOfRecPerPage)
                    {
                        break;
                    }
                    sLine = txtReader.ReadLine();
                    ParseTable.Rows.Add(sLine);
                    //TempTable.Rows.Add(sLine);
                    //arrText.Add(sLine);
                    RowCount++;
                }

                txtReader.Close();
                // dataGrid.ItemsSource = TempTable.DefaultView;
                ListProducts(0);
            }
        }

        public string GetHeader(String fileName, Encoding encode)
        {
            string HeaderLine = "";
            string[] lines = File.ReadAllLines(fileName);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("------"))
                {

                    return HeaderLine = lines[i - 1];
                }
            }
            return HeaderLine;
        }

        /* Get Data Field Button Click Event */
        private void GetMetaDataFieldBtn_Click(object sender, RoutedEventArgs e)
        {
            if (file_extension == ".csv")
            {
                DataTable mdt = ConvertArrayListToDataTable(new ArrayList(Reader_metadata));
                metaDataGrid.ItemsSource = mdt.DefaultView;
            }
            else if (file_extension == ".dbf")
            {
                DataTable mdt = ConvertArrayListToDataTable(new ArrayList(Reader_metadata));
                metaDataGrid.ItemsSource = mdt.DefaultView;
            }
            else if (file_extension == ".txt")
            {
                DataTable mdt = ConvertArrayListToDataTable(new ArrayList(Reader_metadata));
                metaDataGrid.ItemsSource = mdt.DefaultView;
            }
            else
            {
                MessageBox.Show("Please Choose the a file like '*.csv',  '*.dbf' or PlannText file '*.txt' ");
            }
        }

        public DataTable DataTabletFromDbfFileLength(string dbf_file_path, Encoding encode = null)
        {

            DataTable dbfData = new DataTable();
            Dbf dbf = new Dbf();

            try
            {
                dbf.Read(dbf_file_path);
                dbfData.Columns.Add("");


                Reader_metadata = metaDataArray;

                for (int i = 0; i < dbf.Records.Count; i++)
                {
                    //string[] cellArray = new string[dbf.Records[i].Data.Count];
                    string str = "";
                    for (int j = 0; j < dbf.Records[i].Data.Count; j++)
                    {
                        string cell = null;
                        if (dbf.Records[i].Data[j] == null)
                        {
                            cell = "null";
                        }
                        else
                        {
                            cell = dbf.Records[i].Data[j].ToString();
                        }
                        str = string.Concat(str, cell);
                        // cellArray[j] = cell;
                    }
                    dbfData.Rows.Add(str);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + "\n Exception Error !");
            }

            return dbfData;
        }

        private void ApplyEdit()
        {

            string sLine = "";
            ParseTable.Clear();
            ParseTable.Columns.Clear();
            bool HasRows = false;
            if (dataGridColumns != null)
            {
                metaDataArray.Clear();
                foreach (GridColumnClass dr in dataGridColumns.ItemsSource)
                {
                    string IsNotNull = dr.Name.ToString();
                    if (!string.IsNullOrEmpty(IsNotNull))
                    {
                        ParseTable.Columns.Add(dr.Name.ToString());
                        HasRows = true;
                        // add in array to parse
                        string DType = ((int)dr.DataType).ToString();
                        string start = dr.StartPos.ToString();
                        string length = dr.Length.ToString();
                        if (!string.IsNullOrEmpty(IsNotNull) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(length))
                        {
                            if (IsDigitsOnly(dr.StartPos.ToString()) == true && IsDigitsOnly(dr.Length.ToString()) == true)
                            {

                                int startIndex = dr.StartPos.Value;
                                int Length = dr.Length.Value;
                                metaData = new MetaDataClass();
                                metaData.Name = dr.Name.ToString();
                                metaData.StartPos = startIndex;
                                metaData.Length = Length;
                                metaData.OpenFileType = FileTypeSelected;
                                metaData.RootArrayName = txtRootArrayName.Text;
                                metaData.InputFile = txtInputFile.Text;
                                if (!string.IsNullOrEmpty(DType))
                                {
                                    if (DType == "1")
                                    {
                                        DType = "String";
                                    }
                                    else if (DType == "2")
                                    {
                                        DType = "Date";
                                    }
                                    else if (DType == "3")
                                    {
                                        DType = "Boolean";
                                    }
                                    else if (DType == "4")
                                    {
                                        DType = "Integer";
                                    }
                                    else if (DType == "5")
                                    {
                                        DType = "LongInteger";
                                    }
                                    metaData.DataTypeStr = DType;
                                }
                                if (file_extension != ".dbf")
                                {
                                    metaData.FileEncoding = FileEncoding;
                                }
                                metaDataArray.Add(metaData);
                            }
                        }
                        //MessageBox.Show(dr[0].ToString());
                    }
                }
            }

            if (HasRows)
            {
                using (StreamReader txtReader = new StreamReader(file_path, encode))
                {
                    int RowsCount = 0;

                    while (sLine != null)
                    {

                        if (RowsCount == paging_NoOfRecPerPage)
                        {
                            break;
                        }
                        // sLine = drLines[0].ToString();//txtReader.ReadLine();
                        // sLine = sLine.Replace(" | ", "");
                        sLine = txtReader.ReadLine();
                        DataRow rowData = ParseTable.NewRow();
                        if (sLine != null)
                        {
                            bool addRow = false;
                            foreach (GridColumnClass dr in dataGridColumns.ItemsSource)
                            {
                                string col = dr.Name.ToString();
                                string start = dr.StartPos.ToString();
                                string length = dr.Length.ToString();
                                if (!string.IsNullOrEmpty(col) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(length))
                                {
                                    if (IsDigitsOnly(dr.StartPos.ToString()) == true && IsDigitsOnly(dr.Length.ToString()) == true)
                                    {
                                        int startIndex = dr.StartPos.Value;
                                        int Length = dr.Length.Value;
                                        int LineLength = sLine.Length;
                                        int Total = LineLength - startIndex;
                                        if (LineLength < Length)
                                        {
                                            Length = LineLength;
                                        }
                                        if (Total < Length)
                                        {
                                            if (Total > 0)
                                            {
                                                Length = Total;
                                            }
                                            else
                                            {
                                                Length = LineLength;
                                            }
                                        }
                                        if (startIndex >= LineLength)
                                        {
                                            startIndex = 0;
                                        }
                                        //  if (startIndex < Length)
                                        //  {
                                        string sub = sLine.Substring(startIndex, Length);

                                        rowData[dr.Name.ToString()] = sub;
                                        addRow = true;
                                        // }

                                    }
                                }
                            }
                            if (addRow)
                            {
                                ParseTable.Rows.Add(rowData);
                                RowsCount++;
                            }
                        }
                    }

                    txtReader.Close();
                    dataGridHidden.ItemsSource = ParseTable.DefaultView;
                    //ListProducts(0);
                }

            }
        }
        private void setColumns()
        {
            List<int> startLength = new List<int>();
            Columnslist.Clear();
            string FileHeader = GetHeader(file_path, Encoding.ASCII);
            int FinalStart = 0;
            int FinalLength = 0;
            bool OneColumn = false;
            if (dataGridColumns != null && dataGridColumns.ItemsSource != null)
            {
                metaDataArray.Clear();
                int RowsCount = 0;
                int BeforeLength = 0;

                foreach (GridColumnClass dr in dataGridColumns.ItemsSource)
                {
                    string IsNotNull = dr.Name.ToString();

                    if (!string.IsNullOrEmpty(IsNotNull))
                    {

                        string start = dr.StartPos.ToString();
                        string length = dr.Length.ToString();

                        if (!string.IsNullOrEmpty(IsNotNull) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(length))
                        {
                            if (IsDigitsOnly(dr.StartPos.ToString()) == true && IsDigitsOnly(dr.Length.ToString()) == true)
                            {
                                int istart = Convert.ToInt32(start);
                                int ilength = Convert.ToInt32(length);
                                int LineLength = FileHeader.Length;
                                int Total = LineLength - istart;
                                if (LineLength < ilength)
                                {
                                    ilength = LineLength;
                                }
                                if (Total < ilength)
                                {
                                    if (Total > 0)
                                    {
                                        ilength = Total;
                                    }
                                    else
                                    {
                                        ilength = LineLength;
                                    }
                                }
                                if (istart >= LineLength)
                                {
                                    istart = 0;
                                }
                                // if (startLength.Max() < istart)
                                // {

                                if (dataGridColumns.Items.Count == 2)
                                {
                                    if (istart == 1 || istart == 0)
                                    {

                                        ParseTable.Columns.Add(dr.Name.ToString());
                                        AllColumns addColumns = new AllColumns();
                                        addColumns.ColumnName = dr.Name.ToString();
                                        addColumns.Starting = istart;
                                        addColumns.Length = ilength;
                                        Columnslist.Add(addColumns);

                                        int allLength2 = istart + ilength;
                                        int Remain2 = FileHeader.Length - allLength2;

                                        ParseTable.Columns.Add("`");
                                        AllColumns addColumns2 = new AllColumns();
                                        addColumns2.ColumnName = ("`");
                                        addColumns2.Starting = allLength2;
                                        addColumns2.Length = Remain2;
                                        Columnslist.Add(addColumns2);
                                        OneColumn = true;
                                        break;
                                    }
                                    else
                                    {
                                        string beforeRow = FileHeader.Substring(0, istart);
                                        if (beforeRow.Length > 0)
                                        {

                                            ParseTable.Columns.Add("`");
                                            AllColumns addColumns1 = new AllColumns();
                                            addColumns1.ColumnName = "`";
                                            addColumns1.Starting = 0;
                                            addColumns1.Length = istart;
                                            Columnslist.Add(addColumns1);
                                        }
                                        ParseTable.Columns.Add(dr.Name.ToString());
                                        AllColumns addColumns = new AllColumns();
                                        addColumns.ColumnName = dr.Name.ToString();
                                        addColumns.Starting = istart;
                                        addColumns.Length = ilength;
                                        Columnslist.Add(addColumns);
                                    }
                                    OneColumn = true;
                                    break;

                                }
                                else
                                {
                                    if (RowsCount == dataGridColumns.Items.Count)
                                    {
                                        break;
                                    }
                                    string beforeRow = "";
                                    if (BeforeLength == 0)
                                    {
                                        beforeRow = FileHeader.Substring(0, istart);
                                    }
                                    else
                                    {
                                        if (istart < BeforeLength)
                                        {
                                            var row = dataGridColumns.ItemContainerGenerator.ContainerFromItem(dr) as DataGridRow;
                                            var cell = dataGridColumns.Columns[2].GetCellContent(row);

                                            if (cell is TextBlock)
                                            {
                                                TextBlock cellContent = cell as TextBlock;
                                                cellContent.Foreground = new SolidColorBrush(Colors.Red);
                                            }
                                        }
                                        if (istart == BeforeLength)
                                        {
                                            beforeRow = "";
                                        }
                                        if (istart > BeforeLength)
                                        {
                                            beforeRow = FileHeader.Substring(BeforeLength, istart - BeforeLength);
                                        }

                                    }
                                    if (beforeRow.Length > 0)
                                    {

                                        ParseTable.Columns.Add("");
                                        int count = ParseTable.Columns.Count;
                                        var lastColumn = ParseTable.Columns[count - 1];
                                        string columnname = lastColumn.ColumnName;
                                        AllColumns addColumns4 = new AllColumns();
                                        addColumns4.ColumnName = columnname;
                                        addColumns4.Starting = BeforeLength;
                                        addColumns4.Length = istart - BeforeLength;
                                        Columnslist.Add(addColumns4);
                                    }

                                    ParseTable.Columns.Add(dr.Name.ToString());
                                    AllColumns addColumns5 = new AllColumns();
                                    addColumns5.ColumnName = dr.Name.ToString();
                                    addColumns5.Starting = istart;
                                    addColumns5.Length = ilength;
                                    Columnslist.Add(addColumns5);
                                    RowsCount++;
                                    BeforeLength = ilength + istart;
                                    FinalStart = istart;
                                    FinalLength = ilength;
                                }
                                //  }
                                //  else
                                //    {
                                //        MessageBox.Show("Confilcts range");
                                //   }

                            }
                        }
                    }
                }
            }

            // return startLength;
            //bool AddLastCol = false;
            if (!OneColumn)
            {
                int allLength = FinalStart + FinalLength;
                int Remain = FileHeader.Length - allLength;
                if (Remain > 0)
                {
                    ParseTable.Columns.Add("");
                    int count = ParseTable.Columns.Count;
                    var lastColumn = ParseTable.Columns[count - 1];
                    string columnname = lastColumn.ColumnName;
                    AllColumns addColumns6 = new AllColumns();
                    addColumns6.ColumnName = columnname;
                    addColumns6.Starting = allLength;
                    addColumns6.Length = Remain;
                    Columnslist.Add(addColumns6);

                }
            }

        }

        public class AllColumns
        {
            public string ColumnName { get; set; }
            public int Starting { get; set; }
            public int Length { get; set; }
        }

        private void MetaDataSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(cbxEncoding.Text);
            if (File.Exists(file_path))
            {
                dataGrid.Visibility = Visibility.Visible;

                encode = encodings[cbxEncoding.SelectedIndex].GetEncoding();

                if (file_extension == ".dbf")
                {
                    //LoadDBF(file_path, encode);
                    CVSViewBtn_Click(sender, e);
                    return;
                }
                if (file_extension == ".csv")
                {
                    if (HeadercheckBox.IsChecked == false && dataGridColumns.Items.Count == 1)
                    {
                        //ReadXcelNoColumns();
                        //return;
                    }
                    CVSViewBtn_Click(sender, e);
                    return;
                }
                // CVSViewBtn_Click(sender, e);
                // return;
                //DataTable LoadedFile = new DataTable();

                if (encode == null)
                {
                    return;
                }

                using (StreamReader txtReader = new StreamReader(file_path, encode))
                {
                    string sLine = "";
                    ParseTable.Clear();
                    ParseTable.Columns.Clear();
                    bool HasRows = false;
                    if (dataGridColumns != null && dataGridColumns.ItemsSource != null)
                    {
                        setColumns();
                        metaDataArray.Clear();
                        foreach (GridColumnClass dr in dataGridColumns.ItemsSource)
                        {
                            string IsNotNull = dr.Name.ToString();
                            if (!string.IsNullOrEmpty(IsNotNull))
                            {
                                //ParseTable.Columns.Add(dr[0].ToString());
                                HasRows = true;
                                // add in array to parse
                                string DType = ((int)dr.DataType).ToString();
                                string start = dr.StartPos.ToString();
                                string length = dr.Length.ToString();
                                if (!string.IsNullOrEmpty(IsNotNull) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(length))
                                {
                                    if (IsDigitsOnly(dr.StartPos.ToString()) == true && IsDigitsOnly(dr.Length.ToString()) == true)
                                    {

                                        int startIndex = dr.StartPos.Value;
                                        int Length = dr.Length.Value;
                                        metaData = new MetaDataClass();
                                        metaData.Name = dr.Name.ToString();
                                        metaData.StartPos = startIndex;
                                        metaData.Length = Length;
                                        metaData.OpenFileType = FileTypeSelected;
                                        metaData.RootArrayName = txtRootArrayName.Text;
                                        metaData.InputFile = txtInputFile.Text;
                                        //if (IsDigitsOnly(DType))
                                        //{
                                        //    metaData.DataType = Enum.Parse(DType);
                                        //}
                                        if (!string.IsNullOrEmpty(DType))
                                        {
                                            if (DType == "1")
                                            {
                                                DType = "String";
                                            }
                                            else if (DType == "2")
                                            {
                                                DType = "Date";
                                            }
                                            else if (DType == "3")
                                            {
                                                DType = "Boolean";
                                            }
                                            else if (DType == "4")
                                            {
                                                DType = "Integer";
                                            }
                                            else if (DType == "5")
                                            {
                                                DType = "LongInteger";
                                            }
                                            metaData.DataTypeStr = DType;
                                        }
                                        else
                                        {
                                            DType = "String";
                                        }
                                        if (file_extension != ".dbf")
                                        {
                                            metaData.FileEncoding = FileEncoding;
                                        }
                                        metaDataArray.Add(metaData);
                                    }
                                }
                                //MessageBox.Show(dr[0].ToString());
                            }
                        }
                    }

                    if (HasRows)
                    {
                        int RowsCount = 0;

                        while (sLine != null)
                        {

                            if (RowsCount == paging_NoOfRecPerPage)
                            {
                                break;
                            }
                            sLine = txtReader.ReadLine();
                            DataRow rowData = ParseTable.NewRow();
                            if (sLine != null)
                            {
                                bool addRow = false;
                                foreach (var ColumnItmes in Columnslist)
                                {

                                    //}
                                    //foreach (System.Data.DataRowView dr in dataGridColumns.ItemsSource)
                                    //{
                                    string col = ColumnItmes.ColumnName;//dr[0].ToString();
                                    string start = ColumnItmes.Starting.ToString();//dr[2].ToString();
                                    string length = ColumnItmes.Length.ToString();//dr[3].ToString();
                                    if (!string.IsNullOrEmpty(col) && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(length))
                                    {
                                        if (IsDigitsOnly(start) == true && IsDigitsOnly(length) == true)
                                        {
                                            int startIndex = Convert.ToInt32(start);
                                            int Length = Convert.ToInt32(length);
                                            int LineLength = sLine.Length;
                                            int Total = LineLength - startIndex;
                                            if (LineLength < Length)
                                            {
                                                Length = LineLength;
                                            }
                                            if (Total < Length)
                                            {
                                                if (Total > 0)
                                                {
                                                    Length = Total;
                                                }
                                                else
                                                {
                                                    Length = LineLength;
                                                }
                                            }
                                            if (startIndex >= LineLength)
                                            {
                                                startIndex = 0;
                                            }
                                            //  if (startIndex < Length)
                                            //  {
                                            string sub = sLine.Substring(startIndex, Length);

                                            rowData[col] = sub;
                                            addRow = true;
                                            // }

                                        }
                                    }
                                }
                                if (addRow)
                                {
                                    ParseTable.Rows.Add(rowData);
                                    RowsCount++;
                                }
                            }
                        }

                        dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
                        //dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
                        // dataGrid.VerticalGridLinesBrush = 
                        txtReader.Close();
                        //dataGridHidden.ItemsSource = ParseTable.DefaultView;
                        ListProducts(0);

                    }
                    else
                    {
                        CVSViewBtn_Click(sender, e);
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid File Path");
            }
        }

        private void ReadXcelNoColumns()
        {
            //DataTable TempTable = new DataTable();
            dataGrid.HeadersVisibility = DataGridHeadersVisibility.All;
            ParseTable.Clear();
            ParseTable.Columns.Clear();
            using (StreamReader txtReader = new StreamReader(file_path, encode))
            {
                string sLine = "";
                //TempTable.reset();
                //ArrayList arrText = new ArrayList();
                //sLine = txtReader.ReadLine();
                //sLine = GetHeader(file_path, encode);
                //string[] lines = File.ReadAllLines(file_path);
                //int cou = 0;
                //while (txtReader.ReadLine() != sLine)
                //{
                //    if (lines.Length <= cou)
                //    {
                //        break;
                //    }
                //    txtReader.ReadLine();
                //    cou++;
                //}
                //strHeader = sLine;
                //sLine = sLine.Replace(".", "");

                //sLine = sLine.Replace(" ", "");
                //TempTable.Columns.Add("dfas df");
                ParseTable.Columns.Add("`");
                int RowCount = 0;
                while (sLine != null)
                {
                    if (RowCount == paging_NoOfRecPerPage)
                    {
                        break;
                    }
                    sLine = txtReader.ReadLine();
                    ParseTable.Rows.Add(sLine);
                    //TempTable.Rows.Add(sLine);
                    //arrText.Add(sLine);
                    RowCount++;
                }

                txtReader.Close();
                // dataGrid.ItemsSource = TempTable.DefaultView;
                ListProducts(0);
            }
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        /* Get Parse to XML Schema Button Click Event */
        private void ParseToXMLBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dataTable == null)
            {
                MessageBox.Show("There is no data to save.");
                return;
            }

            // Save Metadata as XML file
            XmlSerializer xs = new XmlSerializer(metaDataArray.GetType());

            using (FileStream fs = new FileStream("ReaderMeta.xml", FileMode.Create))
                xs.Serialize(fs, metaDataArray);

            // Save file data as XML file 
            dataTable.TableName = "dataList";
            dataTable.WriteXml(@"ReaderData.xml");
            MessageBox.Show("Success export into Data & MetaData!");
        }

        /* ArrayList to DataTable functions */
        public static DataTable ConvertArrayListToDataTable(ArrayList arrayList)
        {
            DataTable dt = new DataTable();

            if (arrayList.Count != 0)
            {
                dt = ConvertObjectToDataTableSchema(arrayList[0]);
                FillData(arrayList, dt);
            }

            return dt;
        }

        public static DataTable ConvertArrayListToDataTable2(ArrayList arrayList, bool IsFillData)
        {
            DataTable dt = new DataTable();

            if (arrayList.Count != 0)
            {
                dt = ConvertObjectToDataTableSchema(arrayList[0]);
                if (IsFillData)
                {
                    FillData(arrayList, dt);
                }
            }

            return dt;
        }

        public static DataTable ConvertObjectToDataTableSchema(Object o)
        {
            DataTable dt = new DataTable();
            PropertyInfo[] properties = o.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                dt.Columns.Add(property.Name, Nullable.GetUnderlyingType(
                    property.PropertyType) ?? property.PropertyType);
            }
            return dt;
        }

        private static void FillData(ArrayList arrayList, DataTable dt)
        {
            foreach (Object o in arrayList)
            {
                DataRow dr = dt.NewRow();
                PropertyInfo[] properties = o.GetType().GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    //if (property.Name == "DataType")
                    //{
                    //    dr[property.Name] =  "2";//property.GetValue(o, null);
                    //   // dr[property.Name] = property.SetValue(o, null);
                    //}
                    // else
                    // {
                    dr[property.Name] = property.GetValue(o, null) ?? DBNull.Value;
                    //}
                }
                dt.Rows.Add(dr);
            }
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {

            bChangeFlag = true;
            if (colNumber == 1)
            {
                //LenTxt.Text = colLengthList[colNumber-1].ToString();
                //LenSlider.Value = colLengthList[colNumber-1];
            }
            else if (colNumber < 1)
            {
                return;
            }
            else
            {
                colNumber = colNumber - 1;
                //LenTxt.Text = (colLengthList[colNumber-1] - colLengthList[colNumber - 2]).ToString();
                //LenSlider.Value = (colLengthList[colNumber-1] - colLengthList[colNumber - 2]);
            }
            ColTxt.Text = colNumber.ToString();
            colStartPosTxt.Text = colLengthList[colNumber - 1].ToString();

            // ParserGrid.ItemsSource = TempTable.DefaultView;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            bChangeFlag = true;
            LenTxt.Text = "0";
            LenSlider.Value = 0;

            if (colNumber == strHeaders.Count())
            {
                //LenTxt.Text = (colLengthList[colNumber-1] - colLengthList[colNumber - 2]).ToString();
                //LenSlider.Value = (colLengthList[colNumber-1] - colLengthList[colNumber - 2]);
            }
            else if (colNumber < strHeaders.Count())
            {
                colNumber = colNumber + 1;

                //LenTxt.Text = colLengthList[colNumber-1].ToString();
                //LenSlider.Value = colLengthList[colNumber-1];
            }

            //ParserGrid.ItemsSource = TempTable.DefaultView;
            ColTxt.Text = colNumber.ToString();
            colStartPosTxt.Text = colLengthList[colNumber - 2].ToString();

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DataTable TxtData = new DataTable();
            TxtData = TempTable;

            //dataGrid.ItemsSource = TxtData.DefaultView;
            metaDataArray.Clear();

            for (int col = 0; col < strHeaders.Count(); col++)
            {
                metaData = new MetaDataClass();
                metaData.Name = strHeaders[col];
                metaData.Length = colLengthList[col];
                metaData.StartPos = colStartPosList[col];
                metaData.OpenFileType = FileTypeSelected;
                metaData.RootArrayName = txtRootArrayName.Text;
                metaData.InputFile = txtInputFile.Text;
                if (file_extension != ".dbf")
                {
                    metaData.FileEncoding = FileEncoding;
                }
                metaDataArray.Add(metaData);
            }

            Reader_metadata = metaDataArray;
        }

        private void LenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (bChangeFlag == true)  // when change column number don't call this function
            {
                bChangeFlag = false;
                return;
            }

            LenTxt.Text = LenSlider.Value.ToString();
            //TempTable = new DataTable();

            int colPos = System.Convert.ToInt32(ColTxt.Text) - 1;

            rowData = new string[strHeaders.Count()];
            TempTable.Clear();
            for (int kk = 0; kk < strHeaders.Count(); kk++)
            {
                if (TempTable.Columns.Contains(strHeaders[kk]) == false)
                    TempTable.Columns.Add(strHeaders[kk]);
            }

            colLengthList[colPos] = System.Convert.ToInt32(LenSlider.Value); // + System.Convert.ToInt32(colStartPosTxt.Text);
            colStartPosList[colPos] = System.Convert.ToInt32(colStartPosTxt.Text);

            if (colPos > 0)
            {
                //colLengthList[colPos] = System.Convert.ToInt32(LenSlider.Value) + colLengthList[colPos - 1];

                colLengthList[colPos] = System.Convert.ToInt32(LenSlider.Value); // + System.Convert.ToInt32(colStartPosTxt.Text);
                colStartPosList[colPos] = System.Convert.ToInt32(colStartPosTxt.Text);

                if (colLengthList[colPos] + colStartPosList[colPos] > rowLength && rowLength > 0)
                {
                    LenSlider.Value = rowLength - colLengthList[colPos - 1];
                    return;
                }
            }
            else
            {
                colLengthList[colPos] = System.Convert.ToInt32(LenSlider.Value); // + System.Convert.ToInt32(colStartPosTxt.Text);
                colStartPosList[colPos] = System.Convert.ToInt32(colStartPosTxt.Text);

                if (colLengthList[colPos] + colStartPosList[colPos] > rowLength && rowLength > 0)
                {
                    LenSlider.Value = rowLength - colLengthList[colPos];

                    return;
                }

                //colLengthList[colPos] = System.Convert.ToInt32(LenSlider.Value);
                //colLengthList[colPos] = System.Convert.ToInt32(colStartPosTxt.Text);// + System.Convert.ToInt32(LenSlider.Value);

            }


            for (int row = 0; row < strLines.Count(); row++)
            {
                if (strLines[row] == null)
                    return;

                rowLength = strLines[row].Length;

                for (int subCol = 0; subCol < (colPos + 1); subCol++)
                {

                    //if (subRow > 0)
                    //{
                    //    if (colLengthList[subRow] == 0)
                    //    {
                    //        //rowData[subRow] = strLines[row].Substring(colLengthList[subRow - 1], colLengthList[subRow - 1]);
                    //        rowData[subRow] = strLines[row].Substring(System.Convert.ToInt32(colStartPosTxt.Text), System.Convert.ToInt32(LenTxt.Text));
                    //    }
                    //    else
                    //    {
                    //        //rowData[subRow] = strLines[row].Substring(colLengthList[subRow - 1], colLengthList[subRow] - colLengthList[subRow - 1]);
                    //        rowData[subRow] = strLines[row].Substring(System.Convert.ToInt32(colStartPosTxt.Text), System.Convert.ToInt32(LenTxt.Text));
                    //    }

                    //}
                    //else if (subRow <= 0)
                    //{
                    rowData[subCol] = strLines[row].Substring(colStartPosList[subCol], colLengthList[subCol]);
                    //}

                }

                TempTable.Rows.Add(rowData);

                ParserGrid.ItemsSource = TempTable.DefaultView;
            }
        }

        private void TabItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            LenSlider.Minimum = 0;
            LenSlider.Maximum = 100;
            LenSlider.IsSnapToTickEnabled = true;
            LenSlider.TickFrequency = 1;
            dataTable = new DataTable();

            //strHeaders = new string[100];

            using (StreamReader txtReader = new StreamReader(file_path, encode))
            {
                string sLine = "";

                //ArrayList arrText = new ArrayList();
                sLine = txtReader.ReadLine();
                if (file_extension == ".txt")
                {
                    sLine = GetHeader(file_path, encode);
                    string[] lines = File.ReadAllLines(file_path);
                    int cou = 0;
                    while (txtReader.ReadLine() != sLine)
                    {
                        if (lines.Length <= cou)
                        {
                            break;
                        }
                        txtReader.ReadLine();
                        cou++;
                    }
                }

                sLine = sLine.Replace(".", "");
                strHeader = sLine;

                strHeaders = strHeader.Trim().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                string[] rowData = new string[strHeaders.Count()];
                colLengthList = new int[strHeaders.Count()];
                colStartPosList = new int[strHeaders.Count()];
                //sLine = sLine.Replace(" ", "");

                dataTable.Columns.Add(sLine);

                while (sLine != null)
                {
                    sLine = txtReader.ReadLine();
                    dataTable.Rows.Add(sLine);
                    strLines.Add(sLine);
                    strRows.Add(rowData);
                    //arrText.Add(sLine);
                }

                txtReader.Close();
                ParserGrid.ItemsSource = dataTable.DefaultView;
            }
        }

        private void ColStartPosTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (colStartPosList == null)
                return;

            colStartPosList[System.Convert.ToInt32(ColTxt.Text)] = System.Convert.ToInt32(colStartPosTxt.Text);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = false;
                this.Hide();
                return;

                if (File.Exists(@"AppDetails.flo"))
                {
                    File.Delete(@"AppDetails.flo");
                }
                if (File.Exists(@"HeaderData.flo"))
                {
                    File.Delete(@"HeaderData.flo");
                }
                if (File.Exists(@"TableData.flo"))
                {
                    File.Delete(@"TableData.flo");
                }
                ProcessStartInfo Info = new ProcessStartInfo();
                Info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetExecutingAssembly().Location + "\"";
                Info.WindowStyle = ProcessWindowStyle.Hidden;
                Info.CreateNoWindow = true;
                Info.FileName = "cmd.exe";
                Process.Start(Info);
                Application.Current.Shutdown();
            }
            catch
            {

            }
        }


        private void ListProducts(int category)
        {

            try
            {

                paging_PageIndex = 1;

                if (ParseTable.Rows.Count > 0)
                {
                    DataTable tmpTable = new DataTable();
                    tmpTable = ParseTable.Clone();

                    if (ParseTable.Rows.Count >= paging_NoOfRecPerPage)
                    {
                        for (int i = 0; i < paging_NoOfRecPerPage; i++)
                        {
                            tmpTable.ImportRow(ParseTable.Rows[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ParseTable.Rows.Count; i++)
                        {
                            tmpTable.ImportRow(ParseTable.Rows[i]);
                        }
                    }

                    dataGrid.DataContext = tmpTable.DefaultView;

                    foreach (var column in dataGrid.Columns)
                    {
                        var rgx = new Regex("Column[0-9]+");

                        if (rgx.IsMatch((string)column.Header))
                        {
                            column.Header = "";
                        }
                    }

                    tmpTable.Dispose();

                    DisplayPagingInfo();
                }
                else
                {
                    if (ShowPopup)
                    {
                        MessageBox.Show("No Records Exists for the selected category");
                    }
                    ShowPopup = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void ChangePageInfo()
        {

            if (!string.IsNullOrEmpty(textBoxFrom.Text))
            {
                if (IsDigitsOnly(textBoxFrom.Text))
                {
                    if (OldPageNumber != Convert.ToInt32(textBoxFrom.Text))
                    {
                        paging_NoOfRecPerPage = Convert.ToInt32(textBoxFrom.Text);
                        if (File.Exists(txtInputFile.Text))
                        {
                            MetaDataSaveBtn_Click(null, null);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Only Digits Allowe..!!");
                }
                OldPageNumber = paging_NoOfRecPerPage;
            }
        }

        private void DisplayPagingInfo()
        {
            string pagingInfo = "Displaying " + (((paging_PageIndex - 1) * paging_NoOfRecPerPage) + 1) + " to " + paging_PageIndex * paging_NoOfRecPerPage;

            if (ParseTable.Rows.Count < (paging_PageIndex * paging_NoOfRecPerPage))
            {
                pagingInfo = "Displaying " + (((paging_PageIndex - 1) * paging_NoOfRecPerPage) + 1) + " to " + ParseTable.Rows.Count;
            }
            lblPagingInfo.Content = pagingInfo;// +" out of "+ TotalRecords;

            lblPageNumber.Content = paging_PageIndex;
        }

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            CustomPaging((int)PagingMode.First);
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            CustomPaging((int)PagingMode.Previous);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            CustomPaging((int)PagingMode.Next);
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            CustomPaging((int)PagingMode.Last);
        }


        private void CustomPaging(int mode)
        {
            int totalRecords = ParseTable.Rows.Count;
            int pageSize = paging_NoOfRecPerPage;
            int currentPageIndex = paging_PageIndex;

            if (ParseTable.Rows.Count <= paging_NoOfRecPerPage)
            {
                return;
            }

            switch (mode)
            {
                case (int)PagingMode.Next:
                    if (ParseTable.Rows.Count > (paging_PageIndex * paging_NoOfRecPerPage))
                    {
                        DataTable tmpTable = new DataTable();
                        tmpTable = ParseTable.Clone();

                        if (ParseTable.Rows.Count >= ((paging_PageIndex * paging_NoOfRecPerPage) + paging_NoOfRecPerPage))
                        {
                            for (int i = paging_PageIndex * paging_NoOfRecPerPage; i < ((paging_PageIndex * paging_NoOfRecPerPage) + paging_NoOfRecPerPage); i++)
                            {
                                tmpTable.ImportRow(ParseTable.Rows[i]);
                            }
                        }
                        else
                        {
                            for (int i = paging_PageIndex * paging_NoOfRecPerPage; i < ParseTable.Rows.Count; i++)
                            {
                                tmpTable.ImportRow(ParseTable.Rows[i]);
                            }
                        }

                        paging_PageIndex += 1;

                        dataGrid.DataContext = tmpTable.DefaultView;
                        tmpTable.Dispose();
                    }
                    break;
                case (int)PagingMode.Previous:
                    if (paging_PageIndex > 1)
                    {
                        DataTable tmpTable = new DataTable();
                        tmpTable = ParseTable.Clone();

                        paging_PageIndex -= 1;

                        for (int i = ((paging_PageIndex * paging_NoOfRecPerPage) - paging_NoOfRecPerPage); i < (paging_PageIndex * paging_NoOfRecPerPage); i++)
                        {
                            tmpTable.ImportRow(ParseTable.Rows[i]);
                        }

                        dataGrid.DataContext = tmpTable.DefaultView;
                        tmpTable.Dispose();
                    }
                    break;
                case (int)PagingMode.First:
                    paging_PageIndex = 2;
                    CustomPaging((int)PagingMode.Previous);
                    break;
                case (int)PagingMode.Last:
                    paging_PageIndex = (ParseTable.Rows.Count / paging_NoOfRecPerPage);
                    CustomPaging((int)PagingMode.Next);
                    break;
            }

            DisplayPagingInfo();
        }

        private void TakeLength()
        {
            ApplyEdit();
            if (dataGridHidden.Items.Count < 1 || dataTable == null)
            {
                MessageBox.Show("There is no data to save.");
                return;
            }

            dataTable.Rows.Clear();
            dataTable.Columns.Clear();
            dataTable = ((DataView)dataGridHidden.ItemsSource).ToTable();  //dataGrid.table;

            System.Windows.Forms.SaveFileDialog fbd = new System.Windows.Forms.SaveFileDialog();
            fbd.Filter = "XML files (*.xml)|*.xml";
            // System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            // string FolderPathMeta = "";
            string FolderPathData = "";
            string ext = "";
            string path = "";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                path = System.IO.Path.GetDirectoryName(fbd.FileName);
                ext = System.IO.Path.GetExtension(fbd.FileName);
                //     FolderPathMeta = fbd.SelectedPath + "\\ReaderMeta.flo";
                FolderPathData = path + "\\SchemaData.xml";
            }
            if (!Directory.Exists(path) || ext != ".xml")
            {
                MessageBox.Show("Invalid file..!");
                return;
            }
            //System.IO.FileStream fss = (System.IO.FileStream)fbd.OpenFile();
            Stream myStream;
            if ((myStream = fbd.OpenFile()) != null)
            {
                // Code to write the stream goes here.
                myStream.Close();
            }
            if (!File.Exists(fbd.FileName))
            {
                MessageBox.Show("Invalid file..!");
                return;
            }
            List<MetaDataClassUpdated> metaDataArrayUpdated = new List<MetaDataClassUpdated>();
            //  if (FileTypeSelected == "DBF")
            //  {
            metaDataArrayUpdated = metaDataArray.Select(x => new MetaDataClassUpdated { Name = x.Name, StartPos = x.StartPos, Length = x.Length, TrimSpaces = x.TrimSpaces, OpenFileType = x.OpenFileType, RootArrayName = x.RootArrayName, InputFile = x.InputFile, FileEncoding = x.FileEncoding, DataTypeStr = x.DataTypeStr }).ToList();
            //  }
            //   else
            //  {
            //metaDataArrayUpdated = metaDataArray.Select(x => new MetaDataClassUpdated { Name = x.Name, StartPos = x.StartPos, Length = x.Length, TrimSpaces = x.TrimSpaces, DataTypeStr = x.DataTypeStr }).ToList();
            // }

            //// Save Metadata as XML file
            //XmlSerializer xs = new XmlSerializer(metaDataArrayUpdated.GetType());

            //using (FileStream fs = new FileStream(fbd.FileName, FileMode.Create))
            //    xs.Serialize(fs, metaDataArrayUpdated);

            // New Code of XML Write
            CreateXML(metaDataArrayUpdated, fbd.FileName);
            //

            // Save file data as XML file 
            dataTable.TableName = "dataList";
            dataTable.WriteXml(FolderPathData);
            MessageBox.Show("Success export into Data & MetaData!");
        }

        private void ParseXml_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridColumns.Items.Count > 1 && file_extension == ".txt")
            {
                TakeLength();
                return;
            }

            if (dataGrid.Items.Count < 1 || dataTable == null)
            {
                MessageBox.Show("There is no data to save.");
                return;
            }

            dataTable.Rows.Clear();
            dataTable.Columns.Clear();
            dataTable = ((DataView)dataGrid.ItemsSource).ToTable();  //dataGrid.table;

            System.Windows.Forms.SaveFileDialog fbd = new System.Windows.Forms.SaveFileDialog();
            fbd.Filter = "XML files (*.xml)|*.xml";
            // System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            // string FolderPathMeta = "";
            string FolderPathData = "";
            string ext = "";
            string path = "";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                path = System.IO.Path.GetDirectoryName(fbd.FileName);
                ext = System.IO.Path.GetExtension(fbd.FileName);
                //     FolderPathMeta = fbd.SelectedPath + "\\ReaderMeta.flo";
                FolderPathData = path + "\\SchemaData.xml";
            }
            if (!Directory.Exists(path) || ext != ".xml")
            {
                MessageBox.Show("Invalid file..!");
                return;
            }
            //System.IO.FileStream fss = (System.IO.FileStream)fbd.OpenFile();
            Stream myStream;
            if ((myStream = fbd.OpenFile()) != null)
            {
                // Code to write the stream goes here.
                myStream.Close();
            }
            if (!File.Exists(fbd.FileName))
            {
                MessageBox.Show("Invalid file..!");
                return;
            }
            List<MetaDataClassUpdated> metaDataArrayUpdated = new List<MetaDataClassUpdated>();
            if (FileTypeSelected == "DBF")
            {
                metaDataArrayUpdated = metaDataArray.Select(x => new MetaDataClassUpdated { Name = x.Name, StartPos = x.StartPos, Length = x.Length, TrimSpaces = x.TrimSpaces, OpenFileType = x.OpenFileType, RootArrayName = x.RootArrayName, InputFile = x.InputFile, FileEncoding = "None", DataTypeStr = x.DataTypeStr }).ToList();
            }
            else
            {
                metaDataArrayUpdated = metaDataArray.Select(x => new MetaDataClassUpdated { Name = x.Name, StartPos = x.StartPos, Length = x.Length, TrimSpaces = x.TrimSpaces, OpenFileType = x.OpenFileType, RootArrayName = x.RootArrayName, InputFile = x.InputFile, FileEncoding = x.FileEncoding, DataTypeStr = x.DataTypeStr }).ToList();
                //metaDataArrayUpdated = metaDataArray.Select(x => new MetaDataClassUpdated { Name = x.Name, StartPos = x.StartPos, Length = x.Length, TrimSpaces = x.TrimSpaces, DataTypeStr = x.DataTypeStr }).ToList();
            }

            // Save Metadata as XML file
            //XmlSerializer xs = new XmlSerializer(metaDataArrayUpdated.GetType());

            //using (FileStream fs = new FileStream(fbd.FileName, FileMode.Create))
            //    xs.Serialize(fs, metaDataArrayUpdated);

            // New Code of XML Write
            CreateXML(metaDataArrayUpdated, fbd.FileName);
            //
            // Save file data as XML file 
            dataTable.TableName = "dataList";
            dataTable.WriteXml(FolderPathData);
            MessageBox.Show("Success export into Data & MetaData!");
        }

        public void CreateXML(List<MetaDataClassUpdated> data, string FileName)
        {
            XNamespace exportNs;
            XDocument exportDoc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
            exportNs = "http://www.w3.org/2001/XMLSchema";
            XElement EConverter = new XElement("XML_Converter", new XAttribute("Guid", Guid.NewGuid().ToString()));
            exportDoc.Add(EConverter);

            string TextEncoding = data != null ? data[0].FileEncoding.ToString() : "";
            string InputType = data != null ? data[0].OpenFileType.ToString() : "";

            var rootAttributes = new List<XAttribute>()
            {
                new XAttribute("TextEncoding", cbxEncoding.SelectedValue.ToString()),
                new XAttribute("InputType", InputType),
                new XAttribute("InputPath", FileName)
            };

            if (txtDelimiter.Visibility == Visibility.Visible)
            {
                rootAttributes.Add(new XAttribute("Delimeter", txtDelimiter.Text));
            }

            if (txtEnclousure.Visibility == Visibility.Visible)
            {
                rootAttributes.Add(new XAttribute("TextQualifier", txtEnclousure.Text));
            }

            XElement ERoot = new XElement(txtRootArrayName?.Text ?? "Root", rootAttributes);
            EConverter.Add(ERoot);

            XElement ESchema = new XElement("Schema");
            ERoot.Add(ESchema);

            XElement EXsSchema = new XElement(exportNs + "schema", new XAttribute("attributeFormDefault", "unqualified"), new XAttribute("elementFormDefault", "qualified"), new XAttribute(XNamespace.Xmlns + "xs", exportNs));
            ESchema.Add(EXsSchema);

            foreach (var item in data)
            {
                string Name = item.Name == null ? "" : item.Name.ToString();
                string DataType = item.DataTypeStr == null ? "string" : item.DataTypeStr.ToString();
                XElement EElement = new XElement(exportNs + "element", new XAttribute("name", Name), new XAttribute("type", "xs:" + DataType.ToLower()), new XAttribute("use", "One"), new XAttribute("change", "Flatten"), new XAttribute("Alias", Name));
                EXsSchema.Add(EElement);
            }

            exportDoc.Save(FileName);
        }

        private void CmbFileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count > 0 && ((ComboBoxItem)e.RemovedItems[0]).IsVisible)
            {
                txtRootArrayName.Text = "ROOT_ARRAY";
                txtInputFile.Text = "";
                cbxEncoding.SelectedIndex = Array.IndexOf(encodings.Select(enc => enc.CodePage).ToArray(), Encoding.ASCII.CodePage);
                txtDelimiter.Text = ",";
                txtEnclousure.Text = "\"";

                if (HeadercheckBox != null)
                {
                    HeadercheckBox.IsChecked = false;
                }

                dataGrid.DataContext = null;
                dataGridColumns.ItemsSource = new List<GridColumnClass>();
            }

            if (CmbFileType.SelectedItem != null)
            {
                ComboBoxItem typeItem = (ComboBoxItem)CmbFileType.SelectedItem;
                if (typeItem != null)
                {
                    if (typeItem.Content != null)
                    {
                        FileTypeSelected = typeItem.Content.ToString();
                        if (FileTypeSelected == "CSV")
                        {
                            dataGridColumns.IsReadOnly = false;
                            DelimiterLbl.Visibility = Visibility.Visible;
                            txtDelimiter.Visibility = Visibility.Visible;
                            EncLbl.Visibility = Visibility.Visible;
                            txtEnclousure.Visibility = Visibility.Visible;
                            if (HeadercheckBox != null)
                            {
                                HeadercheckBox.Visibility = Visibility.Visible;
                            }

                            // grid margin
                            if (dataGrid != null)
                            {

                                Thickness margindataGrid = dataGrid.Margin;
                                margindataGrid.Top = 10;
                                dataGrid.Margin = margindataGrid;
                            }
                            if (dataGridColumns != null)
                            {
                                if (HeadercheckBox != null)
                                {
                                    if (HeadercheckBox.IsChecked == true)
                                    {
                                        // grid margin
                                        Thickness margindataGrid = dataGrid.Margin;
                                        margindataGrid.Top = 10;
                                        dataGrid.Margin = margindataGrid;
                                        dataGridColumns.Visibility = Visibility.Hidden;
                                        schemeHeaderLabel.Visibility = Visibility.Hidden;
                                    }
                                    else
                                    {
                                        // grid margin
                                        Thickness margindataGrid = dataGrid.Margin;
                                        margindataGrid.Top = 156;
                                        dataGrid.Margin = margindataGrid;
                                        if (!string.IsNullOrEmpty(txtInputFile.Text))
                                        {
                                            dataGridColumns.Visibility = Visibility.Visible;
                                            schemeHeaderLabel.Visibility = Visibility.Visible;
                                        }
                                        if (dataGridColumns.Columns.Count > 2)
                                        {
                                            dataGridColumns.Columns[2].Visibility = Visibility.Hidden;
                                        }
                                        if (dataGridColumns.Columns.Count > 3)
                                        {
                                            dataGridColumns.Columns[3].Visibility = Visibility.Hidden;
                                        }
                                    }
                                }
                            }
                        }
                        else if (FileTypeSelected == "Fixed Width")
                        {
                            dataGridColumns.Columns[2].Visibility = Visibility.Visible;
                            dataGridColumns.Columns[3].Visibility = Visibility.Visible;
                            dataGridColumns.IsReadOnly = false;
                            DelimiterLbl.Visibility = Visibility.Hidden;
                            txtDelimiter.Visibility = Visibility.Hidden;
                            EncLbl.Visibility = Visibility.Hidden;
                            txtEnclousure.Visibility = Visibility.Hidden;
                            if (HeadercheckBox != null)
                            {
                                HeadercheckBox.Visibility = Visibility.Hidden;
                            }

                            // grid margin
                            Thickness margindataGrid = dataGrid.Margin;
                            margindataGrid.Top = 156;
                            dataGrid.Margin = margindataGrid;
                            if (!string.IsNullOrEmpty(txtInputFile.Text))
                            {
                                dataGridColumns.Visibility = Visibility.Visible;
                                schemeHeaderLabel.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            dataGridColumns.IsReadOnly = true;
                            DelimiterLbl.Visibility = Visibility.Hidden;
                            txtDelimiter.Visibility = Visibility.Hidden;
                            EncLbl.Visibility = Visibility.Hidden;
                            txtEnclousure.Visibility = Visibility.Hidden;
                            if (HeadercheckBox != null)
                            {
                                HeadercheckBox.Visibility = Visibility.Hidden;
                            }

                            // grid margin
                            Thickness margindataGrid = dataGrid.Margin;
                            margindataGrid.Top = 10;
                            dataGrid.Margin = margindataGrid;

                            dataGridColumns.Visibility = Visibility.Hidden;
                            schemeHeaderLabel.Visibility = Visibility.Hidden;
                        }


                    }
                }
            }
        }

        private void HeadercheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (HeadercheckBox.IsChecked == true)
            {
                dataGridColumns.IsReadOnly = true;

                // grid margin
                Thickness margindataGrid = dataGrid.Margin;
                margindataGrid.Top = 10;
                dataGrid.Margin = margindataGrid;

                dataGridColumns.Visibility = Visibility.Hidden;
                schemeHeaderLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                dataGridColumns.IsReadOnly = false;

                // grid margin
                Thickness margindataGrid = dataGrid.Margin;
                margindataGrid.Top = 156;
                dataGrid.Margin = margindataGrid;

                dataGridColumns.Visibility = Visibility.Visible;
                schemeHeaderLabel.Visibility = Visibility.Visible;
            }

            btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void textBoxFrom_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangePageInfo();
        }

        private void cbxEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxEncoding.SelectedItem != null)
            {
                ComboBoxItem typeItem = (ComboBoxItem)cbxEncoding.SelectedItem;
                if (typeItem != null)
                {
                    if (typeItem.Content != null)
                    {
                        FileEncoding = typeItem.Content.ToString();
                        if (btnMetaDataSave != null && !string.IsNullOrEmpty(txtInputFile.Text))
                        {
                            btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                        }
                    }
                }
            }
        }
        public DataGridCell GetCell(int rowIndex, int columnIndex, DataGrid dg)
        {
            DataGridRow row = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            DataGridCellsPresenter p = GetVisualChild<DataGridCellsPresenter>(row);
            DataGridCell cell = p.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
            return cell;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {


            if (dataGrid.Items.Count < 1 || dataTable == null)
            {
                MessageBox.Show("There is no data", "Data Missing Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                /*
                dataTable.Rows.Clear();
                dataTable.Columns.Clear();
                dataTable = ((DataView)dataGrid.ItemsSource).ToTable();  //dataGrid.table;
                dataTable.TableName = "dataList";
                dataTable.WriteXml(@"TableData.flo");
                */
            }



            if (dataGridColumns.Items.Count > 1)
            {
                DataTable headerTable = ConvertArrayListToDataTable(new ArrayList((List<GridColumnClass>)dataGridColumns.ItemsSource));

                headerTable.TableName = "headerList";
                headerTable.WriteXml(@"HeaderData.flo");
            }

            /*
            DataTable FileInfo = new DataTable();
            FileInfo.Columns.Add("FileType");
            FileInfo.Columns.Add("RootArray");
            FileInfo.Columns.Add("InputFile");
            FileInfo.Columns.Add("FileEncoding");
            FileInfo.Columns.Add("Delimiter");
            FileInfo.Columns.Add("TextQualifier");
            FileInfo.Columns.Add("FirstHeader");

            FileInfo.Rows.Add(FileTypeSelected, txtRootArrayName.Text, txtInputFile.Text, FileEncoding, txtDelimiter.Text, txtEnclousure.Text, HeadercheckBox.IsChecked);
            FileInfo.TableName = "AppDetails";
            FileInfo.WriteXml(@"AppDetails.flo");
            */


            this.SetPropsWindowToModule();

            SetSchema();

            this.DialogResult = true;
            this.Hide();
        }

        private void SetSchema()
        {
            this.RootSchema = null;

            Field item = null;
            if (this.RootSchema == null)
            {
                this.RootSchema = new Field();
                this.RootSchema.Name = this.DataFileInfo.RootArrayName;
                this.RootSchema.Alias = this.RootSchema.Name;
                this.RootSchema.Optionality = "One";
                this.RootSchema.Change = "Flatten";
                this.RootSchema.DataType = "String";

                if (this.RootSchema.ChildNodes == null)
                {
                    this.RootSchema.ChildNodes = new List<Field>();
                }

                item = new Field();
                item.Name = "Item";
                item.Alias = "Item";
                item.Type = "Element";
                item.DataType = "Array";
                item.Optionality = "One";
                item.Change = "Flatten";
                item.ChildNodes = new List<Field>();
                this.RootSchema.ChildNodes.Add(item);

            }

            List<string> headerList = new List<string>();
            foreach (DataColumn column in this.dataTable.Columns)
            {
                headerList.Add(column.ColumnName);
            }

            string[] headers = ParserBase.GetCleanHeaders(headerList.ToArray());

            for (int i = 0; i < headers.Length; i++)
            {
                var column = this.dataTable.Columns[i];

                Field f = new JdSuite.Common.Module.Field();
                f.Type = "Element";
                f.Name = headers[i];
                f.DataType = column.DataType.Name;
                f.Optionality = "One";
                f.Change = "Flatten";
                f.Alias = f.Name;

                item.ChildNodes.Add(f);

            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (LoadDropDown)
            {
                BindDropDown();
            }
            //else
            //{
            //    DataGridCell cell = GetCell(0, 1, dataGridColumns);
            //    DataGridCell cell2 = GetCell(0, 4, dataGridColumns);
            //    cell.Content = EnumDataType.String;
            //    cell2.Content = EnumTrimType.None;
            //}
            if (dataGrid.Items.Count > 1 && dataGridColumns.Items.Count > 1 && file_extension == ".txt")
            {
                dataGrid.GridLinesVisibility = DataGridGridLinesVisibility.All;
            }
        }



        private void dataGridColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DataRowView customer = (DataRowView)dataGridColumns.SelectedItems[0];
            //if (customer.Row.ItemArray.Length > 4)
            //{
            //    if (string.IsNullOrEmpty(customer.Row[1].ToString()))
            //    {
            //        DataGridCell cell = GetCell(dataGridColumns.SelectedIndex, 1, dataGridColumns);
            //        cell.Content = EnumDataType.String;
            //    }
            //}
        }

        private void dataGridColumns_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            //int i = e.Row.GetIndex();
            //int NewRow = dataGridColumns.Items.Count;

            //if (NewRow > oldRow)
            //{

            //    metaDataGrd = new GirdColumnClass();
            //    metaDataGrd.Name = "";
            //    metaDataGrd.DataType = (EnumDataType)Enum.Parse(typeof(EnumDataType), "String");
            //    metaDataGrd.StartPos = 0;
            //    metaDataGrd.Length = 0;
            //    metaDataGrd.TrimSpaces = (EnumTrimType)Enum.Parse(typeof(EnumTrimType), "None");
            //    metaDataArrayGrid.Add(metaDataGrd);


            //    Reader_metadataGrd = metaDataArrayGrid;
            //    // Reader_metadataGrd = metaDataArrayGrid;
            //    DataTable mdt = ConvertArrayListToDataTable2(new ArrayList(Reader_metadataGrd), true);
            //    dataGridColumns.ItemsSource = mdt.DefaultView;
            //    dataGridColumns.SelectedValue = 1;

            //Thread.Sleep(5000);
            //DataGridCell cell = GetCell(NewRow - 1, 1, dataGridColumns);
            //DataGridCell cell2 = GetCell(NewRow - 1, 4, dataGridColumns);
            //cell.Content = EnumDataType.String;
            //cell2.Content = EnumTrimType.None;
            //oldRow = dataGridColumns.Items.Count;
            //}
            //e.NewItem = new GirdColumnClass
            //{
            //    Name = "",
            //    DataType = EnumDataType.String,
            //    StartPos = 0,
            //    Length = 0,
            //    TrimSpaces = EnumTrimType.None
            //};
        }

        private void TxtInputFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtInputFile.Text))
            {
                if (FileTypeSelected == "CSV")
                {
                    if (dataGridColumns != null)
                    {
                        if (HeadercheckBox != null)
                        {
                            if (HeadercheckBox.IsChecked == true)
                            {
                                dataGridColumns.Visibility = Visibility.Hidden;
                                schemeHeaderLabel.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                dataGridColumns.Visibility = Visibility.Visible;
                                schemeHeaderLabel.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                else if (FileTypeSelected == "Fixed Width")
                {
                    dataGridColumns.Visibility = Visibility.Visible;
                    schemeHeaderLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    dataGridColumns.Visibility = Visibility.Hidden;
                    schemeHeaderLabel.Visibility = Visibility.Hidden;
                }

                btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                dataGrid.Visibility = Visibility.Hidden;
                dataGridColumns.Visibility = Visibility.Hidden;
                schemeHeaderLabel.Visibility = Visibility.Hidden;
            }
        }

        private void DataGridColumns_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void DataGridColumns_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {

        }

        private void TxtDelimiter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (btnMetaDataSave != null && !string.IsNullOrEmpty(txtInputFile.Text))
            {
                btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void TxtEnclousure_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (btnMetaDataSave != null && !string.IsNullOrEmpty(txtInputFile.Text))
            {
                btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void TxtRootArrayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (btnMetaDataSave != null && !string.IsNullOrEmpty(txtInputFile.Text))
            {
                btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void TextBoxFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (btnMetaDataSave != null && !string.IsNullOrEmpty(txtInputFile.Text))
            {
                btnMetaDataSave.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }
    }

    public class DataTypePlaceholderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value.ToString() == "{NewItemPlaceholder}")
                return EnumDataType.String;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TrimTypePlaceholderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value.ToString() == "{NewItemPlaceholder}")
                return EnumTrimType.None;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
