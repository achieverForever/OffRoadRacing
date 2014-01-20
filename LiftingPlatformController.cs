using UnityEngine;
using System.Collections;

public class LiftingPlatformController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey("q"))
		{
			gameObject.transform.Translate(Vector3.up * Time.deltaTime * .5f, Space.World);
		}
		if(Input.GetKey("e"))
		{
			gameObject.transform.Translate(-Vector3.up * Time.deltaTime * .5f, Space.World);
		}	
	}
}
