using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace CodingCheck
{
  public partial class CheckForm : Form
  {
    CodeChecking m_CheckThread;
    public CheckForm()
    {
      InitializeComponent();
      Init();

    }
    void Init()
    {
      m_CheckThread = new CodeChecking();
      m_CheckThread.InitThread();
      InitCheckTypeDropList();
      CodeChecking.CheckingDisplayHandler += HandleCheckingDisplay;
      CodeChecking.CheckingResultHandler += HandleCheckingResult;
      CodeChecking.CheckingEndHandler += HandleCheckingEnd;
      CodeChecking.DebugHandler += HandleDebugLog;
    }

    private void InitCheckTypeDropList()
    {
        checkTypeBox.Items.Add("Server-GetScene检查");
        checkTypeBox.Items.Add("Server-GetXXXById检查");
        checkTypeBox.Items.Add("Server-除0检查");
        checkTypeBox.Items.Add("Server-Erase检查");
        checkTypeBox.Items.Add("Client-Mono脚本Instance检查");
        checkTypeBox.Items.Add("Client-Mono脚本Public检查");
        checkTypeBox.Items.Add("Client-协程中空引用检查");
        checkTypeBox.SelectedIndex = 0;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      string path = textBox1.Text;
      if (string.IsNullOrEmpty(path)) {
        Console.Write("hello button1_Click");
        return;
      }
      textBox2.Clear();
      m_CheckThread.Start(path);  
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {

    }

    private void button2_Click(object sender, EventArgs e)
    {
      string path;
      FolderBrowserDialog dialog = new FolderBrowserDialog();
      dialog.Description = "请选择文件夹";
      if (dialog.ShowDialog() == DialogResult.OK || dialog.ShowDialog() == DialogResult.Yes) {
        path = dialog.SelectedPath;
        textBox1.Text = path;
      }
    }
    private void HandleCheckingDisplay(string fileName)
    {
      if (CheckingDisplay.InvokeRequired) {
        CodeChecking.CheckingDisplayDelegate handle = new CodeChecking.CheckingDisplayDelegate(HandleCheckingDisplay);
        Invoke(handle, new object[] { fileName });
      } else {
        CheckingDisplay.Text = fileName;
      }
    }
    private void HandleCheckingResult(string fileName, Dictionary<int, string> ret)
    {
      if (textBox2.InvokeRequired) {
        CodeChecking.CheckingResultDelegate handle = new CodeChecking.CheckingResultDelegate(HandleCheckingResult);
        Invoke(handle, new object[] { fileName, ret });
      } else {
        textBox2.AppendText(fileName);
        textBox2.AppendText("\n");
        foreach (KeyValuePair<int, string> pair in ret) {
         string text = string.Format("Line:{0},  {1}\n", pair.Key,pair.Value);
          textBox2.AppendText(text);
        }
      }
    }

    private void HandleDebugLog(string debugLog)
    {
        if(textBox2.InvokeRequired)
        {
            CodeChecking.CheckingResultDelegate handle = new CodeChecking.CheckingResultDelegate(HandleCheckingResult);
            Invoke(handle, new object[] { debugLog });
        }
        else
        {
            textBox2.AppendText(debugLog);
            textBox2.AppendText("\n");
        }
    }
    private void HandleCheckingEnd()
    {
      MessageBox.Show("检测完成");
    }


    private void comboBox1_OnCheckTypeChanged(object sender, EventArgs e)
    {
        if(checkTypeBox.SelectedIndex<0 || checkTypeBox.SelectedIndex>=(int)CheckLogic.CheckTypeEnum.C_Max)
        {
            MessageBox.Show("类型错误");
            return;
        }
        m_CheckThread.SetCheckType((CheckLogic.CheckTypeEnum)checkTypeBox.SelectedIndex);
    }

    private void CheckForm_Load(object sender, EventArgs e)
    {

    }

    private void CheckForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        m_CheckThread.Stop();
    }
  }
}
