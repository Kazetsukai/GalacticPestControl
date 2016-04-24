using UnityEngine;
using System.Collections;

public class BackNForth : MonoBehaviour {
    private Vector3 _startPos;

    // Use this for initialization
    void Start () {
        _startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = _startPos + Vector3.right * Mathf.Sin(Time.realtimeSinceStartup);
	}
}
