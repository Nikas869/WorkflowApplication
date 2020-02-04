using System.Text;

namespace JdSuite.Common.FileProcessing
{
    public class DbfLoadingOptions
    {
        public string XmlRootName { get; set; } = "Root";
        public Encoding FileEncoding { get; set; } = Encoding.UTF8;
    }
}