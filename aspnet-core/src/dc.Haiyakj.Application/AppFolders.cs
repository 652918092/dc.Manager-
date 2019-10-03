using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dc.Haiyakj
{
    public class AppFolders : ISingletonDependency
    {
        public string ExcelFolder { get; set; }
    }
}
