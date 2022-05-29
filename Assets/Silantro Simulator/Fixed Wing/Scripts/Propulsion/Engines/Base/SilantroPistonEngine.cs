using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroPistonEngine: MonoBehaviour {

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
	public enum FuelType{AVGas100,AVGas100LL,AVGas82UL}
	[HideInInspector]public FuelType fuelType = FuelType.AVGas100;
	//CARBURATOR
	public enum CarburatorType{RAECorrected}
	[HideInInspector]public CarburatorType carburettorType = CarburatorType.RAECorrected;
	//DISPLACEMENT
	public enum DisplacementUnit{Liter,CubicMeter,CubicInch,CubicCentimeter,CubicFoot}
	[HideInInspector]public DisplacementUnit displacementUnit = DisplacementUnit.Liter;

	//---------------------------------------------CONNECTION POINTS
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Transform ExhaustPoint;
	[HideInInspector]public SilantroFuelDistributor fuelSystem;
	[HideInInspector]public SilantroCore EMU;
	[HideInInspector]public SilantroController controller;

	//---------------------------------------------ENGINE CORE FACTORS
	[HideInInspector]public float norminalRPM,baseRPM;[HideInInspector]public float functionalRPM = 1000;
	[HideInInspector]public float FuelInput;
	[HideInInspector]public float nth,nm = 80f,ng = 90f,brakePower;
	[HideInInspector]public float emissionCore;
	[HideInInspector]public float actualConsumptionrate;

	[HideInInspector]public float stroke = 5;
	[HideInInspector]public float bore = 6;
	[HideInInspector]public float displacement = 1000,actualDisplacement;
	[HideInInspector]public float compressionRatio = 10,residue = 4f;
	[HideInInspector]public int numberOfCylinders = 1;[HideInInspector]public bool diplaySettings;

	//THERMODYNAMICS
	[HideInInspector]public float P1,P2,P3,P4;
	[HideInInspector]public float T1,T2,T3,T4;
	[HideInInspector]public float vc,vd;
	[HideInInspector]public float V1, V2, V3;
	[HideInInspector]public float AF = 15f,mm,me,ma,mf;
	[HideInInspector]public float Ma, Mf,BSFC;
	[HideInInspector]public float Qin, W3_4, W1_2,Wnet,Wb,Pb,combustionEnergy = 43000f;


	//-------------------------------------------ENGINE EFFECTS
	[HideInInspector]public ParticleSystem exhaustSmoke;
	ParticleSystem.EmissionModule smokeModule;Color baseColor;Color finalColor;
	[HideInInspector]public float maximumEmissionValue = 50f;[HideInInspector]public bool showPerformance;

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
	[HideInInspector]public bool adjustSoundSettings;
	[HideInInspector]public float currentEnginePitch,maximumEnginePitch = 1f,IdleEnginePitch= 0.5f;
	[HideInInspector]public float currentEngineVolume ,maximumEngineVolume;

	//---------------------------------------------ENGINE VARIABLES
	float startRange;float endRange;float cycleRange;float offset,fuelFactor;
	[HideInInspector]public float corePower,coreAcceleration = 0.5f,coreValue,coreFactor,value;
	[HideInInspector]public float coreRPM;

	//---------------------------------------------CONTROL BOOLS
	[HideInInspector]public bool start,stop,clutching,active,isControllable;







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
		if (displacementUnit == DisplacementUnit.CubicCentimeter) {actualDisplacement = displacement/1000000;}
		if (displacementUnit == DisplacementUnit.CubicFoot) {actualDisplacement = displacement/35.315f;}
		if (displacementUnit == DisplacementUnit.CubicInch) {actualDisplacement = displacement/61023.744f;}
		if (displacementUnit == DisplacementUnit.CubicMeter) {actualDisplacement = displacement;}
		if (displacementUnit == DisplacementUnit.Liter) {actualDisplacement = displacement/1000;}
	}
	#endif



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (fuelSystem != null && EMU != null && connectedAircraft != null) {
			allOk = true;
		} 
		else if (EMU == null) {
			Debug.LogError("Prerequisites not met on Engine "+transform.name + "....Core not connected");
			allOk = false;
		}
		else if (connectedAircraft == null) {
			Debug.LogError("Prerequisites not met on Engine "+transform.name + "....Aircraft not connected");
			allOk = false;
		}
		else if (fuelSystem == null) {
			Debug.LogError("Prerequisites not met on Engine "+transform.name + "....Fuel system not attached");
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
			baseRPM = functionalRPM/10f;
			active = false;clutching = false;start = false;stop = false;
			//SET ENGINE JUMP START VALUE;
			if (engineStartMode == EngineStartMode.Hot) {coreAcceleration = 10f;}
			//SET ENGINE JUMP START VALUE;
			if (engineStartMode == EngineStartMode.Hot) {coreAcceleration = 10f;}
			if (displacementUnit == DisplacementUnit.CubicCentimeter) {actualDisplacement = displacement/1000000;}
			if (displacementUnit == DisplacementUnit.CubicFoot) {actualDisplacement = displacement/35.315f;}
			if (displacementUnit == DisplacementUnit.CubicInch) {actualDisplacement = displacement/61023.744f;}
			if (displacementUnit == DisplacementUnit.CubicMeter) {actualDisplacement = displacement;}
			if (displacementUnit == DisplacementUnit.Liter) {actualDisplacement = displacement/1000;}
			////SET UP ENGINE FUEL COMBUSTION VALUES
			if (fuelType == FuelType.AVGas100){combustionEnergy = 42.8f;}
			if (fuelType == FuelType.AVGas100LL){combustionEnergy = 43.5f;}
			if (fuelType == FuelType.AVGas82UL) {combustionEnergy = 43.6f;} 
			combustionEnergy *= 1000f;
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
		if(allOk && isControllable){
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
			actualConsumptionrate = Mf;


			//SIMULATE ENGINE FUEL CHOKING
			if (fuelSystem != null && ExteriorIdleSound != null){
				if (fuelSystem.lowFuel) {
					if (active) {
						startRange = 0.6f;endRange = 1.0f;cycleRange = (endRange - startRange) / 2f;offset = cycleRange + startRange;fuelFactor = offset + Mathf.Sin (Time.time * 3f) * cycleRange;
						ExteriorIdleSource.pitch = fuelFactor;if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIdleSource.pitch = fuelFactor;}
					}
				}
				else {ExteriorIdleSource.pitch = currentEnginePitch;if (InteriorIdleSource != null && soundState == SoundState.Available) {InteriorIdleSource.pitch = currentEnginePitch ;}}
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
			else {coreValue = Mathf.Lerp (coreValue, 0f, 0.04f);}
			//
			if (exhaustSmoke != null) {emissionCore = maximumEmissionValue * coreFactor;smokeModule.rateOverTime = emissionCore;
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
	//CALCULATE ENGINE THRUST
	private void EngineCalculation()
	{
		if (FuelInput < 0.01) {FuelInput = 0.10f;}
		//---------------------------------------------------------------ENGINE CASE
		P1 = EMU.ambientPressure;
		T1 = 273.5f + EMU.ambientTemperature;
		float airdensity = EMU.airDensity;

		//STAGE 1=====================================================================
		vd = actualDisplacement/numberOfCylinders;
		vc = vd/(compressionRatio-1);
		V1 = vc + vd;
		mm = (P1 * 1000 * V1) / (287f * T1);

		//STAGE 2=====================================================================
		float supremeRatio = compressionRatio*FuelInput;
		P2 = P1*Mathf.Pow(supremeRatio,1.35f);
		T2 = T1 * Mathf.Pow (supremeRatio, 0.35f);
		V2 = V1 / compressionRatio;
		//MASSES
		me = (residue/100)*mm;
		mf = ((mm - me) / (AF + 1))*FuelInput;
		ma = AF * mf;

		//STAGE 3=====================================================================
		Qin  = mf*combustionEnergy;
		T3 = (Qin / (mm * 0.821f)) + T2;
		V3 = V1;
		P3 = P2 * (T3 / T2);

		//STAGE 4=====================================================================
		P4 = P3*Mathf.Pow((1/supremeRatio),1.35f);
		T4 = T3*Mathf.Pow((1/supremeRatio),0.35f);
		W3_4 = (mm * 0.287f * (T4 - T3)) / (-0.35f);
		W1_2 = (mm * 0.287f * (T2 - T1)) / (-0.35f);
		Wnet = W3_4 + W1_2;
		nth = (Wnet / Qin) * 100f;

		//OUTPUT=====================================================================
		Wb = (nm/100)*Wnet;
		Pb = (Wb * (coreRPM / 60f) * 0.5f) * numberOfCylinders;
		brakePower = (Pb * 1000) / 745.6f;
		if(brakePower < 0){brakePower = 0;}

		Mf = (mf * (coreRPM / 60) * 0.5f) * numberOfCylinders;
		Ma = (ma * (coreRPM / 60) * 0.5f) * numberOfCylinders;
		AF = (Ma * 3600) / (numberOfCylinders * 15.56f);
		AF = Mathf.Clamp (AF, 10, 20);
		if (brakePower > 0) {BSFC = (Mf * 3600f * 2.2046f) / (brakePower);}
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPistonEngine))]
public class PistonEngineEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	Color fuelColor = Color.cyan;
	[HideInInspector]public int toolbarTab;
	[HideInInspector]public string currentTab;
	//
	//SOUNDS
	[HideInInspector]public int EngineTab;
	[HideInInspector]public string currentEngineTab;
	SilantroPistonEngine engine;SerializedObject engineObject;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		engine = (SilantroPistonEngine)target;
		engineObject = new SerializedObject (engine);
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();serializedObject.Update ();
		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Identifier", MessageType.None);
		GUI.color = backgroundColor;
		engine.engineIdentifier = EditorGUILayout.TextField (" ", engine.engineIdentifier);
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		engine.stroke = EditorGUILayout.FloatField ("Stroke", engine.stroke);
		GUILayout.Space (2f);
		engine.bore = EditorGUILayout.FloatField ("Bore", engine.bore);
		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Displacement", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		engine.displacement = EditorGUILayout.FloatField (" ", engine.displacement);
		GUILayout.Space (3f);
		engine.displacementUnit = (SilantroPistonEngine.DisplacementUnit)EditorGUILayout.EnumPopup(" ",engine.displacementUnit);

		GUILayout.Space (5f);
		engine.carburettorType = (SilantroPistonEngine.CarburatorType)EditorGUILayout.EnumPopup ("Carburettor Type", engine.carburettorType);
		GUILayout.Space(10f);
		engine.compressionRatio = EditorGUILayout.FloatField ("Compression Ratio", engine.compressionRatio);
		GUILayout.Space (2f);
		engine.numberOfCylinders = EditorGUILayout.IntField ("No of Cylinders", engine.numberOfCylinders);
		GUILayout.Space (5f);
		engine.functionalRPM = EditorGUILayout.FloatField ("Functional RPM", engine.functionalRPM);
		GUILayout.Space (3f);
		engine.coreAcceleration = EditorGUILayout.Slider ("Core Acceleration", engine.coreAcceleration, 0.01f, 1f);
		GUILayout.Space (10f);
		engine.ExhaustPoint = EditorGUILayout.ObjectField ("Exhaust Point", engine.ExhaustPoint, typeof(Transform), true) as Transform;

		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Exhaust Gas Residual (%)", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		engine.residue = EditorGUILayout.Slider (" ",engine.residue,0,15);
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Mechanical Efficiency", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		engine.nm = EditorGUILayout.Slider (" ",engine.nm,50,90);
		GUILayout.Space(8f);
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Fuel", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		GUI.color = fuelColor;
		engine.fuelType = (SilantroPistonEngine.FuelType)EditorGUILayout.EnumPopup ("Fuel Type", engine.fuelType);
		GUI.color = backgroundColor;
		if (engine.fuelType == SilantroPistonEngine.FuelType.AVGas100){engine.combustionEnergy = 42.8f;fuelColor = Color.green;}
		if (engine.fuelType == SilantroPistonEngine.FuelType.AVGas100LL){engine.combustionEnergy = 43.5f;fuelColor = Color.cyan;}
		if (engine.fuelType == SilantroPistonEngine.FuelType.AVGas82UL) {engine.combustionEnergy = 43.6f;fuelColor = Color.red;} 
		engine.combustionEnergy *= 1000f;
		GUILayout.Space(6f);
		EditorGUILayout.LabelField ("Q ", engine.combustionEnergy.ToString ("0.00") + " KJ");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Air-Fuel Ratio", engine.AF.ToString ("0.00"));
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("PSFC ", engine.BSFC.ToString ("0.00") + " lb/hp.hr");

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Performance", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		engine.showPerformance = EditorGUILayout.Toggle ("Show", engine.showPerformance);
		if (engine.showPerformance) {
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("INTAKE", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P1: " + engine.P1.ToString ("0.00") + " KPa" + " || T1: " + engine.T1.ToString ("0.00") + " °K");
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("COMPRESSION", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P2: " + engine.P2.ToString ("0.00") + " KPa" + " || T2: " + engine.T2.ToString ("0.00") + " °K");
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("POWER", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P3: " + engine.P3.ToString ("0.00") + " KPa" + " || T3: " + engine.T3.ToString ("0.00") + " °K");
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("EXHAUST", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P4: " + engine.P4.ToString ("0.00") + " KPa" + " || T4: " + engine.T4.ToString ("0.00") + " °K");

			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Module Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Expansion Work", (engine.W3_4).ToString ("0.00") + " kJ");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Compression Work", (engine.W1_2).ToString ("0.00") + " kJ");

			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Flows Rates", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Intake Air", engine.Ma.ToString ("0.00") + " kg/s");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Fuel", engine.Mf.ToString ("0.00") + " kg/s");
		}

		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		engine.soundState = (SilantroPistonEngine.SoundState)EditorGUILayout.EnumPopup("Cabin Sounds", engine.soundState);
		GUILayout.Space(5f);
		if (engine.soundState == SilantroPistonEngine.SoundState.Absent) {
			engine.ExteriorIgnitionSound = EditorGUILayout.ObjectField ("Ignition Sound", engine.ExteriorIgnitionSound, typeof(AudioClip), true) as AudioClip;
			GUILayout.Space (2f);
			engine.ExteriorIdleSound = EditorGUILayout.ObjectField ("Engine Idle Sound", engine.ExteriorIdleSound, typeof(AudioClip), true) as AudioClip;
			GUILayout.Space (2f);
			engine.ExteriorShutdownSound = EditorGUILayout.ObjectField ("Shutdown Sound", engine.ExteriorShutdownSound, typeof(AudioClip), true) as AudioClip;
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
				engine.ExteriorIgnitionSound = EditorGUILayout.ObjectField ("Exterior Ignition", engine.ExteriorIgnitionSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				engine.ExteriorIdleSound = EditorGUILayout.ObjectField ("Exterior Idle", engine.ExteriorIdleSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				engine.ExteriorShutdownSound = EditorGUILayout.ObjectField ("Exterior Shutdown", engine.ExteriorShutdownSound, typeof(AudioClip), true) as AudioClip;
				//
				break;
			case "Interior Sounds":
				engine.InteriorIgnitionSound = EditorGUILayout.ObjectField ("Interior Ignition", engine.InteriorIgnitionSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				engine.InteriorIdleSound = EditorGUILayout.ObjectField ("Interior Idle", engine.InteriorIdleSound, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (2f);
				engine.InteriorShutdownSound = EditorGUILayout.ObjectField ("Interior Shutdown", engine.InteriorShutdownSound, typeof(AudioClip), true) as AudioClip;
				//
				break;
			}
		}
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(8f);
		engine.adjustSoundSettings = EditorGUILayout.Toggle("Show Sound Settings",engine.adjustSoundSettings);
		GUILayout.Space(3f);
		if (engine.adjustSoundSettings) {
			engine.IdleEnginePitch = EditorGUILayout.FloatField ("Base Pitch", engine.IdleEnginePitch);
			GUILayout.Space(2f);
			engine.maximumEnginePitch = EditorGUILayout.FloatField ("Military Pitch", engine.maximumEnginePitch);
		}
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Effects Configuration", MessageType.None);
		GUI.color = backgroundColor;
		engine.diplaySettings = EditorGUILayout.Toggle ("Show Extras",  engine.diplaySettings);
		if ( engine.diplaySettings) {
			GUILayout.Space (5f);
			engine.exhaustSmoke = EditorGUILayout.ObjectField ("Exhaust Smoke", engine.exhaustSmoke, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space (2f);
			engine.maximumEmissionValue = EditorGUILayout.FloatField ("Maximum Emission", engine.maximumEmissionValue);
		}

		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Core Power",(engine.corePower*engine.coreFactor*100f).ToString("0.00") + " %");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Brake Power",engine.brakePower.ToString("0.0")+ " Hp");

		if (GUI.changed) {
			EditorUtility.SetDirty (engine);
			EditorSceneManager.MarkSceneDirty (engine.gameObject.scene);
		}
		engineObject.ApplyModifiedProperties();
	}
}
#endif