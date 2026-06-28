using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Controls red-pixel glow flashing on a target UI Image that uses UIRedGlowShader.
    /// Initially no glow; on trigger starts flashing for a configurable duration.
    /// </summary>
    public class RedPixelFlashEffect : MonoBehaviour
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private HighlightButton _processButton;
        [SerializeField] private float _flashDuration = 5f;
        [SerializeField] private float _glowIntensity = 35f;

        private Material _materialInstance;
        private Coroutine _routine;
        private static readonly int GlowIntensityId = Shader.PropertyToID("_GlowIntensity");

        private void Start()
        {
            if (_targetImage == null)
            {
                enabled = false;
                return;
            }

            _materialInstance = _targetImage.material;
            _materialInstance.SetFloat(GlowIntensityId, 0f);

            if (_processButton != null)
                _processButton.OnClicked += OnProcessClicked;
        }

        private void OnDestroy()
        {
            if (_processButton != null)
                _processButton.OnClicked -= OnProcessClicked;
            if (_materialInstance != null)
                DestroyImmediate(_materialInstance);
        }

        private void OnProcessClicked()
        {
            if (!gameObject.activeInHierarchy)
                return;
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _materialInstance.SetFloat(GlowIntensityId, _glowIntensity);
            yield return new WaitForSeconds(_flashDuration);
            _materialInstance.SetFloat(GlowIntensityId, 0f);
            _routine = null;
        }
    }
}
