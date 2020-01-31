using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JdSuite.Common;
using JdSuite.Common.Module;

namespace JdSuite.DataFilter.ViewModels
{
    public class FilterControlViewModel : ViewModelBase
    {
        private FilterField _ff;
        private ObservableCollection<string> _conditions;

        public FilterField FF
        {
            get { return _ff; }
            set { SetPropertry(ref _ff, value); }
        }

        public ObservableCollection<string> Conditions
        {
            get { return _conditions; }
            set { SetPropertry(ref _conditions, value); }
        }
    }
}
