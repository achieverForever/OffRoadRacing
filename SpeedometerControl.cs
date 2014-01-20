using UnityEngine;
using System.Collections;

public class SpeedometerControl : MonoBehaviour {

	public GameObject vehicle;
	public Texture speedometer_dial;
	public Texture speedometer_needle;
	public float minSpeedAngle, maxSpeedAngle;
	public float maxSpeed;
	public GUISkin mySkin;

	private Vector2 pivot = Vector2.zero;
	private Vector2 upperLeft;
	private float currentAngle, desiredAngle;
	private PlayerCar playerCar;
	private float timer = 0.0f;
	private string speedText;

	// Use this for initialization
	void Start () {
		if(vehicle != null)
		{
			playerCar = vehicle.GetComponent<PlayerCar>();
			if(!playerCar)
			{
				Debug.LogError("PlayerCar script not found");
			}
		}
		upperLeft = new Vector2(Screen.width-speedometer_dial.width-90.0f, Screen.height-speedometer_dial.height-10.0f);
		pivot = new Vector2(upperLeft.x + 0.5f*speedometer_dial.width, upperLeft.y + 0.5f*speedometer_dial.height);
		speedText = "0";						
	}
	
	// Update is called once per frame
	void Update () {
		float speed_mph = playerCar.GetSpeed_KMP() * 0.621371192f;
		desiredAngle = minSpeedAngle + ( speed_mph / maxSpeed) * (maxSpeedAngle-minSpeedAngle);
		currentAngle = Mathf.Lerp(currentAngle, desiredAngle, Time.deltaTime*10.0f);
		timer += Time.deltaTime;
		if(timer >= 0.1f)
		{
			timer = 0.0f;
			speedText = string.Format("{0:F0}", speed_mph);
		}
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(upperLeft.x, upperLeft.y, speedometer_dial.width, 
					speedometer_dial.height), speedometer_dial, ScaleMode.ScaleToFit, true);
		Matrix4x4 savedMatrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(currentAngle, pivot);
		GUI.DrawTexture(new Rect(upperLeft.x, upperLeft.y, speedometer_needle.width, 
					speedometer_needle.height), speedometer_needle, ScaleMode.ScaleToFit, true);
		GUI.matrix = savedMatrix;
		GUI.skin = mySkin;
		GUI.Label(new Rect(pivot.x-8.0f, pivot.y+28.0f, 200.0f, 100.0f), speedText);
	}

}
