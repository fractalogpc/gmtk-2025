using UnityEngine;
using UnityEditor;

public class ReplaceTreesWithPrefab : MonoBehaviour
{

    [SerializeField] private GameObject treePrefab;
    [SerializeField] private string nameToReplace = "Tree";

    #if UNITY_EDITOR
        [CustomEditor(typeof(ReplaceTreesWithPrefab))]
        public class ReplaceTreesWithPrefabEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                ReplaceTreesWithPrefab script = (ReplaceTreesWithPrefab)target;
                if (GUILayout.Button("Replace Trees With Prefab"))
                {
                    ReplaceTrees(script);
                }
            }

            private void ReplaceTrees(ReplaceTreesWithPrefab script)
            {
                var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var obj in allObjects)
                {
                    if (obj.name == script.nameToReplace)
                    {
                        var newTree = (GameObject)PrefabUtility.InstantiatePrefab(script.treePrefab);
                        newTree.transform.position = obj.transform.position;
                        newTree.transform.rotation = obj.transform.rotation;
                        newTree.transform.localScale = obj.transform.localScale;
                        newTree.transform.parent = obj.transform.parent;
                        Undo.RegisterCreatedObjectUndo(newTree, "Replace Tree");
                        Undo.DestroyObjectImmediate(obj);
                    }
                }
            }
        }
    #endif
}
