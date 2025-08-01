using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SheepObject", menuName = "Scriptable Objects/SheepObject")]
public class SheepObject : ScriptableObject
{
    public Material color;
    public int colorIndex;
    public Texture2D heatmap;
    public GameObject sheep;
    public int minSize;
    public int maxSize;
    public int priority;
}
