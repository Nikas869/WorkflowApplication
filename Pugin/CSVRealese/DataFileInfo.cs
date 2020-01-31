using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JdSuite.Common;
using JdSuite.Common.Module;

namespace CSVInput
{


    public class DataFileInfo : ViewModelBase
    {
        

        [XmlIgnore]
        private int _id;

        [XmlIgnore]
        private string _FileType;

        [XmlIgnore]
        private string _RootArrayName;

        [XmlIgnore]
        private string _FilePath;

        [XmlIgnore]
        private string _Encoding;

        [XmlIgnore]
        private string _ShowRecordCount;

        [XmlIgnore]
        private string _Delimiter;

        [XmlIgnore]
        private string _TextQualifier;

        [XmlIgnore]
        private bool _FirstRowHasHeader;
                     
        [XmlAttribute]
        public string FileType { get { return _FileType; } set { SetPropertry(ref _FileType, value); } }

        [XmlAttribute]
        public string RootArrayName { get { return _RootArrayName; } set { SetPropertry(ref _RootArrayName, value); } }

        [XmlAttribute]
        public string FilePath { get { return _FilePath; } set { SetPropertry(ref _FilePath, value); } }

        [XmlAttribute]
        public string Encoding { get { return _Encoding; } set { SetPropertry(ref _Encoding, value); } }

        [XmlAttribute]
        public string ShowRecordCount { get { return _ShowRecordCount; } set {SetPropertry(ref _ShowRecordCount, value); } }

        [XmlAttribute]
        public string Delimiter { get { return _Delimiter; }  set { SetPropertry(ref _Delimiter, value); } }

        [XmlAttribute]
        public int Id { get { return _id; } set { SetPropertry(ref _id, value); } }

        [XmlAttribute]
        public string TextQualifier { get { return _TextQualifier; } set { SetPropertry(ref _TextQualifier, value); } }

        [XmlAttribute]
        public bool FirstRowHasHeader { get { return _FirstRowHasHeader; } set { SetPropertry(ref _FirstRowHasHeader, value); } }

        public void CopyTo(DataFileInfo target)
        {
            target.Delimiter = this.Delimiter;
            target.Encoding = this.Encoding;
            target.FilePath = this.FilePath;
            target.FileType = this.FileType;
            target.FirstRowHasHeader = this.FirstRowHasHeader;
            target.Id = this.Id;
            target.TextQualifier = this.TextQualifier;
            target.ShowRecordCount = this.ShowRecordCount;
            target.RootArrayName = this.RootArrayName;
        }
    }


}
