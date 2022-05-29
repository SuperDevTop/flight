//
//Property of Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
//
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroWingActuator : MonoBehaviour {

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CONNECTED WINGS
	[HideInInspector]public Transform LeftWing;
	[HideInInspector]public Transform RightWing;
	//SPECIFICATIONS
	[HideInInspector]public float sweepAngle;
	[HideInInspector]public float currentSweepAngle;
	[HideInInspector]public bool open;
	[HideInInspector]bool activated;
	//
	public enum SweepMode
	{
		Preset,Free
	}
	[HideInInspector]public SweepMode sweepMode = SweepMode.Preset;
	//
	public enum CurrentState
	{
		Open,Closed
	}
	[HideInInspector]public CurrentState currentState = CurrentState.Closed;
	//
	public enum StartState
	{
		Open,Closed
	}
	[HideInInspector]public StartState startState = StartState.Closed;
	[HideInInspector]public bool started;
	//
	[HideInInspector]public bool close ;
	[HideInInspector]public float MoveRate = 0.01f;
	[HideInInspector]public float openTime = 5f;
	[HideInInspector]public float closeTime = 5f;
	//
	//
	[HideInInspector]public AudioClip openSound;
	[HideInInspector]public AudioClip closeSound;
	[HideInInspector]public bool playSound = true;
	[HideInInspector]public bool isControllable = true;
	public enum RotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	[HideInInspector]public Vector3 AxisRotation;
	[HideInInspector]public Quaternion InitialLeftWingRotation;
	[HideInInspector]public Quaternion InitialRightWingRotation;
	[HideInInspector]public Quaternion FinalLeftWingRotation;
	[HideInInspector]public Quaternion FinalRightWingRotation;
	[HideInInspector]public bool negativeLeftWingRotation;//
	//
	[HideInInspector]public bool negativeRightwingRotation;
	AudioSource wingSound;




	// --------------------------------------------------------CONTROL FUNCTIONS--------------------------------------------------------------------------------------------------
	//EXTEND WINGS
	public void SwingWings()
	{
		if (currentState == CurrentState.Closed && !activated) {
			open = true;
		}
	}
	//SWING WINGS
	public void ExtendWings()
	{
		if (currentState == CurrentState.Open && !activated) {
			close = true;
		}
	}
	//SET WING ANGLE
	public void SetWingAngle(float angle)
	{
		if (sweepMode == SweepMode.Free) {
			currentSweepAngle = angle;
		}
	}



	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (LeftWing != null && RightWing ) {
			allOk = true;
		} else {
			Debug.LogError("Prerequisites not met on actuator "+transform.name + "....wings not properly assigned");
			allOk = false;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeWingActuator () {


		_checkPrerequisites ();


		if (allOk) {
			if (rotationAxis == RotationAxis.X) {
				AxisRotation = new Vector3 (-1, 0, 0);
			} else if (rotationAxis == RotationAxis.Y) {
				AxisRotation = new Vector3 (0, -1, 0);
			} else if (rotationAxis == RotationAxis.Z) {
				AxisRotation = new Vector3 (0, 0, -1);
			}
			//
			AxisRotation.Normalize ();
			//
			GameObject soundPoint = new GameObject ();
			soundPoint.transform.parent = this.transform;
			soundPoint.transform.localPosition = new Vector3 (0, 0, 0);
			soundPoint.name = this.name + " Sound Point";
			//
			wingSound = soundPoint.AddComponent<AudioSource> ();
			//
			InitialLeftWingRotation = LeftWing.localRotation;
			InitialRightWingRotation = RightWing.localRotation;

		} else {gameObject.SetActive (false);}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update () {

		if(allOk){

		if (sweepMode == SweepMode.Preset) {
			if (open && currentState == CurrentState.Closed) {
				//
				StartCoroutine (Increase ());
				//ROTATE LEFT WING
				if (!activated) {
					StartCoroutine (Open ());
					activated = true;
					if (openSound != null && playSound) {
						wingSound.PlayOneShot (openSound);
					}
				}
			}
			if (close && currentState == CurrentState.Open) {
				//
				StartCoroutine (Decrease ());
				//ROTATE LEFT WING
				if (!activated) {
					StartCoroutine (Close ());
					activated = true;
					if (closeSound != null && playSound) {
						wingSound.PlayOneShot (closeSound);
					}
				}
			}
		}
		//
		if (currentSweepAngle > sweepAngle) {currentSweepAngle = sweepAngle;}
		//ROTATE RIGHT WING
		if (RightWing != null) {
			if (negativeRightwingRotation) {
				RightWing.transform.localRotation = InitialRightWingRotation;RightWing.transform.Rotate (AxisRotation, -currentSweepAngle);
			} else {
				RightWing.transform.localRotation = InitialRightWingRotation;RightWing.transform.Rotate (AxisRotation, currentSweepAngle);
			}
		}
		//
		//ROTATE Left WING
		if (LeftWing != null) {
			if (negativeLeftWingRotation) {
				LeftWing.transform.localRotation = InitialLeftWingRotation;LeftWing.transform.Rotate (AxisRotation, -currentSweepAngle);
			} else {
				LeftWing.transform.localRotation = InitialLeftWingRotation;LeftWing.transform.Rotate (AxisRotation, currentSweepAngle);
			}
		}
		
		}
	}











	// ----------------------------------------------------------CONTROL TIMERS------------------------------------------------------------------------------------------------
	IEnumerator Open()
	{
		yield return new WaitForSeconds (openTime);
		CloseSwitches ();
		currentState = CurrentState.Open;
		activated = false;

	}
	IEnumerator Close()
	{
		yield return new WaitForSeconds (closeTime);
		currentState = CurrentState.Closed;
		CloseSwitches ();
		activated = false;

	}

	public IEnumerator Decrease()
	{
		float timeTemp = 0.0f;
		while (timeTemp < closeTime) {
			//
			float rate = Mathf.Sin ((3.142f / 2f) * (timeTemp / closeTime));
			currentSweepAngle -= MoveRate * rate;
			if (currentSweepAngle < 0) {
				currentSweepAngle = 0;
			}
			timeTemp += Time.deltaTime;
			yield return null;
		}
	}
	//
	public IEnumerator Increase()
	{
		float timeTemp = 0.0f;
		while (timeTemp < openTime) {
			//
			float rate = Mathf.Sin((3.142f/2f)*(timeTemp/closeTime) + (3.142f/2f));
			currentSweepAngle += MoveRate * rate;
			if (currentSweepAngle > sweepAngle) {currentSweepAngle = sweepAngle;}
			timeTemp += Time.deltaTime;yield return null;
		}
	}



	// --------------------------------------------------------RESET--------------------------------------------------------------------------------------------------
	void CloseSwitches()
	{
		open =  false;
		close = false;
	}


}
//
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroWingActuator))]
public class WingActuatorEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = Color.cyan;
	[HideInInspector]public int toolbarTab;
	[HideInInspector]public string currentTab;
	//
	SilantroWingActuator actuator;
	SerializedObject actuatorObject;
	//
	private void OnEnable()
	{
		actuator = (SilantroWingActuator)target;
		actuatorObject = new SerializedObject (actuator);
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
		EditorGUILayout.HelpBox ("Connections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Left Wing Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		actuator.LeftWing = EditorGUILayout.ObjectField ("Left Wing", actuator.LeftWing, typeof(Transform), true) as Transform;
		GUILayout.Space(2f);
		actuator.negativeLeftWingRotation = EditorGUILayout.Toggle ("Negative Rotation", actuator.negativeLeftWingRotation);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Right Wing Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		actuator.RightWing = EditorGUILayout.ObjectField ("Right Wing", actuator.RightWing, typeof(Transform), true) as Transform;
		GUILayout.Space(2f);
		actuator.negativeRightwingRotation = EditorGUILayout.Toggle ("Negative Rotation", actuator.negativeRightwingRotation);
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sweep Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		actuator.sweepMode = (SilantroWingActuator.SweepMode)EditorGUILayout.EnumPopup("Sweep Mode",actuator.sweepMode);
		GUILayout.Space(3f);
		actuator.sweepAngle = EditorGUILayout.FloatField ("Maximum Sweep", actuator.sweepAngle);
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Rotation Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		actuator.rotationAxis = (SilantroWingActuator.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis",actuator.rotationAxis);
		//
		if (actuator.sweepMode == SilantroWingActuator.SweepMode.Free) {
			GUILayout.Space (5f);
			actuator.currentSweepAngle = EditorGUILayout.Slider ("Current Sweep Angle", actuator.currentSweepAngle, 0f, 90f);
		}
		else {
			GUILayout.Space (20f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Controls", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			//
			actuator.open = EditorGUILayout.Toggle("Engage",actuator.open);
			GUILayout.Space (2f);
			actuator.close = EditorGUILayout.Toggle ("DisEngage", actuator.close);
		}
		//
		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Hydraulics Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		actuator.openTime = EditorGUILayout.FloatField ("Engage Time", actuator.openTime);
		GUILayout.Space (2f);
		actuator.closeTime = EditorGUILayout.FloatField ("DisEngage Time", actuator.closeTime);
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Current State", actuator.currentState.ToString());
		//
		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		actuator.openSound = EditorGUILayout.ObjectField ("Engage Sound", actuator.openSound, typeof(AudioClip), true) as AudioClip;
		GUILayout.Space (2f);
		actuator.closeSound = EditorGUILayout.ObjectField ("DisEngage Sound", actuator.closeSound, typeof(AudioClip), true) as AudioClip;
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (actuatorObject.targetObject, "Wing Actuator Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (actuator);
			EditorSceneManager.MarkSceneDirty (actuator.gameObject.scene);
		}
		actuatorObject.ApplyModifiedProperties();
	}
}
#endif