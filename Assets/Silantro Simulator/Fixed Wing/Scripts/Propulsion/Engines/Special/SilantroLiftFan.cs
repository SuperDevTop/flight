//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
using System.IO;
using System.Text;
//
public class SilantroLiftFan : MonoBehaviour {
	[HideInInspector]public string engineIdentifier = "Default Engine";
	//FAN STATE
	public enum FanState
	{
		Off,Clutching,Active
	}
	[HideInInspector]public FanState CurrentFanState = FanState.Off;
	//START MODE
	public enum EngineStartMode
	{
		Cold,
		Hot
	}
	[HideInInspector]public EngineStartMode engineStartMode = EngineStartMode.Cold;
	//CONTROL BOOLS
	private bool starting;
	[HideInInspector]public bool start;
	[HideInInspector]public bool stop;
	[HideInInspector]public bool isAccelerating;
	[HideInInspector]public bool fanOn;
	//CALCULATION VARIABLES
	[HideInInspector]public float fanPower;
	[HideInInspector]public float fanShaftPower;
	[HideInInspector]public float fanThrust;
	[HideInInspector]public float fanDiameter = 1f;
	[HideInInspector]public float fanAcceleration = 0.2f;
	[HideInInspector]public float fanEfficiency = 86f;
	[HideInInspector]public float extractionRatio = 60f;//Percentage of power extracted
	[HideInInspector]public float airDensity = 1.225f;
	//SOUND CONFIG
	[HideInInspector]public AudioClip fanStartClip;
	[HideInInspector]public AudioClip fanRunningClip;
	[HideInInspector]public AudioClip fanShutdownClip;
	private float idlePitch = 0.5f;
	private float maximumPitch = 1.2f;
	private float fanSoundVolume = 1.5f;
	private AudioSource fanStart;
	private AudioSource fanRun;
	private AudioSource fanShutdown;
	//EXTERNAL CONNECTIONS
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public SilantroTurboFan attachedEngine;
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Transform Thruster;
	[HideInInspector]public Transform fan;
	[HideInInspector]public ParticleSystem engineDistortion;
	[HideInInspector]public SilantroCore coreSystem;
	//
	[HideInInspector]ParticleSystem.EmissionModule distortionModule;
	[HideInInspector]public float maximumDistortionEmission = 20;
	//ROTATION SETTINGS
	public enum RotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	public enum RotationDirection
	{
		CW,CCW
	}
	[HideInInspector]public RotationDirection rotationDirection = RotationDirection.CCW;
	//RPM 
	[HideInInspector]public float CurrentRPM;
	[HideInInspector]public float DesiredRPM;
	private float FanIdleRPM = 10f;
	private float FanMaximumRPM = 100f;






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeEngine()
	{
		//RECIEVE CONNECTIONS
		connectedAircraft = attachedEngine.connectedAircraft;
		FanMaximumRPM = attachedEngine.functionalRPM * (extractionRatio / 100f);
		FanIdleRPM = FanMaximumRPM * 0.3f;
		//SETUP SOUND PARTS
		SetupSoundSystem();
		//SET ENGINE JUMP START VALUE;
		if (engineStartMode == EngineStartMode.Hot) {
			fanAcceleration = 10f;
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SOUND SYSTEM
	void SetupSoundSystem()
	{
		if (null != fanStartClip)
		{
			fanStart = Thruster.gameObject.AddComponent<AudioSource>();
			fanStart.clip = fanStartClip;
			fanStart.loop = false;
			fanStart.dopplerLevel = 0f;
			fanStart.spatialBlend = 1f;
			fanStart.rolloffMode = AudioRolloffMode.Custom;
			fanStart.
			maxDistance = 650f;
		}
		if (null != fanRunningClip)
		{
			fanRun = Thruster.gameObject.AddComponent<AudioSource>();
			fanRun.clip =  fanRunningClip;
			fanRun.loop = true;
			fanRun.Play();
			fanRun.volume = 0f;
			fanRun.spatialBlend = 1f;
			fanRun.dopplerLevel = 0f;
			fanRun.rolloffMode = AudioRolloffMode.Custom;
			fanRun.maxDistance = 600f;
		}
		if (null != fanShutdownClip)
		{
			fanShutdown = Thruster.gameObject.AddComponent<AudioSource>();
			fanShutdown.clip = fanShutdownClip;
			fanShutdown.loop = false;
			fanShutdown.dopplerLevel = 0f;
			fanShutdown.spatialBlend = 1f;
			fanShutdown.rolloffMode = AudioRolloffMode.Custom;
			fanShutdown.maxDistance = 650f;
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if(controller != null){
		//RUN SOUND
		if (fanRun != null) {
			//CALCULATE BASE FACTORS
			float rpmControl = (CurrentRPM - FanIdleRPM) / (FanMaximumRPM - FanIdleRPM);
			float pitchControl = idlePitch + (maximumPitch - idlePitch) * rpmControl;
			pitchControl = Mathf.Clamp(pitchControl, 0f, maximumPitch);
			fanRun.pitch = pitchControl*fanPower;
			if (CurrentRPM < FanIdleRPM * 0.1f) {
				fanRun.volume = 0f;
			} else {
					if (controller.currentSoundState == SilantroController.SoundState.Exterior) {
						fanRun.volume = fanSoundVolume * pitchControl;
					} else {
						fanRun.volume = fanSoundVolume * pitchControl*0.25f;
					}
			}
		}
		//CONTROL FAN STATE
		FanPowering();
		//SEND CALCULATIO
		if (fanPower > 0f) {FanCalculation ();}
		//
		if (connectedAircraft) {
			switch (CurrentFanState) {
			case FanState.Off:
				ShutDown ();
				break;
			case FanState.Clutching:
				Clutching ();
				break;
			case FanState.Active:
				Running ();
				break;
			}
		}
		//RPM REV
		if (fanOn) {
			CurrentRPM =   Mathf.Lerp (CurrentRPM, DesiredRPM,(fanAcceleration*4f) * Time.deltaTime * fanPower);
		} else {
			CurrentRPM =  Mathf.Lerp (CurrentRPM, 0.0f, (fanAcceleration*4f) * Time.deltaTime);
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SHUTDOWN
	private void ShutDown()
	{
		if (fanStart.isPlaying) {
			fanStart.Stop ();
			start = false;
		}
		if (start && attachedEngine != null && attachedEngine.active == true) {
			fanOn = true;
			fanStart.Play ();
			//attachedEngine.HighPressureFanRPM = Mathf.Lerp (attachedEngine.HighPressureFanRPM, FanMaximumRPM, 1.5f);
			CurrentFanState = FanState.Clutching;
			starting = true;
			StartCoroutine (ReturnIgnition ());
		}
		DesiredRPM = 0f;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//START FAN
	private void Clutching()
	{
		if (starting) {
			if (!fanStart.isPlaying) {
				CurrentFanState = FanState.Active;
				starting = false;
				Running();
			}
		}
		else
		{
			fanStart.Stop();
			CurrentFanState = FanState.Off;
		}
		DesiredRPM = FanIdleRPM;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RUN FAN
	private void Running()
	{
		if (fanStart.isPlaying) {
			fanStart.Stop ();
		}
		DesiredRPM = FanIdleRPM + (FanMaximumRPM - FanIdleRPM) * attachedEngine.FuelInput;
		if (stop)
		{
			CurrentFanState = FanState.Off;
			fanOn = false;
			fanShutdown.Play();
			StartCoroutine(ReturnIgnition());
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//POWER UP FAN
	private void FanPowering()
	{
		if (fanOn) {
			if (fanPower < 1f && !isAccelerating) {fanPower += Time.deltaTime * fanAcceleration;}
		} 
		else if (fanPower > 0f) {fanPower -= Time.deltaTime * fanAcceleration;} 
		else {fanPower = 0f;}
		if (fanPower > 1) {fanPower = 1f;}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	IEnumerator ReturnIgnition()
	{
		yield return new WaitForSeconds (0.5f);
		start = false;
		stop = false;
	}



	float rpmFactor;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE FAN THRUST
	void FanCalculation()
	{
		if (coreSystem != null) {
			airDensity = coreSystem.airDensity;
		} else {
			airDensity = 1.225f;
		}

		//---------------------------------USE THIS FOR OLD TURBOFAN ENGINE
//		//Calculate FAN INTAKE AREA
//		float fanIntakeArea = (3.142f * fanDiameter *fanDiameter)/4f;
//		float radialSpeed = (2*3.142f*CurrentRPM)/60f;
//		float tipSpeed = (radialSpeed * fanDiameter / 2f);
//		//CALCULATE FAN THRUST
//		fanThrust = 0.04f *airDensity*fanIntakeArea *tipSpeed*tipSpeed;

		rpmFactor = CurrentRPM / FanMaximumRPM;
		if (attachedEngine != null) {
			fanShaftPower = attachedEngine.Wc*(extractionRatio/100);
			float propellerArea = (3.142f * Mathf.Pow((3.28084f *fanDiameter),2f))/4f;
			float dynamicPower = Mathf.Pow((fanShaftPower * 550f),2/3f);
			float dynamicArea = rpmFactor*Mathf.Pow((2f * airDensity * 0.0624f * propellerArea),1/3f);
			fanThrust = dynamicArea * dynamicPower;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (connectedAircraft != null) {
			Handles.color = Color.red;
			if (Thruster != null) {
				Handles.DrawWireDisc (Thruster.transform.position, connectedAircraft.transform.up, 0.2f);
			}
			Handles.color = Color.blue;
			if (fan != null && connectedAircraft != null) {
				Handles.DrawWireDisc (fan.transform.position, connectedAircraft.transform.up, (fanDiameter / 2f));
			}
		}
		//
		if (Thruster != null) {
			Handles.color = Color.cyan;
			if (Thruster != null && fan != null) {
				Handles.DrawLine (fan.transform.position, Thruster.position);
			}
		}
	}
	#endif





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public float forceFactor,liftFactor;
	[HideInInspector]public float fanLift;
	private void FixedUpdate()
	{
		if (rotationDirection == RotationDirection.CCW) {
			if (rotationAxis == RotationAxis.X) {fan.Rotate (new Vector3 (CurrentRPM * Time.deltaTime, 0, 0));}
			if (rotationAxis == RotationAxis.Y) {fan.Rotate (new Vector3 (0, CurrentRPM * Time.deltaTime, 0));}
			if (rotationAxis == RotationAxis.Z) {fan.Rotate (new Vector3 (0, 0, CurrentRPM * Time.deltaTime));}
		}
		//
		if (rotationDirection == RotationDirection.CW) {
			if (rotationAxis == RotationAxis.X) {fan.Rotate (new Vector3 (-1f * CurrentRPM * Time.deltaTime, 0, 0));}
			if (rotationAxis == RotationAxis.Y) {fan.Rotate (new Vector3 (0, -1f * CurrentRPM * Time.deltaTime, 0));}
			if (rotationAxis == RotationAxis.Z) {fan.Rotate (new Vector3 (0, 0, -1f * CurrentRPM * Time.deltaTime));}
		}
		if (CurrentRPM <= 0f){CurrentRPM = 0f;}
		//CALCULATE SPEED FACTOR
		float speedFactor = connectedAircraft.velocity.magnitude/50f;
		speedFactor = Mathf.Clamp (speedFactor, 0, 1f);
		//
		fanLift = fanThrust*forceFactor*speedFactor*0.6f*liftFactor;
		if (connectedAircraft != null) {
			Vector3 liftForce = Thruster.up * fanLift;
			connectedAircraft.AddForce (liftForce, ForceMode.Force);
		}
	}
}








#if UNITY_EDITOR
[CustomEditor(typeof(SilantroLiftFan))]
public class LiftfanEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroLiftFan fan;
	SerializedObject engineObject;
	//
	private void OnEnable()
	{
		fan = (SilantroLiftFan)target;
		engineObject = new SerializedObject (fan);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck ();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Power Configuration", MessageType.None);
		GUI.color = backgroundColor;
		//
		fan.attachedEngine = EditorGUILayout.ObjectField ("Connected Engine", fan.attachedEngine, typeof(SilantroTurboFan), true) as SilantroTurboFan;
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Power Extraction Settings", MessageType.None);
		GUI.color = backgroundColor;
		fan.extractionRatio = EditorGUILayout.Slider ("Extraction",fan.extractionRatio,0,50);
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Shaft Power", fan.fanShaftPower.ToString ("0.00 Hp"));
		GUILayout.Space(3f);fan.fanDiameter = EditorGUILayout.FloatField ("Fan Diameter", fan.fanDiameter);
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Connections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		fan.connectedAircraft = EditorGUILayout.ObjectField ("Connected Aircraft", fan.connectedAircraft, typeof(Rigidbody), true) as Rigidbody;
		GUILayout.Space(2f);
		fan.fan = EditorGUILayout.ObjectField ("Intake Fan", fan.fan, typeof(Transform), true) as Transform;
		GUILayout.Space(3f);
		fan.rotationAxis = (SilantroLiftFan.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis",fan.rotationAxis);
		GUILayout.Space(3f);
		fan.rotationDirection = (SilantroLiftFan.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",fan.rotationDirection);
		GUILayout.Space(5f);
		fan.Thruster = EditorGUILayout.ObjectField ("Thruster", fan.Thruster, typeof(Transform), true) as Transform;
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		fan.fanStartClip = EditorGUILayout.ObjectField ("Fan Start Sound", fan.fanStartClip, typeof(AudioClip), true) as AudioClip;
		fan.fanRunningClip = EditorGUILayout.ObjectField ("Fan Running Sound", fan.fanRunningClip, typeof(AudioClip), true) as AudioClip;
		fan.fanShutdownClip = EditorGUILayout.ObjectField ("Fan Shutdown Sound", fan.fanShutdownClip, typeof(AudioClip), true) as AudioClip;
		//
		fan.start = EditorGUILayout.Toggle ("Start", fan.start);
		//
		GUILayout.Space(25f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Engine Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		//
		EditorGUILayout.LabelField ("Fan State",fan.CurrentFanState.ToString());
		EditorGUILayout.LabelField ("Fan Power",(fan.fanPower*100f).ToString("0.00") + " %");
		//
		EditorGUILayout.LabelField ("Core Speed",fan.CurrentRPM.ToString("0.0")+ " RPM");
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Engine Output", MessageType.None);
		GUI.color = backgroundColor;
		EditorGUILayout.LabelField ("Fan Thrust",fan.fanThrust.ToString("0.0")+ " N");
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (engineObject.targetObject, "Liftfan Engine Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (fan);
			EditorSceneManager.MarkSceneDirty (fan.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif