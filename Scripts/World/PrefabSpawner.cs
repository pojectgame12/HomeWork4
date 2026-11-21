using UnityEngine;

public sealed class PrefabSpawner : MonoBehaviour
{
    public GameObject prefab;
    public Vector3 spawnPosition = new Vector3(-7.1f, -1.95f, 0f);
    private void Start()
    {
        if (prefab == null) { Debug.LogError("PrefabSpawner: no prefab assigned"); return; }
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}
