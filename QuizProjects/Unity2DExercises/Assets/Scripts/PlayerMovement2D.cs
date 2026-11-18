using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private float jumpSpeed = 12f;

    private Rigidbody2D body;
    private Collider2D playerCollider;
    private float horizontalInput;
    private bool jumpQueued;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        jumpQueued |= Input.GetButtonDown("Jump");

        if (Input.GetKeyDown(KeyCode.R) || transform.position.y < -9f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void FixedUpdate()
    {
        float verticalSpeed = body.linearVelocity.y;
        if (jumpQueued && IsGrounded())
        {
            verticalSpeed = jumpSpeed;
        }

        body.linearVelocity = new Vector2(horizontalInput * moveSpeed, verticalSpeed);
        jumpQueued = false;
    }

    private bool IsGrounded()
    {
        Vector2 feet = (Vector2)transform.position + Vector2.down * 0.68f;
        Collider2D[] contacts = Physics2D.OverlapCircleAll(feet, 0.12f);
        foreach (Collider2D contact in contacts)
        {
            if (contact != playerCollider && !contact.isTrigger)
            {
                return true;
            }
        }

        return false;
    }
}
