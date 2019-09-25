using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace CodingCheck.CheckLogic
{
    class CheckLogic_Server_GetXXXById : CheckLogicBase
    {
        public CheckLogic_Server_GetXXXById()
        {
            BuildFullData();
        }
        private Dictionary<int, string> DoCheckFile(FileInfo file, Params p)
        {
            m_LineNumber = 0;
            m_MatchLineInfo.Clear();
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
            //倒叙查
            for(int idx = lineCache.Count - 1; idx >= 0; --idx)
            {
                CheckLine(lineCache[idx].content, lineCache[idx].lineNum, p);
            }
            //匹配使用对象的情况
            foreach(KeyValuePair<int, string> pair in m_MatchLineInfo)
            {
                ret[pair.Key] = pair.Value.Trim();
                break;
            }
            return ret;
           
        }

        public override Dictionary<int, string> CheckFile(FileInfo file)
        {
            Dictionary<int, string> ret = new Dictionary<int, string>();
            if(!IsSuffixFile(file, C_SUFFIX))
            {
                return ret;
            }
            Dictionary<int, string> tmpRet = new Dictionary<int, string>();
            for(int idx=0;idx<m_FullData.Count;++idx)
            {
                tmpRet.Clear();
                tmpRet = DoCheckFile(file, m_FullData[idx]);
                foreach(KeyValuePair<int, string> pair in tmpRet)
                {
                    ret[pair.Key] = pair.Value;
                }
            }
            return ret;
        }
        public void CheckLine(string line, int lineNumbers, Params curCheckRegexPatterns)
        {
            if(IsCommentLine(line))
            {
                return;
            }
            Params p = curCheckRegexPatterns;
            if(null == p)
                return;


            for(int index = 0; index < p.m_CurCheckRegexPatters.Count; ++index)
            {
                Match match = Regex.Match(line, p.m_CurCheckRegexPatters[index]);
                if(match.Success)
                {
                    m_MatchLineInfo[lineNumbers] = p.m_CurCheckRegexPatters[index];
                }

            }
            if(IsMatch(line, p.m_CurTargetRegexPatters))
            {
                m_MatchLineInfo.Clear();
            }
            if(IsMatch(line, m_FunctionHeaderPatterns))
            {
                return;
            }
        }

        private void BuildFullData()
        {
            m_FullData.Clear();
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetUnitByIndex"}, new List<string>(){"IsIndexValid"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetCooldownById"}, new List<string>(){"IsCooldownIdValid"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetCooldownByIndex"}, new List<string>(){"IsCooldownIndexValid"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetElemByIndex"}, new List<string>(){"IsIndexValid"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetElemByGuid"}, new List<string>(){"IsGuidValid"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetBattleFairy"}, new List<string>(){"BattleFairyIsValid"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetGemByIndex"}, new List<string>(){"IsValidByIndex"}));
            m_FullData.Add(Params.BuildParams(new List<string>() { "GetGemBySlotId"}, new List<string>(){"IsValidBySlotId"}));
        }

        private const string C_SUFFIX = ".cpp";
        private string[] m_FunctionHeaderPatterns = { @"__SOL_TRACE" };

        private List<Params> m_FullData = new List<Params>();
    }

    public class Params
    {
        public List<string> m_CurCheckRegexPatters = new List<string>();
        public List<string> m_CurTargetRegexPatters = new List<string>();
        public static Params BuildParams(List<string> curCheckList, List<string> targetList)
        {
            Params p = new Params();
            p.m_CurCheckRegexPatters.AddRange(curCheckList);
            p.m_CurTargetRegexPatters.AddRange(targetList);
            return p;
        }
    }
}
