using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AutoDosingController : MonoBehaviour
    {
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private Image _progressBar;
        [SerializeField] private Text _statusText;
        [SerializeField] private float _duration = 5f;

        public void StartDosing()
        {
            if (_popupPanel == null || _progressBar == null) return;
            _popupPanel.SetActive(true);
            _progressBar.fillAmount = 0f;
            if (_statusText != null)
                _statusText.text = "正在制药中...";
            StartCoroutine(RunProgress());
        }

        private IEnumerator RunProgress()
        {
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                _progressBar.fillAmount = Mathf.Clamp01(elapsed / _duration);
                yield return null;
            }
            _progressBar.fillAmount = 1f;
            if (_statusText != null)
                _statusText.text = "投药完成";
            yield return new WaitForSeconds(1f);
            _popupPanel.SetActive(false);
        }
    }
}
