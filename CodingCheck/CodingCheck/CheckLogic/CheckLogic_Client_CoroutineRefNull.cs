using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

//客户端-协程中的未判空引用检查
/*
 * 1、获取类中所有成员变量（保留引用类型，去除值类型）
 * 2、查找所有协程函数
 * 3、yield前面加了空引用判断的，如果yield后面用到该变量也必须判空
 * */
namespace CodingCheck.CheckLogic
{
    class CheckLogic_Client_CoroutineRefNull : CheckLogicBase
    {
         public override Dictionary<int, string> CheckFile(System.IO.FileInfo file)
        {
            m_AllVariables.Clear();
            Dictionary<int, string> ret = new Dictionary<int, string>();
            if (!IsSuffixFile(file, C_SUFFIX))
            {
                return ret;
            }
            if(IsProtoBuf(file))
            {
                return ret;
            }

             if(IsGameTable(file))
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

            //ExtractAllVar(lineCache);

            for (int idx = 0; idx < lineCache.Count; ++idx)
            {
                CheckLine(lineCache[idx].content, lineCache[idx].lineNum);
            }
            foreach (KeyValuePair<int, string> pair in m_MatchLineInfo)
            {
                m_CurFileResult[pair.Key] = pair.Value.Trim();
            }
            foreach (int key in m_CurFileResult.Keys)
            {
                ret[key] = m_CurFileResult[key];
            }
            return ret;
        }
        public override void CheckLine(string line, int lineNumbers)
        {
            if(IsCommentLine(line))
            {
                return;
            }
            for (int index = 0; index < m_MatchPatterns.Length; ++index)
            {
                Match match = Regex.Match(line, m_MatchPatterns[index]);
                if (match.Success && match.Groups.Count>=3)
                {
                    m_CurFunc.Clean();
                    m_CurFunc.FuncName = match.Groups[1].Value;                 
                    break;
                }
            }
            
            if(!m_CurFunc.IsValid())
            {//不是在协程函数中，无需继续处理
                return;
            }

            if(IsMatch(line, m_BreakPatterns))
            {//yiled break
                m_CurFunc.Clean();
                return;
            }

            if(IsMatch(line, m_LeftBracket))
            {
                m_CurFunc.PustBracket();
            }
            if(IsMatch(line, m_RightBracket))
            {
                m_CurFunc.PopBracket();
                if(m_CurFunc.GetBracketNum() <=0)
                {//协程函数结束
                    m_CurFunc.Clean();
                    return;
                }
            }
            
            for(int idx=0;idx<m_NullPatterns.Length;++idx)
            {
                Match m = Regex.Match(line, m_NullPatterns[idx]);
                if(!m.Success)
                {
                    continue;
                }
                string strMaybeNullData = m.Groups[2].Value.Trim();
                m_CurFunc.MaybeNullVar.Add(strMaybeNullData);
                m_CurFunc.JudgeNullVar.Add(strMaybeNullData);
                break;
            }

            if(IsMatch(line, m_YeildPatterns))
            {
                m_CurFunc.JudgeNullVar.Clear();
                return;
            }

            ///用空格符号来判断引用语句
            string[] refDataList = line.Split(new char[]{' '});   
            if(null!=refDataList && refDataList.Length>1)
            {//引用数据
                for(int idx=0;idx<refDataList.Length;++idx)
                {
                    string refData = refDataList[idx].Trim();
                    if(!refData.Contains('.'))
                    {
                        continue;
                    }
                    if(!string.IsNullOrEmpty(refData) && m_CurFunc.IsErrorRef(refData))
                    {
                        m_MatchLineInfo[lineNumbers] = refData;
                        break;
                    }
                }
            }
            
        }

        void ExtractAllVar(List<LineInfo> lineList)
        {
            for(int idx=0;idx<lineList.Count;++idx)
            {
                string var = ExtractClassMemberVar(lineList[idx].content);
                if(!string.IsNullOrEmpty(var))
                {
                    m_AllVariables.Add(var);
                }
            }
        }

        public string[] m_MatchPatterns = new string[] { @" *IEnumerator *(\w+) *\((.)*\)",};
        //第一个if加了(),纯粹为了统一子Match,序列一致。
        public string[] m_NullPatterns = new string[] { @" *(if) *\((.+) *(=|!)= *null *\)", @" *if *\( *null *(=|!)= *(.+) *\)" };
        public string[] m_LeftBracket = new string[] { @"{" };
        public string[] m_RightBracket = new string[] { @"}"};
        public string[] m_YeildPatterns = new string[] { @"yield" };
        public string[] m_BreakPatterns = new string[] { @" *yield *break *;"};
        private CoroutineFunc m_CurFunc = new CoroutineFunc();
        private const string C_SUFFIX = ".cs";
        private List<string> m_AllVariables = new List<string>();
        
    }

    public class CoroutineFunc
    {
        public CoroutineFunc()
        {
            Clean();
        }
        public void Clean()
        {
            MaybeNullVar.Clear();
            JudgeNullVar.Clear();
            LeftBracket = 0;
            FuncName = "";
        }
        public string FuncName;          //函数名
        public List<string> MaybeNullVar = new List<string>();   //需要判空的变量
        public List<string> JudgeNullVar = new List<string>();   //已经判空的对象
        private int LeftBracket = 0;
        public void PustBracket()
        {
            LeftBracket++;
        }
        public void PopBracket()
        {
            LeftBracket--;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(FuncName);
        }
        public int GetBracketNum()
        {
            return LeftBracket;
        }

        public bool IsErrorRef(string refData)
        {
            bool bMaybeNull = false;

            string var = "";
            for(int idx=0;idx<MaybeNullVar.Count;++idx)
            {
               if(refData.Contains(MaybeNullVar[idx] + "."))
               {
                   bMaybeNull = true;
                   var = MaybeNullVar[idx];
                   break;
               }
            }
            if(!bMaybeNull)
            {
                return false;
            }
            bool bJudged = false;
            for(int idx=0;idx<JudgeNullVar.Count;++idx)
            {
                if(var == JudgeNullVar[idx])
                {
                    bJudged = true;
                    break;
                }
            }
            return !bJudged;
        }
    }
}
