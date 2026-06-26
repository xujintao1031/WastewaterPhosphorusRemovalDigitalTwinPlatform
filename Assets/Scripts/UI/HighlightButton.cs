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

        private bool _isHovered;

        private void Awake()
        {
            if (_targetImage == null)
                _targetImage = GetComponent<Image>();
        }

        private void UpdateVisual()
        {
            if (_targetImage != null)
                _targetImage.sprite = _isHovered ? _highlightSprite : _normalSprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UpdateVisual();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisual();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }
    }
}
