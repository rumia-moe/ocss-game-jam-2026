using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTarget : MonoBehaviour {
    void Update() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(
            new Vector3(Mouse.current.position.x.ReadValue(),
                        Mouse.current.position.y.ReadValue(),
                        Mathf.Abs(Camera.main.transform.position.z))
        );
        mousePos.z = 0f;
        transform.position = mousePos;
    }
}