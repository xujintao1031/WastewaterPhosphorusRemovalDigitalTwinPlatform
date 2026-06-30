using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls the calculation result popup: click calculate → show popup with spinning icon,
    /// after delay hide the spinner and show the result text.
    /// </summary>
    public class CalculationResultPopup : MonoBehaviour
    {
        [SerializeField] private GameObject _popup;
        [SerializeField] private GameObject _rotateIcon;
        [SerializeField] private Text _resultText;
        [SerializeField] private HighlightButton _calculateButton;
        [SerializeField] private float _delaySeconds = 2f;
        [SerializeField] private float _rotateSpeed = 360f;

        private Coroutine _routine;
        private bool _isSpinning;
        private bool _initialized;

        private void Start()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            _popup.SetActive(false);
            if (_resultText != null)
                _resultText.gameObject.SetActive(false);

            if (_calculateButton != null)
                _calculateButton.OnClicked += OnCalculateClicked;

            var bottomNav = FindObjectOfType<BottomNavController>();
            if (bottomNav != null)
                bottomNav.OnTabChanged += OnTabChanged;
        }

        private void OnDestroy()
        {
            if (_calculateButton != null)
                _calculateButton.OnClicked -= OnCalculateClicked;

            var bottomNav = FindObjectOfType<BottomNavController>();
            if (bottomNav != null)
                bottomNav.OnTabChanged -= OnTabChanged;
        }

        private void Update()
        {
            if (_isSpinning && _rotateIcon != null)
                _rotateIcon.transform.Rotate(0f, 0f, -_rotateSpeed * Time.deltaTime);
        }

        private void OnCalculateClicked()
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _popup.SetActive(true);

            if (_rotateIcon != null)
                _rotateIcon.SetActive(true);

            if (_resultText != null)
                _resultText.gameObject.SetActive(false);

            _isSpinning = true;
            _routine = StartCoroutine(ShowResultRoutine());
        }

        private IEnumerator ShowResultRoutine()
        {
            yield return new WaitForSeconds(_delaySeconds);

            _isSpinning = false;

            if (_rotateIcon != null)
                _rotateIcon.SetActive(false);

            if (_resultText != null)
                _resultText.gameObject.SetActive(true);

            _routine = null;
        }

        private void OnTabChanged(int index)
        {
            if (_popup.activeSelf)
                HidePopup();
        }

        private void HidePopup()
        {
            _isSpinning = false;

            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            _popup.SetActive(false);
        }
    }
}
