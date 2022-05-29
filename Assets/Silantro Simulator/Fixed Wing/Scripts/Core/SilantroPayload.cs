﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


public class SilantroPayload : MonoBehaviour {

	//-----GENERAL CLASS
	public enum PayloadType
	{
		Crew,Cargo,Equipment
		//ADD MORE
	}
	[HideInInspector]public PayloadType payloadType = PayloadType.Crew;

	//-----CREW CLASS
	public enum CrewType
	{
		Pilot,CoPilot,Passenger
	}
	[HideInInspector]public CrewType crewType = CrewType.Pilot;

	//----EQUIPMENT CLASS
	public enum EquipmentType
	{
		Tyre
		//ADD MORE
	}
	[HideInInspector]public EquipmentType equipmentType = EquipmentType.Tyre;

	//-----PROPERTIES
	[HideInInspector]public float weight;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnDrawGizmos()
	{
		//DRAW IDENTIFIER
		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(transform.position,0.1f);
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine (this.transform.position, (this.transform.up * 2f + this.transform.position));
	}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPayload))]
public class PayloadEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	//
	SilantroPayload payload;

	private void OnEnable()
	{
		payload = (SilantroPayload)target;
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		serializedObject.UpdateIfRequiredOrScript();
		//
		GUILayout.Space (2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Payload Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		payload.payloadType = (SilantroPayload.PayloadType)EditorGUILayout.EnumPopup("Type",payload.payloadType);
		GUILayout.Space (3f);
		if (payload.payloadType == SilantroPayload.PayloadType.Crew) {
			payload.crewType = (SilantroPayload.CrewType)EditorGUILayout.EnumPopup("Crew Designation",payload.crewType);
			GUILayout.Space (3f);
			payload.weight = EditorGUILayout.FloatField ("Crew Weight", payload.weight);
		}
		//2. EQUIPMENTS
		if (payload.payloadType == SilantroPayload.PayloadType.Equipment) {
			payload.equipmentType = (SilantroPayload.EquipmentType)EditorGUILayout.EnumPopup("Functionality",payload.equipmentType);
			GUILayout.Space (3f);
			payload.weight = EditorGUILayout.FloatField ("Weight", payload.weight);
		}
		//3. CAROG
		if (payload.payloadType == SilantroPayload.PayloadType.Cargo) {
			payload.weight = EditorGUILayout.FloatField ("Useful Weight", payload.weight);
		}
	}
}
#endif