using UnityEngine;
using System.Collections;

public class MainCameraPixelSnap : MonoBehaviour {
    private Camera _camera;

    // Use this for initialization
    void Start () {
        _camera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        var noms = _camera.pixelHeight / 320;
        var size = _camera.pixelHeight / 320f;
        _camera.orthographicSize = (size * 5) / noms;
    }
}
