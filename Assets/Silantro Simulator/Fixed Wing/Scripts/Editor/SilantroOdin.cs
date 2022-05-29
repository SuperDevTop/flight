﻿using UnityEngine;
using System;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroOdin : MonoBehaviour {

	public class SilantroMenu
	{
		//SETUP AEROFOILS
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Controllable/Wing",false,300)]
		private static void AddWing()
		{
			GameObject wing;
			if (Selection.activeGameObject != null )
			{
				wing = new GameObject ("Default Right Wing");
				wing.transform.parent = Selection.activeGameObject.transform;
				wing.transform.localPosition = new Vector3 (0, 0, 0);
			} else {
				wing = new GameObject ("Default Right Wing");
				GameObject parent = new GameObject ("Aerodynamics");
				wing.transform.parent = parent.transform;
			}
			EditorSceneManager.MarkSceneDirty (wing.gameObject.scene);
			SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil> ();wingAerofoil.foilSubdivisions = 5;wingAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Wing;wingAerofoil.wingPosition = SilantroAerofoil.WingPosition.Monoplane;wingAerofoil.horizontalPosition = SilantroAerofoil.HorizontalPosition.Right;
			SilantroAirfoil wng = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Wing.prefab",typeof(SilantroAirfoil));
			wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;wingAerofoil.tipAirfoil = wng;wingAerofoil.rootAirfoil = wng;
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Controllable/Rudder",false,400)]
		private static void AddRudder()
		{
			GameObject wing;
			if (Selection.activeGameObject != null )
			{
				wing = new GameObject ("Default Rudder");
				wing.transform.parent = Selection.activeGameObject.transform;wing.transform.localPosition = new Vector3 (0, 0, 0);
			} else {
				wing = new GameObject ("Default Rudder");GameObject parent = new GameObject ("Aerodynamics");wing.transform.parent = parent.transform;
			}
			//
			wing.transform.rotation = Quaternion.Euler(0,0,90);
			EditorSceneManager.MarkSceneDirty (wing.gameObject.scene);
			SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil> ();wingAerofoil.foilSubdivisions = 4;wingAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;wingAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Center;wingAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Vertical;
			GameObject start = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Control.prefab",typeof(GameObject));
			wingAerofoil.rootAirfoil = start.GetComponent<SilantroAirfoil> ();wingAerofoil.tipAirfoil = start.GetComponent<SilantroAirfoil> ();
			wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Rudder;
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Controllable/Stabilizer",false,500)]
		private static void AddTail()
		{
			GameObject wing;
			if (Selection.activeGameObject != null )
			{
				wing = new GameObject ("Default Right Stabilizer");
				wing.transform.parent = Selection.activeGameObject.transform;
				wing.transform.localPosition = new Vector3 (0, 0, 0);

			} else {
				wing = new GameObject ("Default Right Stabilizer");
				GameObject parent = new GameObject ("Aerodynamics");
				wing.transform.parent = parent.transform;
			}
			EditorSceneManager.MarkSceneDirty (wing.gameObject.scene);
			//
			SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil> ();wingAerofoil.foilSubdivisions = 4;wingAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
			SilantroAirfoil start = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Control.prefab",typeof(SilantroAirfoil));
			wingAerofoil.tipAirfoil = start;wingAerofoil.rootAirfoil = start;wingAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Right;wingAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Horizontal;
		}
		//
		//STAIONARY
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Stationary/Stabilizer",false,600)]
		private static void AddStationaryTail()
		{
			GameObject wing;
			if (Selection.activeGameObject != null )
			{
				wing = new GameObject ("Default Stationary Stabilizer");
				wing.transform.parent = Selection.activeGameObject.transform;
				wing.transform.localPosition = new Vector3 (0, 0, 0);
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
			} else {
				wing = new GameObject ("Default Stationary Stabilizer");
				GameObject parent = new GameObject ("Aerodynamics");
				wing.transform.parent = parent.transform;
			}
			//
			SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil> ();wingAerofoil.foilSubdivisions = 4;wingAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			SilantroAirfoil start = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Control.prefab",typeof(SilantroAirfoil));
			wingAerofoil.tipAirfoil = start;wingAerofoil.rootAirfoil = start;wingAerofoil.controlState = SilantroAerofoil.ControType.Stationary;wingAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Center;wingAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Horizontal;
		}
		//
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Stationary/Wing",false,700)]
		private static void AddStationaryWing()
		{
			GameObject wing;
			if (Selection.activeGameObject != null )
			{
				wing = new GameObject ("Balance Center Wing");
				wing.transform.parent = Selection.activeGameObject.transform;
				wing.transform.localPosition = new Vector3 (0, 0, 0);
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
			} else {
				wing = new GameObject ("Balance Center Wing");
				GameObject parent = new GameObject ("Aerodynamics");
				wing.transform.parent = parent.transform;
			}
			//
			SilantroAerofoil wingAerofoil = wing.AddComponent<SilantroAerofoil> ();wingAerofoil.foilSubdivisions = 4;wingAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Wing;
			SilantroAirfoil start = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Wing.prefab",typeof(SilantroAirfoil));
			wingAerofoil.tipAirfoil = start;wingAerofoil.rootAirfoil = start;wingAerofoil.controlState = SilantroAerofoil.ControType.Stationary;
			wingAerofoil.wingPosition = SilantroAerofoil.WingPosition.Monoplane;wingAerofoil.horizontalPosition = SilantroAerofoil.HorizontalPosition.Center;
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Structures/Left Structure",false,800)]
		private static void AddLeftWing()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				wing = Selection.activeGameObject;
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null) {
					SilantroMagic magic = wingAerofoil.gameObject.AddComponent<SilantroMagic> ();
					magic.currentFoil = wingAerofoil;
					magic.CreateLeftWing ();
					magic.Done ();
				} else {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				}
			} 
			else {
				Debug.Log ("Please select a wing gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Combined/Ruddervator",false,900)]
		private static void AddRuddervator()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				wing = Selection.activeGameObject;
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Ruddervator;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType != SilantroAerofoil.AerofoilType.Stabilizer) {
					Debug.Log ("Ruddervator can only be used on a Stabilzer!!!");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Combined/Flaperon",false,1000)]
		private static void AddFlaperon()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				wing = Selection.activeGameObject;
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.canUseFlap = true;wingAerofoil.flapType = SilantroAerofoil.FlapType.Flaperon;
					//
					AudioClip down = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Flaps/Flaps_Down.wav",typeof(AudioClip));
					AudioClip up = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Flaps/Flaps_Up.wav",typeof(AudioClip));
					wingAerofoil.flapDown = down;wingAerofoil.flapUp = up;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType != SilantroAerofoil.AerofoilType.Wing) {
					Debug.Log ("Flaperon can only be used on a wing!!!");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//

		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Combined/Elevon",false,1100)]
		private static void AddElevon()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				wing = Selection.activeGameObject;
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevon;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType != SilantroAerofoil.AerofoilType.Stabilizer) {
					Debug.Log ("Elevon can only be used to control the Stabilizer!!!");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//SETUP CONTROLS
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Primary/Aileron",false,1200)]
		private static void AddAileron()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				wing = Selection.activeGameObject;
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();

				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					Debug.Log ("Aileron can only be used to control the Wing!!!, Add Elevator or Rudder to the Tail");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Primary/Elevator",false,1300)]
		private static void AddElevator()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				wing = Selection.activeGameObject;
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null&& wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				}
				else if (wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					Debug.Log ("Elevator can only be used to control the Stabilizer!!!, Add Aileron or Flap to the Wing");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Primary/Rudder",false,1400)]
		private static void AddRudderControl()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				wing = Selection.activeGameObject;
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null&& wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Rudder;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				}
				else if (wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					Debug.Log ("Rudder can only be used to control the Stabilizer!!!, Add Aileron or Flap to the Wing");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Secondary/Flaps",false,1500)]
		private static void AddFlaps()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				wing = Selection.activeGameObject;
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.canUseFlap = true;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					Debug.Log ("Flap can only be used to control the Wing!!!");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Secondary/Slats",false,1600)]
		private static void AddSlats()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				wing = Selection.activeGameObject;
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.canUseSlat = true;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					Debug.Log ("Slats can only be used to control the Wing!!!");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Aerofoil System/Controls/Secondary/Spoilers",false,1700)]
		private static void AddSpoilers()
		{
			GameObject wing;
			if (Selection.activeGameObject != null)
			{
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				wing = Selection.activeGameObject;
				SilantroAerofoil wingAerofoil = wing.GetComponent<SilantroAerofoil> ();
				if (wingAerofoil != null && wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
					//
					wingAerofoil.controlState = SilantroAerofoil.ControType.Controllable;
					wingAerofoil.canUseSpoilers = true;
					//
				} else if (wingAerofoil == null) {
					Debug.Log ("Selected GameObject is not an Aerofoil! Create an Aerofoil and try again");
				} else if (wingAerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
					Debug.Log ("Spoilers can only be used to control the Wing!!!");
				}
			} else {
				Debug.Log ("Please select a foil gameObject and try again!");
			}
		}







		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/State/Activate",false,1800)]
		private static void AddWeapons()
		{
			GameObject aircraft;
			if (Selection.activeGameObject != null) {
				aircraft = Selection.activeGameObject;
				SilantroArmament armanent = aircraft.GetComponentInChildren<SilantroArmament> ();
				if (armanent == null) {
					GameObject weapons = new GameObject ("Armament System");
					GameObject pylon = new GameObject ("Pylon A");
					//
					Transform aircraftParent = aircraft.transform;
					Vector3 defaultPosition = new Vector3(0,0,0);
					weapons.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
					pylon.transform.parent = weapons.transform;pylon.transform.localPosition = defaultPosition;
					weapons.AddComponent<SilantroArmament> ();pylon.AddComponent<SilantroPylon> ();
				} else {
					Debug.Log ("Aircraft already contains a weapon system");
				}

			} else {
				Debug.Log ("Please select an aircraft gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/State/Disable",false,1900)]
		private static void DisableWeapons()
		{
			GameObject aircraft;
			if (Selection.activeGameObject != null) {
				aircraft = Selection.activeGameObject;
				SilantroArmament armanent = aircraft.GetComponentInChildren<SilantroArmament> ();
				if (armanent == null) {
					Debug.Log ("Aircraft does not contain a weapon system");
				}
				else {
					DestroyImmediate (armanent.gameObject);
				}
			}
			else {
				Debug.Log ("Please select an aircraft gameObject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/Create/Missile/ASM",false,2000)]
		private static void AddMissileASM()
		{
			GameObject missile = new GameObject("ASM Missile");
			GameObject avionics = new GameObject ("Avionics");
			GameObject motor = new GameObject ("Rocket Motor");
			GameObject model = new GameObject ("Model");
			//
			Transform missileParent = missile.transform;
			Vector3 defaultPosition = new Vector3(0,0,0);
			//
			avionics.transform.parent = missileParent;avionics.transform.localPosition = defaultPosition;
			motor.transform.parent = missileParent;motor.transform.localPosition = defaultPosition;
			model.transform.parent = missileParent;model.transform.localPosition = defaultPosition;
			//
			SilantroMunition asm = missile.AddComponent<SilantroMunition>();missile.AddComponent<Rigidbody>();missile.AddComponent<CapsuleCollider>();
			SilantroComputer computer = avionics.AddComponent<SilantroComputer> ();
			SilantroRocketMotor rocketMotor = motor.AddComponent<SilantroRocketMotor> ();
			SilantroTransponder ponder = avionics.AddComponent<SilantroTransponder> ();
			//
			asm.missileType = SilantroMunition.MissileType.ASM;asm.munitionType = SilantroMunition.MunitionType.Missile;asm.motorEngine = rocketMotor;
			computer.computerType = SilantroComputer.ComputerType.Guidance;computer.homingType = SilantroComputer.HomingType.SemiActiveRadar;
			ponder.silantroTag = SilantroTransponder.SilantroTag.Missile;
			//
			AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Rocket Core.wav",typeof(AudioClip));
			//
			GameObject effects = new GameObject("Engine Effects");
			effects.transform.parent = motor.transform;
			effects.transform.localPosition = new Vector3 (0, 0, 0);
			GameObject fire = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Burner.prefab",typeof(GameObject));
			GameObject fireEffect = GameObject.Instantiate (fire, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Glow.prefab",typeof(GameObject));
			GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			//
			rocketMotor.motorSound = start;rocketMotor.exhaustFlame = fireEffect.GetComponent<ParticleSystem>();rocketMotor.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>();
		}
		//
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/Create/Missile/AAM",false,2100)]
		private static void AddMissileAGM()
		{
			GameObject missile = new GameObject("AAM Missile");
			GameObject avionics = new GameObject ("Avionics");
			GameObject motor = new GameObject ("Rocket Motor");
			GameObject model = new GameObject ("Model");
			//
			Transform missileParent = missile.transform;
			Vector3 defaultPosition = new Vector3(0,0,0);
			//
			avionics.transform.parent = missileParent;avionics.transform.localPosition = defaultPosition;
			motor.transform.parent = missileParent;motor.transform.localPosition = defaultPosition;
			model.transform.parent = missileParent;model.transform.localPosition = defaultPosition;
			//
			SilantroMunition asm = missile.AddComponent<SilantroMunition>();missile.AddComponent<Rigidbody>();missile.AddComponent<CapsuleCollider>();
			SilantroComputer computer = avionics.AddComponent<SilantroComputer> ();
			SilantroRocketMotor rocketMotor = motor.AddComponent<SilantroRocketMotor> ();
			SilantroTransponder ponder = avionics.AddComponent<SilantroTransponder> ();
			//
			asm.missileType = SilantroMunition.MissileType.AAM;asm.munitionType = SilantroMunition.MunitionType.Missile;asm.motorEngine = rocketMotor;
			computer.computerType = SilantroComputer.ComputerType.Guidance;computer.homingType = SilantroComputer.HomingType.SemiActiveRadar;
			ponder.silantroTag = SilantroTransponder.SilantroTag.Missile;
			//
			AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Rocket Core.wav",typeof(AudioClip));
			//
			GameObject effects = new GameObject("Engine Effects");
			effects.transform.parent = motor.transform;
			effects.transform.localPosition = new Vector3 (0, 0, 0);
			GameObject fire = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Burner.prefab",typeof(GameObject));
			GameObject fireEffect = GameObject.Instantiate (fire, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Glow.prefab",typeof(GameObject));
			GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			//
			rocketMotor.motorSound = start;rocketMotor.exhaustFlame = fireEffect.GetComponent<ParticleSystem>();rocketMotor.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>();
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/Create/Rocket",false,2200)]
		private static void AddRocket()
		{
			GameObject missile = new GameObject("Base Rocket");
			GameObject avionics = new GameObject ("Avionics");
			GameObject motor = new GameObject ("Rocket Motor");
			GameObject model = new GameObject ("Model");
			//
			Transform missileParent = missile.transform;
			Vector3 defaultPosition = new Vector3(0,0,0);
			//
			avionics.transform.parent = missileParent;avionics.transform.localPosition = defaultPosition;
			motor.transform.parent = missileParent;motor.transform.localPosition = defaultPosition;
			model.transform.parent = missileParent;model.transform.localPosition = defaultPosition;
			//
			SilantroMunition asm = missile.AddComponent<SilantroMunition>();missile.AddComponent<Rigidbody>();missile.AddComponent<CapsuleCollider>();
			SilantroComputer computer = avionics.AddComponent<SilantroComputer> ();
			SilantroRocketMotor rocketMotor = motor.AddComponent<SilantroRocketMotor> ();
			//
			asm.rocketType = SilantroMunition.RocketType.Unguided;asm.munitionType = SilantroMunition.MunitionType.Rocket;asm.motorEngine = rocketMotor;
			computer.computerType = SilantroComputer.ComputerType.DataProcessing;
			//
			AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Rocket Core.wav",typeof(AudioClip));
			//
			GameObject effects = new GameObject("Engine Effects");
			effects.transform.parent = motor.transform;
			effects.transform.localPosition = new Vector3 (0, 0, 0);
			GameObject fire = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Burner.prefab",typeof(GameObject));
			GameObject fireEffect = GameObject.Instantiate (fire, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Glow.prefab",typeof(GameObject));
			GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			//
			rocketMotor.motorSound = start;rocketMotor.exhaustFlame = fireEffect.GetComponent<ParticleSystem>();rocketMotor.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>();
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/Create/Bomb",false,2300)]
		private static void AddBomb()
		{
			GameObject missile = new GameObject("Base Bomb");
			GameObject avionics = new GameObject ("Avionics");
			GameObject model = new GameObject ("Model");
			//
			Transform missileParent = missile.transform;
			Vector3 defaultPosition = new Vector3(0,0,0);
			//
			avionics.transform.parent = missileParent;avionics.transform.localPosition = defaultPosition;
			model.transform.parent = missileParent;model.transform.localPosition = defaultPosition;
			//
			SilantroMunition bomb = missile.AddComponent<SilantroMunition>();bomb.munitionType = SilantroMunition.MunitionType.Bomb;
			SilantroComputer computer = avionics.AddComponent<SilantroComputer> ();computer.computerType = SilantroComputer.ComputerType.DataProcessing;
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/Create/Gun/Cannon",false,2400)]
		private static void AddCannon()
		{
			GameObject gun = new GameObject("Base Cannon");
			EditorSceneManager.MarkSceneDirty (gun.gameObject.scene);
			GameObject shellPoint = new GameObject ("Shell Ejection Point");shellPoint.transform.parent = gun.transform;
			SilantroGun minigun = gun.AddComponent<SilantroGun> ();minigun.shellEjectPoint = shellPoint.transform;minigun.weaponType = SilantroGun.WeaponType.Cannon;
			AudioClip shoot = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Weapons/Minigun/Base Cannon.wav",typeof(AudioClip));
			minigun.fireSound = shoot;
			//
			minigun.muzzles = new Transform[1];
			GameObject muzzle = new GameObject ("Muzzle"); minigun.muzzles [0] = muzzle.transform;muzzle.transform.parent = gun.transform;
			GameObject flash = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Flash/Default Muzzle Flash.prefab",typeof(GameObject));
			GameObject impact = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Impacts/Ground Impact.prefab",typeof(GameObject));
			//
			minigun.muzzleFlash = flash;minigun.groundHit = impact;minigun.metalHit = impact;minigun.woodHit = impact;
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Armament System/Create/Gun/Machine Gun",false,2500)]
		private static void AddMG()
		{
			GameObject gun = new GameObject("Base Machine Gun");
			EditorSceneManager.MarkSceneDirty (gun.gameObject.scene);
			GameObject shellPoint = new GameObject ("Shell Ejection Point");shellPoint.transform.parent = gun.transform;
			SilantroGun minigun = gun.AddComponent<SilantroGun> ();minigun.shellEjectPoint = shellPoint.transform;minigun.weaponType = SilantroGun.WeaponType.Machine;
			AudioClip shoot = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Weapons/Minigun/Base Cannon.wav",typeof(AudioClip));
			minigun.fireSound = shoot;
			//
			minigun.muzzles = new Transform[1];
			GameObject muzzle = new GameObject ("Muzzle"); minigun.muzzles [0] = muzzle.transform;muzzle.transform.parent = gun.transform;
			GameObject flash = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Flash/Default Muzzle Flash.prefab",typeof(GameObject));
			GameObject impact = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Impacts/Ground Impact.prefab",typeof(GameObject));
			//
			minigun.muzzleFlash = flash;minigun.groundHit = impact;minigun.metalHit = impact;minigun.woodHit = impact;

		}



		[MenuItem("Oyedoyin/Fixed Wing/Avionic System/Cockpit/Dial",false,2600)]
		private static void AddDial()
		{
			GameObject panel;
			GameObject dial;
			if (Selection.activeGameObject != null && Selection.activeGameObject.name == "Avionics")
			{
				panel = new GameObject ("Control Panel");
				panel.transform.parent = Selection.activeGameObject.transform;
				//
				dial = new GameObject("Default Dial");
				dial.transform.parent = panel.transform;
				SilantroDial dialer = dial.AddComponent<SilantroDial> ();
				//
			} else {
				Debug.Log ("Please Select 'Avionics' GameObject within the Aircraft, or create one if its's missing");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Avionic System/Cockpit/Lever",false,2700)]
		private static void AddLever()
		{
			GameObject panel;
			GameObject lever;
			if (Selection.activeGameObject != null && Selection.activeGameObject.name == "Avionics")
			{
				panel = new GameObject ("Control Panel");
				panel.transform.parent = Selection.activeGameObject.transform;
				//
				lever = new GameObject("Default Lever");
				lever.transform.parent = panel.transform;
				SilantroLever dialer = lever.AddComponent<SilantroLever> ();
				//
			} else {
				Debug.Log ("Please Select 'Avionics' GameObject within the Aircraft, or create one if its's missing");
			}
		}
		//
		//
		//
		[MenuItem("Oyedoyin/Fixed Wing/Avionic System/Radar/Civillian",false,2800)]
		private static void AddCivillianRadar()
		{
			GameObject radome;
			if (Selection.activeGameObject != null && Selection.activeGameObject.name == "Avionics")
			{
				radome = new GameObject ("Radar");
				radome.transform.parent = Selection.activeGameObject.transform;
				//
				//
				SilantroRadar radar = radome.AddComponent<SilantroRadar>();
				SilantroTransponder ponder = radome.AddComponent<SilantroTransponder> ();
				SilantroController controller = radome.GetComponentInParent<SilantroController> ();
				if (controller != null) {
					radar.connectedAircraft = controller;
				}
				radar.radarType = SilantroRadar.RadarType.Civilian;
				ponder.silantroTag = SilantroTransponder.SilantroTag.Aircraft;
				//Load Textures
				Texture background = (Texture)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Radar Background.png",typeof(Texture));
				Texture compass = (Texture)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Radar Needle.png",typeof(Texture));
				Texture2D aircraft = (Texture2D)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Icons/Aircraft.png",typeof(Texture2D));
				//
				ponder.silantroTexture = aircraft;
				radar.background = background;radar.compass = compass;
				EditorSceneManager.MarkSceneDirty (radome.gameObject.scene);

			} else {
				Debug.Log ("Please Select 'Avionics' GameObject within the Aircraft, or create one if its's missing");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Avionic System/Radar/Military",false,2900)]
		private static void AddMilitaryRadar()
		{
			GameObject radome;
			if (Selection.activeGameObject != null && Selection.activeGameObject.name == "Avionics")
			{
				radome = new GameObject ("Radar");
				radome.transform.parent = Selection.activeGameObject.transform;
				//
				//
				SilantroRadar radar = radome.AddComponent<SilantroRadar>();
				SilantroTransponder ponder = radome.AddComponent<SilantroTransponder> ();
				SilantroController controller = radome.GetComponentInParent<SilantroController> ();
				if (controller != null) {
					radar.connectedAircraft = controller;
				}
				radar.radarType = SilantroRadar.RadarType.Military;
				ponder.silantroTag = SilantroTransponder.SilantroTag.Aircraft;
				//Load Textures
				Texture background = (Texture)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Radar Background.png",typeof(Texture));
				Texture compass = (Texture)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Radar Needle.png",typeof(Texture));
				Texture2D aircraft = (Texture2D)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Icons/Aircraft.png",typeof(Texture2D));
				Texture target1 = (Texture)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Radar Target.png",typeof(Texture));
				Texture target2 = (Texture)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Rotary Wing/Textures/Radar/Target Locked.png",typeof(Texture));
				//

				EditorSceneManager.MarkSceneDirty (radome.gameObject.scene);

			} else {
				Debug.Log ("Please Select 'Avionics' GameObject within the Aircraft, or create one if its's missing");
			}
		}
		//











		[MenuItem("Oyedoyin/Fixed Wing/Hydraulic System/Create Actuator",false,3000)]
		private static void AddDoorSystem()
		{
			GameObject door;door = new GameObject ("Door Hydraulics");
			//
			SilantroHydraulicSystem doorSystem = door.AddComponent<SilantroHydraulicSystem> ();
			AudioClip open = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Door/Complex/Door Open.wav",typeof(AudioClip));
			AudioClip close = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Door/Complex/Door Close.wav",typeof(AudioClip));
			doorSystem.openSound = open;doorSystem.closeSound = close;
			EditorSceneManager.MarkSceneDirty (door.gameObject.scene);
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Hydraulic System/Gear System/Combined Hydraulics",false,3100)]
		private static void AddCombinedGearSystem()
		{
			GameObject wheelSystem;
			wheelSystem = new GameObject ("Gear System");

			SilantroGearSystem gearSystem = wheelSystem.AddComponent<SilantroGearSystem> ();
			//SETUP WHEELS
			GameObject frontWheel = new GameObject("Front Wheel");frontWheel.transform.parent = wheelSystem.transform;frontWheel.transform.localPosition = new Vector3(0,-0.5f,0);frontWheel.AddComponent<WheelCollider>();frontWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject leftWheel = new GameObject ("Left Wheel");leftWheel.transform.parent = wheelSystem.transform;leftWheel.transform.localPosition = new Vector3 (-1, -0.5f, -2);leftWheel.AddComponent<WheelCollider>();leftWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject rightWheel = new GameObject ("Right Wheel");rightWheel.transform.parent = wheelSystem.transform;rightWheel.transform.localPosition = new Vector3 (1, -0.5f, -2);rightWheel.AddComponent<WheelCollider>();rightWheel.GetComponent<WheelCollider>().radius = 0.2f;
			//
			GameObject wheelBucket = new GameObject("Wheels");
			wheelBucket.transform.parent = gearSystem.transform;
			frontWheel.transform.parent = wheelBucket.transform;rightWheel.transform.parent = wheelBucket.transform;leftWheel.transform.parent = wheelBucket.transform;
			//
			SilantroGearSystem.WheelSystem frontGearSystem = new SilantroGearSystem.WheelSystem ();frontGearSystem.collider = frontWheel.GetComponent<WheelCollider>();frontGearSystem.Identifier = "Front Gear";frontGearSystem.steerable = true;
			SilantroGearSystem.WheelSystem leftGearSystem = new SilantroGearSystem.WheelSystem ();leftGearSystem.collider = leftWheel.GetComponent<WheelCollider>();leftGearSystem.Identifier = "Left Gear";leftGearSystem.attachedMotor = true;
			SilantroGearSystem.WheelSystem rightGearSystem = new SilantroGearSystem.WheelSystem ();rightGearSystem.collider = rightWheel.GetComponent<WheelCollider>();rightGearSystem.Identifier = "Right Gear";rightGearSystem.attachedMotor = true;
			//
			gearSystem.wheelSystem.Add(frontGearSystem);gearSystem.wheelSystem.Add(leftGearSystem);gearSystem.wheelSystem.Add(rightGearSystem);
			AudioClip open = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Open.wav",typeof(AudioClip));
			AudioClip close = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Close.wav",typeof(AudioClip));
			//
			//SETUP GEAR HYDRAULICS
			GameObject gearHydraulics = new GameObject ("Gear Hydraulics");gearHydraulics.transform.parent = wheelSystem.transform;
			SilantroHydraulicSystem hydraulics = gearHydraulics.AddComponent<SilantroHydraulicSystem> ();
			gearSystem.gearHydraulics = hydraulics;
			gearSystem.gearType = SilantroGearSystem.GearType.Combined;
			hydraulics.closeSound = close;hydraulics.openSound = open;
			EditorSceneManager.MarkSceneDirty (wheelSystem.gameObject.scene);
		}
		//
		//
		//
		[MenuItem("Oyedoyin/Fixed Wing/Hydraulic System/Gear System/Separate Hydraulics",false,3200)]
		private static void AddSeparateGearSystem()
		{
			GameObject wheelSystem;
			wheelSystem = new GameObject ("Gear System");

			SilantroGearSystem gearSystem = wheelSystem.AddComponent<SilantroGearSystem> ();
			EditorSceneManager.MarkSceneDirty (wheelSystem.gameObject.scene);
			//SETUP WHEELS
			GameObject frontWheel = new GameObject("Front Wheel");frontWheel.transform.parent = wheelSystem.transform;frontWheel.transform.localPosition = new Vector3(0,-0.5f,0);frontWheel.AddComponent<WheelCollider>();frontWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject leftWheel = new GameObject ("Left Wheel");leftWheel.transform.parent = wheelSystem.transform;leftWheel.transform.localPosition = new Vector3 (-1, -0.5f, -2);leftWheel.AddComponent<WheelCollider>();leftWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject rightWheel = new GameObject ("Right Wheel");rightWheel.transform.parent = wheelSystem.transform;rightWheel.transform.localPosition = new Vector3 (1, -0.5f, -2);rightWheel.AddComponent<WheelCollider>();rightWheel.GetComponent<WheelCollider>().radius = 0.2f;
			//
			GameObject wheelBucket = new GameObject("Wheels");
			wheelBucket.transform.parent = gearSystem.transform;
			frontWheel.transform.parent = wheelBucket.transform;rightWheel.transform.parent = wheelBucket.transform;leftWheel.transform.parent = wheelBucket.transform;
			//
			SilantroGearSystem.WheelSystem frontGearSystem = new SilantroGearSystem.WheelSystem ();frontGearSystem.collider = frontWheel.GetComponent<WheelCollider>();frontGearSystem.Identifier = "Front Gear";frontGearSystem.steerable = true;
			SilantroGearSystem.WheelSystem leftGearSystem = new SilantroGearSystem.WheelSystem ();leftGearSystem.collider = leftWheel.GetComponent<WheelCollider>();leftGearSystem.Identifier = "Left Gear";leftGearSystem.attachedMotor = true;
			SilantroGearSystem.WheelSystem rightGearSystem = new SilantroGearSystem.WheelSystem ();rightGearSystem.collider = rightWheel.GetComponent<WheelCollider>();rightGearSystem.Identifier = "Right Gear";rightGearSystem.attachedMotor = true;
			//
			gearSystem.wheelSystem.Add(frontGearSystem);gearSystem.wheelSystem.Add(leftGearSystem);gearSystem.wheelSystem.Add(rightGearSystem);
			AudioClip open = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Open.wav",typeof(AudioClip));
			AudioClip close = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Close.wav",typeof(AudioClip));
			//
			//SETUP GEAR HYDRAULICS
			GameObject gearHydraulics = new GameObject ("Gear Hydraulics");gearHydraulics.transform.parent = wheelSystem.transform;
			GameObject doorHydraulics = new GameObject ("Door Hydraulics");doorHydraulics.transform.parent = wheelSystem.transform;
			SilantroHydraulicSystem hydraulics = gearHydraulics.AddComponent<SilantroHydraulicSystem> ();
			SilantroHydraulicSystem doorhydraulics = doorHydraulics.AddComponent<SilantroHydraulicSystem> ();
			gearSystem.gearHydraulics = hydraulics;gearSystem.doorHydraulics = doorhydraulics;hydraulics.playSound = true;doorhydraulics.playSound = false;
			gearSystem.gearType = SilantroGearSystem.GearType.Seperate;
			hydraulics.closeSound = close;hydraulics.openSound = open;
			EditorSceneManager.MarkSceneDirty (wheelSystem.gameObject.scene);
		}
		//





		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Fuel System/Fuel Tanks/Internal",false,3300)]
		private static void AddInternalTank()
		{
			GameObject tank;
			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<SilantroFuelDistributor>())
			{
				tank = new GameObject ();
				tank.transform.parent = Selection.activeGameObject.transform;tank.transform.localPosition = new Vector3 (0, 0, 0);
				EditorSceneManager.MarkSceneDirty (tank.gameObject.scene);
				tank.name = "Internal Fuel Tank";
				SilantroFuelTank fuelTank = tank.AddComponent<SilantroFuelTank> ();fuelTank.Capacity = 1000f;fuelTank.tankType = SilantroFuelTank.TankType.Internal;
				EditorSceneManager.MarkSceneDirty (tank.gameObject.scene);
			} else {
				Debug.Log ("Please select the fuel distributor gameobject and try again!");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Fuel System/Fuel Tanks/External",false,3400)]
		private static void AddExternalTank()
		{
			GameObject tank;
			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<SilantroFuelDistributor>())
			{
				tank = new GameObject ();
				tank.transform.parent = Selection.activeGameObject.transform;tank.transform.localPosition = new Vector3 (0, 0, 0);
			} else {
				tank = new GameObject ();
			}
			tank.name = "External Fuel Tank";
			EditorSceneManager.MarkSceneDirty (tank.gameObject.scene);
			SilantroFuelTank fuelTank = tank.AddComponent<SilantroFuelTank> ();fuelTank.Capacity = 400f;fuelTank.tankType = SilantroFuelTank.TankType.External;
			EditorSceneManager.MarkSceneDirty (tank.gameObject.scene);
		}
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Fuel System/Fuel Distributor",false,3500)]
		private static void AddFuelControl()
		{
			GameObject tank = new GameObject ("Fuel Distributor");
			SilantroFuelDistributor distributor = tank.AddComponent<SilantroFuelDistributor> ();
			AudioClip alert = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Fuel Alert.wav",typeof(AudioClip));
			distributor.fuelAlert = alert;
			EditorSceneManager.MarkSceneDirty (tank.gameObject.scene);
		}

		//ENGINES
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Reaction/TurboJet Engine",false,3600)]
		private static void AddTurboJetEngine()
		{
			if (Selection.activeGameObject != null) {
				GameObject thruster = new GameObject ();
				thruster.name = "Thruster";
				thruster.transform.parent = Selection.activeGameObject.transform;
				thruster.transform.localPosition = new Vector3 (0, 0, -2);
				//
				EditorSceneManager.MarkSceneDirty (thruster.gameObject.scene);
				//
				GameObject fan = new GameObject ();
				fan.name = "Fan";
				fan.transform.parent = Selection.activeGameObject.transform;
				fan.transform.localPosition = new Vector3 (0, 0, 2);
				Selection.activeGameObject.name = "Default TurboJet Engine";
				//
				//
				GameObject effects = new GameObject("Engine Effects");
				effects.transform.parent = Selection.activeGameObject.transform;
				effects.transform.localPosition = new Vector3 (0, 0, -2);
				//
				GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
				GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				//
				Rigidbody parent = Selection.activeGameObject.transform.root.gameObject.GetComponent<Rigidbody> ();
				if (parent == null) {
					Debug.Log ("Engine is not parented to an Aircraft!! Create a Kinematic Rigidbody if you're just testing the Engine");
				}
				SilantroTurboJet jet = Selection.activeGameObject.AddComponent<SilantroTurboJet> ();
				jet.ExhaustPoint = thruster.transform;
				jet.IntakePoint = fan.transform;
				if (parent != null) {
					jet.connectedAircraft = parent;
				}//
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Jet Start.wav",typeof(AudioClip));
				AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Jet Stop.wav",typeof(AudioClip));
				AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Low Bypass Idle.wav",typeof(AudioClip));
				jet.ExteriorIdleSound = run;
				jet.ExteriorIgnitionSound = start;jet.ExteriorShutdownSound = stop;
				//
				jet.diplaySettings = true;
				jet.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>();

			} else {
				Debug.Log ("Please Select GameObject to add Engine to..");
			}
		}
		//
		//SETUP ROCKET ENGINE
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Reaction/Rocket Motor",false,3700)]
		private static void AddRocketEngine()
		{
			GameObject motor;
			if (Selection.activeGameObject != null) {
				motor = Selection.activeGameObject;
				motor.transform.name = "Default Rocket Engine";
				EditorSceneManager.MarkSceneDirty (motor.scene);
				//
				SilantroRocketMotor rocket = motor.AddComponent<SilantroRocketMotor>();
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Rocket Core.wav",typeof(AudioClip));
				//
				GameObject effects = new GameObject("Engine Effects");
				effects.transform.parent = Selection.activeGameObject.transform;
				effects.transform.localPosition = new Vector3 (0, 0, 0);
				GameObject fire = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Burner.prefab",typeof(GameObject));
				GameObject fireEffect = GameObject.Instantiate (fire, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Motor/Engine Glow.prefab",typeof(GameObject));
				GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				//
				rocket.motorSound = start;rocket.exhaustFlame = fireEffect.GetComponent<ParticleSystem>();rocket.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>();
			}
			else {
				Debug.Log ("Please Select GameObject to add Motor to..");
			}
		}
		//
		//SETUP TURBOFAN ENGINE
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Reaction/TurboFan Engine",false,3800)]
		private static void AddTurboFanEngine()
		{
			if (Selection.activeGameObject != null) {
				GameObject thruster = new GameObject ();
				thruster.name = "Thruster";
				thruster.transform.parent = Selection.activeGameObject.transform;
				thruster.transform.localPosition = new Vector3 (0, 0, -2);
				//
				EditorSceneManager.MarkSceneDirty (thruster.gameObject.scene);
				//
				GameObject fan = new GameObject ();
				fan.name = "Fan";
				fan.transform.parent = Selection.activeGameObject.transform;
				fan.transform.localPosition = new Vector3 (0, 0, 2);
				Selection.activeGameObject.name = "Default TurboFan Engine";
				//
				//
				GameObject effects = new GameObject("Engine Effects");
				effects.transform.parent = Selection.activeGameObject.transform;
				effects.transform.localPosition = new Vector3 (0, 0, -2);
				//
				GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
				GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				//
				Rigidbody parent = Selection.activeGameObject.transform.root.gameObject.GetComponent<Rigidbody> ();
				if (parent == null) {
					Debug.Log ("Engine is not parented to an Aircraft!! Create a Kinematic Rigidbody if you're just testing the Engine");
				}
				SilantroTurboFan jet = Selection.activeGameObject.AddComponent<SilantroTurboFan> ();
				jet.ExhaustPoint = thruster.transform;
				jet.IntakePoint = fan.transform;
				if (parent != null) {
					jet.connectedAircraft = parent;
				}//
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Jet Start.wav",typeof(AudioClip));
				AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Jet Stop.wav",typeof(AudioClip));
				AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/High Bypass Idle.wav",typeof(AudioClip));
				jet.ExteriorIdleSound = run;
				jet.ExteriorIgnitionSound = start;jet.ExteriorShutdownSound = stop;
				//
				jet.diplaySettings = true;
				jet.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem>();
			} 
			else {
				Debug.Log ("Please Select GameObject to add Engine to..");
			}
		}
		//
		//SETUP TURBOPROP ENGINE
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Drive/TurboProp Engine",false,3900)]
		private static void AddTurboPropEngine()
		{
			if (Selection.activeGameObject != null) {
				GameObject thruster = new GameObject ();
				thruster.name = "Thruster";
				thruster.transform.parent = Selection.activeGameObject.transform;
				thruster.transform.localPosition = new Vector3 (0, 0, -1);
				//
				EditorSceneManager.MarkSceneDirty (thruster.gameObject.scene);
				//
				Rigidbody parent = Selection.activeGameObject.transform.root.gameObject.GetComponent<Rigidbody>();
				if (parent == null) {
					Debug.Log ("Engine is not parented to an Aircraft!! Create a default Rigidbody is you're just testing the Engine");
				}
				SilantroTurboProp prop = Selection.activeGameObject.AddComponent<SilantroTurboProp> ();
				prop.ExhaustPoint = thruster.transform;
				prop.connectedAircraft = parent;	Selection.activeGameObject.name = "Default TurboProp Engine";
				//
				GameObject Props = new GameObject ("Default Propeller");
				//
				Props.transform.parent = Selection.activeGameObject.transform;
				SilantroBlade blade = Props.AddComponent<SilantroBlade> ();
				blade.engineType = SilantroBlade.EngineType.TurbopropEngine;
				blade.propEngine = prop;
				//
				GameObject effects = new GameObject("Engine Effects");
				effects.transform.parent = Selection.activeGameObject.transform;
				effects.transform.localPosition = new Vector3 (0, 0, -2);
				//
				GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
				GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				//
				prop.diplaySettings = true;
				prop.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem> ();
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Start.wav",typeof(AudioClip));
				AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Shutdown.wav",typeof(AudioClip));
				AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Propeller/Propeller Running.wav",typeof(AudioClip));
				prop.ExteriorIgnitionSound = start;prop.ExteriorShutdownSound = stop;prop.ExteriorIdleSound = run;
			} else {
				Debug.Log ("Please Select GameObject to add Engine to..");
			}
		}
		//
		//SETUP PiSTON ENGINE
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Drive/Piston Engine",false,4000)]
		private static void AddPistonEngine()
		{
			if (Selection.activeGameObject != null) {
				GameObject thruster = new GameObject ();
				thruster.name = "Thruster";
				thruster.transform.parent = Selection.activeGameObject.transform;
				thruster.transform.localPosition = new Vector3 (0, 0, -1);
				//
				EditorSceneManager.MarkSceneDirty (thruster.gameObject.scene);
				//
				Rigidbody parent = Selection.activeGameObject.transform.root.gameObject.GetComponent<Rigidbody>();
				if (parent == null) {
					Debug.Log ("Engine is not parented to an Aircraft!! Create a default Rigidbody is you're just testing the Engine");
				}
				SilantroPistonEngine prop = Selection.activeGameObject.AddComponent<SilantroPistonEngine> ();
				prop.ExhaustPoint = thruster.transform;
				prop.connectedAircraft = parent;	Selection.activeGameObject.name = "Default Piston Engine";
				//
				GameObject Props = new GameObject ("Default Propeller");
				//
				Props.transform.parent = Selection.activeGameObject.transform;
				SilantroBlade blade = Props.AddComponent<SilantroBlade> ();
				blade.engineType = SilantroBlade.EngineType.PistonEngine;
				blade.pistonEngine = prop;
				//
				GameObject effects = new GameObject("Engine Effects");
				effects.transform.parent = Selection.activeGameObject.transform;
				effects.transform.localPosition = new Vector3 (0, 0, -2);
				//
				GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
				GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				//
				prop.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem> ();
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Start.wav",typeof(AudioClip));
				AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Shutdown.wav",typeof(AudioClip));
				AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Propeller/Propeller Running.wav",typeof(AudioClip));
				prop.ExteriorIgnitionSound = start;prop.ExteriorShutdownSound = stop;prop.ExteriorIdleSound = run;
			} else {
				Debug.Log ("Please Select GameObject to add Engine to..");
			}
		}
		//
		//SETUP TURBOFAN ENGINE
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Drive/LiftFan Engine",false,4100)]
		private static void AddLiftFanEngine()
		{
			if (Selection.activeGameObject != null) {
				GameObject thruster = new GameObject ();
				thruster.name = "Thruster";
				thruster.transform.parent = Selection.activeGameObject.transform;
				thruster.transform.localPosition = new Vector3 (0, -1, 0);
				//
				EditorSceneManager.MarkSceneDirty (thruster.gameObject.scene);
				//
				GameObject fan = new GameObject ();
				fan.name= "Fan";
				fan.transform.parent = Selection.activeGameObject.transform;
				fan.transform.localPosition = new Vector3 (0, 1, 0);
				//
				Selection.activeGameObject.name = "Default LiftFan Engine";
				Rigidbody parent = Selection.activeGameObject.transform.root.gameObject.GetComponent<Rigidbody>();
				if (parent == null) {
					Debug.Log ("Engine is not parented to an Aircraft!! Create a default Rigidbody is you're just testing the Engine");
				}SilantroLiftFan liftfan =  Selection.activeGameObject.AddComponent<SilantroLiftFan> ();
				//
				liftfan.Thruster = thruster.transform;
				liftfan.fan = fan.transform;
				//
				if (parent != null) {
					liftfan.connectedAircraft = parent;
				}
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/LiftFan/Fan Start.wav",typeof(AudioClip));
				AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/LiftFan/Fan Stop.wav",typeof(AudioClip));
				AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/LiftFan/Fan Running.wav",typeof(AudioClip));
				liftfan.fanStartClip = start;liftfan.fanShutdownClip = stop;liftfan.fanRunningClip = run;
			} else {
				Debug.Log ("Please Select GameObject to add Engine to..");
			}
		}
		//SETUP TURBOFAN ENGINE
		[MenuItem("Oyedoyin/Fixed Wing/Propulsion System/Engines/Special/Pegasus Engine",false,4200)]
		private static void AddPegasusEngine()
		{
			if (Selection.activeGameObject != null) {
				GameObject thruster = new GameObject ();
				thruster.name = "Thruster";
				thruster.transform.parent = Selection.activeGameObject.transform;
				thruster.transform.localPosition = new Vector3 (0, 0, -2);
				//
				EditorSceneManager.MarkSceneDirty (thruster.gameObject.scene);
				//
				GameObject fan = new GameObject ();
				fan.name = "Fan";
				fan.transform.parent = Selection.activeGameObject.transform;
				fan.transform.localPosition = new Vector3 (0, 0, 2);
				Selection.activeGameObject.name = "Default Pegasus Engine";
				//
				//
				GameObject effects = new GameObject("Engine Effects");
				effects.transform.parent = Selection.activeGameObject.transform;
				effects.transform.localPosition = new Vector3 (0, 0, -2);
				//
				GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
				GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
				//
				Rigidbody parent = Selection.activeGameObject.transform.root.gameObject.GetComponent<Rigidbody> ();
				if (parent == null) {
					Debug.Log ("Engine is not parented to an Aircraft!! Create a Kinematic Rigidbody if you're just testing the Engine");
				}
				SilantroPegasusEngine jet = Selection.activeGameObject.AddComponent<SilantroPegasusEngine> ();
				jet.Thruster1 = thruster.transform;
				jet.Thruster2 = thruster.transform;
				jet.Thruster3 = thruster.transform;
				jet.Thruster4 = thruster.transform;
				jet.intakeFanPoint = fan.transform;
				if (parent != null) {
					jet.connectedAircraft = parent;
				}//
				AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Jet Start.wav",typeof(AudioClip));
				AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/Jet Stop.wav",typeof(AudioClip));
				AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Jet/High Bypass Idle.wav",typeof(AudioClip));
				jet.engineIdleSound = run;
				jet.ignitionSound = start;jet.shutdownSound = stop;
				//
				jet.diplaySettings = true;
				jet.exhaustSmoke1 = smokeEffect.GetComponent<ParticleSystem>();
				jet.exhaustSmoke2 = smokeEffect.GetComponent<ParticleSystem>();
				jet.exhaustSmoke3 = smokeEffect.GetComponent<ParticleSystem>();
				jet.exhaustSmoke4 = smokeEffect.GetComponent<ParticleSystem>();
			} 
			else {
				Debug.Log ("Please Select GameObject to add Engine to..");
			}
		}
		//











		[MenuItem("Oyedoyin/Fixed Wing/Components/Black Box",false,4300)]
		private static void AddBlackBox()
		{
			if (Selection.activeGameObject != null && Selection.activeGameObject.name == "Avionics") {
				GameObject box;
				box = new GameObject ("Black Box");
				box.transform.parent = Selection.activeGameObject.transform;
				SilantroDataLogger blackBox = box.AddComponent<SilantroDataLogger> ();
				//
				SilantroController controller = box.GetComponentInParent<SilantroController> ();
				if (controller != null) {
					blackBox.source = controller;
					blackBox.savefileName = controller.gameObject.name + " data";
					blackBox.logRate = 5f;
				}
				EditorSceneManager.MarkSceneDirty (blackBox.gameObject.scene);
			}
			else {
				Debug.Log ("Please Select 'Avionics' GameObject within the Aircraft, or create one if its's missing");
			}
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Components/Core",false,4400)]
		private static void COG()
		{
			GameObject box;
			if (Selection.activeGameObject != null)
			{
				box = new GameObject ();
				box.transform.parent = Selection.activeGameObject.transform;
			} else {
				box = new GameObject ();
			}
			box.name = "COG";box.AddComponent<SilantroCore> ();box.tag = "Brain";box.AddComponent<SilantroCore> ();
			box.transform.localPosition = new Vector3 (0, 0, 0);
			Rigidbody boxParent = box.transform.root.gameObject.GetComponent<Rigidbody> ();
			if (boxParent != null) {
				box.GetComponent<SilantroCore> ().aircraft = boxParent;
			} else {
				Debug.Log ("Please parent Center of Gravity to Rigidbody Airplane");
			}
			EditorSceneManager.MarkSceneDirty (box.gameObject.scene);
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Components/Controller",false,4500)]
		private static void AddSimpleController()
		{
			GameObject plane;
			if (Selection.activeGameObject != null) {
				plane = Selection.activeGameObject;
				if (plane.GetComponentInChildren<SilantroAerofoil> () && plane.GetComponentInChildren<SilantroCore> ()) {
					plane.AddComponent<SilantroController> ();
					EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				} else {
					Debug.Log ("Controller can't be added to this Object!!>..Vital Components are missing");
				}
			} else {
				Debug.Log ("Please Select an Aircraft to add Controller to!!");
			}
			//
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Components/Camera/Orbit Camera",false,4600)]
		private static void OrbitCamera()
		{
			GameObject box;
			if (Selection.activeGameObject != null)
			{
				box = new GameObject ();
				box.transform.parent = Selection.activeGameObject.transform;
			} else {
				box = new GameObject ();
			}
			box.name = "Camera System";box.transform.localPosition = new Vector3 (0, 0, 0);SilantroCamera system = box.AddComponent<SilantroCamera> ();
			GameObject focalPoint = new GameObject ("Camera Focus Point");focalPoint.transform.parent = box.transform;
			GameObject exterior = new GameObject("Exterior Camera");focalPoint.transform.localPosition = new Vector3 (0, 0, 0);exterior.gameObject.transform.parent = box.transform;
			Camera exteriorCam = exterior.AddComponent<Camera> ();exteriorCam.farClipPlane = 10000f;
			GameObject interior = new GameObject("Exterior Camera");interior.gameObject.transform.parent = box.transform;
			Camera InteriorCam = interior.AddComponent<Camera> ();exteriorCam.farClipPlane = 10000f;
			system.cameraMode = SilantroCamera.CameraMode.Orbit;system.actualCamera = exteriorCam;system.interiorcamera = InteriorCam;
			EditorSceneManager.MarkSceneDirty (box.gameObject.scene);
		}
		//
		[MenuItem("Oyedoyin/Fixed Wing/Components/Camera/Free Camera",false,4700)]
		private static void FreeCamera()
		{
			GameObject box;
			if (Selection.activeGameObject != null)
			{
				box = new GameObject ();
				box.transform.parent = Selection.activeGameObject.transform;
			} else {
				box = new GameObject ();
			}
			box.name = "Camera System";box.transform.localPosition = new Vector3 (0, 0, 0);SilantroCamera system = box.AddComponent<SilantroCamera> ();
			GameObject focalPoint = new GameObject ("Camera Focus Point");focalPoint.transform.parent = box.transform;
			GameObject exterior = new GameObject("Exterior Camera");focalPoint.transform.localPosition = new Vector3 (0, 0, 0);exterior.gameObject.transform.parent = box.transform;
			Camera exteriorCam = exterior.AddComponent<Camera> ();exteriorCam.farClipPlane = 10000f;
			GameObject interior = new GameObject("Exterior Camera");interior.gameObject.transform.parent = box.transform;
			Camera InteriorCam = interior.AddComponent<Camera> ();exteriorCam.farClipPlane = 10000f;
			system.cameraMode = SilantroCamera.CameraMode.Free;system.actualCamera = exteriorCam;system.interiorcamera = InteriorCam;
			EditorSceneManager.MarkSceneDirty (box.gameObject.scene);
		}
		//









		[MenuItem ("Oyedoyin/Fixed Wing/Miscellaneous/Create Internals",false,4800)]
		public static void Helper1 ()
		{
			GameObject aircraft;
			if (Selection.activeGameObject != null)
			{
				aircraft = Selection.activeGameObject;
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject .gameObject.scene);
				//
				GameObject core = new GameObject ("COG (Core)");
				GameObject body = new GameObject ("Body");
				GameObject surfaces = new GameObject ("Control Surfaces");
				GameObject doors = new GameObject ("Doors");
				GameObject avionics = new GameObject ("Avionics");
				GameObject aerodynamics = new GameObject("Aerodynamics");
				GameObject propulsion = new GameObject ("Propulsion System");
				GameObject gears = new GameObject ("Gear System");
				GameObject hyraulics = new GameObject ("Hydraulic System");
				GameObject cameraSystem = new GameObject ("Camera System");
				GameObject focusPoint = new GameObject ("Camera Focus Point");
				GameObject incamera = new GameObject ("Interior Camera");
				GameObject outcamera = new GameObject ("Exterior Camera");
				GameObject weapons = new GameObject ("Armament System");
				GameObject lights = new GameObject ("Lights");
				GameObject pylon = new GameObject ("Pylon A");
				//
				Transform aircraftParent = aircraft.transform;
				Vector3 defaultPosition = new Vector3(0,0,0);

				core.transform.parent = aircraftParent;aerodynamics.transform.localPosition = defaultPosition;
				body.transform.parent = aircraftParent;body.transform.localPosition = defaultPosition;
				avionics.transform.parent = aircraftParent;avionics.transform.localPosition = defaultPosition;
				aerodynamics.transform.parent = aircraftParent;aerodynamics.transform.localPosition = defaultPosition;
				propulsion.transform.parent = aircraftParent;propulsion.transform.localPosition = defaultPosition;
				gears.transform.parent = aircraftParent;gears.transform.localPosition = defaultPosition;
				hyraulics.transform.parent = aircraftParent;hyraulics.transform.localPosition = defaultPosition;
				cameraSystem.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
				weapons.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
				//
				surfaces.transform.parent = body.transform;surfaces.transform.localPosition = defaultPosition;
				doors.transform.parent = body.transform;doors.transform.localPosition = defaultPosition;
				lights.transform.parent = avionics.transform;lights.transform.localPosition = defaultPosition;
				incamera.transform.parent = cameraSystem.transform;incamera.transform.localPosition = defaultPosition;
				outcamera.transform.parent = cameraSystem.transform;outcamera.transform.localPosition = defaultPosition;
				focusPoint.transform.parent = cameraSystem.transform;focusPoint.transform.localPosition = defaultPosition;
				pylon.transform.parent = weapons.transform;pylon.transform.localPosition = defaultPosition;
				//
				//ADD CAMERAS
				Camera interior = incamera.AddComponent<Camera>();incamera.AddComponent<AudioListener>(); Camera exterior = outcamera.AddComponent<Camera>();outcamera.AddComponent<AudioListener>();
				SilantroCamera view = cameraSystem.AddComponent<SilantroCamera> ();
				view.actualCamera = exterior;view.interiorcamera = interior;view.cameraTarget = focusPoint;
				//ADD LIGHT
				lights.AddComponent<SilantroLightControl>();
				//ADD GEAR
				gears.AddComponent<SilantroGearSystem>();core.AddComponent<SilantroCore>();
				weapons.AddComponent<SilantroArmament> ();pylon.AddComponent<SilantroPylon> ();
			} else {
				Debug.Log ("Please Select Aircraft GameObject to Setup..");
			}
		}
		//
		[MenuItem ("Oyedoyin/Fixed Wing/Miscellaneous/Setup Input",false,4900)]
		public static void InitializeQuick ()
		{
			string sourcePath = Application.dataPath + "/Silantro Simulator/Fixed Wing/Data/Input/Keyboard/PhantomKeyboard.dat";
			string destPath = Application.dataPath + "/../ProjectSettings/InputManager.asset";
			string defaultPath = Application.dataPath + "/Silantro Simulator/Fixed Wing/Data/Input/Default/PhantomDefault.dat";
			//
			if (!File.Exists (sourcePath)) {
				Debug.LogError ("Source File missing....Please Reimport file");
			}
			if (!File.Exists (destPath)) {
				Debug.LogError ("Destination input manager missing");
			}
			if (File.Exists (destPath) && File.Exists (sourcePath)) {
				File.Copy (defaultPath, destPath, true);
				File.Copy (sourcePath, destPath, true);
				AssetDatabase.Refresh ();
				Debug.Log ("Keyboard Input Setup Successful!");
			}
		}
		//
		[MenuItem ("Oyedoyin/Fixed Wing/Miscellaneous/Help/Forum Page",false,5000)]
		public static void ForumPage ()
		{
			Application.OpenURL("https://forum.unity.com/threads/released-silantro-flight-simulator.522642/");
		}
		[MenuItem ("Oyedoyin/Fixed Wing/Miscellaneous/Help/Youtube Channel",false,5100)]
		public static void YoutubeChannel ()
		{
			Application.OpenURL("https://www.youtube.com/channel/UCYXrhYRzY11qokg59RFg7gQ/videos");
		}
		[MenuItem ("Oyedoyin/Fixed Wing/Miscellaneous/Help/Update Log",false,5200)]
		public static void UpdateLog ()
		{
			Application.OpenURL("https://www.dropbox.com/sh/18jjmack1biif4x/AADctV8QKzErur08p7ZDl9Lea?dl=0");
		}
		[MenuItem ("Oyedoyin/Fixed Wing/Miscellaneous/Help/Report Bug_Contact Developer",false,5300)]
		public static void Contact ()
		{
			Application.OpenURL("mailto:" + "silantrosimulator@gmail.com" + "?subject:" + "Silantro Bug" + "&body:" + " ");
		}

		[MenuItem("Oyedoyin/Fixed Wing/Miscellaneous/Foil Magic",false,5400)]
		public static void ShowBladeMagic()
		{
			GameObject foil;
			if (Selection.activeGameObject != null) {
				foil = Selection.activeGameObject;
				SilantroAerofoil bladeComponent = foil.GetComponent<SilantroAerofoil> ();
				if (bladeComponent != null) {
					SilantroMagic magic = foil.AddComponent<SilantroMagic> ();
					magic.currentFoil = bladeComponent;
				}
				else {
					Debug.Log ("Please Select an aerofoil to add Magic to..");
				}
			}
			else {
				Debug.Log ("Please Select a Foil GamaObject..");
			}
		}
		[MenuItem("Oyedoyin/Fixed Wing/Miscellaneous/Create Aircraft/Jet Powered",false,5500)]
		public static void CreateBaseJet()
		{
			GameObject plane = new GameObject ("Jet Plane");
			Undo.RegisterCreatedObjectUndo(plane,"Create " + plane.name);
			Selection.activeObject = plane;
			//
			Rigidbody aircraftBody = plane.AddComponent<Rigidbody>();
			aircraftBody.mass = 1000f;
			//SETUP PLANE
			GameObject core = new GameObject ("COG (Core)");
			GameObject body = new GameObject ("Body");
			GameObject surfaces = new GameObject ("Control Surfaces");
			GameObject doors = new GameObject ("Doors");
			GameObject avionics = new GameObject ("Avionics");
			GameObject aerodynamics = new GameObject("Aerodynamics");
			GameObject propulsion = new GameObject ("Propulsion System");
			GameObject gears = new GameObject ("Gear System");
			GameObject hyraulics = new GameObject ("Hydraulic System");
			GameObject cameraSystem = new GameObject ("Camera System");
			GameObject focusPoint = new GameObject ("Camera Focus Point");
			GameObject incamera = new GameObject ("Interior Camera");
			GameObject outcamera = new GameObject ("Exterior Camera");
			GameObject weapons = new GameObject ("Armament System");
			GameObject lights = new GameObject ("Lights");
			GameObject pylon = new GameObject ("Pylon A");
			//
			Transform aircraftParent = plane.transform;
			Vector3 defaultPosition = new Vector3(0,0,0);

			core.transform.parent = aircraftParent;aerodynamics.transform.localPosition = defaultPosition;
			body.transform.parent = aircraftParent;body.transform.localPosition = defaultPosition;
			avionics.transform.parent = aircraftParent;avionics.transform.localPosition = defaultPosition;
			aerodynamics.transform.parent = aircraftParent;aerodynamics.transform.localPosition = defaultPosition;
			propulsion.transform.parent = aircraftParent;propulsion.transform.localPosition = defaultPosition;
			gears.transform.parent = aircraftParent;gears.transform.localPosition = defaultPosition;
			hyraulics.transform.parent = aircraftParent;hyraulics.transform.localPosition = defaultPosition;
			cameraSystem.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
			weapons.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
			//
			surfaces.transform.parent = body.transform;surfaces.transform.localPosition = defaultPosition;
			doors.transform.parent = body.transform;doors.transform.localPosition = defaultPosition;
			lights.transform.parent = avionics.transform;lights.transform.localPosition = defaultPosition;
			incamera.transform.parent = cameraSystem.transform;incamera.transform.localPosition = defaultPosition;
			outcamera.transform.parent = cameraSystem.transform;outcamera.transform.localPosition = defaultPosition;
			focusPoint.transform.parent = cameraSystem.transform;focusPoint.transform.localPosition = defaultPosition;
			pylon.transform.parent = weapons.transform;pylon.transform.localPosition = defaultPosition;
			//
			//ADD CAMERAS
			Camera interior = incamera.AddComponent<Camera>();incamera.AddComponent<AudioListener>(); Camera exterior = outcamera.AddComponent<Camera>();outcamera.AddComponent<AudioListener>();
			SilantroCamera view = cameraSystem.AddComponent<SilantroCamera> ();
			view.actualCamera = exterior;view.interiorcamera = interior;view.cameraTarget = focusPoint;
			//ADD LIGHT
			lights.AddComponent<SilantroLightControl>();
			//ADD GEAR
			gears.AddComponent<SilantroGearSystem>();core.AddComponent<SilantroCore>();core.GetComponent<SilantroCore>().aircraft = aircraftBody;
			weapons.AddComponent<SilantroArmament> ();pylon.AddComponent<SilantroPylon> ();


			//
			GameObject engine = new GameObject ("TurboJet Engine");engine.transform.parent = propulsion.transform;

			//ADD NECESSARY COMPONENTS
			SilantroTurboJet propEngine = engine.AddComponent<SilantroTurboJet> ();
			GameObject thruster = new GameObject ();
			thruster.name = "Thruster";
			thruster.transform.parent = engine.transform;
			thruster.transform.localPosition = new Vector3 (0, 0, -2);
			//
			propEngine.ExhaustPoint = thruster.transform;
			propEngine.connectedAircraft = plane.GetComponent<Rigidbody>();
			//
			//
			GameObject effects = new GameObject("Engine Effects");
			effects.transform.parent = engine.transform;
			effects.transform.localPosition = new Vector3 (0, 0, -2);
			//
			GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
			GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			//
			propEngine.diplaySettings = true;
			propEngine.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem> ();
			AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Start.wav",typeof(AudioClip));
			AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Shutdown.wav",typeof(AudioClip));
			AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Propeller/Propeller Running.wav",typeof(AudioClip));
			propEngine.ExteriorIgnitionSound = start;propEngine.ExteriorShutdownSound = stop;propEngine.ExteriorIdleSound = run;
			//
			//
			GameObject fuelSystem = new GameObject ("Fuel System");fuelSystem.transform.parent = plane.transform;fuelSystem.AddComponent<SilantroFuelDistributor>();
			GameObject tank = new GameObject ("Main Tank");tank.transform.parent = fuelSystem.transform; SilantroFuelTank tnk = tank.AddComponent<SilantroFuelTank> ();tnk.Capacity = 1000f;tnk.tankType = SilantroFuelTank.TankType.Internal;
			//
			//SETUP WHEELS
			GameObject frontWheel = new GameObject("Front Wheel");frontWheel.transform.parent = gears.transform;frontWheel.transform.localPosition = new Vector3(0,-0.5f,0);frontWheel.AddComponent<WheelCollider>();frontWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject leftWheel = new GameObject ("Left Wheel");leftWheel.transform.parent = gears.transform;leftWheel.transform.localPosition = new Vector3 (-1, -0.5f, -2);leftWheel.AddComponent<WheelCollider>();leftWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject rightWheel = new GameObject ("Right Wheel");rightWheel.transform.parent = gears.transform;rightWheel.transform.localPosition = new Vector3 (1, -0.5f, -2);rightWheel.AddComponent<WheelCollider>();rightWheel.GetComponent<WheelCollider>().radius = 0.2f;
			//
			GameObject wheelBucket = new GameObject("Wheels");
			wheelBucket.transform.parent = gears.transform;
			frontWheel.transform.parent = wheelBucket.transform;rightWheel.transform.parent = wheelBucket.transform;leftWheel.transform.parent = wheelBucket.transform;
			//
			SilantroGearSystem.WheelSystem frontGearSystem = new SilantroGearSystem.WheelSystem ();frontGearSystem.collider = frontWheel.GetComponent<WheelCollider>();frontGearSystem.Identifier = "Front Gear";frontGearSystem.steerable = true;
			SilantroGearSystem.WheelSystem leftGearSystem = new SilantroGearSystem.WheelSystem ();leftGearSystem.collider = leftWheel.GetComponent<WheelCollider>();leftGearSystem.Identifier = "Left Gear";leftGearSystem.attachedMotor = true;
			SilantroGearSystem.WheelSystem rightGearSystem = new SilantroGearSystem.WheelSystem ();rightGearSystem.collider = rightWheel.GetComponent<WheelCollider>();rightGearSystem.Identifier = "Right Gear";rightGearSystem.attachedMotor = true;
			//
			SilantroGearSystem wheelSys = gears.AddComponent<SilantroGearSystem>();
			wheelSys.wheelSystem.Add(frontGearSystem);wheelSys.wheelSystem.Add(leftGearSystem);wheelSys.wheelSystem.Add(rightGearSystem);
			AudioClip open = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Open.wav",typeof(AudioClip));
			AudioClip close = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Close.wav",typeof(AudioClip));
			//
			//SETUP GEAR HYDRAULICS
			GameObject gearHydraulics = new GameObject ("Gear Hydraulics");gearHydraulics.transform.parent = gears.transform;
			SilantroHydraulicSystem hydraulics = gearHydraulics.AddComponent<SilantroHydraulicSystem> ();
			wheelSys.gearHydraulics = hydraulics;
			wheelSys.gearType = SilantroGearSystem.GearType.Combined;
			hydraulics.closeSound = close;hydraulics.openSound = open;
			//
			SilantroController control = plane.AddComponent<SilantroController> ();
			control.gearHelper = wheelSys;
			control.fuelsystem = plane.GetComponentInChildren<SilantroFuelDistributor> ();
			control.engineType = SilantroController.EngineType.TurboJet;
			//
			CapsuleCollider col =plane.AddComponent<CapsuleCollider>();
			col.height = 5f;
			col.radius = 0.5f;
			col.direction = 2;
			//
			//SETUP WINGS
			GameObject leftWing = new GameObject("Left Wing");leftWing.transform.parent = aerodynamics.transform;leftWing.transform.localPosition = new Vector3(-1,0,0);leftWing.transform.localScale = new Vector3(-1,1,1);
			GameObject rightWing = new GameObject("Right Wing");rightWing.transform.parent = aerodynamics.transform;rightWing.transform.localPosition = new Vector3(1,0,0);
			GameObject leftTail = new GameObject("Left Stabilizer");leftTail.transform.parent = aerodynamics.transform;leftTail.transform.localPosition = new Vector3(-1,0,-2);leftTail.transform.localScale = new Vector3(-1,1,1);
			GameObject rightTail = new GameObject("Right Stabilizer");rightTail.transform.parent = aerodynamics.transform;rightTail.transform.localPosition = new Vector3(1,0,-2);
			GameObject rudder = new GameObject("Rudder");rudder.transform.parent = aerodynamics.transform;rudder.transform.localPosition = new Vector3(0,0.5f,-2);rudder.transform.localRotation = Quaternion.Euler (0, 0, 90);
			//ADD WING COMPONENTS
			SilantroAerofoil rightWingAerofoil = rightWing.AddComponent<SilantroAerofoil>();rightWingAerofoil.foilSubdivisions = 5;rightWingAerofoil.horizontalPosition = SilantroAerofoil.HorizontalPosition.Right;
			rightWingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;
			//
			SilantroAerofoil leftWingAerofoil = leftWing.AddComponent<SilantroAerofoil>();leftWingAerofoil.foilSubdivisions = 5;leftWingAerofoil.horizontalPosition = SilantroAerofoil.HorizontalPosition.Left;
			leftWingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;
			//
			SilantroAerofoil leftTailAerofoil = leftTail.AddComponent<SilantroAerofoil>();leftTailAerofoil.foilSubdivisions = 3;leftTailAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			leftTailAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
			leftTailAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Left;leftTailAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Horizontal;
			//
			SilantroAerofoil rightTailAerofoil = rightTail.AddComponent<SilantroAerofoil>();rightTailAerofoil.foilSubdivisions = 3;rightTailAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			rightTailAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
			rightTailAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Right;rightTailAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Horizontal;
			//
			SilantroAerofoil rudderAerofoil = rudder.AddComponent<SilantroAerofoil>();rudderAerofoil.foilSubdivisions = 3;rudderAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			rudderAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Rudder;
			rudderAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Center;rudderAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Vertical;
			//
			SilantroAirfoil wng = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Wing.prefab",typeof(SilantroAirfoil));
			SilantroAirfoil ctl = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Control.prefab",typeof(SilantroAirfoil));
			rightTailAerofoil.rootAirfoil = ctl;rudderAerofoil.rootAirfoil = ctl;leftTailAerofoil.rootAirfoil = ctl;leftWingAerofoil.rootAirfoil = wng;rightWingAerofoil.rootAirfoil = wng;
			rightTailAerofoil.tipAirfoil = ctl;rudderAerofoil.tipAirfoil = ctl;leftTailAerofoil.tipAirfoil = ctl;leftWingAerofoil.tipAirfoil = wng;rightWingAerofoil.tipAirfoil = wng;
			//
			//
			plane.transform.position = new Vector3(0,2,0);
		}

		[MenuItem("Oyedoyin/Fixed Wing/Miscellaneous/Create Aircraft/Propeller Powered",false,5600)]
		public static void CreateBasePropeller()
		{
			GameObject plane = new GameObject ("Propeller Plane");
			Undo.RegisterCreatedObjectUndo(plane,"Create " + plane.name);
			Selection.activeObject = plane;
			//
			Rigidbody aircraftBody = plane.AddComponent<Rigidbody>();
			aircraftBody.mass = 1000f;
			//SETUP PLANE
			GameObject core = new GameObject ("COG (Core)");
			GameObject body = new GameObject ("Body");
			GameObject surfaces = new GameObject ("Control Surfaces");
			GameObject doors = new GameObject ("Doors");
			GameObject avionics = new GameObject ("Avionics");
			GameObject aerodynamics = new GameObject("Aerodynamics");
			GameObject propulsion = new GameObject ("Propulsion System");
			GameObject gears = new GameObject ("Gear System");
			GameObject hyraulics = new GameObject ("Hydraulic System");
			GameObject cameraSystem = new GameObject ("Camera System");
			GameObject focusPoint = new GameObject ("Camera Focus Point");
			GameObject incamera = new GameObject ("Interior Camera");
			GameObject outcamera = new GameObject ("Exterior Camera");
			GameObject weapons = new GameObject ("Armament System");
			GameObject lights = new GameObject ("Lights");
			GameObject pylon = new GameObject ("Pylon A");
			//
			Transform aircraftParent = plane.transform;
			Vector3 defaultPosition = new Vector3(0,0,0);

			core.transform.parent = aircraftParent;aerodynamics.transform.localPosition = defaultPosition;
			body.transform.parent = aircraftParent;body.transform.localPosition = defaultPosition;
			avionics.transform.parent = aircraftParent;avionics.transform.localPosition = defaultPosition;
			aerodynamics.transform.parent = aircraftParent;aerodynamics.transform.localPosition = defaultPosition;
			propulsion.transform.parent = aircraftParent;propulsion.transform.localPosition = defaultPosition;
			gears.transform.parent = aircraftParent;gears.transform.localPosition = defaultPosition;
			hyraulics.transform.parent = aircraftParent;hyraulics.transform.localPosition = defaultPosition;
			cameraSystem.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
			weapons.transform.parent = aircraftParent;weapons.transform.localPosition = defaultPosition;
			//
			surfaces.transform.parent = body.transform;surfaces.transform.localPosition = defaultPosition;
			doors.transform.parent = body.transform;doors.transform.localPosition = defaultPosition;
			lights.transform.parent = avionics.transform;lights.transform.localPosition = defaultPosition;
			incamera.transform.parent = cameraSystem.transform;incamera.transform.localPosition = defaultPosition;
			outcamera.transform.parent = cameraSystem.transform;outcamera.transform.localPosition = defaultPosition;
			focusPoint.transform.parent = cameraSystem.transform;focusPoint.transform.localPosition = defaultPosition;
			pylon.transform.parent = weapons.transform;pylon.transform.localPosition = defaultPosition;
			//
			//ADD CAMERAS
			Camera interior = incamera.AddComponent<Camera>();incamera.AddComponent<AudioListener>(); Camera exterior = outcamera.AddComponent<Camera>();outcamera.AddComponent<AudioListener>();
			SilantroCamera view = cameraSystem.AddComponent<SilantroCamera> ();
			view.actualCamera = exterior;view.interiorcamera = interior;view.cameraTarget = focusPoint;
			//ADD LIGHT
			lights.AddComponent<SilantroLightControl>();
			//ADD GEAR
			gears.AddComponent<SilantroGearSystem>();core.AddComponent<SilantroCore>();core.GetComponent<SilantroCore>().aircraft = aircraftBody;
			weapons.AddComponent<SilantroArmament> ();pylon.AddComponent<SilantroPylon> ();
			core.GetComponent<SilantroCore> ().aircraftType = SilantroCore.AircraftType.Propeller;

			//
			GameObject engine = new GameObject ("TurboProp Engine");engine.transform.parent = propulsion.transform;

			//ADD NECESSARY COMPONENTS
			SilantroTurboProp propEngine = engine.AddComponent<SilantroTurboProp> ();
			GameObject thruster = new GameObject ();
			thruster.name = "Thruster";
			thruster.transform.parent = engine.transform;
			thruster.transform.localPosition = new Vector3 (0, 0, -2);
			//
			propEngine.ExhaustPoint = thruster.transform;
			propEngine.connectedAircraft = plane.GetComponent<Rigidbody>();
			//
			GameObject Props = new GameObject ("Default Propeller");
			//
			Props.transform.parent = engine.transform;
			SilantroBlade blade = Props.AddComponent<SilantroBlade> ();
			blade.engineType = SilantroBlade.EngineType.TurbopropEngine;
			blade.propEngine = propEngine;
			//
			GameObject effects = new GameObject("Engine Effects");
			effects.transform.parent = engine.transform;
			effects.transform.localPosition = new Vector3 (0, 0, -2);
			//
			GameObject smoke = (GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Effects/Engine/Exhaust Smoke.prefab",typeof(GameObject));
			GameObject smokeEffect = GameObject.Instantiate (smoke, effects.transform.position, Quaternion.Euler(0,-180,0),effects.transform);
			//
			propEngine.diplaySettings = true;
			propEngine.exhaustSmoke = smokeEffect.GetComponent<ParticleSystem> ();
			AudioClip start = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Start.wav",typeof(AudioClip));
			AudioClip stop = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Default Shutdown.wav",typeof(AudioClip));
			AudioClip run = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Engines/Propeller/Propeller Running.wav",typeof(AudioClip));
			propEngine.ExteriorIgnitionSound = start;propEngine.ExteriorShutdownSound = stop;propEngine.ExteriorIdleSound = run;
			//
			//
			GameObject fuelSystem = new GameObject ("Fuel System");fuelSystem.transform.parent = plane.transform;fuelSystem.AddComponent<SilantroFuelDistributor>();
			GameObject tank = new GameObject ("Main Tank");tank.transform.parent = fuelSystem.transform; SilantroFuelTank tnk = tank.AddComponent<SilantroFuelTank> ();tnk.Capacity = 1000f;tnk.tankType = SilantroFuelTank.TankType.Internal;
			//
			//SETUP WHEELS
			GameObject frontWheel = new GameObject("Front Wheel");frontWheel.transform.parent = gears.transform;frontWheel.transform.localPosition = new Vector3(0,-0.5f,0);frontWheel.AddComponent<WheelCollider>();frontWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject leftWheel = new GameObject ("Left Wheel");leftWheel.transform.parent = gears.transform;leftWheel.transform.localPosition = new Vector3 (-1, -0.5f, -2);leftWheel.AddComponent<WheelCollider>();leftWheel.GetComponent<WheelCollider>().radius = 0.2f;
			GameObject rightWheel = new GameObject ("Right Wheel");rightWheel.transform.parent = gears.transform;rightWheel.transform.localPosition = new Vector3 (1, -0.5f, -2);rightWheel.AddComponent<WheelCollider>();rightWheel.GetComponent<WheelCollider>().radius = 0.2f;
			//
			GameObject wheelBucket = new GameObject("Wheels");
			wheelBucket.transform.parent = gears.transform;
			frontWheel.transform.parent = wheelBucket.transform;rightWheel.transform.parent = wheelBucket.transform;leftWheel.transform.parent = wheelBucket.transform;
			//
			SilantroGearSystem.WheelSystem frontGearSystem = new SilantroGearSystem.WheelSystem ();frontGearSystem.collider = frontWheel.GetComponent<WheelCollider>();frontGearSystem.Identifier = "Front Gear";frontGearSystem.steerable = true;
			SilantroGearSystem.WheelSystem leftGearSystem = new SilantroGearSystem.WheelSystem ();leftGearSystem.collider = leftWheel.GetComponent<WheelCollider>();leftGearSystem.Identifier = "Left Gear";leftGearSystem.attachedMotor = true;
			SilantroGearSystem.WheelSystem rightGearSystem = new SilantroGearSystem.WheelSystem ();rightGearSystem.collider = rightWheel.GetComponent<WheelCollider>();rightGearSystem.Identifier = "Right Gear";rightGearSystem.attachedMotor = true;
			//
			SilantroGearSystem wheelSys = gears.AddComponent<SilantroGearSystem>();
			wheelSys.wheelSystem.Add(frontGearSystem);wheelSys.wheelSystem.Add(leftGearSystem);wheelSys.wheelSystem.Add(rightGearSystem);
			AudioClip open = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Open.wav",typeof(AudioClip));
			AudioClip close = (AudioClip)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Sounds/Default/Gear/Gear Close.wav",typeof(AudioClip));
			//
			//SETUP GEAR HYDRAULICS
			GameObject gearHydraulics = new GameObject ("Gear Hydraulics");gearHydraulics.transform.parent = gears.transform;
			SilantroHydraulicSystem hydraulics = gearHydraulics.AddComponent<SilantroHydraulicSystem> ();
			wheelSys.gearHydraulics = hydraulics;
			wheelSys.gearType = SilantroGearSystem.GearType.Combined;
			hydraulics.closeSound = close;hydraulics.openSound = open;
			//
			SilantroController control = plane.AddComponent<SilantroController> ();
			control.gearHelper = wheelSys;
			control.fuelsystem = plane.GetComponentInChildren<SilantroFuelDistributor> ();
			control.engineType = SilantroController.EngineType.TurboProp;
			//
			CapsuleCollider col =plane.AddComponent<CapsuleCollider>();
			col.height = 5f;
			col.radius = 0.5f;
			col.direction = 2;
			//
			//SETUP WINGS
			GameObject leftWing = new GameObject("Left Wing");leftWing.transform.parent = aerodynamics.transform;leftWing.transform.localPosition = new Vector3(-1,0,0);leftWing.transform.localScale = new Vector3(-1,1,1);
			GameObject rightWing = new GameObject("Right Wing");rightWing.transform.parent = aerodynamics.transform;rightWing.transform.localPosition = new Vector3(1,0,0);
			GameObject leftTail = new GameObject("Left Stabilizer");leftTail.transform.parent = aerodynamics.transform;leftTail.transform.localPosition = new Vector3(-1,0,-2);leftTail.transform.localScale = new Vector3(-1,1,1);
			GameObject rightTail = new GameObject("Right Stabilizer");rightTail.transform.parent = aerodynamics.transform;rightTail.transform.localPosition = new Vector3(1,0,-2);
			GameObject rudder = new GameObject("Rudder");rudder.transform.parent = aerodynamics.transform;rudder.transform.localPosition = new Vector3(0,0.5f,-2);rudder.transform.localRotation = Quaternion.Euler (0, 0, 90);
			//ADD WING COMPONENTS
			SilantroAerofoil rightWingAerofoil = rightWing.AddComponent<SilantroAerofoil>();rightWingAerofoil.foilSubdivisions = 5;rightWingAerofoil.horizontalPosition = SilantroAerofoil.HorizontalPosition.Right;
			rightWingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;
			//
			SilantroAerofoil leftWingAerofoil = leftWing.AddComponent<SilantroAerofoil>();leftWingAerofoil.foilSubdivisions = 5;leftWingAerofoil.horizontalPosition = SilantroAerofoil.HorizontalPosition.Left;
			leftWingAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Aileron;
			//
			SilantroAerofoil leftTailAerofoil = leftTail.AddComponent<SilantroAerofoil>();leftTailAerofoil.foilSubdivisions = 3;leftTailAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			leftTailAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
			leftTailAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Left;leftTailAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Horizontal;
			//
			SilantroAerofoil rightTailAerofoil = rightTail.AddComponent<SilantroAerofoil>();rightTailAerofoil.foilSubdivisions = 3;rightTailAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			rightTailAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Elevator;
			rightTailAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Right;rightTailAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Horizontal;
			//
			SilantroAerofoil rudderAerofoil = rudder.AddComponent<SilantroAerofoil>();rudderAerofoil.foilSubdivisions = 3;rudderAerofoil.aerofoilType = SilantroAerofoil.AerofoilType.Stabilizer;
			rudderAerofoil.surfaceType = SilantroAerofoil.SurfaceType.Rudder;
			rudderAerofoil.stabilizerPosition = SilantroAerofoil.StabilizerPosition.Center;rudderAerofoil.stabOrientation = SilantroAerofoil.StabilizerOrientation.Vertical;
			//
			SilantroAirfoil wng = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Wing.prefab",typeof(SilantroAirfoil));
			SilantroAirfoil ctl = (SilantroAirfoil)AssetDatabase.LoadAssetAtPath ("Assets/Silantro Simulator/Fixed Wing/Prefabs/Base/Airfoils/Legacy/NACA Control.prefab",typeof(SilantroAirfoil));
			rightTailAerofoil.rootAirfoil = ctl;rudderAerofoil.rootAirfoil = ctl;leftTailAerofoil.rootAirfoil = ctl;leftWingAerofoil.rootAirfoil = wng;rightWingAerofoil.rootAirfoil = wng;
			rightTailAerofoil.tipAirfoil = ctl;rudderAerofoil.tipAirfoil = ctl;leftTailAerofoil.tipAirfoil = ctl;leftWingAerofoil.tipAirfoil = wng;rightWingAerofoil.tipAirfoil = wng;
			plane.transform.position = new Vector3(0,2,0);
		}

		[MenuItem("Oyedoyin/Fixed Wing/Drag System/Create Panel/Fuselage",false,5700)]
		public static void CreateBaseFuselarge()
		{
			GameObject aircraft;
			if (Selection.activeGameObject != null) {
				aircraft = Selection.activeGameObject;
				EditorSceneManager.MarkSceneDirty (Selection.activeGameObject.gameObject.scene);
				GameObject dragPanel = new GameObject ("Fuselage Panel");
				dragPanel.transform.parent = aircraft.transform;
				dragPanel.transform.localPosition = Vector3.zero;

				//PLACE MARKERS
				SilantroBody fuselagePanel = dragPanel.AddComponent<SilantroBody>();
				fuselagePanel.AddElement ();
				fuselagePanel.AddSupplimentElement(-1);fuselagePanel.AddSupplimentElement(-2);

			} else {
				Debug.Log ("Please select a valid aircraft GameObject");
			}
		}

		[MenuItem("Oyedoyin/Fixed Wing/Drag System/Create Panel/Engine Cowling",false,5800)]
		public static void CreateBaseEngine()
		{
			Debug.Log ("Coming Soon!!");
		}

		[MenuItem("Oyedoyin/Fixed Wing/Drag System/Create Panel/Wheel Struct",false,5900)]
		public static void CreateBaseStruct()
		{
			Debug.Log ("Coming Soon!!");
		}

		[MenuItem("Oyedoyin/Fixed Wing/Drag System/Create Panel/Wheel",false,6000)]
		public static void CreateBaseWheel()
		{
			Debug.Log ("Coming Soon!!");
		}

		[MenuItem("Oyedoyin/Fixed Wing/Drag System/Tutorial",false,6100)]
		public static void DragTutorial()
		{
			Application.OpenURL("https://www.youtube.com/channel/UCYXrhYRzY11qokg59RFg7gQ/videos");
		}
	}
}