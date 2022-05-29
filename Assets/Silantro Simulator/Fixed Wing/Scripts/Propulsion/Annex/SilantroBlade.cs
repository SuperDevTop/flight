using System.Collections;
using System.Collections.Generic;
using UnityEngine;using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroBlade : MonoBehaviour {
	//ENGINE TYPE
	public enum EngineType{TurbopropEngine,PistonEngine,TurboshaftEngine,ElectricMotor}
	[HideInInspector]public EngineType engineType = EngineType.PistonEngine;
	//CONNECTED ENGINES
	[HideInInspector]public SilantroTurboShaft shaftEngine;
	[HideInInspector]public SilantroTurboProp propEngine;
	[HideInInspector]public SilantroPistonEngine pistonEngine;
	[HideInInspector]public SilantroElectricMotor electricMotor;
	//
	//POWER SETTINGS
	[HideInInspector]public float availablePower;
	[HideInInspector]public float powerPercentage;
	[HideInInspector]public float usefulPower;
	[HideInInspector]public float bladeDiameter;
	//ROTATION SETTINGS
	public enum GearSelection{Gear1,Gear2,Gear3,Gear4,Gear5,Gear6,Gear0}
	[HideInInspector]public GearSelection gearSelection = GearSelection.Gear1;
	[HideInInspector]public float gearRatio;
	[HideInInspector]public float currentRPM;
	[HideInInspector]public float engineRPM;
	[HideInInspector]public float maximumRPM;
	[HideInInspector]public float divisionRatio =1;
	public enum RotationAxis{X,Y,Z}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	//
	//
	public enum RotationDirection{CW,CCW}
	[HideInInspector]public RotationDirection rotationDirection = RotationDirection.CCW;
	//
	//CALCULATION VALUES
	[HideInInspector]public float torque;
	[HideInInspector]public bool useTorqueEffect = false;
	float fuelFactor;
	//PROPELLER SETTINGS
	[HideInInspector]public Transform Propeller;
	[HideInInspector]public Transform thruster;
	[HideInInspector]public Rigidbody airplane;
	[HideInInspector]public bool useFastPropeller;
	float propellerRPM;
	[HideInInspector]public Material fastPropellerMaterial;
	Color fastPropellerColor;
	[HideInInspector]public float AlphaSettings;
	[HideInInspector]public bool isControllable = true;
	float superRPM;float aircraftSpeed;float airDensity;//GET FROM ENGINE
	float propellerArea;float dynamicPower;float rpmFactor;float dynamicArea;
	Vector3 torqueAxis;Vector3 actualTorqueEffect;Vector3 force;
	[HideInInspector]public float Thrust;
	float inducedAirSpeed;float omega;float inducedRPM;[HideInInspector]public bool useFastProp;
	[HideInInspector]public float J,Mv;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeBlade()
	{
		//SETUP PROPELLER
		if (fastPropellerMaterial && useFastProp) {
			fastPropellerColor = fastPropellerMaterial.color;
			AlphaSettings = 0;
		}
		//SEND START DATA
		ConfigureSupply();
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SET ENGINE PROPERTIES
	void ConfigureSupply()
	{
		divisionRatio = gearRatio;
		if (engineType == EngineType.PistonEngine)
		{
			fuelFactor = pistonEngine.coreFactor;
			thruster = pistonEngine.ExhaustPoint;
			airplane = pistonEngine.connectedAircraft;
			maximumRPM = pistonEngine.norminalRPM;
		}
		else if (engineType == EngineType.TurbopropEngine) 
		{
			fuelFactor = propEngine.coreFactor;
			thruster = propEngine.ExhaustPoint;
			airplane = propEngine.connectedAircraft;
			maximumRPM = propEngine.functionalRPM;
		}
		else if (engineType == EngineType.TurboshaftEngine)
		{
			fuelFactor = shaftEngine.coreFactor;
			thruster = shaftEngine.ExhaustPoint;
			airplane = shaftEngine.connectedAircraft;
			maximumRPM = shaftEngine.functionalRPM;
		}
		else if (engineType == EngineType.ElectricMotor)
		{
			thruster = electricMotor.Thruster;
			airplane = electricMotor.Parent;
			fuelFactor = electricMotor.coreRPM/electricMotor.ratedRPM;
			maximumRPM = electricMotor.ratedRPM;
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (isControllable) {
			//COLLECT DATA FROM CONNECTED ENGINE
			//1. PISTON
			if (engineType == EngineType.PistonEngine) {
				availablePower = pistonEngine.brakePower;
				aircraftSpeed = pistonEngine.connectedAircraft.velocity.magnitude;
				engineRPM = pistonEngine.coreRPM;
				maximumRPM = pistonEngine.norminalRPM;
				airDensity = pistonEngine.EMU.airDensity;
				fuelFactor = pistonEngine.coreFactor;
			}
			//TURBO PROP
			if (engineType == EngineType.TurbopropEngine) {
				availablePower = propEngine.brakePower;
				aircraftSpeed = propEngine.connectedAircraft.velocity.magnitude;
				engineRPM = propEngine.coreRPM;
				maximumRPM = propEngine.functionalRPM;
				airDensity = propEngine.computer.airDensity;
				fuelFactor = propEngine.coreFactor;
			}
			//TURBO SHAFT
			if (engineType == EngineType.TurboshaftEngine) {
				availablePower = shaftEngine.brakePower;
				aircraftSpeed = shaftEngine.connectedAircraft.velocity.magnitude;
				engineRPM = shaftEngine.coreRPM;
				maximumRPM = shaftEngine.functionalRPM;
				airDensity = shaftEngine.EMU.airDensity;
				fuelFactor = shaftEngine.coreFactor;
			}
			if (engineType == EngineType.ElectricMotor) {
				availablePower = electricMotor.horsePower;
				aircraftSpeed = electricMotor.Parent.velocity.magnitude;
				engineRPM = electricMotor.coreRPM;
				maximumRPM = electricMotor.ratedRPM;
				airDensity = electricMotor.EMU.airDensity;
				fuelFactor = electricMotor.coreRPM/electricMotor.ratedRPM;
			}
			currentRPM = engineRPM/divisionRatio;
			//SEND THRUST DATA
			if (currentRPM > 0) {
				CalculateThrust ();
			}
			//SEND PROPELLER DATA
			RotatePropeller();
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void RotatePropeller()
	{
		//
		if (fastPropellerMaterial && useFastProp) {
			fastPropellerMaterial.color = new Color (fastPropellerColor.r, fastPropellerColor.g, fastPropellerColor.b, AlphaSettings);
		}
		//CALCULATE AIR INDUCED RPM
		inducedAirSpeed = 17f * airplane.velocity.magnitude;
		omega = (inducedAirSpeed) / (3.14264f * bladeDiameter);
		inducedRPM = (60f * omega) / (2f * 3.14265f);
		//
		propellerRPM = engineRPM+(inducedRPM);
		//
		if (Propeller != null) {
			if (rotationDirection == RotationDirection.CCW) {
				if (rotationAxis == RotationAxis.X) {Propeller.Rotate (new Vector3 (propellerRPM / 2f * Time.deltaTime, 0, 0));}
				if (rotationAxis == RotationAxis.Y) {Propeller.Rotate (new Vector3 (0, propellerRPM / 2f * Time.deltaTime, 0));}
				if (rotationAxis == RotationAxis.Z) {Propeller.Rotate (new Vector3 (0, 0, propellerRPM / 2f * Time.deltaTime));}
			}
			if (rotationDirection == RotationDirection.CW) {
				if (rotationAxis == RotationAxis.X) {Propeller.Rotate (new Vector3 (-1f * propellerRPM / 2f * Time.deltaTime, 0, 0));}
				if (rotationAxis == RotationAxis.Y) {Propeller.Rotate (new Vector3 (0, -1f * propellerRPM / 2f * Time.deltaTime, 0));}
				if (rotationAxis == RotationAxis.Z) {Propeller.Rotate (new Vector3 (0, 0, -1f * propellerRPM / 2f * Time.deltaTime));}
			}
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CalculateThrust()
	{
		//BLADE RPM
		rpmFactor = engineRPM / maximumRPM;
		superRPM = currentRPM / 60f;AlphaSettings =  rpmFactor;
		//THRUST
		propellerArea = (3.142f * Mathf.Pow((3.28084f *bladeDiameter),2f))/4f;
		dynamicPower = Mathf.Pow((availablePower * 550f),2/3f);
		dynamicArea = rpmFactor*Mathf.Pow((2f * airDensity * 0.0624f * propellerArea),1/3f);
		//
		if (engineType != EngineType.ElectricMotor) {
			Thrust = fuelFactor * dynamicArea * dynamicPower;
		} else {
			Thrust = dynamicArea * dynamicPower;
		}


		//EFFICIENCY
		J = aircraftSpeed/((currentRPM/60f)*bladeDiameter);
		Mv = 2 * 3.142f * (bladeDiameter / 2f) * (currentRPM / 60f);
		float altitude = airplane.gameObject.transform.position.y * 3.28084f;
		float a1 = 0.000000003f * altitude * altitude;
		float a2 = 0.0021f * altitude;
		float ambientTemperature = a1 - a2 + 15.443f;
		float soundSpeed = Mathf.Pow((1.2f*287f*(273.15f+ambientTemperature)),0.5f);
		Mv /= soundSpeed;


		//TORQUE CALCULATION
		if (currentRPM > 1f) {
			torque = ((availablePower * 60 * 746) / (2 * 3.142f * (currentRPM / 60f) * 1000f)) * bladeDiameter*engineRPM/maximumRPM;
		}
		if (useTorqueEffect) {
			if (rotationDirection == RotationDirection.CCW) {
				torqueAxis = new Vector3 (0, 0, -1);
			} else if (rotationDirection == RotationDirection.CW) {
				torqueAxis = new Vector3 (0, 0, 1);
			}
			actualTorqueEffect = torqueAxis * torque;
			airplane.AddTorque (actualTorqueEffect, ForceMode.Force);
		}
		//
		//APPLY THRUST
		if (Thrust > 0f && thruster != null) {
			force = thruster.forward * Thrust;
			airplane.AddForce (force, ForceMode.Force);
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (Propeller != null) {
			float propellerRaduis = bladeDiameter / 2f;
			Handles.color = Color.red;
			if(Propeller != null){
				Handles.DrawWireDisc (Propeller.position, this.transform.forward, propellerRaduis);
			}
		}
	}
	#endif
}








#if UNITY_EDITOR
[CustomEditor(typeof(SilantroBlade))]
public class BladeEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	//
	SilantroBlade blade;
	SerializedObject bladeObject;
	//
	private void OnEnable()
	{
		blade = (SilantroBlade)target;
		bladeObject = new SerializedObject (blade);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		serializedObject.Update ();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Power Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		blade.engineType = (SilantroBlade.EngineType)EditorGUILayout.EnumPopup("Powerplant",blade.engineType);
		GUILayout.Space(3f);
		if (blade.engineType == SilantroBlade.EngineType.PistonEngine)
		{
			blade.pistonEngine = EditorGUILayout.ObjectField (" ", blade.pistonEngine, typeof(SilantroPistonEngine), true) as SilantroPistonEngine;
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Shaft Power",blade.availablePower.ToString("0.00")+ " Hp");	
		}
		if (blade.engineType == SilantroBlade.EngineType.TurbopropEngine)
		{
			blade.propEngine = EditorGUILayout.ObjectField (" ", blade.propEngine, typeof(SilantroTurboProp), true) as SilantroTurboProp;
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Shaft Power",blade.availablePower.ToString("0.00")+ " Hp");	
		}
		if (blade.engineType == SilantroBlade.EngineType.TurboshaftEngine)
		{
			blade.shaftEngine = EditorGUILayout.ObjectField (" ", blade.shaftEngine, typeof(SilantroTurboShaft), true) as SilantroTurboShaft;
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Shaft Power",blade.availablePower.ToString("0.00")+ " Hp");	
		}
		if (blade.engineType == SilantroBlade.EngineType.ElectricMotor)
		{
			blade.electricMotor = EditorGUILayout.ObjectField (" ", blade.electricMotor, typeof(SilantroElectricMotor), true) as SilantroElectricMotor;
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Shaft Power",blade.availablePower.ToString("0.00")+ " Hp");	
		}
		//
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Blade Configuration", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		blade.bladeDiameter = EditorGUILayout.Slider ("Blade Diameter",blade.bladeDiameter,0,10);
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Model Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		blade.Propeller = EditorGUILayout.ObjectField ("Propeller Transform", blade.Propeller, typeof(Transform), true)as Transform;
		//
		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Extras", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		blade.rotationAxis = (SilantroBlade.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis",blade.rotationAxis);
		GUILayout.Space(3f);
		blade.rotationDirection = (SilantroBlade.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",blade.rotationDirection);
		//
		GUILayout.Space(7f);
		blade.useFastProp = EditorGUILayout.Toggle ("Use Blurred Propeller", blade.useFastProp);
		if (blade.useFastProp) {
			GUILayout.Space(3f);
			blade.fastPropellerMaterial = EditorGUILayout.ObjectField ("Blurred Material", blade.fastPropellerMaterial, typeof(Material), true) as Material;
		}
		//
		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Torque System", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		blade.useTorqueEffect = EditorGUILayout.Toggle ("Torque Effect", blade.useTorqueEffect);
		if (blade.useTorqueEffect) {
			//
			if (blade.rotationDirection == SilantroBlade.RotationDirection.CW) {
				GUILayout.Space(3f);
				EditorGUILayout.LabelField (" ", blade.torque.ToString ("0.0") + " Nm" + " (Anti-Clockwise)");
			} else if (blade.rotationDirection == SilantroBlade.RotationDirection.CCW) {
				GUILayout.Space(3f);
				EditorGUILayout.LabelField (" ", blade.torque.ToString ("0.0") + " Nm" + " (Clockwise)");
			}
			//
		}
		//
		//
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Gear Configuration", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		blade.gearSelection = (SilantroBlade.GearSelection)EditorGUILayout.EnumPopup("Gear Selection",blade.gearSelection);
		//
		if (blade.gearSelection == SilantroBlade.GearSelection.Gear1) {
			blade.gearRatio = 17.4f;
		}
		else if (blade.gearSelection == SilantroBlade.GearSelection.Gear2) {
			blade.gearRatio = 14.6f;
		}
		else if (blade.gearSelection == SilantroBlade.GearSelection.Gear3) {
			blade.gearRatio = 11.8f;
		}
		else if (blade.gearSelection == SilantroBlade.GearSelection.Gear4) {
			blade.gearRatio = 9.0f;
		}
		else if (blade.gearSelection == SilantroBlade.GearSelection.Gear5) {
			blade.gearRatio = 6.2f;
		}
		else if (blade.gearSelection == SilantroBlade.GearSelection.Gear6) {
			blade.gearRatio = 2.4f;
		}
		else if (blade.gearSelection == SilantroBlade.GearSelection.Gear0) {
			blade.gearRatio = 1.0f;
		}
		//
		EditorGUILayout.LabelField ("Gear Ratio", blade.gearRatio.ToString("0.0"));
		//
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Current RPM", blade.currentRPM.ToString("0.0")+ " RPM");
		//
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Output", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Thrust Generated", blade.Thrust.ToString("0.00")+ " N");
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (bladeObject.targetObject, "Blade Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (blade);
			EditorSceneManager.MarkSceneDirty (blade.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif