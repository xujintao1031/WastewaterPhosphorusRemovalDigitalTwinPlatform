using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PhosphorusMonitorController : MonoBehaviour
    {
        [SerializeField] private HighlightButton _calcButton;
        [SerializeField] private GameObject _resultPopup;

        private Coroutine _autoHideRoutine;

        private void Start()
        {
            if (_resultPopup != null)
            {
                var popup = _resultPopup.GetComponent<CalculationResultPopup>();
                if (popup != null)
                    popup.EnsureInitialized();
            }

            if (_calcButton != null)
                _calcButton.OnClicked += ShowResult;
        }

        private void OnDestroy()
        {
            if (_calcButton != null)
                _calcButton.OnClicked -= ShowResult;
        }

        private void ShowResult()
        {
            if (_resultPopup == null) return;
            _resultPopup.SetActive(true);

            if (_autoHideRoutine != null)
                StopCoroutine(_autoHideRoutine);
            _autoHideRoutine = StartCoroutine(AutoHideRoutine());
        }

        private IEnumerator AutoHideRoutine()
        {
            yield return new WaitForSeconds(3f);
            HideResult();
        }

        public void HideResult()
        {
            if (_resultPopup != null)
                _resultPopup.SetActive(false);

            if (_autoHideRoutine != null)
            {
                StopCoroutine(_autoHideRoutine);
                _autoHideRoutine = null;
            }
        }
    }
}
