using UnityEngine;

namespace UI
{
    public class ResolutionLock : MonoBehaviour
    {
        private void Start()
        {
            Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
        }
    }
}
