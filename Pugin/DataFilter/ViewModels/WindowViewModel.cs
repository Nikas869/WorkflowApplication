using System.Collections.ObjectModel;
using System.ComponentModel;
using JdSuite.Common;
using JdSuite.Common.Module;
using JdSuite.DataFilter.Models;

namespace JdSuite.DataFilter.ViewModels
{
    public class ViewModel : NotifyPropertyChangeBase
    {
        public ViewModel()
        {
            FieldNodes = new ObservableCollection<Field>();
            
        }

        private bool isAddFieldButtonEnabled;

        private bool isSortedFieldSelected;

        private bool isSortedFieldMovable;

        private bool isAnyFieldSorted;

        private string _dataDirectory;
        public string DataDirectory
        {
            get { return _dataDirectory; }
            set { SetPropertry(ref _dataDirectory, value); }
        }


        private string _schemaFile;
        public string SchemaFile
        {
            get { return _schemaFile; }
            set { SetPropertry(ref _schemaFile, value); }
        }
          


        public ObservableCollection<Field> FieldNodes { get; set; }

       

        public bool IsAddFieldButtonEnabled
        {
            get { return isAddFieldButtonEnabled; }
            set
            {
                SetPropertry(ref isAddFieldButtonEnabled, value);
            }
        }

        public bool IsSortedFieldSelected
        {
            get { return isSortedFieldSelected; }
            set
            {
                SetPropertry(ref isSortedFieldSelected, value);
            }
        }

        public bool IsSortedFieldMovable
        {
            get { return isSortedFieldMovable; }
            set
            {
                SetPropertry(ref isSortedFieldMovable, value);
            }
        }

        public bool IsAnyFieldSorted
        {
            get { return isAnyFieldSorted; }
            set
            {
                SetPropertry(ref isAnyFieldSorted, value);
            }
        }



        public void Unload()
        {
            this.IsAddFieldButtonEnabled = false;
            this.IsSortedFieldSelected = false;
            this.IsSortedFieldMovable = false;
            this.IsAnyFieldSorted = false;

            this.FieldNodes.Clear();
            
        }
    }
}
