using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TopPanelController : MonoBehaviour
    {
        [SerializeField] private Text _timeText;
        [SerializeField] private Button exitBtn;

        private void Start()
        {
            if (_timeText != null)
                InvokeRepeating(nameof(UpdateTime), 0f, 1f);
            if(exitBtn != null)
                exitBtn.onClick.AddListener(QuitApplication);
        }

        private void UpdateTime()
        {
            _timeText.text = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
