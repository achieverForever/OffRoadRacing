using UnityEngine;
using System.Collections;

public class WheelControlTest : MonoBehaviour {

	private enum State{ Forward, Stop, Reverse };

	public Transform[] wheelModels;
	public Transform[] wheelColliders;
	public float wheelRadius;
	public Transform centerOfMass;

	public float brakeForce = 500.0f;
	public float resistForce = 100.0f;		// Forces to slow down the car
	public float maxTurn = 25.0f;
	public float minTurn = 10.0f;

	public float[] GearRatios = null;
	public float[] SpeedRanges = null;

	public Light[] headLights;
	public Light[] brakeLights;
	public Light[] reverseLights;

	public Skidmarks skidmarks;
	public ParticleEmitter skidSmoke;

	private Wheel[] wheels;
	private float speed_MPS = 0.0f, speed_KMP = 0.0f;
	private float engRPM = 0.0f;
	private int gear;
	private float engineRPM, engineTorque, wheelTorque;
	private State currentState = State.Stop;
	private float k, b;
	private WheelFrictionCurve wfc;

	// Variables to store the input state.
	private float deltaX, deltaY;
	private bool braked, recovered, headLightsOn; 



	class Wheel{
		public WheelCollider collider;
		public Transform model;
		public bool canSteer = false;
		public int lastIndex;
	}

	// Use this for initialization
	void Start () {
		SetupWheels();
		
		SetupCenterOfMass(centerOfMass.position);

		k = (minTurn - maxTurn) / 13.0f;
		b = maxTurn - 5 * k;
		gear = 0;
	}
	
	// Update is called once per frame
	void Update () {

		GetInput();

		CalculateSpeed();

		CheckFlipAndRecover();

		// Physics control
		Steer_Physics(deltaX, speed_MPS);

		Throttle_Physics(deltaY);

		Brake_Physics();

		// Visual control
		SteerAndRotate_Model();	
	}

	void SetupWheels()
	{
		wheels = new Wheel[wheelColliders.Length];
		for(int i=0; i<wheelColliders.Length; i++)
		{
			wheels[i] = new Wheel();
			wheels[i].collider = wheelColliders[i].GetComponent<WheelCollider>();
			wheels[i].model = wheelModels[i];
			if(i<2)
				wheels[i].canSteer = true;
			else
				wheels[i].canSteer = false;
			wheels[i].lastIndex = -1;
		}
	}

	void SetupCenterOfMass(Vector3 pos)
	{
		rigidbody.centerOfMass = transform.InverseTransformPoint(pos);	// Set up the centerOfMass of the rigidbody
	}

	void GetInput()
	{
		deltaX = Input.GetAxis("Horizontal");
		deltaY = Input.GetAxis("Vertical");		

		if(Input.GetKeyDown("l"))
		{
			headLightsOn = !headLightsOn;
			foreach(Light light in headLights)
				light.enabled = headLightsOn;
		}
		if(Input.GetKeyDown("r"))
			recovered = true;
		else
			recovered = false;
		if(Input.GetKey("space"))
			braked = true;
		else
			braked = false;
	}

	void CalculateSpeed()
	{
		speed_MPS = wheelRadius * wheels[3].collider.rpm * .033333334f * Mathf.PI;
		speed_MPS = Mathf.Abs(speed_MPS);
		speed_KMP = speed_MPS * 3.6f;
		if(gear == 0){
			engRPM = 1000.0f;
		}else{
			engRPM = wheels[3].collider.rpm*GearRatios[gear-1]*3.6f;
		}
		engRPM = Mathf.Clamp(engRPM, 1000.0f, 7000.0f);
	}

	void CheckFlipAndRecover()
	{
		if(recovered)
			transform.root.rotation = Quaternion.identity;
	}

	void Steer_Physics(float deltaX, float velocity)
	{
		for(int i=0; i<wheels.Length; i++) // Apply steering to the front WheelColliders
		{
			if(wheels[i].canSteer)
			{
				if(Mathf.Abs(speed_MPS) >= 15.0f)	// Constrain the steer angle when we are at high speed.
				{
					wheels[i].collider.steerAngle = Mathf.Lerp(wheels[i].collider.steerAngle, deltaX * minTurn, Time.deltaTime * 4.0f);
				}else if(Mathf.Abs(speed_MPS) <= 10.0f)
				{
					wheels[i].collider.steerAngle = Mathf.Lerp(wheels[i].collider.steerAngle, deltaX * maxTurn, Time.deltaTime * 4.0f);
				}else
				{
					wheels[i].collider.steerAngle = Mathf.Lerp(wheels[i].collider.steerAngle, deltaX * (k * velocity + b), Time.deltaTime * 4.0f);
				}
			}
		}
	}

	void Throttle_Physics(float deltaY)
	{
		//engineRPM = colliders[3].rpm * GearRatios[gear-1];			
		float airDrag = speed_MPS * speed_MPS * 0.3f * wheelRadius;		// Calculate air resistance.

		engineRPM = 5000.0f;
		engineTorque = LookupTorqueCurve(engineRPM);

		UpdateCurrentState();
		if(currentState == State.Stop)	// Stop state
		{
			if(deltaY >= 0.05f)
			{
				gear = ShiftGear(speed_KMP);			
				wheelTorque = ((engineTorque * deltaY) * GearRatios[gear-1] - airDrag) * 0.25f;
			}else if(deltaY <= -0.05f)
			{
	 			gear = GearRatios.Length;
				wheelTorque = -((engineTorque * deltaY) * GearRatios[gear-1]- airDrag) * 0.25f;			
			}else{
				gear = 0;
				wheelTorque = 0.0f;				
			}
		}else if(currentState == State.Reverse)	// Reverse state
		{
//			if(deltaY <= 0.0f){
	 			gear = GearRatios.Length;
				wheelTorque = -((engineTorque * deltaY) * GearRatios[gear-1]- airDrag) * 0.25f;		
//			}
		}else{	// Forward state
//			if(deltaY >= 0.0f){
				gear = ShiftGear(speed_KMP);			
				wheelTorque = ((engineTorque * deltaY) * GearRatios[gear-1] - airDrag) * 0.25f;
//			}
		}
		if(gear == GearRatios.Length)
		{
			foreach(Light light in reverseLights)
				light.enabled = true;
		}else{
			foreach(Light light in reverseLights)
				light.enabled = false;			
		}	


		for(int i=0; i<wheels.Length; i++)
			wheels[i].collider.motorTorque = wheelTorque;
	}

	void Brake_Physics()
	{
		if(braked){
			foreach(Light light in brakeLights)
				light.enabled = true;
			for(int i=0; i<wheels.Length; i++)
			{
				if(wheels[i].canSteer)
					wheels[i].collider.brakeTorque = brakeForce * .8f;				
				else
					wheels[i].collider.brakeTorque = brakeForce;
			}
		}else
		{
			foreach(Light light in brakeLights)
				light.enabled = false;
			for(int j=0; j<wheels.Length; j++)
			{
				wheels[j].collider.brakeTorque = resistForce;
			}
		}
	}

	void UpdateCurrentState()
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

	// Rotate the geometry wheel models
	void SteerAndRotate_Model()
	{
		for(int i=0; i<wheels.Length; i++)	// Steering
		{
			if(wheels[i].canSteer)
			{
				Vector3 ea = wheels[i].model.localEulerAngles;
				ea.y = wheels[i].collider.steerAngle;
				wheels[i].model.localEulerAngles = ea;				
			}
		}
		for(int i=0; i<wheels.Length; i++)	// Spinning and positioning
		{
			// Position the wheels' models based on if we are grounded.
			WheelHit hit;
			if (wheels[i].collider.GetGroundHit(out hit))
			{	
				wheels[i].model.position = new Vector3(wheelColliders[i].position.x, hit.point.y + wheelRadius, wheelColliders[i].position.z);

				if(i==0)
				{
					Debug.DrawLine(hit.point, wheels[i].model.position, Color.white);
					string s = string.Format("forwardSlip: {0:F3}, sidewaySlip: {1:F3}   {2:F5}", hit.forwardSlip, hit.sidewaysSlip, hit.force / wheels[i].collider.suspensionSpring.spring);
					print(s);
				}

				// Create skidmark and emit smoke if skidding.
				if(Mathf.Abs(hit.sidewaysSlip) >= 1.6f || Mathf.Abs(hit.forwardSlip) >= 0.35f)
				{
					wheels[i].lastIndex = skidmarks.AddSkidMark(hit.point + hit.forwardDir*Time.deltaTime*2.0f, hit.normal, 
								hit.sidewaysDir, hit.force / wheels[i].collider.suspensionSpring.spring, wheels[i].lastIndex);
					skidSmoke.Emit(	hit.point + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)),		// Position of the particle.
									Vector3.up, Random.Range(skidSmoke.minSize, skidSmoke.maxSize),										// Velocity and size.
									Random.Range(skidSmoke.minEnergy, skidSmoke.maxEnergy)*.2f, Color.white);							// Lifttime and color.
				}
				else
				{
					wheels[i].lastIndex = -1;
				}

				// Adjust the friction factor according to the collider we hit.
				wfc = wheels[i].collider.forwardFriction;
				wfc.stiffness = hit.collider.material.staticFriction * 0.8f;
				wheels[i].collider.forwardFriction = wfc;

				wfc = wheels[i].collider.sidewaysFriction;
				wfc.stiffness = hit.collider.material.staticFriction;
				wheels[i].collider.sidewaysFriction = wfc;

			}
			else
			{	// Not grounded, set it to the relaxed position.
				Vector3 relaxedPos = wheelColliders[i].position;
				relaxedPos += -transform.root.up * wheels[i].collider.suspensionDistance;
				wheels[i].model.position = relaxedPos;
			}

			// Rotate the wheels' models
			if(wheels[i].canSteer)	// The steering wheels' spinning need some special processing.
				wheels[i].model.GetChild(0).Rotate(Vector3.right, wheels[i].collider.rpm * 6.0f * Time.deltaTime);
			else
				wheels[i].model.Rotate(Vector3.right, wheels[i].collider.rpm * 6.0f * Time.deltaTime);
		}

	}

	int ShiftGear(float speed)
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

	float LookupTorqueCurve(float engRPM)
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
}
