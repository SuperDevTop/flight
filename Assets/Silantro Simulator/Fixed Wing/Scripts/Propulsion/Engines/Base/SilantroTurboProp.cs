using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SilantroTurboProp : MonoBehaviour {

	[HideInInspector]public string engineIdentifier = "Default Engine";

	//------------------------------------------------SELECTORS
	//CURRENT ENGINE STATE
	public enum EngineState{Off,Starting,Active}
	[HideInInspector]public EngineState CurrentEngineState;
	//START MODE
	public enum EngineStartMode{Cold,Hot}
	[HideInInspector]public EngineStartMode engineStartMode = EngineStartMode.Cold;
	//SOUND MODE
	public enum SoundState{Available,Absent}
	[HideInInspector]public SoundState soundState = SoundState.Absent;
	//FUEL TYPE
	public enum FuelType{JetB,JetA1,JP6,JP8}
	[HideInInspector]public FuelType fuelType = FuelType.JetB;
	//FAN ROTATION AXIS
	public enum RotationAxis{X,Y,Z}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	//ROTATION DIRECTION
	public enum RotationDirection{CW,CCW}
	[HideInInspector]public RotationDirection rotationDirection = RotationDirection.CCW;
	[HideInInspector]public bool isControllable;
	public enum IntakeShape{Rectangular,Circular,Oval}
	[HideInInspector]public IntakeShape intakeType = IntakeShape.Circular;
	float intakeFactor;

	//---------------------------------------------ENGINE DIMENSIONS
	[HideInInspector]public float EngineDiameter = 1f;
	[HideInInspector]public float IntakeDiameterPercentage = 90f;
	[HideInInspector]public float ExhaustDiameterPercentage = 90f;
	[HideInInspector]public float IntakeDiameter;
	[HideInInspector]public float ExhaustDiameter;
	[HideInInspector]public float actualIntakeDiameter,actualExhaustDiameter;[HideInInspector]public bool diplaySettings;

	//---------------------------------------------CONNECTION POINTS
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Transform IntakePoint;
	[HideInInspector]public Transform ExhaustPoint;
	[HideInInspector]public SilantroFuelDistributor fuelSystem;
	[HideInInspector]public SilantroCore computer;
	[HideInInspector]public SilantroController controller;


	//---------------------------------------------ENGINE CORE FACTORS
	float norminalRPM,baseRPM;[HideInInspector]public float functionalRPM = 1000;
	//[HideInInspector]
	[HideInInspector]public float FuelInput;

	[HideInInspector]public AnimationCurve pressureFactor;
	[HideInInspector]public AnimationCurve adiabaticFactor;

	//-------------------------------------------CASES
	[HideInInspector]public float Pa,P1,P2,P3,P4,P5,P6,Pc;
	[HideInInspector]public float Ta,T1,T2,T3,T4,T5,T6;
	[HideInInspector]public float Ya, Y1, Y2, Y3, Y4, Y5, Y6;
	[HideInInspector]public float Cpa,Cp1,Cp2,Cp3,Cp4,Cp5,Cp6;
	[HideInInspector]public float TurbineTemperature = 1000f,MaximumTemperature = 2000f;
	[HideInInspector]public float np, nth, no,Me,Ma,Mf,PPC;
	[HideInInspector]public float exhaustVelocity,exhaustTemperature;
	[HideInInspector]public float pressureRatio = 10f, combustionEnergy;[HideInInspector]public float f,Ae;
	float drawDiameter;
	//EFFICIENCIES
	[HideInInspector]public float pcc = 6f,nd = 92f,nc = 95f,nb = 98f,nt = 97f,nab = 92f,nn = 96f,ng = 95f;
	[HideInInspector]public float Wc,alpha,Wt,Wshaft,Pshaft,Tj,Pt,brakePower,Hc,mach;
	[HideInInspector]public float TSFC;[HideInInspector]public float actualConsumptionrate;

	//-------------------------------------------ENGINE EFFECTS
	[HideInInspector]public ParticleSystem exhaustSmoke;
	ParticleSystem.EmissionModule smokeModule;Color baseColor;Color finalColor;
	[HideInInspector]public float maximumEmissionValue = 50f;
	[HideInInspector]public float emissionCore;
	[HideInInspector]public float T7Factor1, T7factor2;

	//---------------------------------------------SOUND SYSTEM
	//1. EXTERIOR
	[HideInInspector]public AudioClip ExteriorIgnitionSound;
	[HideInInspector]public AudioClip ExteriorIdleSound;
	[HideInInspector]public AudioClip ExteriorShutdownSound;
	private AudioSource ExteriorIgnitionSource;
	private AudioSource ExteriorIdleSource;
	private AudioSource ExteriorShutdownSource;
	//2. INTERIOR
	[HideInInspector]public AudioClip InteriorIgnitionSound;
	[HideInInspector]public AudioClip InteriorIdleSound;
	[HideInInspector]public AudioClip InteriorShutdownSound;
	private AudioSource InteriorIgnitionSource;
	private AudioSource InteriorIdleSource;
	private AudioSource InteriorShutdownSource;
	//
	[HideInInspector]public bool adjustSoundSettings,showPerformance,showConstants;
	[HideInInspector]public float currentEnginePitch,maximumEnginePitch = 0.9f,IdleEnginePitch= 0.5f;
	[HideInInspector]public float currentEngineVolume ,maximumEngineVolume;

	//---------------------------------------------ENGINE VARIABLES
	float startRange;float endRange;float cycleRange;float offset,fuelFactor;
	[HideInInspector]public float corePower,coreAcceleration = 0.25f,coreValue,coreFactor,value;
	[HideInInspector]public float coreRPM;

	//---------------------------------------------CONTROL BOOLS
	[HideInInspector]public bool start,stop,clutching,active;







	//ENGINE CONTROL FUNCTIONS
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void StartEngine()
	{
		//MAKE SURE SOUND IS SET PROPERLY
		if (ExteriorIdleSound == null || ExteriorIgnitionSound == null || ExteriorShutdownSound == null) {
			Debug.Log ("Engine " + transform.name + " cannot start due to incorrect Audio configuration");
		} 
		else {
			//MAKE SURE THERE IS FUEL TO START THE ENGINE
			if (fuelSystem && fuelSystem.TotalFuelRemaining > 1f) {
				//MAKE SURE CORRECT FUEL IS SELECTED
				if (fuelType.ToString () == fuelSystem.fuelType.ToString ()) {

					//ACTUAL START ENGINE
					if (engineStartMode == EngineStartMode.Cold) {
						start = true;
					}
					if (engineStartMode == EngineStartMode.Hot) {
						//JUMP START ENGINE
						active = true;
						StateActive ();clutching = false;CurrentEngineState = EngineState.Active;
					}
				} 
				else {Debug.Log ("Engine " + transform.name + " cannot start due to incorrect fuel selection");}} 
			else {Debug.Log ("Engine " + transform.name + " cannot start due to low fuel");}
		}
	}


	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ShutDownEngine(){stop = true;}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SET THROTTLE VALUE
	public void SetEngineThrottle(float inputThrottle)
	{
		if (inputThrottle < 1.1f) {FuelInput = inputThrottle;}
	}



	//DRAW ENGINE LAYOUT	
	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		ExhaustDiameter = EngineDiameter * (ExhaustDiameterPercentage/100f);drawDiameter = ExhaustDiameter;
		Handles.color = Color.red;
		if(ExhaustPoint != null){Handles.DrawWireDisc (ExhaustPoint.position, ExhaustPoint.transform.forward, (drawDiameter/2f));}
		//INTAKE
		IntakeDiameter = EngineDiameter * (IntakeDiameterPercentage / 100f);
		actualIntakeDiameter = IntakeDiameter;
		Handles.color = Color.blue;
		if(IntakePoint != null && connectedAircraft!=null){Handles.DrawWireDisc (IntakePoint.transform.position, connectedAircraft.transform.forward, (actualIntakeDiameter / 2f));}
		Handles.color = Color.cyan;
		if(ExhaustPoint != null && IntakePoint != null ){Handles.DrawLine (IntakePoint.transform.position, ExhaustPoint.position);}
		DrawPressureFactor ();DrawAdiabaticConstant ();
	}
	#endif


	private void DrawPressureFactor()
	{
		pressureFactor = new AnimationCurve ();
		//-----------------------------------SPECIFIC PRESSURE
		Keyframe a1 = new Keyframe (250, 1.003f);Keyframe b1 = new Keyframe (300, 1.005f);Keyframe c1 = new Keyframe (350, 1.008f);
		Keyframe d1 = new Keyframe (400, 1.013f);Keyframe e1 = new Keyframe (450, 1.020f);Keyframe f1 = new Keyframe (500, 1.029f);
		Keyframe g1 = new Keyframe (550, 1.040f);Keyframe h1 = new Keyframe (600, 1.051f);Keyframe i1 = new Keyframe (650, 1.063f);
		Keyframe j1 = new Keyframe (700, 1.075f);Keyframe k1 = new Keyframe (750, 1.087f);Keyframe l1 = new Keyframe (800, 1.099f);Keyframe m1 = new Keyframe (900, 1.121f);
		Keyframe n1 = new Keyframe (1000, 1.142f);Keyframe o1 = new Keyframe (1100, 1.155f);Keyframe p1 = new Keyframe (1200, 1.173f);
		Keyframe q1 = new Keyframe (1300, 1.190f);Keyframe r1 = new Keyframe (1400, 1.204f);Keyframe s1 = new Keyframe (1500, 1.216f);
		//PLOT
		pressureFactor.AddKey (a1);pressureFactor.AddKey (b1);pressureFactor.AddKey (c1);pressureFactor.AddKey (d1);pressureFactor.AddKey (e1);pressureFactor.AddKey (f1);
		pressureFactor.AddKey (g1);pressureFactor.AddKey (h1);pressureFactor.AddKey (i1);pressureFactor.AddKey (j1);pressureFactor.AddKey (k1);pressureFactor.AddKey (l1);
		pressureFactor.AddKey (m1);pressureFactor.AddKey (n1);pressureFactor.AddKey (o1);pressureFactor.AddKey (p1);pressureFactor.AddKey (q1);pressureFactor.AddKey (r1);
		pressureFactor.AddKey (s1);
	}
	private void DrawAdiabaticConstant()
	{
		adiabaticFactor = new AnimationCurve ();
		//---------------------------------ADIABATIC CONSTANT
		Keyframe a1 = new Keyframe (250, 1.401f);Keyframe b1 = new Keyframe (300, 1.400f);Keyframe c1 = new Keyframe (350, 1.398f);Keyframe d1 = new Keyframe (400, 1.395f);
		Keyframe e1 = new Keyframe (450, 1.391f);Keyframe f1 = new Keyframe (500, 1.387f);Keyframe g1 = new Keyframe (550, 1.381f);Keyframe h1 = new Keyframe (600, 1.376f);
		Keyframe i1 = new Keyframe (650, 1.370f);Keyframe j1 = new Keyframe (700, 1.364f);Keyframe k1 = new Keyframe (750, 1.359f);Keyframe l1 = new Keyframe (800, 1.354f);
		Keyframe m1 = new Keyframe (900, 1.344f);Keyframe n1 = new Keyframe (1000, 1.336f);Keyframe o1 = new Keyframe (1100, 1.331f);Keyframe p1 = new Keyframe (1200, 1.324f);
		Keyframe q1 = new Keyframe (1300, 1.318f);Keyframe r1 = new Keyframe (1400, 1.313f);Keyframe s1 = new Keyframe (1500, 1.309f);
		//PLOT
		adiabaticFactor.AddKey (a1);adiabaticFactor.AddKey (b1);adiabaticFactor.AddKey (c1);adiabaticFactor.AddKey (d1);adiabaticFactor.AddKey (e1);adiabaticFactor.AddKey (f1);adiabaticFactor.AddKey (g1);adiabaticFactor.AddKey (h1);
		adiabaticFactor.AddKey (i1);adiabaticFactor.AddKey (j1);adiabaticFactor.AddKey (k1);adiabaticFactor.AddKey (l1);adiabaticFactor.AddKey (m1);adiabaticFactor.AddKey (n1);adiabaticFactor.AddKey (o1);adiabaticFactor.AddKey (p1);
		adiabaticFactor.AddKey (q1);adiabaticFactor.AddKey (r1);adiabaticFactor.AddKey (s1);
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (fuelSystem != null && computer != null && connectedAircraft != null) {
			allOk = true;
		} else if (fuelSystem == null) {
			Debug.LogError("Prerequisites not met on Engine "+transform.name + "....Fuel system not attached");
			allOk = false;
		}
		else if (computer == null) {
			Debug.LogError("Prerequisites not met on Engine "+transform.name + "....Core not connected");
			allOk = false;
		}
		else if (connectedAircraft == null) {
			Debug.LogError("Prerequisites not met on Engine "+transform.name + "....Aircraft not connected");
			allOk = false;
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeEngine () {


		//CHECK SYSTEMS
		_checkPrerequisites ();


		if (allOk) {
			//SETUP SOUND SYSTEM
			SoundConfiguration ();
			//SETUP EFFECTS
			baseColor = Color.white;
			if (exhaustSmoke != null) {smokeModule = exhaustSmoke.emission;smokeModule.rateOverTime = 0f;}
			//RESET VALUES
			baseRPM = functionalRPM/10f;DrawPressureFactor ();DrawAdiabaticConstant ();
			actualExhaustDiameter = EngineDiameter * ExhaustDiameterPercentage / 100f;
			actualIntakeDiameter = EngineDiameter * IntakeDiameterPercentage/100f;
			active = false;clutching = false;start = false;stop = false;
			//SET ENGINE JUMP START VALUE;
			if (engineStartMode == EngineStartMode.Hot) {coreAcceleration = 10f;}
			//SET UP ENGINE FUEL COMBUSTION VALUES
			if (fuelType == FuelType.JetB){combustionEnergy = 42.8f;}
			else if (fuelType == FuelType.JetA1) {combustionEnergy = 43.5f;}
			else if (fuelType == FuelType.JP6) {combustionEnergy = 43.02f;} 
			else if (fuelType == FuelType.JP8) {combustionEnergy = 43.28f;}
			combustionEnergy *= 1000f;
			//INTAKE
			if (intakeType == IntakeShape.Circular) {intakeFactor = 0.431f;}
			else if (intakeType == IntakeShape.Oval) {intakeFactor = 0.395f;} 
			else if (intakeType == IntakeShape.Rectangular) {intakeFactor = 0.32f;}
		}
	}




	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	void SoundConfiguration()
	{
		//CREATE A GAMEOBJECT TO ADD SOUND SOURCE TO
		//1. EXTERIOR
		GameObject exteriorSoundPoint = new GameObject();exteriorSoundPoint.transform.parent = this.transform;exteriorSoundPoint.transform.localPosition = new Vector3 (0, 0, 0);exteriorSoundPoint.name = "Exterior Sound Point";
		//SETUP IGNITION
		if (ExteriorIgnitionSound != null) {
			ExteriorIgnitionSource = exteriorSoundPoint.gameObject.AddComponent<AudioSource>();ExteriorIgnitionSource.maxDistance = 650f;ExteriorIgnitionSource.clip = ExteriorIgnitionSound;ExteriorIgnitionSource.loop = false;ExteriorIgnitionSource.dopplerLevel = 0f;//
			ExteriorIgnitionSource.volume = 0.7f;ExteriorIgnitionSource.spatialBlend = 1f;ExteriorIgnitionSource.rolloffMode = AudioRolloffMode.Custom;//Limit sound range
		}
		//SETUP IDLE
		if (ExteriorIdleSound != null) {
			ExteriorIdleSource = exteriorSoundPoint.gameObject.AddComponent<AudioSource>();ExteriorIdleSource.maxDistance = 600f;ExteriorIdleSource.clip = ExteriorIdleSound;ExteriorIdleSource.loop = true;ExteriorIdleSource.Play();ExteriorIdleSource.volume = 0f;
			ExteriorIdleSource.spatialBlend = 1f;ExteriorIdleSource.dopplerLevel = 0f;ExteriorIdleSource.rolloffMode = AudioRolloffMode.Custom;
		}
		//SETUP SHUTDOWN
		if (ExteriorShutdownSound != null) {
			ExteriorShutdownSource = exteriorSoundPoint.gameObject.AddComponent<AudioSource>();ExteriorShutdownSource.maxDistance = 650f;ExteriorShutdownSource.clip = ExteriorShutdownSound;ExteriorShutdownSource.loop = false;ExteriorShutdownSource.volume = 0.7f;
			ExteriorShutdownSource.dopplerLevel = 0f;ExteriorShutdownSource.spatialBlend = 1f;ExteriorShutdownSource.rolloffMode = AudioRolloffMode.Custom;
		}
		//
		if (soundState == SoundState.Available) {
			//2. INTERIOR
			GameObject interiorSoundPoint = new GameObject ();interiorSoundPoint.transform.parent = this.transform;interiorSoundPoint.transform.localPosition = new Vector3 (0, 0, 0);interiorSoundPoint.name = "Interior Sound Point";
			//
			//SETUP IGNITION
			if (InteriorIgnitionSound != null) {
				InteriorIgnitionSource = interiorSoundPoint.gameObject.AddComponent<AudioSource> ();InteriorIgnitionSource.clip = InteriorIgnitionSound;
				InteriorIgnitionSource.loop = false;InteriorIgnitionSource.dopplerLevel = 0f;InteriorIgnitionSource.spatialBlend = 1f;InteriorIgnitionSource.rolloffMode = AudioRolloffMode.Custom;InteriorIgnitionSource.maxDistance = 650f;
			}
			//SETUP IDLE
			if (InteriorIdleSound != null) {
				InteriorIdleSource = interiorSoundPoint.gameObject.AddComponent<AudioSource> ();InteriorIdleSource.clip = InteriorIdleSound;InteriorIdleSource.loop = true;
				InteriorIdleSource.Play ();InteriorIdleSource.volume = 0f;InteriorIdleSource.spatialBlend = 1f;InteriorIdleSource.dopplerLevel = 0f;InteriorIdleSource.rolloffMode = AudioRolloffMode.Custom;InteriorIdleSource.maxDistance = 600f;
			}
			//SETUP SHUTDOWN
			if (InteriorShutdownSound != null) {
				InteriorShutdownSource = interiorSoundPoint.gameObject.AddComponent<AudioSource> ();InteriorShutdownSource.clip = InteriorShutdownSound;InteriorShutdownSource.loop = false;
				InteriorShutdownSource.dopplerLevel = 0f;InteriorShutdownSource.spatialBlend = 1f;InteriorShutdownSource.rolloffMode = AudioRolloffMode.Custom;InteriorShutdownSource.maxDistance = 650f;
			}
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (isControllable) {
			//SEND CALCULATION DATA
			if (corePower > 0f) {EngineCalculation ();}
			//SEND CORE DATA
			CoreEngine();
			//ENGINE STATE CONTROL
			switch (CurrentEngineState) {
			case EngineState.Off:StateOff ();break;
			case EngineState.Starting:StateStart ();break;
			case EngineState.Active:StateActive ();break;
			}

			coreFactor = coreRPM / functionalRPM;
			if (active) {currentEngineVolume = coreFactor + (0.5f * corePower);} else {currentEngineVolume = coreFactor;}
			//ENGINE VOLUMES
			if (currentEngineVolume > 0.0001f && currentEngineVolume < 2f && ExteriorIdleSource != null) {
				if (soundState == SoundState.Available && InteriorIdleSource != null && controller.currentSoundState == SilantroController.SoundState.Interior) { 
					InteriorIdleSource.volume = currentEngineVolume;ExteriorIdleSource.volume = 0f;
				} else {ExteriorIdleSource.volume = currentEngineVolume;if (InteriorIdleSource != null) {InteriorIdleSource.volume = 0f;}}
			}
			//MONITOR OTHER ENGINE SOUND STATES
			if (controller.currentSoundState == SilantroController.SoundState.Exterior && ExteriorIgnitionSource != null && ExteriorShutdownSource != null) {
				ExteriorIgnitionSource.volume = 1f;ExteriorShutdownSource.volume = 1f;
				if (InteriorIdleSource != null && InteriorIgnitionSource != null) {InteriorIgnitionSource.volume = 0f;InteriorShutdownSource.volume = 0f;}
			} 
			else if(controller.currentSoundState == SilantroController.SoundState.Interior && InteriorIgnitionSource != null && InteriorShutdownSource != null)
			{
				ExteriorIgnitionSource.volume = 0f;InteriorIgnitionSource.volume = 1f;ExteriorShutdownSource.volume = 0f;InteriorShutdownSource.volume = 1f;
			}



			//SIMULATE ENGINE FUEL CHOKING
			if (fuelSystem != null && ExteriorIdleSound != null){
				if (fuelSystem.lowFuel) {
					if (active) {
						startRange = 0.6f;endRange = 1.0f;cycleRange = (endRange - startRange) / 2f;offset = cycleRange + startRange;fuelFactor = offset + Mathf.Sin (Time.time * 3f) * cycleRange;
						ExteriorIdleSource.pitch = fuelFactor;if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIdleSource.pitch = fuelFactor;}
					}
				}
				else {ExteriorIdleSource.pitch = currentEnginePitch ;if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIdleSource.pitch = currentEnginePitch ;}}
			} 

			//STOP ENGINE IF FUEL IS EXHAUSTED
			if (fuelSystem != null && fuelSystem.TotalFuelRemaining <= 0) {stop = true;}
			if (active) {coreRPM = Mathf.Lerp (coreRPM, norminalRPM, coreAcceleration * Time.deltaTime *corePower );}
			else{coreRPM = Mathf.Lerp (coreRPM, 0, coreAcceleration * Time.deltaTime);}if (coreRPM < 0) {coreRPM = 0;}


			//CALCULATE ENGINE PITCH
			if (ExteriorIdleSound != null && connectedAircraft != null) {
				float speedFactor = ((coreRPM + (connectedAircraft.velocity.magnitude * 1.943f) + 10f) - baseRPM) / (functionalRPM - baseRPM);
				currentEnginePitch = IdleEnginePitch + ((maximumEnginePitch - IdleEnginePitch) * speedFactor);
				currentEnginePitch = Mathf.Clamp (currentEnginePitch, 0, 2);
			}


			//ENGINE EFFECTS
			if (active == true) {coreValue = corePower;}
			else {coreValue = Mathf.Lerp (coreValue, 0f, 0.04f);}FuelInput = Mathf.Clamp (FuelInput, 0, 1.0f);
			//
			if (exhaustSmoke != null) {emissionCore = maximumEmissionValue * coreFactor;}
			if(exhaustSmoke != null){smokeModule.rateOverTime = emissionCore;}
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (coreRPM <= 0f) {coreRPM = 0f;}
		if (isControllable) {
			//ROTATE ENGINE FAN
			if (IntakePoint) {
				if (rotationDirection == RotationDirection.CCW) {
					if (rotationAxis == RotationAxis.X) {IntakePoint.Rotate (new Vector3 (coreRPM * Time.deltaTime, 0, 0));}
					if (rotationAxis == RotationAxis.Y) {IntakePoint.Rotate (new Vector3 (0, coreRPM * Time.deltaTime, 0));}
					if (rotationAxis == RotationAxis.Z) {IntakePoint.Rotate (new Vector3 (0, 0, coreRPM * Time.deltaTime));}
				}
				//
				if (rotationDirection == RotationDirection.CW) {
					if (rotationAxis == RotationAxis.X) {IntakePoint.Rotate (new Vector3 (-1f * coreRPM * Time.deltaTime, 0, 0));}
					if (rotationAxis == RotationAxis.Y) {IntakePoint.Rotate (new Vector3 (0, -1f * coreRPM * Time.deltaTime, 0));}
					if (rotationAxis == RotationAxis.Z) {IntakePoint.Rotate (new Vector3 (0, 0, -1f * coreRPM * Time.deltaTime));}
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void CoreEngine()
	{
		if (active) {if(corePower < 1f){corePower += Time.deltaTime * coreAcceleration;}}
		else if(corePower > 0f){corePower -= Time.deltaTime * coreAcceleration;}
		if (corePower > 1) {corePower = 1f;}if (!active && corePower < 0) {corePower = 0f;}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENGINE RUN CONFIGURATION
	private void StateActive()
	{
		//STOP IGINITION SOUND IF ITS STILL PLAYING
		if (ExteriorIgnitionSource.isPlaying) {ExteriorIgnitionSource.Stop ();}
		if (InteriorIdleSource != null && soundState == SoundState.Available) {if (InteriorIgnitionSource.isPlaying) {InteriorIgnitionSource.Stop ();}}
		norminalRPM = baseRPM + (functionalRPM - baseRPM) * FuelInput;
		if (stop) {CurrentEngineState = EngineState.Off;active = false;
			ExteriorShutdownSource.Play();if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorShutdownSource.Play ();}
			//RESET
			StartCoroutine(ReturnIgnition());
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//START ENGINE
	private void StateStart()
	{
		if (clutching) {
			if (!ExteriorIgnitionSource.isPlaying) {CurrentEngineState = EngineState.Active;clutching = false;StateActive ();}} 
		else {ExteriorIgnitionSource.Stop();if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIgnitionSource.Stop ();}CurrentEngineState = EngineState.Off;}
		norminalRPM = baseRPM;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//STOP ENGINE
	private void StateOff()
	{
		//STOP IGNITION SOUND IF PLAYING
		if (ExteriorIgnitionSource.isPlaying)
		{
			ExteriorIgnitionSource.Stop();start = false;
			if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIgnitionSource.Stop ();}
		}
		norminalRPM = 0f;
		//START ENGINE PROCEDURE
		if (start) {
			ExteriorIgnitionSource.Play();if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIgnitionSource.Play ();}
			CurrentEngineState = EngineState.Starting;clutching = true;active = true;
			//RESET
			StartCoroutine(ReturnIgnition());
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RESET CONTROL VALUES
	public IEnumerator ReturnIgnition()
	{
		yield return new WaitForSeconds (0.5f);
		start = false;stop = false;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE ENGINE POWER
	private void EngineCalculation()
	{
		//-----------------------------------------------------------ENGINE CASE
		Ae = (Mathf.PI*actualExhaustDiameter*actualExhaustDiameter)/4f;
		Pa = computer.ambientPressure;
		Ta = 273.5f + computer.ambientTemperature;
		Ya = adiabaticFactor.Evaluate (Ta);Cpa = pressureFactor.Evaluate (Ta);
		float speed = connectedAircraft.velocity.magnitude;
		mach = speed/(Mathf.Sqrt(1.4f*287f*Ta));

		//DIFFUSER=====================================================================
		Y2 = Y1 = adiabaticFactor.Evaluate (Ta);Cp1=Cp2 = pressureFactor.Evaluate (Ta);
		T2 = Ta + ((speed * speed) / (2 * (Cp1*1000)));
		float innerP = 1 + ((nd/100f) * ((T2 - Ta) / Ta));
		P2 =P1= Pa * Mathf.Pow (innerP, (Y2 / (Y2 - 1f)));

		//COMPRESSOR==================================================================
		Y3 = adiabaticFactor.Evaluate (T3);Cp3= pressureFactor.Evaluate (T3)*1000f;
		P3 = P2*pressureRatio*coreFactor;
		T3 = T2 * (1 + ((Mathf.Pow ((pressureRatio*coreFactor), ((Y3 - 1) / Y3))) - 1) / (nc/100));
		Wc = (Cp3 *(T3 - T2)) / (nc/100f);

		//BURNER=======================================================================
		Y4 = adiabaticFactor.Evaluate  (T4);Cp4 = pressureFactor.Evaluate (T4)*1000;
		P4= P3*(1-(pcc/100));
		T4 = TurbineTemperature;
		float F1 = (((Cp4/1000) * T4) - ((Cp3/1000) * T3));
		float F2 = (((nb/100)* combustionEnergy) - ((Cp4/1000) * T4));
		f = (F1 / F2)*FuelInput;

		//TURBINE========================================================================
		Y5 =  adiabaticFactor.Evaluate (T5);Cp5 = pressureFactor.Evaluate(T5)*1000f;
		T5 = T4 - ((Cp3 * (T3 - T2)) / (Cp4 * (1 + f)));
		P5 = P4 * (Mathf.Pow ((T5 / T4), (Y5 / (Y5 - 1))));
		float p6_p4 = Mathf.Pow ((Pa / P4), ((Y5 - 1) / Y5));
		Hc = Cp5 * T4 * (1 - p6_p4);

		//NOZZLE========================================================================
		P6 = P5;Pc = P6 / 1.893f;
		T6 = T4/(Mathf.Pow ((P4 / Pa), ((Y5 - 1) / Y5)));
		Y6 = adiabaticFactor.Evaluate   (T6);Cp6 = pressureFactor.Evaluate(T6);


		//EXHAUST VELOCITY
		exhaustVelocity = Mathf.Sqrt (1.4f * 287f * T6)*0.5f;
		PPC = ((P6 *1000)/ (287f * T6));
		Me = PPC * exhaustVelocity * Ae;

		float intakeArea = (3.142f * actualIntakeDiameter * actualIntakeDiameter) / 4f;
		float intakeAirVelocity = (3.142f * actualIntakeDiameter * functionalRPM) / 60f;
		Ma = ((Pa * 1000) / (287f * Ta)) * intakeAirVelocity * intakeArea * intakeFactor;
		Mf = (f) * Ma;
		actualExhaustDiameter = EngineDiameter * ExhaustDiameterPercentage / 100f;
		alpha = 1 - ((Mathf.Pow (speed, 2)) / (2 * (nt / 100) * Hc));
		Wt = (nt / 100) * alpha * Hc;
		Wshaft = (Wt * (nn / 100)) - Wc;
		Pshaft = Ma * Wshaft;
		Tj = Ma*((1+f)*exhaustVelocity-speed);exhaustTemperature = T6 - 273.15f;
		//
		Pt = ((ng/100)*Pshaft) + (Tj/8.5f);
		Pt /= 1000f;
		brakePower = (Pt / 0.7457f)*coreFactor;if(brakePower < 0){brakePower = 0;}

		TSFC = ((Mf * 3600f*2.20462f) / (brakePower));actualConsumptionrate = Mf;
	}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroTurboProp))]
public class PlumSilantroTurboPropEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	[HideInInspector]public int toolbarTab;[HideInInspector]public string currentTab;
	//SOUNDS
	[HideInInspector]public int EngineTab;[HideInInspector]public string currentEngineTab;
	SilantroTurboProp jet;SerializedObject engineObject;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		jet = (SilantroTurboProp)target;
		engineObject = new SerializedObject (jet);
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		engineObject.UpdateIfRequiredOrScript();
		//
		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Identifier", MessageType.None);
		GUI.color = backgroundColor;
		jet.engineIdentifier = EditorGUILayout.TextField (" ", jet.engineIdentifier);
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Properties", MessageType.None);
		GUI.color = backgroundColor;
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		//DISPLAY ENGINE DIMENSIONS
		jet.EngineDiameter = EditorGUILayout.FloatField("Engine Diameter",jet.EngineDiameter);
		GUILayout.Space(2f);
		jet.IntakeDiameterPercentage = EditorGUILayout.Slider ("Intake Ratio",jet.IntakeDiameterPercentage,0,100);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Intake Diameter", jet.IntakeDiameter.ToString ("0.000") + " m");
		GUILayout.Space(2f);
		jet.ExhaustDiameterPercentage = EditorGUILayout.Slider ("Exhaust Ratio",jet.ExhaustDiameterPercentage,0,100);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Exhaust Diameter", jet.ExhaustDiameter.ToString ("0.000") + " m");
		GUILayout.Space(5f);
		jet.intakeType = (SilantroTurboProp.IntakeShape)EditorGUILayout.EnumPopup ("Intake Type", jet.intakeType);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Compressor Fan RPM", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.functionalRPM = EditorGUILayout.FloatField (" ", jet.functionalRPM);
		GUILayout.Space(5f);
		jet.coreAcceleration = EditorGUILayout.Slider ("Core Acceleration",jet.coreAcceleration,0.01f,1f);
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.pressureRatio = EditorGUILayout.FloatField ("Pressure Ratio", jet.pressureRatio);
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Pressure Drop (%)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.pcc = EditorGUILayout.Slider ("Compressor",jet.pcc,0,15);


		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Turbine Inlet Temperature (°K)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.TurbineTemperature = EditorGUILayout.FloatField (" ", jet.TurbineTemperature);
		GUILayout.Space(8f);
		GUI.color = Color.green;
		jet.fuelType = (SilantroTurboProp.FuelType)EditorGUILayout.EnumPopup ("Fuel Type", jet.fuelType);
		GUI.color = backgroundColor;
		if (jet.fuelType == SilantroTurboProp.FuelType.JetB){jet.combustionEnergy = 42.8f;}
		else if (jet.fuelType == SilantroTurboProp.FuelType.JetA1) {jet.combustionEnergy = 43.5f;}
		else if (jet.fuelType == SilantroTurboProp.FuelType.JP6) {jet.combustionEnergy = 49.6f;} 
		else if (jet.fuelType == SilantroTurboProp.FuelType.JP8) {jet.combustionEnergy = 43.28f;}
		jet.combustionEnergy *= 1000f;
		GUILayout.Space(6f);
		EditorGUILayout.LabelField ("Q ", jet.combustionEnergy.ToString ("0.00") + " KJ");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("PSFC ", jet.TSFC.ToString ("0.00") + " lb/hp.hr");

		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Efficiency Configuration (%)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.nd = EditorGUILayout.Slider ("Diffuser",jet.nd,70,90);
		GUILayout.Space(3f);
		jet.nc = EditorGUILayout.Slider ("Compressor",jet.nc,85,90);
		GUILayout.Space(3f);
		jet.nb = EditorGUILayout.Slider ("Burner",jet.nb,97,100);
		GUILayout.Space(3f);
		jet.nt = EditorGUILayout.Slider ("Turbine",jet.nt,90,95);
		GUILayout.Space(3f);
		jet.nn = EditorGUILayout.Slider ("Nozzle",jet.nn,95,98);
		GUILayout.Space(3f);
		jet.ng = EditorGUILayout.Slider ("Gear",jet.ng,90,98);
		GUILayout.Space(5f);
		jet.showConstants = EditorGUILayout.Toggle ("Show Constants", jet.showConstants);
		if (jet.showConstants) {
			GUILayout.Space(3f);
			EditorGUILayout.CurveField ("Gamma Curve", jet.adiabaticFactor);
			GUILayout.Space(3f);
			EditorGUILayout.CurveField ("Pressure Curve", jet.pressureFactor);
		}

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Performance", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.showPerformance = EditorGUILayout.Toggle ("Show", jet.showPerformance);
		if (jet.showPerformance) {
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("AMBIENT", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Pa: " + jet.Pa.ToString ("0.00") + " KPa" + " || Ta: " + jet.Ta.ToString ("0.00") + " °K" + " || Cpa: " + jet.Cpa.ToString ("0.0000") + " || Ya: " + jet.Ya.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("DIFFUSER", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P2: " + jet.P2.ToString ("0.00") + " KPa" + " || T2: " + jet.T2.ToString ("0.00") + " °K" + " || Cp2: " + jet.Cp2.ToString ("0.0000") + " || Y2: " + jet.Y2.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("COMPRESSOR", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P3: " + jet.P3.ToString ("0.00") + " KPa" + " || T3: " + jet.T3.ToString ("0.00") + " °K" + " || Cp3: " + jet.Cp2.ToString ("0.0000") + " || Y3: " + jet.Y3.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("BURNER", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P4: " + jet.P4.ToString ("0.00") + " KPa" + " || T4: " + jet.T4.ToString ("0.00") + " °K" + " || Cp4: " + jet.Cp4.ToString ("0.0000") + " || Y4: " + jet.Y4.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("TURBINE", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P5: " + jet.P5.ToString ("0.00") + " KPa" + " || T5: " + jet.T5.ToString ("0.00") + " °K" + " || Cp5: " + jet.Cp5.ToString ("0.0000") + " || Y5: " + jet.Y5.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("NOZZLE", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P6: " + jet.P6.ToString ("0.00") + " KPa" + " || T6: " + jet.T6.ToString ("0.00") + " °K" + " || Cp6: " + jet.Cp6.ToString ("0.0000") + " || Y6: " + jet.Y6.ToString ("0.0000"));
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Module Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Compressor Work", (jet.Wc/1000).ToString ("0.00") + " kJ/kg");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Turbine Work", (jet.Wt/1000).ToString ("0.00") + " kJ/kg");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Shaft Work", (jet.Wshaft/1000).ToString ("0.00") + " kJ/kg");

			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Exhaust Gas Properties", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Velocity", jet.exhaustVelocity.ToString ("0.00") + " m/s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Temperature (EGT)", jet.exhaustTemperature.ToString ("0.00") + " °C");
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Flows Rates", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Intake Air", jet.Ma.ToString ("0.00") + " kg/s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Exhaust gas", jet.Me.ToString ("0.00") + " kg/s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Fuel", jet.Mf.ToString ("0.00") + " kg/s");
		}

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Connections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.IntakePoint = EditorGUILayout.ObjectField ("Intake Fan", jet.IntakePoint, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		jet.rotationAxis = (SilantroTurboProp.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis",jet.rotationAxis);
		GUILayout.Space(3f);
		jet.rotationDirection = (SilantroTurboProp.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",jet.rotationDirection);
		//
		GUILayout.Space(5f);
		jet.ExhaustPoint = EditorGUILayout.ObjectField ("Exhaust Point", jet.ExhaustPoint, typeof(Transform), true) as Transform;
		//
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		jet.soundState = (SilantroTurboProp.SoundState)EditorGUILayout.EnumPopup("Cabin Sounds", jet.soundState);
		GUILayout.Space(5f);
		if (jet.soundState == SilantroTurboProp.SoundState.Absent) {
			jet.ExteriorIgnitionSound = EditorGUILayout.ObjectField ("Ignition Sound", jet.ExteriorIgnitionSound, typeof(AudioClip), true) as AudioClip;
			GUILayout.Space (2f);
			jet.ExteriorIdleSound = EditorGUILayout.ObjectField ("Engine Idle Sound", jet.ExteriorIdleSound, typeof(AudioClip), true) as AudioClip;
			GUILayout.Space (2f);
			jet.ExteriorShutdownSound = EditorGUILayout.ObjectField ("Shutdown Sound", jet.ExteriorShutdownSound, typeof(AudioClip), true) as AudioClip;
			//
		} else {
			EngineTab = GUILayout.Toolbar (EngineTab, new string[]{ "Exterior Sounds", "Interior Sounds" });
			GUILayout.Space(5f);
			switch (EngineTab) {
			case 0:
				currentEngineTab = "Exterior Sounds";
				break;
			case 1:
				currentEngineTab = "Interior Sounds";
				break;
			}
			switch (currentEngineTab) {
			case "Exterior Sounds":
				jet.ExteriorIgnitionSound = EditorGUILayout.ObjectField ("Exterior Ignition", jet.ExteriorIgnitionSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				jet.ExteriorIdleSound = EditorGUILayout.ObjectField ("Exterior Idle", jet.ExteriorIdleSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				jet.ExteriorShutdownSound = EditorGUILayout.ObjectField ("Exterior Shutdown", jet.ExteriorShutdownSound, typeof(AudioClip), true) as AudioClip;
				//
				break;
			case "Interior Sounds":
				jet.InteriorIgnitionSound = EditorGUILayout.ObjectField ("Interior Ignition", jet.InteriorIgnitionSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				jet.InteriorIdleSound = EditorGUILayout.ObjectField ("Interior Idle", jet.InteriorIdleSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				jet.InteriorShutdownSound = EditorGUILayout.ObjectField ("Interior Shutdown", jet.InteriorShutdownSound, typeof(AudioClip), true) as AudioClip;
				//
				break;
			}
		}
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(8f);
		jet.adjustSoundSettings = EditorGUILayout.Toggle("Show Sound Settings",jet.adjustSoundSettings);
		GUILayout.Space(3f);
		if (jet.adjustSoundSettings) {
			jet.IdleEnginePitch = EditorGUILayout.FloatField ("Base Pitch", jet.IdleEnginePitch);
			GUILayout.Space(2f);
			jet.maximumEnginePitch = EditorGUILayout.FloatField ("Military Pitch", jet.maximumEnginePitch);
		}


		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Effects Configuration", MessageType.None);
		GUI.color = backgroundColor;
		jet.diplaySettings = EditorGUILayout.Toggle ("Show Extras",  jet.diplaySettings);
		if ( jet.diplaySettings) {
			GUILayout.Space (5f);
			jet.exhaustSmoke = EditorGUILayout.ObjectField ("Exhaust Smoke", jet.exhaustSmoke, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (2f);
			jet.maximumEmissionValue = EditorGUILayout.FloatField ("Maximum Emission", jet.maximumEmissionValue);
		}

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Core Power",(jet.corePower*jet.coreFactor*100f).ToString("0.00") + " %");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Brake Power",jet.brakePower.ToString("0.0")+ " Hp");

		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (engineObject.targetObject, "Turbojet Engine Change");}
		if (GUI.changed) {
			EditorUtility.SetDirty (jet);
			EditorSceneManager.MarkSceneDirty (jet.gameObject.scene);
		}
		engineObject.ApplyModifiedProperties();
	}
}
#endif
