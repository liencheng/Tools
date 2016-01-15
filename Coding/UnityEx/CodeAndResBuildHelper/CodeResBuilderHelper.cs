using System.IO;
using System.Diagnostics;
using UnityEngine;
using System.Collections;
using UnityEditor;

public class CodeResBuilderHelper {
  private static string s_RunServer = @"..\..\DcoreEnv\" + "RunServer.cmd";
  private static string s_StopServer = @"..\..\DcoreEnv\" + "StopServer.cmd";
  private static string s_BuildCodeClient = @"..\..\" + "build_code_client.bat";
  private static string s_BuildCodeServer = @"..\..\" + "build_code_server.bat";
  private static string s_BuildAll = @"..\..\" + "build.bat";
  private static string s_CopyResRelease = @"..\..\" + "copy_res_release.bat";
  private static string s_SvnUpdateAll = @"..\..\" + "svnupdate.bat";
  private static string s_TableGenerator = @"..\..\" + "table_tools.bat";

  [MenuItem("CodeBuilderHelper/RunServer",false,0)]
  public static void RunServer()
  {
    ProcessBatFile(s_RunServer);
  }
  [MenuItem("CodeBuilderHelper/StopServer",false,1)]
  public static void StopServer()
  {
    ProcessBatFile(s_StopServer);
  }
  [MenuItem("CodeBuilderHelper/Build Code Client",false,2)]
  public static void BuildCodeClient()
  {
    ProcessBatFile(s_BuildCodeClient);
  }
  [MenuItem("CodeBuilderHelper/Build Code Server",false,3)]
  public static void BuildCodeServer()
  {
    ProcessBatFile(s_BuildCodeServer);
  }
  [MenuItem("CodeBuilderHelper/Build All",false,4)]
  public static void BuildAll()
  {
    ProcessBatFile(s_BuildAll);
  }
  [MenuItem("CodeBuilderHelper/Copy Resource Release",false,5)]
  public static void BuildCopyResRelease()
  {
    ProcessBatFile(s_CopyResRelease);
  }
  [MenuItem("CodeBuilderHelper/Svn Update",false,6)]
  public static void BuildSvnUpdate()
  {
    ProcessBatFile(s_SvnUpdateAll);
  }
  [MenuItem("CodeBuilderHelper/Others/table_generator",false,7)]
  public static void BuildTableGenerator()
  {
    ProcessBatFile(s_TableGenerator);
  }

  public static void ProcessBatFile(string path)
  {
    int lastIdx = path.LastIndexOf(@"\");
    string newPath = path.Substring(0, lastIdx);
    string bat = path.Substring(lastIdx+1);
    UnityEngine.Debug.Log(newPath);
    UnityEngine.Debug.Log(bat);
    string preDirectory = System.IO.Directory.GetCurrentDirectory();
    System.IO.Directory.SetCurrentDirectory(newPath);
    Process p = Process.Start(bat);
    System.IO.Directory.SetCurrentDirectory(preDirectory);
  }
}
