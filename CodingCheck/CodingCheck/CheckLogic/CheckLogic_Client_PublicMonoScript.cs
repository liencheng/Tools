using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CodingCheck.CheckLogic
{
    class CheckLogic_Client_PublicMonoScript : CheckLogicBase
    {
        public override Dictionary<int, string> CheckFile(System.IO.FileInfo file)
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();
            if(!IsSuffixFile(file, C_SUFFIX))
            {
                return ret;
            }
            StreamReader reader = file.OpenText();
            List<LineInfo> lineCache = new List<LineInfo>();
            if(null == reader)
            {
                return ret;
            }
            while(reader.Peek() > 0)
            {
                string line = reader.ReadLine();
                LineInfo li = new LineInfo(m_LineNumber++, line);
                lineCache.Add(li);
            }
            for(int idx = 0; idx < lineCache.Count; ++idx)
            {
                CheckLine(lineCache[idx].content, lineCache[idx].lineNum);
            }
            //匹配使用对象的情况
            foreach(KeyValuePair<int, string> pair in m_MatchLineInfo)
            {
                ret[pair.Key] = pair.Value.Trim();
            }
            return ret;
        }
        public override void CheckLine(string line, int lineNumbers)
        {
            if(IsCommentLine(line))
            {
                return;
            }
            if(IsMatch(line, m_MonoScriptPatterns)  && !IsMatch(line, m_PublicPatters))
            {
                m_MatchLineInfo[lineNumbers] = line;
                return;
            }
       
        }
        public string[] m_MonoScriptPatterns = new string[] { @"class( )*[\w]*( )*:( )*MonoBehaviour" };
        public string[] m_PublicPatters = new string[] { "public"};
        private const string C_SUFFIX = ".cs";

    }
}
