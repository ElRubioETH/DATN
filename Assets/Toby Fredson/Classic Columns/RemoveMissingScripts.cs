using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Cleanup/Remove Missing Scripts in Scene")]
    public static void RemoveInCurrentScene()
    {
        int count = 0;
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>(true);

        foreach (GameObject g in go)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(g);
            if (removed > 0)
            {
                Debug.Log($"Removed {removed} missing script(s) from GameObject: {g.name}", g);
                count += removed;
            }
        }

        Debug.Log($"✅ Total missing scripts removed from scene: {count}");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Cleanup/Remove Missing Scripts in All Prefabs")]
    public static void RemoveInAllPrefabs()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int totalRemoved = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = PrefabUtility.LoadPrefabContents(path);
            int removed = 0;

            foreach (Transform t in instance.GetComponentsInChildren<Transform>(true))
            {
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }

            if (removed > 0)
            {
                Debug.Log($"Removed {removed} missing script(s) from prefab: {prefab.name}", prefab);
                totalRemoved += removed;
                PrefabUtility.SaveAsPrefabAsset(instance, path);
            }

            PrefabUtility.UnloadPrefabContents(instance);
        }

        Debug.Log($"✅ Total missing scripts removed from prefabs: {totalRemoved}");
    }
}
