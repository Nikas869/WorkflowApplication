using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JdSuite.DataFilter
{
   public static class Extentions
    {
        public static string InnerXml(this XNode node)
        {
            using (var reader = node.CreateReader())
            {
                reader.MoveToContent();
                return reader.ReadInnerXml();
            }
        }
    }
}
