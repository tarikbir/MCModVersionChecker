using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCModVersionChecker.Models;
internal class ModQueryResult
{
    public required bool IsFilter { get; set; }
    public required string ModName { get; set; }
    public required Uri ModLink { get; set; }
}
