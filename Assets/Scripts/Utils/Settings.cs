using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Utils {
    public class Settings : MonoBehaviour {
        public Toggle crtToggle;
        [SerializeField] private GameObject confirmWindow;
        [SerializeField] private Renderer2DData renderer2DData;
        [SerializeField] private Volume volume;

        private CRTFilter.CRTRendererFeature crtFilter;

        private void Start() {
            foreach (var feature in renderer2DData.rendererFeatures) {
                if (feature is CRTFilter.CRTRendererFeature filter) {
                    crtFilter = filter;
                    Debug.Log("Cached CRT Filter feature!");
                    break;
                }
            }

            // Set to some defaults instead of my last unity setting
            CRT();
            ScreenColor(4);
        }

        public void OnEnable() {
            Time.timeScale = 0;
        }

        public void CloseWindow() {
            gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        public void ConfirmWindow() {
            confirmWindow.SetActive(true);
            // Application.Quit();
        }

        public void ToTitle() {
            SceneManager.LoadScene("Title");
        }

        private void OnDisable() {
            if (gameObject != null)
                Time.timeScale = 1;
        }

        private void ScreenColor(int color) {
            switch (color) {
                case 0: // yellow
                    crtFilter.red = 2f;
                    crtFilter.green = 2f;
                    crtFilter.blue = 0f;
                break;

                case 1: // green
                    crtFilter.red = 0.62f;
                    crtFilter.green = 2f;
                    crtFilter.blue = 0f;
                break;

                case 2: // blue
                    crtFilter.red = 0f;
                    crtFilter.green = 0.89f;
                    crtFilter.blue = 2f;
                break;

                case 3: // white
                    crtFilter.red = 1f;
                    crtFilter.green = 1f;
                    crtFilter.blue = 1f;
                break;

                case 4: // orange
                    crtFilter.red = 1.2f;
                    crtFilter.green = 0.5f;
                    crtFilter.blue = 0f;
                break;
            }
        }

        private void CRT() {
            if (crtToggle.isOn) {
                // Strong
                crtFilter.screenBend = 0.51f;
                crtFilter.screenOverscan = 0.5f;
                crtFilter.vignetteSize = 5.7f;
                crtFilter.vignetteSmooth = 2.8f;
                crtFilter.vignetteRound = 70;
                crtFilter.blur = 0.8f;
                crtFilter.scanlinesStrength = 2.8f;
                crtFilter.apertureStrength = 1f;
                crtFilter.shadowlines = 4.2f;
                crtFilter.shadowlinesSpeed = 0.5f;
                crtFilter.shadowlinesAlpha = 0.005f;
                crtFilter.glitchFrequency = 0.026f;
                crtFilter.glitchBands = 26;
                crtFilter.glitchPosition = 0.132f;
                crtFilter.glitchPositionFlicker = 0.211f;
                crtFilter.glitchMovementSpeed = 1.11f;
                crtFilter.glitchStrength = 0.232f;
                crtFilter.glitchNoise = 0f;
                crtFilter.glitchNoiseSpeed = 0f;
                
                if (volume.profile.TryGet(out Bloom bloom))
                    bloom.intensity.value = 1.5f;
                // Subtle
                // crtFilter.screenBend = 0.51f;
                // crtFilter.vignetteSize = 5.7f;
                // crtFilter.vignetteSmooth = 2;
                // crtFilter.vignetteRound = 63;
                // crtFilter.blur = 0.5f;
                // crtFilter.scanlinesStrength = 1f;
                // crtFilter.apertureStrength = 0.1f;
            } else {
                crtFilter.screenBend = 0f;
                crtFilter.screenOverscan = 0f;
                crtFilter.vignetteSize = 0;
                crtFilter.vignetteSmooth = 0;
                crtFilter.vignetteRound = 0;
                crtFilter.blur = 0;
                crtFilter.scanlinesStrength = 0;
                crtFilter.apertureStrength = 0;
                crtFilter.shadowlines = 0;
                crtFilter.shadowlinesSpeed = 0;
                crtFilter.shadowlinesAlpha = 0;
                crtFilter.glitchFrequency = 0;
                crtFilter.glitchBands = 0;
                crtFilter.glitchPosition = 0;
                crtFilter.glitchPositionFlicker = 0;
                crtFilter.glitchMovementSpeed = 0;
                crtFilter.glitchStrength = 0;
                crtFilter.glitchNoise = 0;
                crtFilter.glitchNoiseSpeed = 0;
                
                if (volume.profile.TryGet(out Bloom bloom))
                    bloom.intensity.value = 0f;
            }
        }
    }
}
