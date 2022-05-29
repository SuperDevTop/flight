using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroRocketMotor : MonoBehaviour {
	//FUNCTION TYPE
	public enum BoosterType
	{
		Aircraft,
		Weapon,
		//ADD MORE IF NEEDED
	}
	[HideInInspector]public BoosterType boosterType = BoosterType.Weapon;
	//FUEL TYPE
	public enum FuelType
	{
		Liquid,Solid
	}
	[HideInInspector]public FuelType fuelType = FuelType.Liquid;
	//LIQUID FUEL
	public enum LiquidFuelType
	{
		RP1,RP2,Hydrogen,MMH,UDMHComposite
	}
	[HideInInspector]public LiquidFuelType liquidfuelType = LiquidFuelType.RP1;
	//SOLID FUEL
	public enum SolidFuelType
	{
		PBSR,//236
		ThiokolAmmoniumPerchlorate,//202
		//RubberAmmoniumPerchlorate,
		PolyurethaneAmmoniumPerchlorate,//241
		NitropolymerAmmoniumPerchlorate,//222
		DoublebaeAmmonumNitrate,//250
		NitropolymerAmmoniumNitrate//215
	}
	//
	[HideInInspector]public AnimationCurve thrustCurve;
	[HideInInspector]public float burnTime = 5;
	//FUEL PROPERTIES
	[HideInInspector]public float specificImpulse;
	[HideInInspector]public float heatingValue;
	[HideInInspector]public float density;
	[HideInInspector]public float combustionTemperature;
	[HideInInspector]public float molarMass;
	[HideInInspector]public float chamberPressure;
	//
	//Performance
	[HideInInspector]public float Thrust;
	[HideInInspector]public float flowRate;
	[HideInInspector]public float fuelFlowRate;
	[HideInInspector]public float currentSpeed;
	float areaFactor;
	[HideInInspector]public float exhaustTemperature; 
	[HideInInspector]public float effectiveExhautVelocity;
	[HideInInspector]public float theoreticalExhaustVelocity;
	[HideInInspector]public float meanExhaustVelocity;
	//
	//Pressures
	[HideInInspector]public float atmosphericPressure;
	[HideInInspector]public float nozzlePressure;
	[HideInInspector]public float nozzleArea;
	[HideInInspector]public float throatArea;
	float coefficientFactor;
	[HideInInspector]public float thrustCoefficient;
	//
	//EXHAUST EFFECTS
	[HideInInspector]public ParticleSystem exhaustSmoke;
	[HideInInspector]ParticleSystem.EmissionModule smokeModule;
	[HideInInspector]public ParticleSystem exhaustFlame;
	[HideInInspector]ParticleSystem.EmissionModule flameModule;
	[HideInInspector]public float maximumSmokeEmissionValue = 50f;
	[HideInInspector]public float maximumFlameEmissionValue = 50f;
	//
	[HideInInspector]public AudioClip motorSound;
	AudioSource boosterSound;
	[HideInInspector]public float maximumPitch =1.2f;
	//
	[HideInInspector]public Rigidbody aircraft;
	[HideInInspector]public Rigidbody weapon;
	[HideInInspector]public SilantroComputer computer;
	[HideInInspector]public float runTime;
	[HideInInspector]public bool active;
	float fuelFactor;
	[HideInInspector]public float nozzleDiameterPercentage = 1f;
	[HideInInspector]public float nozzleDiameter = 1f;
	[HideInInspector]public float boosterDiameter = 1f;
	[HideInInspector]public float demoArea;
	[HideInInspector]public float overallLength = 1f;
	[HideInInspector]public float exhaustConeLength;
	[HideInInspector]public float fuelLength;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void FireRocket()
	{
		active = true;
		runTime = 0.0f;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeRocket()
	{
		//SETUP FACTORS
		nozzleDiameter = boosterDiameter * nozzleDiameterPercentage/100f;
		demoArea = (3.142f * nozzleDiameter * nozzleDiameter) / 4f;
		nozzleArea = demoArea;
		//SETUP PARTICULES
		if (exhaustSmoke != null) {smokeModule = exhaustSmoke.emission;smokeModule.rateOverTime = 0.0f;}
		if (exhaustFlame != null) {flameModule = exhaustFlame.emission;flameModule.rateOverTime = 0.0f;}
		//SETUP SOUND
		GameObject soundPoint = new GameObject("Booster Sound");
		soundPoint.transform.parent = this.transform;
		soundPoint.transform.localPosition = new Vector3 (0, 0, 0);
		boosterSound = soundPoint.AddComponent<AudioSource>();
		boosterSound.dopplerLevel = 0f;
		boosterSound.spatialBlend = 1f;
		boosterSound.rolloffMode = AudioRolloffMode.Custom;
		boosterSound.maxDistance = 650f;
		boosterSound.clip = motorSound;
		boosterSound.loop = true;boosterSound.Play ();boosterSound.volume = 0.0f;
		//
		areaFactor = (0.0088f* (chamberPressure)) + 0.96f;
		throatArea = nozzleArea / areaFactor;
		float exhaustFactor = (-0.0004f*(chamberPressure))+0.7488f;
		exhaustTemperature = combustionTemperature * exhaustFactor;
		effectiveExhautVelocity = 9.801f*specificImpulse;
		float velocityFactor = (97.608f * exhaustTemperature) / molarMass;
		theoreticalExhaustVelocity = Mathf.Pow (velocityFactor, 0.5f);
		meanExhaustVelocity = (theoreticalExhaustVelocity+effectiveExhautVelocity)/2f;
		//
		float flowFactor1 = ((2.6066f*molarMass)/(8.134f*combustionTemperature));
		float flowFactor2 = Mathf.Pow (flowFactor1, 0.5f);
		flowRate = chamberPressure * throatArea * flowFactor2;
		fuelFlowRate = flowRate;
		nozzlePressure = 0.564f*chamberPressure;
		//
		//SOLID BOOSTER
		//Calculations based on assumption that gamma y=1.4
		float a = (nozzlePressure / chamberPressure);
		float b = Mathf.Pow (a, 0.3857f);
		float c = 7.7f * b * (1 - b)*0.1937f;
		float f = nozzleArea / throatArea;
		coefficientFactor = c*f;
		//SEND CURVE DATA
		DrawCurve();
		//
		if (computer == null) {
			SilantroComputer childComputer = GetComponentInChildren<SilantroComputer> ();
			if (childComputer != null) {computer = childComputer;} 
			else {
				Debug.Log ("Please add a computer to rocket booster " + transform.name);
			}
		}
		if (boosterType == BoosterType.Aircraft){
			if (aircraft != null && computer != null) {computer.connectedSystem = aircraft;} 
			else {
				Debug.Log ("Aircraft has not been assigned");
			}
		}
	}











	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	////SETUP THRUST CURVE
	void DrawCurve()
	{
		if (burnTime > 0) {
			thrustCurve = new AnimationCurve ();
			//1. TIME
			float a1 = 0.10f * burnTime;
			float b1 = 0.20f * burnTime;
			float c1 = 0.30f * burnTime;
			float d1 = 0.40f * burnTime;
			float e1 = 0.50f * burnTime;
			float f1 = 0.60f * burnTime;
			float g1 = 0.70f * burnTime;
			float h1 = 0.80f * burnTime;
			float i1 = 0.90f * burnTime;
			float j1 = 1.0f * burnTime;
			//2. THRUST PERCENTAGE
			Keyframe a2 = new Keyframe (0, 80);
			Keyframe b2 = new Keyframe (a1, 90f);
			Keyframe c2 = new Keyframe (b1, 96f);
			Keyframe d2 = new Keyframe (c1, 100f);

			Keyframe e2 = new Keyframe (d1, 100f);
			Keyframe f2 = new Keyframe (e1, 100f);
			Keyframe g2 = new Keyframe (f1, 100f);
			Keyframe h2 = new Keyframe (g1, 80);
			Keyframe i2 = new Keyframe (h1, 68);
			Keyframe j2 = new Keyframe (i1, 40);
			Keyframe k2 = new Keyframe (j1, 0);
			//3. SUBMIT
			thrustCurve.AddKey (a2);
			thrustCurve.AddKey (b2);
			thrustCurve.AddKey (c2);
			thrustCurve.AddKey (d2);
			thrustCurve.AddKey (e2);
			thrustCurve.AddKey (f2);
			thrustCurve.AddKey (g2);
			thrustCurve.AddKey (h2);
			thrustCurve.AddKey (i2);
			thrustCurve.AddKey (j2);
			thrustCurve.AddKey (k2);
		}
	}










	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		//SEND CURVE DATA
		DrawCurve();
		//DRAW MARKERS
		nozzleDiameter = boosterDiameter * nozzleDiameterPercentage/100f;
		demoArea = (3.142f * nozzleDiameter * nozzleDiameter) / 4f;
		Handles.color = Color.red;
		Handles.DrawWireDisc (this.transform.position,this.transform.forward, (nozzleDiameter/2f)); 
		//
		Vector3 throatPoint = transform.position + transform.forward * exhaustConeLength;
		Vector3 fuelPoint = throatPoint +transform.forward * fuelLength;
		//
		Handles.color = Color.blue;
		Handles.DrawWireDisc (fuelPoint, transform.forward, (boosterDiameter / 2f));
		//
		Handles.color = Color.red;
		Handles.DrawLine (fuelPoint, throatPoint); 
		//
	}
	#endif







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DO MATHEMATICAL CALCULATIONS
	void RocketCalculations()
	{
		//COLLECT VARIBLES
		fuelFactor = thrustCurve.Evaluate(runTime);
		atmosphericPressure = computer.ambientPressure;
		//CALCULATE THRUST
		if (fuelType == FuelType.Liquid) { 
			Thrust = (fuelFlowRate * meanExhaustVelocity )+ ((nozzlePressure*6894.757f) - (atmosphericPressure*1000f)) * nozzleArea ;
		}
		if (fuelType == FuelType.Solid) {
			//Calculations based on assumption that gamma y=1.4
			float d = ((chamberPressure*6894.757f) - (atmosphericPressure*1000f)) / (chamberPressure*6894.757f);
			thrustCoefficient = coefficientFactor * d;
			Thrust = (chamberPressure*6894.757f) * throatArea * thrustCoefficient*(fuelFactor/100f);
		}
		//ANNEX
		float soundVolume = maximumPitch *(fuelFactor/100f);
		boosterSound.volume = soundVolume;
		if (exhaustFlame) {flameModule.rateOverTime = maximumFlameEmissionValue * (fuelFactor / 100f);}
		if (exhaustSmoke) {smokeModule.rateOverTime = maximumSmokeEmissionValue *(fuelFactor/100f);}
		//STOP ROCKET
		if (runTime > burnTime) {active = false;}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (active) {
			runTime += Time.deltaTime;
			//SEND CALCULATIONS DATA
			RocketCalculations ();
			currentSpeed = computer.currentSpeed;
			//
			//APPLY THRUST
			if (Thrust > 0) {
				Vector3 force = this.transform.forward * Thrust;
				//
				if (boosterType == BoosterType.Aircraft) {
					aircraft.AddForce (force, ForceMode.Force);
				}
				if (boosterType == BoosterType.Weapon) {
					weapon.AddForce (force, ForceMode.Force);
				}
			}
		}
	}
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroRocketMotor))]
[CanEditMultipleObjects]
public class RocketMotorEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroRocketMotor motor;
	SerializedObject motorObject;
	//
	private void OnEnable()
	{
		motor = (SilantroRocketMotor)target;
		motorObject = new SerializedObject (motor);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		motorObject.UpdateIfRequiredOrScript();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Motor Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.boosterType = (SilantroRocketMotor.BoosterType)EditorGUILayout.EnumPopup("Booster Connection",motor.boosterType);
		if (motor.boosterType == SilantroRocketMotor.BoosterType.Aircraft) {
			GUILayout.Space(3f);
			motor.aircraft = EditorGUILayout.ObjectField ("Connected Aircraft", motor.aircraft, typeof(Rigidbody), true) as Rigidbody;
			GUILayout.Space(15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Booster Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			//
			motor.boosterDiameter = EditorGUILayout.FloatField("Booster Diameter",motor.boosterDiameter);
			GUILayout.Space(3f);
			motor.overallLength = EditorGUILayout.FloatField("Booster Length",motor.overallLength);
			GUILayout.Space(2f);
			motor.nozzleDiameterPercentage = EditorGUILayout.Slider ("Nozzle Diameter Percentage",motor.nozzleDiameterPercentage,0,100);
			//
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Nozzle Diameter", motor.nozzleDiameter.ToString ("0.00") + " m");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Nozzle Area", motor.demoArea.ToString ("0.00") + " m2");
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Fuel Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			motor.exhaustConeLength = EditorGUILayout.Slider ("Exhaust Cone Length", motor.exhaustConeLength,0f,motor.overallLength/2f);
			GUILayout.Space(2f);
			motor.fuelLength = EditorGUILayout.Slider ("Fuel Length", motor.fuelLength,0f,motor.overallLength);
		}
		if (motor.boosterType == SilantroRocketMotor.BoosterType.Weapon) {
			GUILayout.Space(3f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Motor Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			motor.nozzleDiameterPercentage = EditorGUILayout.Slider ("Nozzle Diameter Percentage",motor.nozzleDiameterPercentage,0,100);
			//
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Nozzle Diameter", motor.nozzleDiameter.ToString ("0.00") + " m");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Nozzle Area", motor.demoArea.ToString ("0.00") + " m2");
			GUILayout.Space(3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Fuel Dimensions", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			motor.exhaustConeLength = EditorGUILayout.Slider ("Exhaust Cone Length", motor.exhaustConeLength,0f,motor.overallLength/2f);
			GUILayout.Space(2f);
			motor.fuelLength = EditorGUILayout.Slider ("Fuel Length", motor.fuelLength,0f,motor.overallLength);
		}
		//
		EditorGUILayout.LabelField ("Throat Area", (motor.throatArea*10000).ToString ("0.00") + " cm2");
		GUILayout.Space(2f);
		motor.chamberPressure = EditorGUILayout.FloatField ("Chamber Pressure", motor.chamberPressure);
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Fuel Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.fuelType = (SilantroRocketMotor.FuelType)EditorGUILayout.EnumPopup ("Fuel Type", motor.fuelType);
		GUILayout.Space(3f);
		//
		if (motor.fuelType == SilantroRocketMotor.FuelType.Liquid) {
			//
			motor.liquidfuelType = (SilantroRocketMotor.LiquidFuelType)EditorGUILayout.EnumPopup("Liquid Fuel Type",motor.liquidfuelType);
			//
			if (motor.liquidfuelType == SilantroRocketMotor.LiquidFuelType.RP1) {
				motor.specificImpulse = 358f;
				motor.heatingValue = 47f;
				motor.density = 0.91f;
				motor.molarMass = 27.6f;
				motor.combustionTemperature = 3670f;
			} else if (motor.liquidfuelType == SilantroRocketMotor.LiquidFuelType.Hydrogen) {
				motor.specificImpulse = 391f;
				motor.heatingValue = 141.7f;
				motor.density = 0.090f;
				motor.molarMass = 28.2f;
				motor.combustionTemperature = 2985f;
			} else if (motor.liquidfuelType == SilantroRocketMotor.LiquidFuelType.MMH) {
				motor.specificImpulse = 292f;
				motor.heatingValue = 47.8f;
				motor.density = 1.021f;
				motor.combustionTemperature = 910f;
			} else if (motor.liquidfuelType == SilantroRocketMotor.LiquidFuelType.RP2) {
				motor.specificImpulse = 373.5f;
				motor.heatingValue = 43.2f;
				motor.density = 0.821f;
				motor.combustionTemperature = 3459f;
			} 
			//
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Density",motor.density.ToString("0.000")+" kg/m3");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Specific Impulse",motor.specificImpulse.ToString("0")+" s");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Heating Value",motor.heatingValue.ToString("0.0")+" MJ/kg");
			GUILayout.Space(1f);
			EditorGUILayout.LabelField ("Combustion Temperature",motor.combustionTemperature.ToString("0")+" °K");
			//
		}
		if (motor.fuelType == SilantroRocketMotor.FuelType.Solid) {
			GUILayout.Space(3f);
			motor.burnTime = EditorGUILayout.FloatField ("Burn Time", motor.burnTime);
			GUILayout.Space(2f);
			EditorGUILayout.CurveField ("Thrust Curve", motor.thrustCurve);
		}
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Rocket Effects", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.exhaustSmoke = EditorGUILayout.ObjectField ("Exhaust Smoke", motor.exhaustSmoke, typeof(ParticleSystem), true) as ParticleSystem;
		GUILayout.Space (2f);
		motor.maximumSmokeEmissionValue = EditorGUILayout.FloatField ("Maximum Emission", motor.maximumSmokeEmissionValue);
		GUILayout.Space(5f);
		motor.exhaustFlame = EditorGUILayout.ObjectField ("Exhaust Flame", motor.exhaustFlame, typeof(ParticleSystem), true) as ParticleSystem;
		GUILayout.Space (2f);
		motor.maximumFlameEmissionValue = EditorGUILayout.FloatField ("Maximum Emission", motor.maximumFlameEmissionValue);
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.motorSound = EditorGUILayout.ObjectField ("Booster Sound", motor.motorSound, typeof(AudioClip), true) as AudioClip;
		GUILayout.Space(2f);
		motor.maximumPitch = EditorGUILayout.FloatField ("Maximum Pitch", motor.maximumPitch);
		//
		GUILayout.Space(20f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Thrust Generated", motor.Thrust.ToString ("0.00") + " N");
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (motorObject.targetObject, "Rocket Motor Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (motor);
			EditorSceneManager.MarkSceneDirty (motor.gameObject.scene);
		}
		motorObject.ApplyModifiedProperties();
	}
}
#endif