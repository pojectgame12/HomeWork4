using UnityEngine;

/// <summary>Rigidbody2D motion; impulse jumping; ground raycasts; Collision2D landing.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PlayerController : MonoBehaviour
{
    private const float DeathBelowWorldY = -8f;
    private const float GroundProbeVerticalInset = 0.06f;
    private const float GroundProbeHorizontalSpreadFactor = 0.63f;
    private const float GroundProbeLength = 0.20f;
    private const float MinimumWalkableNormalY = 0.35f;

    // Preserve the original serialized field names and prefab settings.
    public float moveSpeed = 5.3f;
    public float jumpSpeed = 9.8f;

    private Rigidbody2D body;
    private BoxCollider2D hitbox;
    private SpriteRenderer sprite;
    private bool jumpQueued;
    private bool airborneAfterJump;
    private bool deathSent;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        LevelController level = FindFirstObjectByType<LevelController>();
        if (level != null && level.Settings != null)
        {
            moveSpeed = level.Settings.moveSpeed;
            jumpSpeed = level.Settings.jumpSpeed;
        }
    }

    private void Update()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);
        jumpPressed |= MobileInputState.ConsumeJump();
        if (jumpPressed && IsGrounded()) jumpQueued = true;

        if (!deathSent && transform.position.y < DeathBelowWorldY)
        {
            deathSent = true;
            GameEvents.RaisePlayerDied();
        }
    }

    private void FixedUpdate()
    {
        float axis = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || MobileInputState.left) axis -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || MobileInputState.right) axis += 1f;
        axis = Mathf.Clamp(axis, -1f, 1f);
        body.linearVelocity = new Vector2(axis * moveSpeed, body.linearVelocity.y);
        if (axis != 0f && sprite != null) sprite.flipX = axis < 0f;

        if (!jumpQueued) return;
        jumpQueued = false;
        if (!IsGrounded()) return;

        // Impulse = mass * desired change in vertical speed.
        body.linearVelocity = new Vector2(body.linearVelocity.x, 0f);
        body.AddForce(Vector2.up * (jumpSpeed * body.mass), ForceMode2D.Impulse);
        airborneAfterJump = true;
    }

    private bool IsGrounded()
    {
        Bounds b = hitbox.bounds;
        float y = b.min.y + GroundProbeVerticalInset;
        float halfWidth = b.extents.x * GroundProbeHorizontalSpreadFactor;
        for (int i = -1; i <= 1; i++)
        {
            Vector2 origin = new Vector2(b.center.x + halfWidth * i, y);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, GroundProbeLength);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider == hitbox || hit.collider.isTrigger) continue;
                if (hit.normal.y > MinimumWalkableNormalY) return true;
            }
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!airborneAfterJump) return;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < MinimumWalkableNormalY) continue;
            airborneAfterJump = false;
            GameEvents.RaiseStatusMessage("Landed safely (Collision2D)");
            break;
        }
    }
}
