using UnityEngine;
using UnityEditor;

public class PrefabSetup : EditorWindow
{
    public GameObject brokenPrefab; // Drag your "Cracked" prefab here
    public float breakForce = 2f;   // Default break force

    [MenuItem("Tools/Prefab Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<PrefabSetup>("Prefab Setup");
    }

    private GameObject[] selectedPrefabs;

    void OnGUI()
    {
        GUILayout.Label("Setup Prefabs for Breaking", EditorStyles.boldLabel);

        brokenPrefab = (GameObject)EditorGUILayout.ObjectField("Broken Prefab Template", brokenPrefab, typeof(GameObject), false);
        breakForce = EditorGUILayout.FloatField("Break Force", breakForce);

        if (GUILayout.Button("Setup Selected Prefabs"))
        {
            SetupPrefabs();
        }
    }

    void SetupPrefabs()
    {
        selectedPrefabs = Selection.gameObjects;

        if (selectedPrefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs selected!");
            return;
        }

        foreach (GameObject prefab in selectedPrefabs)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefabRoot != null)
            {
                // Open prefab in Prefab Mode
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);

                // Add Rigidbody if missing
                if (instance.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rb = instance.AddComponent<Rigidbody>();
                    rb.useGravity = true;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }

                // Add Collider if missing
                if (instance.GetComponent<Collider>() == null)
                {
                    MeshCollider mc = instance.AddComponent<MeshCollider>();
                    mc.convex = true;
                }

                // Add Breakable script if missing
                if (instance.GetComponent<BreakableObject>() == null)
                {
                    BreakableObject bo = instance.AddComponent<BreakableObject>();
                }

                // Apply changes back to the prefab
                PrefabUtility.SaveAsPrefabAsset(instance, path);
                DestroyImmediate(instance);
            }
        }

        Debug.Log("Prefabs updated successfully!");
    }
}