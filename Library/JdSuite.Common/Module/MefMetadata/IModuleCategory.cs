using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JdSuite.Common.Module.MefMetadata
{
    public enum IModuleCategory
    {
        DATA_INPUTS,
        DATA_MANIPULATION,
        DESIGNS,
        IMPOSITION,
        OUTPUTS,
        MISC
    }

    public enum ModuleType
    {
        XMLInput=1,
        CSVInput=2,
       
        DataSorter=3,
        DataFilter=4,

        DataOutput=5,
        Designer=6,
        Scripting=7

    }
}
