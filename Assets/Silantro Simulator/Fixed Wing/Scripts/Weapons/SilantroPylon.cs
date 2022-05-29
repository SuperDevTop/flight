//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroPylon : MonoBehaviour {
	//POSITION
	public enum PylonPosition
	{
		External,Internal
	}
	[HideInInspector]public PylonPosition pylonPosition;
	[HideInInspector]public SilantroArmament manager;
	public enum LauncherType
	{
		Trapeze,Drop,Tube
	}
	[HideInInspector]public LauncherType laucnherType = LauncherType.Drop;
	//TRAPEZE POSITION
	public enum TrapezePosition
	{
		Left,Right
	}
	[HideInInspector]public TrapezePosition trapezePosition = TrapezePosition.Right;
	//WEAPON TYPE
	public enum OrdnanceType
	{
		Bomb,Missile
	}
	[HideInInspector]public OrdnanceType munitionType = OrdnanceType.Bomb;
	[HideInInspector]public SilantroHydraulicSystem pylonBay;
	//WEAPONS
	[HideInInspector]public SilantroMunition missile;
	[HideInInspector]public List<SilantroMunition> bombs;
	public enum DropMode
	{
		Single,Salvo
	}
	[HideInInspector]public DropMode bombMode = DropMode.Single;
	[HideInInspector]public float dropInterval = 1f;
	//TARGET
	[HideInInspector]public Transform target;
	[HideInInspector]public string targetID;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//COUNT ATTACHED MUNITIONS
	public void InitializePylon()
	{
		//CLOSE DOOR
		if (pylonBay != null && pylonPosition == PylonPosition.Internal) {
			if (pylonBay.currentState == SilantroHydraulicSystem.CurrentState.Open) {
				pylonBay.close = true;
			}
		}
		//IDENTIFY ATTACHED MUNITION
		SilantroMunition[] munitions = GetComponentsInChildren<SilantroMunition> ();
		bombs = new List<SilantroMunition> ();
		foreach (SilantroMunition munition in munitions) {
			//MISSILE
			if (munitionType == OrdnanceType.Missile) {
				if (munition.munitionType == SilantroMunition.MunitionType.Missile) {
					missile = munition;
					missile.connectedPylon = this.gameObject.GetComponent<SilantroPylon> ();
				}
			}
			//BOMB
			if (munitionType == OrdnanceType.Bomb) {
				if (munition.munitionType == SilantroMunition.MunitionType.Bomb) {
					bombs.Add (munition);
					munition.connectedPylon = this.gameObject.GetComponent<SilantroPylon> ();
				}
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RECOUNT BOMBS
	void CountBombs()
	{
		SilantroMunition[] munitions = GetComponentsInChildren<SilantroMunition> ();
		bombs = new List<SilantroMunition> ();
		foreach (SilantroMunition munition in munitions) {
			//BOMB
			if (munitionType == OrdnanceType.Bomb) {
				if (munition.munitionType == SilantroMunition.MunitionType.Bomb) {
					bombs.Add (munition);
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//START SEQUENCE LAUNCH
	public void StartLaunchSequence()
	{
		engaged = true;
		//DETERMINE LAUNCH SEQUENCE
		if (pylonPosition == PylonPosition.External) {
			LaunchMissile ();
		}
		if (pylonPosition == PylonPosition.Internal) {
			//OPEN DOOR
			if (pylonBay != null) {
				StartCoroutine (OpenBayDoor ());
			}
			//LAUNCH IF DOOR IS UNAVAILABLE
			else {
				LaunchMissile ();
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//START SEQUENCE DROP
	public void StartDropSequence()
	{
		if (pylonPosition == PylonPosition.External) {
			BombRelease ();
		}
		if (pylonPosition == PylonPosition.Internal) {
			//OPEN DOOR
			engaged = true;
			if (pylonBay != null) {
				bombMode = DropMode.Salvo;
				StartCoroutine (OpenBayDoor ());
			}
			//LAUNCH IF DOOR IS UNAVAILABLE
			else {
				BombRelease ();
			}
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public bool engaged;
	//OPEN DOOR
	IEnumerator OpenBayDoor()
	{
		pylonBay.open = true;
		yield return new WaitForSeconds (pylonBay.openTime);
		//RELEASE MUNITION
		if (munitionType == OrdnanceType.Missile) {LaunchMissile ();}
		if (munitionType == OrdnanceType.Bomb) {BombRelease ();}
	}
	//CLOSE DOOR
	IEnumerator CloseDoor()
	{
		yield return new WaitForSeconds (0.5f);
		pylonBay.close = true;
		//REMOVE PYLON
		Destroy (this.gameObject);
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL MISSILE LAUNCH
	void LaunchMissile()
	{
		//1. TUBE LAUNCH
		if (laucnherType == LauncherType.Tube) {
			missile.FireMunition (target, targetID,2);
		}
		//2. DROP LAUNCH
		if (laucnherType == LauncherType.Drop) {
			missile.FireMunition (target, targetID,1);
		}

		//3. TRAPEZE LAUNCH RIGHT
		if (laucnherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Right) {
			missile.FireMunition (target, targetID,3);
		}

		//4. TRAPEZE LAUNCH LEFT
		if (laucnherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Left) {
			missile.FireMunition (target, targetID,4);
		}

		//CLOSE BAY DOOR
		if (pylonPosition == PylonPosition.Internal && pylonBay != null) {
			StartCoroutine (CloseDoor ());
		}
	}
		

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL BOMB DROP
	void BombRelease()
	{
		//1. SINGLE BOMB DROP
		//SELECT RANDOM BOMB
		if (bombs.Count > 0) {
			if (bombs [0] != null) {
				bombs [0].ReleaseMunition ();
			}
			CountBombs ();
			manager.CountOrdnance ();
			//2. SALVO DROP
			if (bombMode == DropMode.Salvo) {
				StartCoroutine (WaitForNextDrop ());
			}
		} else {
			if (pylonPosition == PylonPosition.Internal && pylonBay != null && pylonBay.currentState == SilantroHydraulicSystem.CurrentState.Open) {
				StartCoroutine (CloseDoor ());
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SALVO TIMER
	IEnumerator WaitForNextDrop()
	{
		yield return new WaitForSeconds (dropInterval);
		BombRelease ();
	}
}





#if UNITY_EDITOR
[CustomEditor(typeof(SilantroPylon))]
public class PylonEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroPylon pylon;
	SerializedObject pylonObject;
	//
	private void OnEnable()
	{
		pylon = (SilantroPylon)target;
		pylonObject = new SerializedObject (pylon);
	}
	//
	public override void OnInspectorGUI ()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		serializedObject.Update ();
		//
		GUILayout.Space (1f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Pylon Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		pylon.pylonPosition = (SilantroPylon.PylonPosition)EditorGUILayout.EnumPopup("Position",pylon.pylonPosition);
		GUILayout.Space (3f);
		pylon.munitionType = (SilantroPylon.OrdnanceType)EditorGUILayout.EnumPopup("Ordnance",pylon.munitionType);
		GUILayout.Space (10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Launch Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		if (pylon.pylonPosition == SilantroPylon.PylonPosition.Internal) {
			GUILayout.Space (3f);
			pylon.pylonBay = EditorGUILayout.ObjectField ("Bay Actuator", pylon.pylonBay, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
		}
		if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb) {
			GUILayout.Space (3f);
			pylon.bombMode = (SilantroPylon.DropMode)EditorGUILayout.EnumPopup("Drop Mode",pylon.bombMode);
			if (pylon.bombMode == SilantroPylon.DropMode.Salvo) {
				GUILayout.Space (3f);
				pylon.dropInterval = EditorGUILayout.FloatField ("Drop Interval", pylon.dropInterval);
			}
		}
		if (pylon.munitionType == SilantroPylon.OrdnanceType.Missile) {
			GUILayout.Space (3f);
			pylon.laucnherType = (SilantroPylon.LauncherType)EditorGUILayout.EnumPopup("Launch Mode",pylon.laucnherType);
			if (pylon.laucnherType == SilantroPylon.LauncherType.Trapeze) {
				GUILayout.Space (3f);
				pylon.trapezePosition = (SilantroPylon.TrapezePosition)EditorGUILayout.EnumPopup("Launch Position",pylon.trapezePosition);
			}
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (pylonObject.targetObject, "Pylon Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (pylon);
			EditorSceneManager.MarkSceneDirty (pylon.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif