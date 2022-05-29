//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class SilantroBody : MonoBehaviour {
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public float maximumDiameter = 20f;[HideInInspector]public int resolution = 10;GameObject point;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[Serializable]
	public class SectionPoint
	{
		public Transform sectionTransform;
		public float sectionDiameterPercentage = 10;
		public float sectionHeightPercentage = 10;
		[HideInInspector]public List<Vector3> sectionPointList;
		public float height, width;
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public List<SectionPoint> sectionPoints;
	[HideInInspector]public float totalArea,aircraftLength;float totalPlateCount;
	[HideInInspector]public float skinDragCoefficient;public enum SurfaceFinish{SmoothPaint,PolishedMetal,ProductionSheetMetal,MoldedComposite,PaintedAluminium}
	[HideInInspector]public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public Rigidbody aircraft;
	[HideInInspector]public float airspeed =10f,k,knotSpeed,totalDrag,RE;


	bool allOk;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeBody()
	{

		//----------------------------
		_checkPrerequisites ();


		if(allOk){
			//SET FINISH FACTOR
			if (surfaceFinish == SurfaceFinish.MoldedComposite) {k = 0.17f;}
			if (surfaceFinish == SurfaceFinish.PaintedAluminium) {k = 3.33f;}
			if (surfaceFinish == SurfaceFinish.PolishedMetal) {k = 0.50f;}
			if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) {k = 1.33f;}
			if (surfaceFinish == SurfaceFinish.SmoothPaint) {k = 2.08f;}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (controller != null &&  aircraft != null) {
			allOk = true;
		} else if (aircraft == null) {
			Debug.LogError("Prerequisites not met on " + transform.name + "....Aircraft rigidbody not assigned");
			allOk = false;
		}
		else if (controller == null) {
			Debug.LogError("Prerequisites not met on " + transform.name + "....controller not assigned");
			allOk = false;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if(controller != null){
		airspeed = aircraft.velocity.magnitude;knotSpeed = airspeed * 1.944f;
		Vector3 dragForce = -aircraft.velocity;dragForce.Normalize();

		if (airspeed > 0) {
			skinDragCoefficient = EstimateSkinDragCoefficient (airspeed);
			totalDrag = 0.5f * controller.coreSystem.airDensity * airspeed * airspeed * totalArea * skinDragCoefficient;
		}

		dragForce *= totalDrag;if (totalDrag > 0) {aircraft.AddForce (dragForce, ForceMode.Force);}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateRe(float inputSpeed)
	{
		float Re1 = (controller.coreSystem.airDensity * inputSpeed * aircraftLength) / controller.coreSystem.viscocity;float Re2;
		if (controller.coreSystem.machSpeed < 0.9f) {Re2 = 38.21f * Mathf.Pow (((aircraftLength *3.28f)/ (k/100000)), 1.053f);} 
		else {Re2 = 44.62f * Mathf.Pow (((aircraftLength*3.28f) / (k/100000)), 1.053f) * Mathf.Pow (controller.coreSystem.machSpeed, 1.16f);}
		float superRe = Mathf.Min (Re1, Re2);RE = superRe;return superRe;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateSkinDragCoefficient(float velocity)
	{
		float Recr = EstimateRe (velocity);
		float baseCf = frictionDragCurve.Evaluate (Recr) / 1000f;
		
		//WRAPPING CORRECTION
		float Cfa = baseCf*(0.0025f*(aircraftLength/maximumDiameter)*Mathf.Pow(Recr,-0.2f));
		//SUPERVELOCITY CORRECTION
		float Cfb = baseCf * Mathf.Pow ((maximumDiameter / aircraftLength), 1.5f);
		//PRESSURE CORRECTION
		float Cfc = baseCf*7*Mathf.Pow((maximumDiameter/aircraftLength),3f);
		float actualCf = 1.03f * (baseCf + Cfa + Cfb + Cfc);
		return actualCf;
	}


	[HideInInspector]public AnimationCurve frictionDragCurve;
	[HideInInspector]public List<float> sectionAreas = new List<float>();
	[HideInInspector]public float maximumCrossArea,maximumSectionDiameter,finenessRatio;[HideInInspector]public int layer;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{

		frictionDragCurve = new AnimationCurve ();
		frictionDragCurve.AddKey (new Keyframe (1000000000, 1.5f));
		frictionDragCurve.AddKey (new Keyframe (100000000, 2.0f));
		frictionDragCurve.AddKey (new Keyframe (10000000, 2.85f));
		frictionDragCurve.AddKey (new Keyframe (1000000, 4.1f));
		frictionDragCurve.AddKey (new Keyframe (100000, 7.0f));


		if (sectionPoints != null && sectionPoints.Count > 0) {
			for (int a = 0; a < sectionPoints.Count; a++) {
				if (sectionPoints [a].sectionTransform == null) {
					sectionPoints.Remove (sectionPoints [a]);
				}
			}

		
			sectionAreas = new List<float> ();
			for (int i = 0; i < sectionPoints.Count - 1; i++) {
				//AREA
				float sectionWidth = sectionPoints [i].width;float sectionHeight = sectionPoints [i].height;
				float area = 3.142f * sectionHeight * sectionWidth;sectionAreas.Add (area);maximumCrossArea = sectionAreas.Max ();
				layer = sectionAreas.IndexOf (maximumCrossArea);
			}
			
			//EQUIVALENT DIAMETER
			float sectionWidthM = sectionPoints [layer].width;
			float sectionHeightM = sectionPoints [layer].height;
			float perimeter = 6.284f * Mathf.Pow ((0.5f * (Mathf.Pow (sectionHeightM, 2) + Mathf.Pow (sectionWidthM, 2))), 0.5f);
			maximumSectionDiameter = (1.55f * Mathf.Pow (maximumCrossArea, 0.625f)) / Mathf.Pow (perimeter, 0.25f);


			totalArea = 0f;
			totalPlateCount = 0;
			foreach (SectionPoint position in sectionPoints) {
			
				if (position.sectionTransform != null) {
					position.height = (maximumDiameter * position.sectionHeightPercentage * 0.01f) / 2;
					position.width = (maximumDiameter * position.sectionDiameterPercentage * 0.01f) / 2;
					DrawEllipse (position.sectionTransform, position.width, position.height, out position.sectionPointList);
				}
			}

			if (sectionPoints.Count > 0) {
				for (int a = 0; a < sectionPoints.Count - 1; a++) {
					float sectionArea = 0;
					float sectionCount = 0;
					DrawConnection (sectionPoints [a].sectionPointList, sectionPoints [a + 1].sectionPointList, out sectionArea, out sectionCount);
					totalArea += sectionArea;
					totalPlateCount += sectionCount;
				}
				aircraftLength = Vector3.Distance (sectionPoints [0].sectionTransform.position, sectionPoints [sectionPoints.Count - 1].sectionTransform.position);
				float noseArea = 3.142f * sectionPoints [0].height * sectionPoints [0].width;
				totalArea += noseArea;
			}
			finenessRatio = aircraftLength / maximumSectionDiameter;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void DrawEllipse(Transform positionTransform,float a,float c,out List<Vector3> outList)
	{
		outList = new List<Vector3> ();
		Quaternion q1 = Quaternion.AngleAxis (90, positionTransform.right);
		for (float i = 0; i < 2 * Mathf.PI; i += 2 * Mathf.PI / resolution) {
			var newPoint = positionTransform.position+(q1*positionTransform.rotation * (new Vector3 (a * Mathf.Cos (i), 0, c* Mathf.Sin (i))));
			var lastPoint = positionTransform.position+(q1*positionTransform.rotation * (new Vector3 (a * Mathf.Cos (i + 2 * Mathf.PI / resolution), 0, c * Mathf.Sin (i + 2 * Mathf.PI / resolution))));
			Handles.DrawLine (newPoint, lastPoint);
			Gizmos.color = Color.red;outList.Add (newPoint);
			Gizmos.DrawSphere (newPoint, 0.02f);
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void DrawConnection(List<Vector3> pointsA,List<Vector3> pointsB,out float area, out float count)
	{
		area = 0f;count = 0;float sectionArea;
		if (pointsA.Count == pointsB.Count && pointsA.Count > 0) {
			for(int i = 0;i< pointsA.Count-1;i++)
			{
				Handles.color = new Color (1, 0, 0, 0.3f);
				Handles.DrawLine (pointsA[i], pointsB[i]);
				Handles.DrawAAConvexPolygon (pointsA [i],pointsA [i + 1],  pointsB [i + 1], pointsB [i]);
				sectionArea = EstimatePanelSectionArea (pointsA [i], pointsA [i + 1], pointsB [i + 1], pointsB [i]);
				area += sectionArea;count += 1;
			}
			//DRAW FROM END BACK TO START
			Handles.DrawAAConvexPolygon (pointsA [pointsA.Count-1],pointsA [0],  pointsB [0], pointsB [pointsA.Count-1]);
			float closeArea = EstimatePanelSectionArea (pointsA [pointsA.Count-1],pointsA [0],  pointsB [0], pointsB [pointsA.Count-1]);
			area += closeArea;
			count += 1;
		}
	}
	#endif


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void AddElement()
	{
		if (sectionPoints != null) {point = new GameObject ("Section " + (sectionPoints.Count + 1));} 
		if(sectionPoints == null){sectionPoints = new List<SectionPoint> ();point = new GameObject ("Section 1");}
		point.transform.parent = this.transform;point.transform.localPosition = Vector3.zero;
		if (sectionPoints != null && sectionPoints.Count > 1) {
			Vector3 predisessorPosition = sectionPoints[sectionPoints.Count-1].sectionTransform.localPosition;
			point.transform.localPosition = new Vector3 (predisessorPosition.x, predisessorPosition.y, predisessorPosition.z - 0.5f);
		}
		SectionPoint dragPoint = new SectionPoint ();
		dragPoint.sectionTransform = point.transform;
		sectionPoints.Add (dragPoint);
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void AddSupplimentElement(float zFloat)
	{
		if (sectionPoints != null) {point = new GameObject ("Section " + (sectionPoints.Count + 1));} 
		point.transform.parent = this.transform;point.transform.localPosition = new Vector3(0,0,zFloat);
		SectionPoint dragPoint = new SectionPoint ();dragPoint.sectionTransform = point.transform;sectionPoints.Add (dragPoint);
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimatePanelSectionArea(Vector3 panelLeadingLeft, Vector3 panelLeadingRight, Vector3 panelTrailingLeft, Vector3 panelTrailingRight)
	{
		//BUILD TRAPEZOID VARIABLES
		float panelArea,panelLeadingEdge,panelTipEdge,panalTrailingEdge,paneRootEdge,panelDiagonal;
		//SOLVE
		panelLeadingEdge = (panelTrailingLeft - panelLeadingLeft).magnitude;panelTipEdge = (panelTrailingRight - panelTrailingLeft).magnitude;panalTrailingEdge = (panelLeadingRight - panelTrailingRight).magnitude;paneRootEdge = (panelLeadingLeft - panelLeadingRight).magnitude;
		panelDiagonal = 0.5f * (panelLeadingEdge + panelTipEdge + panalTrailingEdge + paneRootEdge);
		panelArea = Mathf.Sqrt(((panelDiagonal-panelLeadingEdge) * (panelDiagonal-panelTipEdge) * (panelDiagonal-panalTrailingEdge) * (panelDiagonal-paneRootEdge)));
		return panelArea;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE FACTOR POSITION
	public Vector3 EstimateSectionPosition(Vector3 lhs,Vector3 rhs,float factor){Vector3 estimatedPosition = lhs + ((rhs - lhs) * factor);return estimatedPosition;}
}



#if UNITY_EDITOR
[CustomEditor(typeof(SilantroBody))]
public class BodyEditor: Editor
{

	private static GUIContent deleteButton = new GUIContent("Remove","Delete");
	private static GUILayoutOption buttonWidth = GUILayout.Width (60f);

	Color backgroundColor;Color silantroColor = new Color(1,0.4f,0);
	SilantroBody body;int listSize;
	SerializedObject bodyObject;SerializedProperty sectionList;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		body = (SilantroBody)target;
		bodyObject = new SerializedObject (body);
		sectionList = bodyObject.FindProperty ("sectionPoints");
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;DrawDefaultInspector ();
		EditorGUI.BeginChangeCheck();bodyObject.UpdateIfRequiredOrScript();

		EditorGUILayout.HelpBox ("Note: Still in the testing phase, please report any bugs or issues you run into :))", MessageType.Warning);
		GUILayout.Space(1f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Section Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		body.maximumDiameter = EditorGUILayout.FloatField ("Maximum Diameter", body.maximumDiameter);
		GUILayout.Space(5f);
		body.resolution = EditorGUILayout.IntSlider ("Resolution", body.resolution, 5, 50);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Sections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		if (sectionList != null) {
			EditorGUILayout.LabelField ("Section Count", sectionList.arraySize.ToString ());
		}

		GUILayout.Space (5f);
		if (GUILayout.Button ("Create Section")) {body.AddElement ();}

		if (sectionList != null) {
			GUILayout.Space (2f);
			for (int i = 0; i < sectionList.arraySize; i++) {
				SerializedProperty reference = sectionList.GetArrayElementAtIndex (i);
				SerializedProperty widthPercentage = reference.FindPropertyRelative ("sectionDiameterPercentage");
				SerializedProperty heightPercentage = reference.FindPropertyRelative ("sectionHeightPercentage");
				SerializedProperty sectionHeight = reference.FindPropertyRelative ("height");
				SerializedProperty sectionWidth = reference.FindPropertyRelative ("width");

				GUI.color = new Color (1, 0.8f, 0);
				EditorGUILayout.HelpBox ("Section : " + (i + 1).ToString (), MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (3f);
				widthPercentage.floatValue = EditorGUILayout.Slider ("Width Percentage",widthPercentage.floatValue, 1f, 100f);
				GUILayout.Space (3f);
				heightPercentage.floatValue = EditorGUILayout.Slider ("Height Percentage",heightPercentage.floatValue, 1f, 100f);
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Section Width", sectionWidth.floatValue.ToString ("0.00") + " m" );
				GUILayout.Space (1f);
				EditorGUILayout.LabelField ("Section Height", sectionHeight.floatValue.ToString ("0.00") + " m");

				GUILayout.Space (3f);
				if (GUILayout.Button (deleteButton, EditorStyles.miniButtonRight, buttonWidth)) {
					Transform trf = body.sectionPoints [i].sectionTransform;
					body.sectionPoints.RemoveAt (i);
					if (trf != null) {
						DestroyImmediate (trf.gameObject);
					}
				}
				GUILayout.Space (5f);
			}
		}

		GUILayout.Space (8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField ("Total Length", body.aircraftLength.ToString ("0.00") + " m");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Equivalent Diameter", body.maximumSectionDiameter.ToString ("0.00") + " m");


		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Flow Consideration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		body.surfaceFinish = (SilantroBody.SurfaceFinish)EditorGUILayout.EnumPopup("Surface Finish",body.surfaceFinish);
		GUILayout.Space (5f);
		EditorGUILayout.LabelField ("Wetted Area", body.totalArea.ToString ("0.000") + " m2");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Sector Area", body.maximumCrossArea.ToString ("0.000") + " m2");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Fineness Ratio", body.finenessRatio.ToString ("0.00"));

		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Output Data", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField ("Friction Coefficient", body.skinDragCoefficient.ToString ("0.00000"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Drag", body.totalDrag.ToString ("0.0") + " N");


		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (bodyObject.targetObject, "Section Change");}
		if (GUI.changed) {
			EditorUtility.SetDirty (body);
			EditorSceneManager.MarkSceneDirty (body.gameObject.scene);
		}
		bodyObject.ApplyModifiedProperties();
	}

	//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	public override bool RequiresConstantRepaint ()
	{
		return true;
	}
}
#endif