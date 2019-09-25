using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CodingCheck.CheckLogic
{
    public class CheckLogic_GetScene : CheckLogicBase
    {
        public override Dictionary<int, string> CheckFile(FileInfo file)
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();
            if (!IsSuffixFile(file, C_SUFFIX))
            {
                return ret;
            }
            StreamReader reader = file.OpenText();
            List<LineInfo> lineCache = new List<LineInfo>();
            if (null == reader)
            {
                return ret;
            }
            while (reader.Peek() > 0)
            {
                string line = reader.ReadLine();
                LineInfo li = new LineInfo(m_LineNumber++, line);
                lineCache.Add(li);
            }
            //倒叙查
            for (int idx = lineCache.Count - 1; idx >= 0; --idx)
            {
                CheckLine(lineCache[idx].content, lineCache[idx].lineNum);
            }
            //匹配使用对象的情况
            foreach (KeyValuePair<int, string> pair in m_MatchLineInfo)
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
            for (int index = 0; index < m_CheckRegexPatterns.Length; ++index)
            {
                Match match = Regex.Match(line, m_CheckRegexPatterns[index]);
                if (match.Success)
                {
                    m_MatchLineInfo[lineNumbers] = m_CheckRegexPatterns[index];
                }
                
            }
            if (IsMatch(line, m_TargetRegexPatterns))
            {
                m_MatchLineInfo.Clear();
            }
            if (IsMatch(line, m_FunctionHeaderPatterns))
            {
                foreach (KeyValuePair<int, string> p in m_MatchLineInfo)
                {
                    m_CurFileResult[p.Key] = p.Value.Trim();
                    break;
                }
                m_MatchLineInfo.Clear();
            }
        }
        private const string C_SUFFIX = ".cpp";
        private string[] m_CheckRegexPatterns = { @"GetScene\(\)",};
        private string[] m_TargetRegexPatterns = { @"IsSceneValid", @"IsInCopyScene" };
        private string[] m_FunctionHeaderPatterns = { @"__SOL_TRACE" };
    }
}
