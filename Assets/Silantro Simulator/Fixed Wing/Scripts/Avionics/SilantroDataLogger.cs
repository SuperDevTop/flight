using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroDataLogger : MonoBehaviour {
	//FILE CONFIG
	[HideInInspector]public string savefileName;
	public enum FileExtension
	{
		txt,
		csv
	}
	[HideInInspector]public FileExtension dataExtension = FileExtension.txt;
	[HideInInspector]public string saveLocation = "C:\\Users\\Public\\";
	//
	[HideInInspector]public float logRate = 5f;
	float timer;
	float actualLogRate;
	public enum DataType
	{
		FlightData
	}
	[HideInInspector]public DataType dataType = DataType.FlightData;
	[HideInInspector]public SilantroController source;
	//
	//STATIC DATA
	string aircraftName;
	string aircraftType;
	string engineName;
	string engineType;
	string engineCount;
	string loadedWeight;
	string loadedFuel;
	string date;
	string wingLoad;
	//
	//DYNAMIC
	float flightTime;
	float speed;
	float mach;
	float altitude;
	float climbRate;
	int heading;
	float currentFuel;
	float currentWeight;
	float engineThrust;
	float ThrustWeightRatio;
	float fuelConsumption;
	float wingLoading;
	//
	float throttle;
	float roll;
	float pitch;
	float yaw;
	float aoa;
	float gforce;float weight;
	//
	private List<string[]> dataRow = new List<string[]>();


	[HideInInspector]public List<Vector3> logPoints = new List<Vector3> ();
	[HideInInspector]public float positionLogRate = 2;
	float actualPositionLogRate;
	float positionTimer;
	string timeStamp;
	[HideInInspector]public bool showPath = true;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeLog()
	{
		timer = 0.0f;positionTimer = 0.0f;
		if (logRate != 0){actualLogRate = 1.0f / logRate;} 
		else {actualLogRate = 0.10f;}
		if (positionLogRate != 0){actualPositionLogRate = 1.0f / positionLogRate;} 
		//GET STATIC VALUES
		aircraftName = source.silantroAircraftName;
		aircraftType = source.aircraftType.ToString ();
		engineType = source.silantroEngineType;
		engineName = source.silantroEngineName;
		engineCount = source.silantroEngineCount.ToString ();
		if (source.fuelsystem != null) {
			loadedFuel = source.fuelsystem.TotalFuelRemaining.ToString ();
		}
		//
		if (source.fuelsystem != null) {
			weight = source.emptyWeight + source.fuelsystem.TotalFuelRemaining;
		}
		if (source.engineType != SilantroController.EngineType.Electric && source.engineType != SilantroController.EngineType.Unpowered) {
			loadedWeight = weight.ToString();
		}
		wingLoad = (weight/source.totalWingArea).ToString();

		//
		timeStamp = DateTime.Now.Minute.ToString();
		//WRITE INITIAL VALUES
		if (saveLocation != null) {
			//POSITION
			using (System.IO.StreamWriter positionLogger = System.IO.File.AppendText (saveLocation + "" + savefileName + timeStamp + ".txt")) {
				Vector3 currentPosition = source.aircraft.transform.position;
				string x = currentPosition.x.ToString();
				string y = currentPosition.y.ToString();
				string z = currentPosition.z.ToString();
				positionLogger.Write (x+","+y+","+z + Environment.NewLine);
			}
			//1. TXT
			if (dataExtension == FileExtension.txt) {
				File.WriteAllText (saveLocation + "" + savefileName + ".txt", "Flight Data");
				//
				using (System.IO.StreamWriter writeText = System.IO.File.AppendText (saveLocation + "" + savefileName + ".txt")) {
					writeText.WriteLine ("<<>>");
					writeText.WriteLine ("<<>>");
					writeText.WriteLine ("Aircraft Name: " + aircraftName);
					writeText.WriteLine ("Aircraft Type: " + aircraftType+ " "+ source.wingType.ToString ());
					writeText.WriteLine ("<<>>");
					writeText.WriteLine ("Engine Name: " + engineName);
					writeText.WriteLine ("Engine Type: " + engineType);
					writeText.WriteLine ("Engine Count: " + engineCount);
					writeText.WriteLine ("<<>>");
					writeText.WriteLine ("Loaded Fuel: " + loadedFuel + " kg");
					writeText.WriteLine ("Loaded Weight: " + loadedWeight + " kg");
					writeText.WriteLine ("Wing Loading: " + wingLoad + " kg/m2");
					writeText.WriteLine ("Date: " + DateTime.Now.ToString ("f"));
					writeText.WriteLine ("<<>>");
					writeText.WriteLine ("Flight Time  " + "Speed  " + " Climb Rate  " + "    Heading  " + " Altitude  " + " Fuel  " + " Thrust Generated  ");
				}
			}
			//2. CSV
			else if (dataExtension == FileExtension.csv) {
				string[] an = new string[1];an [0] = "Aircraft Name: " + aircraftName;dataRow.Add (an);
				string[] bn = new string[1];bn [0] = "Aircraft Type: " + aircraftType + " "+ source.wingType.ToString ();dataRow.Add (bn);
				string[] cn = new string[1];cn [0] =  " ";;dataRow.Add (cn);
				string[] dn = new string[1];dn [0] = "Engine Name: " + engineName;dataRow.Add (dn);
				string[] en = new string[1];en [0] = "Engine Type: " + engineType;dataRow.Add (en);
				string[] fn = new string[1];fn [0] = "Engine Count: " + engineCount;dataRow.Add (fn);
				string[] gn = new string[1];gn [0] =  " ";dataRow.Add (gn);
				string[] hn = new string[1];hn [0] = "Loaded Fuel: " + loadedFuel + " kg";dataRow.Add (hn);
				string[] In = new string[1];In [0] = "Loaded Weight: " + loadedWeight + " kg";dataRow.Add (In);
				string[] jn = new string[1];jn [0] = "Wing Loading: " + wingLoad + " kg/m2";dataRow.Add (jn);
				string[] kn = new string[1];kn [0] = "Date: " + DateTime.Now.ToString ("f");dataRow.Add (kn);
				string[] space = new string[2];
				space [0] = " ";
				space [1] = " ";
				dataRow.Add (space);
				//
				//
				string[] performanceRow = new string[20];
				performanceRow[0] = "Flight Time";
				performanceRow[1] = "Speed";
				performanceRow[2] = "Mach";
				performanceRow[3] = "Altitude";
				performanceRow[4] = "Heading";
				performanceRow[5] = "Climb Rate";
				performanceRow[6] = "Wing Loading";
				performanceRow[7] = "Weight";
				performanceRow[8] = "FCC";
				performanceRow[9] = "Thrust";
				performanceRow[10] = "TWC";
				performanceRow[11] = "Fuel";
				performanceRow[12] = " ";
				performanceRow[13] = "AOA";
				performanceRow[14] = "Roll Input";
				performanceRow[15] = "Pitch Input";
				performanceRow[16] = "Yaw Input";
				performanceRow[17] = "Throttle Level";
				performanceRow[18] = " ";
				performanceRow[19] = "G-Force";
				dataRow.Add (performanceRow);
			}
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		timer += Time.deltaTime;positionTimer += Time.deltaTime;
		if (source != null && source.silantroActive) {
			flightTime += Time.deltaTime;
			//COLLECT DATA
			if (source != null) {
				speed = source.silantroSpeed;
				altitude = source.silantroAltitude;
				climbRate = source.silantroVerticalSpeed;
				heading = (int)source.silantroHeading;
				currentFuel = source.silantroFuel;
				currentWeight = source.silantroWeight;
				wingLoading = source.silantroWingLoading;
				ThrustWeightRatio = source.silantroThrustWeightRatio;
				engineThrust = source.silantroThrust;
				fuelConsumption = source.silantroFuelConsumption;
				//
				throttle = source.throttleInput;
				roll = source.rollInput;
				pitch = source.pitchInput;
				yaw = source.yawInput;
				//
				aoa = source.wings[0].angleOfAttack;
				gforce = source.coreSystem.gForce;
			}
			//LOG CURRENT DATA
			if (timer > actualLogRate) {
				if (dataExtension == FileExtension.csv) {
					WriteCsvLog ();
				} else {
					WriteTxtLog ();
				}
			}

			//LOG POSITION
			if (positionTimer > actualPositionLogRate) {
				LogPosition ();
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void LogPosition()
	{
		positionTimer = 0f;//RESET
		Vector3 currentPosition = source.aircraft.transform.position;
		logPoints.Add(currentPosition);
		if (saveLocation != null) {
			using (System.IO.StreamWriter positionLogger = System.IO.File.AppendText (saveLocation + "" + savefileName + timeStamp + ".txt")) {
				string x = currentPosition.x.ToString();
				string y = currentPosition.y.ToString();
				string z = currentPosition.z.ToString();
				positionLogger.Write (x+","+y+","+z + Environment.NewLine);
			}
		}
	}


	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if (logPoints.Count > 3 && showPath) {
			for (int i = 0; i < logPoints.Count-1; i++) {
				Vector3 point1 = logPoints [i];
				Vector3 point2 = logPoints [i + 1];
				Handles.DrawLine (point1, point2);
				//
				Gizmos.color = Color.red;
				Gizmos.DrawSphere (point1, 5f);

			}
		}
	}
	#endif


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//WRITES TXT LOG FILE
	void WriteTxtLog()
	{
		timer = 0f;//RESET
		if (source != null) {
			string minSec = string.Format ("{0}:{1:00}", (int)flightTime / 60, (int)flightTime % 60);
			//WRITE TXT DATA
			if (saveLocation != null) {
				using (System.IO.StreamWriter writeText = System.IO.File.AppendText (saveLocation + "" + savefileName + ".txt")) {
					writeText.Write (minSec + " mins     " + speed.ToString ("0000.0") + " knots    " + climbRate.ToString ("  0000.0") + " ft/min    " + heading.ToString ("000.0 ") + "°      " + altitude.ToString ("000000.0") + " ft          " + currentFuel.ToString ("00000.0") + " kg            " + engineThrust.ToString ("000000.0") + " N             "  + Environment.NewLine);
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//WRITES CSV LOG FILE
	void WriteCsvLog()
	{
		timer = 0f;//RESET
		string minSec = string.Format ("{0}:{1:00}", (int)flightTime / 60, (int)flightTime % 60);
		if (saveLocation != null) {
			string[] performanceData = new string[20];
			performanceData[0] = minSec + " min";
			performanceData [1] = speed.ToString ("0.0") + " knots";mach = speed / 666.7f;
			performanceData [2] = mach.ToString ("0.00");
			performanceData [3] = altitude.ToString ("0.0") + " ft";
			performanceData [4] = heading.ToString ("0.0") + " °";
			performanceData [5] = climbRate.ToString ("0.0") + " ft/min";
			performanceData [6] = wingLoading.ToString ("0.0") + " kg/m2";
			performanceData [7] = currentWeight.ToString ("0.0") + " kg";
			performanceData [8] = fuelConsumption.ToString ("0.0") + " kg/s";
			performanceData [9] = engineThrust.ToString ("0.0") + " N";
			performanceData [10] = ThrustWeightRatio.ToString ("0.00") + " N/kg";
			performanceData [11] = currentFuel.ToString ("0.0") + " kg";
			performanceData[12] = " ";
			performanceData[13] = aoa.ToString ("0.0") + " °";
			performanceData[14] = roll.ToString ("0.00");
			performanceData[15] = pitch.ToString ("0.00");
			performanceData[16] = yaw.ToString ("0.00");
			performanceData[17] = (throttle*100f).ToString ("0.0") + " %";
			performanceData[18] = " ";
			performanceData[19] = gforce.ToString ("0.00");
			dataRow.Add (performanceData);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnApplicationQuit(){StoreCSVData ();}



	//----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void StoreCSVData()
	{
		if (dataExtension == FileExtension.csv && saveLocation != null) {
			string[][] output = new string[dataRow.Count][];
			for (int i = 0; i < output.Length; i++) {
				output [i] = dataRow [i];
			}
			//
			int length = output.GetLength (0);
			string delimiter = ",";
			StringBuilder builder = new StringBuilder ();
			for (int index = 0; index < length; index++)
				builder.AppendLine (string.Join (delimiter, output [index]));

			StreamWriter streamWriter = System.IO.File.CreateText (saveLocation + "" + savefileName + ".csv");
			streamWriter.WriteLine (builder);
			streamWriter.Close ();
		}
	}
}




#if UNITY_EDITOR
[CustomEditor(typeof(SilantroDataLogger))]
public class BlackBoxEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		serializedObject.Update ();
		//
		SilantroDataLogger box = (SilantroDataLogger)target;
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Data Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		box.savefileName = EditorGUILayout.TextField ("Identifier", box.savefileName);
		GUILayout.Space (4f);
		box.dataExtension = (SilantroDataLogger.FileExtension)EditorGUILayout.EnumPopup ("Extension", box.dataExtension);
		GUILayout.Space (5f);
		box.saveLocation = EditorGUILayout.TextField ("Save Location", box.saveLocation);
		GUILayout.Space (3f);
		box.logRate = EditorGUILayout.FloatField ("Log Rate", box.logRate);

		GUILayout.Space (8f);
		box.showPath = EditorGUILayout.Toggle ("Show Flight Path", box.showPath);
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (serializedObject.targetObject, "BlackBox Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (box);
			EditorSceneManager.MarkSceneDirty (box.gameObject.scene);
		}
	}
}
#endif