using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CodingCheck.CheckLogic
{
    public class CheckLogic_Server_ZeroCheck:CheckLogicBase
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
        private bool IsQuater(string line)
        {
            return line.Contains("\"");
        }
        public override void CheckLine(string line, int lineNumbers)
        {
            if(IsQuater(line))   //忘了当初为什么要这么写
            {
                return;
            }
            //注释， 排除
            if(IsCommentLine(line))
            {
                return;
            }
            for (int index = 0; index < m_CheckRegexPatterns.Length; ++index)
            {
                Match match = Regex.Match(line, m_CheckRegexPatterns[index]);
                if (match.Success)  //匹配到除法操作
                {
                    //m_MatchLineInfo[lineNumbers] = m_CheckRegexPatterns[index];
                    Match matchValue = Regex.Match(line, @" *\w* */ *(\w*)");    //获取被除数的值类型
                    if(matchValue.Groups.Count>1)
                    {
                        //如果强转为float，则排除
                        if(matchValue.Groups[1].Value.Trim() == "float")
                        {
                            continue;
                        }
                        //如果为纯数字，则排除
                        Match isNum = Regex.Match(matchValue.Groups[1].Value.Trim(), @"^[0-9]*$");
                        if(isNum.Success)
                        {
                            continue;
                        }
                        //认为所有的大写都是宏，不做判断
                        Match isCH = Regex.Match(matchValue.Groups[1].Value.Trim(), @"^[A-Z]+((_)*[A-Z]+)*$");
                        if(isCH.Success)
                        {
                            continue;
                        }
                        m_MatchLineInfo[lineNumbers] = matchValue.Groups[1].Value.Trim();
                    }
                }
                else
                {
                    //判断是否有判空的情况，如果有则加入到keys
                    List<int> keys = new List<int>();
                    foreach(KeyValuePair<int, string> pair in m_MatchLineInfo)
                    {
                        string parttern1 = "( *== *| *!= *)";
                        Regex r = new Regex(parttern1 + pair.Value);
                        if(r.IsMatch(line))
                        {
                            keys.Add(pair.Key);
                        }
                        string parttern2 = @"( *(\))* *>(=)* *| *<(=)* *)";
                        r = new Regex(parttern2 + pair.Value);
                        if(r.IsMatch(line))
                        {
                            keys.Add((pair.Key));
                            continue;
                        }
                        r = new Regex(pair.Value + parttern2);
                        if(r.IsMatch(line))
                        {
                            keys.Add(pair.Key);
                            continue;
                        }

                        // 排除float double
                        string p3 = @"(float *)";
                        r = new Regex(p3 + pair.Value);
                        if(r.IsMatch(line))
                        {
                            keys.Add(pair.Key);
                            continue;
                        }
                    }
                    //这些事已经判空的，可以删除了
                    for(int k =0;k<keys.Count;++k)
                    {
                        if(m_MatchLineInfo.ContainsKey(keys[k]))
                        {
                            m_MatchLineInfo.Remove(keys[k]);
                        }
                    }
                }
                
            }
        }
        private const string C_SUFFIX = ".cpp";
        private string[] m_CheckRegexPatterns = { @"(\w)* */ *(\w)*",};
        private string[] m_TargetRegexPatterns = { @"IsSceneValid", @"IsInCopyScene" };
        private string[] m_FunctionHeaderPatterns = { @"__SOL_TRACE" };
    }
}
