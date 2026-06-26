using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Forces a layout rebuild on the target RectTransform at Start.
    /// Works around Unity UGUI initialization-order issues with nested LayoutGroups + ContentSizeFitters.
    /// </summary>
    public class LayoutRefresher : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;

        private void Start()
        {
            if (_target == null)
                _target = GetComponent<RectTransform>();

            LayoutRebuilder.ForceRebuildLayoutImmediate(_target);
        }
    }
}
