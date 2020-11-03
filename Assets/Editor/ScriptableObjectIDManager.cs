using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScriptableObjectIDManager : EditorWindow
{
    [MenuItem("Tools/Scriptable Object ID Manager")]
    public static void Open()
    {
        GetWindow<ScriptableObjectIDManager>();
    }

    public void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.BeginVertical("box");
        DrawButtons();
        EditorGUILayout.EndVertical();

        obj.ApplyModifiedProperties();
    }

    void DrawButtons()
    {
        if (GUILayout.Button("Set ItemObject ID's"))
        {
            FindSOandSetIDs();
        }
    }

    void FindSOandSetIDs()
    {
        List<ItemObject> list = FindAssetsByType<ItemObject>();
        Debug.Log("Item Object occurences: " + list.Count);

        int highestID = FindHighestID(list);
        SetIDs(list, highestID);
        AssetDatabase.SaveAssets();
    }

    private List<T> FindAssetsByType<T>() where T : UnityEngine.Object
    {
        List<T> assets = new List<T>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                assets.Add(asset);
            }
        }
        return assets;
    }

    private int FindHighestID(List<ItemObject> _list)
    {
        int max = 0;

        foreach(ItemObject item in _list)
        {
            if(item.itemID > max)
            {
                max = item.itemID;
            }
        }
        return max;
    }

    private void SetIDs(List<ItemObject> _list, int _currentID)
    {
        foreach (ItemObject item in _list)
        {
            if(item.itemID == 0)
            {
                _currentID++;
                item.itemID = _currentID;
            }
        }
    }
}
