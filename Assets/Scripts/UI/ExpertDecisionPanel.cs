using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ExpertDecisionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private GameObject _blocker;
        [SerializeField] private HighlightButton _triggerButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _historyButton;
        [SerializeField] private Button _returnButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _scenarioText;
        [SerializeField] private RawImage _scenarioImage;
        [SerializeField] private Texture2D _defaultTexture;
        [SerializeField] private string _defaultName;
        [SerializeField] private Texture2D[] _scenarioTextures;
        [SerializeField] private string[] _scenarioNames;

        private int _currentIndex;
        private bool _isHistoryMode;

        private void Start()
        {
            _panel.SetActive(false);
            if (_blocker != null) _blocker.SetActive(false);

            _triggerButton.OnClicked += ShowPanel;
            _closeButton.onClick.AddListener(HidePanel);
            _historyButton.onClick.AddListener(EnterHistoryMode);
            _returnButton.onClick.AddListener(ReturnToCurrentDecision);
            _prevButton.onClick.AddListener(PrevScenario);
            _nextButton.onClick.AddListener(NextScenario);

            var bottomNav = FindObjectOfType<BottomNavController>();
            if (bottomNav != null)
                bottomNav.OnTabChanged += OnTabChanged;

            SetCurrentDecisionMode();
        }

        private void OnDestroy()
        {
            if (_triggerButton != null)
                _triggerButton.OnClicked -= ShowPanel;

            var bottomNav = FindObjectOfType<BottomNavController>();
            if (bottomNav != null)
                bottomNav.OnTabChanged -= OnTabChanged;

            _closeButton.onClick.RemoveAllListeners();
            _historyButton.onClick.RemoveAllListeners();
            _returnButton.onClick.RemoveAllListeners();
            _prevButton.onClick.RemoveAllListeners();
            _nextButton.onClick.RemoveAllListeners();
        }

        private void ShowPanel()
        {
            _panel.SetActive(true);
            if (_blocker != null) _blocker.SetActive(true);
        }

        private void OnTabChanged(int index)
        {
            if (_panel.activeSelf)
                HidePanel();
        }

        private void HidePanel()
        {
            _panel.SetActive(false);
            if (_blocker != null) _blocker.SetActive(false);
        }

        private void SetCurrentDecisionMode()
        {
            _isHistoryMode = false;
            _historyButton.gameObject.SetActive(true);
            _returnButton.gameObject.SetActive(false);
            _prevButton.gameObject.SetActive(false);
            _nextButton.gameObject.SetActive(false);
            ShowDefault();
        }

        private void EnterHistoryMode()
        {
            _isHistoryMode = true;
            _historyButton.gameObject.SetActive(false);
            _returnButton.gameObject.SetActive(true);
            _prevButton.gameObject.SetActive(true);
            _nextButton.gameObject.SetActive(true);

            _currentIndex = 0;
            ShowScenario(_currentIndex);
        }

        private void ReturnToCurrentDecision()
        {
            SetCurrentDecisionMode();
            ShowDefault();
        }

        private void PrevScenario()
        {
            _currentIndex = (_currentIndex - 1 + _scenarioNames.Length) % _scenarioNames.Length;
            ShowScenario(_currentIndex);
        }

        private void NextScenario()
        {
            _currentIndex = (_currentIndex + 1) % _scenarioNames.Length;
            ShowScenario(_currentIndex);
        }

        private void ShowDefault()
        {
            _scenarioText.text = _defaultName ?? "";

            var textRT = _scenarioText.GetComponent<RectTransform>();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(textRT.parent as RectTransform);

            if (_defaultTexture != null)
            {
                _scenarioImage.texture = _defaultTexture;
                FitImageToTexture();
            }
        }

        private void ShowScenario(int index)
        {
            if (_scenarioNames == null || _scenarioTextures == null) return;
            if (index < 0 || index >= _scenarioNames.Length) return;

            _scenarioText.text = _scenarioNames[index];

            var textRT = _scenarioText.GetComponent<RectTransform>();
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(textRT.parent as RectTransform);

            if (index < _scenarioTextures.Length && _scenarioTextures[index] != null)
            {
                _scenarioImage.texture = _scenarioTextures[index];
                FitImageToTexture();
            }
        }

        private void FitImageToTexture()
        {
            var tex = _scenarioImage.texture;
            if (tex == null) return;

            var imgRT = _scenarioImage.GetComponent<RectTransform>();
            imgRT.sizeDelta = new Vector2(tex.width, tex.height);

            var scrollContentRT = imgRT.parent.GetComponent<RectTransform>();
            scrollContentRT.sizeDelta = new Vector2(0, tex.height);
        }
    }
}
