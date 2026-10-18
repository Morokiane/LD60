using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace Utils {
    public class Title : MonoBehaviour {
        [SerializeField] private GameObject settingsWindow;
        [SerializeField] private Renderer2DData renderer2DData; 

        private CRTFilter.CRTRendererFeature crtFilter;

        private void Start() {
            foreach (var feature in renderer2DData.rendererFeatures) {
                if (feature is CRTFilter.CRTRendererFeature filter) {
                    crtFilter = filter;
                    Debug.Log("Cached CRT Filter feature!");
                    break;
                }
            }

            crtFilter.red = 1.2f;
            crtFilter.green = 0.5f;
            crtFilter.blue = 0f;
        }
        
        public void StartGame() {
            SceneManager.LoadScene("Main");
        }

        public void ExitGame() {
            Application.Quit();
        }

        public void Settings() {
            settingsWindow.SetActive(true);    
        }
    }
}
