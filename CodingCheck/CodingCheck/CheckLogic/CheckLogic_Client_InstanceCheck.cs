using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CodingCheck.CheckLogic
{
    class CheckLogic_Client_InstanceCheck:CheckLogicBase
    {
        public override Dictionary<int, string> CheckFile(System.IO.FileInfo file)
        {
            m_FuncList.Clear();
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
            for (int idx = 0; idx < lineCache.Count; ++idx)
            {
                CheckLine(lineCache[idx].content, lineCache[idx].lineNum);
            }
            //匹配使用对象的情况
            if(IsValid())
            {
                return ret;
            }
            foreach (KeyValuePair<int, string> pair in m_MatchLineInfo)
            {
                m_CurFileResult[pair.Key] = pair.Value.Trim();
            }
            foreach (int key in m_CurFileResult.Keys)
            {
                ret[key] = m_CurFileResult[key];
                break;   //只要输出一行即可
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
                if (match.Success)
                {
                    if(null!=m_CurFunc &&m_CurFunc.InsOp!= Func.InstanceOp.None)
                    {
                        m_FuncList.Add(m_CurFunc);
                    }
                    m_CurFunc = new Func(OnFuncEnd, m_MatchPatterns[index]);
                    m_MatchLineInfo[lineNumbers] = m_MatchPatterns[index];
                    break;
                }
            }
            if(null== m_CurFunc)
            {
                return;
            }
            if(IsMatch(line, m_SetPatterns))
            {
                m_CurFunc.InsOp = Func.InstanceOp.SetInstance;
                m_FuncList.Add(m_CurFunc);
                m_CurFunc = null;
                return;
            }
            if (IsMatch(line, m_NullPatterns))
            {
                m_CurFunc.InsOp = Func.InstanceOp.SetNull;
                m_FuncList.Add(m_CurFunc);
                m_CurFunc = null;
                return;
            }
            if (IsMatch(line, m_LeftBracket))
            {
                m_CurFunc.PustBracket();
            }
            if (IsMatch(line, m_RightBracket))
            {
                m_CurFunc.PopBracket();
            }
        }
        private void OnFuncEnd()
        {
            if (null != m_CurFunc && m_CurFunc.InsOp != Func.InstanceOp.None)
            {
                m_FuncList.Add(m_CurFunc);
            }
            m_CurFunc = null;
        }
        private bool IsValid()
        {
            if (m_FuncList.Count <= 0)
            {
                return true;
            }
            if (m_FuncList.Count != 2)
            {
                return false;
            }
            if(Contains("Awake") && Contains("OnDestroy"))
            {
                return true;
            }
            if(Contains("Start") && Contains("OnDestroy"))
            {
                return true;
            }
            if(Contains("OnEnable") && Contains("OnDisable"))
            {
                return true;
            }
            return false;
        }

        private bool Contains(string value)
        {
            for(int idx=0;idx<m_FuncList.Count;++idx)
            {
                if(m_FuncList[idx].FuncName.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }

        public string[] m_MatchPatterns = new string[] { @"void *Awake *\( *\)", @"void *Start *\( *\)", @"void *OnEnable *\( *\)", @"void *OnDestroy *\( *\)", @"void *OnDisable *\( *\)",};
        public string[] m_SetPatterns = new string[] { @"Instance *= *this" };
        public string[] m_NullPatterns = new string[] { @"Instance *= *null"};
        public string[] m_LeftBracket = new string[] { @"{" };
        public string[] m_RightBracket = new string[] { @"}"};
        private List<Func> m_FuncList = new List<Func>();
        private Func m_CurFunc = null;
        private const string C_SUFFIX = ".cs";
        
    }
    public class Func
    {
        public Func(FunEndDelegate handler, string funcname)
        {
            FunEndHandler = handler;
            FuncName = funcname;
        }
        public delegate void FunEndDelegate();
        public FunEndDelegate FunEndHandler;
        public enum InstanceOp
        {
            None,
            SetInstance,
            SetNull
        }
        public string FuncName;
        public InstanceOp InsOp = InstanceOp.None;
        public int LeftBracket = 0;
        public void PustBracket()
        {
            LeftBracket++;
        }
        public void PopBracket()
        {
            LeftBracket--;
            if (LeftBracket <= 0 && null != FunEndHandler)
            {
                FunEndHandler();
            }
        }
    }
}
