using System.Collections;
using UnityEngine;

public class PanCamera : MonoBehaviour {
    public float panSpeed = 3.5f;
    private float X;
    private float Y;
    float moveSpeed = 100.0f;

    void Update() {
        if (Input.GetMouseButton(0)) {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * panSpeed, -Input.GetAxis("Mouse X") * panSpeed, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);
        }
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W)) {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S)) {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A)) {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            p_Velocity += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.Space)) {
            p_Velocity += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.LeftShift)) {
            p_Velocity += new Vector3(0, -1, 0);
        }
        transform.Translate(p_Velocity * moveSpeed * Time.deltaTime);
    }

    // public float panSpeed = 20f;
    // public float panBorderThickness = 10f;

    // void Update() {
    //     Vector3 pos = transform.position;

    //     if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness) {
    //         pos.z += panSpeed * Time.deltaTime;
    //     }

    //     transform.position = pos;
    // }
}