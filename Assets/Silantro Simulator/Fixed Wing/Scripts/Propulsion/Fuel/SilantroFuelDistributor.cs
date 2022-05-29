//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroFuelDistributor : MonoBehaviour {
	//
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public SilantroFuelTank[] fuelTanks;
	[HideInInspector]public List<SilantroFuelTank> internalFuelTanks;
	[HideInInspector]public List<SilantroFuelTank> externalTanks = new List<SilantroFuelTank> ();
	[HideInInspector]public float TotalFuelRemaining;//Total amount of fuel remaining in the aircraft (Combined total from all the attached fuel tanks)
	[HideInInspector]public float TotalFuelCapacity;
	//
	[HideInInspector]public string currentTank;//Name of selected tank (i.e tank where fuel is currently being drawn from)
	[HideInInspector]public string tankType;
	[HideInInspector]public float currentTankFuel;
	[HideInInspector]public string timeLeft;
	[HideInInspector]public float totalConsumptionRate =1f;
	//
	[HideInInspector]public bool dumpFuel = false;
	[HideInInspector]public float fuelDumpRate = 1f;//Rate at which fuel will be dumped in kg/s
	[HideInInspector]public float actualFlowRate;
	//
	[HideInInspector]public bool refillTank = false;
	[HideInInspector]public float refuelRate = 1f;//Rate at which tank will be refilled in kg/s
	[HideInInspector]public float actualrefuelRate;//

	[HideInInspector]public bool lowFuel;
	bool fuelAlertActivated;
	[HideInInspector]public float minimumFuelAmount = 50f;
	[HideInInspector]public AudioClip fuelAlert;
	AudioSource FuelAlert;
	//
	[HideInInspector]public bool isControllable = true;//Main control variable

	//TANKS
	[HideInInspector]public List<SilantroFuelTank> RightTanks;
	[HideInInspector]public List<SilantroFuelTank> LeftTanks;
	[HideInInspector]public List<SilantroFuelTank> CentralTanks;
	//FUEL AMOUNTS
	[HideInInspector]public float RightFuelAmount;
	[HideInInspector]public float LeftFuelAmount;
	[HideInInspector]public float CenterFuelAmount;
	[HideInInspector]public float ExternalFuelAmount;
	[HideInInspector]public float engineConsumption;

	//SELECTOR
	public enum FuelSelector
	{
		Left,Right,External,Automatic
	}
	[HideInInspector]public FuelSelector fuelSelector = FuelSelector.Automatic;
	[HideInInspector]public string fuelType;
	public enum FuelFactor
	{
		DeltaTime,FixedDeltaTime
	}
	[HideInInspector]public FuelFactor usageFactor = FuelFactor.DeltaTime;
	[HideInInspector]public float timeFactor = 1;


	// ---------------------------------------------------------------------CONTROLS-------------------------------------------------------------------------------------
	//START/STOP FUEL DUMP
	public void ActivateFuelDump()
	{
		if (!refillTank) {dumpFuel = !dumpFuel;}
	}
	//START/STOP TANK REFILL
	public void ActivateTankRefill()
	{
		if (!dumpFuel) {refillTank = !refillTank;}
	}
	//STOP ALERT SOUND
	public void StopAlertSound()
	{
		if (fuelAlertActivated) {FuelAlert.Stop ();lowFuel = false;}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeDistributor () {

		if (fuelTanks.Length > 0) {
			//SEPARATE TANKS
			//1. TYPE
			TotalFuelCapacity = 0f;
			externalTanks=new List<SilantroFuelTank>();
				LeftTanks=new List<SilantroFuelTank>();
					RightTanks=new List<SilantroFuelTank>();
						CentralTanks = new List<SilantroFuelTank>();
			fuelType = fuelTanks [0].fuelType.ToString ();
			foreach (SilantroFuelTank tank in fuelTanks) {
				TotalFuelCapacity += tank.Capacity;
				//INTERNAL
				if (tank.tankType == SilantroFuelTank.TankType.Internal) {
					internalFuelTanks.Add (tank);
				}
				//EXTERNAL
				if (tank.tankType == SilantroFuelTank.TankType.External) {
					externalTanks.Add (tank);
				}
				//CHECK FUEL TYPE
				if (tank.fuelType.ToString () != fuelType) {
					Debug.Log ("Fuel Type Selection not uniform");
				}
			}
			

			//2. POSITION
			foreach (SilantroFuelTank internalTank in internalFuelTanks) {
				//CENTER
				if (internalTank.tankPosition == SilantroFuelTank.TankPosition.Center) {
					CentralTanks.Add (internalTank);
				}
				//LEFT
				if (internalTank.tankPosition == SilantroFuelTank.TankPosition.Left) {
					LeftTanks.Add (internalTank);
				}
				//RIGHT
				if (internalTank.tankPosition == SilantroFuelTank.TankPosition.Right) {
					RightTanks.Add (internalTank);
				}
			}
			//
			//FUEL SETUP
			if (internalFuelTanks != null && internalFuelTanks.Count > 0) {
				TotalFuelRemaining = 0;
				//1. INTERNAL FUEL
				foreach (SilantroFuelTank internalTank in internalFuelTanks) {
					TotalFuelRemaining += internalTank.Capacity;
				}
				//2. EXTERNAL FUEL
				if (externalTanks.Count > 0) {
					foreach (SilantroFuelTank external in externalTanks) {
						TotalFuelRemaining += external.Capacity;
					}
				}
			} 


			//ALERT SOUND SETUP
			GameObject soundPoint = new GameObject ("Warning Horn");
			soundPoint.transform.parent = this.transform;
			soundPoint.transform.localPosition = new Vector3 (0, 0, 0);
			//
			if (null != fuelAlert) {
				FuelAlert = soundPoint.gameObject.AddComponent<AudioSource> ();
				FuelAlert.clip = fuelAlert;
				FuelAlert.loop = true;
				FuelAlert.volume = 1f;
				FuelAlert.dopplerLevel = 0f;
				FuelAlert.spatialBlend = 1f;
				FuelAlert.rolloffMode = AudioRolloffMode.Custom;
				FuelAlert.maxDistance = 650f;
			}
		}
		else {
			Debug.LogError ("No fuel tank is assigned to distributor!!");
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate () {
		if (isControllable && controller != null) {

			TotalFuelRemaining = 0;
			//COUNT FUEL
			if (fuelTanks.Length > 0) {
				foreach (SilantroFuelTank tank in fuelTanks) {
					TotalFuelRemaining += tank.CurrentAmount;
				}
			}
			//CLAMP
			foreach (SilantroFuelTank tank in fuelTanks) {if (tank.CurrentAmount < 0) {tank.CurrentAmount = 0f;}}

			//LOW FUEL WARNING
			if (TotalFuelRemaining <= minimumFuelAmount) {
				lowFuel = true;
				if (!fuelAlertActivated) {
					//
					fuelAlertActivated = true;
					StartCoroutine (LowFuelAction ());
				}
			}
			//DEACTIVATE ALERT SOUND
			if (TotalFuelRemaining > minimumFuelAmount && fuelAlertActivated) {
				fuelAlertActivated = false;
				FuelAlert.Stop ();
			}


			//DISPLAY FUEL EXHAUSTION TIME
			if (totalConsumptionRate > 0) {
				float flightTime = (TotalFuelRemaining) / totalConsumptionRate;
				timeLeft = string.Format ("{0}:{1:00}", (int)flightTime / 60, (int)flightTime % 60);
			}
		
			//Dump Fuel
			if (dumpFuel) {
				DumpFuel ();
			}
			//Refuel Tank
			if (refillTank) {
				RefuelTank ();
			}

			if (usageFactor == FuelFactor.DeltaTime) {timeFactor = Time.deltaTime;} 
			else {timeFactor = Time.fixedDeltaTime;}

			//SEND USAGE DATA
			DepleteTanks();
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	////REFUEL TANKS
	void DepleteTanks()
	{
		//timeFactor = 1f;
		engineConsumption = controller.totalConsumptionRate;
		fuelFlow = engineConsumption * timeFactor;
		//ACCOUNT FOR FUEL TANKS
		LeftFuelAmount = RightFuelAmount = CenterFuelAmount = ExternalFuelAmount = 0f;
		//CALCULATE
		foreach(SilantroFuelTank tank in LeftTanks){LeftFuelAmount += tank.CurrentAmount;}
		foreach(SilantroFuelTank tank in RightTanks){RightFuelAmount += tank.CurrentAmount;}
		foreach(SilantroFuelTank tank in CentralTanks){CenterFuelAmount += tank.CurrentAmount;}
		foreach(SilantroFuelTank tank in externalTanks){ExternalFuelAmount += tank.CurrentAmount;}


		//1. USE EXTERNAL FUEL TANKS
		if (fuelSelector == FuelSelector.External) {
			if (externalTanks != null && externalTanks.Count > 0) {
				//DEPLETE
				float individualRate = engineConsumption / externalTanks.Count;
				foreach (SilantroFuelTank tank in externalTanks) {tank.CurrentAmount -= individualRate* timeFactor;}
				//SWITCH IF TANK IS EXHAUSTED
				if(ExternalFuelAmount <= 0){fuelSelector = FuelSelector.Automatic;}
			} else {
				fuelSelector = FuelSelector.Automatic;
			}
		}

		//2. USE LEFT TANKS
		if (fuelSelector == FuelSelector.Left) {
			if (LeftTanks != null && LeftTanks.Count > 0) {
				//DEPLETE
				float individualRate = engineConsumption / LeftTanks.Count;
				foreach (SilantroFuelTank tank in LeftTanks) {tank.CurrentAmount -= individualRate* timeFactor;}
				//SWITCH IF TANK IS EXHAUSTED
				if(LeftFuelAmount <= 0){fuelSelector = FuelSelector.Automatic;}
			}
			else {
				fuelSelector = FuelSelector.Automatic;
			}
		}

		//3. USE RIGHT TANKS
		if (fuelSelector == FuelSelector.Right) {
			if (RightTanks != null && RightTanks.Count > 0) {
				//DEPLETE
				float individualRate = engineConsumption / RightTanks.Count;
				foreach (SilantroFuelTank tank in RightTanks) {tank.CurrentAmount -= individualRate* timeFactor;}
				//SWITCH IF TANK IS EXHAUSTED
				if(RightFuelAmount <= 0){fuelSelector = FuelSelector.Automatic;}
			}
			else {
				fuelSelector = FuelSelector.Automatic;
			}
		}


		//4 AUTOMATIC
		if (fuelSelector == FuelSelector.Automatic) {
			//A> USE CENTRAL TANKS FIRST
			if (CentralTanks != null && CentralTanks.Count > 0 && CenterFuelAmount > 0) {
				//DEPLETE
				float individualRate = engineConsumption / CentralTanks.Count;
				foreach (SilantroFuelTank tank in CentralTanks) {
					tank.CurrentAmount -= individualRate * timeFactor;
				}
			} else {
				//B> USE EXTERNAL TANKS
				if (externalTanks != null && externalTanks.Count > 0 && ExternalFuelAmount > 0) {
					//DEPLETE
					float individualRate = engineConsumption / externalTanks.Count;
					foreach (SilantroFuelTank tank in externalTanks) {
						tank.CurrentAmount -= individualRate * timeFactor;
					}
				}
				//C> USE OTHER TANKS
				else {
					int usefulTanks = LeftTanks.Count + RightTanks.Count;
					float individualRate = engineConsumption / usefulTanks;
					//LEFT
					foreach (SilantroFuelTank tank in LeftTanks) {
						tank.CurrentAmount -= individualRate *timeFactor;
					}
					//RIGHT
					foreach (SilantroFuelTank tank in RightTanks) {
						tank.CurrentAmount -= individualRate * timeFactor;
					}
				}
			}
		}
	}
	[HideInInspector]public float fuelFlow;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	////REFUEL TANKS
	public void RefuelTank()
	{
		actualrefuelRate = refuelRate * Time.deltaTime;
		if (internalFuelTanks != null && internalFuelTanks.Count> 0) {
			float indivialRate = actualrefuelRate / internalFuelTanks.Count;
			foreach (SilantroFuelTank tank in internalFuelTanks) {
				tank.CurrentAmount += indivialRate;
			}
		}
		//CONTROL AMOUNT
		foreach (SilantroFuelTank tank in internalFuelTanks) {
			if (tank.CurrentAmount > tank.Capacity) {
				tank.CurrentAmount = tank.Capacity;
			}
		}
		if (TotalFuelRemaining >= TotalFuelCapacity) {
			refillTank = false;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	////REFUEL TANKS
	void DumpFuel()
	{
		actualFlowRate = fuelDumpRate * Time.deltaTime;
		if (internalFuelTanks != null && internalFuelTanks.Count> 0) {
			float indivialRate = actualFlowRate / internalFuelTanks.Count;
			foreach (SilantroFuelTank tank in internalFuelTanks) {
				tank.CurrentAmount -= indivialRate;
			}
		}
		//CONTROL AMOUNT
		foreach (SilantroFuelTank tank in fuelTanks) {
			if (tank.CurrentAmount <= 0) {
				tank.CurrentAmount = 0;
			}
		}
		if (TotalFuelRemaining <= 0) {
			dumpFuel = false;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTIVATE FUEL ALERT SOUND
	IEnumerator LowFuelAction()
	{
		if (FuelAlert != null) {
			yield return new WaitForSeconds (0.5f);
			FuelAlert.Play ();
		}
	}
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroFuelDistributor))]
public class DistributorEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	//
	SilantroFuelDistributor system;
	SerializedObject systemObject;
	//
	private void OnEnable()
	{
		system = (SilantroFuelDistributor)target;
		systemObject = new SerializedObject (system);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		systemObject.UpdateIfRequiredOrScript();
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Tank Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		//
		EditorGUILayout.LabelField ("Total Fuel: ", system.TotalFuelRemaining.ToString ("0.0") + " kg");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Engine Consumption: ", system.engineConsumption.ToString ("0.000") + " kg/s");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Time Left: ", system.timeLeft+ " min");


		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Fuel Distribution", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Right Fuel: ", system.RightFuelAmount.ToString ("0.0") + " kg");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Left Fuel: ", system.LeftFuelAmount.ToString ("0.0") + " kg");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Central Fuel: ", system.CenterFuelAmount.ToString ("0.0") + " kg");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("External Fuel: ", system.ExternalFuelAmount.ToString ("0.0") + " kg");
		GUILayout.Space(3f);
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Fuel Usage", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		system.fuelSelector = (SilantroFuelDistributor.FuelSelector)EditorGUILayout.EnumPopup("Tank Selector",system.fuelSelector);
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Fuel Type: ", system.fuelType);
		GUILayout.Space(5f);
		system.usageFactor = (SilantroFuelDistributor.FuelFactor)EditorGUILayout.EnumPopup("Update Function",system.usageFactor);
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Fuel Flow: ", system.fuelFlow.ToString ("0.00000") + " kg/Tick");
		//
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Tank Operations", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		system.dumpFuel = EditorGUILayout.Toggle ("Dump Fuel", system.dumpFuel);
		if (system.dumpFuel) {
			system.refillTank = false;
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Dump Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			system.fuelDumpRate = EditorGUILayout.FloatField ("Dump Rate", system.fuelDumpRate);
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Fuel Flow: ", system.actualFlowRate.ToString ("0.00") + " kg/s");
		}
		GUILayout.Space(5f);
		system.refillTank = EditorGUILayout.Toggle ("Refuel Tank", system.refillTank);
		if (system.refillTank) {
			system.dumpFuel = false;
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Refil Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			system.refuelRate = EditorGUILayout.FloatField ("Transfer Rate", system.refuelRate);
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Fuel Flow: ", system.actualrefuelRate.ToString ("0.00") + " kg/s");
		}
		//
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Alert Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		system.minimumFuelAmount = EditorGUILayout.FloatField ("Minimum Fuel Amount", system.minimumFuelAmount);
		GUILayout.Space(4f);
		system.fuelAlert = EditorGUILayout.ObjectField ("Fuel Alert", system.fuelAlert, typeof(AudioClip), true) as AudioClip;
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (systemObject.targetObject, "Distributor Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (system);
			EditorSceneManager.MarkSceneDirty (system.gameObject.scene);
		}
		systemObject.ApplyModifiedProperties();
	}
}
#endif