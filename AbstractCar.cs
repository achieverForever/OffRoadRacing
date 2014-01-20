using UnityEngine;
using System.Collections;

public abstract class AbstractCar : MonoBehaviour { 

	protected enum State{ Forward, Stop, Reverse };

	// Don't know how to encapsulate these members, just "public" them temporally.
	public float wheelRadius;
	public Transform centerOfMass;

	public float brakeForce = 500.0f;
	public float resistForce = 100.0f;		// Forces to slow down the car
	public float maxTurn = 25.0f;
	public float minTurn = 10.0f;

	public float[] GearRatios = null;
	public float[] SpeedRanges = null;

	protected Transform[] wheelModels;
	protected Transform[] wheelColliders;

	protected float speed_MPS, speed_KMP;
	protected float engRPM = 0.0f;
	protected int gear;
	protected float engineRPM, engineTorque, wheelTorque;
	protected State currentState = State.Stop;
	protected float k, b;
	protected WheelFrictionCurve wfc;

	// Variables to store the input state.
	protected float deltaX, deltaY;
	protected bool braked, recovered, headLightsOn; 


	// Use this for initialization
	protected void Init() {		
		k = (minTurn - maxTurn) / 30.0f;
		b =	-0.66667f * minTurn + 1.66667f * maxTurn;
		gear = 0;
	}
	
	protected void SetupCenterOfMass()
	{
		rigidbody.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);	// Set up the centerOfMass of the rigidbody
	}

	protected void UpdateCurrentState()
	{
		float tmp = Vector3.Dot(rigidbody.velocity, transform.forward);
		if(rigidbody.velocity.sqrMagnitude <= 0.1f)
		{
			currentState = State.Stop;
		}else if(tmp >= 0.0f)
		{
			currentState = State.Forward;
		}
		else
		{
			currentState = State.Reverse;
		}
	}

	protected int ShiftGear(float speed)
	{
		int g = 1;
		for(int i = 0; i < SpeedRanges.Length ; i++)
		{
			if(speed - SpeedRanges[i] > 0.1f)
				g = i + 1;
			else
				break;
		}
		return g;
	}

	protected float LookupTorqueCurve(float engRPM)
	{
		engRPM = Mathf.Clamp(engRPM, 1000.0f, 7000.0f);
		float maxTorque = 0.0f;
		if(engRPM >= 1000.0f && engRPM <= 5000.0f)
			maxTorque = .05f * engRPM + 350.0f;
		else
			maxTorque = -.05f * engRPM + 850.0f;
		return maxTorque;
	}

	public float GetSpeed_KMP()
	{
		return speed_KMP;
	}

	public int GetGear()
	{
		return gear;
	}

	public float GetEngineRPM(){
		return engRPM;
	}

	// Must be overridden by derived classes.
	public abstract void SetupWheels();

	public abstract void CalculateSpeed();

	public abstract void CheckFlipAndRecover();

	public abstract void Steer_Physics(float deltaX);

	public abstract void Throttle_Physics(float deltaY);

	public abstract void Brake_Physics();

	public abstract void SteerAndRotate_Model();

	public abstract void GetInput();
}
