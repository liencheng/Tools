using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCheck
{
  internal class CheckThread
  {
    private Thread m_Thread;
    private bool m_IsRun = false;
    public CheckThread()
    {
      InitThread();
    }
    private void InitThread()
    {
      m_Thread = new Thread(Loop);
    }
    private void Loop()
    {
      while(m_IsRun) {

      }
    }
    #region public
    internal void Start()
    {
      m_IsRun = true;
      m_Thread.Start();
    }
    internal void Stop()
    {
      
    }
	
    #endregion
  }
}
