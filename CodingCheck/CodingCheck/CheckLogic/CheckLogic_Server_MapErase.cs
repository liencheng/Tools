using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

/* 覆盖以下测试用例
 * for(;it!=end();it++){it = map.erase(it)}      错误   
 * for(;it!=end();it++){map.erase(it);}          错误
 * for(;it!=end();){map.erase(it);continue;}     错误
 * for(;it!=end();){map.erase(it);}              错误
 * for(;it!=end();){map.erase(it);break;}        正确
 * for(;it!=end();){map.erase(it);return;}       正确
 * for(;it!=end();){it = map.erase(it)}          正确
 */
namespace CodingCheck.CheckLogic
{
    class CheckLogic_Server_MapErase :CheckLogicBase
    {
        public override Dictionary<int, string> CheckFile(FileInfo file)
        {
            CleanUp();
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
            //step1. 遇到for语句块
            for(int index = 0; index < m_CheckRegexPatterns.Length; ++index)
            {
                Match match = Regex.Match(line, m_CheckRegexPatterns[index]);
                if(match.Success)
                {

                    ForBlock fb = new ForBlock();
                    fb.m_ForStatement = line;
                    m_ForBlockList.Add(fb);
                    break;
                }

            }

            for(int idx = 0; idx < m_ForBlockList.Count;++idx )
            {
                CheckForBlock(line, lineNumbers, m_ForBlockList[idx]);
            }
        }


        void CheckForBlock(string line, int lineNumbers, ForBlock forBlock)
        {
            if(!forBlock.IsValid())
            {//不在for语句块中，不做处理
                return;
            }

            //step2. 判断for语句声明块是否结束
            if(false == forBlock.m_ForParenthesesEnd)
            { 
                //提取iterator
                Match itM = Regex.Match(line, m_ItemNamePatters);
                if(itM.Success && itM.Groups.Count>1)
                {
                    forBlock.m_IterName = itM.Groups[1].Value.Trim();
                }

                //判断for语句的左括号(
                if(IsMatch(line, m_SelfIncOrDecPatterns))
                {//包含自增自减语句
                    forBlock.m_bSelfIncOrDec = true;
                }
                MatchCollection mcL = Regex.Matches(line, @"\(");
                forBlock.PushForParentheses(mcL.Count);

                MatchCollection mcR = Regex.Matches(line, @"\)");
                if(mcR.Count > 0)
                {
                    forBlock.PopForParentheses(mcR.Count);
                    if(forBlock.m_ForParentheses <= 0)
                    {
                        forBlock.m_ForParenthesesEnd = true;
                    }
                }
            }


            if(false == forBlock.m_ForParenthesesEnd)
            {//for起始声明语句还没结束
                return;
            }

            //step3.判断for语句主体总的erase
            if(IsMatch(line, m_LeftBracket))
            {//左括弧
                forBlock.PushBracket();
            }



            Match erase0M = Regex.Match(line, m_ErasePatterns0[0]);
            if(erase0M.Success && erase0M.Groups.Count>3)
            {
                string itName = erase0M.Groups[3].Value.Trim();
                if(!string.IsNullOrEmpty(itName) && itName == forBlock.m_IterName)
                {
                    forBlock.StartEraseStatement(line, lineNumbers, true);
                }
            }
            else 
            {
                Match erase1M = Regex.Match(line, m_ErasePatterns1[0]);
                if(erase1M.Success && erase1M.Groups.Count > 2)
                {
                    string itName = erase1M.Groups[2].Value.Trim();
                    if(!string.IsNullOrEmpty(itName) && itName == forBlock.m_IterName)
                    {
                        forBlock.StartEraseStatement(line, lineNumbers, false);
                    }
                }
            }

            if(forBlock.m_bHaveEraseStatements)
            {//有erase语句
                if(!forBlock.m_bEraseReturnValue)
                {//erase没有返回值的情况

                    if(IsMatch(line, m_EndStatements))
                    {//如果有结束循环的语句，则认为该erase正常，不再检测
                        forBlock.EndEraseCodeBlock();
                    }
                    else if(IsMatch(line, m_LeftBracket) || IsMatch(line, m_RightBracket))
                    {
                        m_MatchLineInfo[forBlock.m_nEraseLine] = forBlock.m_strEraseLine;
                        forBlock.EndEraseCodeBlock();
                    }
                }
                else
                {//erase有返回值的情况,有返回值基本上算是没什么大问题了。这里再判断一下声明语句中是否有自增自减
                    if(forBlock.m_bSelfIncOrDec)
                    {//这样写认为有bug
                        //for(;;++it)
                        //{ it = map.erase(it);}
                        m_MatchLineInfo[forBlock.m_nEraseLine] = forBlock.m_strEraseLine;
                        forBlock.EndEraseCodeBlock();
                    }

                    if(IsMatch(line, m_EndStatements))
                    {//如果有结束循环的语句，则认为该erase正常，不再检测
                        forBlock.EndEraseCodeBlock();
                    }
                    else if(IsMatch(line, m_LeftBracket) || IsMatch(line, m_RightBracket))
                    {
                        forBlock.EndEraseCodeBlock();
                    }

                }
            }
            if(IsMatch(line, m_RightBracket))
            {//右括弧
                forBlock.PopBracket();
                if(forBlock.m_LeftBracket <= 0)
                {//走出for语句块  
                    forBlock.Clean();
                }
            }
        }

        void CleanUp()
        {
            m_ForBlockList.Clear();
        }

        private string[] m_CheckRegexPatterns = { @"for *\(", };
        private string m_ItemNamePatters =  @" *(\w+) *!= *(\w*)(\.*)end";
        private string[] m_ErasePatterns0 = { @" *(\w+) *= *(\w+)\.erase\((\w+)\) *;" };    // it = xxx.erase(it);
        private string[] m_ErasePatterns1 = { @" *(\w+)\.erase\((\w+)\) *;" };              // xxx.erase(it);
        private string[] m_LeftBracket = new string[] { @"{" };
        private string[] m_RightBracket = new string[] { @"}"};
        public string[] m_SelfIncOrDecPatterns = new string[] { @"\+\+ *(\w+)", @"-- *(\w+)", @"(\w+) *\+\+", @"(\w+) *--", };
        public string[] m_EndStatements = new string[] { @"return *(\w*);", "break *;"};
        public string[] m_ContinueStatements = new string[] { @"continue *;"};
        private const string C_SUFFIX = ".cpp";

        private List<ForBlock> m_ForBlockList = new List<ForBlock>();
    }
    
    class ForBlock
    {
        public ForBlock()
        {
            Clean();
        }
        public void Clean()
        {
            m_ForStatement = "";
            m_IterName = "";
            m_bSelfIncOrDec = false;
            m_bHaveEraseStatements = false;
            m_bEraseReturnValue = false;
            m_LeftBracket = 0;
            m_ForParentheses = 0;
            m_ForParenthesesEnd = false;
            m_strEraseLine = "";
            m_nEraseLine = 0;
        }
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(m_ForStatement);
        }

        public void PushBracket()
        {
            m_LeftBracket++;
        }
        public void PopBracket()
        {
            m_LeftBracket--;
        }

        public void PushForParentheses(int ct)
        {
            m_ForParentheses += ct;
        }
        public void PopForParentheses(int ct)
        {
            m_ForParentheses -= ct;
        }
        public bool IsForStatementEnd()
        {//for声明语句是否结束
            return m_ForParentheses <= 0;
        }

        public void StartEraseStatement(string line, int lineNum, bool bReturnValue)
        {
            m_bHaveEraseStatements = true;
            m_bEraseReturnValue = bReturnValue;
            m_strEraseLine = line;
            m_nEraseLine = lineNum;
        }

        public void EndEraseCodeBlock()
        {
            m_bHaveEraseStatements = false;
            m_bEraseReturnValue = false;
            m_strEraseLine = "";
            m_nEraseLine = 0;
        }

        public string m_ForStatement = "";
        public string m_IterName = "";
        public bool m_bSelfIncOrDec = false;            //for语句中是否有自增自减操作
        public bool m_bHaveEraseStatements = false;     //是否有erase语句
        public bool m_bEraseReturnValue = false;        //erase语句是否有返回值
        public int m_LeftBracket = 0;
        public int m_ForParentheses = 0;                //for语句的小括号，用于判断for起始语句得结束位
        public bool m_ForParenthesesEnd = false;
        public string m_strEraseLine = "";
        public int m_nEraseLine = 0;

    }
}
