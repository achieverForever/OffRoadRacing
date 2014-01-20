using UnityEngine;
using System.Collections;

public class AIHeadSensor : MonoBehaviour {

	private AICar aiCar;
	private bool isBraking;

	// Use this for initialization
	void Start () {
		aiCar = transform.root.GetComponent<AICar>();
		if(!aiCar)
			Debug.LogError("AICar script not found! - AIHeadSensor.cs");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "AICar")
		{
			StartCoroutine(BrakeForSeconds(2.0f, 700.0f));
		}
		else if(other.tag == "Player")
		{
			StartCoroutine(BrakeForSeconds(2.0f, 600.0f));
		}
		else if(other.tag == "BrakeZone")
		{
			StartCoroutine(BrakeForSeconds(2.0f, 400.0f));
		}
	}

	void OnTriggerStay(Collider other){
		if(!isBraking)
		{
			if(other.tag == "AICar")
				StartCoroutine(BrakeForSeconds(2.0f, 500.0f));
			else if(other.tag == "Player")
				StartCoroutine(BrakeForSeconds(2.0f, 500.0f));
		}
	}

	IEnumerator BrakeForSeconds(float time, float force)
	{
		isBraking = true;
		aiCar.SetBrake(true, force);
		yield return new WaitForSeconds(time);
		aiCar.SetBrake(false, 0.0f);
		isBraking = false;
	}
}
