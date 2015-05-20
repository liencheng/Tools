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
  public partial class Form1 : Form
  {
    CodeChecking m_CheckThread;
    public Form1()
    {
      InitializeComponent();
      Init();

    }
    void Init()
    {
      m_CheckThread = new CodeChecking();
      m_CheckThread.InitThread();
      CodeChecking.CheckingDisplayHandler += HandleCheckingDisplay;
      CodeChecking.CheckingResultHandler += HandleCheckingResult;
      CodeChecking.CheckingEndHandler += HandleCheckingEnd;
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
    private void HandleCheckingEnd()
    {
      MessageBox.Show("检测完成");
    }
  }
}
