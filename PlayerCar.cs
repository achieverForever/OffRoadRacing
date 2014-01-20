using UnityEngine;
using System.Collections;

public class PlayerCar : AbstractCar {

	public Light[] headLights;
	public Light[] brakeLights;
	public Light[] reverseLights;

	public Skidmarks skidmark;
	public ParticleEmitter skidSmoke;

	public Transform modelsParent;
	public Transform collidersParent;

	private Wheel[] wheels;
	private bool isRemoteControlled;
	private bool isHorn;
	private Vector3 lastForwardDir;
	private float timer;
	// private Vector3 toPosition;
	// private Quaternion toRotation;
	// private Vector3 toVelocity;

	class Wheel{
		public WheelCollider collider;
		public Transform model;
		public bool canSteer = false;
		public int lastIndex;
	}

	// Use this for initialization
	void Start () {

		base.Init();

		// toPosition = rigidbody.position;
		// toRotation = rigidbody.rotation;
		// toVelocity = rigidbody.velocity;

		if(!networkView.isMine)
		{
			isRemoteControlled = true;
			gameObject.name += "_Remote";
		}

		SetupCenterOfMass();

		SetupWheels();
	}
	
	// Update is called once per frame
	void Update () {

		if(!isRemoteControlled)
		{	
			GetInput();	// We ourself control our own player. Otherwise, it is remote controlled.

		}

		LightsControl();

		HornControl();

		CalculateSpeed();

		CheckFlipAndRecover();

		// Physics control
		Steer_Physics(deltaX);

		Throttle_Physics(deltaY);

		Brake_Physics();

		// Visual control
		SteerAndRotate_Model();	

		timer += Time.deltaTime;
		if(timer >= 2.0f)	// Record the last front direction every 2 seconds.
		{
			timer = 0.0f;
			lastForwardDir = transform.forward;
		}

	}

/* 	void OnFixedUpdate()
	{
		if(isRemoteControlled)
		{
			rigidbody.position = Vector3.Lerp(rigidbody.position, toPosition, 0.8f);
			rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, toRotation, 0.8f);
			rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, toVelocity, 0.8f);
		}
	} */

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
			wheels[i].lastIndex = -1;
		}
	}

	public override void GetInput()
	{
		deltaX = Input.GetAxis("Horizontal");
		deltaY = Input.GetAxis("Vertical");		

		if(Input.GetKeyDown("l"))
			headLightsOn = !headLightsOn;

		if(Input.GetKeyDown("r"))
			recovered = true;
		else
			recovered = false;

		if(Input.GetKey("space"))
			braked = true;
		else
			braked = false;

		if(Input.GetKeyDown("h"))
			isHorn = true;
		else
			isHorn = false;
	}

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
		if(recovered)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			lastForwardDir.y = 0.0f;
			transform.rotation = Quaternion.LookRotation(lastForwardDir, Vector3.up);
			transform.position += Vector3.up * 2.0f;
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
					wheels[i].collider.steerAngle = Mathf.LerpAngle(wheels[i].collider.steerAngle, deltaX * minTurn, Time.deltaTime * 4.0f);
				}
				else if(speed_KMP <= 20.0f)
				{
					wheels[i].collider.steerAngle = Mathf.LerpAngle(wheels[i].collider.steerAngle, deltaX * maxTurn, Time.deltaTime * 4.0f);
				}
				else
				{
					wheels[i].collider.steerAngle = Mathf.LerpAngle(wheels[i].collider.steerAngle, deltaX * (k * speed_KMP + b), Time.deltaTime * 4.0f);
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
		// Spin and position the wheel models
		for(int i=0; i<wheels.Length; i++)	
		{
			// Position the wheels' models based on if we are grounded.
			WheelHit hit;
			if (wheels[i].collider.GetGroundHit(out hit))
			{	// Grounded.
				wheels[i].model.position = new Vector3(wheelColliders[i].position.x, 
					hit.point.y + Vector3.Dot(wheelColliders[i].up, Vector3.up) * wheelRadius, wheelColliders[i].position.z);

				/*if(i==0)
				{
					string s = string.Format("forwardSlip: {0:F3}, sidewaySlip: {1:F3}   {2:F5}", hit.forwardSlip, hit.sidewaysSlip, hit.force / wheels[i].collider.suspensionSpring.spring);
					print(s);
				}*/

				// Create skidmark and emit smoke if skidding.
				if(!isRemoteControlled && skidmark && skidSmoke)
				{
					if(Mathf.Abs(hit.sidewaysSlip) >= 1.2f || Mathf.Abs(hit.forwardSlip) >= 0.35f)
					{
						wheels[i].lastIndex = skidmark.AddSkidMark(hit.point + hit.forwardDir*Time.deltaTime*2.0f, hit.normal, 
									hit.sidewaysDir, hit.force / wheels[i].collider.suspensionSpring.spring, wheels[i].lastIndex);
						skidSmoke.Emit(	hit.point + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)),		// Position of the particle.
										Vector3.up, Random.Range(skidSmoke.minSize, skidSmoke.maxSize),										// Velocity and size.
										Random.Range(skidSmoke.minEnergy, skidSmoke.maxEnergy)*.2f, Color.white);							// Lifttime and color.
					}
					else
					{
						wheels[i].lastIndex = -1;
					}					
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

	void LightsControl()
	{
		// Headlights
		foreach(Light light in headLights)
			light.enabled = headLightsOn;

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

		// Reverselights
		if(gear == GearRatios.Length)
		{
			foreach(Light light in reverseLights)
				light.enabled = true;
		}else{
			foreach(Light light in reverseLights)
				light.enabled = false;			
		}	
	}

	void HornControl()
	{
		if(isHorn && !audio.isPlaying)
		{
			audio.Play();
		}
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		float delX = 0.0f, delY = 0.0f;
		bool headLitsOn = false, recov = false, brk = false, isHon = false;
		Vector3 vel = Vector3.zero, pos = Vector3.zero, angVel = Vector3.zero;
		Quaternion rot = Quaternion.identity;

		if(stream.isWriting)
		{	// Sending data
		 	delX = deltaX;
		 	delY = deltaY;
		 	headLitsOn = headLightsOn;
		 	recov = recovered;
		 	brk = braked;
		 	isHon = isHorn;
		 	vel = rigidbody.velocity;
		 	rot = rigidbody.rotation;
		 	pos = rigidbody.position;

		 	stream.Serialize(ref delX);
		 	stream.Serialize(ref delY);
		 	stream.Serialize(ref headLitsOn);
		 	stream.Serialize(ref recov);
		 	stream.Serialize(ref brk);
		 	stream.Serialize(ref isHon);
		 	stream.Serialize(ref vel);
		 	stream.Serialize(ref rot);
		 	stream.Serialize(ref pos);
		}
		else
		{
		 	// Receiving data
		 	stream.Serialize(ref delX);
		 	stream.Serialize(ref delY);
		 	stream.Serialize(ref headLitsOn);
		 	stream.Serialize(ref recov);
		 	stream.Serialize(ref brk);
		 	stream.Serialize(ref isHon);
		 	stream.Serialize(ref vel);
		 	stream.Serialize(ref rot);
		 	stream.Serialize(ref pos);

		 	deltaX = delX;
		 	deltaY = delY;
		 	headLightsOn = headLitsOn;
		 	recovered = recov;
		 	braked = brk;
		 	isHorn = isHon;
		 	float distance = (rigidbody.position - pos).sqrMagnitude;
		 	if( distance >= 0.25f)
		 	{
		 		if(distance >= 5.0f)
		 		{	// If we go too far from our real player, use transform to correct it.
		 			transform.position = pos;
		 			rigidbody.position = pos;
		 			rigidbody.rotation = rot;
		 			rigidbody.velocity = vel;
		 		}
		 		else
		 		{
	 			 	if((rigidbody.velocity - vel).sqrMagnitude > 0.5f)	// Only update our data when it's larger than theadshold to avoid shaking.
	 			 	{
	 				 	// toVelocity = vel;
	 				 	rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, vel, Time.deltaTime);
	 			 	}
	 			 	if(Quaternion.Angle(rigidbody.rotation, rot) > 5.0f)	// Only update our data when it's larger than theadshold to avoid shaking.
	 			 	{
	 			 		// toRotation = rot;
	 			 		rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, rot, Time.deltaTime);
	 			 	}
		 			// toPosition = pos;		 				 			
		 			rigidbody.position = Vector3.Lerp(rigidbody.position, pos, Time.deltaTime);
		 		}
		 	}
		}	 
	}

}
