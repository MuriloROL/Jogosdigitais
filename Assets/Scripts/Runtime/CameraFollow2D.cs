using UnityEngine;

namespace ProjetoJogosDigitais.Presentation
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.18f;
        [SerializeField] private Vector2 minimum = new(-12f, -8f);
        [SerializeField] private Vector2 maximum = new(12f, 8f);

        private Vector3 velocity;

        public void Configure(Transform followTarget, Vector2 min, Vector2 max)
        {
            target = followTarget;
            minimum = min;
            maximum = max;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desired = new(
                Mathf.Clamp(target.position.x, minimum.x, maximum.x),
                Mathf.Clamp(target.position.y, minimum.y, maximum.y),
                transform.position.z);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref velocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }
    }
}
