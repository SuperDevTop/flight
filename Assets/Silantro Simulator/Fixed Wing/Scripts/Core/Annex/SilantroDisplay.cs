//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;using UnityEngine.UI;
//
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroDisplay : MonoBehaviour {
	//AIRCRAFT
	[HideInInspector]public SilantroController connectedAircraft;
	//
	[HideInInspector]public bool displayPoints;
	//DATA
	[HideInInspector]public Text speed;
	[HideInInspector]public Text altitude;
	[HideInInspector]public Text fuel;
	[HideInInspector]public Text weight;
	[HideInInspector]public Text brake;
	[HideInInspector]public Text density;
	[HideInInspector]public Text temperature;
	[HideInInspector]public Text pressure;
	[HideInInspector]public Text engineName;
	[HideInInspector]public Text enginePower;
	[HideInInspector]public Text engineThrust;
	[HideInInspector]public Text incrementalBrake;
	[HideInInspector]public Text flapLevel;
	[HideInInspector]public Text slatLevel;
	[HideInInspector]public Text Time;
	[HideInInspector]public Text weaponCount;
	[HideInInspector]public Text currentWeapon;
	[HideInInspector]public Text ammoCount;
	//
	public enum UnitsSetup
	{
		Metric,
		Imperial,
		Custom
	}
	[HideInInspector]public UnitsSetup units = UnitsSetup.Metric;
	public enum SpeedUnit
	{
		MeterPerSecond,
		Knots,
		FeetPerSecond,
		MilesPerHour,
		KilometerPerHour,
		Mach
	}
	[HideInInspector]public SpeedUnit speedUnit = SpeedUnit.MeterPerSecond;
	//
	public enum AltitudeUnit
	{
		Meter,
		Feet,
		NauticalMiles,
		Kilometer
	}
	[HideInInspector]public AltitudeUnit altitudeUnit = AltitudeUnit.Meter;
	//
	public enum TemperatureUnit
	{
		Celsius,
		Fahrenheit
	}
	[HideInInspector]public TemperatureUnit temperatureUnit = TemperatureUnit.Celsius;
	//
	public enum WeightUnit
	{
		Tonne,
		Pound,
		Ounce,
		Stone,
		Kilogram
	}
	[HideInInspector]public WeightUnit weightUnit = WeightUnit.Kilogram;
	//
	public enum ForceUnit
	{
		Newton,
		KilogramForce,
		PoundForce
	}
	[HideInInspector]public ForceUnit forceUnit = ForceUnit.Newton;
	//
	public enum TorqueUnit
	{
		NewtonMeter,
		PoundForceFeet
	}
	[HideInInspector]public TorqueUnit torqueUnit = TorqueUnit.NewtonMeter;
	Text[] children;




	//------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (connectedAircraft != null) {
			if (connectedAircraft != null) {
				if (engineName != null) {
					engineName.text = connectedAircraft.silantroEngineName;
				}
			}
			flapLevel.text = "Flaps = " + (Mathf.Abs(connectedAircraft.silantroFlapAngle)).ToString ("0.0") + " °";
			if (slatLevel != null) {slatLevel.text = "Slats = " + connectedAircraft.silantroSlatState;}
			incrementalBrake.text = "Brake Lever = " + (connectedAircraft.gearHelper.brakeInput * 100f).ToString ("0.0") + " %";
			//PARKING BRAKE
			if (connectedAircraft.gearHelper.brakeActivated == true) {
				brake.text = "Parking Brake = On";
			} else {
				brake.text = "Parking Brake = Off";
			}
			if (connectedAircraft.coreSystem.weatherController != null) {
				Time.text = connectedAircraft.coreSystem.weatherController.CurrentTime;
			}
			//WEIGHT SETTINGS
			float Weight = connectedAircraft.silantroWeight;
			if (weightUnit == WeightUnit.Kilogram) {
				weight.text = "Weight = "+ Weight.ToString ("0.0") + " kg";
			}
			if (weightUnit == WeightUnit.Tonne) {
				float tonneWeight = Weight * 0.001f;
				weight.text = "Weight = "+ tonneWeight.ToString ("0.00") + " T";
			}
			if (weightUnit == WeightUnit.Pound) {
				float poundWeight = Weight * 2.20462f;
				weight.text = "Weight = "+ poundWeight.ToString ("0.0") + " lb";
			}
			if (weightUnit == WeightUnit.Ounce) {
				float ounceWeight = Weight * 35.274f;
				weight.text = "Weight = "+ounceWeight.ToString("0.0") + " Oz";
			}
			if (weightUnit == WeightUnit.Stone) {
				float stonneWeight = Weight * 0.15747f;
				weight.text = "Weight = "+stonneWeight.ToString ("0.0") + " St";
			}
			//FUEL
			float Fuel = connectedAircraft.silantroFuel;
			if (weightUnit == WeightUnit.Kilogram) {
				fuel.text = "Fuel = "+ Fuel.ToString ("0.0") + " kg";
			}
			if (weightUnit == WeightUnit.Tonne) {
				float tonneWeight = Fuel * 0.001f;
				fuel.text = "Fuel = "+ tonneWeight.ToString ("0.00") + " T";
			}
			if (weightUnit == WeightUnit.Pound) {
				float poundWeight = Fuel * 2.20462f;
				fuel.text = "Fuel = "+ poundWeight.ToString ("0.0") + " lb";
			}
			if (weightUnit == WeightUnit.Ounce) {
				float ounceWeight = Fuel * 35.274f;
				fuel.text = "Fuel = "+ounceWeight.ToString("0.0") + " Oz";
			}
			if (weightUnit == WeightUnit.Stone) {
				float stonneWeight = Fuel * 0.15747f;
				fuel.text = "Fuel = "+stonneWeight.ToString ("0.0") + " St";
			}
			//SPEED
			float Speed = connectedAircraft.coreSystem.currentSpeed/1.944f;
			if (speedUnit == SpeedUnit.Knots) {
				float speedly = Speed * 1.944f;
				speed.text = "Airspeed = " + speedly.ToString ("0.0") + " knots";
			}
			if (speedUnit == SpeedUnit.MeterPerSecond) {
				float speedly = Speed;
				speed.text = "Airspeed = " + speedly.ToString ("0.0") + " m/s";
			}
			if (speedUnit == SpeedUnit.FeetPerSecond) {
				float speedly = Speed * 3.2808f;
				speed.text = "Airspeed = " + speedly.ToString ("0.0") + " ft/s";
			}
			if (speedUnit == SpeedUnit.MilesPerHour) {
				float speedly = Speed * 2.237f;
				speed.text = "Airspeed = " + speedly.ToString ("0.0") + " mph";
			}
			if (speedUnit == SpeedUnit.KilometerPerHour) {
				float speedly = Speed * 3.6f;
				speed.text = "Airspeed = " + speedly.ToString ("0.0") + " kmh";
			}
			if (speedUnit == SpeedUnit.Mach) {
				float speedly = connectedAircraft.silantroMach;
				speed.text = "Airspeed = " + speedly.ToString ("0.0") + " M";
			}
			//THRUST
			if (forceUnit == ForceUnit.Newton) {
				engineThrust.text = "Total Thrust = " + connectedAircraft.totalThrustGenerated.ToString ("0.0") + " N";
			}
			if (forceUnit == ForceUnit.KilogramForce) {
				engineThrust.text = "Total Thrust = " + (connectedAircraft.totalThrustGenerated*0.101972f).ToString ("0.0") + " kgF";
			}
			if (forceUnit == ForceUnit.PoundForce) {
				engineThrust.text = "Total Thrust = " + (connectedAircraft.totalThrustGenerated*0.224809f).ToString ("0.0") + " lbF";
			}
			//ENGINE POWER
			enginePower.text = "Engine Throttle = " + (connectedAircraft.throttleInput * 100f).ToString ("0.0") + " %";
			//ALTITUDE
			float Altitude = connectedAircraft.coreSystem.currentAltitude;
			if (altitudeUnit == AltitudeUnit.Feet) {
				float distance = Altitude;
				altitude.text = "Altitude = " + distance.ToString ("0.0") + " ft";
			}
			if (altitudeUnit == AltitudeUnit.NauticalMiles) {
				float distance = Altitude *0.00054f;
				altitude.text = "Altitude = " + distance.ToString ("0.0") + " NM";
			}
			if (altitudeUnit == AltitudeUnit.Kilometer) {
				float distance = Altitude/3280.8f;
				altitude.text = "Altitude = " + distance.ToString ("0.0") + " km";
			}
			if (altitudeUnit == AltitudeUnit.Meter) {
				float distance = Altitude/3.2808f;
				altitude.text = "Altitude = " + distance.ToString ("0.0") + " m";
			}
			//AMBIENT
			pressure.text = "Pressure = " + connectedAircraft.coreSystem.ambientPressure.ToString ("0.0") + " kpa";
			density.text = "Air Density = " + connectedAircraft.coreSystem.airDensity.ToString ("0.000") + " kg/m3";
			//
			float Temperature = connectedAircraft.coreSystem.ambientTemperature;
			if (temperatureUnit == TemperatureUnit.Celsius) {
				temperature.text = "Temperature = " + Temperature.ToString ("0.0") + " °C";
			}
			if (temperatureUnit == TemperatureUnit.Fahrenheit) {
				float temp = (Temperature * (9 / 5)) + 32f;
				temperature.text = "Temperature = " + temp.ToString ("0.0") + " °F";
			}



			//WEAPON
			if (connectedAircraft.Armaments != null) {
				//ACTIVATE
				if (!weaponCount.gameObject.active) {
					weaponCount.gameObject.SetActive (false);
					currentWeapon.gameObject.SetActive (false);
					ammoCount.gameObject.SetActive (false);
				}
				//SET VALUES
				weaponCount.text = "Weapon Count: " + connectedAircraft.Armaments.availableWeapons.Count.ToString();
				currentWeapon.text = "Current Weapon: " + connectedAircraft.Armaments.currentWeapon;
				if (connectedAircraft.Armaments.currentWeapon == "Gun") {
					int ammoTotal = 0;
					foreach(SilantroGun gun in connectedAircraft.Armaments.attachedGuns)
					{
						ammoTotal += gun.currentAmmo;
					}
					ammoCount.text = "Ammo Count: " + ammoTotal.ToString ();
				}
				//
				if (connectedAircraft.Armaments.currentWeapon == "Missile") {
					ammoCount.text = "Ammo Count: " + connectedAircraft.Armaments.missiles.Count.ToString ();
				}
				//
				if (connectedAircraft.Armaments.currentWeapon == "Bomb") {
					ammoCount.text = "Ammo Count: " + connectedAircraft.Armaments.bombs.Count.ToString ();
				}
				//
				if (connectedAircraft.Armaments.currentWeapon == "Rocket") {
					ammoCount.text = "Ammo Count: " + connectedAircraft.Armaments.rockets.Count.ToString ();
				}

			} else {
				if (weaponCount != null && weaponCount.gameObject.active) {
					weaponCount.gameObject.SetActive (false);
					currentWeapon.gameObject.SetActive (false);
					ammoCount.gameObject.SetActive (false);
				}
			}
		}
	}
}


#if UNITY_EDITOR
[CustomEditor(typeof(SilantroDisplay))]
public class DisplayEditor: Editor
{
	Color backgroundColor;
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();
		SilantroDisplay control = (SilantroDisplay)target;
		//
		serializedObject.UpdateIfRequiredOrScript();

		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox ("Connected Aircraft", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.connectedAircraft = EditorGUILayout.ObjectField (" ", control.connectedAircraft, typeof(SilantroController), true) as SilantroController;

		GUILayout.Space(15f);
		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox ("Unit Display Setup", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.units = (SilantroDisplay.UnitsSetup)EditorGUILayout.EnumPopup("Unit System",control.units);
		GUILayout.Space(3f);
		if (control.units == SilantroDisplay.UnitsSetup.Custom) {
			//
			EditorGUI.indentLevel++;
			GUILayout.Space(3f);
			control.speedUnit = (SilantroDisplay.SpeedUnit)EditorGUILayout.EnumPopup("Speed Unit",control.speedUnit);
			GUILayout.Space(3f);
			control.altitudeUnit = (SilantroDisplay.AltitudeUnit)EditorGUILayout.EnumPopup("Altitude Unit",control.altitudeUnit);
			GUILayout.Space(3f);
			control.temperatureUnit = (SilantroDisplay.TemperatureUnit)EditorGUILayout.EnumPopup("Temperature Unit",control.temperatureUnit);
			GUILayout.Space(3f);
			control.forceUnit = (SilantroDisplay.ForceUnit)EditorGUILayout.EnumPopup("Force Unit",control.forceUnit);
			GUILayout.Space(3f);
			control.weightUnit = (SilantroDisplay.WeightUnit)EditorGUILayout.EnumPopup("Weight Unit",control.weightUnit);
			GUILayout.Space(3f);
			control.torqueUnit = (SilantroDisplay.TorqueUnit)EditorGUILayout.EnumPopup("Torque Unit",control.torqueUnit);
			EditorGUI.indentLevel--;
		} else if (control.units == SilantroDisplay.UnitsSetup.Metric) {
			//
			control.speedUnit = SilantroDisplay.SpeedUnit.MeterPerSecond;
			control.altitudeUnit = SilantroDisplay.AltitudeUnit.Meter;
			control.temperatureUnit = SilantroDisplay.TemperatureUnit.Celsius;
			control.forceUnit = SilantroDisplay.ForceUnit.Newton;
			control.weightUnit = SilantroDisplay.WeightUnit.Kilogram;
			control.torqueUnit = SilantroDisplay.TorqueUnit.NewtonMeter;
			//
		} else if (control.units == SilantroDisplay.UnitsSetup.Imperial) {
			//
			//
			control.speedUnit = SilantroDisplay.SpeedUnit.Knots;
			control.altitudeUnit = SilantroDisplay.AltitudeUnit.Feet;
			control.temperatureUnit = SilantroDisplay.TemperatureUnit.Fahrenheit;
			control.forceUnit = SilantroDisplay.ForceUnit.PoundForce;
			control.weightUnit = SilantroDisplay.WeightUnit.Pound;
			control.torqueUnit = SilantroDisplay.TorqueUnit.PoundForceFeet;
			//
		}
		//
		GUILayout.Space(5f);
		GUI.color = Color.yellow;
		EditorGUILayout.HelpBox ("Output Ports", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.displayPoints = EditorGUILayout.Toggle ("Show", control.displayPoints);
		if (control.displayPoints) {
			GUILayout.Space(5f);
			control.speed = EditorGUILayout.ObjectField ("Speed Text", control.speed, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.altitude = EditorGUILayout.ObjectField ("Altitude Text", control.altitude, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.fuel = EditorGUILayout.ObjectField ("Fuel Text", control.fuel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.weight = EditorGUILayout.ObjectField ("Weight Text", control.weight, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.engineName = EditorGUILayout.ObjectField ("Engine Name Text", control.engineName, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.enginePower = EditorGUILayout.ObjectField ("Engine Power Text", control.enginePower, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.engineThrust = EditorGUILayout.ObjectField ("Engine Thrust Text", control.engineThrust, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.density = EditorGUILayout.ObjectField ("Density Text", control.density, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.pressure = EditorGUILayout.ObjectField ("Pressure Text", control.pressure, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.temperature = EditorGUILayout.ObjectField ("Temperature Text", control.temperature, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.Time = EditorGUILayout.ObjectField ("Time Text", control.Time, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.brake = EditorGUILayout.ObjectField ("Parking Brake Text", control.brake, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.flapLevel = EditorGUILayout.ObjectField ("Flap Text", control.flapLevel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.slatLevel = EditorGUILayout.ObjectField ("Slat Text", control.slatLevel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.incrementalBrake = EditorGUILayout.ObjectField ("Incremental Brake Text", control.incrementalBrake, typeof(Text), true) as Text;

			//
			GUILayout.Space(5f);
			control.weaponCount = EditorGUILayout.ObjectField ("Weapon Count", control.weaponCount, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.currentWeapon = EditorGUILayout.ObjectField ("Current Weapon", control.currentWeapon, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.ammoCount = EditorGUILayout.ObjectField ("Ammo Count", control.ammoCount, typeof(Text), true) as Text;
		}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (control);
			EditorSceneManager.MarkSceneDirty (control.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif