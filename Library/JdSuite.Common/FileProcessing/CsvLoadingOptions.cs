using System.Text;

namespace JdSuite.Common.FileProcessing
{
    public class CsvLoadingOptions
    {
        public bool FirstLineHeaders { get; set; } = false;
        public string XmlRootName { get; set; } = "Root";
        public string Delimiter { get; set; } = ",";
        public Encoding FileEncoding { get; set; } = Encoding.UTF8;
    }
}