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
public class SilantroDial : MonoBehaviour {
	//CONNECTED AIRCRAFT
	[HideInInspector]public Transform Aircraft;
	[HideInInspector]public SilantroController controller;
	//DIAL TYPE
	public enum DialType
	{
		Speed,Altitude,Fuel,VerticalSpeed,RPM,BankLevel,Compass,AngleOfAttack,ArtificialHorizon,Accelerometer,FuelFlow,NozzlePosition,Mach,AirTemperature,Clock,FlapAngle
	}
	[HideInInspector]public DialType dialType = DialType.Speed;
	//
	public enum SpeedType
	{
		Knots,MPH,KPH
	}
	[HideInInspector]public SpeedType speedType = SpeedType.Knots;
	//
	public enum AltitudeType
	{
		Meters,Feet,Miles,KiloMeter,NauticalMiles
	}
	[HideInInspector]public AltitudeType altitudeType = AltitudeType.Meters;
	//
	[HideInInspector]public float initialValue;
	//
	public enum ClimbType
	{
		FPM,MPS,MPH,KPH
	}
	[HideInInspector]public ClimbType climbType = ClimbType.FPM;
	//
	[HideInInspector]public bool negativePitchRotation;
	[HideInInspector]public bool negativeYawRotation;
	public enum PitchRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public PitchRotationAxis pitchRotationAxis = PitchRotationAxis.X;
	//
	//
	public enum YawRotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public YawRotationAxis yawRotationAxis = YawRotationAxis.X;
	Vector3 pitchAxisRotation;
	Vector3 yawAxisRotation;
	//
	public enum RotationMode
	{
		Clamped,
		Free
	}
	[HideInInspector]public RotationMode rotationMode = RotationMode.Clamped;
	//
	public enum RotationAxis
	{
		X,Y,Z
	}
	[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	//q
	public enum RotationDirection
	{
		CW,
		CCW
	}
	[HideInInspector]public RotationDirection direction = RotationDirection.CW;
	//
	[HideInInspector]public Transform Needle;
	[HideInInspector]private Quaternion InitialRotation = Quaternion.identity;
	[HideInInspector]Vector3 axisRotation;
	//
	//Supply Values
	[HideInInspector]public float tempValue;
	[HideInInspector]public float currentValue;
	[HideInInspector]public float MinimumValue = 0.0f;
	[HideInInspector]public float MaximumValue = 100.0f;
	[HideInInspector]public float MinimumAngleDegrees = 0.0f;
	[HideInInspector]public float MaximumAngleDegrees = 360.0f;
	[HideInInspector]public float mulitplier = 1;
	//Supply Point
	[HideInInspector]public SilantroCore dataLog;
	[HideInInspector]public SilantroTurboFan turboFan;
	[HideInInspector]public SilantroTurboJet turbojet;
	[HideInInspector]public SilantroPistonEngine piston;
	[HideInInspector]public SilantroLiftFan liftFan;
	[HideInInspector]public SilantroTurboShaft turboShaft;
	[HideInInspector]public SilantroTurboProp turboProp;
	[HideInInspector]public SilantroElectricMotor motor;
	//
	public enum EngineType
	{
		TurboFan,
		TurboJet,
		TurboProp,
		TurboShaft,
		ElectricMotor,
		PistonEngine,
		LiftFan
	}
	[HideInInspector]public EngineType engineType = EngineType.PistonEngine;








	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeDial()
	{
		if (direction == RotationDirection.CCW) {
			if (rotationAxis == RotationAxis.X) {axisRotation = new Vector3 (-1, 0, 0);} 
			else if (rotationAxis == RotationAxis.Y) {axisRotation = new Vector3 (0, -1, 0);} 
			else if (rotationAxis == RotationAxis.Z) {axisRotation = new Vector3 (0, 0, -1);}
		} else {
			if (rotationAxis == RotationAxis.X) {axisRotation = new Vector3 (1, 0, 0);}
			else if (rotationAxis == RotationAxis.Y) {axisRotation = new Vector3 (0, 1, 0);
			} else if (rotationAxis == RotationAxis.Z) {axisRotation = new Vector3 (0, 0, 1);}
		}
		//Store Initial Model Rotation
		if (Needle != null ) {InitialRotation = Needle.localRotation;} 
		else {
			if (dialType != DialType.Clock) {
				Debug.LogError ("Needle for Dial " + transform.name + " has not been assigned");
			}
		}
		axisRotation.Normalize ();
		//
		if (dialType == DialType.Clock) {
			if (minuteHand && hourhand) {
				initialHour = hourhand.localRotation;
				initialMinute = minuteHand.localRotation;
			}
		}
		//
		if (pitchRotationAxis == PitchRotationAxis.X) {pitchAxisRotation = new Vector3 (1, 0, 0);} else if (pitchRotationAxis == PitchRotationAxis.Y) {
		pitchAxisRotation = new Vector3 (0, 1, 0);} else if (pitchRotationAxis == PitchRotationAxis.Z) {pitchAxisRotation = new Vector3 (0, 0, 1);}
		pitchAxisRotation.Normalize();
		//
		if (yawRotationAxis == YawRotationAxis.X) {yawAxisRotation = new Vector3 (1, 0, 0);}
		else if (yawRotationAxis == YawRotationAxis.Y) {yawAxisRotation = new Vector3 (0, 1, 0);
		} else if (yawRotationAxis == YawRotationAxis.Z) {yawAxisRotation = new Vector3 (0, 0, 1);}
		//
		yawAxisRotation.Normalize();
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update () {
		if (dataLog != null) {
			//Extract data from the board
			//SPEED
			if (dialType == DialType.Speed) {
				tempValue = dataLog.currentSpeed;
				//
				if (speedType == SpeedType.Knots) {
					currentValue = tempValue;
				}
				if (speedType == SpeedType.KPH) {
					currentValue = tempValue * 1.852f;
				}
				if (speedType == SpeedType.MPH) {
					currentValue = tempValue * 1.15078f;
				}
			}
			//ALTITUDE
			if (dialType == DialType.Altitude) {
				tempValue = dataLog.currentAltitude;
				//
				if (altitudeType == AltitudeType.Feet) {
					currentValue = tempValue;
				}
				if (altitudeType == AltitudeType.Meters) {
					currentValue = tempValue / 0.3048f;
				}
				if (altitudeType == AltitudeType.KiloMeter) {
					currentValue = tempValue * 0.0003048f;
				}
				if (altitudeType == AltitudeType.Miles) {
					currentValue = tempValue * 0.000189394f;
				}
				if (altitudeType == AltitudeType.NauticalMiles) {
					currentValue = tempValue * 0.000164579f;
				}
			}
			//VERTICAL SPEED
			if (dialType == DialType.VerticalSpeed) {
				tempValue = dataLog.verticalSpeed;
				//
				if (climbType == ClimbType.FPM) {
					currentValue = tempValue;
				}
				if (climbType == ClimbType.MPH) {
					currentValue = tempValue * 0.0113636f;
				}
				if (climbType == ClimbType.MPS) {
					currentValue = tempValue * 0.00508f;
				}
				if (climbType == ClimbType.KPH) {
					currentValue = tempValue * 0.018288f;
				}
			}
			//
			if (dialType == DialType.AngleOfAttack) {
				var grav = -Physics.gravity.normalized;
				var pitch = Mathf.Asin (Vector3.Dot (Aircraft.forward, grav)) * Mathf.Rad2Deg;
				pitch = Mathf.DeltaAngle (pitch, 0); 
				currentValue = Mathf.Abs (pitch);
			}
			//RPM
			if (dialType == DialType.RPM) {
				//
				if (engineType == EngineType.ElectricMotor) {
					if (motor) {
						currentValue = motor.coreRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
				if (engineType == EngineType.TurboFan) {
					if (turboFan) {
						currentValue = turboFan.coreRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
				if (engineType == EngineType.TurboJet) {
					if (turbojet) {
						currentValue = turbojet.coreRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
				if (engineType == EngineType.TurboProp) {
					if (turboProp) {
						currentValue = turboProp.coreRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
				if (engineType == EngineType.TurboShaft) {
					if (turboShaft) {
						currentValue = turboShaft.coreRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
				if (engineType == EngineType.LiftFan) {
					if (liftFan) {
						currentValue = liftFan.CurrentRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
				if (engineType == EngineType.PistonEngine) {
					if (piston) {
						currentValue = piston.coreRPM;
					} else {
						Debug.Log ("Engine for Dial " + transform.name + " has not been assigned");
					}
				}
			}
			//FUEL QUANTITY
			if (dialType == DialType.Fuel) {
				currentValue = controller.fuelsystem.TotalFuelRemaining;
			}
			//FUEL FLOW
			if (dialType == DialType.FuelFlow) {
				currentValue = controller.silantroFuelConsumption;
			}
			//BANK LEVEL
			if (dialType == DialType.BankLevel) {
				currentValue = -1 * dataLog.bankAngle;
			}
			//BANK LEVEL
			if (dialType == DialType.Mach) {
				currentValue = dataLog.machSpeed;
			}
			//COMPASS
			if (dialType == DialType.Compass) {
				currentValue = dataLog.headingDirection;
			}
			//G METER
			if (dialType == DialType.Accelerometer) {
				currentValue = dataLog.gForce;
			}
			//FLAP ANGLE
			if (dialType == DialType.FlapAngle) {
				currentValue = controller.silantroFlapAngle;
			}
			//AIR TEMPERATURE
			if (dialType == DialType.AirTemperature) {
				currentValue = dataLog.ambientTemperature;
			}
			//NOZZLE
			if (dialType == DialType.NozzlePosition) {
				float valueExtract = controller.throttleInput;
				currentValue = Mathf.Lerp (currentValue, valueExtract, Time.deltaTime * 0.8f);
			}
			//DETERMINE CURRENT VALUE
			currentValue = currentValue / mulitplier;
			//CLAMP ROTATION
			if (rotationMode == RotationMode.Clamped) {
				currentValue = Mathf.Clamp (currentValue, MinimumValue, MaximumValue);
			}
			//
			if (Needle != null) {
				Calculations ();
			}
		}
	}
	//
	[HideInInspector]public Transform hourhand;
	[HideInInspector]public Transform minuteHand;
	Quaternion initialHour,initialMinute;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void Calculations()
	{
		//
		if (dialType == DialType.ArtificialHorizon) {
			var yaw = Aircraft.eulerAngles.y;
			var pitch = Aircraft.eulerAngles.x;
			//
			if (negativeYawRotation) {
				yaw *= -1f;
			}
			if (negativePitchRotation) {
				pitch *= -1f;
			}
			//
			var yawEffect = Quaternion.AngleAxis (yaw, yawAxisRotation);
			var pitchEffect = Quaternion.AngleAxis (pitch, pitchAxisRotation);
			//
			var angleEffect = yawEffect * pitchEffect;
			Needle.localRotation = InitialRotation * angleEffect;
			//
		} else if (dialType == DialType.Clock) {
			if (dataLog.weatherController != null) {
				float minute = dataLog.weatherController.currentMinute;
				float hour = dataLog.weatherController.currentHour;
				if (hour > 12) {
					hour = hour - 12;
				}
				float hourDelta = (hour/12)*360;
				float minuteDelta = (minute/ 60)*360;
				//POSITION HANDS
				hourhand.transform.localRotation = initialHour;hourhand.transform.Rotate (axisRotation, hourDelta);
				minuteHand.transform.localRotation = initialMinute;minuteHand.transform.Rotate (axisRotation, minuteDelta);
			}
		}
		else {
			//Calculate Difference
			float valueDelta = (currentValue - MinimumValue) / (MaximumValue - MinimumValue);
			float angleDeltaDegrees = MinimumAngleDegrees + ((MaximumAngleDegrees - MinimumAngleDegrees) * valueDelta);
			//Move needle to appropriate Point
			Needle.transform.localRotation = InitialRotation;
			Needle.transform.Rotate (axisRotation, angleDeltaDegrees);
		}
	}
}










#if UNITY_EDITOR
[CustomEditor(typeof(SilantroDial))]
public class DialEditor: Editor
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
		SilantroDial dial = (SilantroDial)target;
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Dial Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		dial.dialType = (SilantroDial.DialType)EditorGUILayout.EnumPopup(" ",dial.dialType);
		GUILayout.Space (3f);
		dial.rotationMode = (SilantroDial.RotationMode)EditorGUILayout.EnumPopup("Rotation Mode",dial.rotationMode);
		GUILayout.Space (5f);
		if (dial.dialType == SilantroDial.DialType.Altitude) {
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Altimeter Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.altitudeType = (SilantroDial.AltitudeType)EditorGUILayout.EnumPopup(" ",dial.altitudeType);
		}
		if (dial.dialType == SilantroDial.DialType.Speed) {
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Speedometer Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.speedType = (SilantroDial.SpeedType)EditorGUILayout.EnumPopup(" ",dial.speedType);
		}
		if (dial.dialType == SilantroDial.DialType.VerticalSpeed) {
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Variometer Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.climbType = (SilantroDial.ClimbType)EditorGUILayout.EnumPopup(" ",dial.climbType);
		}
		if (dial.dialType == SilantroDial.DialType.Fuel) {

		}
		if (dial.dialType == SilantroDial.DialType.BankLevel) {

		}
		if (dial.dialType == SilantroDial.DialType.RPM) {
			//
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Engine Type", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (5f);
			dial.engineType = (SilantroDial.EngineType)EditorGUILayout.EnumPopup(" ",dial.engineType);
			GUILayout.Space (3f);
			if (dial.engineType == SilantroDial.EngineType.ElectricMotor) {
				dial.motor = EditorGUILayout.ObjectField (" ", dial.motor, typeof(SilantroElectricMotor), true) as SilantroElectricMotor;
			}
			if (dial.engineType == SilantroDial.EngineType.TurboFan) {
				dial.turboFan = EditorGUILayout.ObjectField (" ", dial.turboFan, typeof(SilantroTurboFan), true) as SilantroTurboFan;
			}
			if (dial.engineType == SilantroDial.EngineType.TurboJet) {
				dial.turbojet = EditorGUILayout.ObjectField (" ", dial.turbojet, typeof(SilantroTurboJet), true) as SilantroTurboJet;
			}
			if (dial.engineType == SilantroDial.EngineType.TurboProp) {
				dial.turboProp = EditorGUILayout.ObjectField (" ", dial.turboProp, typeof(SilantroTurboProp), true) as SilantroTurboProp;
			}
			if (dial.engineType == SilantroDial.EngineType.TurboShaft) {
				dial.turboShaft = EditorGUILayout.ObjectField (" ", dial.turboShaft, typeof(SilantroTurboShaft), true) as SilantroTurboShaft;
			}
			if (dial.engineType == SilantroDial.EngineType.PistonEngine) {
				dial.piston = EditorGUILayout.ObjectField (" ", dial.piston, typeof(SilantroPistonEngine), true) as SilantroPistonEngine;
			}
			if (dial.engineType == SilantroDial.EngineType.LiftFan) {
				dial.liftFan = EditorGUILayout.ObjectField (" ", dial.liftFan, typeof(SilantroLiftFan), true) as SilantroLiftFan;
			}
		}
		if (dial.dialType == SilantroDial.DialType.Compass) {

		}
		if (dial.dialType != SilantroDial.DialType.ArtificialHorizon && dial.dialType != SilantroDial.DialType.Clock) {
			//
			GUILayout.Space (10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Rotation Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.rotationAxis = (SilantroDial.RotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", dial.rotationAxis);
			GUILayout.Space (3f);
			dial.direction = (SilantroDial.RotationDirection)EditorGUILayout.EnumPopup ("Rotation Direction", dial.direction);
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Rotation Amounts", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.MinimumAngleDegrees = EditorGUILayout.FloatField ("Minimum Angle", dial.MinimumAngleDegrees);
			GUILayout.Space (3f);
			dial.MaximumAngleDegrees = EditorGUILayout.FloatField ("Maximum Angle", dial.MaximumAngleDegrees);
			//
			//
			GUILayout.Space (15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Data Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.MinimumValue = EditorGUILayout.FloatField ("Minimum Value", dial.MinimumValue);
			GUILayout.Space (3f);
			dial.MaximumValue = EditorGUILayout.FloatField ("Maximum Value", dial.MaximumValue);
			GUILayout.Space (10f);
			dial.mulitplier = EditorGUILayout.FloatField ("Value Multiplier", dial.mulitplier);
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Current Amount", dial.currentValue.ToString ("0.00"));
			//
			//
			GUILayout.Space (15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.Needle = EditorGUILayout.ObjectField ("Needle", dial.Needle, typeof(Transform), true) as Transform;
			//
		} else if (dial.dialType == SilantroDial.DialType.ArtificialHorizon) {
			GUILayout.Space (10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.pitchRotationAxis = (SilantroDial.PitchRotationAxis)EditorGUILayout.EnumPopup ("Pitch Rotation Axis", dial.pitchRotationAxis);
			GUILayout.Space (2f);
			GUILayout.Space (2f);
			dial.negativePitchRotation = EditorGUILayout.Toggle ("Invert Pitch Rotation", dial.negativePitchRotation);
			//
			GUILayout.Space (5f);
			dial.yawRotationAxis = (SilantroDial.YawRotationAxis)EditorGUILayout.EnumPopup ("Yaw Rotation Axis", dial.yawRotationAxis);
			GUILayout.Space (2f);
			GUILayout.Space (2f);
			dial.negativeYawRotation = EditorGUILayout.Toggle ("Invert Yaw Rotation", dial.negativeYawRotation);
			//
			GUILayout.Space (15f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Connections", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.Needle = EditorGUILayout.ObjectField ("Ball Indicator", dial.Needle, typeof(Transform), true) as Transform;
		} 
		//
		else if (dial.dialType == SilantroDial.DialType.Clock) {
			GUILayout.Space (10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Rotation Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.rotationAxis = (SilantroDial.RotationAxis)EditorGUILayout.EnumPopup ("Rotation Axis", dial.rotationAxis);
			GUILayout.Space (3f);
			dial.direction = (SilantroDial.RotationDirection)EditorGUILayout.EnumPopup ("Rotation Direction", dial.direction);
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Hands", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			dial.hourhand = EditorGUILayout.ObjectField ("Hour Hand", dial.hourhand, typeof(Transform), true) as Transform;
			GUILayout.Space (3f);
			dial.minuteHand = EditorGUILayout.ObjectField ("Minute Hand", dial.minuteHand, typeof(Transform), true) as Transform;
		}
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (serializedObject.targetObject, "Dial Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (dial);
			EditorSceneManager.MarkSceneDirty (dial.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif