using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Action button with hover highlight via sprite swap.
    /// Hovering shows highlightSprite; click fires OnClicked, no persistent state.
    /// </summary>
    public class HighlightButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _highlightSprite;

        public System.Action OnClicked;
        public Sprite HighlightSprite { get => _highlightSprite; set => _highlightSprite = value; }

        private bool _isHovered;
        private Color _savedColor;

        private void Awake()
        {
            if (_targetImage == null)
                _targetImage = GetComponent<Image>();
            var cr = GetComponent<CanvasRenderer>();
            if (cr != null)
                cr.cullTransparentMesh = false;
        }

        private void UpdateVisual()
        {
            if (_targetImage != null)
                _targetImage.sprite = _isHovered ? _highlightSprite : _normalSprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (_targetImage != null)
            {
                _savedColor = _targetImage.color;
                _targetImage.color = Color.white;
            }
            UpdateVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (_targetImage != null)
                _targetImage.color = _savedColor;
            UpdateVisual();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }
    }
}
