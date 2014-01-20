using UnityEngine;
using System.Collections;

public class WaypointGizmo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawCube(transform.position, new Vector3(1.0f, 1.0f, 1.0f));
	}
}
