using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CodingCheck.CheckLogic;
namespace CodingCheck
{
    static class MainLogic
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CheckForm());
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
        internal static CheckingDisplayDelegate DebugHandler;
        #endregion
        private DirectoryInfo m_RootDirectory;
        private string m_RootPath;
        private Thread m_Thread;
        private CheckTypeEnum m_CheckType = CheckTypeEnum.C_Client_InstanceCheck;
        private CheckLogicBase m_CheckLogic;
        private Dictionary<string, Dictionary<int, string>> m_CurDirResult = new Dictionary<string, Dictionary<int, string>>();
       
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
            while (true)
            {
                if (m_Start)
                {
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
            if (null != CheckingEndHandler)
            {
                CheckingEndHandler();
            }
        }
        internal void Stop()
        {
            m_Thread.Abort();
        }
        internal void StartCheck(string path)
        {
            m_CheckLogic = CheckLogicManager.Instance.GetCheckLogic(m_CheckType);
            if (string.IsNullOrEmpty(path)) return;
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                Dictionary<int, string> ret = m_CheckLogic.StartCheckFile(file);
                if(null!=ret && ret.Count>0)
                {
                    m_CurDirResult[file.Name] = ret;
                    if (null != CheckingResultHandler)
                    {
                        CheckingResultHandler(file.FullName, ret);
                    }
                }
                if (null != CheckingDisplayHandler)
                {
                    CheckingDisplayHandler(file.FullName);
                }
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                Console.Write(path + "is not exist!");
                return;
            }
            FileSystemInfo[] systemInfo = dir.GetFileSystemInfos();
            for (int index = 0; index < systemInfo.Length; ++index)
            {
                if (null != systemInfo[index])
                {
                    if (IsFile(systemInfo[index].FullName, out file))
                    {
                        Dictionary<int, string> ret = m_CheckLogic.StartCheckFile(file);
                        if (null != ret && ret.Count > 0)
                        {
                            m_CurDirResult[file.Name] = ret;
                            if(null!=CheckingResultHandler)
                            {
                                CheckingResultHandler(file.FullName, ret);
                            }
                        }
                        if (null != CheckingDisplayHandler)
                        {
                            CheckingDisplayHandler(file.FullName);
                        }
                    }
                    else if (IsFolder(systemInfo[index].FullName))
                    {
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
        #region public funcc
        public void SetCheckType(CheckTypeEnum type)
        {
            m_CheckType = type;
        }
        #endregion
    }//Class end.
}