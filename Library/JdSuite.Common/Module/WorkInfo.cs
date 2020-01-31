using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JdSuite.Common.Module
{
    public class WorkInfo
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger("WorkInfo");

        public Task ActiveTask { get; set; }

        public int Command { get; set; }
        public BaseModule Requester { get; set; }
        public NodeIndexer Indexer { get; set; }

        public Field Schema { get; set; }

        public bool RequesterStatus { get; set; }

        public bool IsCallAsync { get; set; }

        public BaseModule OutPutModule { get; set; }

        public List<BaseModule> ModuleList { get; private set; } = new List<BaseModule>();

        public WorkInfo(List<BaseModule> moduleList)
        {
            this.ModuleList = moduleList;
        }

        public void Add(BaseModule module)
        {
            if (!ModuleList.Contains(module))
            {
                ModuleList.Add(module);
            }
        }

        public void Log(string Source, NLog.LogLevel level, string Message)
        {
            if (OutPutModule != null)
            {
                ((IOutPutModule)OutPutModule).Log(Source, level, Message);
            }
            else
            {
                logger.Warn($"OutputModule is not set, cannot send message to output [{Source}] [{level}] [{Message}]");
            }
        }

        public void UpdateProgress(BaseModule Source, float Progress)
        {
            ((IOutPutModule)OutPutModule).UpdateProgress(Source, Progress);
        }

        /// <summary>
        /// Finds next module to the right of Caller module
        /// </summary>
        /// <param name="Caller"></param>
        /// <returns></returns>
        public BaseModule NextModule(BaseModule Caller)
        {
            var idx = ModuleList.IndexOf(Caller);
            if (idx == -1)
                return null;

            idx--;
            return idx > -1 ? ModuleList[idx] : null;
        }

        /// <summary>
        /// Finds next module to the left of Caller module
        /// </summary>
        /// <param name="Caller"></param>
        /// <returns></returns>
        public BaseModule NextModuleRTL(BaseModule Caller)
        {
            var idx = ModuleList.IndexOf(Caller);
            if (idx == -1)
                return null;

            idx++;
            return idx >= ModuleList.Count ? null : ModuleList[idx];
        }
    }
}
