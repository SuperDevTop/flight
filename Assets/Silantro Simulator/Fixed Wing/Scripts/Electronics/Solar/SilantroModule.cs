using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
[RequireComponent(typeof(BoxCollider))]
public class SilantroModule : MonoBehaviour {
	[HideInInspector]public SilantroSapphire weatherSystem;
	//
	[HideInInspector]float solarIntensity;
	//
	[HideInInspector]private BoxCollider panelCollider;
	[HideInInspector]private Vector3 BottomLeftEdge = Vector3.zero;
	[HideInInspector]private Vector3 BottomRightEdge = Vector3.zero;
	[HideInInspector]private Vector3 TopLeftEdge = Vector3.zero;
	[HideInInspector]private Vector3 TopRightEdge = Vector3.zero;
	//
	[HideInInspector]public float moduleLength;
	[HideInInspector]public float moduleWidth;
	[HideInInspector]public float moduleArea;
	[HideInInspector]float localScale;
	//
	[HideInInspector]public float terminalVoltage;
	[HideInInspector]public float terminalCurrent;
	[HideInInspector]public float terminalPower;
	//
	[HideInInspector]public SilantroCell solarCell;
	[HideInInspector]public float cellVoltage;
	[HideInInspector]public float cellCurrent;
	[HideInInspector]public int cellCount;
	//
	[HideInInspector]public float cellLength;
	[HideInInspector]public float cellWidth;
	[HideInInspector]public float cellArea;
	[HideInInspector]public float idealCellArea;
	[HideInInspector]public float superlation;
	//
	[HideInInspector]public Transform sunCenter;
	[HideInInspector]public Color silantroColor = new Color(1,0.2275f,0.2275f,0.4588f);
	//
	public enum ConnectionType
	{
		Parallel,
		Series
	}
	[HideInInspector]public ConnectionType connectionType = ConnectionType.Series;
	//
	[HideInInspector]public float incidentRadiation;
	//
	public enum Shape
	{
		Square,
		Rectangular
	}
	[HideInInspector]public Shape shape = Shape.Rectangular;
	//
	[HideInInspector]public int panelSections = 5;
	[HideInInspector]public int lengthSections = 5;
	[HideInInspector]public int widthSections = 5;
	float powerFactor;
	//
	public void OnDrawGizmos()
	{
		panelCollider = (BoxCollider)gameObject.GetComponent<Collider> ();
		if (panelCollider) {
			panelCollider.size = new Vector3 (1.0f, 0.2f, 1.0f);
		}
		//
		CalculateShape();
		//
		//DRAW EDGES
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine (BottomRightEdge, BottomLeftEdge);
		Gizmos.DrawLine (TopLeftEdge, TopRightEdge);
		Gizmos.DrawLine (BottomLeftEdge, TopLeftEdge);
		Gizmos.DrawLine (BottomRightEdge, TopRightEdge);
		//
		Gizmos.color = Color.red;
		//
		//DRAW POINTS
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(BottomRightEdge,0.03f);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(BottomLeftEdge,0.03f);
		Gizmos.color = Color.grey;
		Gizmos.DrawSphere(TopLeftEdge,0.03f);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(TopRightEdge,0.03f);
		//
		Gizmos.color = Color.red;
		if (sunCenter) {
			Gizmos.DrawLine (sunCenter.position, this.transform.position);
		}
		//Sections.
		Gizmos.color = Color.yellow;
		if (shape == Shape.Square) {
			for (int i = 0; i < panelSections; i++) {
				Vector3 HorizontalStart = BottomRightEdge + ((TopRightEdge - BottomRightEdge) * (float)i / (float)panelSections);
				Vector3 HorizontalEnd = BottomLeftEdge + ((TopLeftEdge - BottomLeftEdge) * (float)i / (float)panelSections);
				Gizmos.DrawLine (HorizontalStart, HorizontalEnd);
				//
				Vector3 VerticalStart = BottomRightEdge + ((BottomLeftEdge - BottomRightEdge) * (float)i / (float)panelSections);
				Vector3 VerticalEnd = TopRightEdge + ((TopLeftEdge - TopRightEdge) * (float)i / (float)panelSections);
				Gizmos.DrawLine (VerticalStart, VerticalEnd);
			}
			//
			moduleLength = moduleWidth = Vector3.Distance(BottomRightEdge,TopRightEdge);
			//
			cellCount = panelSections * panelSections;
			//
			cellLength = cellWidth = moduleLength/panelSections;
		} 
		else {
			for (int i = 0; i < lengthSections; i++) {
				Vector3 HorizontalStart = BottomRightEdge + ((TopRightEdge - BottomRightEdge) * (float)i / (float)lengthSections);
				Vector3 HorizontalEnd = BottomLeftEdge + ((TopLeftEdge - BottomLeftEdge) * (float)i / (float)lengthSections);
				Gizmos.DrawLine (HorizontalStart, HorizontalEnd);
				//
			}
			for (int i = 0; i < widthSections; i++) {
				Vector3 VerticalStart = BottomRightEdge + ((BottomLeftEdge - BottomRightEdge) * (float)i / (float)widthSections);
				Vector3 VerticalEnd = TopRightEdge + ((TopLeftEdge - TopRightEdge) * (float)i / (float)widthSections);
				Gizmos.DrawLine (VerticalStart, VerticalEnd);
			}
			//
			moduleLength = Vector3.Distance(BottomRightEdge,TopRightEdge);
			moduleWidth = Vector3.Distance (TopLeftEdge, TopRightEdge);
			//
			cellCount =lengthSections*widthSections;//Count number of solar cells
			//
			cellLength = moduleLength/lengthSections;
			cellWidth = moduleWidth / widthSections;
		}
		//
		//MAINTAIN SQUARE SHAPE
		if (shape == Shape.Square) {
			float xScale = this.transform.localScale.x;
			this.transform.localScale = new Vector3 (xScale, 0.2f, xScale);
			//
		}
		//
		idealCellArea = solarCell.cellLength/100f * solarCell.cellWidth/100f;
		cellArea = cellLength * cellWidth;
		superlation = cellArea / idealCellArea;
		//
		moduleArea = moduleLength*moduleWidth;//Calculate Surface Area
		//
		if (solarCell) {
			cellVoltage = solarCell.junctionVoltage*superlation;
			cellCurrent = solarCell.junctionCurrent*superlation;
			//
			if (connectionType == ConnectionType.Series) {
				terminalCurrent = cellCurrent;
				terminalVoltage = cellVoltage * cellCount;
			} else if (connectionType == ConnectionType.Parallel) {
				terminalVoltage = cellVoltage;
				terminalCurrent = cellCurrent * cellCount;
			}
		}
		//
		terminalPower = terminalCurrent*terminalVoltage;
	}
	//
	void CalculateOutput()
	{
		//
		if (solarCell) {
			cellVoltage = solarCell.junctionVoltage*superlation;
			cellCurrent = solarCell.junctionCurrent*superlation;
			//
			if (connectionType == ConnectionType.Series) {
				terminalCurrent = cellCurrent;
				terminalVoltage = cellVoltage * cellCount;
			} else if (connectionType == ConnectionType.Parallel) {
				terminalVoltage = cellVoltage;
				terminalCurrent = cellCurrent * cellCount;
			}
		}
		//
		terminalPower = terminalCurrent*terminalVoltage;
		//
		float currentTilt = this.transform.rotation.eulerAngles.x;
		float difference = weatherSystem.sunAngle - currentTilt;
		//
		if (difference > 0 && difference < 180) {
			powerFactor = Mathf.Cos (difference * Mathf.Deg2Rad);
		} else {
			powerFactor = 0;
		}
		//
		float actualCurrent = (terminalCurrent*solarIntensity)/1000f;
		float actualVoltage = (terminalVoltage*solarIntensity)/1000f;
		//
		terminalVoltage = actualVoltage;
		terminalCurrent = actualCurrent;
		//
		terminalPower = solarIntensity*moduleArea*solarCell.cellEfficiency/100f;
	}
	//
	void CalculateShape()
	{
		Vector3 BottomCenter = transform.position - ( transform.right * (transform.localScale.x * 0.5f) );
		Vector3 TopCenter = transform.position + ( transform.right * (transform.localScale.x * 0.5f) );
		//
		localScale = transform.localScale.magnitude;
		//
		//CORNERS
		BottomLeftEdge = BottomCenter + ( transform.forward * (transform.localScale.z * 0.5f) );
		BottomRightEdge = BottomCenter - ( transform.forward * (transform.localScale.z * 0.5f) );
		TopLeftEdge = TopCenter + ( transform.forward * ((transform.localScale.z * 0.5f) ) );
		TopRightEdge = TopCenter - ( transform.forward * ((transform.localScale.z * 0.5f) ) );

	}
	//
	// Update is called once per frame
	void Update () {
		if (weatherSystem) {
			solarIntensity = weatherSystem.currentSolarIntensity;
			//
			CalculateOutput();
		}

	}
}
//
//
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroModule))]
public class ModuleEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = Color.yellow;
	//

	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();serializedObject.Update ();
		//
		SilantroModule module = (SilantroModule)target;
		//
		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Module Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		module.shape = (SilantroModule.Shape)EditorGUILayout.EnumPopup ("Shape", module.shape);
		//
		GUILayout.Space(10f);
		if (module.shape == SilantroModule.Shape.Rectangular) {
			module.lengthSections = EditorGUILayout.IntField ("Length Sections", module.lengthSections);
			GUILayout.Space(3f);
			module.widthSections = EditorGUILayout.IntField ("Width Sections", module.widthSections);
		} else if (module.shape == SilantroModule.Shape.Square) {
			module.panelSections = EditorGUILayout.IntField ("Module Sections", module.panelSections);
		}
		//
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Module Length", module.moduleLength.ToString ("0.0") + " m");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Module Width", module.moduleWidth.ToString ("0.0") + " m");
		//
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Module Area", module.moduleArea.ToString ("0.0") + " m2");
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Cell Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		module.solarCell = EditorGUILayout.ObjectField ("Cell Definition", module.solarCell, typeof(SilantroCell), true) as SilantroCell;
		//
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Cell Voltage", module.cellVoltage.ToString ("0.000")+ " V");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Cell Current", module.cellCurrent.ToString ("0.000") + " A");
		GUILayout.Space(10f);
		EditorGUILayout.LabelField ("Cell Count", module.cellCount.ToString ());
		GUILayout.Space(3f);
		module.connectionType = (SilantroModule.ConnectionType)EditorGUILayout.EnumPopup("Cell Connection",module.connectionType);
		//
		if (module.cellLength*100f > module.solarCell.cellLength) {
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Module cell Length is greater than selected cell length, Reduce scale or increase cell count", MessageType.Warning);

		} else if (module.cellWidth *100f > module.solarCell.cellWidth) {
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Module cell Width is greater than selected cell width, Reduce scale or increase cell count", MessageType.Warning);
		}
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Output", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Terminal Voltage", module.terminalVoltage.ToString ("0.0")+ " V");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Terminal Current", module.terminalCurrent.ToString ("0.0")+ " A");
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Power", module.terminalPower.ToString ("0.0")+ " Watts");
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (module);
			EditorSceneManager.MarkSceneDirty (module.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif