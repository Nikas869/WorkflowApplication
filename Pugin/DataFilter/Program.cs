using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JdSuite.DataFilter
{
    public static class Program
    {

        [STAThread]
        public static void Main()
        {
            Xml.XmlFilter filter = new Xml.XmlFilter();
            filter.LoadData();
            filter.Filter();
        }
    }
}
