using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroArmament : MonoBehaviour {
	//WEAPONS
	//1. ROCKETS
	[HideInInspector]public List<SilantroMunition> rockets;
	[HideInInspector]public float rateOfFire = 3f;
	[HideInInspector]public float actualFireRate;
	float fireTimer;
	//2. MISSILES
	[HideInInspector]public List<SilantroMunition> missiles;
	[HideInInspector]public List<SilantroMunition> AAMS;//AIR TO AIR MISSILES
	[HideInInspector]public List<SilantroMunition> AGMS;//AIR TO GROUND MISSILES
	//3. BOMBS
	[HideInInspector]public List<SilantroMunition> bombs;//BOMBS
	[HideInInspector]public List<SilantroPylon> externalBombRacks;
	[HideInInspector]public List<SilantroPylon> internalBombRacks;
	[HideInInspector]public float minimumDropHeight = 300f;
	//4. GUNS
	[HideInInspector]public SilantroGun[] attachedGuns;
	//PROPERTIES
	[HideInInspector]public float weaponsLoad;//Total Weight
	//CONTROLS
	[HideInInspector]public SilantroRadar connectedRadar;
	[HideInInspector]public bool isControllable;
	bool canFire;
	bool canLaunch;
	//
	[HideInInspector]public AudioClip fireSound;
	[HideInInspector]public float fireVolume = 0.7f;
	AudioSource launcherSound;
	[HideInInspector]public SilantroPylon[] attachedPylons;

	//WEAPONS SELECTION
	[HideInInspector]public List<string> availableWeapons = new List<string>();
	[HideInInspector]public string currentWeapon;
	[HideInInspector]public int selectedWeapon;
	[HideInInspector]public bool setRocket;
	[HideInInspector]public bool setMissile;
	[HideInInspector]public bool setBomb;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//0. CHANGE SELECTED WEAPON
	public void ChangeWeapon()
	{
		selectedWeapon += 1;
		if (selectedWeapon > (availableWeapons.Count - 1)) {selectedWeapon = 0;}
		currentWeapon = availableWeapons [selectedWeapon];
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//0.5. SELECT WEAPON
	public void SelectWeapon(int weaponPoint)
	{
		currentWeapon = availableWeapons [weaponPoint];
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//1. ROCKET
	public void FireRocket()
	{
		if (isControllable) {
			if (rockets.Count > 0) {
				fireTimer = 0f;
				//SELECT RANDOM ROCKET
				int index = UnityEngine.Random.Range (0, rockets.Count);
				if (rockets [index] != null) {
					//FIRE GUIDED ROCKET
					if (rockets [index].rocketType == SilantroMunition.RocketType.Guided) {
						if (connectedRadar != null && connectedRadar.lockedTarget != null) {
							//COLLECT PROPERTIES
							Transform lockedTarget = connectedRadar.lockedTarget.body.transform;
							string lockedTargetID = connectedRadar.lockedTarget.trackingID;
							rockets [index].FireMunition (lockedTarget, lockedTargetID, 0);
						} else {
							rockets [index].ReleaseMunition ();
						}
					} 
					//LAUNCH UNGUIDED ROCKET
					if (rockets [index].rocketType == SilantroMunition.RocketType.Unguided) {
						rockets [index].ReleaseMunition ();
					}
					//PLAY SOUND
					launcherSound.PlayOneShot (fireSound);
					CountOrdnance ();
				}
			} else {
				Debug.Log ("Rocket System Offline");
			}
		}
		else {
			Debug.Log ("Weapon System Offline");
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//2. MISSILE
	public void FireMissile()
	{
		if (isControllable) {
			CountOrdnance ();
			//MAKE SURE MISSILES ARE AVAILABLE
			if (missiles.Count > 0) {
				//COLLECT TARGET DATA
				if (connectedRadar != null && connectedRadar.lockedTarget != null) {
				
					//1. PROCESS AIR TARGET
					if (connectedRadar.lockedTarget.form == "Aircraft") {
						//SELECT AAM
						if (AAMS.Count > 0) {
							//SELECT RANDOM AAM
							int index = UnityEngine.Random.Range (0, AAMS.Count);
							if (AAMS [index] != null && AAMS [index].connectedPylon != null && !AAMS [index].connectedPylon.engaged) {
								AAMS [index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;//SET TARGET
								AAMS [index].computer.supportRadar = connectedRadar;//SET SUPPORT RADAR ?? FOR SEMI_ACTIVE GUIDANCE
								AAMS [index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;//SET TARGET ID
								AAMS [index].connectedPylon.StartLaunchSequence ();//TRIGGER
							}
						} else if (AGMS.Count > 0) {
							Debug.Log ("No AAM Available so AGM has been launched");
							//SELECT RANDOM AGM
							int index = UnityEngine.Random.Range (0, AGMS.Count);
							if (AGMS [index] != null && AGMS [index].connectedPylon != null && !AGMS [index].connectedPylon.engaged) {
								AGMS [index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;
								AGMS [index].computer.supportRadar = connectedRadar;
								AGMS [index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;
								AGMS [index].connectedPylon.StartLaunchSequence ();
							}
						}
						//PLAY SOUND
						launcherSound.PlayOneShot (fireSound);
						CountOrdnance ();
					}

				//2. PROCESS GROUND TARGET
					else if (connectedRadar.lockedTarget.form == "SAM Battery" || connectedRadar.lockedTarget.form == "Truck" || connectedRadar.lockedTarget.form == "Tank") {
						//SELECT AGM
						if (AGMS.Count > 0) {
							//SELECT RANDOM AGM
							int index = UnityEngine.Random.Range (0, AGMS.Count);
							if (AGMS [index] != null && AGMS [index].connectedPylon != null && !AGMS [index].connectedPylon.engaged) {
								AGMS [index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;
								AGMS [index].computer.supportRadar = connectedRadar;
								AGMS [index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;
								AGMS [index].connectedPylon.StartLaunchSequence ();
							}
						} else if (AAMS.Count > 0) {
							Debug.Log ("No AGM Available so AAM has been launched");
							//SELECT RANDOM AAM
							int index = UnityEngine.Random.Range (0, AAMS.Count);
							if (AAMS [index] != null && AAMS [index].connectedPylon != null && !AAMS [index].connectedPylon.engaged) {
								AAMS [index].connectedPylon.target = connectedRadar.lockedTarget.body.transform;
								AAMS [index].computer.supportRadar = connectedRadar;
								AAMS [index].connectedPylon.targetID = connectedRadar.lockedTarget.trackingID;
								AAMS [index].connectedPylon.StartLaunchSequence ();
							}
						}
						//PLAY SOUND
						launcherSound.PlayOneShot (fireSound);
						CountOrdnance ();
					}
				//3.
				else {
						Debug.Log ("Locked Target form is either null or not supported. You can add a new definition in the Armaments code");
					}
				} 
			//NO LOCKED TARGET
			else {
					Debug.Log ("Locked Target/Radar Unavailable");
				}
			} 
		//NO MISSILES
		else {
				Debug.Log ("Missile System Offline");
			}
		}
		else {
			Debug.Log ("Weapon System Offline");
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//3. BOMB
	public void DropBomb()
	{
		if (isControllable) {
			if (connectedRadar.connectedAircraft.transform.position.y > minimumDropHeight) {
				if (bombs.Count > 0) {
					if (externalBombRacks.Count > 0) {
						//SELECT RANDOM BOMB FROM EXTERNAL RACK
						if (externalBombRacks [0].bombs.Count > 0 && !externalBombRacks [0].engaged) {
							externalBombRacks [0].StartDropSequence ();
						}
					} else if (internalBombRacks.Count > 0) {
						//SELECT RANDOM BOMB FROM INTERNAL RACK
						if (internalBombRacks [0].bombs.Count > 0 && !internalBombRacks [0].engaged) {
							internalBombRacks [0].StartDropSequence ();
						}
					}
					//
					CountOrdnance ();
				} 
		//NO BOMBS
		else {
					Debug.Log ("Bomb System Offline");
				}
			} 
		//TOO LOW
		else {
				Debug.Log ("Aircraft too low to drop bombs");
			}
		}
		else {
			Debug.Log ("Weapon System Offline");
		}
	}

	//4. GUNS
	public void FireGuns()
	{
		if (isControllable) {
			if (attachedGuns.Length > 0) {
				foreach (SilantroGun gun in attachedGuns) {
					gun.FireGun ();
				}
			}
			//NO GUNS
			else {
				Debug.Log ("Gun System Offline");
			}
		}
		//NOT CONTROLLABLE
		else {
			Debug.Log ("Weapon System Offline");
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//INITIALIZE Armament
	public void InitializeWeapons()
	{
		//0.
		if (connectedRadar == null) {
			Debug.Log ("No Radar is connected to the aircraft, some functionalities will not work");
		}
		//1. SOUND
		GameObject speaker = new GameObject("Launch Sound Point");
		speaker.transform.parent = this.transform;
		speaker.transform.localPosition = new Vector3 (0, 0, 0);
		launcherSound = speaker.AddComponent<AudioSource> ();
		//
		launcherSound.loop = false;
		launcherSound.dopplerLevel = 0f;
		launcherSound.spatialBlend = 1f;
		launcherSound.rolloffMode = AudioRolloffMode.Custom;
		launcherSound.maxDistance = 100f;
		launcherSound.volume = fireVolume;
		//ROCKET FIRE RATE
		if (rateOfFire != 0) {actualFireRate = 1.0f / rateOfFire;} 
		else {actualFireRate = 0.01f;}
		fireTimer = 0.0f;
		//
		Rigidbody connectedAircraft = connectedRadar.connectedAircraft.GetComponent<Rigidbody>();
		//
		SilantroMunition[] munitions = this.gameObject.GetComponentsInChildren<SilantroMunition> ();
		foreach (SilantroMunition munition in munitions) {
			if(connectedAircraft != null){munition.connectedAircraft = connectedAircraft;}
			munition.InitializeMunition ();
		}
		//SETUP GUNS
		attachedGuns = GetComponentsInChildren<SilantroGun>();
		foreach (SilantroGun gun in attachedGuns) {
			gun.InitializeGun ();
			if (connectedRadar != null) {
				gun.connectedAircraft = connectedRadar.connectedAircraft.aircraft;
			}
		}
		//REFRESH PYLON INFO
		attachedPylons = connectedRadar.connectedAircraft.gameObject.GetComponentsInChildren<SilantroPylon> ();
		foreach (SilantroPylon pylon in attachedPylons) {
			pylon.InitializePylon ();
			pylon.manager = this.gameObject.GetComponent<SilantroArmament> ();
		}
		//INITIAL COUNT
		CountOrdnance();
		//SET AVAILABLE WEAPONS
		if (attachedGuns.Length > 0) {availableWeapons.Add ("Gun");}
		if (bombs.Count > 0) {availableWeapons.Add ("Bomb");}
		if (missiles.Count > 0) {availableWeapons.Add ("Missile");}
		if (rockets.Count > 0) {availableWeapons.Add ("Rocket");}
		//SELECT DEFAULT WEAPON
		selectedWeapon = 0;
		if (availableWeapons.Count > 0) {
			currentWeapon = availableWeapons [selectedWeapon];
		} else {
			Debug.Log ("No weapons attached to Fire-Control System!!");
		}
	}




	[HideInInspector]public SilantroMunition[] munitions;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//COUNT ROCKETS
	public void CountOrdnance()
	{
		munitions = this.gameObject.GetComponentsInChildren<SilantroMunition> ();
		//RESET BUCKETS
		weaponsLoad = 0f;rockets = new List<SilantroMunition> ();
		missiles = new List<SilantroMunition> ();
		bombs = new List<SilantroMunition> ();
		AAMS = new List<SilantroMunition> ();AGMS = new List<SilantroMunition> ();
		//COUNT
		foreach (SilantroMunition munition in munitions) {
			weaponsLoad += munition.munitionWeight;
			//SEPARATE ROCKETS
			if (munition.munitionType == SilantroMunition.MunitionType.Rocket) {
				rockets.Add (munition);
			}
			//SEPARATE MISSILE
			if (munition.munitionType == SilantroMunition.MunitionType.Missile) {
				//CHECK IF LAUNCH SEQUENCE HAS BEEN INITIALIZED
				if(munition.connectedPylon != null && munition.connectedPylon.engaged != true){
				missiles.Add (munition);
				//BY TYPE
					if (munition.missileType == SilantroMunition.MissileType.AAM) {AAMS.Add (munition);}
					if (munition.missileType == SilantroMunition.MissileType.ASM) {AGMS.Add (munition);}
				}
			}
			//SEPARATE BOMB
			if (munition.munitionType == SilantroMunition.MunitionType.Bomb) {
				bombs.Add (munition);
			}
		}
		//REFRESH BOMB PYLONS
		externalBombRacks = new List<SilantroPylon>();internalBombRacks = new List<SilantroPylon>();
		attachedPylons = GetComponentsInChildren<SilantroPylon> ();
		foreach (SilantroPylon pylon in attachedPylons) {
			if (pylon.bombs.Count > 0) {
				//1. EXTERNAL PYLON
				if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb && pylon.pylonPosition == SilantroPylon.PylonPosition.External) {
					externalBombRacks.Add (pylon);
				}
				//2. INTERNAL BAY
				if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb && pylon.pylonPosition == SilantroPylon.PylonPosition.Internal) {
					internalBombRacks.Add (pylon);
				}
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//REFRESH
	void Update()
	{
		fireTimer += Time.deltaTime;
		//
		if (isControllable && bombs.Count > 0) {
			//PREDICT BOMB IMPACT POSITION
			if (connectedRadar && connectedRadar.connectedAircraft) {
				CalculateImpactPosition ();
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public Vector3 impactPosition;
	//CALCULATE PROBABLE BOMB IMPACT POSITION
	void CalculateImpactPosition()
	{
		float altitude = this.transform.position.y;
		float time = Mathf.Pow (((altitude*2f)/ 9.8f), 0.5f);
		//
		float speed = connectedRadar.connectedAircraft.GetComponent<Rigidbody>().velocity.magnitude;
		float distanceInitial = time * speed;
		Vector3 direction = transform.forward;
		Vector3 finalDirection = direction.normalized * distanceInitial;
		Vector3 finalPosition = this.transform.position + finalDirection;
		//
		impactPosition = new Vector3(finalPosition.x,0,finalPosition.z);
		//
		Debug.DrawLine(this.transform.position,impactPosition);
//		if (actualtarget != null) {
//			actualtarget.transform.position = impactPosition + new Vector3(0,2,0);
//		}
	}
}



//
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroArmament))]
public class ArmamentEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroArmament carrier;
	SerializedObject carrierObject;
	//
	private void OnEnable()
	{
		carrier = (SilantroArmament)target;
		carrierObject = new SerializedObject (carrier);
	}
	//
	public override void OnInspectorGUI ()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("System Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Payload", carrier.weaponsLoad.ToString ("0.0") + " kg");
		GUILayout.Space (8f);
		//
		if (!carrier.setMissile) {
			if (GUILayout.Button ("Configure Missile")) {
				carrier.setMissile = true;
			}
		}
		if (carrier.setMissile) {
			if (GUILayout.Button ("Hide Missile Board")) {
				carrier.setMissile = false;
			}
			GUILayout.Space (8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Missile Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Missile Count", carrier.missiles.Count.ToString ());
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("AAM Count", carrier.AAMS.Count.ToString ());
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("AGM Count", carrier.AGMS.Count.ToString ());
			//
			GUILayout.Space (8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Launcher Sound", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			carrier.fireSound = EditorGUILayout.ObjectField ("Fire Sound", carrier.fireSound, typeof(AudioClip), true) as AudioClip;
			GUILayout.Space (2f);
			carrier.fireVolume = EditorGUILayout.FloatField ("Fire Volume", carrier.fireVolume);
		}

		//2. ROCKETS
		GUILayout.Space (10f);
		if (!carrier.setRocket) {
			if (GUILayout.Button ("Configure Rockets")) {
				carrier.setRocket = true;
			}
		}
		if (carrier.setRocket) {
			if (GUILayout.Button ("Hide Rocket Board")) {
				carrier.setRocket = false;
			}
			GUILayout.Space (8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Rocket Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Rocket Count", carrier.rockets.Count.ToString ());
			GUILayout.Space (2f);
			carrier.rateOfFire = EditorGUILayout.FloatField ("Rate of Fire", carrier.rateOfFire);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("AROF", carrier.actualFireRate.ToString ("0.00"));
		}

		//2. BOMB
		GUILayout.Space (10f);
		if (!carrier.setBomb) {
			if (GUILayout.Button ("Configure Bombs")) {
				carrier.setBomb = true;
			}
		}
		if (carrier.setBomb) {
			if (GUILayout.Button ("Hide Bomb Board")) {
				carrier.setBomb = false;
			}
			GUILayout.Space (8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Bomb Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			carrier.minimumDropHeight = EditorGUILayout.FloatField ("Minimum Drop Height", carrier.minimumDropHeight);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Bomb Count", carrier.bombs.Count.ToString ());
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("External Racks", carrier.externalBombRacks.Count.ToString ());
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Internal Racks", carrier.internalBombRacks.Count.ToString ());
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (carrierObject.targetObject, "Armament Change");}
		//SAVE
		if (GUI.changed) {
			EditorUtility.SetDirty (carrier);
			EditorSceneManager.MarkSceneDirty (carrier.gameObject.scene);
		}
		carrierObject.ApplyModifiedProperties();
	}
}
#endif