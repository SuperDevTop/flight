using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
[RequireComponent(typeof(Rigidbody))][RequireComponent(typeof(Collider))]
public class SilantroMunition : MonoBehaviour {
	//DEFINITION
	public enum MunitionType
	{
		Missile,Rocket,Bullet,Bomb
	}
	[HideInInspector]public MunitionType munitionType = MunitionType.Rocket;
	[HideInInspector]public SilantroPylon connectedPylon;
	// ----------------------------------------------------WEAPON CLASSES------------------------------------------------------------------------------------------------------
	//1.ROCKET
	public enum RocketType
	{
		Guided,Unguided
	}
	[HideInInspector]public RocketType rocketType = RocketType.Unguided;
	public enum FuzeType
	{
		MK352,//Time
		M423,//Proximity
		MK193Mod0,//Impact
	}
	[HideInInspector]public FuzeType fuzeType = FuzeType.M423;
	[HideInInspector]public string detonationMechanism = "Default";
	[HideInInspector]public float timer = 10f;
	[HideInInspector]public float selfDestructTimer;
	float triggerTimer;
	//PROXIMITY
	[HideInInspector]public float proximity = 100f;//Distance to target
	[HideInInspector]public Transform target;
	bool lostTarget;
	//2. BOMB
	public enum TriggerMechanism
	{
		Proximity,
		ImpactForce
	}
	[HideInInspector]public TriggerMechanism triggerMechanism = TriggerMechanism.ImpactForce;
	[HideInInspector]public float detonationDistance = 100f;
	[HideInInspector]public float speedThreshhold = 10;
	//
	[HideInInspector]public float CDCoefficient;
	[HideInInspector]public float surfaceArea;
	[HideInInspector]public float percentageSkinning = 70f;
	[HideInInspector]public float dragForce;
	[HideInInspector]public float fillingWeight = 10f;
	[HideInInspector]public bool falling;
	[HideInInspector]public float speed;
	[HideInInspector]public float distanceToTarget;
	[HideInInspector]public float fallTime;
	//
	//3. MISSILE
	public enum MissileType
	{
		ASM,
		AAM,
		SAM
	}
	[HideInInspector]public MissileType missileType = MissileType.ASM;
	public enum DetonationType
	{
		Proximity,
		Impact,
		Timer
	}
	[HideInInspector]public DetonationType detonationType = DetonationType.Impact;
	//CONTROL
	[HideInInspector]public string Identifier;
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Rigidbody munition;
	[HideInInspector]public SilantroRocketMotor motorEngine;
	[HideInInspector]public SilantroComputer computer;
	[HideInInspector]public bool armed;
	//DIMENSIONS
	[HideInInspector]public float munitionDiameter = 1f;
	[HideInInspector]public float munitionLength = 1f;
	[HideInInspector]public float munitionWeight = 5f;
	//
	[HideInInspector]public float maximumRange = 1000f;
	[HideInInspector]public float distanceTraveled;
	[HideInInspector]public float activeTime;
	//WARHEAD
	public enum ExplosiveType
	{
		RDX,
		TNT,
		RDX_TNT,
		PETN,
		Nitroglycerine
	}
	[HideInInspector]public ExplosiveType explosiveType = ExplosiveType.RDX;
	//EXPLOSIVE PROPERTIES
	[HideInInspector]public float density;
	[HideInInspector]public float detonationVelocity;
	[HideInInspector]public float streamingVelocity;
	[HideInInspector]public float detonationPressure;
	[HideInInspector]public float energy;
	[HideInInspector]public GameObject explosionPrefab;
	bool exploded;
	//
	//
	//4. BULLET
	//
	//PROPERTIES
	public enum AmmunitionType
	{
		AP,
		HEI,
		FMJ,
		Tracer
	}
	[HideInInspector]public AmmunitionType ammunitionType = AmmunitionType.Tracer;
	public enum AmmunitionForm
	{
		SecantOgive,//0.171
		TangentOgive,//0.165
		RoundNose,//0.235
		FlatNose,//0.316
		Spitzer//0.168
	}
	[HideInInspector]public AmmunitionForm ammunitionForm = AmmunitionForm.RoundNose;
	[HideInInspector]public float mass;
	[HideInInspector]public float caseLength;
	[HideInInspector]public float overallLength;
	[HideInInspector]public float diameter;
	[HideInInspector]float area;
	//PERFORMANCE
	[HideInInspector]public float ballisticVelocity;
	[HideInInspector]float currentVelocity;
	[HideInInspector]public float currentEnergy;
	[HideInInspector]public float drag;
	[HideInInspector]public float skinningRatio;
	[HideInInspector]public float dragCoefficient;
	//
	[HideInInspector]public GameObject groundHit;
	[HideInInspector]public GameObject metalHit;
	[HideInInspector]public GameObject woodHit;
	//
	[HideInInspector]public float airDensity;
	[HideInInspector]public float altitude;
	[HideInInspector]public float destroyTime;float bullettimer;
	public enum BulletFuzeType
	{
		M1032,
		ME091
	}
	[HideInInspector]public BulletFuzeType bulletfuseType = BulletFuzeType.M1032;
	[HideInInspector]public Collider ammunitionCasing;
	[HideInInspector]public float damage;
	[HideInInspector]float damageMulitplier;
	[HideInInspector]float damageFactor;//f-a
	[HideInInspector]float damageCompiler;//a
	[HideInInspector]public Vector3 ejectionPoint;
	//
	Vector3 dropVelocity;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Fires the bullet.
	/// </summary>
	/// <param name="muzzleVelocity"> muzzle/exit velocity of the gun.</param>
	/// <param name="parentVelocity">velocity vector of the parent aircraft.</param>
	public void FireBullet(float muzzleVelocity,Vector3 parentVelocity)
	{
		//DETERMINE INITIAL SPEED
		float startingSpeed;
		if (muzzleVelocity > ballisticVelocity) {
			startingSpeed = muzzleVelocity;
		} else {
			startingSpeed = ballisticVelocity;
		}
		//ADD BASE SPEED
		Vector3 ejectVelocity = transform.forward * startingSpeed;
		Vector3 resultantVelocity = ejectVelocity + parentVelocity;
		//RELEASE BULLET
		munition.velocity = resultantVelocity;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// Releases the bomb or rocket.
	/// </summary>
	public void ReleaseMunition()
	{
		//GET VELOCITY FROM PARENT
		if (connectedAircraft != null) {
			dropVelocity = connectedAircraft.velocity;
		}
		//LAUNCH ROCKET
		if (munitionType == MunitionType.Rocket) {
			munition.transform.parent = null;
			munition.isKinematic = false;
			munition.velocity = dropVelocity;
			motorEngine.FireRocket ();
			StartCoroutine(TimeStep(0.3f));
		}
		//DROP BOMB
		if (munitionType == MunitionType.Bomb) {
			munition.transform.parent = null;
			munition.isKinematic = false;
			munition.velocity = dropVelocity;
			StartCoroutine(TimeStep(1f));
			falling = true;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SMART/GUIDED MUNITIONS
	/// <summary>
	/// Fires guided munition.
	/// </summary>
	/// <param name="markedTarget">locked target from the radar.</param>
	/// <param name="ID">tracking ID for the locked target</param>
	/// <param name="mode">fire mode for the missiles; 1: Drop, 2: Tube, 3: Trapeze Launch</param>
	public void FireMunition(Transform markedTarget, string ID, int mode)
	{
		//GET VELOCITY FROM PARENT
		if (connectedAircraft != null) {
			dropVelocity = connectedAircraft.velocity;
		}
		//LAUNCH ROCKET
		if (munitionType == MunitionType.Rocket) {
			if (computer != null && rocketType == RocketType.Guided) {
				//FIRE
				motorEngine.FireRocket ();
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				StartCoroutine(TimeStep(0.3f));
				//SET TARGET DATA
				computer.Target = markedTarget;
				computer.targetID = ID;
				target = markedTarget;
				//ACTIVATE SEEKER
				computer.seeking = true;
				computer.active = true;
			}
		}
		//LAUNCH MISSILE
		if (munitionType == MunitionType.Missile) {
			//1. DROP MISSILE
			if (mode == 1) {
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget,ID));
			}
			//2. TUBE LAUNCH
			if (mode == 2) {
				//FIRE
				motorEngine.FireRocket ();
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				StartCoroutine(TimeStep(0.8f));
				//SET TARGET DATA
				computer.Target = markedTarget;
				computer.targetID = ID;
				target = markedTarget;
				//ACTIVATE SEEKER
				computer.seeking = true;
				computer.active = true;
			}


			//3. TRAPEZE LAUNCH RIGHT
			if (mode == 3) {
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//PUSH OUT
				float pushForce = munition.mass * 500f;
				Vector3 force = munition.transform.right * pushForce;
				munition.AddForce (force);
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget,ID));
			}

			//4. TRAPEZE LAUNCH LEFT
			if (mode == 4) {
				munition.transform.parent = null;
				munition.isKinematic = false;
				munition.velocity = dropVelocity;
				//PUSH OUT
				float pushForce = munition.mass * 500f;
				Vector3 force = munition.transform.right * -pushForce;
				munition.AddForce (force);
				//FIRE
				StartCoroutine(WaitForDrop(markedTarget,ID));
			}
			//REMOVE PYLON
			if (connectedPylon != null && connectedPylon.pylonPosition == SilantroPylon.PylonPosition.External) {
				Destroy (connectedPylon.gameObject);
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTIVATE AFTER DROP
	IEnumerator WaitForDrop(Transform markedTarget,string ID)
	{
		yield return new WaitForSeconds (1f);
		////FIRE
		motorEngine.FireRocket ();
		StartCoroutine(TimeStep(0.8f));
		//SET TARGET DATA
		computer.Target = markedTarget;
		computer.targetID = ID;
		target = markedTarget;
		//ACTIVATE SEEKER
		computer.seeking = true;
		computer.active = true;
	}
	//CLEAR AIRCRAFT BEFORE ARMING
	IEnumerator TimeStep(float time)
	{
		yield return new WaitForSeconds (time);
		armed = true;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SETUP WEAPON
	public void InitializeMunition()
	{
		//GET COMPONENTS
		munition = GetComponent<Rigidbody>();
		//SETUP MISSILES?ROCKETS?BOMBS
		if (munitionType != MunitionType.Bullet) {
			computer = GetComponentInChildren<SilantroComputer> ();
			if (computer == null) {
				Debug.Log ("No computer is connected to munition " + transform.name);
			} else {
				if (munition != null) {
					computer.connectedSystem = munition;
				}
			}
			//SET ROCKET MOTOR PROPERTIES
			if (motorEngine != null) {
				motorEngine.computer = computer;
				motorEngine.weapon = munition;
				motorEngine.InitializeRocket ();
			}
		}
		//SET FACTORS
		if (munition != null) {
			if (munitionType != MunitionType.Bullet) {
				munition.mass = munitionWeight;
				munition.isKinematic = true;
			} else {
				munition.mass = ((mass*0.0648f)/1000f);
			}
			munition.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
		} else {
			Debug.Log ("Rigidbody for munition is missing " + transform.name);
		}
		armed = false;
		//SEND DATA
		if (munitionType == MunitionType.Missile) {
			computer.connectedSystem = munition;
			computer.computerType = SilantroComputer.ComputerType.Guidance;
			computer.InitializeComputer ();
		}
		if (munitionType == MunitionType.Rocket) {
			if (rocketType == RocketType.Guided) {
				computer.computerType = SilantroComputer.ComputerType.Guidance;
				computer.InitializeComputer ();
			} else {
				computer.computerType = SilantroComputer.ComputerType.DataProcessing;
				target = null;computer.Target = null;
			}
		}
		if (munitionType == MunitionType.Bomb) {
			//CALCULATE AREA
			float radius = munitionDiameter/2;
			float a1 = 3.142f*radius*radius;
			float error = UnityEngine.Random.Range((percentageSkinning-4.5f),(percentageSkinning +5f));
			surfaceArea = (a1 )*(error/100f);
		}
		if (munitionType == MunitionType.Bullet) {
			//SET AERODYNAMIC PROPERTIES
			if (ammunitionForm == AmmunitionForm.FlatNose) {
				skinningRatio = 0.99f;
				dragCoefficient = 0.316f;
			} 
			else if (ammunitionForm == AmmunitionForm.SecantOgive) {
				skinningRatio = 0.913f;
				dragCoefficient = 0.171f;
			}
			else if (ammunitionForm == AmmunitionForm.RoundNose) {
				skinningRatio = 0.95f;
				dragCoefficient = 0.235f;
			} 
			else if (ammunitionForm == AmmunitionForm.TangentOgive) {
				skinningRatio = 0.914f;
				dragCoefficient = 0.165f;
			} 
			else if (ammunitionForm == AmmunitionForm.Spitzer) {
				skinningRatio = 0.921f;
				dragCoefficient = 0.168f;
			}
			//
			ammunitionCasing = GetComponent<Collider> ();
			if (munition == null) {
				Debug.Log ("Bullet " + transform.name + " rigidbody has not been assigned");
			} else {
				if (ammunitionCasing == null) {
					Debug.Log ("Ammunition cannot work without a collider");
				} else {
					float radius = diameter / 2000f;
					area = Mathf.PI * radius * radius;
				}
			}
			//
			float a = Random.Range(78,98);
			float f = Random.Range (27, 43);
			damageCompiler = a;
			damageFactor = f - a;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAW MARKERS
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		//SEND DATA TO ENGINE
		if (motorEngine != null && munitionType != MunitionType.Bullet) {
			motorEngine.boosterDiameter = munitionDiameter;
			motorEngine.overallLength = munitionLength;
		}
		Identifier = transform.name;

		//DRAW IDENTIFIER
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position,0.1f);
		Gizmos.color = Color.yellow;
		if (connectedAircraft != null) {
			Gizmos.DrawLine (this.transform.position, (connectedAircraft.transform.up * 2f + this.transform.position));
		} else {
			Gizmos.DrawLine (this.transform.position, (this.transform.up * 2f + this.transform.position));
		}
	}
	#endif





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//MUNITION COLLISION
	void OnCollisionEnter(Collision col)
	{
		//1. ROCKET
		if (munitionType == MunitionType.Rocket) {
			//TRIGGER WITH IMPACT
			if (armed && fuzeType == FuzeType.MK193Mod0) {
				Explode ();
			}
			//DESTROY IF IMPACT IS STRONG ENOUGH
			if (armed && munition.velocity.magnitude > 5) {
				StartCoroutine (WaitForMomentumShead ());
			}
		}
		//2. BOMB
		if (munitionType == MunitionType.Bomb) {
			//TRIGGER WITH IMPACT
			if (armed && triggerMechanism == TriggerMechanism.ImpactForce && speed > speedThreshhold) {
				StartCoroutine (WaitForMomentumShead ());
			}
			//DESTROY IF IMPACT IS STRONG ENOUGH
			if (armed && speed > 5) {
				StartCoroutine (WaitForMomentumShead ());
			}
		}
		//3. MISSILE
		if (munitionType == MunitionType.Missile) {
			//TRIGGER WITH IMPACT
			if (armed && detonationType == DetonationType.Impact && speed > speedThreshhold) {
				StartCoroutine (WaitForMomentumShead ());
			}
			//DESTROY IF IMPACT IS STRONG ENOUGH
			if (armed && speed > 5) {
				StartCoroutine (WaitForMomentumShead ());
			}
		}
		//4. BULLET
		if (munitionType == MunitionType.Bullet) {
			if (ammunitionType == AmmunitionType.HEI) {
				Explode ();
			}
			if (ammunitionType == AmmunitionType.AP) {
				damageMulitplier = 5f;
			}
			if (ammunitionType == AmmunitionType.FMJ) {
				damageMulitplier = 1.56f;
			}
			if (ammunitionType == AmmunitionType.Tracer) {
				damageMulitplier = 1.65f;
			}
			//DISTANCE FALLOFF
			float distance = Vector3.Distance(this.transform.position,ejectionPoint);
			float mix = (((distance - 10f) * damageFactor) / 1990) + damageCompiler;
			float actualDamage = damage*damageMulitplier*(mix/100f);
			//APPLY
			if (actualDamage > 0) {
				col.collider.gameObject.SendMessage ("SilantroDamage", actualDamage, SendMessageOptions.DontRequireReceiver);
			}
			//EFFECT
			if (col.collider.tag == "Ground") {
				Instantiate(groundHit, col.contacts[0].point, Quaternion.FromToRotation(Vector3.up, col.contacts[0].normal));
			}
			if (col.collider.tag == "Wood") {
				Instantiate(woodHit, col.contacts[0].point, Quaternion.FromToRotation(Vector3.up, col.contacts[0].normal));
			}
			if (col.collider.tag == "Metal") {
				Instantiate(metalHit, col.contacts[0].point, Quaternion.FromToRotation(Vector3.up, col.contacts[0].normal));
			}
			//
			Destroy(gameObject);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SIMULATE ACTIVATION LATENCY
	IEnumerator WaitForMomentumShead()
	{
		yield return new WaitForSeconds (0.02f);
		Explode ();
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//WARHEAD ACTIVATION
	public void Explode()
	{
		if (explosionPrefab != null && !exploded) {
			GameObject explosion = Instantiate (explosionPrefab, this.transform.position, Quaternion.identity);
			explosion.SetActive (true);
			explosion.GetComponentInChildren<AudioSource> ().Play ();
			exploded = true;
		}
		Destroy (gameObject);
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		//BULLET
		if (munitionType == MunitionType.Bullet) {
			//SEND DATA
			CalculateData ();
			//
			bullettimer += Time.deltaTime;
			if (bullettimer > destroyTime && bulletfuseType == BulletFuzeType.M1032) {
				Destroy (gameObject);
			}
		}
		//OTHERS
		if (armed) {
			//CALCULATE PERFORMANCE
			speed = munition.velocity.magnitude;
			if (computer.Target != null) {
				distanceToTarget = Vector3.Distance (transform.position, computer.Target.position);
			}
			activeTime += Time.deltaTime;
			distanceTraveled = speed * activeTime;
			//
			//1.ROCKET
			if (munitionType == MunitionType.Rocket) {
				//TIMER FUZE
				if (fuzeType == FuzeType.MK352) {
					triggerTimer += Time.deltaTime;
					if (triggerTimer > timer) {
						Explode ();
					}
				}
				//PROXIMITY FUZE
				if (fuzeType == FuzeType.M423) {
					//ACTIVATE IF TARGET IS WITHIN RANGE
					if (computer.Target != null) {
						if (distanceToTarget < proximity) {
							Explode ();
						}
					} 
				}
				//RESET TIMER
				if (target) {
					selfDestructTimer = 0f;
				}
				////DESTROY IF TARGET IS NULL
				if (rocketType == RocketType.Guided && computer.seeking && computer.Target == null) {
					{	//DESTROY AFTER 5 Seconds IF TARGET IS NULL
						selfDestructTimer += Time.deltaTime;
						if (selfDestructTimer > 5) {
							Explode ();
						}
					}
				}
			}
			//2. BOMB
			if (munitionType == MunitionType.Bomb) {
				if (speed > 0 && falling) {
					fallTime += Time.deltaTime;
				}
				//CALCULATE DRAG
				if (falling) {
					//CALCULATE REYNOLDS NUMBER
					float viscocity = (0.00000009f*computer.ambientTemperature)+0.00001f;
					float reynolds = (computer.airDensity * speed * munitionLength) / viscocity;
					//CALCULATE CD
					if (reynolds < 1000000) {CDCoefficient = (0.1f * Mathf.Log10 (reynolds)) - 0.4f;} 
					else {CDCoefficient = 0.19f - (80000 / reynolds);}
					//DRAG
					dragForce = (0.5f * computer.airDensity * CDCoefficient * surfaceArea * speed*speed);
				}
			}
			//3. MISSILE
			if (munitionType == MunitionType.Missile) {
				//TIMER FUZE
				if (detonationType == DetonationType.Timer) {
					triggerTimer += Time.deltaTime;
					if (triggerTimer > timer) {
						Explode ();
					}
				}
				//PROXIMITY FUZE
				if (detonationType == DetonationType.Proximity) {
					//ACTIVATE IF TARGET IS WITHIN RANGE
					if (computer.Target != null) { 
						if (distanceToTarget < proximity) {
							Explode ();
						}
					} 
				}
				//RESET TIMER
				if (target) {
					selfDestructTimer = 0f;
				}
				////DESTROY IF TARGET IS NULL
				if (computer.seeking && computer.Target == null) {
					{	//DESTROY AFTER 5 Seconds IF TARGET IS NULL
						selfDestructTimer += Time.deltaTime;
						if (selfDestructTimer > 5) {
							Explode ();
						}
					}
				}
			}
			//
			if (munitionType != MunitionType.Bomb) {
				//DESTROY IF OUT OF RANGE
				if (distanceTraveled > maximumRange) {
					Explode ();
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//APPLY BOMB PHYSICS
	void FixedUpdate()
	{
		if (dragForce > 0 && munitionType == MunitionType.Bomb) {
			Vector3 force = -munition.velocity.normalized * dragForce;
			munition.AddForce (force,ForceMode.Force);
		}
		if (drag > 0 && munitionType == MunitionType.Bullet) {
			Vector3 Force = transform.forward * -drag;
			munition.AddForce (Force, ForceMode.Force);
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//PROCESS AFTER EFFECTS
	void LateUpdate()
	{
		//ROTATE MUNITION IN TRAVEL DIRECTION
		if (munition != null) {
			munition.transform.forward = Vector3.Slerp (munition.transform.forward, munition.velocity.normalized, Time.deltaTime);
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//BULLET DATA
	void CalculateData()
	{
		altitude = munition.gameObject.transform.position.y * 3.28f;
		float altiKmeter = altitude / 3280.84f;
		//AIR DENSITY
		float a =  0.0025f * Mathf.Pow(altiKmeter,2f);
		float b = 0.106f * altiKmeter;
		airDensity = a -b +1.2147f;
		currentVelocity = munition.velocity.magnitude;
		//BULLET DRAG
		drag = 0.5f*airDensity*dragCoefficient*area*currentVelocity*currentVelocity;
		//BULLET ENERGY
		currentEnergy = 0.5f*(mass/1000)*currentVelocity*currentVelocity;
	}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroMunition))]
public class MunitionEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroMunition munition;
	SerializedObject munitionObject;
	//
	private void OnEnable()
	{
		munition = (SilantroMunition)target;
		munitionObject = new SerializedObject (munition);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		munitionObject.UpdateIfRequiredOrScript();
		//
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Munition Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Identifier", munition.Identifier);
		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Munition Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		munition.munitionType = (SilantroMunition.MunitionType)EditorGUILayout.EnumPopup(" ",munition.munitionType);
		GUILayout.Space(2f);
		//1. ROCKET
		if (munition.munitionType == SilantroMunition.MunitionType.Rocket) {
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Rocket Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.rocketType = (SilantroMunition.RocketType)EditorGUILayout.EnumPopup("Mode",munition.rocketType);
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Warhead Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			munition.explosiveType = (SilantroMunition.ExplosiveType)EditorGUILayout.EnumPopup ("Explosive Type", munition.explosiveType);
			GUILayout.Space(3f);
			if (munition.explosiveType == SilantroMunition.ExplosiveType.Nitroglycerine)
			{
				munition.density = 1.60f;
				munition.detonationPressure = 254.6f;
				munition.energy = 6283f;
				munition.streamingVelocity = 1550f;
				munition.detonationVelocity = 7327f;

			}
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.PETN) 
			{
				munition.density = 1.60f;
				munition.detonationPressure = 254.6f;
				munition.energy = 5881f;
				munition.streamingVelocity = 1550f;
				munition.detonationVelocity = 7327f;
			}
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.RDX) 
			{
				munition.density = 1.762f;
				munition.detonationPressure = 337.9f;
				munition.energy = 5763f;
				munition.streamingVelocity = 2213f;
				munition.detonationVelocity = 8639f;
			} 
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.RDX_TNT)
			{
				munition.density = 1.743f;
				munition.detonationPressure =312.5f;
				munition.energy = 4985f;
				munition.streamingVelocity = 2173f;
				munition.detonationVelocity = 8252f;
			} 
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.TNT)
			{
				munition.density = 1.637f;
				munition.detonationPressure = 189.1f;
				munition.energy = 5810f;
				munition.streamingVelocity = 1664f;
				munition.detonationVelocity = 6942f;
			} 
			//
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Density",munition.density.ToString("0.000")+" gm/l");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Detonation Velocity",munition.detonationVelocity.ToString("0")+" m/s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Streaming Velocity",munition.streamingVelocity.ToString("0")+" m/s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Detonation Pressure",munition.detonationPressure.ToString("0")+" e+8 Mpa");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Energy",munition.energy.ToString("0")+" J/g");
			//
			GUILayout.Space(5f);
			munition.explosionPrefab = EditorGUILayout.ObjectField ("Explosion Prefab", munition.explosionPrefab, typeof(GameObject), true) as GameObject;
			//
			if (munition.explosionPrefab != null) {
				SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion> ();
				if (explosive != null) {
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Performance", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					GUILayout.Space(3f);
					EditorGUILayout.LabelField ("Explosive Force",explosive.explosionForce.ToString("0.0")+" N");
					GUILayout.Space(1f);
					EditorGUILayout.LabelField ("Explosive Radius",explosive.explosionRadius.ToString("0")+" m");
				}
			}
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Detonation System", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.fuzeType = (SilantroMunition.FuzeType)EditorGUILayout.EnumPopup ("Fuze Type", munition.fuzeType);
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Trigger System", munition.detonationMechanism);
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Armed", munition.armed.ToString ());
			GUILayout.Space(3f);
			//IMPACK
			if (munition.fuzeType == SilantroMunition.FuzeType.MK193Mod0 ) {
				munition.detonationMechanism = "Nose Impact";
				GUILayout.Space(2f);
			} 
			//TIME
			else if (munition.fuzeType == SilantroMunition.FuzeType.MK352) {
				munition.detonationMechanism = "Mechanical Timer";
				GUILayout.Space(2f);
				munition.timer = EditorGUILayout.FloatField ("Trigger Timer", munition.timer);
			} 
			else if (munition.fuzeType == SilantroMunition.FuzeType.M423) 
			{
				munition.detonationMechanism = "Proximity";
				GUILayout.Space(2f);
				munition.proximity = EditorGUILayout.FloatField ("Trigger Distance", munition.proximity);
				if (munition.target != null) {
					GUILayout.Space (2f);
					EditorGUILayout.LabelField ("Target", munition.target.name);
				}
			}
			GUILayout.Space(5f);
			munition.maximumRange = EditorGUILayout.FloatField ("Maximum Range", munition.maximumRange);

			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Rocket Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.munitionDiameter = EditorGUILayout.FloatField("Diameter",munition.munitionDiameter);
			GUILayout.Space(2f);
			munition.munitionLength = EditorGUILayout.FloatField("Length",munition.munitionLength);
			GUILayout.Space(3f);
			munition.munitionWeight = EditorGUILayout.FloatField("Weight",munition.munitionWeight);
			//
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Propulsion", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.motorEngine = EditorGUILayout.ObjectField ("Rocket Motor", munition.motorEngine, typeof(SilantroRocketMotor), true) as SilantroRocketMotor;
			GUILayout.Space(3f);
		}
		//2. BOMB
		if (munition.munitionType == SilantroMunition.MunitionType.Bomb) {
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Trigger Mechanism", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.triggerMechanism = (SilantroMunition.TriggerMechanism)EditorGUILayout.EnumPopup(" ",munition.triggerMechanism);
			if (munition.triggerMechanism == SilantroMunition.TriggerMechanism.ImpactForce) {
				GUILayout.Space(2f);
				munition.speedThreshhold = EditorGUILayout.FloatField ("Trigger Speed", munition.speedThreshhold);
			}
			if (munition.triggerMechanism == SilantroMunition.TriggerMechanism.Proximity) {
				GUILayout.Space(2f);
				if (munition.target != null) {
					EditorGUILayout.LabelField ("Target", munition.target.name);
				}
				GUILayout.Space(2f);
				munition.detonationDistance = EditorGUILayout.FloatField ("Trigger Distance", munition.detonationDistance);
			}
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Warhead Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			munition.fillingWeight = EditorGUILayout.FloatField ("Filling Weight", munition.fillingWeight);
			GUILayout.Space(3f);
			munition.explosiveType = (SilantroMunition.ExplosiveType)EditorGUILayout.EnumPopup ("Explosive Type", munition.explosiveType);
			GUILayout.Space(3f);
			if (munition.explosiveType == SilantroMunition.ExplosiveType.Nitroglycerine)
			{
				munition.density = 1.60f;
				munition.detonationPressure = 254.6f;
				munition.energy = 6283f;
				munition.streamingVelocity = 1550f;
				munition.detonationVelocity = 7327f;

			}
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.PETN) 
			{
				munition.density = 1.60f;
				munition.detonationPressure = 254.6f;
				munition.energy = 5881f;
				munition.streamingVelocity = 1550f;
				munition.detonationVelocity = 7327f;
			}
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.RDX) 
			{
				munition.density = 1.762f;
				munition.detonationPressure = 337.9f;
				munition.energy = 5763f;
				munition.streamingVelocity = 2213f;
				munition.detonationVelocity = 8639f;
			} 
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.RDX_TNT)
			{
				munition.density = 1.743f;
				munition.detonationPressure =312.5f;
				munition.energy = 4985f;
				munition.streamingVelocity = 2173f;
				munition.detonationVelocity = 8252f;
			} 
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.TNT)
			{
				munition.density = 1.637f;
				munition.detonationPressure = 189.1f;
				munition.energy = 5810f;
				munition.streamingVelocity = 1664f;
				munition.detonationVelocity = 6942f;
			} 
			//
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Density",munition.density.ToString("0.000")+" gm/l");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Detonation Velocity",munition.detonationVelocity.ToString("0")+" m/s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Streaming Velocity",munition.streamingVelocity.ToString("0")+" m/s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Detonation Pressure",munition.detonationPressure.ToString("0")+" e+8 Mpa");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Energy",munition.energy.ToString("0")+" J/g");
			//
			GUILayout.Space(5f);
			munition.explosionPrefab = EditorGUILayout.ObjectField ("Explosion Prefab", munition.explosionPrefab, typeof(GameObject), true) as GameObject;
			//
			if (munition.explosionPrefab != null) {
				SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion> ();
				if (explosive != null) {
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Performance", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					GUILayout.Space(3f);
					EditorGUILayout.LabelField ("Explosive Force",explosive.explosionForce.ToString("0.0")+" N");
					GUILayout.Space(1f);
					EditorGUILayout.LabelField ("Explosive Radius",explosive.explosionRadius.ToString("0")+" m");
				}
			}
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Bomb Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.munitionDiameter = EditorGUILayout.FloatField("Diameter",munition.munitionDiameter);
			GUILayout.Space(2f);
			munition.munitionLength = EditorGUILayout.FloatField("Length",munition.munitionLength);
			GUILayout.Space(3f);
			munition.munitionWeight = EditorGUILayout.FloatField("Weight",munition.munitionWeight);
			GUILayout.Space(2f);
			munition.percentageSkinning = EditorGUILayout.Slider ("Skinning", munition.percentageSkinning, 0, 100f);
			//
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Bomb Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Drop Speed", munition.speed.ToString ("0.0") + " m/s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Distance To Target", (munition.distanceToTarget/3.286f).ToString ("0.0") + " m");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Fall Time", munition.fallTime.ToString ("0.0") + " s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Drag Force", munition.dragForce.ToString ("0.0") + " N");
		}
		//3. MISSILE
		if (munition.munitionType == SilantroMunition.MunitionType.Missile) {
			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Missile Configuration", MessageType.None);
			GUI.color = backgroundColor;
			munition.missileType = (SilantroMunition.MissileType)EditorGUILayout.EnumPopup("Mode",munition.missileType);
			//
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Warhead Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			munition.explosiveType = (SilantroMunition.ExplosiveType)EditorGUILayout.EnumPopup ("Explosive Type", munition.explosiveType);
			GUILayout.Space(3f);
			if (munition.explosiveType == SilantroMunition.ExplosiveType.Nitroglycerine)
			{
				munition.density = 1.60f;
				munition.detonationPressure = 254.6f;
				munition.energy = 6283f;
				munition.streamingVelocity = 1550f;
				munition.detonationVelocity = 7327f;

			}
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.PETN) 
			{
				munition.density = 1.60f;
				munition.detonationPressure = 254.6f;
				munition.energy = 5881f;
				munition.streamingVelocity = 1550f;
				munition.detonationVelocity = 7327f;
			}
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.RDX) 
			{
				munition.density = 1.762f;
				munition.detonationPressure = 337.9f;
				munition.energy = 5763f;
				munition.streamingVelocity = 2213f;
				munition.detonationVelocity = 8639f;
			} 
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.RDX_TNT)
			{
				munition.density = 1.743f;
				munition.detonationPressure =312.5f;
				munition.energy = 4985f;
				munition.streamingVelocity = 2173f;
				munition.detonationVelocity = 8252f;
			} 
			else if (munition.explosiveType == SilantroMunition.ExplosiveType.TNT)
			{
				munition.density = 1.637f;
				munition.detonationPressure = 189.1f;
				munition.energy = 5810f;
				munition.streamingVelocity = 1664f;
				munition.detonationVelocity = 6942f;
			} 
			//
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Density",munition.density.ToString("0.000")+" gm/l");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Detonation Velocity",munition.detonationVelocity.ToString("0")+" m/s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Streaming Velocity",munition.streamingVelocity.ToString("0")+" m/s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Detonation Pressure",munition.detonationPressure.ToString("0")+" e+8 Mpa");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Energy",munition.energy.ToString("0")+" J/g");
			//
			GUILayout.Space(5f);
			munition.explosionPrefab = EditorGUILayout.ObjectField ("Explosion Prefab", munition.explosionPrefab, typeof(GameObject), true) as GameObject;
			//
			if (munition.explosionPrefab != null) {
				SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion> ();
				if (explosive != null) {
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Performance", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					GUILayout.Space(3f);
					EditorGUILayout.LabelField ("Explosive Force",explosive.explosionForce.ToString("0.0")+" N");
					GUILayout.Space(1f);
					EditorGUILayout.LabelField ("Explosive Radius",explosive.explosionRadius.ToString("0")+" m");
				}
			}
			//
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Detonation System", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Armed State", munition.armed.ToString ());
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Current Speed", munition.speed.ToString () + " m/s");
			GUILayout.Space(2f);
			munition.detonationType = (SilantroMunition.DetonationType)EditorGUILayout.EnumPopup("Detonation Type",munition.detonationType);
			//
			if (munition.detonationType == SilantroMunition.DetonationType.Proximity) {
				GUILayout.Space(2f);
				munition.proximity = EditorGUILayout.FloatField ("Trigger Distance", munition.proximity);
				//
				if (munition.computer != null && munition.computer.Target) {
					GUILayout.Space(2f);
					EditorGUILayout.LabelField ("Distance To Target", munition.distanceToTarget.ToString ("0.00") + " m");
				}
				if (munition.target != null) {
					EditorGUILayout.LabelField ("Current Target",munition.target.name);
				} else {
					EditorGUILayout.LabelField ("Current Target","Null");
				}
			}
			if (munition.detonationType == SilantroMunition.DetonationType.Timer) {
				GUILayout.Space(2f);
				munition.timer = EditorGUILayout.FloatField ("Trigger Timer", munition.timer);
			}
			if (munition.detonationType == SilantroMunition.DetonationType.Impact) {
				GUILayout.Space(2f);
				munition.speedThreshhold = EditorGUILayout.FloatField ("Trigger Speed", munition.speedThreshhold);
			}
			GUILayout.Space(5f);
			munition.maximumRange = EditorGUILayout.FloatField ("Maximum Range", munition.maximumRange);
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Missile Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.munitionDiameter = EditorGUILayout.FloatField("Diameter",munition.munitionDiameter);
			GUILayout.Space(2f);
			munition.munitionLength = EditorGUILayout.FloatField("Length",munition.munitionLength);
			GUILayout.Space(3f);
			munition.munitionWeight = EditorGUILayout.FloatField("Weight",munition.munitionWeight);
			//
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Propulsion", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			munition.motorEngine = EditorGUILayout.ObjectField ("Rocket Motor", munition.motorEngine, typeof(SilantroRocketMotor), true) as SilantroRocketMotor;
			GUILayout.Space(3f);
		}
		//4. BULLET
		if (munition.munitionType == SilantroMunition.MunitionType.Bullet) {
			GUILayout.Space(3f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Bullet Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			munition.ammunitionType = (SilantroMunition.AmmunitionType)EditorGUILayout.EnumPopup("Ammunition Type",munition.ammunitionType);
			//
			GUILayout.Space(3f);
			munition.ammunitionForm = (SilantroMunition.AmmunitionForm)EditorGUILayout.EnumPopup("Ammunition Form",munition.ammunitionForm);
			GUILayout.Space(3f);
			munition.bulletfuseType = (SilantroMunition.BulletFuzeType)EditorGUILayout.EnumPopup("Fuze Type",munition.bulletfuseType);
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("System Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			munition.mass = EditorGUILayout.FloatField ("Mass", munition.mass);
			GUILayout.Space(3f);
			munition.caseLength = EditorGUILayout.FloatField ("Case Length", munition.caseLength);
			GUILayout.Space(3f);
			munition.overallLength = EditorGUILayout.FloatField ("Overall Length", munition.overallLength);
			GUILayout.Space(3f);
			munition.diameter = EditorGUILayout.FloatField ("Diameter", munition.diameter);
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Performance Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			munition.ballisticVelocity = EditorGUILayout.FloatField ("Ballistic Velocity", munition.ballisticVelocity);
			GUILayout.Space(3f);
			munition.damage = EditorGUILayout.FloatField ("Damage", munition.damage);
			GUILayout.Space(3f);
			munition.destroyTime = EditorGUILayout.FloatField ("Destroy Time", munition.destroyTime);
			//
			if (munition.ammunitionType == SilantroMunition.AmmunitionType.HEI) {
				GUILayout.Space(10f);
				GUI.color = silantroColor;
				EditorGUILayout.HelpBox ("Explosive Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space(3f);
				munition.explosionPrefab = EditorGUILayout.ObjectField ("Explosion Prefab", munition.explosionPrefab, typeof(GameObject), true) as GameObject;
				//
				if (munition.explosionPrefab != null) {
					SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion> ();
					if (explosive != null) {
						GUI.color = Color.white;
						EditorGUILayout.HelpBox ("Performance", MessageType.None);
						GUI.color = backgroundColor;
						GUILayout.Space(2f);
						GUILayout.Space(3f);
						EditorGUILayout.LabelField ("Explosive Force",explosive.explosionForce.ToString("0.0")+" N");
						GUILayout.Space(1f);
						EditorGUILayout.LabelField ("Explosive Radius",explosive.explosionRadius.ToString("0")+" m");
					}
				}
			}

		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (munitionObject.targetObject, "Munition Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (munition);
			EditorSceneManager.MarkSceneDirty (munition.gameObject.scene);
		}
		munitionObject.ApplyModifiedProperties();
	}
}
#endif