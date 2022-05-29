using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
//
public class SilantroLever : MonoBehaviour {
	//LEVER TYPE
	public enum LeverType
	{
		Stick,Throttle,Pedal,Flaps,GearIndicator
	}
	[HideInInspector]public LeverType leverType = LeverType.Stick;
	//CONTROL STICK
	public enum StickType
	{
		Joystick,Yoke
	}
	[HideInInspector]public StickType stickType = StickType.Joystick;
	//
	[HideInInspector]public Transform lever;
	[HideInInspector]public Transform yoke;
	//
	private Quaternion initialYokeRotation;
	float throttleAmount;
	[HideInInspector]public float maximumDeflection;
	//
	[HideInInspector]public float MaximumPitchDeflection;
	[HideInInspector]public bool negativePitchRotation;
	[HideInInspector]public float MaximumRollDeflection;
	[HideInInspector]public bool negativeRollRotation;
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public SilantroAerofoil wingInput;
	[HideInInspector]public SilantroGearSystem gearSystem;
	[HideInInspector]public bool combined;
	//GEAR BULB MATERIALS
	[HideInInspector]public Material bulbMaterials;
	float currentGearRotation;
	//
	//
	public enum PitchRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public PitchRotationAxis pitchRotationAxis = PitchRotationAxis.X;
	//
	//
	public enum RollRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RollRotationAxis rollRotationAxis = RollRotationAxis.X;
	//
	//
	public enum RudderRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RudderRotationAxis rudderRotationAxis = RudderRotationAxis.X;
	//
	//Supply Point
	[HideInInspector]public SilantroCore dataLog;
	//
	public enum RotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	//
	Vector3 axisRotation;
	Vector3 pitchAxisRotation;
	Vector3 rollAxisRotation;
	public enum RotationDirection
	{
		CW,
		CCW
	}
	[HideInInspector]public RotationDirection direction = RotationDirection.CW;
	private Quaternion InitialRotation = Quaternion.identity;
	public enum PedalType
	{
		Sliding,
		Hinged
	}
	[HideInInspector]public PedalType pedalType = PedalType.Hinged;
	//RIGHT PEDAL
	[HideInInspector]public Transform rightPedal;
	[HideInInspector]public enum RightRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RightRotationAxis rightRotationAxis = RightRotationAxis.X;
	Vector3 rightAxisRotation;
	Quaternion initialRightRotation;
	Vector3 initialRightPosition;
	//LEFT PEDAL
	[HideInInspector]public Transform leftPedal;
	[HideInInspector]public enum LeftRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public LeftRotationAxis leftRotationAxis = LeftRotationAxis.X;
	Vector3 leftAxisRotation;
	Quaternion initialLeftRotation;
	Vector3 initialLeftPosition;
	[HideInInspector]public float maximumPedalDeflection = 20f;
	[HideInInspector]public float maximumSlidingDistance = 10f;
	//CURRENT VALUES
	[HideInInspector]public float currentPedalDeflection;
	[HideInInspector]public float currentDistance;
	//INPUT
	[HideInInspector]public float rudderInput;












	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeLever () {
		//
		if (direction == RotationDirection.CCW) {
			if (rotationAxis == RotationAxis.X) {axisRotation = new Vector3 (-1, 0, 0);} 
			else if (rotationAxis == RotationAxis.Y) {axisRotation = new Vector3 (0, -1, 0);} 
			else if (rotationAxis == RotationAxis.Z) {axisRotation = new Vector3 (0, 0, -1);}
		} else {
			if (rotationAxis == RotationAxis.X) {axisRotation = new Vector3 (1, 0, 0);} 
			else if (rotationAxis == RotationAxis.Y) {axisRotation = new Vector3 (0, 1, 0);} 
			else if (rotationAxis == RotationAxis.Z) {axisRotation = new Vector3 (0, 0, 1);}
		}
		//
		axisRotation.Normalize ();
		//STORE INITIAL ROTATION
		if (lever != null) {
			InitialRotation = lever.localRotation;
		}
		//
		if (stickType == StickType.Yoke) {
			initialYokeRotation = yoke.localRotation;
		}
		if (pitchRotationAxis == PitchRotationAxis.X) {pitchAxisRotation = new Vector3 (1, 0, 0);
		} else if (pitchRotationAxis == PitchRotationAxis.Y) {pitchAxisRotation = new Vector3 (0, 1, 0);} 
		else if (pitchRotationAxis == PitchRotationAxis.Z) {pitchAxisRotation = new Vector3 (0, 0, 1);}
		//
		pitchAxisRotation.Normalize();
		//
		if (rollRotationAxis == RollRotationAxis.X) {rollAxisRotation = new Vector3 (1, 0, 0);
		} else if (rollRotationAxis == RollRotationAxis.Y) {rollAxisRotation = new Vector3 (0, 1, 0);
		} else if (rollRotationAxis == RollRotationAxis.Z) {rollAxisRotation = new Vector3 (0, 0, 1);}
		//
		//
		rollAxisRotation.Normalize();
		//
		//SETUP PEDALS
		if (leverType == LeverType.Pedal) {
			//STORE ROTATIONS/POSITIONS
			initialLeftRotation = leftPedal.localRotation;initialRightRotation = rightPedal.localRotation;
			initialLeftPosition = leftPedal.localPosition;initialRightPosition = rightPedal.localPosition;
			//DETERMINE ROTATION/SLIDING AXIS
			if (direction == RotationDirection.CCW) {
				//RIGHT
				if (rightRotationAxis == RightRotationAxis.X) {rightAxisRotation = new Vector3 (-1, 0, 0);}
				else if (rightRotationAxis == RightRotationAxis.Y) {rightAxisRotation = new Vector3 (0, -1, 0);} 
				else if (rightRotationAxis == RightRotationAxis.Z) {rightAxisRotation = new Vector3 (0, 0, -1);}
				//LEFT
				if (leftRotationAxis == LeftRotationAxis.X) {leftAxisRotation = new Vector3 (1, 0, 0);
				} else if (leftRotationAxis == LeftRotationAxis.Y) {leftAxisRotation = new Vector3 (0, 1, 0);
				} else if (leftRotationAxis == LeftRotationAxis.Z) {leftAxisRotation = new Vector3 (0, 0, 1);}
			} else {
				//RIGHT
				if (rightRotationAxis == RightRotationAxis.X) {rightAxisRotation = new Vector3 (1, 0, 0);
				} else if (rightRotationAxis == RightRotationAxis.Y) {rightAxisRotation = new Vector3 (0, 1, 0);
				} else if (rightRotationAxis == RightRotationAxis.Z) {rightAxisRotation = new Vector3 (0, 0, 1);}
				//LEFT
				if (leftRotationAxis == LeftRotationAxis.X) {leftAxisRotation = new Vector3 (-1, 0, 0);} 
				else if (leftRotationAxis == LeftRotationAxis.Y) {leftAxisRotation = new Vector3 (0, -1, 0);} 
				else if (leftRotationAxis == LeftRotationAxis.Z) {leftAxisRotation = new Vector3 (0, 0, -1);}
			}
			//NORMALIZE ROTATION
			leftAxisRotation.Normalize();rightAxisRotation.Normalize ();
		}
	}










	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update () {
		//
		if (controller != null) {
			//
			if (leverType == LeverType.Stick) {
				float pitch = controller.pitchInput * MaximumPitchDeflection;
				if (negativePitchRotation) {
					pitch *= -1f;
				}
				float roll = controller.rollInput * MaximumRollDeflection;
				if (negativeRollRotation) {
					roll *= -1f;
				}
				//
				var rollEffect = Quaternion.AngleAxis (roll, rollAxisRotation);
				var pitchEffect = Quaternion.AngleAxis (pitch, pitchAxisRotation);
				//
				if (stickType == StickType.Joystick) {
					var angleEffect = rollEffect * pitchEffect;
					lever.localRotation = InitialRotation * angleEffect;
				} else {
					lever.localRotation = InitialRotation * pitchEffect;
					yoke.localRotation = initialYokeRotation * rollEffect;
				}
				//
			}
			//
			if (leverType == LeverType.Throttle) {
				//COLELCT THROTTLE INPUT
				throttleAmount = controller.throttleInput;
				//
				throttleAmount = Mathf.Clamp (throttleAmount, 0, 1.0f);
				throttleAmount *= maximumDeflection;
				lever.localRotation = InitialRotation;
				lever.Rotate (axisRotation, throttleAmount);
			}
			//
			if (leverType == LeverType.Flaps && wingInput != null) {
				//COLLECT INPUT
				float flapAngle = Mathf.Abs (wingInput.flapAngle);
				lever.localRotation = InitialRotation;
				lever.Rotate (axisRotation, flapAngle);
			}
			if (leverType == LeverType.GearIndicator) {
				//
				if (gearSystem != null) {
					if (gearSystem.gearOpened == true) {
						currentGearRotation = Mathf.Lerp (currentGearRotation, 0, Time.deltaTime * 2f);
					} else {
						currentGearRotation = Mathf.Lerp (currentGearRotation, maximumDeflection, Time.deltaTime * 2f);
					}
				}
				//
				if (lever != null) {
					lever.transform.localRotation = InitialRotation;
					lever.transform.Rotate (axisRotation, currentGearRotation);
				}
				//
				if (gearSystem != null && bulbMaterials != null) {
					if (gearSystem.gearOpened == true) {
						bulbMaterials.color = Color.green;
					} else {
						bulbMaterials.color = Color.red;
					}
				}
			}
			if (leverType == LeverType.Pedal) {
				rudderInput = controller.yawInput;
				//ROTATE
				if (pedalType == PedalType.Hinged) {
					currentPedalDeflection = rudderInput * maximumDeflection;
					//ROTATE PEDALS
					rightPedal.localRotation = initialRightRotation;
					rightPedal.Rotate (rightAxisRotation, currentPedalDeflection);
					if (!combined) {
						leftPedal.localRotation = initialLeftRotation;
						leftPedal.Rotate (leftAxisRotation, currentPedalDeflection);
					} else {
						leftPedal.localRotation = initialLeftRotation;
						leftPedal.Rotate (leftAxisRotation, -currentPedalDeflection);
					}
				}
				//MOVE
				if (pedalType == PedalType.Sliding) {
					currentDistance = rudderInput * (maximumSlidingDistance / 100);
					//MOVE PEDALS
					rightPedal.localPosition = initialRightPosition;
					rightPedal.localPosition += rightAxisRotation * currentDistance;
					leftPedal.localPosition = initialLeftPosition;
					leftPedal.localPosition += leftAxisRotation * currentDistance;
				}
			}
		}
	}
}












#if UNITY_EDITOR
[CustomEditor(typeof(SilantroLever))]
public class LeverEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		serializedObject.Update ();
		//
		SilantroLever lever = (SilantroLever)target;
		//
		GUILayout.Space(2f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Lever Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		lever.leverType = (SilantroLever.LeverType)EditorGUILayout.EnumPopup("Lever Type",lever.leverType);
		if (lever.leverType == SilantroLever.LeverType.Stick) {
			GUILayout.Space (5f);
			lever.stickType = (SilantroLever.StickType)EditorGUILayout.EnumPopup("Control Type",lever.stickType);
			GUILayout.Space (5f);
			if (lever.stickType == SilantroLever.StickType.Joystick) {
				GUILayout.Space (5f);
				lever.lever = EditorGUILayout.ObjectField ("Stick", lever.lever, typeof(Transform), true) as Transform;
			} else {
				GUILayout.Space (5f);
				lever.lever = EditorGUILayout.ObjectField ("Yoke Stick", lever.lever, typeof(Transform), true) as Transform;
				GUILayout.Space (3f);
				lever.yoke = EditorGUILayout.ObjectField ("Yoke Wheel", lever.yoke, typeof(Transform), true) as Transform;
			}
			GUILayout.Space(2f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			lever.pitchRotationAxis = (SilantroLever.PitchRotationAxis)EditorGUILayout.EnumPopup("Pitch Rotation Axis",lever.pitchRotationAxis);
			GUILayout.Space(2f);
			lever.MaximumPitchDeflection = EditorGUILayout.FloatField ("Max Pitch Deflection", lever.MaximumPitchDeflection);
			GUILayout.Space(2f);
			lever.negativePitchRotation = EditorGUILayout.Toggle ("Invert Pitch Rotation", lever.negativePitchRotation);
			//
			GUILayout.Space (5f);
			lever.rollRotationAxis = (SilantroLever.RollRotationAxis)EditorGUILayout.EnumPopup("Roll Rotation Axis",lever.rollRotationAxis);
			GUILayout.Space(2f);
			lever.MaximumRollDeflection = EditorGUILayout.FloatField ("Max Roll Deflection", lever.MaximumRollDeflection);
			GUILayout.Space(2f);
			lever.negativeRollRotation = EditorGUILayout.Toggle ("Invert Roll Rotation", lever.negativeRollRotation);


		}
		if (lever.leverType == SilantroLever.LeverType.Throttle) {
			GUILayout.Space (5f);
			lever.lever = EditorGUILayout.ObjectField ("Throttle", lever.lever, typeof(Transform), true) as Transform;
			GUILayout.Space (2f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			//
			GUILayout.Space (10f);
			lever.rotationAxis = (SilantroLever.RotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", lever.rotationAxis);
			GUILayout.Space (3f);
			lever.direction = (SilantroLever.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",lever.direction);
			GUILayout.Space (3f);
			lever.maximumDeflection = EditorGUILayout.FloatField ("Maximum Deflection", lever.maximumDeflection);
		}
		//
		if (lever.leverType == SilantroLever.LeverType.Flaps) {
			GUILayout.Space (5f);
			lever.lever = EditorGUILayout.ObjectField ("Flap Lever", lever.lever, typeof(Transform), true) as Transform;
			GUILayout.Space (2f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			//
			GUILayout.Space (10f);
			lever.rotationAxis = (SilantroLever.RotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", lever.rotationAxis);
			GUILayout.Space (3f);
			lever.direction = (SilantroLever.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",lever.direction);
		}
		//
		if (lever.leverType == SilantroLever.LeverType.Pedal) {
			GUILayout.Space (5f);
			lever.pedalType = (SilantroLever.PedalType)EditorGUILayout.EnumPopup ("Pedal Type", lever.pedalType);
			//
			//
			if (lever.pedalType == SilantroLever.PedalType.Hinged) {
				GUILayout.Space (10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Right Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				lever.rightPedal = EditorGUILayout.ObjectField ("Right Pedal", lever.rightPedal, typeof(Transform), true) as Transform;
				GUILayout.Space (3f);
				lever.rightRotationAxis = (SilantroLever.RightRotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", lever.rightRotationAxis);
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Left Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				lever.leftPedal = EditorGUILayout.ObjectField ("Left Pedal", lever.leftPedal, typeof(Transform), true) as Transform;
				GUILayout.Space (3f);
				lever.leftRotationAxis = (SilantroLever.LeftRotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", lever.leftRotationAxis);
				GUILayout.Space (10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				lever.combined = EditorGUILayout.Toggle ("Clamped Together", lever.combined);
				GUILayout.Space (2f);
				lever.direction = (SilantroLever.RotationDirection)EditorGUILayout.EnumPopup ("Rotation Direction", lever.direction);
				GUILayout.Space (3f);
				lever.maximumDeflection = EditorGUILayout.FloatField ("Maximum Deflection", lever.maximumDeflection);
			}
			if (lever.pedalType == SilantroLever.PedalType.Sliding) {
				GUILayout.Space (10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Right Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				lever.rightPedal = EditorGUILayout.ObjectField ("Right Pedal", lever.rightPedal, typeof(Transform), true) as Transform;
				GUILayout.Space (3f);
				lever.rightRotationAxis = (SilantroLever.RightRotationAxis)EditorGUILayout.EnumPopup ("Sliding Axis", lever.rightRotationAxis);
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Left Pedal Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				lever.leftPedal = EditorGUILayout.ObjectField ("Left Pedal", lever.leftPedal, typeof(Transform), true) as Transform;
				GUILayout.Space (3f);
				lever.leftRotationAxis = (SilantroLever.LeftRotationAxis)EditorGUILayout.EnumPopup ("Sliding Axis", lever.leftRotationAxis);
				GUILayout.Space (10f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Sliding Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				lever.direction = (SilantroLever.RotationDirection)EditorGUILayout.EnumPopup ("Rotation Direction", lever.direction);
				GUILayout.Space (3f);
				lever.maximumSlidingDistance = EditorGUILayout.FloatField ("Sliding Distance (cm)", lever.maximumSlidingDistance);
			}
		}
		//
		if (lever.leverType == SilantroLever.LeverType.GearIndicator) {
			//
			GUILayout.Space (5f);
			lever.lever = EditorGUILayout.ObjectField ("Gear Lever", lever.lever, typeof(Transform), true) as Transform;
			GUILayout.Space (2f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			//
			GUILayout.Space (10f);
			lever.rotationAxis = (SilantroLever.RotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", lever.rotationAxis);
			GUILayout.Space (3f);
			lever.direction = (SilantroLever.RotationDirection)EditorGUILayout.EnumPopup("Rotation Direction",lever.direction);
			GUILayout.Space (3f);
			lever.maximumDeflection = EditorGUILayout.FloatField ("Maximum Deflection", lever.maximumDeflection);
			//
			GUILayout.Space (10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Bulb Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (2f);
			lever.bulbMaterials = EditorGUILayout.ObjectField ("Bulb Material", lever.bulbMaterials, typeof(Material), true) as Material;
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (serializedObject.targetObject, "Lever Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (lever);
			EditorSceneManager.MarkSceneDirty (lever.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif