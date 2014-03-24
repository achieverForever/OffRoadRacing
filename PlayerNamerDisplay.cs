using UnityEngine;
using System.Collections;

public class PlayerNamerDisplay : MonoBehaviour {

	private GUIText playerName = null;

	// Use this for initialization
	void Start () {
		playerName = guiText;
		if(!playerName)
		{
			Debug.Log("GUIText Not Found");
		}
		playerName.text = "Hello";

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{
		Vector3 screenPos = Camera.allCameras[0].WorldToScreenPoint(transform.root.position);
		playerName.pixelOffset = new Vector2(screenPos.x - Camera.allCameras[0].pixelWidth*.5f, 
											 screenPos.y - Camera.allCameras[0].pixelHeight*.5f);
		Debug.Log(playerName.pixelOffset);
	}
}
