using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PACMedicinePanelController : MonoBehaviour
    {
        [SerializeField] private Button[] _tabButtons;
        [SerializeField] private Sprite[] _tableSprites;
        [SerializeField] private Image _displayImage;
        [SerializeField] private Sprite _tabSelectedSprite;
        private int _currentIndex = -1;
        private GameObject _bottomPanel;
        private RectTransform _displayRect;
        private RectTransform _contentRect;

        private void OnEnable()
        {
            if (_bottomPanel == null)
                _bottomPanel = GameObject.Find("Canvas/BottomPanel");
            if (_bottomPanel != null)
                _bottomPanel.SetActive(false);

            _currentIndex = -1;
            SelectTab(0);
        }

        private void OnDisable()
        {
            if (_bottomPanel != null)
                _bottomPanel.SetActive(true);
        }

        private void Awake()
        {
            if (_displayImage != null)
            {
                _displayRect = _displayImage.rectTransform;
                _contentRect = _displayRect.parent as RectTransform;
            }
        }

        private void Start()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int idx = i;
                _tabButtons[i].onClick.AddListener(() => SelectTab(idx));

                var cr = _tabButtons[i].GetComponent<CanvasRenderer>();
                if (cr != null)
                    cr.cullTransparentMesh = false;
            }
            SelectTab(0);
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _tabButtons.Length || index == _currentIndex) return;
            _currentIndex = index;

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                var img = _tabButtons[i].GetComponent<Image>();
                if (img == null) continue;

                if (i == index)
                {
                    img.sprite = _tabSelectedSprite;
                    img.color = Color.white;
                }
                else
                {
                    img.sprite = null;
                    img.color = Color.clear;
                }
            }

            if (_displayImage != null && index < _tableSprites.Length)
            {
                var sprite = _tableSprites[index];
                _displayImage.sprite = sprite;
                if (sprite != null && _contentRect != null)
                {
                    float containerWidth = _contentRect.rect.width;
                    float height = containerWidth * sprite.rect.height / sprite.rect.width;
                    _displayRect.sizeDelta = new Vector2(0, height);
                    _contentRect.sizeDelta = new Vector2(0, height);
                }
            }
        }
    }
}
