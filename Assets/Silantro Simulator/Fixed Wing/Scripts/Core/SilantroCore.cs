using System.Collections;
using System.Collections.Generic;
using UnityEngine;using System;using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroCore : MonoBehaviour {
	//CONNECTIONS
	[HideInInspector]public Rigidbody aircraft;
	public enum AircraftType{Jet,Propeller,Rocket,Glider}

	[HideInInspector]public AircraftType aircraftType;
	//FLIGHT DATA
	[HideInInspector]public float currentSpeed;
	[HideInInspector]public float groundSpeed;
	[HideInInspector]public float machSpeed;
	[HideInInspector]public float currentAltitude;
	[HideInInspector]public float headingDirection,verticalSpeed,airDensity,viscocity;
	[HideInInspector]public float ambientTemperature,ambientPressure,bankAngle;

	public enum SpeedType{Supersonic,Subsonic}
	[HideInInspector]public SpeedType speedType = SpeedType.Subsonic;

	//CALCULATION VALUES
	float baseDensity;
	float altimeter;float a;float b;
	float XbankAngle;
	float YbankAngle;
	float ZbankAngle;
	[HideInInspector]public float xforce;float yforce;[HideInInspector]public float zforce;
	[HideInInspector]public float gForce;
	//CONTROL BOOLS
	[HideInInspector]public bool ambientSimulation;
	[HideInInspector]public SilantroSapphire weatherController;

	//SUPERSONIC STUFF
	[HideInInspector]public ParticleSystem sonicCone;[HideInInspector]public AudioClip sonicBoom;
	ParticleSystem.EmissionModule sonicModule;
	float sonicEmission = 50f;
	bool sonicing,played = false;
	[HideInInspector]public AudioSource boom;
	[HideInInspector]public AudioSource warner;

	//CENTER OF MASS STUFF
	//1. STATE
	public enum SystemType{Basic,Advanced}
	[HideInInspector]public SystemType functionality = SystemType.Basic;
	//
	[HideInInspector]public Transform emptyCenterOfMass;
	[HideInInspector]public Transform currentCOM;
	[HideInInspector]public Vector3 baseCenter,centerOfMass;

	//2. LOADS
	[HideInInspector]public float emptyWeight;
	[HideInInspector]public float munitionLoad;
	[HideInInspector]public float componentLoad;
	[HideInInspector]public float fuelLoad;
	[HideInInspector]public float totalWeight;

	//3. SYSTEMS
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public SilantroArmament armament;
	[HideInInspector]SilantroFuelTank[] tanks;
	[HideInInspector]SilantroMunition[] munitions;
	[HideInInspector]SilantroPayload[] payloadElements;
	[HideInInspector]public List<SilantroAerofoil> wings;
	[HideInInspector]public List<float> stallAngles;
	[HideInInspector]public List<float> AOAs;
	[HideInInspector]public List<float> stallSpeeds;
	[HideInInspector]public float minimumStallAngle,stallThreshold,speedThreshold = 30f,stallLimitFactor = 2;
	[HideInInspector]public bool stalling,stallWarner;
	[HideInInspector]public AudioClip stallClip;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeCore () {
		//CALCULATE BASE DENSITY
		currentAltitude = aircraft.gameObject.transform.position.y * 3.28f;
		altimeter = currentAltitude / 3280.84f;
		a =  0.0025f * Mathf.Pow(altimeter,2f);b = 0.106f * altimeter;
		airDensity = a -b +1.2147f;
		baseDensity = airDensity;
		//
		GameObject SoundPoint = new GameObject();SoundPoint.transform.parent = this.transform;SoundPoint.transform.localPosition = new Vector3 (0, 0, 0);
		SoundPoint.name = "Sound Point";

		if (sonicCone != null && speedType == SpeedType.Supersonic) {
			sonicModule = sonicCone.emission;
			sonicModule.rateOverTime = 0f;
			//
			boom = SoundPoint.AddComponent<AudioSource>();boom.clip = sonicBoom;boom.loop = false;
			boom.dopplerLevel = 0f;boom.spatialBlend = 1f;boom.rolloffMode = AudioRolloffMode.Custom;boom.maxDistance = 650f;//Actual Sound Range
		}

		//SETUP STALL WARNER
		if (stallWarner && stallClip != null) {
			warner = SoundPoint.AddComponent<AudioSource>();warner.clip = stallClip;warner.loop = true;
			warner.dopplerLevel = 0f;warner.spatialBlend = 1f;warner.rolloffMode = AudioRolloffMode.Custom;warner.maxDistance = 650f;//Actual Sound Range
		}
		emptyWeight = controller.emptyWeight;

		//SETUP ADVANCED COM
		if (functionality == SystemType.Advanced) {
			GameObject core = new GameObject ("Current COM");
			core.transform.parent = aircraft.transform;
			core.transform.localPosition = new Vector3 (0, 0, 0);
			currentCOM = core.transform;
		}

		//INITIAL CALCULATION
		CalculateCenter ();
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public float speedFactor,sonicInput;
	void Update()
	{
		CalculateCenter ();
		//SONIC CONE EFFECT

		if (aircraftType == AircraftType.Jet && speedType == SpeedType.Supersonic && sonicCone != null) {
			speedFactor = machSpeed * sonicEmission * UnityEngine.Random.Range (0f, 1f) * UnityEngine.Random.Range (0f, 1f);
			if (machSpeed > 0.95f) {sonicInput = speedFactor;} 
			else {sonicInput = Mathf.Lerp (sonicInput, 0, 0.8f * Time.deltaTime);}
			//APPLY
			sonicModule.rateOverTime = sonicInput;
		}

		//COLLECT STALL DATA
		stallAngles = new List<float>();AOAs = new List<float>();stallSpeeds = new List<float>();
		if (wings.Count > 0) {
			foreach (SilantroAerofoil foil in wings) {
				if (foil.rootAirfoil != null && foil.tipAirfoil != null) {
					stallAngles.Add (foil.rootAirfoil.stallAngle);
					stallAngles.Add (foil.tipAirfoil.stallAngle);
					float stallSpeed = EstimateStallSpeed (foil.foilArea, Mathf.Min (foil.rootAirfoil.maxCl, foil.tipAirfoil.maxCl));
					stallSpeeds.Add (stallSpeed);
				}
				AOAs.Add (foil.angleOfAttack);
			}
		}

		if (stallAngles.Count > 0) {
			//SEPARATE
			stallThreshold = AOAs.Max ();
			minimumStallAngle = stallAngles.Min ();
			//CHECK
			if (stallThreshold >= (Mathf.Abs (minimumStallAngle) - stallLimitFactor) && currentSpeed > speedThreshold) {stalling = true;} else {stalling = false;}
			//PLAY WARNING
			if (warner != null) {
				if (stalling && !warner.isPlaying) {
					warner.Play ();
				}
				if (!stalling && warner.isPlaying) {
					warner.Stop ();
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimateStallSpeed(float sectionArea,float sectionClMax)
	{
		float upper = 2f*controller.currentWeight * 9.81f;
		float lower = airDensity * sectionArea * sectionClMax;
		float stallSpeed = Mathf.Sqrt ((upper / lower));
		return stallSpeed;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SET AIRCRAFT CENTER OF GRAVITY
	void CalculateCenter()
	{
		if (aircraft != null) {
			//1. BASIC CALCULATION
			if (functionality == SystemType.Basic) {
				aircraft.centerOfMass = this.transform.localPosition;
			}
			//2, ADVANCED
			if (functionality == SystemType.Advanced) {
				//COLLECT COMPONENTS
				tanks = aircraft.GetComponentsInChildren<SilantroFuelTank>();
				payloadElements = aircraft.GetComponentsInChildren<SilantroPayload> ();
				//WEAPONS
				if (armament != null) {munitions = armament.munitions;}
				totalWeight = controller.emptyWeight;
				//RESET VARIABLES
				fuelLoad = munitionLoad = componentLoad = 0f;
				//CALCULATE WEIGHT EFFECT
				//1. EMPTY AIRCRAFT
				baseCenter = emptyCenterOfMass.position;
				centerOfMass = transform.TransformDirection(baseCenter) * emptyWeight;

				//2. FUEL
				if (tanks.Length > 0) {
					foreach (SilantroFuelTank tank in tanks) {
						Vector3 tankPosition = tank.transform.position;
						float tankFuel = tank.CurrentAmount;
						//CALCULATE EFFECT
						fuelLoad += tankFuel;totalWeight += tankFuel;
						centerOfMass += aircraft.transform.TransformDirection(tankPosition)*tankFuel;
					}
				}
					

				//4. PAYLOAD
				if (payloadElements != null && payloadElements.Length > 0) {
					foreach (SilantroPayload element in payloadElements) {
						Vector3 elementPosition = element.transform.position;
						float elementWeight = element.weight;
						//CALCULATE EFFECT
						componentLoad += elementWeight;
						totalWeight += elementWeight;
						centerOfMass += aircraft.transform.TransformDirection(elementPosition)*elementWeight;
					}
				}

				//5. MUNITIONS
				if (munitions != null && munitions.Length > 0) {
					foreach (SilantroMunition munition in munitions) {
						if (munition != null) {
							Vector3 munitionPosition = munition.transform.position;
							float munitionWeight = munition.munitionWeight;
							//CALCULATE EFFECT
							munitionLoad += munitionWeight;
							totalWeight += munitionWeight;
							centerOfMass += aircraft.transform.TransformDirection (munitionPosition) * munitionWeight;
						}
					}
				}

				if (totalWeight > 0) {centerOfMass /= (totalWeight);} 
				else {centerOfMass /= controller.emptyWeight;}

				currentCOM.position = transform.transform.InverseTransformDirection(centerOfMass);
				//SET
				aircraft.centerOfMass = currentCOM.localPosition;
			}
		}
	}



	////DRAW COG POSITION
	public void OnDrawGizmos() 
	{
		Gizmos.color = Color.blue;
		if (currentCOM != null) {
			Gizmos.DrawSphere (currentCOM.position, 0.2f);
		} else {
			if (emptyCenterOfMass) {
				Gizmos.DrawSphere (emptyCenterOfMass.position, 0.2f);
			}
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		
		//COLLECT AIRCRAFT DATA
		if (aircraft != null) {
			currentAltitude = aircraft.gameObject.transform.position.y * 3.28084f;//Convert altitude from meter to feet
			currentSpeed = aircraft.velocity.magnitude * 1.944f;//Speed in knots
			verticalSpeed = aircraft.velocity.y * 3.28084f * 60f;//Vertical speed in ft/min
			headingDirection = aircraft.transform.eulerAngles.y;
			//SEND DATA
			CalculateData(currentAltitude);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE AIRCRAFT PERFORMANCE DATA
	void CalculateData(float altitude)
	{
		float kelvinTemperatrue;
		kelvinTemperatrue = ambientTemperature+273.15f;
		airDensity = (ambientPressure*1000f) / (287.05f * kelvinTemperatrue);
		viscocity = 1.458f / (1000000) * Mathf.Pow (kelvinTemperatrue, 1.5f) * (1 / (kelvinTemperatrue + 110.4f));
		float a1;float a2;float soundSpeed;float densityRatio;float m;
		//CALCULATE TEMPERATURE
		a1 = 0.000000003f * altitude * altitude;
		a2 = 0.0021f * altitude;
		if (ambientSimulation && weatherController != null) {
			ambientTemperature = a1-a2+weatherController.localTemperature;//
		} else {
			ambientTemperature = a1 - a2 + 15.443f;
		}
		//CALCULATE PRESSURE
		a = 0.0000004f * altitude * altitude;
		b = (0.0351f*altitude);
		ambientPressure =( a - b + 1009.6f)/10f;
		//CALCULATE MACH SPEED
		soundSpeed = Mathf.Pow((1.2f*287f*(273.15f+ambientTemperature)),0.5f);
		machSpeed = (currentSpeed / 1.944f) / soundSpeed;
		//CALCULATE GROUND SPEED
		densityRatio = airDensity/baseDensity;
		m = Mathf.Pow (densityRatio, 0.5f);
		groundSpeed = currentSpeed / m;
		//CALCULATE BANK ANGLE
		XbankAngle = aircraft.transform.eulerAngles.x;if (XbankAngle > 0) {
		xforce *= -1;}
		if (XbankAngle > 180) {	XbankAngle -= 360;}
		YbankAngle =  aircraft.transform.eulerAngles.y;if (YbankAngle > 180) {
			YbankAngle -= 360;
		}
		ZbankAngle = aircraft.transform.eulerAngles.z;if ( ZbankAngle > 180.0f )
		{ZbankAngle = -(360.0f - ZbankAngle);}
		bankAngle = ZbankAngle;
		//CALCULATE G-FORCE
		xforce = 1/(Mathf.Cos(XbankAngle*0.0174556f));
		yforce =1/(Mathf.Cos(YbankAngle*0.0174556f));
		zforce =1/(Mathf.Cos(ZbankAngle*0.0174556f));
		//
		gForce = (xforce+zforce)/2f;
		//
		if (aircraftType == AircraftType.Jet && speedType == SpeedType.Supersonic) {
			if (machSpeed >= 0.98f && !played) {Boom ();}
			//
			if (machSpeed < 0.98f && played) {played = false;}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Boom()
	{
		boom.PlayOneShot (sonicBoom);
		played = true;
	}
}
	


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroCore))]
public class CoreEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	//
	SilantroCore core;
	SerializedObject coreObject;
	//
	private void OnEnable()
	{
		core = (SilantroCore)target;
		coreObject = new SerializedObject (core);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		coreObject.UpdateIfRequiredOrScript();
		//
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Aircraft Configuration", MessageType.None);
		GUI.color = backgroundColor;GUILayout.Space(2f);
		GUILayout.Space(3f);
		core.aircraftType = (SilantroCore.AircraftType)EditorGUILayout.EnumPopup("Type",core.aircraftType);
		//
		if (core.aircraftType == SilantroCore.AircraftType.Jet) {
			GUILayout.Space(3f);
			core.speedType = (SilantroCore.SpeedType)EditorGUILayout.EnumPopup("Mode",core.speedType);
		}
		//
		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Core Configuration", MessageType.None);
		GUI.color = backgroundColor;
		core.functionality = (SilantroCore.SystemType)EditorGUILayout.EnumPopup(" ",core.functionality);
		if (core.functionality == SilantroCore.SystemType.Advanced) {
			GUILayout.Space (5f);
			core.emptyCenterOfMass = EditorGUILayout.ObjectField ("Empty COM", core.emptyCenterOfMass, typeof(Transform), true) as Transform;}


		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Performance Data", MessageType.None);
		GUI.color = backgroundColor;GUILayout.Space(2f);
		GUILayout.Space(5f);
		EditorGUILayout.LabelField ("Air Speed", core.currentSpeed.ToString ("0.0") + " knots");
		EditorGUILayout.LabelField ("Ground Speed", core.groundSpeed.ToString ("0.0") + " knots");
		if (core.aircraftType == SilantroCore.AircraftType.Jet) {
			EditorGUILayout.LabelField ("Mach", core.machSpeed.ToString ("0.00"));
		}
		if (core.aircraftType == SilantroCore.AircraftType.Jet) {
			EditorGUILayout.LabelField ("G-Force", core.gForce.ToString ("0.0"));
		}
		EditorGUILayout.LabelField ("Altitude", core.currentAltitude.ToString ("0.0") + " feet");
		EditorGUILayout.LabelField ("Vertical Speed", core.verticalSpeed.ToString ("0.0") + " ft/min");
		EditorGUILayout.LabelField ("Heading Direction", core.headingDirection.ToString ("0.0") + " °");
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Ambient Data", MessageType.None);
		GUI.color = backgroundColor;GUILayout.Space(2f);
		GUILayout.Space(5f);
		EditorGUILayout.LabelField ("Air Density", core.airDensity.ToString ("0.000")+ " kg/m3");
		EditorGUILayout.LabelField ("Temperature", core.ambientTemperature.ToString ("0.0")+" °C");
		EditorGUILayout.LabelField ("Pressure", core.ambientPressure.ToString ("0.0")+ " kPa");
		//
		GUILayout.Space(10f);
		if (core.speedType == SilantroCore.SpeedType.Supersonic && core.aircraftType == SilantroCore.AircraftType.Jet) {
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Supersonic Effects", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			core.sonicCone = EditorGUILayout.ObjectField ("Sonic Cone Effect", core.sonicCone, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space(5f);
			core.sonicBoom = EditorGUILayout.ObjectField ("Sonic Boom Sound", core.sonicBoom, typeof(AudioClip),true) as AudioClip;
		}
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Weather", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		core.ambientSimulation = EditorGUILayout.Toggle ("Simulate", core.ambientSimulation);
		if (core.ambientSimulation) {
			GUILayout.Space(5f);
			core.weatherController = EditorGUILayout.ObjectField ("Weather Core", core.weatherController, typeof(SilantroSapphire), true) as SilantroSapphire;
		}
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Stall Warning System", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		core.stallWarner = EditorGUILayout.Toggle ("Active", core.stallWarner);
		if (core.stallWarner) {
			GUILayout.Space(3f);
			core.stallLimitFactor = EditorGUILayout.Slider ("Threshold", core.stallLimitFactor, 0f, 8f);
			GUILayout.Space(4f);
			EditorGUILayout.LabelField ("Stall Angle", core.minimumStallAngle.ToString ("0.00")+ " °");
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Angle of Attack", core.stallThreshold.ToString ("0.00")+ " °");
			GUILayout.Space(4f);
			core.stallClip = EditorGUILayout.ObjectField ("Warning Sound", core.stallClip, typeof(AudioClip), true) as AudioClip;
		}


		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (coreObject.targetObject, "Core Change");}
		if (GUI.changed) {
			EditorUtility.SetDirty (core);
			EditorSceneManager.MarkSceneDirty (core.gameObject.scene);
		}
		coreObject.ApplyModifiedProperties();
	}
}
#endif