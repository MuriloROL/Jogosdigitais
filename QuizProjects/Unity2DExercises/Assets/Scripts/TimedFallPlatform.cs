using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class TimedFallPlatform : MonoBehaviour
{
    public enum PlatformPhase
    {
        Waiting,
        Moving,
        Falling
    }

    public const float MovementDuration = 5f;

    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float fallGravityScale = 3f;

    public PlatformPhase Phase { get; private set; }
    public float SecondsRemaining { get; private set; }

    private Rigidbody2D body;
    private double movementStartTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        Phase = PlatformPhase.Waiting;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Phase != PlatformPhase.Waiting || !collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Phase = PlatformPhase.Moving;
        SecondsRemaining = MovementDuration;
        movementStartTime = Time.fixedTimeAsDouble;
        body.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        body.linearVelocity = new Vector2(moveSpeed, 0f);
    }

    private void FixedUpdate()
    {
        if (Phase != PlatformPhase.Moving)
        {
            return;
        }

        double elapsed = Time.fixedTimeAsDouble - movementStartTime;
        SecondsRemaining = Mathf.Max(0f, MovementDuration - (float)elapsed);
        if (elapsed + 0.000001d >= MovementDuration)
        {
            SecondsRemaining = 0f;
            body.linearVelocity = Vector2.zero;
            body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            body.gravityScale = fallGravityScale;
            Phase = PlatformPhase.Falling;
            return;
        }

        body.linearVelocity = new Vector2(moveSpeed, 0f);
    }
}
