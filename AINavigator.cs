using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AINavigator : MonoBehaviour {

	private AICar aiCar;
	private float deltaX, deltaY;
	private bool braked;
	
	private float steerAngle, maxTurn;
	private Transform _transform;
	private Vector3 targetDir;
	private Queue<Vector3> wayPtQueue = null;
	private float resetTimer, updateTimer1 = 1.0f;		// Timer used to reset the position of the AI car if it loses its waypoints.

	// Record the last position and last waypoint so that we can restore to it.
	private Vector3 last2Waypt = Vector3.zero, last1Waypt = Vector3.zero;	

	// Use this for initialization
	void Start () {
		_transform = transform;
		wayPtQueue = new Queue<Vector3>();
		aiCar = _transform.root.GetComponent<AICar>();
		if(!aiCar)
			Debug.LogError("AICar script not found! - AINavigator.cs");
		maxTurn = aiCar.maxTurn;
	}
	
	// Update is called once per frame
	void Update () {
		if(wayPtQueue.Count == 0)	// No waypoint detected.
		{
			deltaX = steerAngle = 0.0f;
			deltaY = 0.4f;
			resetTimer += Time.deltaTime;
			if(resetTimer >= 5.0f)
			{
				resetTimer = 0.0f;
				_transform.root.rigidbody.velocity = Vector3.zero;
				_transform.root.position = last2Waypt + Vector3.up;
 				_transform.root.position -= _transform.root.forward * 2.0f;
				wayPtQueue.Clear();
			}
		}
		else
		{
			// Calculate the steerAngle we need to turn to the focused waypoint.
			Vector3 targetPt;
			Vector3 currPos = _transform.root.position;

 			targetPt = wayPtQueue.Peek();	// Fetch the first met waypoint, we need to turn to it first.
	
			targetPt.y = currPos.y = 0.0f;
			targetDir = (targetPt - currPos).normalized;
			steerAngle = Vector3.Angle(_transform.forward, targetDir);
			if( AngleDir(_transform.forward, targetDir, Vector3.up) == -1 )
				steerAngle = -steerAngle;

			Debug.DrawLine(currPos, targetPt, Color.white);

			deltaY = 0.8f;
			deltaX = Mathf.Clamp(steerAngle / maxTurn, -1.0f, 1.0f);

			updateTimer1 += Time.deltaTime;	// Update the last1Waypt and last2Waypt every 2 and 4 seconds respectively.
			if(updateTimer1 >= 2.0f)
			{
				updateTimer1 = 0.0f;
				last2Waypt = last1Waypt;
				last1Waypt = targetPt;
			}
		}

		if(aiCar.isBraked())
			deltaY = 0.0f;

		aiCar.SetDeltaX(deltaX);
		aiCar.SetDeltaY(deltaY);
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Waypoint")
			wayPtQueue.Enqueue(other.transform.position);
	}

	void OnTriggerExit(Collider other){
		if(other.tag == "Waypoint" && wayPtQueue.Count>0)
			wayPtQueue.Dequeue();	
	}

	int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		Vector3 perp = Vector3.Cross(fwd, targetDir);
		float dir = Vector3.Dot(perp, up);
		
		if (dir > 0f) {
			return 1;
		} else if (dir < 0f) {
			return -1;
		} else {
			return 0;
		}
	}

}
