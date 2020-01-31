using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVInput
{
    public class ParserBase
    {
        public static string[] GetCleanHeaders(string[] rawheaders)
        {

            string[] headers = new string[rawheaders.Length];

            char[] prohibited_chars = new char[] { (char)0x20 };

            for (int i = 0; i < rawheaders.Length; i++)
            {
                headers[i] = rawheaders[i].Trim(prohibited_chars);
                headers[i] = rawheaders[i].Replace(" ", "_");
            }

            int idx = 0;
            do
            {
                idx = Array.FindIndex(headers, x => x.IndexOfAny(prohibited_chars) >= 0);
                if (idx >= 0)
                {
                    headers[idx] = headers[idx].Trim(prohibited_chars);
                    headers[idx] = headers[idx].Replace(" ", "_");
                }

            } while (idx >= 0);

            return headers;
        }
    }
}
