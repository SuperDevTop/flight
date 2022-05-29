using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroPulseJet : MonoBehaviour {

	[HideInInspector]public string engineIdentifier = "Default Engine";
	//------------------------------------------------SELECTORS
	//CURRENT ENGINE STATE
	public enum EngineState{Off,Active}
	[HideInInspector]public EngineState CurrentEngineState = EngineState.Off;
	//FUEL TYPE
	[HideInInspector]public enum FuelType{JetB,JetA1,JP6,JP8}
	[HideInInspector]public FuelType fuelType = FuelType.JetB;

	//---------------------------------------------ENGINE DIMENSIONS
	[HideInInspector]public float EngineDiameter = 1f;
	[HideInInspector]public float IntakeDiameterPercentage = 90f;
	[HideInInspector]public float ExhaustDiameterPercentage = 90f;
	[HideInInspector]public float CombustionPercentage = 90f;
	[HideInInspector]public float CombustionDiameter;
	[HideInInspector]public float IntakeDiameter;
	[HideInInspector]public float ExhaustDiameter;
	[HideInInspector]public float actualIntakeDiameter,actualExhaustDiameter;
	[HideInInspector]float coreLength;[HideInInspector]public bool isControllable;
	[HideInInspector]public float engineLength, combustionChamberLength,CombustionVolume;[HideInInspector]public bool diplaySettings;

	//---------------------------------------------CONNECTION POINTS
	[HideInInspector]public Rigidbody connectedAircraft;
	//MARKERS
	[HideInInspector]public Transform IntakePoint;
	[HideInInspector]public Transform ExhaustPoint;
	[HideInInspector]public Transform CombustionEntry;
	[HideInInspector]public Transform CombustionExit;
	[HideInInspector]public SilantroFuelDistributor fuelSystem;
	[HideInInspector]public SilantroCore computer;
	[HideInInspector]public SilantroController controller;

	[Range(0,1)]public float FuelInput;float norminalSpeed,baseSpeed;float functionalSpeed = 1000;
	[HideInInspector]public float engineFrequency = 100f;
	[HideInInspector]public float coreCounter,actualFrequency;

	//CORE
	[HideInInspector]public float pressureRatio = 10f, combustionEnergy;
	[HideInInspector]public float EngineThrust;
	[HideInInspector]public AnimationCurve pressureFactor;
	[HideInInspector]public AnimationCurve adiabaticFactor;
	//
	[HideInInspector]public float Pa,P2,P3;
	[HideInInspector]public float Ta, T2, T3,T4;
	[HideInInspector]public float Ya, Y2, Y3,Y4;
	[HideInInspector]public float Cpa, Cp2, Cp3,Cp4;
	[HideInInspector]public float f,Ae,nb = 90f,np, nth, no,Ma,Mf;[HideInInspector]public float exhaustVelocity,exhaustTemperature;
	[HideInInspector]public float TSFC;[HideInInspector]public float actualConsumptionrate;
	//---------------------------------------------SOUND SYSTEM
	[HideInInspector]public AudioClip ExteriorIdleSound;private AudioSource ExteriorIdleSource;
	//
	[HideInInspector]public bool adjustSoundSettings,showPerformance,showConstants;
	[HideInInspector]public float currentEnginePitch,maximumEnginePitch = 0.9f,IdleEnginePitch= 0.5f;
	[HideInInspector]public float currentEngineVolume ,maximumEngineVolume;
	//-------------------------------------------ENGINE EFFECTS
	[HideInInspector]public ParticleSystem exhaustSmoke;
	ParticleSystem.EmissionModule smokeModule;
	[HideInInspector]public ParticleSystem exhaustGlow;
	ParticleSystem.EmissionModule glowModule;
	[HideInInspector]public ParticleSystem exhaustFlame;
	ParticleSystem.EmissionModule flameModule;
	[HideInInspector]public float maximumEmissionValue = 50f;[HideInInspector]public float emissionCore;


	//---------------------------------------------ENGINE VARIABLES
	float startRange;float endRange;float cycleRange;float offset,fuelFactor;
	[HideInInspector]public float corePower,coreValue,coreSpeed,coreFactor,value,powerFactor,mach;
	//---------------------------------------------CONTROL BOOLS
	//[HideInInspector]
	public bool start,stop,active;


	//ENGINE CONTROL FUNCTIONS
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void StartEngine()
	{
		//MAKE SURE SOUND IS SET PROPERLY
		if (ExteriorIdleSound == null) {
			Debug.Log ("Engine " + transform.name + " cannot start due to incorrect Audio configuration");
		} 
		else {
			//MAKE SURE THERE IS FUEL TO START THE ENGINE
			if (fuelSystem && fuelSystem.TotalFuelRemaining > 1f) {
				//MAKE SURE CORRECT FUEL IS SELECTED
				if (fuelType.ToString () == fuelSystem.fuelType.ToString ()) {

					//JUMP START ENGINE
					active = true;
					StateActive ();CurrentEngineState = EngineState.Active;
				} 
				else {Debug.Log ("Engine " + transform.name + " cannot start due to incorrect fuel selection");}} 
			else {Debug.Log ("Engine " + transform.name + " cannot start due to low fuel");}
		}
	}


	// -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ShutDownEngine(){stop = true;ReturnIgnition();}

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
		Handles.color = Color.red;
		if(ExhaustPoint != null){Handles.DrawWireDisc (ExhaustPoint.position, ExhaustPoint.transform.forward, (ExhaustDiameter/2f));}
		//INTAKE
		IntakeDiameter = EngineDiameter * (IntakeDiameterPercentage / 100f);
		actualIntakeDiameter = IntakeDiameter;
		Handles.color = Color.blue;
		if(IntakePoint != null && connectedAircraft!=null){Handles.DrawWireDisc (IntakePoint.transform.position, connectedAircraft.transform.forward, (actualIntakeDiameter / 2f));}
		Handles.color = Color.cyan;
		if(ExhaustPoint != null && IntakePoint != null ){Handles.DrawLine (IntakePoint.transform.position, ExhaustPoint.position);}
		DrawPressureFactor ();DrawAdiabaticConstant ();
		//
		CombustionDiameter = EngineDiameter *(CombustionPercentage/100f);
		Handles.color = Color.yellow;
		if(CombustionEntry != null){Handles.DrawWireDisc (CombustionEntry.position, CombustionEntry.transform.forward, (CombustionDiameter/2f));}
		if(CombustionExit != null){Handles.DrawWireDisc (CombustionExit.position, CombustionExit.transform.forward, (CombustionDiameter/2f));}
		Handles.color = Color.red;
		if(CombustionExit != null && CombustionEntry != null ){Handles.DrawLine (CombustionExit.position, CombustionEntry.position);}
		//CALCULATE DIMENSIONS
		combustionChamberLength = Vector3.Distance (CombustionEntry.position, CombustionExit.position);
		engineLength = Vector3.Distance (IntakePoint.position, ExhaustPoint.position);
		CombustionVolume = ((3.142f*CombustionDiameter*CombustionDiameter)/4f)*combustionChamberLength;
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

	void Start()
	{
		InitializeEngine ();
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeEngine () {


		//CHECK SYSTEMS
		_checkPrerequisites ();


		if (allOk) {
			//SETUP SOUND SYSTEM
			//CREATE A GAMEOBJECT TO ADD SOUND SOURCE TO
			//1. EXTERIOR
			GameObject exteriorSoundPoint = new GameObject();exteriorSoundPoint.transform.parent = this.transform;exteriorSoundPoint.transform.localPosition = new Vector3 (0, 0, 0);exteriorSoundPoint.name = "Exterior Sound Point";
			//SETUP IDLE
			if (ExteriorIdleSound != null) {
				ExteriorIdleSource = exteriorSoundPoint.gameObject.AddComponent<AudioSource>();ExteriorIdleSource.maxDistance = 600f;ExteriorIdleSource.clip = ExteriorIdleSound;ExteriorIdleSource.loop = true;ExteriorIdleSource.Play();ExteriorIdleSource.volume = 0f;
				ExteriorIdleSource.spatialBlend = 1f;ExteriorIdleSource.dopplerLevel = 0f;ExteriorIdleSource.rolloffMode = AudioRolloffMode.Custom;
			}
			//SETUP EFFECTS
			if (exhaustSmoke != null) {smokeModule = exhaustSmoke.emission;smokeModule.rateOverTime = 0f;}
			if (exhaustGlow != null) {glowModule = exhaustGlow.emission;glowModule.rateOverTime = 0f;}
			if (exhaustFlame != null) {flameModule = exhaustFlame.emission;flameModule.rateOverTime = 0f;}
			//RESET VALUES
			DrawPressureFactor ();DrawAdiabaticConstant ();
			actualExhaustDiameter = EngineDiameter * ExhaustDiameterPercentage / 100f;
			actualIntakeDiameter = EngineDiameter * IntakeDiameterPercentage/100f;
			active = false;start = false;stop = false;
			//SET ENGINE JUMP START VALUE;
			baseSpeed = 100f;actualFrequency = 1/engineFrequency;
			//SET UP ENGINE FUEL COMBUSTION VALUES
			if (fuelType == FuelType.JetB){combustionEnergy = 42.8f;}
			else if (fuelType == FuelType.JetA1) {combustionEnergy = 43.5f;}
			else if (fuelType == FuelType.JP6) {combustionEnergy = 43.02f;} 
			else if (fuelType == FuelType.JP8) {combustionEnergy = 43.28f;}
			combustionEnergy *= 1000f;
			combustionChamberLength = Vector3.Distance (CombustionEntry.position, CombustionExit.position);
			engineLength = Vector3.Distance (IntakePoint.position, ExhaustPoint.position);
			CombustionVolume = ((3.142f*CombustionDiameter*CombustionDiameter)/4f)*combustionChamberLength;
		}
	}
		



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		//APPLY GENERATED FORCE
		if (EngineThrust > 0f && connectedAircraft != null && ExhaustPoint != null) {
			Vector3 force = ExhaustPoint.forward * EngineThrust;
			connectedAircraft.AddForce (force, ForceMode.Force);
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void CoreEngine()
	{
		if (active) {if(corePower < 1f){corePower += Time.deltaTime * 5;}}
		else if(corePower > 0f){corePower -= Time.deltaTime * 5;}
		if (corePower > 1) {corePower = 1f;}if (!active && corePower < 0) {corePower = 0f;}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENGINE RUN CONFIGURATION
	private void StateActive()
	{
		norminalSpeed = baseSpeed + (functionalSpeed - baseSpeed) * FuelInput;
		//STOP IGINITION SOUND IF ITS STILL PLAYING
		if (stop) {
			CurrentEngineState = EngineState.Off;active = false;
			//RESET
			StartCoroutine(ReturnIgnition());
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//STOP ENGINE
	private void StateOff()
	{
		norminalSpeed = 0f;
		//START ENGINE PROCEDURE
		if (start) {
			CurrentEngineState = EngineState.Active;active = true;
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
			case EngineState.Active:StateActive ();break;
			}

			if (active) {currentEngineVolume = coreFactor + (0.5f * corePower);} else {currentEngineVolume = coreFactor;}currentEngineVolume = Mathf.Clamp(currentEngineVolume,0,1f);
			//ENGINE VOLUMES
			if (currentEngineVolume > 0.0001f && currentEngineVolume < 2f && ExteriorIdleSource != null) {
				ExteriorIdleSource.volume = currentEngineVolume;
			}
				
			//SIMULATE ENGINE FUEL CHOKING
			if (fuelSystem != null && ExteriorIdleSound != null){
				if (fuelSystem.lowFuel) {
					if (active) {
						startRange = 0.6f;endRange = 1.0f;cycleRange = (endRange - startRange) / 2f;offset = cycleRange + startRange;fuelFactor = offset + Mathf.Sin (Time.time * 3f) * cycleRange;
						ExteriorIdleSource.pitch = fuelFactor;
					}
				}
				else {ExteriorIdleSource.pitch = currentEnginePitch ;}
			} 
			if (active) {coreSpeed= Mathf.Lerp (coreSpeed, norminalSpeed, 5 * Time.deltaTime *corePower );}
			else{coreSpeed = Mathf.Lerp (coreSpeed, 0, 5 * Time.deltaTime);}if (coreSpeed < 0) {coreSpeed = 0;}

			//STOP ENGINE IF FUEL IS EXHAUSTED
			if (fuelSystem != null && fuelSystem.TotalFuelRemaining <= 0) {stop = true;}
			//CALCULATE ENGINE PITCH
			if (ExteriorIdleSound != null && connectedAircraft != null) {
				float speedFactor = ((coreSpeed + (connectedAircraft.velocity.magnitude * 1.943f) + 10f) - baseSpeed) / (functionalSpeed - baseSpeed);
				currentEnginePitch = IdleEnginePitch + ((maximumEnginePitch - IdleEnginePitch) * speedFactor);
				currentEnginePitch = Mathf.Clamp (currentEnginePitch, 0, 2);
			}

			if (coreSpeed <= 0f) {coreSpeed = 0f;}
			//ENGINE EFFECTS
			if (active == true) {coreValue = corePower;}
			else {coreValue = Mathf.Lerp (coreValue, 0f, 0.04f);}FuelInput = Mathf.Clamp (FuelInput, 0, 1.0f);
			//
			if (exhaustSmoke != null) {emissionCore = maximumEmissionValue * coreFactor*powerFactor;}
			if(exhaustSmoke != null){smokeModule.rateOverTime = emissionCore;}
			if(exhaustGlow != null){glowModule.rateOverTime = emissionCore;}
			if(exhaustFlame != null){flameModule.rateOverTime = emissionCore;}
			coreFactor = coreSpeed / functionalSpeed;
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE ENGINE THRUST
	private void EngineCalculation()
	{
		//-----------------------------------------------------------OPERATION
		coreCounter += Time.smoothDeltaTime;powerFactor = 0f;
		if (coreCounter > actualFrequency) {
			//RESET COUNTER
			coreCounter = 0f;powerFactor = 1f;
		}
		if(FuelInput <0.01){FuelInput = 0.01f;}
		//-----------------------------------------------------------ENGINE CASE
		Ae = (Mathf.PI*actualExhaustDiameter*actualExhaustDiameter)/4f;
		Pa = computer.ambientPressure;
		Ta = 273.5f + computer.ambientTemperature;
		Ya = adiabaticFactor.Evaluate (Ta);Cpa = pressureFactor.Evaluate (Ta);
		float speed = connectedAircraft.velocity.magnitude;
		mach = speed/(Mathf.Sqrt(1.4f*287f*Ta));

		//DIFFUSER=====================================================================
		Y2 = adiabaticFactor.Evaluate (Ta);Cp2 = pressureFactor.Evaluate (Ta);
		float T2_Ta = (Y2 - 1) / 2f;
		T2 = Ta *(1+(T2_Ta*mach*mach));
		P2 = Pa * (Mathf.Pow ((T2 / Ta), (Y2 / (Y2 - 1))));

		//BURNER=======================================================================
		T3 = T2*pressureRatio;P3 = pressureRatio*P2;
		Y3 = adiabaticFactor.Evaluate (T3);Cp3= pressureFactor.Evaluate (T3)*1000f;
		float F1 = (((Cp3/1000) * T3) - ((Cp2/1000) * T2));
		float F2 = (((nb/100)* combustionEnergy) - ((Cp3/1000) * T3));
		f = (F1 / F2)*FuelInput;

		//TAIL PIPE=======================================================================
		float T3_T4 = Mathf.Pow((P3/Pa),((Y3-1)/Y3));
		T4 = (T3 / T3_T4)*coreFactor;
		Y4 = adiabaticFactor.Evaluate  (T4);Cp4 = pressureFactor.Evaluate (T4)*1000;
		exhaustTemperature = T4 - 273.15f;
		float pa_p3 = Mathf.Pow ((Pa / P3), ((Y4 - 1) / Y4));
		exhaustVelocity = Mathf.Sqrt (2f * Cp4 * T4* (1 - pa_p3))*coreFactor;

		Ma = computer.airDensity * (engineFrequency * CombustionVolume)*coreFactor;
		Mf = Ma * f;

		//THRUST
		EngineThrust = Ma*(((1+f)*exhaustVelocity)-speed);
		if(EngineThrust < 0){EngineThrust = 0;}
		float pt = EngineThrust*0.2248f;
		TSFC = ((Mf * 3600f) / (pt * 0.4536f));actualConsumptionrate = Mf;
		//--------------------------------------------EFFICIENCIES
		//1. PROPULSIVE
		np = (2*(speed+1))/((speed+1)+exhaustVelocity);
		//2. THERMAL
		float nthTop = (EngineThrust*(speed+1))+(0.5f*Ma*(1+f)*Mathf.Pow((exhaustVelocity-speed),2));
		float nthBot = Mf * combustionEnergy *1000;
		nth = nthTop / nthBot;
		//3. OVERALL
		no = nth*np;
		nth *= 100f;np *= 100f;no *= 100f;
	}

}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPulseJet))]
public class SilantroPulseJetEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	[HideInInspector]public int toolbarTab;[HideInInspector]public string currentTab;
	//SOUNDS
	[HideInInspector]public int EngineTab;[HideInInspector]public string currentEngineTab;
	SilantroPulseJet jet;SerializedObject engineObject;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnEnable()
	{
		jet = (SilantroPulseJet)target;
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
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Intake Diameter", jet.IntakeDiameter.ToString ("0.000") + " m");
		GUILayout.Space(3f);
		jet.ExhaustDiameterPercentage = EditorGUILayout.Slider ("Exhaust Ratio",jet.ExhaustDiameterPercentage,0,100);
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Exhaust Diameter", jet.ExhaustDiameter.ToString ("0.000") + " m");
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Combustion Chamber Ratio", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.CombustionPercentage = EditorGUILayout.Slider (" ",jet.CombustionPercentage,0,100);
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Chamber Diameter", jet.CombustionDiameter.ToString ("0.000") + " m");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Chamber Length", jet.combustionChamberLength.ToString ("0.000") + " m");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Chamber Volume", jet.CombustionVolume.ToString ("0.000") + " m3");
		GUILayout.Space(5f);
		jet.engineFrequency = EditorGUILayout.Slider ("Frequency",jet.engineFrequency,20f,200f);
		GUILayout.Space(5f);
		jet.nb = EditorGUILayout.Slider ("Burner Efficiency",jet.nb,90,98);
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.pressureRatio = EditorGUILayout.FloatField ("Pressure Ratio", jet.pressureRatio);
		GUILayout.Space(8f);
		GUI.color = Color.green;
		jet.fuelType = (SilantroPulseJet.FuelType)EditorGUILayout.EnumPopup ("Fuel Type", jet.fuelType);
		GUI.color = backgroundColor;
		if (jet.fuelType == SilantroPulseJet.FuelType.JetB){jet.combustionEnergy = 42.8f;}
		else if (jet.fuelType == SilantroPulseJet.FuelType.JetA1) {jet.combustionEnergy = 43.5f;}
		else if (jet.fuelType == SilantroPulseJet.FuelType.JP6) {jet.combustionEnergy = 49.6f;} 
		else if (jet.fuelType == SilantroPulseJet.FuelType.JP8) {jet.combustionEnergy = 43.28f;}
		jet.combustionEnergy *= 1000f;
		GUILayout.Space(6f);
		EditorGUILayout.LabelField ("Q ", jet.combustionEnergy.ToString ("0.00") + " KJ");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("TSFC ", jet.TSFC.ToString ("0.00") + " lb/lbf.hr");
		GUILayout.Space(6f);
		jet.showConstants = EditorGUILayout.Toggle ("Show Constants", jet.showConstants);
		if (jet.showConstants) {
			GUILayout.Space(3f);
			EditorGUILayout.CurveField ("Gamma Curve", jet.adiabaticFactor);
			GUILayout.Space(3f);
			EditorGUILayout.CurveField ("Pressure Curve", jet.pressureFactor);
		}

		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Thermodynamic Performance", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		jet.showPerformance = EditorGUILayout.Toggle ("Show", jet.showPerformance);
		if (jet.showPerformance) {
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("AMBIENT", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Pa: " + jet.Pa.ToString ("0.00") + " KPa" + " || Ta: " + jet.Ta.ToString ("0.00") + " °K" + " || Cpa: " + jet.Cpa.ToString ("0.0000") + " || Ya: " + jet.Ya.ToString ("0.0000"));
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("DIFFUSER", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P2: " + jet.P2.ToString ("0.00") + " KPa" + " || T2: " + jet.T2.ToString ("0.00") + " °K" + " || Cp2: " + jet.Cp2.ToString ("0.0000") + " || Y2: " + jet.Y2.ToString ("0.0000"));
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("COMBUSTION CHAMBER", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P3: " + jet.P3.ToString ("0.00") + " KPa" + " || T3: " + jet.T3.ToString ("0.00") + " °K" + " || Cp3: " + jet.Cp2.ToString ("0.0000") + " || Y3: " + jet.Y3.ToString ("0.0000"));
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("TAIL PIPE", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("P4: " + jet.P3.ToString ("0.00") + " KPa" + " || T4: " + jet.T4.ToString ("0.00") + " °K" + " || Cp4: " + jet.Cp4.ToString ("0.0000") + " || Y4: " + jet.Y4.ToString ("0.0000"));
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Exhaust Gas Properties", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Velocity", jet.exhaustVelocity.ToString ("0.00") + " m/s");
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Temperature (EGT)", jet.exhaustTemperature.ToString ("0.00") + " °C");
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Flows Rates", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Intake Air", jet.Ma.ToString ("0.00") + " kg/s");
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Fuel", jet.Mf.ToString ("0.00") + " kg/s");
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Efficieny Ratings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Propulsive", jet.np.ToString ("0.00") + " %");
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Thermal", jet.nth.ToString ("0.00") + " %");
			GUILayout.Space (3f);
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
			jet.ExhaustPoint = EditorGUILayout.ObjectField ("Exhaust Point", jet.ExhaustPoint, typeof(Transform), true) as Transform;
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Combustion Chamber", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			GUILayout.Space(3f);
			jet.CombustionEntry = EditorGUILayout.ObjectField ("Compressor Entry", jet.CombustionEntry, typeof(Transform), true) as Transform;
			GUILayout.Space(3f);
			jet.CombustionExit = EditorGUILayout.ObjectField ("Compressor Exit", jet.CombustionExit, typeof(Transform), true) as Transform;

			// ----------------------------------------------------------------------------------------------------------------------------------------------------------
			GUILayout.Space(25f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			jet.ExteriorIdleSound = EditorGUILayout.ObjectField ("Engine Idle Sound", jet.ExteriorIdleSound, typeof(AudioClip), true) as AudioClip;
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
			GUILayout.Space(3f);
			jet.exhaustFlame = EditorGUILayout.ObjectField ("Exhaust Flame", jet.exhaustFlame, typeof(ParticleSystem), true) as ParticleSystem;
			GUILayout.Space(3f);
			jet.exhaustGlow = EditorGUILayout.ObjectField ("Exhaust Glow", jet.exhaustGlow, typeof(ParticleSystem), true) as ParticleSystem;
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