//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroCamera : MonoBehaviour {
	//CAMERA TYPE
	public enum CameraType
	{
		Aircraft,Player
	}
	[HideInInspector]public CameraType cameraType = CameraType.Aircraft;
	//CAMERA MODE
	public enum CameraMode
	{
		Orbit,Free,Chase
	}
	[HideInInspector]public CameraMode cameraMode = CameraMode.Orbit;
	//
	public enum CameraState
	{
		Interior,Exterior
	}
	[HideInInspector]public CameraState cameraState = CameraState.Exterior;
	//CONNECTION
	[HideInInspector]public GameObject cameraTarget;
	[HideInInspector]public Camera actualCamera;
	[HideInInspector]public Camera currentCamera;
	//
	//CHASE CAMERA PROPERTIES
	[HideInInspector]public bool isControllable = true;
	[HideInInspector]public float chaseSpeed = 0.5f;
	[HideInInspector]public float turnSpeed = 5f;
	[HideInInspector]public float cameraHeight = 1f;//Camera distance on the Y_AXIS
	[HideInInspector]public float cameraDistance = 20f;//Camera distance on the Z_AXIS
	[HideInInspector]public float lateralDistance = 20f;//Camera distance on the X_AXIS
	Vector3 targetPosition;Vector3 demoPosition;float actualDistance;
	//
	//ORBIT CAMERA PROPERTIES
	[HideInInspector]public float orbitDistance = 20.0f;
	[HideInInspector]public float orbitHeight = 2.0f;
	[HideInInspector]private bool FirstClick = false;
	[HideInInspector]private Vector3 MouseStart;
	[HideInInspector]private float orbitAngle = 180.0f;
	Vector3 cameraRange;Vector3 cameraPosition;
	//
	//FREE CAMERA PROPERTIES
	[HideInInspector]public float azimuthSensitivity = 1;
	[HideInInspector]public float elevationSensitivity = 1;
	[HideInInspector]public float radiusSensitivity = 100;
	[HideInInspector]private float azimuth, elevation;
	[HideInInspector]public float radius;
	[HideInInspector]public float maximumRadius = 20f;
	float xRotation;
	float yRotation;
	Vector3 cameraDirection;
	[HideInInspector]public bool snap = true;
	//
	//INTERIOR CAMERA PROPERTIES
	[HideInInspector]public Camera interiorcamera;
	[HideInInspector]public float mouseSensitivity = 100.0f;
	[HideInInspector]public float clampAngle = 80.0f;
	[HideInInspector]float yInteriorRotation;
	float xInteriorRotation;
	Vector3 initialCameraPosition;
	Quaternion initialCameraRotation;
	[HideInInspector]public GameObject interiorPilot;//PILOT GAMEOBJECT
	[HideInInspector]public SilantroController controller;
	Vector3 filterPosition;float filerY;Vector3 filteredPosition;
	Quaternion filterRotation;float filterX;Quaternion filteredRotation;
	Quaternion interiorRotation;

	// ---------------------------------------------------------CONTROL FUNCTIONS-------------------------------------------------------------------------------------------------
	public void ActivateInteriorCamera()
	{
		if (isControllable) {
			if (interiorcamera != null) {
				//DISABLE EXTERIOR CAMERA
				actualCamera.enabled = false;
				AudioListener exteriorListener = actualCamera.GetComponent<AudioListener> ();
				if (exteriorListener != null) {
					exteriorListener.enabled = false;
				}
				//CHANGE PILOT STATE
				if (interiorPilot != null) {
					interiorPilot.SetActive (false);
				}
				cameraState = CameraState.Interior;
				//ENABLE INTERIOR CAMERA
				if (interiorcamera != null) {
					interiorcamera.enabled = true;
					AudioListener interiorListener = interiorcamera.GetComponent<AudioListener> ();
					if (interiorListener != null) {
						interiorListener.enabled = true;
					}
					if (snap) {
						//RETURN TO INITIAL POSITION
						interiorcamera.transform.localPosition = initialCameraPosition;
						interiorcamera.transform.localRotation = initialCameraRotation;
						interiorRotation = initialCameraRotation;
						yInteriorRotation = 0f;
						xInteriorRotation = 0f;
					}
				}
				//
				currentCamera = interiorcamera;
				//SET SOUND STATE
				if (controller != null) {
					controller.currentSoundState = SilantroController.SoundState.Interior;
				}
			} else {
				Debug.Log ("Interior Camera has not been setup");
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ActivateExteriorCamera()
	{
		if (isControllable && actualCamera != null) {
			if (interiorcamera != null) {
				//DISABLE INTERIOR CAMERA
				interiorcamera.enabled = false;
				AudioListener interiorListener = interiorcamera.GetComponent<AudioListener> ();
				if (interiorListener != null) {
					interiorListener.enabled = false;
				}
			}
			//CHANGE PILOT STATE
			if (interiorPilot != null) {
				interiorPilot.SetActive (true);
			}
			cameraState = CameraState.Exterior;
			//ENABLE EXTERIOR CAMERA
			actualCamera.enabled = true;
			AudioListener exteriorListener = actualCamera.GetComponent<AudioListener> ();
			if (exteriorListener != null) {
				exteriorListener.enabled = true;
			}
			//
			currentCamera = actualCamera;
			//SET SOUND STATE
			if (controller != null) {
				controller.currentSoundState = SilantroController.SoundState.Exterior;
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ActivateAndSetExteriorCamera(int mode)
	{
		if (isControllable) {
			if (interiorcamera != null) {
				//DISABLE INTERIOR CAMERA
				interiorcamera.enabled = false;
				AudioListener interiorListener = interiorcamera.GetComponent<AudioListener> ();
				if (interiorListener != null) {interiorListener.enabled = false;}
			}
			//CHANGE PILOT STATE
			if (interiorPilot != null) {interiorPilot.SetActive (true);}
			cameraState = CameraState.Exterior;
			//ENABLE EXTERIOR CAMERA
			actualCamera.enabled = true;
			AudioListener exteriorListener = actualCamera.GetComponent<AudioListener> ();
			if (exteriorListener != null) {
				exteriorListener.enabled = true;
			}
			//SET CAMERA
			if (mode == 1) {cameraMode = CameraMode.Free;}
			if (mode == 2) {cameraMode = CameraMode.Orbit;}
			currentCamera = actualCamera;
			//SET SOUND STATE
			if (controller != null) {controller.currentSoundState = SilantroController.SoundState.Exterior;}
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ToggleCamera()
	{
		if (isControllable) {
			if (cameraState == CameraState.Exterior) {ActivateInteriorCamera ();} 
			else {ActivateExteriorCamera ();}
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (cameraType == CameraType.Aircraft) {
			if (actualCamera != null && controller != null) {
				allOk = true;
			} else if (actualCamera == null) {
				Debug.LogError ("Prerequisites not met on Camera " + transform.name + "....Exterior camera not assigned");
				allOk = false;
			} else if (controller == null) {
				Debug.LogError ("Prerequisites not met on Camera " + transform.name + "....Aircraft controller not assigned");
				allOk = false;
			}
		} else {
			allOk = true;
		}
	}








	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeCamera()
	{

		//CHECK COMPONENTS
		_checkPrerequisites ();


		if (allOk) {
			if (cameraType == CameraType.Aircraft) {
				//GET CAMERA COMPONENT FROM THIS GAMEOBJECT//YOU CAN CHANGE THE ACTUAL CAMERA IF YOU PLAN TO USE A DIFFERENT CAMERA
				if (actualCamera == null) {
					Debug.Log ("Exterior Camera has not been assigned");
				}
				if (actualCamera != null && actualCamera.GetComponent <AudioListener> () == null) {
					actualCamera.gameObject.AddComponent<AudioListener> ();
				}
			} else {
				actualCamera = Camera.main;
				gameObject.GetComponent<Camera> ().enabled = false;
			}
			//SETUP FREE CAMERA
			if (cameraType == CameraType.Aircraft && cameraMode == CameraMode.Orbit && actualCamera != null) {
				CartesianToSpherical (cameraTarget.transform.InverseTransformDirection (actualCamera.transform.position - cameraTarget.transform.position), out radius, out azimuth, out elevation);
				actualCamera.transform.LookAt (cameraTarget.transform);
			}
			//SETUP CAMERA AS EXTERIOR
			ActivateExteriorCamera ();
			//STORE CAMERA PROPERTIES
			if (interiorcamera != null) {
				initialCameraPosition = interiorcamera.transform.localPosition;initialCameraRotation = interiorcamera.transform.localRotation;
			}
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (actualCamera != null) {
			//PROCESS AIRCRAFT CAMERAS
			if (cameraType == CameraType.Aircraft) {
				//PROCESS EXTERior CAMERAS
				if (cameraState == CameraState.Exterior) {
					//SEND ORBIT DATA
					if (cameraMode == CameraMode.Orbit) {
						OrbitSystem ();
					}
					//SEND FREE DATA
					if (cameraMode == CameraMode.Free) {
						FreeSystem ();
					}
					//SEND CHASE DATA
					if (cameraMode == CameraMode.Chase) {
						ChaseSystem ();
					}
				} else {
					//SEND INTERIOR DATA
					InteriorSystem ();
				}
			}
			//PROCESS PLAYER CAMERA
			if (cameraType == CameraType.Player) {
				//SEND PLAYER ORBIT DATA
				PlayerSystem ();
			}
		}
	}
	//



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void InteriorSystem()
	{
		//INTERIOR CAMERA SYSTEM
		if (interiorcamera != null && Input.GetMouseButton (0)) {
			yInteriorRotation += Input.GetAxis ("Mouse X") * mouseSensitivity * Time.deltaTime;
			xInteriorRotation += -Input.GetAxis ("Mouse Y") * mouseSensitivity * Time.deltaTime;
			//CLAMP ANGLES (You can make them independent to have a different maximum for each)
			xInteriorRotation = Mathf.Clamp(xInteriorRotation,-clampAngle,clampAngle);
			yInteriorRotation = Mathf.Clamp (yInteriorRotation, -clampAngle, clampAngle);
			//ASSIGN ROTATION
			interiorRotation = Quaternion.Euler(xInteriorRotation,yInteriorRotation,0.0f);
			interiorcamera.transform.localRotation = interiorRotation;
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void ChaseSystem()
	{
		//CHASE CAMERA SYSTEM
		actualCamera.transform.LookAt (cameraTarget.transform.position + cameraTarget.transform.forward * lateralDistance);
		targetPosition = Vector3.Lerp (targetPosition, (-cameraTarget.transform.position + (cameraTarget.transform.up * cameraHeight)), Time.fixedDeltaTime * turnSpeed);
		demoPosition = cameraTarget.transform.position + (targetPosition * cameraDistance);
		actualDistance = Vector3.Distance (demoPosition, actualCamera.transform.position);
		actualCamera.transform.position = Vector3.Lerp (actualCamera.transform.position, demoPosition, Time.fixedDeltaTime * (actualDistance * chaseSpeed));
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OrbitSystem()
	{
		//ORBIT CAMERA SYSTEM
		if (Input.GetMouseButton (0)) {
			if (FirstClick) {
				MouseStart = Input.mousePosition;FirstClick = false;//Set initial click position
			}
			orbitAngle += (Input.mousePosition - MouseStart).x * Time.deltaTime;
		} 
		else {FirstClick = true;}
		//CALCULATE CAMERA ANGLE
		cameraRange = cameraTarget.transform.forward;
		cameraRange.y = 0f;cameraRange.Normalize ();cameraRange = Quaternion.Euler (0, orbitAngle, 0) * cameraRange;
		//CALCULATE CAMERA POSITION
		cameraPosition = cameraTarget.transform.position;
		cameraPosition += cameraRange * orbitDistance;
		cameraPosition += new Vector3 (0.0f, 1.0f, 0.0f) * orbitHeight;
		//SET VIEW
		targetPosition = cameraTarget.transform.position;
		//
		//APPLY TO CAMERA
		actualCamera.transform.position = cameraPosition;
		actualCamera.transform.LookAt (targetPosition);
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ORBIT PLAYER CAMERA
	void PlayerSystem()
	{
		//CALCULATE CAMERA ANGLE
		cameraRange = cameraTarget.transform.forward;
		cameraRange.y = 0f;cameraRange.Normalize ();cameraRange = Quaternion.Euler (0, orbitAngle, 0) * cameraRange;
		//CALCULATE CAMERA POSITION
		cameraPosition = cameraTarget.transform.position;
		cameraPosition += cameraRange * orbitDistance;
		cameraPosition += new Vector3 (0.0f, 1.0f, 0.0f) * orbitHeight;
		//SET VIEW
		targetPosition = cameraTarget.transform.position;
		//
		//APPLY TO CAMERA
		actualCamera.transform.position = cameraPosition;
		actualCamera.transform.LookAt (targetPosition);
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FreeSystem()
	{
		//FREE CAMERA SYSTEM
		if (Input.GetMouseButton (0)) {//Only rotate if left mouse button is pressed  down
			azimuth -= Input.GetAxis ("Mouse X") * azimuthSensitivity * Time.deltaTime;
			elevation -= Input.GetAxis ("Mouse Y") * elevationSensitivity * Time.deltaTime;
		}
		//CALCULATE DIRECTION AND POSITION
		SphericalToCartesian(radius,azimuth,elevation, out cameraDirection);
		//CLAMP ROTATION IF AIRCRAFT IS ON THE GROUND//LESS THAN radius meters
		if (cameraTarget.transform.position.y < maximumRadius) {
			filterPosition = cameraTarget.transform.position + cameraDirection;
			filerY = filterPosition.y;
			if (filerY < 2)filerY = 2;
			filteredPosition = new Vector3 (filterPosition.x, filerY, filterPosition.z);
			actualCamera.transform.position = filteredPosition;
		} else {
			actualCamera.transform.position = cameraTarget.transform.position + cameraDirection;;
		}
		//POSITION CAMERA
		actualCamera.transform.LookAt(cameraTarget.transform);
		radius = maximumRadius;//use this to control the distance from the aircraft
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SUPER CALCULATIONS
	public static void CartesianToSpherical(Vector3 cordinatesInput, out float outRadius, out float outPolar, out float outElevation) {
		if (cordinatesInput.x == 0)
		cordinatesInput.x = Mathf.Epsilon;
		outRadius = Mathf.Sqrt((cordinatesInput.x * cordinatesInput.x)+ (cordinatesInput.y * cordinatesInput.y)+ (cordinatesInput.z * cordinatesInput.z));
		outPolar = Mathf.Atan(cordinatesInput.z / cordinatesInput.x);
		if (cordinatesInput.x < 0)outPolar += Mathf.PI;
		outElevation = Mathf.Asin(cordinatesInput.y / outRadius);
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	static float jester;
	public static void SphericalToCartesian(float radius, float polar, float elevation, out Vector3 outCart) {
		jester = radius * Mathf.Cos(elevation);
		outCart.x = jester * Mathf.Cos(polar);
		outCart.y = radius * Mathf.Sin(elevation);
		outCart.z = jester * Mathf.Sin(polar);
	}
	//
}
#if UNITY_EDITOR
[CustomEditor(typeof(SilantroCamera))]
public class CameraEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	[HideInInspector]public int toolbarTab;
	[HideInInspector]public string currentTab;
	//
	SilantroCamera camera;
	SerializedObject cameraObject;
	//
	private void OnEnable()
	{
		camera = (SilantroCamera)target;
		cameraObject = new SerializedObject (camera);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		//
		serializedObject.Update();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Camera Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(6f);
		camera.cameraType = (SilantroCamera.CameraType)EditorGUILayout.EnumPopup("Function",camera.cameraType);
		//
		if (camera.cameraType == SilantroCamera.CameraType.Aircraft) {
			GUILayout.Space (10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Functionality Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (5f);
			toolbarTab = GUILayout.Toolbar (toolbarTab, new string[]{ "Exterior Camera", "Interior Camera" });
			switch (toolbarTab) {
			case 0:
				currentTab = "Exterior Camera";
				break;
			case 1:
				currentTab = "Interior Camera";
				break;
			}
			//
			switch (currentTab) {
			//
			case "Exterior Camera":
				//
				GUILayout.Space (3f);
				camera.actualCamera = EditorGUILayout.ObjectField ("Exterior Camera", camera.actualCamera, typeof(Camera), true) as Camera;
				GUILayout.Space(7f);
				camera.cameraMode = (SilantroCamera.CameraMode)EditorGUILayout.EnumPopup(" ",camera.cameraMode);
				//
				if (camera.cameraMode == SilantroCamera.CameraMode.Chase) {
					GUILayout.Space (5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Chase Camera Settings", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (2f);
					camera.cameraTarget = EditorGUILayout.ObjectField ("Aircraft", camera.cameraTarget, typeof(GameObject), true) as GameObject;
					GUILayout.Space(5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Camera Movement", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					camera.chaseSpeed = EditorGUILayout.FloatField ("Chase Speed", camera.chaseSpeed);
					GUILayout.Space(2f);
					camera.turnSpeed = EditorGUILayout.FloatField ("Turn Speed", camera.turnSpeed);
					GUILayout.Space(5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Camera Positioning", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					camera.cameraHeight = EditorGUILayout.FloatField ("Camera Height", camera.cameraHeight);
					GUILayout.Space(2f);
					camera.lateralDistance = EditorGUILayout.FloatField ("Lateral Distance", camera.lateralDistance);
					GUILayout.Space(2f);
					camera.cameraDistance = EditorGUILayout.FloatField ("View Distance", camera.cameraDistance);
				}
				if (camera.cameraMode == SilantroCamera.CameraMode.Free) {
					GUILayout.Space (5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Free Camera Settings", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (2f);
					camera.cameraTarget = EditorGUILayout.ObjectField ("Focus Point", camera.cameraTarget, typeof(GameObject), true) as GameObject;
					GUILayout.Space(5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Camera Movement", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					camera.elevationSensitivity = EditorGUILayout.Slider ("Elevation Sensitivity", camera.elevationSensitivity,0f,1f);
					GUILayout.Space(2f);
					camera.azimuthSensitivity = EditorGUILayout.Slider ("Azimuth Sensitivity", camera.azimuthSensitivity,0f,1f);
					GUILayout.Space(5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Camera Positioning", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					camera.maximumRadius = EditorGUILayout.FloatField ("Camera Distance", camera.maximumRadius);
				}
				if (camera.cameraMode == SilantroCamera.CameraMode.Orbit) {
					GUILayout.Space (5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Orbit Camera Settings", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (2f);
					camera.cameraTarget = EditorGUILayout.ObjectField ("Focus Point", camera.cameraTarget, typeof(GameObject), true) as GameObject;
					GUILayout.Space(5f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Camera Positioning", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space(2f);
					camera.orbitDistance = EditorGUILayout.FloatField ("Orbit Distance", camera.orbitDistance);
					GUILayout.Space(3f);
					camera.orbitHeight = EditorGUILayout.FloatField ("Orbit Height", camera.orbitHeight);
				}
				break;
			case "Interior Camera":
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Interior Camera Settings", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				camera.interiorcamera = EditorGUILayout.ObjectField ("Interior Camera", camera.interiorcamera, typeof(Camera), true) as Camera;
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Camera Movement", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				camera.mouseSensitivity = EditorGUILayout.Slider ("Mouse Sensitivity", camera.mouseSensitivity, 0f, 100f);
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Camera Positioning", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				camera.clampAngle = EditorGUILayout.Slider ("View Angle", camera.clampAngle, 10f, 210f);
				GUILayout.Space (5f);
				camera.snap = EditorGUILayout.Toggle ("Snap Camera", camera.snap);
				break;
			}
		}
			

		if (camera.cameraType == SilantroCamera.CameraType.Player) {
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Player Camera Settings", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			camera.actualCamera = EditorGUILayout.ObjectField ("Camera", camera.actualCamera, typeof(Camera), true) as Camera;
			GUILayout.Space(7f);
			camera.cameraTarget = EditorGUILayout.ObjectField ("Player", camera.cameraTarget, typeof(GameObject), true) as GameObject;
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Camera Positioning", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			camera.orbitDistance = EditorGUILayout.FloatField ("Orbit Distance", camera.orbitDistance);
			GUILayout.Space(3f);
			camera.orbitHeight = EditorGUILayout.FloatField ("Orbit Height", camera.orbitHeight);
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (cameraObject.targetObject, "Camera Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (camera);
			EditorSceneManager.MarkSceneDirty (camera.gameObject.scene);
		}
		cameraObject.ApplyModifiedProperties();
	}
}
#endif