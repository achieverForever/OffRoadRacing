using UnityEngine;
using System.Collections;

public class SetupCameras : MonoBehaviour {

	public Transform target;
	public bool isNewTarget;
	
	private Transform[] cameras;
	private int currentEnabled = 0;

	// Use this for initialization
	void Start () {
		cameras = new Transform[3];
		int i, n;
		n = transform.childCount;

		for(i=0; i < n; i++)
		{
			cameras[i] = transform.GetChild(i);
			switch(cameras[i].name)
			{
				case "Camera_BackView":
					cameras[i].position = target.position;
					cameras[i].GetComponent<SmoothFollow>().target = target;
					cameras[i].GetChild(0).renderer.enabled = true;
					break;
				case "Camera_FreeOrbitView":
					cameras[i].position = target.position;
					cameras[i].GetComponent<MouseOrbit>().target = target;
					break;
				case "Camera_InnerView":
					cameras[i].parent = target;
					break;
				default:
					break;
			}
		}		

		cameras[0].gameObject.SetActive(true);
		for(i=1; i<cameras.Length; i++)
		{
			cameras[i].gameObject.SetActive(false);
		}

	}
	
	// Update is called once per frame
	void Update () {
		if(isNewTarget)
		{
			isNewTarget = false;
			for(int i=0; i < cameras.Length; i++)
			{
				switch(cameras[i].name)
				{
					case "Camera_BackView":
						cameras[i].GetComponent<SmoothFollow>().target = target;
						cameras[i].GetChild(0).renderer.enabled = false;
						break;
					case "Camera_FreeOrbitView":
						cameras[i].GetComponent<MouseOrbit>().target = target;
						break;
					case "Camera_InnerView":
						cameras[i].parent = target;
						cameras[i].localPosition = new Vector3(-0.4f, 0.68f, 0.11f);
						break;
					default:
						Debug.LogError("Errors occured when setting up cameras");
						break;
				}
			}		
		}

		// Switch camera
		if(Input.GetKeyUp("c"))
		{
			cameras[currentEnabled].gameObject.SetActive(false);
			currentEnabled = (currentEnabled+1) % cameras.Length;
			cameras[currentEnabled].gameObject.SetActive(true);
		}
	}
}
