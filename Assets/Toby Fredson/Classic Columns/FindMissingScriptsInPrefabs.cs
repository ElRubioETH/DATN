using UnityEngine;
using UnityEditor;

public class FindMissingScriptsInPrefabs : MonoBehaviour
{
    [MenuItem("Tools/Find Missing Scripts in All Prefabs")]
    public static void FindInAllPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Component[] components = prefab.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"Missing script in prefab '{prefab.name}' at path: {path}", prefab);
                    break;
                }
            }
        }
    }
}
