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
public class SilantroGun : MonoBehaviour {
	//TYPE
	public enum WeaponType
	{
		Machine,Cannon
	}
	[HideInInspector]public WeaponType weaponType = WeaponType.Machine;
	//AMMUNITION TYPE
	public enum BulletType
	{
		Raycast,
		Rigidbody
	}
	[HideInInspector]public BulletType bulletType;
	//PERFORMANCE
	[HideInInspector]public float muzzleVelocity = 500;
	[HideInInspector]public float barrelLength = 2f;
	[HideInInspector]public float gunWeight;
	[HideInInspector]public float drumWeight;
	[HideInInspector]public float damage;
	//
	[HideInInspector]public float projectileForce;
	[HideInInspector]public float damperStrength = 60f;
	//
	[HideInInspector]public GameObject ammunition;GameObject currentBullet;
	[HideInInspector]public int ammoCapacity = 1000;
	[HideInInspector]public int currentAmmo;
	[HideInInspector]public bool unlimitedAmmo;
	[HideInInspector]public bool advancedSettings;
	//MUZZLES
	[HideInInspector]public Transform[] muzzles;
	private int muzzle = 0;
	[HideInInspector]private Transform currentMuzzle;

	//FIRE RATE
	[HideInInspector]public float rateOfFire = 500;
	[HideInInspector]public float actualRate;
	[HideInInspector]public float fireTimer;
	//ACCURACY
	[HideInInspector]public float accuracy = 80f;
	[HideInInspector]public float currentAccuracy;
	[HideInInspector]public float accuracyDrop = 0.2f;
	[HideInInspector]public float accuracyRecover = 0.5f;
	float acc;
	//RANGE
	[HideInInspector]public float range = 1000f;
	[HideInInspector]public float rangeRatio = 1f;

	//BARREL
	[HideInInspector]public Transform barrel;
	[HideInInspector]private float barrelRPM;
	[HideInInspector]public float currentRPM;
	public enum RotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	public enum RotationDirection
	{
		CW,
		CCW
	}
	[HideInInspector]public RotationDirection rotationDirection = RotationDirection.CCW;

	//CASE
	[HideInInspector]public GameObject bulletCase;
	[HideInInspector]private float shellSpitForce = 1.5f;					
	[HideInInspector]private float shellForceRandom = 1.5f;
	[HideInInspector]private float shellSpitTorqueX = 0.5f;
	[HideInInspector]private float shellSpitTorqueY = 0.5f;
	[HideInInspector]private float shellTorqueRandom = 1.0f;
	[HideInInspector]public bool ejectShells = false;
	[HideInInspector]public Transform shellEjectPoint;

	//EFFECTS
	[HideInInspector]public GameObject muzzleFlash;
	[HideInInspector]public GameObject groundHit;
	[HideInInspector]public GameObject metalHit;
	[HideInInspector]public GameObject woodHit;//ADD MORE
	//CONTROL BOOLS
	bool canFire = true;
	[HideInInspector]public bool running;
	//AIRCRAFT
	[HideInInspector]public Rigidbody connectedAircraft;
	[HideInInspector]public Vector3 baseVelocity;

	//SOUND
	[HideInInspector]public AudioClip fireSound;
	[HideInInspector]public float soundVolume = 0.75f;
	[HideInInspector]public AudioSource gunSource;
	float bulletMass;




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//FIRE FUNCTION
	public void FireGun()
	{
		if (canFire) {
			if ((fireTimer > (actualRate + 0.001f))) {
				Fire ();
			}
		}
		//OFFLINE
		else {
			Debug.Log ("Gun System Offline");
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeGun()
	{
		//SETUP FIRE RATE
		if (rateOfFire > 0) {
			float secFireRate = rateOfFire / 60f;//FROM RPM TO RPS
			actualRate = 1.0f / secFireRate;
		} else {
			actualRate = 0.01f;
		}
		fireTimer = 0.0f;
		//SETUP SOUND
		GameObject speaker = new GameObject("Gun Sound Point");
		speaker.transform.parent = this.transform;
		speaker.transform.localPosition = new Vector3 (0, 0, 0);
		gunSource = speaker.AddComponent<AudioSource> ();
		//SET PROPS
		gunSource.loop = false;
		gunSource.dopplerLevel = 0f;
		gunSource.spatialBlend = 1f;
		gunSource.rolloffMode = AudioRolloffMode.Custom;
		gunSource.maxDistance = 150f;
		gunSource.volume = soundVolume;
		//
		currentAmmo = ammoCapacity;
		barrelRPM = rateOfFire;
		//
		CountBullets();
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void CountBullets()
	{
		//CALCULATE BULLET DRUM WEIGHT
		if (bulletType == BulletType.Rigidbody) {
			if (ammunition != null) {
				bulletMass = ammunition.GetComponent<SilantroMunition> ().mass;
			} else {
				Debug.Log ("Gun " + transform.name + " ammunition gameobject has not been assigned");
			}
		} else {
			if (weaponType == WeaponType.Cannon) {bulletMass = 300f;} 
			else {bulletMass = 150f;}
		}
		drumWeight = currentAmmo * ((bulletMass * 0.0648f) / 1000f);
		if (currentAmmo > 0) {
			canFire = true;
		}
	}
		



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//REFRESH
	void LateUpdate()
	{
		fireTimer += Time.deltaTime;
		//CLAMP RPM
		if (currentRPM <= 0f){currentRPM = 0f;}
		//LERP ACCURACY
		currentAccuracy = Mathf.Lerp(currentAccuracy, accuracy, accuracyRecover * Time.deltaTime);
		//CLAMP AMMO
		if (currentAmmo < 0){currentAmmo = 0;}
		if (currentAmmo == 0) {canFire = false;}
		//CLAMP ROTATION
		if (currentRPM < 0){currentRPM = 0;}
		if (currentRPM > barrelRPM){currentRPM = barrelRPM;}

		//ROTATE BARREL
		if (barrel)
		{
			//ANTICLOCKWISE
			if (rotationDirection == RotationDirection.CCW) {
				if (rotationAxis == RotationAxis.X) {barrel.Rotate (new Vector3 (currentRPM * Time.deltaTime, 0, 0));}
				if (rotationAxis == RotationAxis.Y) {barrel.Rotate (new Vector3 (0, currentRPM * Time.deltaTime, 0));}
				if (rotationAxis == RotationAxis.Z) {barrel.Rotate (new Vector3 (0, 0, currentRPM * Time.deltaTime));}
			}
			//CLOCKWISE
			if (rotationDirection == RotationDirection.CW) {
				if (rotationAxis == RotationAxis.X) {barrel.Rotate (new Vector3 (-1f *currentRPM * Time.deltaTime, 0, 0));}
				if (rotationAxis == RotationAxis.Y) {barrel.Rotate (new Vector3 (0, -1f *currentRPM * Time.deltaTime, 0));}
				if (rotationAxis == RotationAxis.Z) {barrel.Rotate (new Vector3 (0, 0, -1f *currentRPM * Time.deltaTime));}
			}
		}
		//
		//REV GUN UP AND DOWN
		if (running) {
			currentRPM = Mathf.Lerp (currentRPM, barrelRPM, Time.deltaTime * 0.5f);
		} else {
			currentRPM = Mathf.Lerp (currentRPM, 0f, Time.deltaTime * 0.5f);
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTUAL FIRE
	void Fire()
	{
		//MAKE SURE THEIR IS A BARREL TO FIRE FROM
		if (muzzles.Length > 0) {
			//SELECT A MUZZLE
			muzzle += 1;
			if (muzzle > (muzzles.Length -1)) {muzzle = 0;}
			currentMuzzle = muzzles [muzzle];

			//RESET TIMER
			fireTimer = 0f;

			//GET BASE VELOCITY
			if (connectedAircraft != null) {
				baseVelocity = connectedAircraft.velocity;
			}

			//REDUCE AMMO COUNT
			if (!unlimitedAmmo) {currentAmmo--;}
			//BULLET WEIGHT
			CountBullets();

			//FIRE DIRECTION AND ACCURACY
			Vector3 direction = currentMuzzle.forward;
			Ray rayout = new Ray (currentMuzzle.position, direction);
			RaycastHit hitout;
			if (Physics.Raycast (rayout, out hitout, range / rangeRatio)) {acc = 1 - ((hitout.distance) / (range / rangeRatio));}
			//VARY ACCURACY
			float accuracyVary = (100 - currentAccuracy) / 1000;
			direction.x += UnityEngine.Random.Range (-accuracyVary, accuracyVary);
			direction.y += UnityEngine.Random.Range (-accuracyVary, accuracyVary);
			direction.z += UnityEngine.Random.Range (-accuracyVary, accuracyVary);
			currentAccuracy -= accuracyDrop;
			if (currentAccuracy <= 0.0f)currentAccuracy = 0.0f;
			//
			Quaternion muzzleRotation = Quaternion.LookRotation(direction);

			//1. FIRE RIGIDBODY AMMUNITION
			if (bulletType == BulletType.Rigidbody) {
				//SHOOT RIGIDBODY
				currentBullet = Instantiate (ammunition, currentMuzzle.position, muzzleRotation) as GameObject;
				SilantroMunition munition = currentBullet.GetComponent<SilantroMunition> ();
				if(munition != null){
					munition.InitializeMunition ();
					munition.ejectionPoint = this.transform.position;
					munition.FireBullet (muzzleVelocity, baseVelocity);
					munition.woodHit = woodHit;munition.metalHit = metalHit;munition.groundHit = groundHit;
				}
				//
			}

			//2. FIRE RAYCAST AMMUNITION
			if (bulletType == BulletType.Raycast) {
				//SETUP RAYCAST
				Ray ray = new Ray (currentMuzzle.position, direction);
				RaycastHit hit;
				//
				if (Physics.Raycast (ray, out hit, range / rangeRatio)) {
					//DAMAGE
					float damageeffect = damage * acc;
					hit.collider.gameObject.SendMessage ("SilantroDamage", -damageeffect,SendMessageOptions.DontRequireReceiver);
					//INSTANTIATE EFFECTS
					if (hit.collider.tag == "Ground") {Instantiate(groundHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));}
					//METAL
					if (hit.collider.tag == "Metal") {Instantiate(metalHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));}
					//WOOD
					if (hit.collider.tag == "Wood") {Instantiate(woodHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));}
				}
			}

			//RECOIL
			if (connectedAircraft != null) {
				//SET BULLET WEIGHT
				float bulletWeight;
				if (bulletType == BulletType.Rigidbody) {
					bulletWeight = currentBullet.GetComponent<SilantroMunition> ().mass;
				} else {
					if (weaponType == WeaponType.Cannon) {bulletWeight = 300f;} 
					else {bulletWeight = 150f;}
				}
				float ballisticEnergy = 0.5f * ((bulletWeight * 0.0648f) / 1000f) * muzzleVelocity * muzzleVelocity * UnityEngine.Random.Range(0.9f,1f);
				projectileForce = ballisticEnergy / barrelLength;
				//APPLY
				Vector3 recoilForce = connectedAircraft.transform.forward * (-projectileForce* (1-(damperStrength/100f)));
				connectedAircraft.AddForce (recoilForce, ForceMode.Impulse);
			}

			//MUZZLE FLASH
			if (muzzleFlash != null) {
				GameObject flash = Instantiate (muzzleFlash, currentMuzzle.position, currentMuzzle.rotation);
				flash.transform.position = currentMuzzle.position;flash.transform.parent = currentMuzzle.transform;
			}

			//SHELLS
			if (ejectShells) {
				GameObject shellGO = Instantiate(bulletCase, shellEjectPoint.position, shellEjectPoint.rotation) as GameObject;
				shellGO.GetComponent<Rigidbody> ().velocity = baseVelocity;
				shellGO.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(shellSpitForce + UnityEngine.Random.Range(0, shellForceRandom), 0, 0), ForceMode.Impulse);
				shellGO.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(shellSpitTorqueX + UnityEngine.Random.Range(-shellTorqueRandom, shellTorqueRandom), shellSpitTorqueY + UnityEngine.Random.Range(-shellTorqueRandom, shellTorqueRandom), 0), ForceMode.Impulse);
			}


			//SOUND
			gunSource.PlayOneShot(fireSound);
		} 
		//NO AVAILABLE MUZZLE
		else {
			Debug.Log ("Gun bullet points not setup properly");
		}
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroGun))]
public class GunEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroGun gun;
	SerializedObject gunObject;
	//
	private void OnEnable()
	{
		gun = (SilantroGun)target;
		gunObject = new SerializedObject (gun);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();serializedObject.Update();
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("System Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		gun.weaponType = (SilantroGun.WeaponType)EditorGUILayout.EnumPopup("Type",gun.weaponType);
		GUILayout.Space(5f);
		gun.bulletType = (SilantroGun.BulletType)EditorGUILayout.EnumPopup("Bullet Type",gun.bulletType);
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Ballistic Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(2f);
		gun.gunWeight = EditorGUILayout.FloatField ("Weight", gun.gunWeight);
		GUILayout.Space(2f);
		gun.barrelLength = EditorGUILayout.FloatField ("Barrel Length", gun.barrelLength);
		GUILayout.Space(2f);
		gun.muzzleVelocity = EditorGUILayout.FloatField ("Muzzle Velocity", gun.muzzleVelocity);
		GUILayout.Space(7f);
		gun.range = EditorGUILayout.FloatField ("Maximum Range", gun.range);
		GUILayout.Space(2f);
		gun.rateOfFire = EditorGUILayout.FloatField ("Rate of Fire", gun.rateOfFire);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Actual Rate", gun.actualRate.ToString ("0.00"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField("Fire Timer",gun.fireTimer.ToString ("0.00"));
		//
		GUILayout.Space(3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Recoil Effect", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField("Recoil Force",gun.projectileForce.ToString ("0.00") + " N");
		GUILayout.Space(3f);
		gun.damperStrength = EditorGUILayout.Slider ("Damper", gun.damperStrength, 0f, 100f);
		//
		//
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Ammo Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		gun.unlimitedAmmo = EditorGUILayout.Toggle ("Infinite Ammo", gun.unlimitedAmmo);
		if (!gun.unlimitedAmmo) {
			GUILayout.Space(5f);
			gun.ammoCapacity = EditorGUILayout.IntField ("Capacity", gun.ammoCapacity);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Current Ammo", gun.currentAmmo.ToString ());
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Drum Weight", gun.drumWeight.ToString () + " kg");
		}
		GUILayout.Space(3f);
		gun.ejectShells = EditorGUILayout.Toggle ("Release Shells", gun.ejectShells);
		if (gun.ejectShells) {
			GUILayout.Space(3f);
			gun.shellEjectPoint = EditorGUILayout.ObjectField ("Release Point", gun.shellEjectPoint, typeof(Transform), true) as Transform;
		}
		//
		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Accuracy Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		gun.accuracy = EditorGUILayout.FloatField ("Accuracy", gun.accuracy);
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Current Accuracy", gun.currentAccuracy.ToString ("0.00"));
		GUILayout.Space(2f);
		gun.advancedSettings = EditorGUILayout.Toggle ("Advanced Settings", gun.advancedSettings);
		if (gun.advancedSettings) {
			GUILayout.Space(3f);
			gun.accuracyDrop = EditorGUILayout.FloatField ("Drop Per Shot", gun.accuracyDrop);
			GUILayout.Space(3f);
			gun.accuracyRecover = EditorGUILayout.FloatField ("Recovery Per Shot", gun.accuracyRecover);
		}
		//
		GUILayout.Space(10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Bullet Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		if (gun.bulletType == SilantroGun.BulletType.Rigidbody) {
			GUILayout.Space (3f);
			gun.ammunition = EditorGUILayout.ObjectField ("Bullet", gun.ammunition, typeof(GameObject), true) as GameObject;
		} else {
			GUILayout.Space (3f);
			gun.damage = EditorGUILayout.FloatField ("Damage", gun.damage);
		}
		GUILayout.Space(5f);
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.BeginVertical ();
		//
		SerializedProperty muzs = this.serializedObject.FindProperty("muzzles");
		GUIContent barrelLabel = new GUIContent ("Barrel Count");
		//
		EditorGUILayout.PropertyField (muzs.FindPropertyRelative ("Array.size"),barrelLabel);
		GUILayout.Space(5f);
		for (int i = 0; i < muzs.arraySize; i++) {
			GUIContent label = new GUIContent("Barrel " +(i+1).ToString ());
			EditorGUILayout.PropertyField (muzs.GetArrayElementAtIndex (i), label);
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();
		//
		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Revolver", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		gun.barrel = EditorGUILayout.ObjectField ("Revolver", gun.barrel, typeof(Transform), true) as Transform;
		GUILayout.Space(5f);
		gun.rotationAxis = (SilantroGun.RotationAxis)EditorGUILayout.EnumPopup("Rotation Axis",gun.rotationAxis);
		GUILayout.Space(3f);
		gun.rotationDirection = (SilantroGun.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",gun.rotationDirection);
		if (gun.barrel != null) {
			GUILayout.Space(3f);
			EditorGUILayout.LabelField ("Barrel RPM", gun.currentRPM.ToString ("0.0") + " RPM");
		}
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Effects Configuration", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		gun.muzzleFlash = EditorGUILayout.ObjectField ("Muzzle Flash", gun.muzzleFlash, typeof(GameObject), true) as GameObject;
		if (gun.ejectShells) {
			GUILayout.Space(3f);
			gun.bulletCase = EditorGUILayout.ObjectField ("Bullet Case", gun.bulletCase, typeof(GameObject), true) as GameObject;
		}
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Impact Effects", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		//
		gun.groundHit = EditorGUILayout.ObjectField ("Ground Hit", gun.groundHit, typeof(GameObject), true) as GameObject;
		GUILayout.Space(3f);
		gun.metalHit = EditorGUILayout.ObjectField ("Metal Hit", gun.metalHit, typeof(GameObject), true) as GameObject;
		GUILayout.Space(3f);
		gun.woodHit = EditorGUILayout.ObjectField ("Wood Hit", gun.woodHit, typeof(GameObject), true) as GameObject;
		//
		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
		GUI.color = backgroundColor;
		//
		GUILayout.Space(3f);
		gun.fireSound = EditorGUILayout.ObjectField ("Fire Sound", gun.fireSound, typeof(AudioClip), true) as AudioClip;
		GUILayout.Space(3f);
		gun.soundVolume = EditorGUILayout.FloatField ("Sound Volume", gun.soundVolume);
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (gunObject.targetObject, "Gun Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (gun);
			EditorSceneManager.MarkSceneDirty (gun.gameObject.scene);
		}
		gunObject.ApplyModifiedProperties();
	}
}
#endif