using UnityEngine;

/// <summary>Movement only; timeline recording is owned by EchoLoopManager.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class EchoPlayer : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 6.2f;
    [SerializeField, Min(0.1f)] private float jumpSpeed = 9.0f;

    private Rigidbody2D body;
    private BoxCollider2D box;
    private SpriteRenderer sprite;
    private bool jumpQueued;

    public bool FacingLeft { get; private set; }
    public Sprite PlayerSprite => sprite != null ? sprite.sprite : null;
    public Vector2 Position => body != null ? body.position : (Vector2)transform.position;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        float axis = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) axis -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) axis += 1f;
        axis = Mathf.Clamp(axis, -1f, 1f);
        body.linearVelocity = new Vector2(axis * moveSpeed, body.linearVelocity.y);
        if (axis != 0f)
        {
            FacingLeft = axis < 0f;
            if (sprite != null) sprite.flipX = FacingLeft;
        }
        if (!jumpQueued) return;
        jumpQueued = false;
        if (!IsGrounded()) return;
        body.linearVelocity = new Vector2(body.linearVelocity.x, 0f);
        body.AddForce(Vector2.up * jumpSpeed * body.mass, ForceMode2D.Impulse);
    }

    private bool IsGrounded()
    {
        Bounds bounds = box.bounds;
        Vector2 from = new Vector2(bounds.center.x, bounds.min.y + .06f);
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(from, Vector2.down, .18f))
        {
            if (hit.collider == null || hit.collider == box || hit.collider.isTrigger) continue;
            if (hit.normal.y > .35f) return true;
        }
        return false;
    }

    public void ResetAt(Vector2 location)
    {
        jumpQueued = false;
        body.position = location;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        FacingLeft = false;
        if (sprite != null) sprite.flipX = false;
        transform.position = location;
        Physics2D.SyncTransforms();
    }

    public void FreezeForResult()
    {
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        enabled = false;
    }
}
