using UnityEngine;
using System.Collections;

public class SnapChildren : MonoBehaviour {
    private readonly float SQRT_TWO = Mathf.Sqrt(2);
    
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	    foreach (Transform child in transform)
        {
            var pos = child.localPosition;
            var yFactor = 32 * SQRT_TWO;
            pos.x = (int)(pos.x * 32) / 32f;
            pos.y = (int)(pos.y * yFactor) / yFactor;
            pos.z = (int)(pos.z * yFactor) / yFactor;

            child.localPosition = pos;
        }
	}
}
