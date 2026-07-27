using UnityEngine;

namespace UI
{
    public class WebLink : MonoBehaviour
    {
        public string url;

        public void OpenLink()
        {
            Application.OpenURL(url);
        }
    }
}
