using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SheepObject", menuName = "Scriptable Objects/SheepObject")]
public class SheepObject : ScriptableObject
{
    public Texture2D heatmap;
    public GameObject sheep;
    public int priority;
}
