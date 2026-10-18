using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Utils {
    public class MouseCursor : MonoBehaviour {
        private void Start() {
            Cursor.visible = false;
        }

        private void Update() {
            if (Camera.main != null && Mouse.current != null) {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);
                transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);
            }
        }
    }
}