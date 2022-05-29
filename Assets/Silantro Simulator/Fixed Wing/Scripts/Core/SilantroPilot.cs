//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroPilot : MonoBehaviour {
	//SPECS
	[HideInInspector]public float maxRayDistance= 2f;
	[HideInInspector]public Transform head;
	//PLAYER TYPE
	public enum ControlType
	{
		ThirdPerson,
		FirstPerson
	}
	[HideInInspector]public ControlType controlType = ControlType.ThirdPerson;
	[HideInInspector]public bool isClose = false;//Is the Player Close to an aircraft
	[HideInInspector]public bool canEnter = false;
	[HideInInspector]SilantroController controller;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENTER 
	public void SendEntryData()
	{
		if (isClose && canEnter) {
			//PLAYER INFO
			if (controller != null) {
				controller.player = this.gameObject;
				if (controlType == ControlType.FirstPerson) {
					controller.playerType = SilantroController.PlayerType.FirstPerson;
				}
				if (controlType == ControlType.ThirdPerson) {
					controller.playerType = SilantroController.PlayerType.ThirdPerson;
				}
				//SEND ACCEPT
				controller.EnterAircraft();
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		if (controlType == ControlType.FirstPerson) {
			GameObject mainCameraObject = Camera.main.gameObject;
			if (mainCameraObject != null) {
				mainCameraObject.SetActive (true);
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DISPLAY ENTERY INFORMATION
	void OnGUI()
	{
		if (isClose && canEnter) {
			GUI.Label (new Rect (Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 100), "Press F to Enter");
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAW EYE SIGHT
	void OnDrawGizmos()
	{
		if (head != null) {
			Gizmos.color = Color.red;
			Gizmos.DrawRay (head.position, transform.forward * maxRayDistance);
		}
	}








	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CHECK AIRCRAFT DISTANCE
	void Update()
	{
		//SEND CHECK DATA
		CheckAircraftState();
		//ENTER
		if (Input.GetKeyDown (KeyCode.F)) {SendEntryData ();}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CheckAircraftState()
	{
		Vector3 direction= transform.TransformDirection(Vector3.forward);
		RaycastHit aircraft;
		//
		if (Physics.Raycast (head.position, direction, out aircraft, maxRayDistance)) {
			//COLLECT AIRCRAFT CONTROLLER
			controller = aircraft.transform.gameObject.GetComponent<SilantroController> ();
			//PROCESS IF CONTROLLER IS AVAILABLE
			if (controller != null) {
				//DISPLAY ENTRY PROCESS?TERMS?CONDITIONS?FACTORS>>>>
				if (!controller.pilotOnboard) {
					isClose = true;
				}
				//CAN ENTER
				canEnter = true;
			} else {
				isClose = false;canEnter = false;
			}
		} 
		//CAN'T SEE ANY AIRCRAFT
		else {
			isClose = false;canEnter = false;
		}
	}
}











#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPilot))]
public class PilotEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		serializedObject.Update ();
		//
		SilantroPilot pilot = (SilantroPilot)target;
		//
		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Player Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		pilot.controlType = (SilantroPilot.ControlType)EditorGUILayout.EnumPopup(" ",pilot.controlType);
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		pilot.head = EditorGUILayout.ObjectField ("Head", pilot.head, typeof(Transform), true) as Transform;
		GUILayout.Space (3f);
		pilot.maxRayDistance = EditorGUILayout.FloatField ("Sight Distance", pilot.maxRayDistance);
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (serializedObject.targetObject, "Pilot Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (pilot);
			EditorSceneManager.MarkSceneDirty (pilot.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif