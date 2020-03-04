using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    [SerializeField]
    // private float speed = 50;
    private Camera cam;

    // Start is called before the first frame update
    void Start() {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey("d")) {
            transform.Translate(new Vector3(Camera.main.orthographicSize * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey("a")) {
            transform.Translate(new Vector3(-Camera.main.orthographicSize * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey("s")) {
            transform.Translate(new Vector3(0, -Camera.main.orthographicSize * Time.deltaTime, 0));
        }
        if (Input.GetKey("w")) {
            transform.Translate(new Vector3(0, Camera.main.orthographicSize * Time.deltaTime, 0));
        }
        if (Input.GetKey("q")) {
            Camera.main.orthographicSize /= 0.99f;
        }
        if (Input.GetKey("e")) {
            Camera.main.orthographicSize *= 0.99f;
        }
    }
}