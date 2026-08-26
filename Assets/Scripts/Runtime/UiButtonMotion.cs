using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ProjetoJogosDigitais.Presentation
{
    [RequireComponent(typeof(Button))]
    public sealed class UiButtonMotion : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        private Vector3 baseScale;

        private void Awake() => baseScale = transform.localScale;

        private void OnDisable() => transform.localScale = baseScale == Vector3.zero ? Vector3.one : baseScale;

        public void OnPointerEnter(PointerEventData eventData) => SetScale(1.025f);

        public void OnPointerExit(PointerEventData eventData) => SetScale(1f);

        public void OnPointerDown(PointerEventData eventData) => SetScale(0.975f);

        public void OnPointerUp(PointerEventData eventData) => SetScale(1.025f);

        private void SetScale(float amount)
        {
            if (baseScale == Vector3.zero)
            {
                baseScale = Vector3.one;
            }

            transform.localScale = GamePreferences.ReduceMotion ? baseScale : baseScale * amount;
        }
    }
}
