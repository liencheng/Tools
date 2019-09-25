using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingCheck.CheckLogic
{
    public class LineInfo
    {
        public LineInfo(int num, string con)
        {
            lineNum = num;
            content = con;
        }
        public int lineNum;
        public string content;
    }
}
