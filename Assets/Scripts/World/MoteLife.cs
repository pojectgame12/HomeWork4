using UnityEngine;

public sealed class MoteLife : MonoBehaviour
{
    public float lifetime = 1f;
    private void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * .6f;
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }
}
