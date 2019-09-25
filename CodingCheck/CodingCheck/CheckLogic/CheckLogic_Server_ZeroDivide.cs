using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingCheck.CheckLogic
{
    public class CheckLogic_Server_ZeroDivide:CheckLogicBase
    {
        public override Dictionary<int, string> CheckFile(System.IO.FileInfo file)
        {
            return base.CheckFile(file);
        }
        public override void CheckLine(string line, int lineNumbers)
        {
            base.CheckLine(line, lineNumbers);
        }
    }
}
