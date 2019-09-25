using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CodingCheck.CheckLogic
{
    public class CheckLogicBase
    {
        protected Dictionary<int, string> m_MatchLineInfo = new Dictionary<int, string>();
        protected Dictionary<int, string> m_CurFileResult = new Dictionary<int, string>();
        protected int m_LineNumber = 1;
        public Dictionary<int, string> StartCheckFile(FileInfo file)
        {
            m_MatchLineInfo.Clear();
            m_CurFileResult.Clear();
            m_LineNumber = 1;
            
           return CheckFile(file);
        }
        public virtual Dictionary<int, string> CheckFile(FileInfo file)
        {
            return new Dictionary<int, string>();
        }

        public virtual void CheckLine(string line, int lineNumbers)
        {

        }

        protected bool IsCommentLine(string line)
        {
            if(string.IsNullOrEmpty(line))
            {
                return false;
            }

            Match matchExcept = Regex.Match(line, @"^ *//");
            return matchExcept.Success;
        }

        protected bool IsGameTable(FileInfo file)
        {
            if(file == null)
            {
                return false;
            }

            return file.FullName.Contains("GameTables");
        }

        protected bool IsProtoBuf(FileInfo file)
        {
            if(null == file)
            {
                return false;
            }

            return file.FullName.Contains("Protobuf");

        }
        protected bool IsSuffixFile(FileInfo file, string suffix)
        {
            if (null != file
              && file.Name.IndexOf('.') > 0
              && file.FullName.Substring(file.FullName.LastIndexOf(".")) == suffix)
            {
                return true;
            }
            return false;
        }
        protected bool IsFolder(string path)
        {
            bool ret = false;
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
                ret = true;
            return ret;
        }
        protected bool IsFile(string path, out FileInfo fileInfo)
        {
            bool ret = false;
            FileInfo file = new FileInfo(path);
            fileInfo = file;
            if (file.Exists)
                ret = true;
            return ret;
        }
        protected bool IsMatch(string line, string[] patterns)
        {
            for(int idx=0;idx<patterns.Length;++idx)
            {
                Regex regex = new Regex(patterns[idx]);
                if (regex.IsMatch(line))
                {
                    return true;
                }
            }
            return false;
        }
        protected bool IsMatch(string line, List<string> patterns)
        {
            for(int idx=0;idx<patterns.Count;++idx)
            {
                Regex regex = new Regex(patterns[idx]);
                if (regex.IsMatch(line))
                {
                    return true;
                }
            }
            return false;
        }

        protected string ExtractClassMemberVar(string line)
        {
            // xxx Var = ;

            string[] exceptPatterns = new string[]
            {
                "return",
                "using",
            };

            if(IsMatch(line, exceptPatterns))
            {
                return "";
            }

            string []varPatterns =new string[]{
                @"(\[|\]|<|>|\w)+ +((_|[a-zA-Z])((_|\d|[a-zA-Z]))+) *;",
                 @"(\[|\]|<|>|\w)+ +((_|[a-zA-Z])((_|\d|[a-zA-Z]))+) *=",};
            for(int idx=0;idx<varPatterns.Length;++idx)
            {
                Regex r = new Regex(varPatterns[idx]);
                Match mc = r.Match(line);
                if(mc.Success)
                {
                    return mc.Groups[2].Value.Trim();
                }
            }
            return "";
        }

        protected void DebugLog(string msg)
        {
            if(null!=CodeChecking.DebugHandler)
            {
                CodeChecking.DebugHandler("helloo");
            }
        }
    }
}
