using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Simulates a camera flash / photo capture effect on the frame image.
    /// Attach to the Frame GameObject; call Capture() to trigger.
    /// </summary>
    public class PhotoCaptureEffect : MonoBehaviour
    {
        [SerializeField] private Image _flashOverlay;
        [SerializeField] private Image _photoImage;
        [SerializeField] private HighlightButton _triggerButton;
        [SerializeField] private float _flashDuration = 0.35f;
        [SerializeField] private float _pulseStrength = 0.06f;

        private Coroutine _routine;

        private void Start()
        {
            // Photo starts hidden
            if (_photoImage != null)
                _photoImage.gameObject.SetActive(false);

            if (_triggerButton != null)
                _triggerButton.OnClicked += OnTriggerClicked;
        }

        private void OnDestroy()
        {
            if (_triggerButton != null)
                _triggerButton.OnClicked -= OnTriggerClicked;
        }

        private void OnTriggerClicked()
        {
            Capture();
        }

        public void Capture()
        {
            // Already captured — skip
            if (_photoImage != null && _photoImage.gameObject.activeSelf)
                return;

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (_flashOverlay == null && _photoImage == null)
                yield break;

            // Show photo at start of flash
            if (_photoImage != null)
                _photoImage.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _flashDuration;

                // Flash overlay: bright white that fades out
                if (_flashOverlay != null)
                    _flashOverlay.color = new Color(1f, 1f, 1f, 1f - t);

                // Photo image: subtle scale pulse
                if (_photoImage != null)
                {
                    float scale = 1f + _pulseStrength * (1f - t);
                    _photoImage.transform.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            // Reset
            if (_flashOverlay != null)
                _flashOverlay.color = Color.clear;
            if (_photoImage != null)
                _photoImage.transform.localScale = Vector3.one;

            _routine = null;
        }
    }
}
