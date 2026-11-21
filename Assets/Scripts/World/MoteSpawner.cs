using UnityEngine;

/// <summary>Spawns light motes only while the bridge is solid.</summary>
public sealed class MoteSpawner : MonoBehaviour
{
    public GameObject motePrefab;
    public ShadowBridge bridge;
    private float nextSpawn;
    private void Update()
    {
        if (motePrefab == null || bridge == null || !bridge.IsSolid || Time.time < nextSpawn) return;
        nextSpawn = Time.time + .25f;
        Vector3 p = bridge.transform.position;
        float halfWidth = bridge.GetComponent<BoxCollider2D>().size.x * .47f;
        p.x += Random.Range(-halfWidth, halfWidth);
        p.y += .18f;
        Instantiate(motePrefab, p, Quaternion.identity);
    }
}
