

using JdSuite.DataFilter.Models.Enums;

namespace JdSuite.DataFilter.Models.Filters
{
    public class StringFilter: Filter
    {
        public ConditionType Condition { get; set; }

        public string Value { get; set; }
    }
}
