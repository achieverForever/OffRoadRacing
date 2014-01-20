using UnityEngine;
using System.Collections;

public class AICar : AbstractCar {

	public Light[] brakeLights;

	public Transform modelsParent;
	public Transform collidersParent;
	public LayerMask detectedLayer;
	public ParticleEmitter skidSmoke;

	private Wheel[] wheels;
	private float timer;
	private Transform _transform;

	class Wheel{
		public WheelCollider collider;
		public Transform model;
		public bool canSteer = false;
	}
	// Use this for initialization
	void Start () {
		_transform = transform;
		
		Init();

		SetupCenterOfMass();

		SetupWheels();
	}
	
	// Update is called once per frame
	void Update () {

		GetInput(); // Get input according to the navigator script.

		LightsControl();

		CalculateSpeed();

		CheckFlipAndRecover();

		// Physics control
		Steer_Physics(deltaX);

		Throttle_Physics(deltaY);

		Brake_Physics();

		// Visual control
		SteerAndRotate_Model();	
	}

	public override void SetupWheels()
	{
		wheelModels = new Transform[modelsParent.childCount];
		for(int i=0; i<wheelModels.Length; i++)
			wheelModels[i] = modelsParent.GetChild(i);

		wheelColliders = new Transform[collidersParent.childCount];
		for(int i=0; i<wheelColliders.Length; i++)
			wheelColliders[i] = collidersParent.GetChild(i);

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
		}
	}

	public override void GetInput() {} // We retrieve AI input control from AINavigator.cs script	

	public override void CalculateSpeed()
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

	public override void CheckFlipAndRecover()	
	{
		Ray ray = new Ray(_transform.position, -_transform.up);
		//Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2.0f, Color.white);
		if(!Physics.Raycast(ray, 10.0f, detectedLayer.value))
			timer += Time.deltaTime;
		if(timer >= 5.0f)
		{
			timer = 0.0f;
			_transform.rotation = Quaternion.identity;
			_transform.position += Vector3.up;
		}
	}

	public override void Steer_Physics(float deltaX)
	{
		for(int i=0; i<wheels.Length; i++) // Apply steering to the front WheelColliders
		{
			if(wheels[i].canSteer)
			{
				if(speed_KMP >= 50.0f)	// Constrain the steer angles when we are at high speed.
				{
					wheels[i].collider.steerAngle = deltaX * minTurn;
				}
				else if(speed_KMP <= 20.0f)
				{
					wheels[i].collider.steerAngle = deltaX * maxTurn;
				}
				else
				{
					wheels[i].collider.steerAngle = deltaX * (k * speed_KMP + b);
				}
			 }
		}
	}

	public override void Throttle_Physics(float deltaY)
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

		for(int i=0; i<wheels.Length; i++)
			wheels[i].collider.motorTorque = wheelTorque;
	}

	public override void Brake_Physics()
	{
		if(braked){
			for(int i=0; i<wheels.Length; i++)
			{
				if(wheels[i].canSteer)
					wheels[i].collider.brakeTorque = brakeForce * .8f;				
				else
					wheels[i].collider.brakeTorque = brakeForce;
			}
		}else
		{
			for(int j=0; j<wheels.Length; j++)
			{
				wheels[j].collider.brakeTorque = resistForce;
			}
		}
	}

	// Rotate the geometry wheel models
	public override void SteerAndRotate_Model()
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
		
				// Skidsmoke
				if(Mathf.Abs(hit.sidewaysSlip) >= 1.6f || Mathf.Abs(hit.forwardSlip) >= 0.4f)
				{
					skidSmoke.Emit(	hit.point + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)),		// Position of the particle.
									Vector3.up + _transform.forward * 0.2f, Random.Range(skidSmoke.minSize, skidSmoke.maxSize),										// Velocity and size.
									Random.Range(skidSmoke.minEnergy, skidSmoke.maxEnergy)*.2f, Color.white);							// Lifttime and color.
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
				relaxedPos += -_transform.root.up * wheels[i].collider.suspensionDistance;
				wheels[i].model.position = relaxedPos;
			}

			// Rotate the wheels' models
			if(wheels[i].canSteer)	// The steering wheels' spinning need some special processing.
				wheels[i].model.GetChild(0).Rotate(Vector3.right, wheels[i].collider.rpm * 6.0f * Time.deltaTime);
			else
				wheels[i].model.Rotate(Vector3.right, wheels[i].collider.rpm * 6.0f * Time.deltaTime);
		}

	}

	void LightsControl()
	{
		// Brakelights
		if(braked)
		{
			foreach(Light light in brakeLights)
				light.enabled = true;
		}
		else
		{
			foreach(Light light in brakeLights)
				light.enabled = false;
		}
	}

	public void SetDeltaX(float x)
	{
		this.deltaX = x;
	}

	public void SetDeltaY(float y)
	{
		this.deltaY = y;
	}

	public void SetBrake(bool isBraked, float force)
	{
		this.braked = isBraked;
		this.brakeForce = force;
	}

	public bool isBraked()
	{
		return this.braked;
	}

}
