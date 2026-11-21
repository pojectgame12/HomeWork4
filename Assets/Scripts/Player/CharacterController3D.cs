using UnityEngine;

/// <summary>Playable 3D mini-level: CharacterController.Move, raycast and Rigidbody push.</summary>
[RequireComponent(typeof(CharacterController))]
public sealed class CharacterController3D : MonoBehaviour
{
    [SerializeField, Min(.1f)] private float moveSpeed = 4.5f;
    [SerializeField, Min(.1f)] private float jumpSpeed = 6f;
    [SerializeField, Min(0f)] private float pushImpulse = 1.1f;
    private const float Gravity = -22f;
    private CharacterController controller;
    private float verticalVelocity;

    private void Awake() { controller = GetComponent<CharacterController>(); }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 planar = new Vector3(horizontal, 0f, vertical).normalized;
        if (planar.sqrMagnitude > .01f) transform.forward = planar;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space)) verticalVelocity = jumpSpeed;
        verticalVelocity += Gravity * Time.deltaTime;
        Vector3 displacement = (planar * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime;
        controller.Move(displacement);

        // Interact with a physics crate in front of the character.
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 origin = transform.position + Vector3.up * 1f;
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, 2.4f))
                Push(hit.rigidbody, transform.forward);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y < -.3f) return;
        Vector3 direction = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        Push(hit.rigidbody, direction);
    }

    private void Push(Rigidbody target, Vector3 direction)
    {
        if (target == null || target.isKinematic || direction.sqrMagnitude < .01f) return;
        target.AddForce(direction.normalized * pushImpulse, ForceMode.Impulse);
    }
}
