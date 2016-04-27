using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
	private float defaultSize;

	private Camera localCamera;
    

	JankMode pixelMode = JankMode.Dejank;

	// Use this for initialization
	void Start()
	{
		localCamera = GetComponent<Camera>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			if (pixelMode == JankMode.Dejank)
				pixelMode = JankMode.FullJank;
			else if (pixelMode == JankMode.FullJank)
				pixelMode = JankMode.Dejank;
		}
        

		localCamera.orthographicSize = 5;

        var size = new Vector2(localCamera.pixelWidth, localCamera.pixelHeight);
        var zoomFactor = 2;

		var pixelXOffset = size.x % 2 == 0 ? 0 : 0.5f;
		var pixelYOffset = size.y % 2 == 0 ? 0 : 0.5f;
		var snapSize = ((int)size.y / (32f * zoomFactor)) / 2f;
		var snapPosition = new Vector3((int)(transform.position.x * 32), (int)((transform.position.y) * 32), -20 * 32) / 32f;

		if (pixelMode == JankMode.Dejank)
		{
			localCamera.orthographicSize = snapSize;
			transform.localPosition = snapPosition;
		}

		Debug.Log(pixelMode + " - " + size + "   - " + localCamera.orthographicSize + " - " + snapSize + " - " + transform.localPosition + " - " + snapPosition);
	}

	enum JankMode
	{
		FullJank,
		Dejank
	}
}
