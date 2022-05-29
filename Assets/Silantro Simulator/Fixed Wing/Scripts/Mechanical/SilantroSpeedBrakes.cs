using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroSpeedBrakes : MonoBehaviour {
	[HideInInspector]public SilantroHydraulicSystem actuator;
	public enum SystemType
	{
		Internal,
		Animation
	}
	[HideInInspector]public SystemType systemType = SystemType.Internal;
	[HideInInspector]public Animator brakeAnimator;
	[HideInInspector]public float waitTime = 5f;
	[HideInInspector]public bool open;bool inTransition;
	[HideInInspector]public bool brakeOpened = false;
	//
	[HideInInspector]public bool isControllable = true;
	[HideInInspector]public bool close ;
	[HideInInspector]public bool brakeClosed = true;














	// ---------------------------------------------------CONTROLS-------------------------------------------------------------------------------------------------------
	//ACTIVATE BRAKE
	public void EngageBrake()
	{
		if (!brakeOpened && !inTransition){
			//INTERNAL MOVEMENT SYSTEM
			if (systemType == SystemType.Internal) {
				open = true;
			}
			//ANIMATION SYSTEM
			if (systemType == SystemType.Animation) {
				//open = true;
				brakeAnimator.SetTrigger ("Open");
				inTransition = true;
				StartCoroutine (WaitToOpenAnimation ());
			}
		}
	}
	//DEACTIVATE BRAKE
	public void DisengageBrake()
	{
		if (!brakeClosed && !inTransition){
			//INTERNAL MOVEMENT SYSTEM
			if (systemType == SystemType.Internal) {
				close = true;
			}
			//ANIMATION SYSTEM
			if (systemType == SystemType.Animation) {
				brakeAnimator.SetTrigger ("Close");
				inTransition = true;
				StartCoroutine (WaitToCloseAnimation ());
			}
		}
	}
	//TOGGLE BRAKE
	public void ToggleSpeedBrake()
	{
		if (!brakeOpened && !inTransition){
			//INTERNAL MOVEMENT SYSTEM
			if (systemType == SystemType.Internal) {
				open = true;
			}
			//ANIMATION SYSTEM
			if (systemType == SystemType.Animation) {
				//open = true;
				brakeAnimator.SetTrigger ("Open");
				inTransition = true;
				StartCoroutine (WaitToOpenAnimation ());
			}
		}
		if (!brakeClosed && !inTransition){
			//INTERNAL MOVEMENT SYSTEM
			if (systemType == SystemType.Internal) {
				close = true;
			}
			//ANIMATION SYSTEM
			if (systemType == SystemType.Animation) {
				brakeAnimator.SetTrigger ("Close");
				inTransition = true;
				StartCoroutine (WaitToCloseAnimation ());
			}
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CloseBrakeSwitches()
	{
		open = false;close = false;
	}
	public void InitializeSpeedBrakes () {
		brakeOpened = false;brakeClosed = true;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update () {
		//
		if (open && !brakeOpened) {
			if (actuator != null) {
				actuator.open = true;
				StartCoroutine (WaitToOpen ());
			} 
		}
		if (close && !brakeClosed) {

			if (actuator != null) {
				actuator.close = true;
				StartCoroutine (WaitToClose ());
			} 
		}
	}









	// ---------------------------------------------------CONTROLS TIMERS-------------------------------------------------------------------------------------------------------
	IEnumerator WaitToClose()
	{
		yield return new WaitForSeconds (actuator.closeTime);
		brakeClosed = true;
		brakeOpened = false;CloseBrakeSwitches ();
		inTransition = false;
	}
	IEnumerator WaitToOpen()
	{
		yield return new WaitForSeconds (actuator.openTime);
		brakeClosed = false;
		brakeOpened = true;CloseBrakeSwitches ();
		inTransition = false;
	}
	//
	//ANIMATION SYSTEM
	//
	IEnumerator WaitToCloseAnimation()
	{
		yield return new WaitForSeconds (waitTime);
		brakeClosed = true;
		brakeOpened = false;CloseBrakeSwitches ();
		inTransition = false;
	}
	IEnumerator WaitToOpenAnimation()
	{
		yield return new WaitForSeconds (waitTime);
		brakeClosed = false;
		brakeOpened = true;CloseBrakeSwitches ();
		inTransition = false;
	}
}





#if UNITY_EDITOR
[CustomEditor(typeof(SilantroSpeedBrakes))]
public class BrakesEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroSpeedBrakes speedbrakes;
	SerializedObject speedbrakesObject;
	//
	private void OnEnable()
	{
		speedbrakes = (SilantroSpeedBrakes)target;
		speedbrakesObject = new SerializedObject (speedbrakes);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		//
		serializedObject.Update ();
		GUILayout.Space (3f);
		speedbrakes.systemType = (SilantroSpeedBrakes.SystemType)EditorGUILayout.EnumPopup ("System Type", speedbrakes.systemType);
		GUILayout.Space (5f);
		//
		if (speedbrakes.systemType == SilantroSpeedBrakes.SystemType.Internal) {
			//
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Internal Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Actuator Settings", MessageType.None);
			speedbrakes.actuator = EditorGUILayout.ObjectField ("Brakes Actuator", speedbrakes.actuator, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
		}
		if (speedbrakes.systemType == SilantroSpeedBrakes.SystemType.Animation) {
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Animation Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			speedbrakes.brakeAnimator = EditorGUILayout.ObjectField ("Gear Animator", speedbrakes.brakeAnimator, typeof(Animator), true) as Animator;
			GUILayout.Space (3f);
			speedbrakes.waitTime = EditorGUILayout.FloatField ("Actuation Time", speedbrakes.waitTime);
		}
		//
		//
		GUILayout.Space (10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Controls", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (2f);
		GUILayout.Space (2f);
		speedbrakes.open = EditorGUILayout.Toggle ("Engage", speedbrakes.open);
		GUILayout.Space (2f);
		speedbrakes.close = EditorGUILayout.Toggle ("Disengage", speedbrakes.close);
		//

		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("System Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		if (speedbrakes.brakeOpened) {
			EditorGUILayout.LabelField ("Current State", "Open");
		}
		if (speedbrakes.brakeClosed) {
			EditorGUILayout.LabelField ("Current State", "Closed");
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (speedbrakesObject.targetObject, "Speed Brake Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (speedbrakes);
			EditorSceneManager.MarkSceneDirty (speedbrakes.gameObject.scene);
		}
		//
		serializedObject.ApplyModifiedProperties();
	}
}
#endif