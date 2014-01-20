using UnityEngine;
using System.Collections;

public class InnerViewCameraControl : MonoBehaviour {

	private Vector3 ea = Vector3.zero;
	private float deltaX, deltaY;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		GetInput();

		//ea = transform.localEulerAngles;
		ea.x -= deltaY * 0.6f;
		ea.y += deltaX;

		ea.x = Mathf.Clamp(ea.x, -10.0f, 10.0f);
		ea.y = Mathf.Clamp(ea.y, -160.0f, 160.0f);
		transform.localRotation = Quaternion.Euler(ea);
	}

	void GetInput()
	{
		deltaX = Input.GetAxis("Mouse X");
		deltaY = Input.GetAxis("Mouse Y");
	}

}
