﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
public class SilantroTurboFan : MonoBehaviour {

	[HideInInspector]public string engineIdentifier = "Default Engine";
	//------------------------------------------------SELECTORS
	public enum EngineType{Unmixed,Mixed}
	[HideInInspector]public EngineType engineType;
	//CURRENT ENGINE STATE
	public enum EngineState{Off,Starting,Active}
	[HideInInspector]public EngineState CurrentEngineState;
	//START MODE
	public enum EngineStartMode{Cold,Hot}
	[HideInInspector]public EngineStartMode engineStartMode = EngineStartMode.Cold;
	//POWER MODE
	public enum ReheatSystem{Afterburning,noReheat}
	[HideInInspector]public ReheatSystem reheatSystem = ReheatSystem.noReheat;
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
	[HideInInspector]public bool afterburnerOperative;[HideInInspector]public bool isControllable;
	public enum IntakeShape{Rectangular,Circular,Oval}
	[HideInInspector]public IntakeShape intakeType = IntakeShape.Circular;
	float intakeFactor;

	//---------------------------------------------ENGINE DIMENSIONS
	[HideInInspector]public float EngineDiameter = 1f;
	[HideInInspector]public float IntakeDiameterPercentage = 90f;
	[HideInInspector]public float ExhaustDiameterPercentage = 90f;
	//
	[HideInInspector]public float fanExhaustDiameterPercentage = 90f;
	[HideInInspector]public float coreExhaustDiameterPercentage = 90f;
	[HideInInspector]public float IntakeDiameter;
	[HideInInspector]public float actualCED,actualFED;
	[HideInInspector]public float ExhaustDiameter,ExhaustDialation;
	[HideInInspector]public float actualIntakeDiameter,actualExhaustDiameter;[HideInInspector]public bool diplaySettings;

	//---------------------------------------------CONNECTION POINTS
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Transform IntakePoint;
	[HideInInspector]public Transform ExhaustPoint;
	[HideInInspector]public Transform fanExhaustPoint;
	[HideInInspector]public SilantroFuelDistributor fuelSystem;
	[HideInInspector]public SilantroCore computer;
	[HideInInspector]public SilantroController controller;


	//---------------------------------------------ENGINE CORE FACTORS
	float norminalRPM,baseRPM;[HideInInspector]public float functionalRPM = 1000;
	[HideInInspector]public float FuelInput;

	[HideInInspector]public AnimationCurve pressureFactor;
	[HideInInspector]public AnimationCurve adiabaticFactor;

	//-------------------------------------------CASES
	[HideInInspector]
	public float Pa,P1,P2,P3,P4,P5,P6,P7,P8,P9,Pc,P10;
	[HideInInspector]
	public float Ta,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10;
	[HideInInspector]
	public float Ya, Y1, Y2, Y3, Y4, Y5, Y6, Y7,Y8,Y9;
	[HideInInspector]public float Cpa,Cp1,Cp2,Cp3,Cp4,Cp5,Cp6,Cp7,Cp8,Cp9;
	[HideInInspector]public float TurbineTemperature = 1000f,MaximumTemperature = 2000f;
	[HideInInspector]public float np, nth, no,Me,Ma,Mf,PPC;
	[HideInInspector]public float exhaustVelocity,exhaustTemperature;
	[HideInInspector]public float fanExhaustVelocity,coreExhaustVelocity;
	[HideInInspector]public float corePressureRatio = 10f,fanPressureRatio = 2f, combustionEnergy;//[HideInInspector]
	[HideInInspector]public float f,fab,Ae;
	[HideInInspector]public float Aeb,drawDiameter,cAe,fAe;
	//EFFICIENCIES
	[HideInInspector]public float pcc = 6f,pcab = 3f,nf = 90f,nd = 92f,nc = 95f,nb = 98f,nthpt = 95f,ntlpt = 97f,nab = 92f,nn = 96f,bypassRatio = 5;
	[HideInInspector]public float CoreThrust,PressureThrust,EngineThrust;
	[HideInInspector]public float TSFC;[HideInInspector]public float actualConsumptionrate;

	//-------------------------------------------ENGINE EFFECTS
	[HideInInspector]public ParticleSystem exhaustSmoke;
	ParticleSystem.EmissionModule smokeModule;Color baseColor;Color finalColor;
	[HideInInspector]public float maximumEmissionValue = 50f;
	[HideInInspector]public bool useEmissionMaterial;
	[HideInInspector]public Material engineMaterial;[HideInInspector]public Material afterburnerTubeMaterial;[HideInInspector]public float emissionCore;
	[HideInInspector]public float maximumNormalEmission = 1f;[HideInInspector]public float maximumAfterburnerEmission = 2f;
	[HideInInspector]public float pc_p8;
	[HideInInspector]public SilantroHydraulicSystem reverseBuckets;
	public enum ReverseThrust{Available,NotAvailable}
	[HideInInspector]public ReverseThrust reverseThrust = ReverseThrust.NotAvailable;
	[HideInInspector]public bool canReverseThrust;
	[HideInInspector]public bool reverseThrustEngaged;
	float ReverseForce;float controlVariable;

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
	[HideInInspector]public float currentEnginePitch,maximumEnginePitch = 0.9f,IdleEnginePitch= 0.5f,afterburnerPitch = 1.7f;
	[HideInInspector]public float currentEngineVolume ,maximumEngineVolume;

	//---------------------------------------------ENGINE VARIABLES
	float startRange;float endRange;float cycleRange;float offset,fuelFactor;
	[HideInInspector]public float corePower,coreAcceleration = 0.25f,coreValue,coreFactor,value;
	[HideInInspector]public float coreRPM;

	//---------------------------------------------CONTROL BOOLS
	[HideInInspector]public bool start,stop,clutching,active,useEmission;






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
	//AFTERBURNER CONTROL
	public void ToggleAfterburner()
	{
		if(reheatSystem == ReheatSystem.Afterburning && corePower > 0.5f && FuelInput > 0.5f && liftFactor > 0.3f) {afterburnerOperative = !afterburnerOperative;}
	}


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
		ExhaustDiameter = EngineDiameter * (ExhaustDiameterPercentage/100f);
		if (!afterburnerOperative) {drawDiameter = ExhaustDiameter;}else {drawDiameter = ExhaustDialation;}
		//
		if(engineType == EngineType.Mixed){
		Handles.color = Color.red;if(ExhaustPoint != null){Handles.DrawWireDisc (ExhaustPoint.position, ExhaustPoint.transform.forward, (drawDiameter/2f));}
		}
		if (engineType == EngineType.Unmixed) {
			Handles.color = Color.cyan;if(fanExhaustPoint != null){Handles.DrawWireDisc (fanExhaustPoint.position, fanExhaustPoint.transform.forward, (actualFED/2f));}
			Handles.color = Color.red;if(ExhaustPoint != null){Handles.DrawWireDisc (ExhaustPoint.position, ExhaustPoint.transform.forward, (actualCED/2f));}
		}
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
			EffectsInitial ();
			//RESET VALUES
			baseRPM = functionalRPM/10f;DrawPressureFactor ();DrawAdiabaticConstant ();
			actualExhaustDiameter = EngineDiameter * ExhaustDiameterPercentage / 100f;
			actualIntakeDiameter = EngineDiameter * IntakeDiameterPercentage/100f;
			actualCED = EngineDiameter*(coreExhaustDiameterPercentage/100f);
			actualFED = EngineDiameter * (fanExhaustDiameterPercentage / 100f);
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
			if (reverseThrust == ReverseThrust.Available) {canReverseThrust = true;} 
			else {canReverseThrust = false;}reverseThrustEngaged = false;
		}
	}





	// ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	//SETUP EFFECTS
	void EffectsInitial()
	{
		baseColor = Color.white;
		if (exhaustSmoke != null) {smokeModule = exhaustSmoke.emission;smokeModule.rateOverTime = 0f;}
		if (engineMaterial != null) {finalColor = baseColor * Mathf.LinearToGammaSpace (0.0f);engineMaterial.SetColor ("_EmissionColor", finalColor);}
		if (afterburnerTubeMaterial != null) {afterburnerTubeMaterial.SetColor("_EmissionColor", finalColor);}
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
	//TOGGLE REVERSE THRUST
	public void InitiateReverseControl()
	{
		if( canReverseThrust && corePower > 0.5f) {
			reverseThrustEngaged = !reverseThrustEngaged;
			if (reverseBuckets) {
				if (reverseBuckets.currentState == SilantroHydraulicSystem.CurrentState.Closed) {
				reverseBuckets.open = true;} 
				else {reverseBuckets.close = true;}
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
			if(ExteriorIdleSound){
			switch (CurrentEngineState) {
			case EngineState.Off:StateOff ();break;
			case EngineState.Starting:StateStart ();break;
			case EngineState.Active:StateActive ();break;
				}
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
				if (afterburnerOperative) {currentEnginePitch = IdleEnginePitch + (afterburnerPitch - IdleEnginePitch) * speedFactor;} 
				else {currentEnginePitch = IdleEnginePitch + ((maximumEnginePitch - IdleEnginePitch) * speedFactor);}
				currentEnginePitch = Mathf.Clamp (currentEnginePitch, 0, 2);
			}


			//ENGINE EFFECTS
			if (active == true) {coreValue = corePower;}
			else {coreValue = Mathf.Lerp (coreValue, 0f, 0.04f);}FuelInput = Mathf.Clamp (FuelInput, 0, 1.0f);
			//
			if (afterburnerOperative) {value = maximumAfterburnerEmission;if (exhaustSmoke != null) {emissionCore = 1.5f * maximumEmissionValue * corePower * coreFactor;}}
			else {value = Mathf.Lerp (value, maximumNormalEmission, 0.02f);if (exhaustSmoke != null) {emissionCore = maximumEmissionValue * coreFactor;}}float actualValue = (coreValue) * value * coreFactor;
			if (engineMaterial != null) {finalColor = baseColor * Mathf.LinearToGammaSpace (actualValue);engineMaterial.SetColor ("_EmissionColor", finalColor);}
			if (afterburnerTubeMaterial != null) {afterburnerTubeMaterial.SetColor("_EmissionColor", finalColor);}if(exhaustSmoke != null){smokeModule.rateOverTime = emissionCore;}
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
			//APPLY GENERATED FORCE
			if (EngineThrust > 0f && connectedAircraft != null && ExhaustPoint != null) {
				//REVERSE THRUST
				if (canReverseThrust&&reverseThrustEngaged) {
					ReverseForce = (FC+FP) * controlVariable*liftFactor;
					Vector3 rforce = fanExhaustPoint.forward * ReverseForce * -1f;
					connectedAircraft.AddForce (rforce, ForceMode.Force);
				} else {
					Vector3 force = ExhaustPoint.forward * EngineThrust*liftFactor;
					connectedAircraft.AddForce (force, ForceMode.Force);
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


	[HideInInspector]public float mach,Aea,Wc,p8_pa,xf,p8_pc,liftFactor = 1f;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE ENGINE THRUST
	private void EngineCalculation()
	{
		//-----------------------------------------------------------ENGINE CASE
		Ae = (Mathf.PI*actualExhaustDiameter*actualExhaustDiameter)/4f;Aeb=Ae;
		if(engineType == EngineType.Unmixed){
			cAe = (Mathf.PI*actualCED*actualCED)/4f;
			fAe = ((Mathf.PI*actualFED*actualFED)/4f)-(cAe/0.895f);
		}
		Pa = computer.ambientPressure;
		Ta = 273.5f + computer.ambientTemperature;
		Ya = adiabaticFactor.Evaluate (Ta);Cpa = pressureFactor.Evaluate (Ta);
		float speed = connectedAircraft.velocity.magnitude;mach = computer.machSpeed;
		float throttleFactor = (coreFactor * FuelInput+0.01f);
		//
		//1. MIXING TYPE
		if(engineType == EngineType.Mixed){
		//DIFFUSER=====================================================================
		Cp1=Cp2 = pressureFactor.Evaluate (Ta);
		float t2_ta =((Y2-1)/2)*mach*mach;
		//T2=T1 = Ta * (1 + t2_ta);
		T2 = Ta + ((speed * speed) / (2 * (Cp1*1000)));
		Y2 = Y1 = adiabaticFactor.Evaluate (T2);
		float innerP = 1 + ((nd/100f) * ((Y2-1)/2)*mach*mach);
		P2 =P1= Pa * Mathf.Pow (innerP, (Y2 / (Y2 - 1f)));


		//FAN=======================================================================
		xf = (0.025f*speed)+0.833f;
		P3 = P2*(fanPressureRatio);
		float piF = Mathf.Pow ((fanPressureRatio), ((Y2 - 1) / Y2));
		T3 = T2 * (1 + ((piF-1)/(nf/100)));
		Y3 = adiabaticFactor.Evaluate (T3);Cp3= pressureFactor.Evaluate (T3)*1000f;

		//COMPRESSOR=================================================================
		P4 = P3*(corePressureRatio);
		float t4_t3 = Mathf.Pow ((corePressureRatio), ((Y3 - 1) / Y3));
		T4 = T3 * (1 + ((t4_t3-1)/(nc/100)));
		Y4 = adiabaticFactor.Evaluate  (T4);Cp4 = pressureFactor.Evaluate (T4)*1000;

		//BURNER=======================================================================
		P5 = P4*(1-(pcc/100));
		T5 = TurbineTemperature;
		Y5 =  adiabaticFactor.Evaluate (T5);Cp5 = pressureFactor.Evaluate(T5)*1000f;
		float F1 = (((Cp5/1000) * T5) - ((Cp4/1000) * T4));
		float F2 = (((nb/100)* combustionEnergy) - ((Cp5/1000) * T5));
		f = (F1 / F2)*FuelInput;

		//LOW PRESSURE TURBINE===========================================================
		float p6A = Cp4 * (T4 - T3);
		float p6B = Cp5 * (1 + f);
		T6 = T5 - (p6A / p6B);
		float p6_p5 = 1-(1 - (T6 / T5))*(1/(ntlpt/100));
		P6 = P5 * Mathf.Pow (p6_p5, (Y5 / (Y5 - 1)));

		//HIGH PRESSURE TURBINE================================================================
		Y6 =  adiabaticFactor.Evaluate (T6);Cp6 = pressureFactor.Evaluate(T6)*1000f;
		P7 = P3;
		float p7_p6 = Mathf.Pow ((P7 / P6), ((Y6 - 1) / Y6));
		T7 = T6*(1 - ((nthpt / 100) * (1 - p7_p6)));
		Wc = ((Cp6 *(T6 - T7)) / (nthpt/100f)*Ma)/745f;


		//MIXING NOZZLE=======================================================================
		if (T7 > 0 && T7 < 10000) {
			Y7 = adiabaticFactor.Evaluate (T7);Cp7 = pressureFactor.Evaluate (T7) * 1000f;
		}
		P8 = P3;
		float t8A = (bypassRatio * Cp6 * T3) + ((1 + f) * Cp7 * T7);
		float t8B = ((1 + f) + bypassRatio) * Cp7;
		T8 = t8A / t8B;
		//NOZZLE========================================================================
		if (T8 > 0 && T8 < 10000) {Y8 =  adiabaticFactor.Evaluate (T8);Cp8 = pressureFactor.Evaluate(T8)*1000f;}
		float pcc8 = 1/(1-(1 / (nn / 100)) * ((Y8 - 1) / (Y8 + 1)));
		pc_p8 = Mathf.Pow(pcc8,(Y8/(Y8-1)));
		Pc = P8 / (pc_p8);

		//CHECK IF CHOCKED
		p8_pa = P8/Pa;
		p8_pc = P8 / Pc;
		float superMe = 0f;
			if (p8_pa > p8_pc) {
				//CHOCKED NOZZLE
				P9 = Pc;
				T9 = T8 / ((Y8 + 1) / 2);
				float p9 = (P9 * 1000) / (287 * T9);
				float Y9 = adiabaticFactor.Evaluate (T9);
				//
				exhaustVelocity = Mathf.Sqrt (Y9 * 287 * T9);
				Me = p9 * exhaustVelocity * Ae;
				superMe = Me;
				Aea = (Ma * (1 + f)) / (p9 * exhaustVelocity);
			} else {
				P9 = Pa;
				float a = (Y8 - 1) / Y8;
				T9 = T8 / ((Y8 + 1) / 2);//T9 = T8 * Mathf.Pow ((P8 / Pa), a);
				exhaustVelocity = Mathf.Sqrt (2f*Cp8*(T8-T9));
				Me = P9 * exhaustVelocity * Ae;
				superMe = Me;
				Aea = (Ma * (1 + f)) / (P9 * exhaustVelocity);
			}
		exhaustVelocity *= coreFactor;
		exhaustTemperature = (T8 - 273.15f)*coreFactor;
	
		float intakeArea = (3.142f * actualIntakeDiameter * actualIntakeDiameter) / 4f;
		float intakeAirVelocity = (3.142f * actualIntakeDiameter * functionalRPM) / 60f;
		Ma = ((Pa*1000) / (287f * Ta)) * intakeAirVelocity * intakeArea*intakeFactor;
		Mf = (f) * Ma;
		actualExhaustDiameter = EngineDiameter * ExhaustDiameterPercentage / 100f;

		//AFTERBURNER PIPE======================================//OPERATIONAL AFTERBURNER
		if (afterburnerOperative) {
			P10 = (1-(pcab/100))*(P6/(1.905f*pc_p8));
			T10 = MaximumTemperature;
			Y9 =  adiabaticFactor.Evaluate (T10);Cp9 = pressureFactor.Evaluate(T10)*1000f;
			fab = (((Cp8/1000) * (T10 - T7)) / (((nab / 100) * combustionEnergy) - ((Cp8/1000) * T10)));
			float a = (Y9 +1)  / 2;
			float t11 = T10 / a;
			exhaustVelocity = Mathf.Sqrt (2f*Cp9*(T10-t11));
			exhaustTemperature = ( t11- 273.15f)*coreFactor;
			//CALCULATE EXIT AREA
			//Aeb = (287f*t11*Ma*(1+f+fab))/((P10/pcc8)*1000*exhaustVelocity);
			float Acf = (287f*t11*Ma*(1+f+fab))/((P8/pc_p8)*1000*exhaustVelocity);
			if (Acf > 0) {ExhaustDialation = Mathf.Sqrt ((Acf * 4f) / (3.142f));}
			if (ExhaustDialation<ExhaustDiameter) {ExhaustDialation = ExhaustDiameter;}
			Mf = Ma * (f + fab);
			//CALCULATE EXHAUST MASS FLOW
			float p9 = (P9 * 1000) / (287 * (T10 / ((Y8 + 1) / 2f)));
			Me = (p9 * exhaustVelocity * Aeb);
		}

		//======================================================================
		if (!afterburnerOperative) {
			//THRUST
			CoreThrust = (((1 + f) * exhaustVelocity) - (speed)) * Ma;
			PressureThrust = (Ae * (((P6/(1.905f*pc_p8)) - Pa) * 1000));
				EngineThrust = (CoreThrust + PressureThrust)*throttleFactor;
		} else {
			CoreThrust = (((1 + f+fab) * exhaustVelocity) - (speed)) * Ma;
			PressureThrust = (Aeb * ((P10- Pa) * 1000));
				EngineThrust = (CoreThrust + PressureThrust)*throttleFactor;;
			}
		}

		//----------------------------------------------------------------------------------------------
		if (reverseThrustEngaged) {controlVariable = Mathf.Lerp (controlVariable, 1f, 0.02f);} 
		else {controlVariable = Mathf.Lerp (controlVariable, 0f, 0.02f);}
		if (controlVariable < 0) {controlVariable = 0;}


		//2. UNMIXED
		if (engineType == EngineType.Unmixed) {
			
			//DIFFUSER=====================================================================
			Cp1=Cp2 = pressureFactor.Evaluate (Ta);
			float t2_ta =((Y2-1)/2)*mach*mach;
			//T2=T1 = Ta * (1 + t2_ta);
			T2 = Ta + ((speed * speed) / (2 * (Cp1*1000)));
			Y2 = Y1 = adiabaticFactor.Evaluate (T2);
			float innerP = 1 + ((nd/100f) * ((Y2-1)/2)*mach*mach);
			P2 =P1= Pa * Mathf.Pow (innerP, (Y2 / (Y2 - 1f)));

			float intakeArea = (3.142f * actualIntakeDiameter * actualIntakeDiameter) / 4f;
			float intakeAirVelocity = (3.142f * actualIntakeDiameter * functionalRPM) / 60f;
			Ma = ((Pa*1000) / (287f * Ta)) * intakeAirVelocity * intakeArea*intakeFactor;
			mh = Ma/(1+bypassRatio);
			Mf = (f) * mh;

			//FAN=======================================================================
			P3 = P2*(fanPressureRatio);
			float piF = Mathf.Pow ((fanPressureRatio), ((Y2 - 1) / Y2));
			T3 = T2 * (1 + ((piF-1)/(nf/100)));
			Y3 = adiabaticFactor.Evaluate (T3);Cp3= pressureFactor.Evaluate (T3)*1000f;
			pcc3a = 1/(1-(1 / (nn / 100)) * ((Y3 - 1) / (Y3 + 1)));
			pcc3b = Mathf.Pow(pcc3a,(Y3/(Y3-1)));
			//FAN NOZZLE=======================================================================
			Pc = P3 / pcc3b;
			if (Pc > Pa) {
				P9 = Pc;
				T9 = T3 / pcc3a;
				float p9 = (P9 * 1000) / (287 * T9);
				fanExhaustVelocity = Mathf.Sqrt (Y3 * 287f * T9);
				FC = bypassRatio*mh*(fanExhaustVelocity-speed);
				FP = fAe * (P9 - Pa) * 1000f;
			} else {
				P9 = Pa;
				float p9c = 1-Mathf.Pow((P9/P3),((Y3-1)/Y3));
				T9 = T3 * (1 - ((nn / 100) * p9c));
				float p9 = (P9 * 1000) / (287 * T9);
				fanExhaustVelocity = Mathf.Sqrt (Y3 * 287f * T9);
				FC = bypassRatio*mh*(fanExhaustVelocity-speed);
			}


			//COMPRESSOR=================================================================
			P4 = P3*(corePressureRatio);
			float t4_t3 = Mathf.Pow ((corePressureRatio), ((Y3 - 1) / Y3));
			T4 = T3 * (1 + ((t4_t3-1)/(nc/100)));
			Y4 = adiabaticFactor.Evaluate  (T4);Cp4 = pressureFactor.Evaluate (T4)*1000;

			//BURNER=======================================================================
			P5 = P4*(1-(pcc/100));
			T5 = TurbineTemperature;
			Y5 =  adiabaticFactor.Evaluate (T5);Cp5 = pressureFactor.Evaluate(T5)*1000f;
			float F1 = (((Cp5/1000) * T5) - ((Cp4/1000) * T3));
			float F2 = (((nb/100)* combustionEnergy) - ((Cp5/1000) * T5));
			f = (F1 / F2)*FuelInput;

			//HIGH PRESSURE TURBINE================================================================
			T6 = T5 -((Cp4*(T4-T3))/((1+f)*Cp5));
			Y6 =  adiabaticFactor.Evaluate (T6);Cp6 = pressureFactor.Evaluate(T6)*1000f;
			float p6c = 1 - ((1 / (nthpt / 100)) * (1 - (T6 / T5)));
			p6cp5 = Mathf.Pow (p6c, (Y6 / (Y6 - 1)));
			P6 = P5 * p6cp5;

			//LOW PRESSURE TURBINE================================================================
			float t7_t6 = ((1+bypassRatio)*Cp6*(T3-T2))/((1+f)*Cp5);
			T7 = T6 - t7_t6;
			Y7 =  adiabaticFactor.Evaluate (T7);Cp7 = pressureFactor.Evaluate(T7)*1000f;
			p7c = 1 - ((1 / (ntlpt / 100)) * (1 - (T7 / T6)));
			p7cp6 = Mathf.Pow (p7c, (Y7 / (Y7 - 1)));
			P7 = P6 * p7cp6;
			Wc = ((Cp6 *(T6 - T7)) / (nthpt/100f)*mh)/745f;

			//HOT NOZZLE
			pcc8a = 1/(1-(1 / (nn / 100)) * ((Y7 - 1) / (Y7 + 1)));
			pcc8b = Mathf.Pow(pcc8a,(Y7/(Y7-1)));
			float P8c = P7 / pcc8b;
			//
			if (P8c > Pa) {
				P8 = P8c;
				T8 = T7 / pcc8a;
				float p88 = (P8 * 1000) / (287f * T8);
				coreExhaustVelocity = Mathf.Sqrt (1.33f * 287 * T8);
				CC = mh * ((1 + f) * coreExhaustVelocity - speed);
				CP = cAe * (P8 - Pa) * 1000f;
			} else {
				P8 = Pa;
				float p9c = 1-Mathf.Pow((P8/P7),((Y7-1)/Y7));
				T8 = T7 * (1 - ((nn / 100) * p9c));
				coreExhaustVelocity = Mathf.Sqrt (1.33f * 287 * T8);
				CC = mh * ((1 + f) * coreExhaustVelocity - speed);
			}
			Y8 =  adiabaticFactor.Evaluate (T8);Cp8 = pressureFactor.Evaluate(T8)*1000f;
			//
			CoreThrust = FC+CC;
			PressureThrust = FP+CP;
			EngineThrust = (CoreThrust + PressureThrust)*throttleFactor;
		}

		if (reverseThrustEngaged && canReverseThrust) {EngineThrust = EngineThrust - ReverseForce;} 

		if(EngineThrust < 0){EngineThrust = 0;}
		//--------------------------------------------EFFICIENCIES
		//1. PROPULSIVE
		if (engineType == EngineType.Mixed) {
			np = (2 * (speed + 1)) / ((speed + 1) + exhaustVelocity);
		} else {
			np = (2 * (speed + 1)) / ((speed + 1) + coreExhaustVelocity);
		}

		//2. THERMAL
		float nthTop = (EngineThrust*(speed+1))+(0.5f*Ma*(1+f)*Mathf.Pow((exhaustVelocity-speed),2));
		float nthBot = Mf * combustionEnergy *1000;
		nth = nthTop / nthBot;

		//3. OVERALL
		no = nth*np;
		//
		nth *= 100f;
		np *= 100f;
		no *= 100f;
		//
		if (EngineThrust > 0) {
			float pt = EngineThrust * 0.2248f;
			TSFC = ((Mf * 3600f) / (pt * 0.4536f));
			actualConsumptionrate = Mf;
		}
		if (afterburnerOperative && FuelInput < 0.5f) {afterburnerOperative = false;}
	}
	[HideInInspector]public float pcc3a,pcc3b,p6cp5,p7c,p7cp6,pcc8a,pcc8b,mh,FC,FP,CC,CP;
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroTurboFan))]
public class SilantroTurboFanEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	[HideInInspector]public int toolbarTab;[HideInInspector]public string currentTab;
	//SOUNDS
	[HideInInspector]public int EngineTab;[HideInInspector]public string currentEngineTab;
	SilantroTurboFan jet;SerializedObject engineObject;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		jet = (SilantroTurboFan)target;
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
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Engine Class", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.engineType = (SilantroTurboFan.EngineType)EditorGUILayout.EnumPopup(" ",jet.engineType);
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
		if (jet.engineType == SilantroTurboFan.EngineType.Mixed) {
			GUILayout.Space (2f);
			jet.ExhaustDiameterPercentage = EditorGUILayout.Slider ("Exhaust Ratio", jet.ExhaustDiameterPercentage, 0, 100);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Exhaust Diameter", jet.ExhaustDiameter.ToString ("0.000") + " m");
		}
		if (jet.engineType == SilantroTurboFan.EngineType.Unmixed) {
			GUILayout.Space (3f);
			jet.fanExhaustDiameterPercentage = EditorGUILayout.Slider ("Fan Exhaust Ratio", jet.fanExhaustDiameterPercentage, 0, 100);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Fan Exhaust Diameter", jet.actualFED.ToString ("0.000") + " m");
			GUILayout.Space (5f);
			jet.coreExhaustDiameterPercentage = EditorGUILayout.Slider ("Core Exhaust Ratio", jet.coreExhaustDiameterPercentage, 0, 100);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Core Exhaust Diameter", jet.actualCED.ToString ("0.000") + " m");

		}
		GUILayout.Space(8f);
		jet.intakeType = (SilantroTurboFan.IntakeShape)EditorGUILayout.EnumPopup ("Intake Type", jet.intakeType);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Compressor Fan RPM", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.functionalRPM = EditorGUILayout.FloatField (" ", jet.functionalRPM);
		GUILayout.Space(5f);
		jet.coreAcceleration = EditorGUILayout.Slider ("Core Acceleration",jet.coreAcceleration,0.01f,1f);
		if (jet.engineType == SilantroTurboFan.EngineType.Mixed) {
			GUILayout.Space (10f);
			jet.reheatSystem = (SilantroTurboFan.ReheatSystem)EditorGUILayout.EnumPopup ("Reheat System", jet.reheatSystem);
		}
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.bypassRatio = EditorGUILayout.FloatField ("By-Pass Ratio", jet.bypassRatio);
		GUILayout.Space(5f);
		jet.corePressureRatio = EditorGUILayout.FloatField ("Core Pressure Ratio", jet.corePressureRatio);
		GUILayout.Space(5f);
		jet.fanPressureRatio = EditorGUILayout.Slider ("Fan Pressure Ratio", jet.fanPressureRatio,0f,5f);
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Pressure Drop (%)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.pcc = EditorGUILayout.Slider ("Compressor",jet.pcc,0,15);
		if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning) {
			GUILayout.Space (3f);
			jet.pcab = EditorGUILayout.Slider ("Afterburner Pipe",jet.pcab,0,15);
		}
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Turbine Inlet Temperature (°K)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.TurbineTemperature = EditorGUILayout.FloatField (" ", jet.TurbineTemperature);
		if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning) {
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Maximum Engine Temperature (°K)", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			jet.MaximumTemperature = EditorGUILayout.FloatField (" ", jet.MaximumTemperature);
		}

		GUILayout.Space(8f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Fuel Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		GUI.color = Color.green;
		jet.fuelType = (SilantroTurboFan.FuelType)EditorGUILayout.EnumPopup ("Fuel Type", jet.fuelType);
		GUI.color = backgroundColor;
		if (jet.fuelType == SilantroTurboFan.FuelType.JetB){jet.combustionEnergy = 42.8f;}
		else if (jet.fuelType == SilantroTurboFan.FuelType.JetA1) {jet.combustionEnergy = 43.5f;}
		else if (jet.fuelType == SilantroTurboFan.FuelType.JP6) {jet.combustionEnergy = 49.6f;} 
		else if (jet.fuelType == SilantroTurboFan.FuelType.JP8) {jet.combustionEnergy = 43.28f;}
		jet.combustionEnergy *= 1000f;
		GUILayout.Space(6f);
		EditorGUILayout.LabelField ("Q ", jet.combustionEnergy.ToString ("0.00") + " KJ");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("TSFC ", jet.TSFC.ToString ("0.00") + " lb/lbf.hr");

		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Efficiency Configuration (%)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.nd = EditorGUILayout.Slider ("Diffuser",jet.nd,70,95);
		GUILayout.Space(3f);
		jet.nf = EditorGUILayout.Slider ("Fan",jet.nf,90,99);
		GUILayout.Space(3f);
		jet.nc = EditorGUILayout.Slider ("Compressor",jet.nc,85,99);
		GUILayout.Space(3f);
		jet.nb = EditorGUILayout.Slider ("Burner",jet.nb,97,100);
		GUILayout.Space(3f);
		jet.nthpt = EditorGUILayout.Slider ("Turbine (HPT)",jet.nthpt,90,98);
		GUILayout.Space(3f);
		jet.ntlpt = EditorGUILayout.Slider ("Turbine (LPT)",jet.ntlpt,90,99);
		GUILayout.Space(3f);
		jet.nn = EditorGUILayout.Slider ("Nozzle",jet.nn,95,98);
		if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning) {
			GUILayout.Space (3f);
			jet.nab = EditorGUILayout.Slider ("Afterburner", jet.nab, 90, 98);
		}
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
			EditorGUILayout.HelpBox ("FAN", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P3: " + jet.P3.ToString ("0.00") + " KPa" + " || T3: " + jet.T3.ToString ("0.00") + " °K" + " || Cp3: " + jet.Cp3.ToString ("0.0000") + " || Y5: " + jet.Y3.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("COMPRESSOR", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P4: " + jet.P4.ToString ("0.00") + " KPa" + " || T4: " + jet.T4.ToString ("0.00") + " °K" + " || Cp4: " + jet.Cp4.ToString ("0.0000") + " || Y4: " + jet.Y4.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("BURNER", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P5: " + jet.P5.ToString ("0.00") + " KPa" + " || T5: " + jet.T5.ToString ("0.00") + " °K" + " || Cp5: " + jet.Cp5.ToString ("0.0000") + " || Y5: " + jet.Y5.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("HIGH PRESSURE TURBINE", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P6: " + jet.P6.ToString ("0.00") + " KPa" + " || T6: " + jet.T6.ToString ("0.00") + " °K" + " || Cp6: " + jet.Cp6.ToString ("0.0000") + " || Y6: " + jet.Y6.ToString ("0.0000"));
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox (" LOW PRESSURE TURBINE", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("P7: " + jet.P7.ToString ("0.00") + " KPa" + " || T7: " + jet.T7.ToString ("0.00") + " °K" + " || Cp7: " + jet.Cp7.ToString ("0.0000") + " || Y7: " + jet.Y7.ToString ("0.0000"));
			if (!jet.afterburnerOperative) {
				GUILayout.Space (3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("NOZZLE", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("P8: " + jet.P8.ToString ("0.00") + " KPa" + " || T8: " + jet.T8.ToString ("0.00") + " °K" + " || Cp8: " + jet.Cp8.ToString ("0.0000") + " || Y8: " + jet.Y8.ToString ("0.0000"));
			} else {
				GUILayout.Space (3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("AFTERBURER PIPE", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("P9: " + jet.P10.ToString ("0.00") + " KPa" + " || T9: " + jet.T10.ToString ("0.00") + " °K" + " || Cp9: " + jet.Cp9.ToString ("0.0000") + " || Y9: " + jet.Y9.ToString ("0.0000"));
				GUILayout.Space (3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("NOZZLE", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				float t10_t9 = (jet.T10 / ((jet.Y9 + 1) / 2));
				EditorGUILayout.LabelField ("P10: " + (jet.P8/jet.pc_p8).ToString ("0.00") + " KPa" + " || T10: " + t10_t9.ToString ("0.00") + " °K" + " || Cp10: " + jet.pressureFactor.Evaluate(t10_t9).ToString ("0.0000") + " || Y10: " + jet.adiabaticFactor.Evaluate(t10_t9).ToString ("0.0000"));
			}
			GUILayout.Space(10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Exhaust Gas Properties", MessageType.None);
			GUI.color = backgroundColor;
			if (jet.engineType == SilantroTurboFan.EngineType.Mixed) {
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("Velocity", jet.exhaustVelocity.ToString ("0.00") + " m/s");
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Temperature (EGT)", jet.exhaustTemperature.ToString ("0.00") + " °C");
			} else {
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("Fan Exhaust", jet.fanExhaustVelocity.ToString ("0.00") + " m/s");
				GUILayout.Space (3f);
				float fEGT = jet.T9 - 273.15f;float cEGT = jet.T8 - 273.15f;
				EditorGUILayout.LabelField ("Fan (EGT)", fEGT.ToString ("0.00") + " °C");
				//
				GUILayout.Space (5f);
				EditorGUILayout.LabelField ("Core Exhaust", jet.coreExhaustVelocity.ToString ("0.00") + " m/s");
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Core (EGT)", cEGT.ToString ("0.00") + " °C");
			}
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Flows Rates", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Intake Air", jet.Ma.ToString ("0.00") + " kg/s");
			if (jet.engineType == SilantroTurboFan.EngineType.Mixed) {
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Exhaust gas", jet.Me.ToString ("0.00") + " kg/s");
			}
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Fuel", jet.Mf.ToString ("0.00") + " kg/s");
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Efficieny Ratings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Propulsive", jet.np.ToString ("0.00") + " %");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Thermal", jet.nth.ToString ("0.00") + " %");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Overall", jet.no.ToString ("0.00") + " %");
		}

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Connections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.IntakePoint = EditorGUILayout.ObjectField ("Intake Fan", jet.IntakePoint, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		jet.rotationAxis = (SilantroTurboFan.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis",jet.rotationAxis);
		GUILayout.Space(3f);
		jet.rotationDirection = (SilantroTurboFan.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",jet.rotationDirection);
		//
		if (jet.engineType == SilantroTurboFan.EngineType.Mixed) {
			GUILayout.Space (5f);
			jet.ExhaustPoint = EditorGUILayout.ObjectField ("Exhaust Point", jet.ExhaustPoint, typeof(Transform), true) as Transform;
		}
		if (jet.engineType == SilantroTurboFan.EngineType.Unmixed) {
			GUILayout.Space (5f);
			jet.ExhaustPoint = EditorGUILayout.ObjectField ("Core Exhaust Point", jet.ExhaustPoint, typeof(Transform), true) as Transform;
			GUILayout.Space (3f);
			jet.fanExhaustPoint = EditorGUILayout.ObjectField ("Fan Exhaust Point", jet.fanExhaustPoint, typeof(Transform), true) as Transform;
		}
		//
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		jet.soundState = (SilantroTurboFan.SoundState)EditorGUILayout.EnumPopup("Cabin Sounds", jet.soundState);
		GUILayout.Space(5f);
		if (jet.soundState == SilantroTurboFan.SoundState.Absent) {
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
			//
			if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning) {
				GUILayout.Space(3f);
				jet.afterburnerPitch = EditorGUILayout.FloatField ("Afterburner Pitch", jet.afterburnerPitch);
			}
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
			//
			GUILayout.Space (10f);
			jet.useEmission = EditorGUILayout.Toggle ("Engine Color Emission",  jet.useEmission);
	
			if(jet.useEmission){
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Exhaust Emission Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			jet.engineMaterial = EditorGUILayout.ObjectField ("Core Material", jet.engineMaterial, typeof(Material), true) as Material;
			if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning) {
				GUILayout.Space (3f);
				jet.afterburnerTubeMaterial = EditorGUILayout.ObjectField ("Afterburner Pipe Material", jet.afterburnerTubeMaterial, typeof(Material), true) as Material;
			} else {
				GUILayout.Space (3f);
				jet.afterburnerTubeMaterial = EditorGUILayout.ObjectField ("Pipe Material", jet.afterburnerTubeMaterial, typeof(Material), true) as Material;
			}
			GUILayout.Space (3f);
			jet.maximumNormalEmission = EditorGUILayout.FloatField ("Maximum Emission", jet.maximumNormalEmission);
			GUILayout.Space (2f);
			if (jet.reheatSystem == SilantroTurboFan.ReheatSystem.Afterburning) {
				jet.maximumAfterburnerEmission = EditorGUILayout.FloatField ("Maximum Afterburner Emission", jet.maximumAfterburnerEmission);
			}
			}
		}

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Core Power",(jet.corePower*jet.coreFactor*100f).ToString("0.00") + " %");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Engine Thrust",jet.EngineThrust.ToString("0.0")+ " N");

		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (engineObject.targetObject, "Turbojet Engine Change");}
		if (GUI.changed) {
			EditorUtility.SetDirty (jet);
			EditorSceneManager.MarkSceneDirty (jet.gameObject.scene);
		}
		engineObject.ApplyModifiedProperties();
	}
}
#endif
