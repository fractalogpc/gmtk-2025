using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class TreeGeneration : MonoBehaviour
{

    [SerializeField] private Transform _treeParent;

    [SerializeField] private GameObject[] _treePrefabs;

    [SerializeField] private Vector3 _areaSize;
    [SerializeField] private Vector3 _areaCenter;
    [SerializeField] private float _treeScaleMin = 0.5f;
    [SerializeField] private float _treeScaleMax = 1.5f;
    [SerializeField] private int _treeGridRes = 100;
    [SerializeField] private LayerMask _terrainLayer;
    [SerializeField] private float _noiseScale = 0.1f;
    [SerializeField] private float _noiseThreshold = 0.5f;
    [SerializeField] private int _noiseOctaves = 3;
    [SerializeField] private float _noiseRoughness = 0.5f;
    [SerializeField] private int _noiseSeed = 0;

#if UNITY_EDITOR
    [CustomEditor(typeof(TreeGeneration))]
    public class TreeGenerationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            TreeGeneration treeGen = (TreeGeneration)target;
            if (GUILayout.Button("Generate Trees"))
            {
                treeGen.GenerateTrees();
            }
        }
    }
#endif

    private void GenerateTrees()
    {
        if (_treeParent == null || _treePrefabs.Length == 0)
        {
            Debug.LogWarning("Tree parent or prefabs not set.");
            return;
        }

        ClearTrees();

        // Create grid of tree positions, jitter them, spawn trees based on perlin noise
        Vector3 start = _areaCenter - _areaSize / 2;
        Vector3 end = _areaCenter + _areaSize / 2;
        float stepX = _areaSize.x / _treeGridRes;
        float stepZ = _areaSize.z / _treeGridRes;
        
        float hash = (float)math.hash(new int2(_noiseSeed, _noiseSeed)) / int.MaxValue * 1000f; // Normalize hash to a range
        float2 seedOffset = new float2(hash, hash);

        for (int x = 0; x < _treeGridRes; x++)
        {
            for (int z = 0; z < _treeGridRes; z++)
            {
                Vector3 position = new Vector3(
                    start.x + x * stepX + Random.Range(-stepX / 2, stepX / 2),
                    _areaCenter.y,
                    start.z + z * stepZ + Random.Range(-stepZ / 2, stepZ / 2)
                );

                float noiseValue = 0f;
                // Simplex noise
                for (int octave = 0; octave < _noiseOctaves; octave++)
                {
                    float frequency = Mathf.Pow(2, octave);
                    float amplitude = Mathf.Pow(_noiseRoughness, octave);
                    noiseValue += amplitude * noise.snoise(new float2((position.x + seedOffset.x) * _noiseScale * frequency, (position.z + seedOffset.y) * _noiseScale * frequency));
                }

                // Skip tree placement based on noise value
                if (noiseValue < _noiseThreshold) continue; // Adjust threshold as needed

                // Find y position based on terrain height
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 500, Vector3.down, out hit, 2000, _terrainLayer))
                {
                    position.y = hit.point.y;
                }
                else
                {
                    position.y = 0;
                }

                GameObject treePrefab = _treePrefabs[Random.Range(0, _treePrefabs.Length)];
                GameObject treeInstance = Instantiate(treePrefab, position, Quaternion.identity, _treeParent);

                // Randomly scale the tree
                float treeScale = Random.Range(_treeScaleMin, _treeScaleMax);
                treeInstance.transform.localScale = new Vector3(treeScale, treeScale, treeScale);
            }
        }

        // for (int i = 0; i < _treeCount; i++)
        // {
        //     Vector3 position = new Vector3(
        //         Random.Range(_areaCenter.x - _areaSize.x / 2, _areaCenter.x + _areaSize.x / 2),
        //         _areaCenter.y,
        //         Random.Range(_areaCenter.z - _areaSize.z / 2, _areaCenter.z + _areaSize.z / 2)
        //     );

        //     // Find y position based on terrain height
        //     RaycastHit hit;
        //     if (Physics.Raycast(position + Vector3.up * 500, Vector3.down, out hit, 2000, _terrainLayer))
        //     {
        //         position.y = hit.point.y;
        //     }
        //     else
        //     {
        //         position.y = 0;
        //     }

        //     GameObject treePrefab = _treePrefabs[Random.Range(0, _treePrefabs.Length)];
        //     GameObject treeInstance = Instantiate(treePrefab, position, Quaternion.identity, _treeParent);

        //     // Randomly scale the tree
        //     float treeScale = Random.Range(_treeScaleMin, _treeScaleMax);
        //     treeInstance.transform.localScale = new Vector3(treeScale, treeScale, treeScale);
        // }
    }

    private void ClearTrees()
    {
        if (_treeParent == null) return;

        for (int i = _treeParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(_treeParent.GetChild(i).gameObject);
        }
    }

}
