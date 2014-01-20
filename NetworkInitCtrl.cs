using UnityEngine;
using System.Collections;

public class NetworkInitCtrl : MonoBehaviour {

	void Awake()
	{
		if(!networkView.isMine)
		{
			// Not our own character, disable the control
			GetComponent<PlayerCar>().enabled = false;	
		}
		else
		{
			GetComponent<PlayerCar>().enabled = true;
		} 
	}

}
