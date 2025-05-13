using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWinformsTest
{
    [DebuggerDisplay("{Description,nq}")]
    class ConfigurationItem
    {
        
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Provider { get; set; } = string.Empty;

        private string Description
        {
            get { return $"{{ Path : {Key}, Value : {Value}, Provider : {Provider} }}"; }
        }
    }
}
