//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
//
using System.IO;
using System.Text;
//
public class SilantroPegasusEngine : MonoBehaviour {
	[HideInInspector]public string engineIdentifier = "Default Engine";
	//CURRENT ENGINE STATE
	public enum EngineState
	{
		Off,
		Starting,
		Running
	}
	[HideInInspector]public EngineState CurrentEngineState;
	//START MODE
	public enum EngineStartMode
	{
		Cold,
		Hot
	}
	[HideInInspector]public EngineStartMode engineStartMode = EngineStartMode.Cold;
	//
	//ENGINE DIMENSIONS
	[HideInInspector]public float EngineDiameter = 1f;
	[HideInInspector]public float IntakeDiameterPercentage = 90f;
	[HideInInspector]public float ExhaustDiameterPercentage = 90f;
	[HideInInspector]public float IntakeDiameter;
	float intakeDiameter;
	[HideInInspector]public float ExhaustDiameter;
	[HideInInspector]public float weight = 500f;
	[HideInInspector]public float overallLength = 4f;
	[HideInInspector]public float OverallPressureRatio =10f;
	//
	[HideInInspector]public bool diplaySettings;
	//
	float intakeFactor;
	float pitchFactor;
	//
	//ENGINE RPM CONFIGURATION
	[HideInInspector]public float LowPressureFanRPM = 100f;
	[HideInInspector]public float HighPressureFanRPM = 1000f;
	[HideInInspector]public float RPMAcceleration = 0.5f;
	[HideInInspector]public float LPRPM;
	[HideInInspector]public float HPRPM;
	[HideInInspector]public float TargetRPM;
	[HideInInspector]public float CurrentRPM;
	//
	float LPIdleRPM;
	float HPIdleRPM;
	float currentHPRPM;
	float targetHPRPM;
	//
	//ENGINE RUNNING VARIABLES
	[HideInInspector]public bool EngineOn;
	[HideInInspector]public float engineAcceleration = 0.2f;
	[HideInInspector]public bool isAccelerating;
	[HideInInspector]public float enginePower;
	[HideInInspector]public float EGT;
	//
	//ENGINE SOUND CONFIGURATION
	[HideInInspector]public bool adjustPitchSettings;
	[HideInInspector]public AudioClip ignitionSound;
	[HideInInspector]public AudioClip engineIdleSound;
	[HideInInspector]public AudioClip shutdownSound;
	[HideInInspector]public float EngineAfterburnerPitch = 1.75f;
	[HideInInspector]public float EngineIdlePitch = 0.5f;
	[HideInInspector]public float EngineMaximumRPMPitch = 1f;
	[HideInInspector]public float maximumPitch = 2f;
	[HideInInspector]public float engineSoundVolume = 1f;
	//
	private AudioSource ignitionSource;
	private AudioSource idleSource;
	private AudioSource shutdownSource;
	[HideInInspector]public SilantroCore computer;
	//
	//
	//CALCULATION VARIABLES
	[HideInInspector]public float intakeAirVelocity ;
	[HideInInspector]public float intakeAirMassFlow ;
	[HideInInspector]public float exhaustAirVelocity ;
	[HideInInspector]public float coreAirMassFlow;
	float  fanAirVelocity;
	[HideInInspector]public float fuelMassFlow;
	[HideInInspector]public float intakeArea;
	float fuelFactor = 1f;
	[HideInInspector]public float exhaustArea ;
	float  fanThrust;
	float  fanAirMassFlow;
	float combusionFactor;
	[HideInInspector]public float bypassRatio = 1f;
	//ENGINE EFFECTS
	[HideInInspector]public ParticleSystem engineDistortion;
	[HideInInspector]public ParticleSystem exhaustSmoke1;
	[HideInInspector]ParticleSystem.EmissionModule smokeModule1;
	[HideInInspector]public ParticleSystem exhaustSmoke2;
	[HideInInspector]ParticleSystem.EmissionModule smokeModule2;
	[HideInInspector]public ParticleSystem exhaustSmoke3;
	[HideInInspector]ParticleSystem.EmissionModule smokeModule3;
	[HideInInspector]public ParticleSystem exhaustSmoke4;
	[HideInInspector]ParticleSystem.EmissionModule smokeModule4;
	[HideInInspector]ParticleSystem.EmissionModule distortionModule;
	[HideInInspector]public float maximumDistortionEmission = 20;
	[HideInInspector]public float maximumEmissionValue = 50f;
	[HideInInspector]public float controlValue;
	//
	//ENGINE FUEL SYSTEM
	public enum FuelType
	{
		JP6,
		JP8
	}
	[HideInInspector]public FuelType fuelType = FuelType.JP6;
	[HideInInspector]public float combustionEnergy;
	[HideInInspector]public SilantroFuelDistributor fuelSystem;
	[HideInInspector]public float TSFC = 0.1f;
	[HideInInspector]public float currentTankFuel;
	[HideInInspector]public float criticalFuelLevel = 10f;
	[HideInInspector]public float actualConsumptionrate;
	[HideInInspector]bool InUse;
	[HideInInspector]public bool LowFuel;
	bool fuelAlertActivated;
	float sfc;
	//
	//ENGINE EXTERNAL CONNECTIONS
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Transform intakeFanPoint;
	[HideInInspector]public Transform Thruster1;
	[HideInInspector]public Transform Thruster2;
	[HideInInspector]public Transform Thruster3;
	[HideInInspector]public Transform Thruster4;
	//
	[HideInInspector]public bool vectoredThrust;
	[HideInInspector]public SilantroNozzle nozzleControl;
	[HideInInspector]public bool canUseNozzle;
	public enum RotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	//
	public enum RotationDirection
	{
		CW,CCW
	}
	[HideInInspector]public RotationDirection rotationDirection = RotationDirection.CCW;
	//
	//
	//ENGINE DATA RECORDING
	[HideInInspector]public bool saveEngineData = false;
	[HideInInspector]public string saveLocation = "C:/Users/";
	[HideInInspector]public float dataLogRate = 5f;
	[HideInInspector]public bool InculdeUnits = true;
	//
	//ENGINE CONTROL VARIABLES
	[HideInInspector]public float FuelInput = 0.2f;
	//
	//CONTROL BOOLS
	[HideInInspector]public bool start;
	[HideInInspector]public bool stop;
	private bool starting;
	[HideInInspector]public bool isControllable;
	//
	//ENVIRONMENTAL VARIABLES
	float ambientPressure;
	[HideInInspector]public float airDensity = 1.225f;
	//
	[HideInInspector]public float EngineThrust;
	//
	//ENGINE CONTROL FUNCTIONS
	public void StartEngine()
	{
		//MAKE SURE THERE IS FUEL TO START THE ENGINE
		if (fuelSystem && fuelSystem.TotalFuelRemaining > 1f) {
			//
			//MAKE SURE CORRECT FUEL IS SELECTED
			if (fuelType.ToString () == fuelSystem.fuelType.ToString ()) {

				if (engineStartMode == EngineStartMode.Cold) {
					start = true;
				}
				if (engineStartMode == EngineStartMode.Hot) {
					//JUMP START ENGINE
					EngineOn = true;
					RunEngine ();
					starting = false;
					CurrentEngineState = EngineState.Running;
				}
			} else {
				Debug.Log ("Engine " + transform.name + " cannot start due to incorrect fuel selection");
			}
		}
	}
	//
	public void ShutDownEngine()
	{
		stop = true;
	}
	//
	//SET THROTTLE VALUE
	public void SetEngineThrottle(float inputThrottle)
	{
		if (inputThrottle < 1.1f) {
			FuelInput = inputThrottle;
		}
	}




	//
	//DRAW ENGINE LAYOUT	
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		//
		IntakeDiameter = EngineDiameter * IntakeDiameterPercentage / 100f;
		Handles.color = Color.blue;
		if(intakeFanPoint != null && connectedAircraft!=null){
			Handles.DrawWireDisc (intakeFanPoint.transform.position, connectedAircraft.transform.forward, (IntakeDiameter / 2f));
		}
	}
	//
	#endif
	//
	public void InitializeEngine () {
		//SETUP SOUND SYSTEM
		SoundConfiguration();
		//SETUP EFFECTS
		EffectsInitial();
		//RECIEVE DIAMETER
		intakeDiameter = IntakeDiameter;
		//
		//SET UP MASS FACTOR FOR EGT CALCULATION
		fuelMassFlow = TSFC / 1000f;
		//SET IDLE RPM VALUES
		LPIdleRPM = LowPressureFanRPM * 0.1f;
		HPIdleRPM = HighPressureFanRPM * 0.09f;
		//
		//SET UP ENGINE FUEL COMBUSTION VALUES
		if (fuelType == FuelType.JP6) 
		{
			combustionEnergy = 49.6f;
		} 
		else if (fuelType == FuelType.JP8) 
		{
			combustionEnergy = 43.28f;
		}
		//
		intakeFactor = UnityEngine.Random.Range(0.38f,0.45f);//FACTOR OF TEMPERATURE IN FUTURE UPDATES
		combusionFactor = combustionEnergy/42f;
		//
		//RESET VALUES
		EngineOn = false;
		starting = false;
		start = false;
		stop = false;
		//SET ENGINE JUMP START VALUE;
		if (engineStartMode == EngineStartMode.Hot) {
			RPMAcceleration = 10f;
			engineAcceleration = 10f;
		}
		//
	}
	//SETUP EFFECTS
	void EffectsInitial()
	{
		if (exhaustSmoke1 != null) {
			smokeModule1 = exhaustSmoke1.emission;
			smokeModule1.rateOverTime = 0f;
		}
		if (exhaustSmoke2 != null) {
			smokeModule2 = exhaustSmoke2.emission;
			smokeModule2.rateOverTime = 0f;
		}
		if (exhaustSmoke3 != null) {
			smokeModule3 = exhaustSmoke3.emission;
			smokeModule3.rateOverTime = 0f;
		}
		if (exhaustSmoke4 != null) {
			smokeModule4 = exhaustSmoke4.emission;
			smokeModule4.rateOverTime = 0f;
		}
		if (engineDistortion != null) {
			distortionModule = engineDistortion.emission;
			distortionModule.rateOverTime = 0f;
		}
	}
	//
	void SoundConfiguration()
	{
		//SETUP IGNITION
		if (ignitionSound != null) {
			ignitionSource = Thruster1.gameObject.AddComponent<AudioSource>();
			ignitionSource.clip = ignitionSound;//Assign sound
			ignitionSource.loop = false;//Ignition sound should only be played once
			ignitionSource.dopplerLevel = 0f;//
			ignitionSource.spatialBlend = 1f;//Make sterio
			ignitionSource.rolloffMode = AudioRolloffMode.Custom;//Limit sound range
			ignitionSource.maxDistance = 650f;//Actual Sound Range
		}
		//SETUP IDLE
		if (engineIdleSound != null) {
			//CREATE A GAMEOBJECT TO ADD SOUND SOURCE TO
			GameObject soundPoint = new GameObject();
			soundPoint.transform.parent = this.transform;
			soundPoint.transform.localPosition = new Vector3 (0, 0, 0);
			soundPoint.name = this.name +" Sound Point";
			//
			idleSource = soundPoint.gameObject.AddComponent<AudioSource>();
			idleSource.clip = engineIdleSound;
			idleSource.loop = true;
			idleSource.Play();
			idleSource.volume = 0f;
			idleSource.spatialBlend = 1f;
			idleSource.dopplerLevel = 0f;
			idleSource.rolloffMode = AudioRolloffMode.Custom;
			idleSource.maxDistance = 600f;
		}
		//SETUP SHUTDOWN
		if (shutdownSound != null) {
			shutdownSource = Thruster1.gameObject.AddComponent<AudioSource>();
			shutdownSource.clip = shutdownSound;
			shutdownSource.loop = false;
			shutdownSource.dopplerLevel = 0f;
			shutdownSource.spatialBlend = 1f;
			shutdownSource.rolloffMode = AudioRolloffMode.Custom;
			shutdownSource.maxDistance = 650f;
		}
	}
	//
	[HideInInspector]public float liftFactor = 1f;
	void FixedUpdate()
	{
		if (isControllable) {
			//RPM
			LPRPM = CurrentRPM;
			HPRPM = currentHPRPM;
			if (CurrentRPM <= 0f) {
				CurrentRPM = 0f;
			}
			//APPLY GENERATED FORCE
			if (EngineThrust > 0f && connectedAircraft != null) {
				float individualForce = (EngineThrust/4f) * liftFactor;
				Vector3 force1 = Thruster1.forward * individualForce;
				Vector3 force2 = Thruster2.forward * individualForce;
				Vector3 force3 = Thruster3.forward * individualForce;
				Vector3 force4 = Thruster4.forward * individualForce;
				//
				connectedAircraft.AddForce (force1, ForceMode.Force);
				connectedAircraft.AddForce (force2, ForceMode.Force);
				connectedAircraft.AddForce (force3, ForceMode.Force);
				connectedAircraft.AddForce (force4, ForceMode.Force);
			}
			//
			if (fuelSystem != null) {
				currentTankFuel = fuelSystem.TotalFuelRemaining;
			}
			//
			//ROTATE ENGINE FAN
			if (intakeFanPoint) {
				if (rotationDirection == RotationDirection.CCW) {
					if (rotationAxis == RotationAxis.X) {
						intakeFanPoint.Rotate (new Vector3 (CurrentRPM * Time.deltaTime, 0, 0));
					}
					if (rotationAxis == RotationAxis.Y) {
						intakeFanPoint.Rotate (new Vector3 (0, CurrentRPM * Time.deltaTime, 0));
					}
					if (rotationAxis == RotationAxis.Z) {
						intakeFanPoint.Rotate (new Vector3 (0, 0, CurrentRPM * Time.deltaTime));
					}
				}
				//
				if (rotationDirection == RotationDirection.CW) {
					if (rotationAxis == RotationAxis.X) {
						intakeFanPoint.Rotate (new Vector3 (-1f * CurrentRPM * Time.deltaTime, 0, 0));
					}
					if (rotationAxis == RotationAxis.Y) {
						intakeFanPoint.Rotate (new Vector3 (0, -1f * CurrentRPM * Time.deltaTime, 0));
					}
					if (rotationAxis == RotationAxis.Z) {
						intakeFanPoint.Rotate (new Vector3 (0, 0, -1f * CurrentRPM * Time.deltaTime));
					}
				}
			}
		}
	}
	//
	float aircraftSpeed;
	float knotsSpeed;
	float speedFactor;
	float rpmFactor;float value;float coreFactor;
	//
	void Update()
	{
		if (isControllable) {
			//SEND CORE DATA
			CoreEngine();
			//SEND FUEL DATA
			if (InUse && fuelSystem != null) {
				UseFuel ();
			}
			//SEND CALCULATION DATA
			if (enginePower > 0f) {
				EngineCalculation ();
			}
			//ENGINE STATE CONTROL
			if (ignitionSound != null && engineIdleSound != null && shutdownSound != null) {
				switch (CurrentEngineState) {
				case EngineState.Off:
					ShutdownEngineProcedure ();
					break;
				case EngineState.Starting:
					StartEngineProcedure ();
					break;
				case EngineState.Running:
					RunEngine ();
					break;
				}
			}
			//INTERPOLATE ENGINE RPM
			if (EngineOn) {
				CurrentRPM = Mathf.Lerp (CurrentRPM, TargetRPM, RPMAcceleration * Time.deltaTime * (enginePower * fuelFactor * fuelFactor));
				currentHPRPM = Mathf.Lerp (currentHPRPM, targetHPRPM, RPMAcceleration * Time.deltaTime * (enginePower * fuelFactor * fuelFactor));
				//exhaustModule.rateOverTime = (enginePower * maximumEmmisionValue);
			} else {
				CurrentRPM = Mathf.Lerp (CurrentRPM, 0.0f, RPMAcceleration * Time.deltaTime);
				currentHPRPM = Mathf.Lerp (currentHPRPM, 0.0f, RPMAcceleration * Time.deltaTime);
			}
			//
			coreFactor = CurrentRPM/LowPressureFanRPM;
			engineVolume = (engineSoundVolume *(LPRPM/LowPressureFanRPM));
			engineVolume = Mathf.Clamp(engineVolume,0.5f,1f);
			//FILTER "NAN" value OUT TO AVOID PROBLEMS WITH WEBGL
			if (engineVolume > 0.0001f && engineVolume < 2f && engineIdleSound != null) {
				idleSource.volume = engineVolume;
			}
			//
			//ENGINE EFFECTS
			if (EngineOn == true) {
				controlValue = enginePower;
			} else {
				controlValue = Mathf.Lerp (controlValue, 0f, 0.04f);
			}
			//

			value = Mathf.Lerp (value, maximumEmissionValue, 0.02f);
			if (exhaustSmoke1 != null) {
				smokeModule1.rateOverTime = maximumEmissionValue * enginePower * coreFactor;
			}
			if (exhaustSmoke2 != null) {
				smokeModule2.rateOverTime = maximumEmissionValue * enginePower * coreFactor;
			}
			if (exhaustSmoke3 != null) {
				smokeModule3.rateOverTime = maximumEmissionValue * enginePower * coreFactor;
			}
			if (exhaustSmoke4 != null) {
				smokeModule4.rateOverTime = maximumEmissionValue * enginePower * coreFactor;
			}
			if (engineDistortion != null) {
				distortionModule.rateOverTime = maximumDistortionEmission * enginePower * coreFactor;
			}

			float actualValue = (controlValue) * value * coreFactor;
			//CALCULATE ENGINE PITCH
			if (engineIdleSound != null && connectedAircraft != null) {
				aircraftSpeed = connectedAircraft.velocity.magnitude;
				knotsSpeed = aircraftSpeed * 1.943f;
				speedFactor = CurrentRPM + knotsSpeed + 10f;
				rpmFactor = (speedFactor - LPIdleRPM) / (LowPressureFanRPM - LPIdleRPM);
				//
				pitchFactor = EngineIdlePitch + (EngineMaximumRPMPitch - EngineIdlePitch) * rpmFactor;
				pitchFactor = Mathf.Clamp (pitchFactor, 0, maximumPitch);
			} 
			float startRange;
			float endRange;
			float cycleRange;
			float offset;
			//
			//SIMULATE ENGINE FUEL CHOKING
			if (fuelSystem != null && engineIdleSound != null){
				if (fuelSystem.TotalFuelRemaining <= criticalFuelLevel) {
					if (EngineOn) {
						startRange = 0.6f;
						endRange = 1.0f;
						cycleRange = (endRange - startRange) / 2f;
						offset = cycleRange + startRange;
						//
						fuelFactor = offset + Mathf.Sin (Time.time * 3f) * cycleRange;
						idleSource.pitch = fuelFactor;
					}
				}
				else {
					idleSource.pitch = pitchFactor * enginePower;
				}
			} 
			//STOP ENGINE IF FUEL IS EXHAUSTED
			if (fuelSystem != null && fuelSystem.TotalFuelRemaining <= 0) {
				stop = true;
			}
			//
		} 
	}
	//
	[HideInInspector]public float engineVolume;
	//ENGINE RUN CONFIGURATION
	private void RunEngine()
	{
		//STOP IGINITION SOUND IF ITS STILL PLAYING
		if (ignitionSource.isPlaying) {
			ignitionSource.Stop ();
		}
		//CLAMP ENGINE INPUT
		FuelInput = Mathf.Clamp(FuelInput,0f,1f);InUse = true;
		//ENGINE RPM CALCULATION
		TargetRPM = LPIdleRPM + (LowPressureFanRPM - LPIdleRPM) * FuelInput;
		targetHPRPM = HPIdleRPM + (HighPressureFanRPM - HPIdleRPM) * FuelInput; 
		//STOP ENGINE
		if (stop)
		{
			CurrentEngineState = EngineState.Off;
			shutdownSource.Play();EngineThrust = 0;EngineOn = false;
			FuelInput = 0f;EngineThrust = 0f;
			//
			StartCoroutine(ReturnIgnition());
		}
	}
	//
	//START ENGINE
	private void StartEngineProcedure()
	{
		if (starting) {
			if (!ignitionSource.isPlaying) {
				CurrentEngineState = EngineState.Running;
				starting = false;
				RunEngine();
			}
		}
		else
		{
			ignitionSource.Stop();
			CurrentEngineState = EngineState.Off;
		}
		//SET RPM VALUES
		TargetRPM = LPIdleRPM;
		targetHPRPM = HPIdleRPM;
	}
	//
	//STOP ENGINE
	private void ShutdownEngineProcedure()
	{
		//STOP IGNITION SOUND IF PLAYING
		if (ignitionSource.isPlaying)
		{
			ignitionSource.Stop();
			start = false;
		}
		//START ENGINE PROCEDURE
		if (start)
		{
			EngineOn = true;
			ignitionSource.Play();
			CurrentEngineState = EngineState.Starting;
			starting = true;
			//RESET
			StartCoroutine(ReturnIgnition());
		}
		//SET RPM VALUES
		TargetRPM = 0f;
		targetHPRPM = 0f;
	}
	//ENGINE CORE 
	private void CoreEngine()
	{
		if (EngineOn) {
			if (!isAccelerating && enginePower < 1f) {
				//REV UP ENGINE
				enginePower += Time.deltaTime * engineAcceleration;
				//Calculate EGT
			}
		} 
		else if (enginePower > 0f) {
			//REV DOWN ENGINE
			enginePower -= Time.deltaTime *engineAcceleration;
		}
		if (!EngineOn && enginePower < 0) {
			enginePower = 0;
			EGT = 0;
		}
		if (enginePower > 1) {enginePower = 1f;}
	}
	//RESET CONTROL VALUES
	public IEnumerator ReturnIgnition()
	{
		yield return new WaitForSeconds (0.5f);
		start = false;
		stop = false;
	}
	//
	//CALCULATE FUEL FLOW
	private float poundThrust;
	void CalculateFuelFlow(float currentThrust)
	{
		poundThrust = currentThrust / 4.448f;
		sfc = (poundThrust * TSFC) / 3600f;
		//
		fuelMassFlow = sfc*0.4536f;
	}
	//
	//DEPLETE FUEL LEVEL WITH USAGE
	private void UseFuel()
	{
		actualConsumptionrate = combusionFactor*fuelMassFlow * enginePower;
		//SHUTDOWN ENGINE IF FUEL IS EXPENDED
		if (fuelSystem != null && fuelSystem.TotalFuelRemaining == 0f)
		{
			stop = true;
			EngineThrust = 0f;
		}
	}
	//CALCULATE ENGINE THRUST
	float bypassfactor;
	float coreThrust;
	private void EngineCalculation()
	{
		//COLLECT ENVIRONMENTAL/AMBIENT VARIABLES
		if (connectedAircraft != null) {

		} else {
			airDensity = 1.225f;
			ambientPressure = 102f;
		}
		if (connectedAircraft != null) {
			aircraftSpeed = connectedAircraft.velocity.magnitude;
		}
		//CORE CALCULATIONS
		intakeArea = (3.142f * intakeDiameter * intakeDiameter) / 4f;
		exhaustArea = (3.142f * ExhaustDiameter * ExhaustDiameter) / 4f;
		//
		intakeAirVelocity = (3.142f * intakeDiameter * LPRPM) / 60f;
		fanAirVelocity = intakeAirVelocity * intakeFactor;
		exhaustAirVelocity = (3.142f * ExhaustDiameter * HPRPM) / 60f;
		intakeAirMassFlow = airDensity * intakeArea * fanAirVelocity;
		bypassfactor = (1+bypassRatio);
		coreAirMassFlow = (1 / bypassfactor) * intakeAirMassFlow;
		fanAirMassFlow = (bypassRatio / bypassfactor) * intakeAirMassFlow;
		//
		fanThrust = fanAirMassFlow *(intakeAirVelocity - aircraftSpeed);
		//
		coreThrust = (((coreAirMassFlow + fuelMassFlow) * (exhaustAirVelocity)) - ((coreAirMassFlow * aircraftSpeed)*0.4f) + (exhaustArea * ((OverallPressureRatio * ambientPressure) - ambientPressure)));
		//
		EngineThrust = (coreThrust + fanThrust)*coreFactor;//TOTAL THRUST GENERATED
		//MAKE SURE THRUST IS NEVER NEGATIVE
		if (EngineThrust < 0) {
			EngineThrust = 0;
		}
		////CALULATE FUEL FLOW
		if (EngineThrust > 0) {
			CalculateFuelFlow (EngineThrust);
		}
	}
	//
}
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPegasusEngine))]
public class PegasusEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	[HideInInspector]public int toolbarTab;
	[HideInInspector]public string currentTab;
	//
	SilantroPegasusEngine fan;
	SerializedObject engineObject;
	//
	private void OnEnable()
	{
		fan = (SilantroPegasusEngine)target;
		engineObject = new SerializedObject (fan);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		//
		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Identifier", MessageType.None);
		GUI.color = backgroundColor;
		fan.engineIdentifier = EditorGUILayout.TextField (" ", fan.engineIdentifier);

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		//DISPLAY ENGINE DIMENSIONS
		fan.EngineDiameter = EditorGUILayout.FloatField("Engine Diameter", fan.EngineDiameter);
		GUILayout.Space(2f);
		fan.IntakeDiameterPercentage = EditorGUILayout.Slider ("Intake Diameter Percentage", fan.IntakeDiameterPercentage,0,100);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Intake Diameter",  fan.IntakeDiameter.ToString ("0.00") + " m");
		GUILayout.Space(2f);
		fan.ExhaustDiameterPercentage = EditorGUILayout.Slider ("Exhaust Diameter Percentage", fan.ExhaustDiameterPercentage,0,100);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Exhaust Diameter",  fan.ExhaustDiameter.ToString ("0.00") + " m");
		//
		GUILayout.Space(3f);
		fan.weight = EditorGUILayout.FloatField("Engine Weight", fan.weight);
		GUILayout.Space(2f);
		fan.overallLength = EditorGUILayout.FloatField("Overall Length", fan.overallLength);
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Turbine Configuration", MessageType.None);
		GUI.color = backgroundColor;
		fan.LowPressureFanRPM = EditorGUILayout.FloatField ("Low Pressure RPM",  fan.LowPressureFanRPM);
		GUILayout.Space(2f);
		fan.HighPressureFanRPM = EditorGUILayout.FloatField ("High Pressure RPM",  fan.HighPressureFanRPM);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("N1", fan.LPRPM.ToString("0.00")+ " RPM");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("N2", fan.HPRPM.ToString("0.00")+ " RPM");
		//
		if ( fan.engineStartMode == SilantroPegasusEngine.EngineStartMode.Cold) {
			GUILayout.Space (2f);
			fan.RPMAcceleration = EditorGUILayout.FloatField ("Engine Acceleration",  fan.RPMAcceleration);
		} 
		//
		GUILayout.Space(9f);
		fan.bypassRatio = EditorGUILayout.FloatField ("Bypass Ratio", fan.bypassRatio);
		GUILayout.Space(3f);
		fan.OverallPressureRatio = EditorGUILayout.FloatField ("Pressure Ratio",  fan.OverallPressureRatio);
		//
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Nozzle System", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		fan.canUseNozzle = EditorGUILayout.Toggle ("Available", fan.canUseNozzle);
		if (fan.canUseNozzle) {
			GUILayout.Space(3f);
			fan.nozzleControl = EditorGUILayout.ObjectField ("Actuator", fan.nozzleControl, typeof(SilantroNozzle), true) as SilantroNozzle;
		}
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Fuel Configuration", MessageType.None);
		GUI.color = backgroundColor;
		//
		//DISPLAY FUEL CONFIGURATION
		GUILayout.Space(2f);
		fan.fuelType = (SilantroPegasusEngine.FuelType)EditorGUILayout.EnumPopup ("Fuel Type",  fan.fuelType);
		//SET UP ENGINE FUEL COMBUSTION VALUES
		if ( fan.fuelType == SilantroPegasusEngine.FuelType.JP6) 
		{
			fan.combustionEnergy = 47.6f;
		} 
		else if ( fan.fuelType == SilantroPegasusEngine.FuelType.JP8)
		{
			fan.combustionEnergy = 43.28f;
		} 
		//
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Combustion Energy", fan.combustionEnergy.ToString("0.0")+" MJoules");
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Fuel Usage Settings", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Fuel Remaining",  fan.currentTankFuel.ToString ("0.00") + " kg");
		GUILayout.Space(5f);
		EditorGUILayout.HelpBox ("Thrust Specific fuel consumption in lb/lbf.hr", MessageType.None);
		GUILayout.Space(3f);
		fan.TSFC = EditorGUILayout.FloatField ("TSFC", fan.TSFC);
		GUILayout.Space(5f);
		EditorGUILayout.LabelField ("Actual Consumption Rate", fan.actualConsumptionrate.ToString("0.00")+" kg/s");
		fan.criticalFuelLevel = EditorGUILayout.FloatField ("Critical Fuel Level",  fan.criticalFuelLevel);
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Connections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		fan.connectedAircraft = EditorGUILayout.ObjectField ("Connected Aircraft",  fan.connectedAircraft, typeof(Rigidbody), true) as Rigidbody;
		GUILayout.Space(2f);
		fan.intakeFanPoint = EditorGUILayout.ObjectField ("Intake Fan",  fan.intakeFanPoint, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		fan.rotationAxis = (SilantroPegasusEngine.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis", fan.rotationAxis);
		GUILayout.Space(3f);
		fan.rotationDirection = (SilantroPegasusEngine.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction", fan.rotationDirection);
		//
		GUILayout.Space(5f);
		fan.Thruster1 = EditorGUILayout.ObjectField ("Thruster1", fan.Thruster1, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		fan.Thruster2 = EditorGUILayout.ObjectField ("Thruster2", fan.Thruster2, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		fan.Thruster3 = EditorGUILayout.ObjectField ("Thruster3", fan.Thruster3, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		fan.Thruster4 = EditorGUILayout.ObjectField ("Thruster4", fan.Thruster4, typeof(Transform), true) as Transform;
		//
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		fan.ignitionSound = EditorGUILayout.ObjectField ("Ignition Sound",  fan.ignitionSound, typeof(AudioClip), true) as AudioClip;
		GUILayout.Space(2f);
		fan.engineIdleSound = EditorGUILayout.ObjectField ("Engine Idle Sound",  fan.engineIdleSound, typeof(AudioClip), true) as AudioClip;
		GUILayout.Space(2f);
		fan.shutdownSound = EditorGUILayout.ObjectField ("Shutdown Sound",  fan.shutdownSound, typeof(AudioClip), true) as AudioClip;
		//
		GUILayout.Space(3f);
		fan.adjustPitchSettings = EditorGUILayout.Toggle("Show Pitch Settings", fan.adjustPitchSettings);
		GUILayout.Space(1f);
		if ( fan.adjustPitchSettings) {
			fan.EngineIdlePitch = EditorGUILayout.FloatField ("Idle Pitch",  fan.EngineIdlePitch);
			GUILayout.Space(2f);
			fan.EngineMaximumRPMPitch = EditorGUILayout.FloatField ("Maximum Pitch",  fan.EngineMaximumRPMPitch);
			//
		}
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Effects Configuration", MessageType.None);
		GUI.color = backgroundColor;
		fan.diplaySettings = EditorGUILayout.Toggle ("Show Extras",  fan.diplaySettings);
		if ( fan.diplaySettings) {
			GUILayout.Space (15f);
			GUI.color = Color.yellow;
			EditorGUILayout.HelpBox ("Engine Effects Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			fan.exhaustSmoke1 = EditorGUILayout.ObjectField ("Exhaust Smoke1", fan.exhaustSmoke1, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (3f);
			fan.exhaustSmoke2 = EditorGUILayout.ObjectField ("Exhaust Smoke2", fan.exhaustSmoke2, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (3f);
			fan.exhaustSmoke3 = EditorGUILayout.ObjectField ("Exhaust Smoke3", fan.exhaustSmoke3, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (3f);
			fan.exhaustSmoke4 = EditorGUILayout.ObjectField ("Exhaust Smoke4", fan.exhaustSmoke4, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (4f);
			fan.maximumEmissionValue = EditorGUILayout.FloatField ("Maximum Emission",  fan.maximumEmissionValue);
			//
			GUILayout.Space (3f);
			fan.engineDistortion = EditorGUILayout.ObjectField ("Engine Distortion",  fan.engineDistortion, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (2f);
			fan.maximumDistortionEmission = EditorGUILayout.FloatField ("Maximum Emission",  fan.maximumDistortionEmission);
			//
		}
		//
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		//
		EditorGUILayout.LabelField ("Throttle Level",( fan.FuelInput*100f).ToString("0.00") + " %");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Engine State", fan.CurrentEngineState.ToString());
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Engine Power",( fan.enginePower*100f).ToString("0.00") + " %");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("EGT", fan.EGT.ToString("0.0")+ " °C");
		GUILayout.Space(2f);
		//
		EditorGUILayout.LabelField ("Core Speed", fan.LPRPM.ToString("0.0")+ " RPM");
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Engine Output", MessageType.None);
		GUI.color = backgroundColor;
		EditorGUILayout.LabelField ("Engine Thrust", fan.EngineThrust.ToString("0.0")+ " N");
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (engineObject.targetObject, "Pegasus Engine Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (fan);
			EditorSceneManager.MarkSceneDirty (fan.gameObject.scene);
		}
		engineObject.ApplyModifiedProperties();
	}
}
#endif