using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Utils {
    public class ButtonScaler : MonoBehaviour {
        [SerializeField] private Vector3 scaleUpSize = new Vector3(1.2f, 1.2f, 1f);
        [SerializeField] private float scaleDuration = 0.1f;
        [Tooltip("Game Object to scale")]
        [SerializeField] private GameObject scaleObject;

        [Header("Shake")]
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeMagnitude = 5f;
        [SerializeField] private float shakeRotationMagnitude = 15f;

        [Header("Sound Hover")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        [Header("Tooltip")]
        [SerializeField] private string info;
        [SerializeField] private TextMeshProUGUI text;

        private Vector3 originalSize;
        private Vector3 originalPos;
        private Quaternion originalRot;
        private bool cachedOriginal;
        private AudioSource audioSource;
        private Button button;

        private Coroutine scaleRoutine;
        private Coroutine shakeRoutine;

        private void Start() {
            originalSize = scaleObject.transform.localScale;
            audioSource = GetComponent<AudioSource>();
            button = GetComponent<Button>();
        }

        public void OnMouseEnter() {
            if (!button.interactable) return;
            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleButton(scaleUpSize));
            
            if (hoverSound != null) {
                audioSource.PlayOneShot(hoverSound);
            }
        }

        public void OnMouseExit() {
            if (scaleRoutine != null) StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleButton(originalSize));
        }

        public void ShowInfo() {
            text.text = info;
        }

        public void HideInfo() {
            text.text = "";
        }
        
        public void ClickSound() {
            // Debug.Log($"ClickSound on {gameObject.name}, clip = {clickSound}", this);
            // if (clickSound != null)
            //     audioSource.PlayOneShot(clickSound);
        }
        
        private IEnumerator ScaleButton(Vector3 targetScale) {
            Vector3 currentScale = scaleObject.transform.localScale;
            float timer = 0;

            while (timer < scaleDuration) {
                scaleObject.transform.localScale = Vector3.Lerp(currentScale, targetScale, timer / scaleDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            scaleObject.transform.localScale = targetScale;
        }

        public void Shake() {
            if (!cachedOriginal) {
                originalPos = scaleObject.transform.localPosition;
                originalRot = scaleObject.transform.localRotation;
                cachedOriginal = true;
            }

            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            scaleObject.transform.localPosition = originalPos;
            scaleObject.transform.localRotation = originalRot;

            shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine() {
            float timer = 0f;

            while (timer < shakeDuration) {
                float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
                float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
                scaleObject.transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

                float offsetAngle = Random.Range(-1f, 1f) * shakeRotationMagnitude;
                scaleObject.transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, offsetAngle);

                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            scaleObject.transform.localPosition = originalPos;
            scaleObject.transform.localRotation = originalRot;
        }
    }
}
