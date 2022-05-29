//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroComputer : MonoBehaviour {
	//MODE
	public enum ComputerType
	{
		DataProcessing,Guidance
	}
	[HideInInspector]public ComputerType computerType = ComputerType.DataProcessing;
	//WEAPON TYPE
	[HideInInspector]public Rigidbody connectedSystem;
	//PERFORMANCE
	[HideInInspector]public float currentSpeed;
	[HideInInspector]public float machSpeed,soundSpeed;
	[HideInInspector]public float currentAltitude;
	[HideInInspector]public float airDensity;
	[HideInInspector]public float ambientTemperature;
	[HideInInspector]public float ambientPressure;float altimeter;float a;float b;
	//
	//GUIDANCE
	public enum WeaponType
	{
		Bomb,Missile,Rocket
	}
	[HideInInspector]public WeaponType weaponType = WeaponType.Missile;
	[HideInInspector]public SilantroMunition munition;
	public enum HomingType
	{
		ActiveRadar,SemiActiveRadar
	}
	[HideInInspector]public HomingType homingType = HomingType.SemiActiveRadar;
	////RADAR CONFIGURAATION
	[HideInInspector]public float range = 1000f;
	[HideInInspector]public float pingRate = 5;[HideInInspector]public float pingTime;
	[HideInInspector]public float actualPingRate;
	[HideInInspector]public bool seeking;
	[HideInInspector]public bool displayBounds;
	[HideInInspector]public SilantroRadar supportRadar;
	GameObject radarCore;
	//TARGET
	[HideInInspector]public Collider[] visibleObjects;
	[HideInInspector]public List<GameObject> processedTargets;
	[HideInInspector]public string targetID = "Unassigned";
	[HideInInspector]public Transform Target;
	const string headerContainer = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	const string qualifierContainer = "1234567890";
	string header;
	string qualifier;
	[HideInInspector]public string WeaponID = "Unassigned";
	//TRACKING
	Vector3 TargetMirror;
	Vector3 TargetPosition;
	Vector3 TargetPositionChange;
	Vector3 desiredRotation;
	[HideInInspector]public float navigationalConstant = 5;
	[HideInInspector]public float maxRotation = 50f;
	[HideInInspector]public bool active;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeComputer()
	{
		if (computerType == ComputerType.Guidance) {
			WeaponID = GenerateTrackID ();
			actualPingRate = (1f / pingRate);
			pingTime = 0.0f;
			//SETUP CORE
			radarCore = new GameObject (connectedSystem.transform.name+" Radar Core");
			radarCore.transform.parent = this.transform;
			radarCore.transform.localPosition = new Vector3 (0, 0, 0);
			//SETUP MISSILE
			if (weaponType == WeaponType.Missile) {
				SilantroTransponder ponder = GetComponent<SilantroTransponder> ();
				if (ponder) {
					ponder.TrackingID = WeaponID;
				}
			}
			//
			if (connectedSystem != null) {
				munition = connectedSystem.GetComponent<SilantroMunition> ();
			}
		}
	}



	// -------------------------------------------------------------//GENERATE TRACKING ID---------------------------------------------------------------------------------------------
	public string GenerateTrackID()
	{
		header = string.Empty;qualifier = string.Empty;
		for (int i = 0; i < 3; i++) {header += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length)];}
		for (int j = 0; j < 2; j++){qualifier += "1234567890"[UnityEngine.Random.Range(0, "1234567890".Length)];}
		return header + qualifier;
	}



	// -----------------------------------------------------------//DISPLAY CORE SEEKER-----------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if(displayBounds && computerType == ComputerType.Guidance && seeking){
			//
			if (radarCore && connectedSystem) {
				// Search View
				float radius = range;
				Vector3 center = radarCore.transform.position;
				Vector3 normal = connectedSystem.transform.up;
				Vector3 from = Quaternion.AngleAxis (120f, normal) * radarCore.transform.forward * -1;
				//
				Handles.color = new Color(Color.yellow.r,Color.yellow.b,Color.yellow.b,0.1f);
				Handles.DrawSolidArc(center,normal,from,120,radius);
			} else {
				// Search View
				float radius = range;
				Vector3 center = this.transform.position;
				if (connectedSystem) {
					Vector3 normal = connectedSystem.transform.up;
					Vector3 from = Quaternion.AngleAxis (120f, normal) * this.transform.forward * -1;
					//
					Handles.color = new Color (Color.yellow.r, Color.yellow.b, Color.yellow.b, 0.1f);
					Handles.DrawSolidArc (center, normal, from, 120, radius);
				}
			}
			//DRAW LINE OF SIGHT
			if (Target) {
				Handles.color = Color.red;
				Handles.DrawLine (transform.position, Target.position);
			}
		}
	}
	#endif











	// ----------------------------------------------------------------------//SEARCH SWEEP------------------------------------------------------------------------------------
	void Ping()
	{
		pingTime = 0.0f;
		//SEARCH
		visibleObjects = Physics.OverlapSphere (transform.position, range);
		processedTargets = new List<GameObject>();
		//FILTER OUT OBJECTS
		foreach (Collider filterCollider in visibleObjects) {
			//AVOID SELF DETECTION
			if (!filterCollider.transform.IsChildOf(munition.transform)) {
				//SEPARATE OBJECTS
				SilantroTransponder transponder;
				//CHECK PARENT BODY
				transponder = filterCollider.gameObject.GetComponent<SilantroTransponder> ();
				if (transponder == null) {
					//CHECK IN CHILDREN
					transponder = filterCollider.gameObject.GetComponentInChildren<SilantroTransponder> ();
				}
				if (transponder != null) {
					//REGISTER DETECTION
					processedTargets.Add (filterCollider.gameObject);
					//SET VARIABLES
					transponder.isTracked = true;
					transponder.TrackingID = WeaponID;
					//
					if (transponder.AssignedID == "Default") {
						transponder.AssignedID = GenerateTrackID ();
					}
				}
			}
		}
		//RESET
		Target = null;
		//
		//SWEEP MODES
		if (homingType == HomingType.ActiveRadar) {
			ActiveTargeting ();
		}
		if (homingType == HomingType.SemiActiveRadar) {
			SemiActiveTargeting ();
		}
		munition.target = Target;
	}




	// -----------------------------------------------------------------------------------//ACTIVE HOMING-----------------------------------------------------------------------
	void ActiveTargeting()
	{
		foreach (GameObject track in processedTargets) {
			if (track != null) {
				//FIND ATTACHED TRANSPONDER
				if (track.GetComponent<SilantroTransponder> ()) {
					SilantroTransponder ponder = track.GetComponent<SilantroTransponder> ();
					if (ponder != null) {
						if (ponder.AssignedID == targetID) {
							Target = track.transform;//Assign visible target
						}
					}
				}
				//
				//FIND ATTACHED TRANSPONDER in CHILDREN
				if (track.GetComponentInChildren<SilantroTransponder> ()) {
					SilantroTransponder ponder = track.GetComponentInChildren<SilantroTransponder> ();
					if (ponder != null) {
						if (ponder.AssignedID == targetID) {
							Target = track.transform;//Assign visible target
						}
					}
				}
			}
		}
		//
		homingType = HomingType.ActiveRadar;
	}





	// ---------------------------------------------------------------------SEMI ACTIVE HOMING-------------------------------------------------------------------------------------
	void SemiActiveTargeting()
	{
		//COLLECT TARGETS FROM PARENT RADAR
		processedTargets = supportRadar.processedObjects;
		//AUGUMENT WITH SUPPORT RADAR
		if (distanceToTarget > range) {
			if (supportRadar != null) {
				foreach (GameObject track in processedTargets) {
					//FIND ATTACHED TRANSPONDER
					if (track != null) {
						if (track.GetComponent<SilantroTransponder> ()) {
							SilantroTransponder ponder = track.GetComponent<SilantroTransponder> ();
							if (ponder != null) {
								if (ponder.AssignedID == targetID) {
									Target = track.transform;//Assign visible target
								}
							}
						}
						//
						//FIND ATTACHED TRANSPONDER in CHILDREN
						if (track.GetComponentInChildren<SilantroTransponder> ()) {
							SilantroTransponder ponder = track.GetComponentInChildren<SilantroTransponder> ();
							if (ponder != null) {
								if (ponder.AssignedID == targetID) {
									Target = track.transform;//Assign visible target
								}
							}
						}
					}
				}
			}
			//
			homingType = HomingType.SemiActiveRadar;
		} else {
			//USE RADAR ONCE TAREGT IS WITHIN RANGE COVER
			ActiveTargeting ();
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void SilantroNavigation()
	{
		//DETERMINE TARGET POSITION VARIATION
		TargetMirror = TargetPosition;
		TargetPosition = Target.position - transform.position;
		TargetPositionChange = TargetPosition - TargetMirror;
		TargetPositionChange = TargetPositionChange - Vector3.Project(TargetPositionChange, TargetPosition);
		desiredRotation = (Time.deltaTime * TargetPosition) + (TargetPositionChange * navigationalConstant);
		//FACE TARGET
		if (seeking && active) {
			Rotate (Quaternion.LookRotation (desiredRotation, transform.up));
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Rotate(Quaternion desiredRotation)
	{
		if (weaponType == WeaponType.Bomb && distanceToTarget > 500) {
			munition.transform.rotation = Quaternion.RotateTowards (munition.transform.rotation, desiredRotation, Time.deltaTime * maxRotation);
		} else {
			munition.transform.rotation = Quaternion.RotateTowards (munition.transform.rotation, desiredRotation, Time.deltaTime * maxRotation);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public float distanceToTarget;
	void Update() 
	{
		if (active && computerType == ComputerType.Guidance) {
			//TRACK TARGET
			if (Target != null) {
				distanceToTarget = Vector3.Distance (this.transform.position, Target.position);
			}
			//SEND OUT PING
			pingTime += Time.deltaTime;
			if (pingTime >= actualPingRate) {
				Ping ();
			}
			//SPIN CORE
			if (seeking) {
				radarCore.transform.Rotate (new Vector3 (0, pingRate * 100 * Time.deltaTime, 0));
			}
			//SEEK
			if (computerType == ComputerType.Guidance) {
				if (seeking && Target != null) {
					SilantroNavigation ();
				}
			}
		}
	}










	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		//SEND CALCULATION DATA
		if (connectedSystem != null) {CalculateData ();}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CalculateData()
	{
		//CALCULATE BASE DATA
		currentAltitude = connectedSystem.gameObject.transform.position.y * 3.28084f;//Convert altitude from meter to feet
		currentSpeed = connectedSystem.velocity.magnitude * 1.944f;//Speed in knots
		//CALCULATE DENSITY
		float kelvinTemperatrue;
		kelvinTemperatrue = ambientTemperature+273.15f;
		airDensity = (ambientPressure*1000f) / (287.05f * kelvinTemperatrue);
		//CALCULATE AMBIENT DATA
		float a1;float a2;
		a1 = 0.000000003f * currentAltitude * currentAltitude;
		a2 = 0.0021f * currentAltitude;ambientTemperature = a1 - a2 + 15.443f;
		//CALCULATE PRESSURE
		a = 0.0000004f * currentAltitude * currentAltitude;
		b = (0.0351f*currentAltitude);
		ambientPressure =( a - b + 1009.6f)/10f;
		//CALCULATE MACH SPEED
		soundSpeed = Mathf.Pow((1.2f*287f*(273.15f+ambientTemperature)),0.5f);
		machSpeed = (currentSpeed / 1.944f) / soundSpeed;
	}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroComputer))]
public class GuidanceEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		SilantroComputer computer = (SilantroComputer)target;
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Computer Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		computer.computerType = (SilantroComputer.ComputerType)EditorGUILayout.EnumPopup(" ",computer.computerType);
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Performance", MessageType.None);
		GUI.color = backgroundColor;
		//1. DATA PROCESSING COMPUTER ONLY
		if (computer.computerType == SilantroComputer.ComputerType.DataProcessing) {
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Mach Speed", computer.machSpeed.ToString("0.00"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Altitude", computer.currentAltitude.ToString("0.0") + " ft");
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Air Speed", (computer.currentSpeed/1.944f).ToString("0.00") + " m/s");
		}

		//2. GUIDANCE COMPUTER
		if (computer.computerType == SilantroComputer.ComputerType.Guidance) {
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Mach Speed", computer.machSpeed.ToString("0.00"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Altitude", computer.currentAltitude.ToString("0.0") + " ft");
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Sensor Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			computer.range = EditorGUILayout.FloatField ("Range", computer.range);
			GUILayout.Space(3f);
			computer.pingRate = EditorGUILayout.FloatField ("Ping Rate", computer.pingRate);
			GUILayout.Space(3f);
			computer.displayBounds = EditorGUILayout.Toggle ("Display Bounds", computer.displayBounds);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Weapon ID", computer.WeaponID);
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Target Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			computer.homingType = (SilantroComputer.HomingType)EditorGUILayout.EnumPopup("Homing Type",computer.homingType);
			if (computer.Target) {
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Target", computer.Target.name);
			} else {
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Target", "Unassigned");
			}
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Target ID", computer.targetID);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Seeking", computer.seeking.ToString ());
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Navigation Control", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			computer.maxRotation = EditorGUILayout.FloatField ("Rotation Rate", computer.maxRotation);
			GUILayout.Space(3f);
			computer.navigationalConstant = EditorGUILayout.FloatField ("Navigation Constant", computer.navigationalConstant);
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (serializedObject.targetObject, "Computer Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (computer);
			EditorSceneManager.MarkSceneDirty (computer.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif