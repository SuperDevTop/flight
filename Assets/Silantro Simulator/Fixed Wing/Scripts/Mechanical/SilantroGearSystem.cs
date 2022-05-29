using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
public class SilantroGearSystem : MonoBehaviour {
	//WHEEL CONFIGURATION
	[HideInInspector]public List<WheelSystem> wheelSystem = new List<WheelSystem>();
	[System.Serializable]
	public class WheelSystem
	{
		public string Identifier;
		public WheelCollider collider;
		public Transform wheelModel;
		//
		public enum WheelRotationAxis
		{
			X,Y,Z
		}
		public WheelRotationAxis rotationWheelAxis = WheelRotationAxis.X;
		//
		public bool steerable;
		public bool attachedMotor;
	}
	//STEER
	[HideInInspector]public float maximumSteerAngle =20f;
	[HideInInspector]public float maximumSteerSpeed = 20f;
	[HideInInspector]public float steerAngle;
	public enum RotationAxis
	{
		X,
		Y,
		Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	[HideInInspector]public Transform frontWheelAxle;
	Vector3 axisRotation;
	//GEAR CONFIGURATION
	public enum GearStructType
	{
		Fixed,
		Retractable
	}
	[HideInInspector]public GearStructType gearStructType = GearStructType.Fixed;
	public enum SystemType
	{
		Internal,
		Custom
	}
	[HideInInspector]public SystemType systemType = SystemType.Internal;
	[HideInInspector]public Animator gearAnimator;
	[HideInInspector]public float waitTime = 5f;
	public enum GearType
	{
		Combined,
		Seperate,
	}
	[HideInInspector]public GearType gearType;
	//
	//SOUND SYSTEM
	[HideInInspector]public bool playSound;
	[HideInInspector]public AudioClip groundRoll;
	[HideInInspector]public AudioSource soundSource;
	//BRAKE SYSTEM
	[HideInInspector]public float ActualBrakeTorque;
	[HideInInspector]public float brakeTorque= 5000f;
	[HideInInspector]public bool brakeActivated = true;
	public enum BrakeMode
	{
		Selective,Total
	}
	[HideInInspector]public BrakeMode brakeMode = BrakeMode.Selective;
	//CONTROL BOOLS
	[HideInInspector]public bool open;
	[HideInInspector]public bool isControllable = true;
	[HideInInspector]public bool gearOpened = true;
	[HideInInspector]public bool close ;
	[HideInInspector]public bool gearClosed = false;
	bool inTransition;
	//CONTROL VARIABLES
	[HideInInspector]float speed;
	[HideInInspector]public float structDrag;
	private Quaternion InitialModelRotation = Quaternion.identity;
	[HideInInspector]public float aircraftWeight;
	[HideInInspector]public SilantroHydraulicSystem gearHydraulics;
	[HideInInspector]public SilantroHydraulicSystem doorHydraulics;
	//
	//CONNECTIONS
	[HideInInspector]public Rigidbody aircraft;
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public SilantroAerofoil[] foils;
	[HideInInspector]public SilantroLight[] lights;
	float maximumRumbleVolume =1f;
	[HideInInspector]public float steerInput;
	[HideInInspector]public float brakeInput;







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	// BRAKE CONTROL
	public void ToggleBrake()
	{
		brakeActivated = !brakeActivated;
	}
	//ENGAGE BRAKE
	public void ActivateBrake()
	{
		brakeActivated = true;
	}
	//RELEASE BRAKE
	public void DeactivateBrake()
	{
		brakeActivated = false;
	}




	//GEAR CONTROL
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//OPEN GEAR
	public void LowerGear()
	{
		if (isControllable) {
			if (gearStructType == GearStructType.Retractable) {
				if (!gearOpened && !inTransition) {
					//INTERNAL MOVEMENT SYSTEM
					if (systemType == SystemType.Internal) {
						open = true;
					}
					//ANIMATION SYSTEM
					if (systemType == SystemType.Custom) {
						//PUT CUSTOM CODE HERE
					}
				}
			}
		}
	}
	//CLOSE GEAR
	public void RaiseGear()
	{
		if (isControllable) {
			if (gearStructType == GearStructType.Retractable) {
				if (!gearClosed && !inTransition) {
					//INTERNAL MOVEMENT SYSTEM
					if (systemType == SystemType.Internal) {
						close = true;
					}
					//ANIMATION SYSTEM
					if (systemType == SystemType.Custom) {
						//PUT CUSTOM CODE HERE e.g ANIMATION MOVEMENT
						gearAnimator.SetTrigger ("Close");
					}
				}
			}
		}
	}
	//TOGGLE GEAR
	public void ToggleGear()
	{
		if (isControllable) {
			if (gearStructType == GearStructType.Retractable) {
				if (!gearOpened && !inTransition) {
					//INTERNAL MOVEMENT SYSTEM
					if (systemType == SystemType.Internal) {
						open = true;
					}
					//ANIMATION SYSTEM
					if (systemType == SystemType.Custom) {
						//PUT CUSTOM CODE HERE
					}
				}
				if (!gearClosed && !inTransition) {
					//INTERNAL MOVEMENT SYSTEM
					if (systemType == SystemType.Internal) {
						close = true;
					}
					//ANIMATION SYSTEM
					if (systemType == SystemType.Custom) {
						//PUT CUSTOM CODE HERE e.g ANIMATION MOVEMENT
						gearAnimator.SetTrigger ("Close");
					}
				}
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//LIGHTS
	//TURN ON LANDING LIGHTS
	public void TurnOnLights()
	{
		foreach (SilantroLight light in lights) {
			if (light.lightType == SilantroLight.LightType.Landing) {light.TurnOn ();}
		}
	}
	//
	//TURN OFF LANDING LIGHTS
	public void TurnOffLights()
	{
		foreach (SilantroLight light in lights) {
			if (light.lightType == SilantroLight.LightType.Landing) {light.TurnOff ();}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (aircraft != null &&  controller != null) {
			allOk = true;
		} else if (aircraft == null) {
			Debug.LogError("Prerequisites not met on Gear "+transform.name + "....aircraft rigidbody not assigned");
			allOk = false;
		}
		else if (controller == null) {
			Debug.LogError("Prerequisites not met on Gear "+transform.name + "....aircraft controller not assigned");
			allOk = false;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeGear()
	{


		//CHECK SYSTEMS
		_checkPrerequisites();

		if(allOk){
		if (rotationAxis == RotationAxis.X) {
			axisRotation = new Vector3 (1, 0, 0);
		} else if (rotationAxis == RotationAxis.Y) {
			axisRotation = new Vector3 (0, 1, 0);
		} else if (rotationAxis == RotationAxis.Z) {
			axisRotation = new Vector3 (0, 0, 1);
		}
		axisRotation.Normalize();
		//SET INITIAL ROTATION
		if ( null != frontWheelAxle )
		{
			InitialModelRotation = frontWheelAxle.transform.localRotation;
		}
		//SETUP SOUND
		if (playSound && groundRoll != null) {
			GameObject source = new GameObject ("Sound Source");
			source.transform.parent = this.transform;
			source.transform.localPosition = new Vector3 (0, 0, 0);
			soundSource = source.AddComponent<AudioSource> ();
			//
			soundSource.clip = groundRoll;
			soundSource.loop = true;
			soundSource.Play ();
			soundSource.volume = 0f;
			soundSource.spatialBlend = 1f;
			soundSource.dopplerLevel = 0f;
			soundSource.rolloffMode = AudioRolloffMode.Custom;
			soundSource.maxDistance = 180f;
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CONTROL RUMBLE SOUND
	void PlayGroundRoll()
	{
		if (controller != null) {
			if (controller.currentSoundState == SilantroController.SoundState.Exterior) {maximumRumbleVolume = 1f;}
			else if (controller.currentSoundState == SilantroController.SoundState.Interior) {maximumRumbleVolume = 0.3f;}
		}
		if (wheelSystem [0].collider != null && wheelSystem [1].collider != null) {
			if (playSound && groundRoll != null && (wheelSystem [0].collider.isGrounded && wheelSystem [1].collider.isGrounded) && aircraft.velocity.magnitude > 3) {
				soundSource.volume = Mathf.Lerp (soundSource.volume, maximumRumbleVolume, 0.2f);
			} else {
				soundSource.volume = Mathf.Lerp (soundSource.volume, 0f, 0.2f);
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ROTATE WHEEL
	void RotateWheel(Transform wheel,WheelSystem system)
	{
		if (system.collider != null) {
			speed = system.collider.rpm;
		}
		if (wheel != null) {
			//
			if (system.rotationWheelAxis == WheelSystem.WheelRotationAxis.X) {
				wheel.Rotate (new Vector3 (speed * Time.deltaTime, 0, 0));
			}
			if (system.rotationWheelAxis == WheelSystem.WheelRotationAxis.Y) {
				wheel.Rotate (new Vector3 (0, speed * Time.deltaTime, 0));
			}
			if (system.rotationWheelAxis == WheelSystem.WheelRotationAxis.Z) {
				wheel.Rotate (new Vector3 (0, 0, speed * Time.deltaTime));
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	////ALLIGN WHEEL TO COLLIDER
	void WheelAllignment(WheelSystem system,Transform wheel)
	{
		if (wheel != null) {
			RaycastHit hit;
			WheelHit CorrespondingGroundHit;
			if (system.collider != null) {
				Vector3 ColliderCenterPoint = system.collider.transform.TransformPoint (system.collider.center);
				system.collider.GetGroundHit (out CorrespondingGroundHit);

				if (Physics.Raycast (ColliderCenterPoint, -system.collider.transform.up, out hit, (system.collider.suspensionDistance + system.collider.radius) * transform.localScale.y)) {
					wheel.position = hit.point + (system.collider.transform.up * system.collider.radius) * transform.localScale.y;
					float extension = (-system.collider.transform.InverseTransformPoint (CorrespondingGroundHit.point).y - system.collider.radius) / system.collider.suspensionDistance;
					Debug.DrawLine (CorrespondingGroundHit.point, CorrespondingGroundHit.point + system.collider.transform.up, extension <= 0.0 ? Color.magenta : Color.white);
					Debug.DrawLine (CorrespondingGroundHit.point, CorrespondingGroundHit.point - system.collider.transform.forward * CorrespondingGroundHit.forwardSlip * 2f, Color.green);
					Debug.DrawLine (CorrespondingGroundHit.point, CorrespondingGroundHit.point - system.collider.transform.right * CorrespondingGroundHit.sidewaysSlip * 2f, Color.red);
				} else {
					wheel.transform.position = Vector3.Lerp (wheel.transform.position, ColliderCenterPoint - (system.collider.transform.up * system.collider.suspensionDistance) * transform.localScale.y, Time.deltaTime * 10f);
				}
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//BRAKE
	void BrakingSystem(WheelSystem wheel)
	{
		if (brakeInput < 0) {
			brakeInput = 0;
		}
		//CALCULATE BRAKE LEVER TORQUE
		ActualBrakeTorque = brakeInput * brakeTorque;
		//PARKING BRAKE
		if (wheel != null && wheel.collider != null) {
			if (brakeActivated) {
				wheel.collider.brakeTorque = brakeTorque;
				wheel.collider.motorTorque = 0;
			} else {
				wheel.collider.motorTorque = 10f;
				wheel.collider.brakeTorque = 0f;
			}
		}
		//
		if (ActualBrakeTorque > 10f) {
			if(wheel.collider != null)
				wheel.collider.brakeTorque = ActualBrakeTorque;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (allOk) {
			//SEND BRAKING DATA
			foreach (WheelSystem system in wheelSystem) {
				if (brakeMode == BrakeMode.Selective) {
					if (system.attachedMotor) {
						BrakingSystem (system);
					}
				} else {
					BrakingSystem (system);
				}
			}
			//STEER WHEEL
			steerAngle = maximumSteerAngle * steerInput;
			//
			foreach (WheelSystem system in wheelSystem) {
				//SEND ROTATION DATA
				RotateWheel (system.wheelModel, system);
				//SEND ALIGNMENT DATA
				WheelAllignment (system, system.wheelModel);
				//
				if (system.steerable) {
					if (system.collider != null) {
						system.collider.steerAngle = steerAngle;
					}
					//ROTATE AXLE
					if (frontWheelAxle != null) {
						frontWheelAxle.transform.localRotation = InitialModelRotation;
						frontWheelAxle.transform.Rotate (axisRotation, steerAngle);
					}
				}
			}
			//PLAY RUMBLE SOUND
			if (playSound && soundSource != null) {
				PlayGroundRoll ();
			}
			//MOVE GEAR
			GearMovement ();
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL GEAR MOVEMENT
	void GearMovement()
	{
		if (isControllable) {
			//LOWER GEAR SYSTEM
			if (open && !gearOpened) {
				//OPEN GEAR
				if (gearType == GearType.Combined) {
					gearHydraulics.open = true;
					//------------------------------------------------------//PUT CUSTOM CODE HERE
					StartCoroutine (WaitToOpen ());
				} else if (gearType == GearType.Seperate) {
					doorHydraulics.open = true;
					StartCoroutine (WaitToOpen ());
					if (!inTransition) {
						StartCoroutine (WaitForDoorOpen ());
						inTransition = true;
					}
				}
			}
			//
			//RAISE GEAR SYSTEM
			if (close && !gearClosed) {
				//TURN OFF TCS MODE
				foreach (SilantroAerofoil foil in foils) {
					if (foil != null) {
						//foil.TCSControlActive = false;
					}
				}
				//CLOSE GEAR
				if (gearType == GearType.Combined) {
					gearHydraulics.close = true;
					StartCoroutine (WaitToClose ());
				} else if (gearType == GearType.Seperate) {
					gearHydraulics.close = true;
					if (!inTransition) {
						StartCoroutine (WaitForGearClose ());
						inTransition = true;
					}
				}
			}
		}
	}












	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RETURN SWITCHES
	IEnumerator WaitToClose()
	{
		TurnOffLights ();
		yield return new WaitForSeconds (gearHydraulics.closeTime);
		gearClosed = true;
		gearOpened = false;CloseGearSwitches ();
		inTransition = false;
	}
	IEnumerator WaitToOpen()
	{
		yield return new WaitForSeconds (gearHydraulics.openTime);
		gearClosed = false;TurnOnLights ();
		gearOpened = true;CloseGearSwitches ();
		inTransition = false;
	}
	//CLOSE GEAR
	IEnumerator WaitForGearClose()
	{
		yield return new WaitForSeconds (gearHydraulics.closeTime);
		doorHydraulics.close = true;
		StartCoroutine (WaitForDoorClose ());
	}
	//
	IEnumerator WaitForDoorClose()
	{
		TurnOffLights ();
		yield return new WaitForSeconds (doorHydraulics.closeTime);
		gearClosed = true;
		gearOpened = false;CloseGearSwitches ();
		inTransition = false;
	}
	//OPEN GEAR
	//
	IEnumerator WaitForDoorOpen()
	{
		yield return new WaitForSeconds (gearHydraulics.closeTime);
		gearHydraulics.open = true;
		StartCoroutine (WaitForGearOpen ());
	}
	//
	IEnumerator WaitForGearOpen()
	{
		yield return new WaitForSeconds (gearHydraulics.openTime);
		gearClosed = false;TurnOnLights ();
		gearOpened = true;CloseGearSwitches ();
		inTransition = false;
	}
	void CloseGearSwitches()
	{
		open =  false;
		close = false;
		//
	}
}










#if UNITY_EDITOR
[CustomEditor(typeof(SilantroGearSystem))]
public class GearEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	SilantroGearSystem gear;
	//
	int listSize;
	SerializedObject wheelObjet;
	SerializedProperty wheelList;
	//
	private static GUIContent deleteButton = new GUIContent("Remove","Delete");
	private static GUILayoutOption buttonWidth = GUILayout.Width (60f);
	//
	void OnEnable()
	{
		gear = (SilantroGearSystem)target;
		wheelObjet = new SerializedObject (gear);
		wheelList = wheelObjet.FindProperty ("wheelSystem");
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();wheelObjet.Update ();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Wheel Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		//
		if (wheelList != null) {
			EditorGUILayout.LabelField ("Wheel Count", wheelList.arraySize.ToString ());
		}
		GUILayout.Space (3f);
		if (GUILayout.Button ("Create Wheel")) {
			gear.wheelSystem.Add (new SilantroGearSystem.WheelSystem ());
		}
		if (wheelList != null) {
			GUILayout.Space (2f);
			//DISPLAY WHEEL ELEMENTS
			for (int i = 0; i < wheelList.arraySize; i++) {
				SerializedProperty reference = wheelList.GetArrayElementAtIndex (i);
				SerializedProperty Identifier = reference.FindPropertyRelative ("Identifier");
				SerializedProperty collider = reference.FindPropertyRelative ("collider");
				SerializedProperty wheelModel = reference.FindPropertyRelative ("wheelModel");
				//
				SerializedProperty rotationWheelAxis = reference.FindPropertyRelative ("rotationWheelAxis");
				//
				SerializedProperty steerable = reference.FindPropertyRelative ("steerable");
				SerializedProperty attachedMotor = reference.FindPropertyRelative ("attachedMotor");
				//
				GUI.color = new Color (1, 0.8f, 0);
				EditorGUILayout.HelpBox ("Wheel : " + (i + 1).ToString (), MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (3f);
				EditorGUILayout.PropertyField (Identifier);
				GUILayout.Space (2f);
				EditorGUILayout.PropertyField (collider);
				GUILayout.Space (1f);
				EditorGUILayout.PropertyField (wheelModel);
				GUILayout.Space (3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Operational Properties", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (3f);
				gear.wheelSystem [i].rotationWheelAxis = (SilantroGearSystem.WheelSystem.WheelRotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", gear.wheelSystem [i].rotationWheelAxis);
				GUILayout.Space (1f);
				EditorGUILayout.PropertyField (steerable);
				GUILayout.Space (1f);
				EditorGUILayout.PropertyField (attachedMotor);
				//
				GUILayout.Space (3f);
				if (GUILayout.Button (deleteButton, EditorStyles.miniButtonRight, buttonWidth)) {
					gear.wheelSystem.RemoveAt (i);
				}
				GUILayout.Space (5f);
			}
		}
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Steering Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Wheel Axle", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		gear.frontWheelAxle = EditorGUILayout.ObjectField ("Steer Axle", gear.frontWheelAxle, typeof(Transform), true) as Transform;
		GUILayout.Space (2f);
		gear.rotationAxis = (SilantroGearSystem.RotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", gear.rotationAxis);
		//
		GUILayout.Space (3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Steer Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		gear.maximumSteerSpeed =  EditorGUILayout.FloatField ("Maximum Steer Speed", gear.maximumSteerSpeed);
		GUILayout.Space (2f);
		gear.maximumSteerAngle = EditorGUILayout.FloatField ("Maximum Steer Angle", gear.maximumSteerAngle);
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Current Angle", gear.steerAngle.ToString ("0.0") + " °");
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Braking Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		gear.brakeMode = (SilantroGearSystem.BrakeMode)EditorGUILayout.EnumPopup("Brake Mode",gear.brakeMode);
		GUILayout.Space (3f);
		gear.brakeTorque = EditorGUILayout.FloatField ("Brake Torque", gear.brakeTorque);
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Brake Engaged", gear.brakeActivated.ToString ());
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Incremental Brake", (gear.brakeInput * 100f).ToString ("0.0") + " %");
		//
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		gear.playSound = EditorGUILayout.Toggle ("Play Sound", gear.playSound);
		if (gear.playSound) {
			GUILayout.Space (3f);
			gear.groundRoll = EditorGUILayout.ObjectField ("Ground Roll", gear.groundRoll, typeof(AudioClip), true) as AudioClip;
			GUILayout.Space (3f);
		}
		//
		GUILayout.Space (20f);
		gear.gearStructType = (SilantroGearSystem.GearStructType)EditorGUILayout.EnumPopup("Gear Type",gear.gearStructType);
		//
		if (gear.gearStructType == SilantroGearSystem.GearStructType.Fixed) {
			GUILayout.Space (5f);	
			gear.structDrag = EditorGUILayout.FloatField ("Struct Drag", gear.structDrag);
		}
		if (gear.gearStructType == SilantroGearSystem.GearStructType.Retractable) {
			GUILayout.Space (5f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Hydraulics Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (5f);
			//
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Gear Hydraulics Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			gear.systemType = (SilantroGearSystem.SystemType)EditorGUILayout.EnumPopup ("System Type", gear.systemType);
			GUILayout.Space (5f);
			//
			if (gear.systemType == SilantroGearSystem.SystemType.Internal) {
				//
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Internal Settings", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (3f);
				gear.gearType = (SilantroGearSystem.GearType)EditorGUILayout.EnumPopup ("Hydraulics Type", gear.gearType);
				GUILayout.Space (5f);
				//
				if (gear.gearType == SilantroGearSystem.GearType.Combined) {

					gear.gearHydraulics = EditorGUILayout.ObjectField ("Gear Actuator", gear.gearHydraulics, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
				}
				if (gear.gearType == SilantroGearSystem.GearType.Seperate) {
					gear.gearHydraulics = EditorGUILayout.ObjectField ("Gear Actuator", gear.gearHydraulics, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
					GUILayout.Space (4f);
					gear.doorHydraulics = EditorGUILayout.ObjectField ("Door Actuator", gear.doorHydraulics, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
				}
			}
			if (gear.systemType == SilantroGearSystem.SystemType.Custom) {
				EditorGUILayout.LabelField ("Put your custom display system here");
			}
		}
		//
		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("System Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		if (gear.gearOpened) {EditorGUILayout.LabelField ("Gear State", "Open");}
		if (gear.gearClosed) {EditorGUILayout.LabelField ("Gear State", "Closed");}
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (wheelObjet.targetObject, "Gear Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (gear);
			EditorSceneManager.MarkSceneDirty (gear.gameObject.scene);
		}
		wheelObjet.ApplyModifiedProperties();
	}
}
#endif