using System.Linq;
using UnityEngine;

public class SheepMapManager : MonoBehaviour
{

    [SerializeField] private Material[] mapMaterials;
    [SerializeField] private SheepSpawner sheepSpawner;

    [SerializeField] private float timePerUpdate = 1f;
    [SerializeField] private Vector2 mapXExtents;
    [SerializeField] private Vector2 mapZExtents;
    private float timeSinceLastUpdate = 0f;

    private void Start()
    {
        // Reset
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < mapMaterials.Length; j++)
            {
                if (mapMaterials[j] != null)
                {
                    mapMaterials[j].SetVector("_Sheep" + i, new Vector4(10000, 10000, 0, 0));
                }
            }
        }

        UpdateSheepMap();
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= timePerUpdate)
        {
            UpdateSheepMap();
            timeSinceLastUpdate = 0f;
        }
    }

    private void UpdateSheepMap()
    {
        Vector2[] rareSheepPositions = sheepSpawner.GetRareSheepPositions();
        for (int i = 0; i < 3; i++)
        {
            Vector2 position = Vector2.one * 10000; // Default position if no rare sheep is found

            if (i < rareSheepPositions.Length)
            {
                position = rareSheepPositions[i];
            }

            // Update the map materials based on the rare sheep positions
            float x = (position.x - mapXExtents.x) / (mapXExtents.y - mapXExtents.x);
            float z = (position.y - mapZExtents.y) / (mapZExtents.x - mapZExtents.y);
            // Debug.Log($"Updating map for sheep {i} at position: {position.x}, {position.y}");
            for (int j = 0; j < mapMaterials.Length; j++)
            {
                if (mapMaterials[j] != null)
                {
                    mapMaterials[j].SetVector("_Sheep" + i, new Vector4(x, z, 0, 0));
                }
            }
        }
    }

}
