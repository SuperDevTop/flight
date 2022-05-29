using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif


[RequireComponent(typeof(SilantroTransponder))]
public class SilantroRadar : MonoBehaviour {
	//CONNECTED SUPPORT
	[HideInInspector]public GameObject connection;
	[HideInInspector]public SilantroController connectedAircraft;
	[HideInInspector]public SilantroTransponder transponder;
	[HideInInspector]private GameObject radarCore;
	//CONTROL BOOLS
	[HideInInspector]public bool isControllable = true;
	[HideInInspector]public bool displayBounds;
	//
	public enum RadarType
	{
		Civilian,Military
	}
	[HideInInspector]public SilantroRadar.RadarType radarType = SilantroRadar.RadarType.Civilian;
	//
	//PROPERTIES
	[HideInInspector]public float range = 1000f;
	[HideInInspector]public float minimumWeaponsRange = 5000;
	[HideInInspector]public float criticalSignature = 0.5f;
	[HideInInspector]public float pingRate = 5;
	[HideInInspector]public float actualPingRate;
	[HideInInspector]public float pingTime;
	//
	[HideInInspector]public Collider[] visibleObjects;
	[HideInInspector]public List<GameObject> processedObjects;
	//FILTER
	[System.Serializable]
	public class FilteredObject
	{
		public string name;
		public GameObject body;
		public string form;
		public float speed;
		public float altitude;
		public float distance;
		public float heading;
		public string trackingID;
		public float ETA;
		public SilantroTransponder transponder;
	}
	//
	[HideInInspector]public List<FilteredObject> filteredObjects = new List<FilteredObject> ();
	[HideInInspector]public bool useSupportRadar;
	[HideInInspector]public SilantroRadar supportRadar;
	//SCREEN DISPLAY
	[HideInInspector]public float size = 250f;
	[HideInInspector]public float Transparency = 0.9f;
	[HideInInspector]public Texture background;
	[HideInInspector]public Texture compass;
	float scale;
	[HideInInspector]public bool rotate;
	[HideInInspector]public Color generalColor = Color.white;
	[HideInInspector]public Color labelColor = Color.green;
	//VARIABLES
	[HideInInspector]public string TrackerID;
	private string header;
	private string qualifier;
	private const string headerContainer = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const string qualifierContainer = "1234567890";
	//
	[HideInInspector]private Vector2 radarPosition;
	//
	[HideInInspector]public SilantroRadar.FilteredObject currentTarget;
	[HideInInspector]public int targetSelection;//Position of target being viewed
	string objectname,objectID,objectForm;float objectSpeed,objectAltitude,objectHeading,objectDistance;

	//LOCKED TARGET PROPERTIES
	[HideInInspector]public bool targetLocked,markTargets;
	[HideInInspector]public SilantroRadar.FilteredObject lockedTarget;
	//
	[HideInInspector]public Camera targetCamera;[HideInInspector]public Camera lockedTargetCamera;
	[HideInInspector]public Vector3 lockedPosition;
	[HideInInspector]public float cameraHeight = 30f;[HideInInspector]public float cameraDistance = 40f;
	[HideInInspector]public Texture2D selectedTargetTexture,lockedTargetTexture;
	[HideInInspector]public float objectScale = 1;
	[HideInInspector]public Camera currentCamera;
	[HideInInspector]public Texture2D TargetLockOnTexture;
	[HideInInspector]public Texture2D TargetLockedTexture;
	[HideInInspector]public GUISkin radarSkin;
	GUIStyle labelStyle = new GUIStyle();



	// --------------------------------------------TARGET SELECTION--------------------------------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------------------------------------
	public void SelectedUpperTarget(){targetSelection++;}//SELECT TARGET ABOVE CURRENT TARGET
	public void SelectLowerTarget(){targetSelection--;}//SELECT TAREGT BELOW CURRENT TARGET
	public void SelectTargetAtPosition(int position){targetSelection = position;}//SELECT TARGET AT A PARTICULAR POSITION




	//-------------------------------------LOCK ONTO A TARGET--------------------------------------------------------------------------------
	public void LockSelectedTarget()
	{
		//SET TARGET PROPERTIES
		if (radarType == RadarType.Military && allOk) {
			lockedTarget = currentTarget;
			targetLocked = true;
			lockedTarget.transponder.isLockedOn = true;
		}
	}
	//RELEASE TARGET LOCK
	public void ReleaseLockedTarget()
	{
		if (radarType == RadarType.Military && allOk) {
			lockedTarget.transponder.isLockedOn = false;
			targetLocked = false;
			lockedTarget = null;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (connectedAircraft != null && transponder != null) {
			allOk = true;
		} else if (transponder == null) {
			Debug.LogError("Prerequisites not met on Radar "+transform.name + "....Aircraft transponder not assigned");
			allOk = false;
		}
		else if (connectedAircraft == null) {
			Debug.LogError("Prerequisites not met on Radar "+transform.name + "....Aircraft controller not assigned");
			allOk = false;
		}
	}






	//-------------------------------------------------------------------------//SETUP RADAR---------------------------------------------------------------------------------------------
	public void InitializeRadar()
	{
		transponder = GetComponent<SilantroTransponder> ();


		_checkPrerequisites ();


		if(allOk){
		radarPosition = Vector2.zero;
		if (pingRate < 0.1f) {
			pingRate = 1f;
		}
		actualPingRate = 1f / pingRate;pingTime = 0f;
		//
		radarCore= new GameObject("Radar Core");
		radarCore.transform.parent = this.transform;
		radarCore.transform.localPosition = Vector3.zero;
		//GENERATE ID
		TrackerID = GenerateTrackID();
		//SEND DATA TO AIRCRAFT
		if (connectedAircraft != null) {
			if (transponder != null) {transponder.TrackingID = TrackerID;} 
			else {Debug.Log ("Please attach a transponder to radar gameObject");}
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//GENERATE TRACKING ID
	public string GenerateTrackID()
	{
		header = string.Empty;qualifier = string.Empty;
		for (int i = 0; i < 3; i++) {header += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length)];}
		for (int j = 0; j < 2; j++){qualifier += "1234567890"[UnityEngine.Random.Range(0, "1234567890".Length)];}
		return header + qualifier;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//PING
	private void Ping()
	{
		pingTime = 0f;
		//SEARCH
		visibleObjects = Physics.OverlapSphere(transform.position, range);
		processedObjects = new List<GameObject> ();filteredObjects = new List<FilteredObject> ();
		//FILTER
		Collider[] filterPool = visibleObjects;
		//
		for (int i = 0; i < filterPool.Length; i++) {
			Collider filterCollider = filterPool [i];
			//AVOID SELF DETECTION
			if (!filterCollider.transform.IsChildOf(connection.transform)) {
				//SEPARATE OBJECTS
				SilantroTransponder transponder;
				//CHECK PARENT BODY
				transponder = filterCollider.gameObject.GetComponent<SilantroTransponder>();
				if (transponder == null) {
					//CHECK IN CHILDREN
					transponder = filterCollider.gameObject.GetComponentInChildren<SilantroTransponder> ();
				}
				//PROCESS DETECTED OBJECT
				if (transponder != null) {
					//CHECK IF WITHIN RANGE
					float objectDistance = Vector3.Distance (filterCollider.gameObject.transform.position, transform.position);
					if (transponder.radarSignature > criticalSignature && objectDistance < range) {
						//REGISTER DETECTION
						processedObjects.Add (filterCollider.gameObject);
						//SET VARIABLES
						if (!transponder.isTracked) {
							transponder.isTracked = true;
							transponder.TrackingID = TrackerID;
							transponder.AssignedID = GenerateTrackID ();
						}
						//FILTER
						Rigidbody filterbody = filterCollider.gameObject.GetComponent<Rigidbody>();
						//COLLECT VARIABLES
						if (filterbody != null) {
							objectSpeed = filterbody.velocity.magnitude;
						}
						objectname = filterCollider.transform.name;
						objectForm = transponder.silantroTag.ToString();
						if (transponder.AssignedID != null) {objectID = transponder.AssignedID;} else {objectID = "Undefined";}
						objectAltitude = filterCollider.gameObject.transform.position.y;
						objectHeading = filterCollider.gameObject.transform.eulerAngles.y;
						objectDistance = Vector3.Distance (filterCollider.gameObject.transform.position, this.transform.position);
						//STORE VARIABLES
						FilteredObject currentObject = new FilteredObject ();
						currentObject.body = filterCollider.gameObject;
						currentObject.name = objectname;
						currentObject.altitude = objectAltitude;
						currentObject.form = objectForm;
						currentObject.speed = objectSpeed;
						currentObject.trackingID = objectID;
						currentObject.heading = objectHeading;
						currentObject.distance = objectDistance;
						currentObject.transponder = transponder;
						if (objectSpeed > 1) {currentObject.ETA = objectDistance / objectSpeed;}
						//ADD TO BUCKET
						filteredObjects.Add(currentObject);
					}
				}
			}
		}
		//REFRESH LOCKED TARGET
		if (lockedTarget != null && targetLocked) {
			//FILTER
			Rigidbody filterbody = lockedTarget.body.gameObject.GetComponent<Rigidbody>();
			//COLLECT VARIABLES
			if (filterbody != null) {
				lockedTarget.speed = filterbody.velocity.magnitude;
			}
			lockedTarget.altitude = lockedTarget.body.transform.position.y;
			lockedTarget.distance = Vector3.Distance (lockedTarget.body.transform.position, this.transform.position);
			lockedPosition = lockedTarget.body.transform.position;
			//
			//UNLOCK TARGET IF OUT OF RANGE
			if (lockedTarget.distance > range) {
				ReleaseLockedTarget ();
			}
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAW EXTENTS
	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Handles.color = new Color(1,0,0,0.1f);
		if (radarCore != null && displayBounds) {
			Handles.DrawSolidArc (radarCore.transform.position, radarCore.transform.up, -radarCore.transform.right, 120f, range);//Horizontal View
			Handles.DrawSolidArc (radarCore.transform.position, radarCore.transform.right, radarCore.transform.forward, 60f, range);//VERTICAL VIEW TOP
			Handles.DrawSolidArc (radarCore.transform.position, -radarCore.transform.right, radarCore.transform.forward, 60f, range);//VERTICAL VIEW BOTTOM
		}
		//

		if (markTargets) {
			//DRAW LINE TO TARGETS
			Handles.color = Color.white;
			foreach (SilantroRadar.FilteredObject filteredObject in filteredObjects) {
				if (filteredObject != null &&  filteredObject != lockedTarget && filteredObject != currentTarget && filteredObject.body != null) {
					Handles.DrawLine (filteredObject.body.transform.position, this.transform.position);
				}
			}
			//DRAW LINE TO CURRENT TARGET
			if (currentTarget != null && currentTarget.body != null) {
				Handles.color = Color.green;
				Handles.DrawLine (currentTarget.body.transform.position, this.transform.position);
			}
			//DRAW LINE TO LOCKED TARGET
			if (lockedTarget != null && lockedTarget.body != null) {
				Handles.color = Color.red;
				Handles.DrawLine (lockedTarget.body.transform.position, this.transform.position);
			}
		}
	}
	#endif






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//REFRESH
	void Update()
	{
		if (isControllable && allOk) {
			//SPIN CORE
			radarCore.transform.Rotate (new Vector3 (0f, this.pingRate * 100f * Time.deltaTime, 0f));
			scale = range / 100f;if (connectedAircraft != null && connectedAircraft.view != null) {
				currentCamera = connectedAircraft.view.currentCamera;
			}
			//SEND PING 
			pingTime += Time.deltaTime;
			if (pingTime >= actualPingRate) {Ping ();}

			//SEND MILITARY RADAR DATA
			if (radarType == RadarType.Military) {
				CombatRadarSystem ();
			}
			//POSITION TARGET CAMERA
			PositionCamera();
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PositionCamera()
	{
		//TARGET CAMERA
		if (filteredObjects.Count > 0 && currentTarget != null && currentTarget.body != null) {
			float x = currentTarget.body.transform.position.x;
			float y = currentTarget.body.transform.position.y + cameraHeight;
			float z = currentTarget.body.transform.position.z + cameraDistance;
			Vector3 cameraPosition = new Vector3 (x, y, z);
			//
			if (targetCamera != null) {
				targetCamera.transform.position = cameraPosition;
				targetCamera.transform.LookAt (currentTarget.body.transform.position);
			}
		} else {
			if (targetCamera != null) {
				targetCamera.transform.position = connectedAircraft.view.actualCamera.transform.position;
				targetCamera.transform.rotation = connectedAircraft.view.actualCamera.transform.rotation;
			}
		}
		//
		float xy = lockedPosition.x;
		float yy = lockedPosition.y + cameraHeight;
		float zy = lockedPosition.z + cameraDistance;
		Vector3 lockedCameraPosition = new Vector3 (xy, yy, zy);
		//
		if (lockedTargetCamera != null) {
			lockedTargetCamera.transform.position = lockedCameraPosition;
			if (lockedTarget != null && lockedTarget.body != null) {
				lockedTargetCamera.transform.LookAt (lockedTarget.body.transform.position);
			}
		}
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CombatRadarSystem()
	{
		//SELECT CLOSEST TARGET
		if (lockedTarget == null && currentTarget == null) {targetSelection++;}
		//CLAMP TARGET COUNT SELECTION
		if (targetSelection < 0) {targetSelection = filteredObjects.Count - 1;}
		if (targetSelection > filteredObjects.Count - 1) {targetSelection = 0;}
		//SELECT CURRENT TARGET
		if (filteredObjects.Count > 0 && filteredObjects != null) {
			currentTarget = filteredObjects [targetSelection];
			if (currentTarget == null) {
				filteredObjects.Remove (currentTarget);
			}
		}

		//RELEASE TARGET IF DESTROYED
		if (targetLocked && lockedTarget.transponder == null) {
			ReleaseLockedTarget ();
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//GET 2D POSITION
	private Vector2 GetPosition(Vector3 position)
	{
		Vector2 cronus = Vector2.zero;
		if (connection)
		{
			cronus.x = radarPosition.x + (position.x - transform.position.x + size * scale/2f) /scale;
			cronus.y = radarPosition.y + (-(position.z - transform.position.z) + size * scale/2f) /scale;
		}
		return cronus;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnGUI()
	{
		if (!isControllable) {
			return;
		}
		//SET SKIN
		if (radarSkin != null) {
			GUI.skin = radarSkin;
		}
		//DRAW LOCKED TARGET
		if (lockedTarget != null && lockedTarget.body != null) {
			//COLLECT DATA
			Vector2 currentposition = GetPosition (lockedTarget.body.transform.position);
			//DRAW CAMERA LOCK INDICATOR
			if (currentCamera) {
				Vector3 dir = (lockedTarget.body.transform.position - currentCamera.transform.position).normalized;
				float direction = Vector3.Dot (dir, currentCamera.transform.forward);
				if (direction > 0.5f) {
					Vector3 screenPos = currentCamera.WorldToScreenPoint (lockedTarget.body.transform.position);
					if (TargetLockedTexture)
						GUI.DrawTexture (new Rect (screenPos.x - TargetLockedTexture.width / 2, Screen.height - screenPos.y - TargetLockedTexture.height / 2, TargetLockedTexture.width, TargetLockedTexture.height), TargetLockedTexture);
					//DISPLAY TARGET PROPERTIES
					GUI.Label (new Rect (screenPos.x + 40, Screen.height - screenPos.y-20, 250, 50), lockedTarget.name);
					GUI.Label (new Rect (screenPos.x + 40, Screen.height - screenPos.y, 200, 50), lockedTarget.trackingID);
					GUI.Label (new Rect (screenPos.x + 40, Screen.height - screenPos.y+20, 200, 50), lockedTarget.speed.ToString("0.0") + " knts");
					GUI.Label (new Rect (screenPos.x + 40, Screen.height - screenPos.y+40, 200, 50),lockedTarget.altitude.ToString("0.0") + " ft");
					GUI.Label (new Rect (screenPos.x + 40, Screen.height - screenPos.y+60, 200, 50), lockedTarget.distance.ToString("0.0") +" m");
					GUI.Label (new Rect (screenPos.x + 40, Screen.height - screenPos.y+80, 200, 50), lockedTarget.ETA.ToString("0.0") + " s");
				}
			}
		}


		//DRAW NORMAL RADAR
		GUI.color = new Color(generalColor.r, generalColor.g,generalColor.b,Transparency);
		if (rotate) {
			//ROTATE RADAR
			GUIUtility.RotateAroundPivot (-base.transform.eulerAngles.y, radarPosition + new Vector2 (size / 2f, size / 2f));
		}


		//DRAW IDENTIFIED OBJECTS
		foreach (SilantroRadar.FilteredObject selectedObject in filteredObjects) {
			if (selectedObject != null && selectedObject.body != null && selectedObject.transponder != null) {
				//COLLECT TARGET DATA
				Vector2 position = GetPosition (selectedObject.body.transform.position);
				Texture2D radarTexture = selectedObject.transponder.silantroTexture;
				string targetID = selectedObject.transponder.AssignedID;
				float superScale = objectScale / selectedObject.transponder.radarSignature;
				//DRAW ON SCREEN
				if (radarTexture != null) {
					GUI.DrawTexture (new Rect (position.x - (float)radarTexture.width / superScale / 2f, position.y + (float)radarTexture.height / superScale / 3f, (float)radarTexture.width / superScale,
						(float)radarTexture.height / superScale), radarTexture);
				}
				//CHOOSE LABEL COLOR
				labelStyle.normal.textColor = new Color (labelColor.r, labelColor.g, labelColor.b, Transparency);
				//DRAW LABEL
				GUI.Label (new Rect (position.x - (float)radarTexture.width / objectScale / 2f, position.y - (float)radarTexture.height / superScale / 2f, 50f/superScale, 40f/superScale), targetID, labelStyle);
				//DRAW CAMERA INDICATOR
				if (currentCamera) {
					Vector3 dir = (selectedObject.body.transform.position - currentCamera.transform.position).normalized;
					float direction = Vector3.Dot (dir, currentCamera.transform.forward);
					if (direction > 0.5f) {
						Vector3 screenPos = currentCamera.WorldToScreenPoint (selectedObject.body.transform.position);
						float distance = Vector3.Distance (transform.position, selectedObject.body.transform.position);
						if (TargetLockOnTexture)
							GUI.DrawTexture (new Rect (screenPos.x - TargetLockOnTexture.width / 2, Screen.height - screenPos.y - TargetLockOnTexture.height / 2, TargetLockOnTexture.width, TargetLockOnTexture.height), TargetLockOnTexture);
					}
				}
			}
		}


		//DRAW LOCKED TARGET
		if (filteredObjects.Count >0 && lockedTarget != null && lockedTarget.body != null) {
			//COLLECT DATA
			Vector2 currentposition = GetPosition (lockedTarget.body.transform.position);
			GUI.DrawTexture (new Rect (currentposition.x - (float)lockedTargetTexture.width / 2.5f / 2f, currentposition.y + (float)lockedTargetTexture.height / 2.5f / 3f,
				(float)lockedTargetTexture.width / 2.5f, (float)lockedTargetTexture.height / 2.5f), lockedTargetTexture);
		}


		//DRAW SELECTED TARGET
		if (filteredObjects.Count >0 && currentTarget != null && currentTarget.body != null) {
			//COLLECT DATA
			Vector2 currentposition = GetPosition (currentTarget.body.transform.position);
			GUI.DrawTexture (new Rect (currentposition.x - (float)selectedTargetTexture.width / 2.5f / 2f, currentposition.y + (float)selectedTargetTexture.height / 2.5f / 3f,
			(float)selectedTargetTexture.width / 2.5f, (float)selectedTargetTexture.height / 2.5f),selectedTargetTexture);
		}


		//DRAW BACKGROUND
		if (background){GUI.DrawTexture(new Rect(radarPosition.x,radarPosition.y, size, size), background);}
		GUIUtility.RotateAroundPivot(base.transform.eulerAngles.y, radarPosition + new Vector2(size / 2f, size / 2f));
		//DRAW RADAR COMPASS
		if (compass){GUI.DrawTexture(new Rect(radarPosition.x + size / 2f - (float)compass.width / 2f, radarPosition.y + size / 2f - (float)compass.height / 2f,
		(float)compass.width, (float)compass.height), compass);}
	}
}







#if UNITY_EDITOR
[CustomEditor(typeof(SilantroRadar))]
public class RadarEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroRadar radar;
	SerializedObject radarObject;
	//
	void OnEnable()
	{
		radar = (SilantroRadar)target;
		radarObject = new SerializedObject (radar);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		radarObject.Update();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Functionality", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		radar.radarType = (SilantroRadar.RadarType)EditorGUILayout.EnumPopup("Type",radar.radarType);
		GUILayout.Space (3f);
		radar.useSupportRadar = EditorGUILayout.Toggle ("Support Radar", radar.useSupportRadar);
		if(radar.useSupportRadar){
			GUILayout.Space (5f);
			radar.supportRadar = EditorGUILayout.ObjectField (" ", radar.supportRadar, typeof(SilantroRadar), true) as SilantroRadar;
			//
		}
		//
		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Radar Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		radar.range = EditorGUILayout.FloatField ("Effective Range", radar.range);
		GUILayout.Space (3f);
		radar.minimumWeaponsRange = EditorGUILayout.FloatField ("Weapons Range", radar.minimumWeaponsRange);
		GUILayout.Space (3f);
		radar.criticalSignature = EditorGUILayout.FloatField ("Critical RCS", radar.criticalSignature);
		GUILayout.Space (5f);
		radar.pingRate = EditorGUILayout.FloatField ("Ping Rate", radar.pingRate);
		GUILayout.Space (5f);
		EditorGUILayout.LabelField ("Last Ping",(radar.pingTime).ToString ("0.0") + " s");
		GUILayout.Space (3f);
		radar.displayBounds = EditorGUILayout.Toggle ("Display Extents", radar.displayBounds);
		//
		GUILayout.Space (10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Object Identification", MessageType.None);
		GUI.color = backgroundColor;
		if (radar.visibleObjects != null) {
			GUILayout.Space (5f);
			EditorGUILayout.LabelField ("Visible Objects", radar.visibleObjects.Length.ToString ());
		}
		if (radar.filteredObjects != null) {
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Filtered Objects", radar.filteredObjects.Count.ToString ());
		}
		if (radar.radarType == SilantroRadar.RadarType.Military) {
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Selected Target", radar.currentTarget.name);
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Locked Target", radar.lockedTarget.name);
		}
		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Display Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		radar.size = EditorGUILayout.FloatField ("Radar Size", radar.size);
		GUILayout.Space (3f);
		radar.objectScale = EditorGUILayout.FloatField ("Object Scale", radar.objectScale);
		GUILayout.Space (5f);
		radar.radarSkin = EditorGUILayout.ObjectField ("GUI Skin", radar.radarSkin, typeof(GUISkin), true) as GUISkin;
		GUILayout.Space (5f);
		radar.rotate = EditorGUILayout.Toggle ("Rotate", radar.rotate);
		GUILayout.Space (5f);
		radar.markTargets = EditorGUILayout.Toggle ("Mark Objects", radar.markTargets);
		//
		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Texture Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		radar.background = EditorGUILayout.ObjectField ("Radar Background", radar.background, typeof(Texture), true) as Texture;
		GUILayout.Space (5f);
		radar.compass = EditorGUILayout.ObjectField ("Compass", radar.compass, typeof(Texture), true) as Texture;
		if (radar.radarType == SilantroRadar.RadarType.Military) {
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Radar Screen Icons", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			radar.selectedTargetTexture = EditorGUILayout.ObjectField ("Selected Target", radar.selectedTargetTexture, typeof(Texture2D), true) as Texture2D;
			GUILayout.Space (5f);
			radar.lockedTargetTexture = EditorGUILayout.ObjectField ("Locked Target", radar.lockedTargetTexture, typeof(Texture2D), true) as Texture2D;
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Camera Screen Icons", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			radar.TargetLockOnTexture = EditorGUILayout.ObjectField ("Selected Target", radar.TargetLockOnTexture, typeof(Texture2D), true) as Texture2D;
			GUILayout.Space (5f);
			radar.TargetLockedTexture = EditorGUILayout.ObjectField ("Locked Target", radar.TargetLockedTexture, typeof(Texture2D), true) as Texture2D;
		}
		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Color Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		radar.Transparency = EditorGUILayout.Slider ("Transparency", radar.Transparency, 0f, 1f);
		GUILayout.Space (3f);
		radar.generalColor = EditorGUILayout.ColorField ("General Color", radar.generalColor);
		GUILayout.Space (3f);
		radar.labelColor = EditorGUILayout.ColorField ("Label Color", radar.labelColor);

		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Camera Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		radar.targetCamera = EditorGUILayout.ObjectField ("Radar Camera", radar.targetCamera, typeof(Camera), true) as Camera;
		if (radar.radarType == SilantroRadar.RadarType.Military) {
			GUILayout.Space (3f);
			radar.lockedTargetCamera = EditorGUILayout.ObjectField ("Locked Camera", radar.lockedTargetCamera, typeof(Camera), true) as Camera;
		}
		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("View Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		radar.cameraHeight = EditorGUILayout.FloatField ("Camera Height", radar.cameraHeight);
		GUILayout.Space (3f);
		radar.cameraDistance = EditorGUILayout.FloatField ("Camera Distance", radar.cameraDistance);
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (radarObject.targetObject, "Radar Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (radar);
			EditorSceneManager.MarkSceneDirty (radar.gameObject.scene);
		}
		radarObject.ApplyModifiedProperties();
	}
}
#endif