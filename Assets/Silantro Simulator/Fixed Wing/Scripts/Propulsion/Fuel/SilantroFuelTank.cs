//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroFuelTank : MonoBehaviour {
	//Type/Position of the tank
	public enum TankType{Internal,External}
	[HideInInspector]public TankType tankType = TankType.Internal;
	//
	public enum TankPosition{Left,Right,Center}
	[HideInInspector]public TankPosition tankPosition = TankPosition.Center;
	//
	//Type of fuel loaded in the tank
	public enum FuelType{JetB,JetA1,JP6,JP8,AVGas100,AVGas100LL,AVGas82UL}
	[HideInInspector]public FuelType fuelType = FuelType.JetB;
	//
	//Unit to use for conversion of the fuel amount
	public enum FuelUnit{Kilogram,Pounds,Liters,Gallon}
	[HideInInspector]public FuelUnit fuelUnit = FuelUnit.Kilogram;
	//
	//
	[HideInInspector]public float Capacity;//Maximum amount of fuel the tank can carry
	[HideInInspector]public float CurrentAmount;//Current amount of fuel in the tank
	[HideInInspector]public float actualAmount;//Factor used for fuel conversion based on assigned unit
	[HideInInspector]public bool attached = true;//Is the tank attached to the aircraft
	//
	[HideInInspector]public SilantroFuelDistributor attachedDitributor;
	float fuelFactor;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start () {ConvertFuel ();CurrentAmount = actualAmount;}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DROP TANK OR DETACH FROM AIRCRAFT
	public void Detach()
	{
		CurrentAmount = 0;
		//Disconnect tank from the fuel distributor
		if (attachedDitributor != null) {
			if (this.GetComponent<SilantroFuelTank>().tankType == TankType.External && attachedDitributor.externalTanks.Contains (this.GetComponent<SilantroFuelTank> ())) {
				attachedDitributor.externalTanks.Remove (this.GetComponent<SilantroFuelTank> ());
			}
		}
		attached = false;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void ConvertFuel()
	{
		if (fuelUnit == FuelUnit.Gallon) {
			fuelFactor = 0.79f;
		}
		if (fuelUnit == FuelUnit.Kilogram) {
			fuelFactor = 1f;
		}
		if (fuelUnit == FuelUnit.Liters) {
			if (fuelType == FuelType.JetA1) {
				fuelFactor = 0.79f;
			}
			if (fuelType == FuelType.JetB) {
				fuelFactor = 0.781f;
			}
			if (fuelType == FuelType.JP6) {
				fuelFactor = 0.81f;
			}
			if (fuelType == FuelType.JP8) {
				fuelFactor = 0.804f;
			}
			if (fuelType == FuelType.AVGas100) {
				fuelFactor = 0.721f;
			}
			if (fuelType == FuelType.AVGas100LL) {
				fuelFactor = 0.769f;
			}
			if (fuelType == FuelType.AVGas82UL) {
				fuelFactor = 0.730f;
			}
		}
		if (fuelUnit == FuelUnit.Pounds) {
			fuelFactor = 0.454f;
		}
		//
		actualAmount = Capacity*fuelFactor;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//EDITOR UPDATE
	public void OnDrawGizmos()
	{
		ConvertFuel ();
		//DRAW IDENTIFIER
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(transform.position,0.1f);
		Gizmos.color = Color.green;
		Gizmos.DrawLine (this.transform.position, (this.transform.up * 2f + this.transform.position));
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (CurrentAmount < 0f)
		{
			CurrentAmount = 0f;
		}
	}
}








#if UNITY_EDITOR
[CustomEditor(typeof(SilantroFuelTank))]
public class FuelTankEditor: Editor
{
	Color backgroundColor;
	//
	SilantroFuelTank tank;
	SerializedObject tankObject;
	//
	private void OnEnable()
	{
		tank = (SilantroFuelTank)target;
		tankObject = new SerializedObject (tank);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		Color silantroColor = new Color(1.0f,0.40f,0f);
		Color fuelColor = Color.white;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		//
		tankObject.Update();
		//
		GUILayout.Space(4f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Tank Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		tank.tankType = (SilantroFuelTank.TankType)EditorGUILayout.EnumPopup ("Tank Type", tank.tankType);
		if (tank.tankType == SilantroFuelTank.TankType.Internal) {
			GUILayout.Space (3f);
			tank.tankPosition = (SilantroFuelTank.TankPosition)EditorGUILayout.EnumPopup ("Position", tank.tankPosition);
		}
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Fuel Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);

		if (tank.fuelType == SilantroFuelTank.FuelType.AVGas100){fuelColor = Color.green;}
		else if (tank.fuelType == SilantroFuelTank.FuelType.AVGas100LL){fuelColor = Color.cyan;}
		else if (tank.fuelType == SilantroFuelTank.FuelType.AVGas82UL) {fuelColor = Color.red;} 

		GUI.color = fuelColor;
		tank.fuelType = (SilantroFuelTank.FuelType)EditorGUILayout.EnumPopup ("Fuel Type", tank.fuelType);
		GUI.color = backgroundColor;

		GUILayout.Space(10f);
		tank.fuelUnit = (SilantroFuelTank.FuelUnit)EditorGUILayout.EnumPopup("Fuel Unit",tank.fuelUnit);
		GUILayout.Space(5f);
		tank.Capacity = EditorGUILayout.FloatField ("Capacity", tank.Capacity);
		GUILayout.Space(5f);
		EditorGUILayout.LabelField ("Actual Capacity", tank.actualAmount.ToString ("0.00") + " kg");
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Fuel Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Current Amount", tank.CurrentAmount.ToString ("0.00") + " kg");
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (tankObject.targetObject, "Tank Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (tank);
			EditorSceneManager.MarkSceneDirty (tank.gameObject.scene);
		}
		tankObject.ApplyModifiedProperties();
	}
}
#endif
