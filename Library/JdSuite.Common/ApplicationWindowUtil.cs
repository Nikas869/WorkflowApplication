using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JdSuite.Common
{
    public static class ApplicationWindowUtil
    {
        public static Action<string> PFuncShowStatusBarMessage { get; set; }
        public static Action<int> PFuncSetStatusBarProgress { get; set; }

        public static void ShowStatusBarMessage(string message)
        {
            PFuncShowStatusBarMessage?.Invoke(message);
        }

        public static void SetProgress(int Percent)
        {
            PFuncSetStatusBarProgress?.Invoke(Percent);
        }
    }
}
