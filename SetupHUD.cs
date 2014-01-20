using UnityEngine;
using System.Collections;

public class SetupHUD : MonoBehaviour {

	public GameObject target;
	public bool isNewTarget;

	// Use this for initialization
	void Start () {
		transform.GetChild(0).GetComponent<RevolutionmeterControl>().enabled = false;
		transform.GetChild(1).GetComponent<SpeedometerControl>().enabled = false;
	}

	void Update()
	{
		if(isNewTarget)
		{
			isNewTarget = false;

			transform.GetChild(0).GetComponent<RevolutionmeterControl>().enabled = true;
			transform.GetChild(1).GetComponent<SpeedometerControl>().enabled = true;

			transform.GetChild(0).GetComponent<RevolutionmeterControl>().vehicle = target;
			transform.GetChild(1).GetComponent<SpeedometerControl>().vehicle = target;			
		}
	}
}
