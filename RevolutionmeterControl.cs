using UnityEngine;
using System.Collections;

public class RevolutionmeterControl : MonoBehaviour {

	public GameObject vehicle;
	public Texture revmeter_dial;
	public Texture revmeter_needle;
	public float minRevAngle, maxRevAngle;
	public float maxRev;
	public GUISkin mySkin;

	private Vector2 pivot = Vector2.zero;
	private Vector2 upperLeft;
	private float currentAngle, desiredAngle;
	private PlayerCar playerCar;
	private float timer = 0.0f;
	private string gearText;

	// Use this for initialization
	void Start () {
		if(vehicle != null)
		{
			playerCar = vehicle.GetComponent<PlayerCar>();			
		}
		upperLeft = new Vector2(Screen.width-revmeter_dial.width*.6f, Screen.height-revmeter_dial.height*0.6f);
		pivot = new Vector2(upperLeft.x + 0.5f*revmeter_dial.width*.6f, upperLeft.y + 0.5f*revmeter_dial.height*.6f);
		gearText = "N";
	}
	
	// Update is called once per frame
	void Update () {
		
		int gear = playerCar.GetGear();
		float engineRPM = playerCar.GetEngineRPM();
		desiredAngle = minRevAngle + ( engineRPM / maxRev) * (maxRevAngle-minRevAngle);
		currentAngle = Mathf.Lerp(currentAngle, desiredAngle, Time.deltaTime*10.0f);
		timer += Time.deltaTime;
		if(timer >= 0.1f)
		{
			timer = 0.0f;
			if(gear == 0)
				gearText = "N";
			else if(gear == 7)
				gearText = "R";
			else
				gearText= string.Format("{0:D}", gear);
		}			
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(upperLeft.x, upperLeft.y, revmeter_dial.width*.6f, 
					revmeter_dial.height*.6f), revmeter_dial, ScaleMode.ScaleToFit, true);
		Matrix4x4 savedMatrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(currentAngle, pivot);
		GUI.DrawTexture(new Rect(upperLeft.x, upperLeft.y, revmeter_needle.width*.6f, 
					revmeter_needle.height*.6f), revmeter_needle, ScaleMode.ScaleToFit, true);
		GUI.matrix = savedMatrix;
		GUI.skin = mySkin;
		GUI.Label(new Rect(pivot.x-3.0f, pivot.y+10.0f, 200.0f, 100.0f), gearText);			
	}

}
