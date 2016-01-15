using UnityEngine;
using System.Collections;
using UnityEditor;

public class UIPrefabCheckTools
{

  [MenuItem("CodeBuilderHelper/Others/CheckPrefab", false, 20)]
  private static void CheckPrefabPanelWithWidget()
  {
    string path = "UIPrefab";
    //string path = "Assets/UI/Resources/UIPrefab/AirshipBuild/Common/AirshipDevelop_CommonTop.prefab";
    Object[] allObjs = Resources.LoadAll(path);
    NGUIDebug.Log("allObjs.length:" + allObjs.Length);
    foreach (Object obj in allObjs) {
      GameObject prefab = obj as GameObject;
      if (null == prefab) continue;
      if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab) {
        CheckPrefabPanelWithWidget_Test(obj);
        //break;
      }
    }
  }
  //[MenuItem("CodeBuilderHelper/Others/CheckPrefab_test", false, 21)]
  private static void CheckPrefabPanelWithWidget_Test(Object obj)
  {
    GameObject prefab = PrefabUtility.InstantiatePrefab(obj) as GameObject;
    if (null == prefab) return;
    if (PrefabUtility.GetPrefabType(obj) == PrefabType.Prefab) {
      UIPanel[] panels = prefab.GetComponentsInChildren<UIPanel>();
      for (int i = 0; i < panels.Length; ++i) {
        if (panels[i].GetComponent<UIWidget>()) {
          Debug.LogError("prefab name is:" + prefab.name + " ...:" + panels[i].name);
          GameObject.DestroyImmediate(panels[i]);
        }
      }
      PrefabUtility.ReplacePrefab(prefab, obj);
      GameObject.DestroyImmediate(prefab);
    }
  }
}

