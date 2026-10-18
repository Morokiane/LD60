using UnityEngine;

namespace Utils {
    public class CloseWindow : MonoBehaviour {
        public void Close() {
            transform.parent.gameObject.SetActive(false);
        }

        public void CloseUnpause() {
            transform.parent.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
