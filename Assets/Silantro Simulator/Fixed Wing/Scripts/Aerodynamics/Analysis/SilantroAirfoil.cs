using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroAirfoil : MonoBehaviour {
	//--------------------------SHAPE
	[HideInInspector]public string Identifier;
	[HideInInspector]public List<float> x = new List<float> ();
	[HideInInspector]public List<float> y = new List<float> ();
	[HideInInspector]public List<float> xt = new List<float> ();
	[HideInInspector]public float maximumThickness;
	[HideInInspector]public float thicknessLocation;
	[HideInInspector]public float tc, xtc;
	[HideInInspector]public float airfoilArea;
	//-------------------------DATA
	[HideInInspector]public List<float> lift;[HideInInspector]public List<float> drag;[HideInInspector]public List<float> moment;
	[HideInInspector]public List<float> LD;[HideInInspector]public List<float> AC;[HideInInspector]public List<float> CP;[HideInInspector]public List<float> alphas;
	[HideInInspector]public AnimationCurve liftCurve;
	[HideInInspector]public AnimationCurve dragCurve;[HideInInspector]public AnimationCurve momentCurve;
	[HideInInspector]public AnimationCurve centerCurve;[HideInInspector]public AnimationCurve pressureCurve;
	//------------------------LIMITS
	[HideInInspector]public float maxCl;[HideInInspector]public float maxCd;[HideInInspector]public float maxClCd;
	[HideInInspector]public float stallAngle;[HideInInspector]public float aerodynamicCenter;[HideInInspector]public string ReynoldsNumber;
	[HideInInspector]public bool detailed;
}

#if UNITY_EDITOR
[CustomEditor(typeof(SilantroAirfoil))]
public class AirfoilEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();
		EditorGUI.BeginChangeCheck ();
		serializedObject.Update ();
		SilantroAirfoil foil = (SilantroAirfoil)target;


		GUILayout.Space(3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Properties", MessageType.None);
		GUI.color = backgroundColor;
		//Write Airfoil Name
		EditorGUILayout.LabelField ("Identifier",foil.Identifier);

		//DIMENSIONS
		GUILayout.Space(8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Geometry", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		EditorGUILayout.LabelField ("Maximum Thickness", foil.tc.ToString ("0.00") + " %c");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Thickness Location", foil.xtc.ToString ("0") + " %c");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Surface Area", foil.airfoilArea.ToString ("0.0000") + " /m2");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Aerodynamic Center", foil.aerodynamicCenter.ToString ("0.00") + " %c");
		//PERFORMANCE
		GUILayout.Space(8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Base Data", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		EditorGUILayout.CurveField ("Lift Curve", foil.liftCurve);
		GUILayout.Space (2f);
		EditorGUILayout.CurveField ("Drag Curve", foil.dragCurve);
		GUILayout.Space (2f);
		EditorGUILayout.CurveField ("Moment Curve", foil.momentCurve);
		if (foil.detailed) {
			GUILayout.Space (2f);
			EditorGUILayout.CurveField ("C.P Curve", foil.centerCurve);
		}
		//LIMITS
		GUILayout.Space(8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Performance", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		EditorGUILayout.LabelField ("Stall Angle",foil.stallAngle.ToString("0.0 °"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Maximum Cl",foil.maxCl.ToString("0.000"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Maximum Cd",foil.maxCd.ToString("0.000"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Max Cl/Cd",foil.maxClCd.ToString("0.00"));
		if (foil.detailed) {
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Reynolds Number", foil.ReynoldsNumber.ToString ());
		}
	}

}
#endif
