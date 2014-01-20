using UnityEngine;
using System.Collections;

public class PauseMgr : MonoBehaviour {

	private bool isPaused;
	private float btnWidth = 300.0f;
	private float btnHeight = 50.0f;
	private float spacing = 10.0f;

	// Use this for initialization
	void Start () {
		// DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("escape"))
		{
			isPaused = true;
		}

		if(isPaused)
			Time.timeScale = 0.0f;
		else
			Time.timeScale = 1.0f;
	}

	void OnGUI()
	{
		if(isPaused)
		{	// Display the pause menu.
			if(GUI.Button(new Rect(Screen.width*.5f - btnWidth*.5f, Screen.height*.2f,
				btnWidth, btnHeight), "Contintue"))
			{
				isPaused = false;
			}

			string text = "Disconnect";
			if(Network.isServer)
			{	
				text = "Shutdown the Server!";
			}
			if(GUI.Button(new Rect(Screen.width*.5f - btnWidth*.5f, Screen.height*.2f+(btnHeight+spacing),
				btnWidth, btnHeight), text))
			{
				isPaused = false;
				Network.Disconnect(1);
				StartCoroutine(ReLoadInSeconds(0.1f, 0));
			}
		}
	}

	IEnumerator ReLoadInSeconds(float time, int level)
	{
		yield return new WaitForSeconds(time);
		Application.LoadLevel(level);
	}
}
