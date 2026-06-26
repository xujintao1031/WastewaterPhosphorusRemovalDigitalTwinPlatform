using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Bottom navigation bar controller.
    /// Manages tab buttons (sprite swap highlight) and content panel switching.
    /// Assign 4 buttons and 4 content panels in the Inspector.
    /// </summary>
    public class BottomNavController : MonoBehaviour
    {
        [Header("Tab Buttons")]
        public Button[] tabButtons;

        [Header("Content Panels (one per tab)")]
        public GameObject[] contentPanels;

        [Header("Sprites")]
        public Sprite normalSprite;
        public Sprite selectedSprite;

        private int _currentIndex = -1;

        /// <summary>Fires when a tab is switched. Arg: tab index (0-3).</summary>
        public System.Action<int> OnTabChanged;

        private void Start()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int idx = i;
                tabButtons[i].onClick.AddListener(() => SwitchTab(idx));
            }

            SwitchTab(0);
        }

        public void SwitchTab(int index)
        {
            if (index < 0 || index >= tabButtons.Length || index == _currentIndex) return;

            _currentIndex = index;

            // Update button sprites
            for (int i = 0; i < tabButtons.Length; i++)
            {
                var img = tabButtons[i].GetComponent<Image>();
                if (img != null)
                    img.sprite = (i == index) ? selectedSprite : normalSprite;
            }

            // Show / hide content panels
            for (int i = 0; i < contentPanels.Length; i++)
            {
                if (contentPanels[i] != null)
                    contentPanels[i].SetActive(i == index);
            }

            OnTabChanged?.Invoke(index);
        }

        public int CurrentIndex => _currentIndex;
    }
}
