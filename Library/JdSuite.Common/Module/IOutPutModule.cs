using JdSuite.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JdSuite.Common
{
    public interface IOutPutModule
    {
        void Log(string Source, NLog.LogEventInfo logEvent);
        void Log(string Source, NLog.LogLevel level,string message);

        void UpdateProgress(BaseModule Module, float ProgressPercent);
    }
}
