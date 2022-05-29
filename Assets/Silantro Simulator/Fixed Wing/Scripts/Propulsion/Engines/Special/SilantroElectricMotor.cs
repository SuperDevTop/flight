using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroElectricMotor : MonoBehaviour {
	//STANDARD
	[HideInInspector]public float ratedRPM;
	[HideInInspector]public float ratedVoltage;
	[HideInInspector]public float ratedCurrent;
	//
	[HideInInspector]public float coreRPM;
	float idleRPM;
	float targetRPM;
	//INPUT
	[HideInInspector]public float inputCurrent;
	[HideInInspector]public float inputVoltage;
	[HideInInspector]public float airDensity = 1.225f;
	//
	float num5;
	[HideInInspector]public float powerRating;
	[HideInInspector]public float weight;
	//
	//ENGINE CONFIGURATION
	[HideInInspector]public bool EngineOn;
	[HideInInspector]public float engineAcceleration = 0.2f;
	[HideInInspector]public float EnginePower;
	[HideInInspector]public float enginePower;
	[HideInInspector]public bool isAccelerating;
	[HideInInspector]public bool isDestructible;
	//
	[HideInInspector]public SilantroBattery batteryPack;
	[HideInInspector]public SilantroController controller;
	//
	//ENGINE STATE
	public enum EngineState
	{
		Off,Starting,Running
	}
	[HideInInspector]public EngineState CurrentEngineState;
	//
	[HideInInspector]public float powerInput = 0.2f;
	//
	[HideInInspector]public float efficiency;
	[HideInInspector]public float torque;
	[HideInInspector]public float horsePower;
	[HideInInspector]public SilantroCore EMU;
	//ENGINE SOUND SETTINGS
	[HideInInspector]public AudioClip runningSound;
	//
	[HideInInspector]private AudioSource EngineRun;
	//
	[HideInInspector]public float idlePitch = 0.5f;
	[HideInInspector]public float maximumPitch = 1f;
	[HideInInspector]float engineSoundVolume = 1f;

	[HideInInspector]public Rigidbody Parent;
	[HideInInspector]public Transform Thruster;
	//
	[HideInInspector]public bool start;[HideInInspector]public bool stop;[HideInInspector]public bool starting;
	//
	[HideInInspector]public float voltageFactor = 1f;
	[HideInInspector]public bool isControllable = true;




	//ENGINE CONTROL FUNCTIONS
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void StartEngine()
	{
		if (runningSound == null) {
			Debug.Log ("Engine " + transform.name + " cannot start due to incorrect Audio configuration");
		} else {
			//JUMP START ENGINE
			EngineOn = true;
			RunEngine ();
			starting = false;
			CurrentEngineState = EngineState.Running;
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ShutDownEngine()
	{
		stop = true;
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (Parent != null && EMU != null && batteryPack != null) {
			allOk = true;
		} else if (batteryPack == null) {
			Debug.LogError("Prerequisites not met on Motor "+transform.name + "....battery not attached");
			allOk = false;
		}
		else if (EMU == null) {
			Debug.LogError("Prerequisites not met on Motor "+transform.name + "....control module not connected");
			allOk = false;
		}
		else if (Parent == null) {
			Debug.LogError("Prerequisites not met on Motor "+transform.name + "....helicopter not connected");
			allOk = false;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeMotor()
	{
		
		//CHECK SYSTEMS
		_checkPrerequisites();
		//


		if (allOk) {
			//SET IDLE RPM VALUES
			idleRPM = ratedRPM*0.1f;
			//SETUP SOUND SYSTEM
			GameObject thrustee = new GameObject("Thruster");
			Thruster = thrustee.transform;Thruster.parent = transform;Thruster.localPosition = new Vector3 (0, 0, 0);
			if (null != runningSound)
			{
				EngineRun = Thruster.gameObject.AddComponent<AudioSource>();
				EngineRun.clip = runningSound;
				EngineRun.loop = true;
				EngineRun.Play();
				EngineRun.volume = 0f;
				EngineRun.spatialBlend = 1f;
				EngineRun.dopplerLevel = 0f;
				EngineRun.rolloffMode = AudioRolloffMode.Custom;
				EngineRun.maxDistance = 600f;
			}

			//RESET VALUES
			EngineOn = false;
			starting = false;start = false;stop = false;
			inputCurrent = ratedCurrent*voltageFactor;
		}

	}




	// ----------------------------------------DEPLETE BATTERY------------------------------------------------------------------------------------------------------------------
	void UseBattery()
	{
		if (batteryPack) {
			//batteryPack.currentCapacity -= (ratedCurrent*powerInput * ratedVoltage);
			//NOT REALLY SURE HOW THIS WORKS :))
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACCELERATE AND DECELERATE ENGINE
	private void CoreEngine()
	{
		if (EngineOn){
			if (enginePower < 1f && !isAccelerating){
				//REV UP ENGINE
				enginePower += Time.deltaTime * engineAcceleration;
			}
		}
		else if (enginePower > 0f){enginePower -= Time.deltaTime * engineAcceleration;}
		if (!EngineOn && enginePower < 0) {enginePower = 0f;}
		if (enginePower > 1) {enginePower = 1f;}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public IEnumerator ReturnIgnition()
	{
		yield return new WaitForSeconds (0.5f);
		start = false;stop = false;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void RunEngine()
	{
		//
		powerInput = Mathf.Clamp(powerInput,0f,1f);
		targetRPM = idleRPM + (ratedRPM - idleRPM) * powerInput;
		//
		if (stop)
		{
			CurrentEngineState = EngineState.Off;
			EngineOn = false;horsePower = 0;powerInput = 0f;
			StartCoroutine(ReturnIgnition());
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void ShutdownEngineProcedure()
	{
		if (start)
		{
			EngineOn = true;
			CurrentEngineState = EngineState.Running;
			starting = true;
			StartCoroutine(ReturnIgnition());
		}
		targetRPM = 0f;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void StartEngineProcedure()
	{
		if (starting)
		{
			CurrentEngineState = EngineState.Running;
			starting = false;RunEngine();
		}
		else{CurrentEngineState = EngineState.Off;}targetRPM = idleRPM;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CalculatePower()
	{
		if (EMU != null) {
			airDensity = EMU.airDensity;
		}
		//
		inputVoltage = voltageFactor *ratedVoltage;
		powerRating = inputCurrent * inputVoltage*(coreRPM/ratedRPM);
		batteryPack.outputCurrent = inputCurrent;
		batteryPack.outputVoltage = inputVoltage;
		//
		torque = (powerRating*efficiency*60f)/(coreRPM*2f*3.142f*100f);
		horsePower =(coreRPM/ratedRPM)* (torque*coreRPM)/5252f;
		UseBattery();
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (allOk && isControllable) {
			//SEND CORE DATA
			CoreEngine();
			//SEND CALCULATION DATA
			if (enginePower > 0f) {CalculatePower ();}
			//
			if (runningSound) {
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
			else {
				Debug.Log ("Sound for Engine " + transform.name + " has not been properly assigned");
				allOk = false;
			}


			//INTERPOLATE ENGINE RPM
			if (EngineOn) {
				coreRPM = Mathf.Lerp (coreRPM, targetRPM, 0.5f * Time.deltaTime * (enginePower));
			} else {
				coreRPM = Mathf.Lerp (coreRPM, 0.0f, 0.5f * Time.deltaTime);
			}
			if (coreRPM <= 0) {coreRPM = 0f;}



			if (runningSound && Parent != null) {
				if (Parent != null) {
					//PERFORM MINOR ENGINE CALCULATIONS TO CONTROL SOUND PITCH
					float magnitude = Parent.velocity.magnitude;
					float num2 = magnitude * 1.94384444f;
					float num3 = coreRPM + num2 * 10f;
					float num4 = (num3 - idleRPM) / (ratedRPM - idleRPM);
					num5 = idlePitch + (maximumPitch - idlePitch) * num4;
					num5 = Mathf.Clamp (num5, 0f, maximumPitch);
				}

				//SET
				engineSoundVolume = num5;
				if (engineSoundVolume > 0.0001f && engineSoundVolume < 3f && runningSound != null) {
					EngineRun.pitch = engineSoundVolume * enginePower * (coreRPM/ratedRPM);
					EngineRun.volume = engineSoundVolume * enginePower * (coreRPM/ratedRPM);
				}
			}
		}
	}
}




#if UNITY_EDITOR
[CustomEditor(typeof(SilantroElectricMotor))]
public class ElectricMotorEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	[HideInInspector]public int toolbarTab;
	[HideInInspector]public string currentTab;

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();	serializedObject.Update();
		//
		SilantroElectricMotor motor = (SilantroElectricMotor)target;
		//

		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Motor Specifications", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.ratedVoltage = EditorGUILayout.FloatField ("Rated Voltage", motor.ratedVoltage);
		GUILayout.Space(3f);
		motor.ratedCurrent = EditorGUILayout.FloatField ("Rated Current", motor.ratedCurrent);
		GUILayout.Space(3f);
		motor.efficiency = EditorGUILayout.Slider ("Motor Efficiency", motor.efficiency, 0f, 100f);
		GUILayout.Space(8f);
		motor.ratedRPM = EditorGUILayout.FloatField ("Rated RPM", motor.ratedRPM);
		GUILayout.Space(3f);
		motor.engineAcceleration = EditorGUILayout.Slider ("Motor Acceleration",motor.engineAcceleration,0.01f,1f);
		GUILayout.Space(3f);
		motor.weight = EditorGUILayout.FloatField ("Weight", motor.weight);
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Power Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.batteryPack = EditorGUILayout.ObjectField ("Battery Pack", motor.batteryPack, typeof(SilantroBattery), true) as SilantroBattery;
		GUILayout.Space(8f);
		EditorGUILayout.LabelField ("Input Voltage", motor.inputVoltage.ToString ("0.0") + " Volts");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Input Current", motor.inputCurrent.ToString ("0.0") + " Amps");
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		motor.runningSound = EditorGUILayout.ObjectField ("Running Sound", motor.runningSound, typeof(AudioClip), true) as AudioClip;
		GUILayout.Space(10f);
		motor.idlePitch = EditorGUILayout.FloatField ("Motor Idle Pitch", motor.idlePitch);
		GUILayout.Space(3f);
		motor.maximumPitch = EditorGUILayout.FloatField ("Motor Maximum Pitch", motor.maximumPitch);

		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Performance", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Motor State", motor.CurrentEngineState.ToString ());
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Motor Power", (motor.powerRating/1000).ToString ("0.00") + " kW");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Shaft Horsepower", motor.horsePower.ToString ("0.0") + " Hp");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Core Speed", motor.coreRPM.ToString ("0.0") + " RPM");
		////
		if (GUI.changed) {
			EditorUtility.SetDirty (motor);
			EditorSceneManager.MarkSceneDirty (motor.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif