using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace CodingCheck
{
  static class Program
  {
    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
    }
  }
  internal class CodeChecking
  {
    #region Event
    internal delegate void CheckingDisplayDelegate(string checkingFile);
    internal delegate void CheckingResultDelegate(string fileName, Dictionary<int, string> ret);
    internal delegate void CheckingVoidDelegate();
    internal static CheckingDisplayDelegate CheckingDisplayHandler;
    internal static CheckingResultDelegate CheckingResultHandler;
    internal static CheckingVoidDelegate CheckingEndHandler;
    #endregion
    private string[] m_ObjPattern = {
                                   @"By\w*Id",
                                   @"ConfigProvider",
                                 };
    private string[] m_ExistPattern ={
                                       @"\b+!=\b+",
                                     };
    private DirectoryInfo m_RootDirectory;
    private Dictionary<string, Dictionary<int, string>> m_CurDirResult = new Dictionary<string, Dictionary<int, string>>();
    private Dictionary<int, string> m_CurFileResult = new Dictionary<int, string>();
    private string m_RootPath;
    private Thread m_Thread;
    public CodeChecking()
    {
    }
    internal void InitThread()
    {
      m_Thread = new Thread(new ThreadStart(Loop));
      m_Thread.Start();
    }
    private bool m_Start = false;
    private void Loop()
    {
      while (true) {
        if (m_Start) {
          StartCheck();
          m_Start = false;
        }
      }
    }
    internal void Start(string path)
    {
      m_RootPath = path;
      m_RootDirectory = new DirectoryInfo(path);
      m_Start = true;
    }
    internal void StartCheck()
    {
      StartCheck(m_RootPath);
      if (null != CheckingEndHandler) {
        CheckingEndHandler();
      }
    }
    internal void Stop()
    {

    }
    internal void StartCheck(string path)
    {
      if (string.IsNullOrEmpty(path)) return;
      FileInfo file = new FileInfo(path);
      if (file.Exists) {
        CheckFile(file);
        return;
      }
      DirectoryInfo dir = new DirectoryInfo(path);
      if (!dir.Exists) {
        Console.Write(path + "is not exist!");
        return;
      }
      FileSystemInfo[] systemInfo = dir.GetFileSystemInfos();
      for (int index = 0; index < systemInfo.Length; ++index) {
        if (null != systemInfo[index]) {
          if (IsFile(systemInfo[index].FullName, out file)) {
            CheckFile(file);
          } else if (IsFolder(systemInfo[index].FullName)) {
            StartCheck(systemInfo[index].FullName);
          }
        }
      }
    }
    private bool IsFolder(string path)
    {
      bool ret = false;
      DirectoryInfo dir = new DirectoryInfo(path);
      if (dir.Exists)
        ret = true;
      return ret;
    }
    private bool IsFile(string path, out FileInfo fileInfo)
    {
      bool ret = false;
      FileInfo file = new FileInfo(path);
      fileInfo = file;
      if (file.Exists)
        ret = true;
      return ret;
    }
    private void CheckFile(FileInfo file)
    {
      m_CurFileResult.Clear();
      objList.Clear();
      objDict.Clear();
      int lineNumber = 0;
      if (null != file
        && file.Name.IndexOf('.') > 0
        && file.FullName.Substring(file.FullName.LastIndexOf(".")) == ".cs") {
        if (null != CheckingDisplayHandler) {
          CheckingDisplayHandler(file.FullName);
        }
        StreamReader reader = file.OpenText();
        if (null != reader) {
          while (reader.Peek() > 0) {
            string line = reader.ReadLine();
            CheckLine(line, lineNumber++);
          }
        }
      }
      if (m_CurFileResult.Count > 0) {
        Dictionary<int, string> ret = new Dictionary<int, string>();
        foreach (int key in m_CurFileResult.Keys) {
          ret[key] = m_CurFileResult[key];
        }
        m_CurDirResult[file.Name] = ret;
        if (null != CheckingResultHandler) {
          CheckingResultHandler(file.FullName, ret);
        }
      }
    }
    private List<string> objList = new List<string>();
    private Dictionary<int, string> objDict = new Dictionary<int, string>();
    private void CheckLine(string line, int lineNumbers)
    {
      for (int index = 0; index < m_ObjPattern.Length; ++index) {
        Match match = System.Text.RegularExpressions.Regex.Match(line, m_ObjPattern[index]);
        if (match.Success) {
          //判断是否为 Class c = GetConfigById()的形式
          Match matchValue = Regex.Match(line, @" *(\w+\.?\w+) *=");
          if (matchValue.Groups.Count > 1) {
            objDict[lineNumbers] = matchValue.Groups[1].Value.Trim();
          }
        } else {
          //判断是否有判空的情况。如果有则加入到keys
          List<int> keys = new List<int>();
          foreach (KeyValuePair<int, string> pair in objDict) {
            //这块的正则表达式应该要改一下。
            string parttern = "( *== *| *!= *)";
            Regex regex = new Regex(parttern + pair.Value);
            if (regex.IsMatch(line)) {
              keys.Add(pair.Key);
              continue;
            }
            regex = new Regex(pair.Value + parttern);
            if (regex.IsMatch(line)) {
              keys.Add(pair.Key);
              continue;
            }
            //匹配使用对象的情况
            regex = new Regex(pair.Value + @"\.");
            if (regex.IsMatch(line)) {
              m_CurFileResult[lineNumbers] = pair.Value.Trim();
            }
          }
          //这些是已经判空的  可以删除了。
          for (int k = 0; k < keys.Count; ++k) {
            if (objDict.ContainsKey(keys[k])) {
              objDict.Remove(keys[k]);
            }
          }
          //for end
        }
        //else end;
      }
      //for end;
    }//Func CheckLine end.
    #region public funcc
    public Dictionary<string, Dictionary<int, string>> GetCheckResult()
    {
      return m_CurDirResult;
    }
    #endregion
  }//Class end.
}
