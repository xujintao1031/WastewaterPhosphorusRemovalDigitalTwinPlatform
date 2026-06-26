using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ImageCaptureButtonGroup : MonoBehaviour
    {
        [Header("Buttons")]
        public Button[] buttons;

        [Header("Sprites")]
        public Sprite normalSprite;
        public Sprite highlightSprite;

        private int _currentIndex = -1;

        public System.Action<int> OnButtonClicked;

        private void Start()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int idx = i;
                buttons[i].onClick.AddListener(() => OnButtonClick(idx));
            }

            if (buttons.Length > 0)
                OnButtonClick(0);
        }

        void OnButtonClick(int index)
        {
            if (index == _currentIndex) return;
            _currentIndex = index;

            for (int i = 0; i < buttons.Length; i++)
            {
                var img = buttons[i].GetComponent<Image>();
                if (img != null)
                    img.sprite = (i == index) ? highlightSprite : normalSprite;
            }

            OnButtonClicked?.Invoke(index);
        }

        public int CurrentIndex => _currentIndex;
    }
}
