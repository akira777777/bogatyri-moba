using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BogatyriMoba.UI.Mobile
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 60f;
        [SerializeField] private float deadzone = 0.2f;

        public Vector2 Direction { get; private set; }

        public System.Action<Vector2> OnDirectionChanged;

        private Canvas _canvas;
        private Camera _uiCamera;

        private void Awake()
        {
            if (background == null)
                background = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _uiCamera = _canvas.worldCamera;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, _uiCamera, out var localPoint);

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
            if (handle != null)
                handle.anchoredPosition = clamped;

            Direction = handleRange > 0f ? clamped / handleRange : Vector2.zero;
            if (Direction.magnitude < deadzone)
                Direction = Vector2.zero;

            OnDirectionChanged?.Invoke(Direction);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Direction = Vector2.zero;
            if (handle != null)
                handle.anchoredPosition = Vector2.zero;
            OnDirectionChanged?.Invoke(Direction);
        }
    }
}
