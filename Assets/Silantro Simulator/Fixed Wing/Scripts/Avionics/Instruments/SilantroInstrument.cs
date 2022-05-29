using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SilantroInstrument : MonoBehaviour {
	//DISPLAY TYPE
	public enum DisplayType
	{
		Speedometer,Mach,Altimeter,Variometer,Compass,Horizon,FuelQuantity,EnginePower,N1,N2,Temperature
	}
	public DisplayType displayType = DisplayType.Speedometer;
	//
	public float currentValue;
	public float maximumValue;
	//
	public float maximumRotation = 360f;
	public float minimumRotation = 0f;
	//
	public RectTransform needle;
	float needleRotation;
	float smoothRotation;
	public Text valueOutput;
	//CONTROL VARIABLES
	public float inputFactor = 1f;
	public float rotationFactor = 1f;
	//
	//CONNECTION
	public SilantroController connectedAircraft;
	//
	public float rollValue;
	public float pitchValue;
	public float minimumPosition;
	public float movementFactor;
	public float maximumPitch;
	public float minimumPitch;
	public RectTransform pitchTape;
	//
	public RectTransform rollAnchor;
	public float minimumRoll;
	public float maximumRoll;
	public float minimumValue;
	//

	public RectTransform TemperatureNeedle;
	private float  TemperatureNeedlePosition = 0.0f;
	private float smoothedTemperatureNeedlePosition = 0.0f;
	//
	public float minimumTemperaturePosition = 20.0f;
	public float maximumTemperaturePosition = 160.0f;

	void Update()
	{
		//1.SPEED
		if(displayType == DisplayType.Speedometer){
		currentValue = connectedAircraft.coreSystem.currentSpeed/1.944f;
		}
		//2.ALTITUDE
		if(displayType == DisplayType.Altimeter){
			currentValue = connectedAircraft.coreSystem.currentAltitude/3.285f;
		}
		//3.COMPASS
		if(displayType == DisplayType.Compass){
			currentValue = connectedAircraft.coreSystem.headingDirection;
		}
		//4.MACH
		if(displayType == DisplayType.Mach){
			currentValue = connectedAircraft.coreSystem.machSpeed;
		}
		//5.CLIMB RATE
		if(displayType == DisplayType.Variometer){
			currentValue = connectedAircraft.coreSystem.verticalSpeed;
			float valueDelta = (currentValue - minimumValue) / (maximumValue - minimumValue);
			float angleDeltaDegrees = minimumRotation + ((maximumRotation - minimumRotation) * valueDelta);
			needle.transform.eulerAngles = new Vector3 (needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -angleDeltaDegrees);

		}
		//6.CLIMB RATE
		if(displayType == DisplayType.FuelQuantity){
			currentValue = connectedAircraft.fuelsystem.TotalFuelRemaining;

		}
		//7.ENGINE POWER
		if(displayType == DisplayType.EnginePower){
			currentValue = connectedAircraft.silantroEnginePower*100f;
		}
		//8.ENGINE N1
		if(displayType == DisplayType.N1){
			currentValue = connectedAircraft.silantroN1;
		}
		//9.ENGINE N2
		if(displayType == DisplayType.N2){
			currentValue = connectedAircraft.silantroN2;
		}
		//10. Temperature
		if(displayType == DisplayType.Temperature){
			currentValue = connectedAircraft.silantroEnginePower * maximumValue;
			TemperatureNeedlePosition = Mathf.Lerp (minimumTemperaturePosition, maximumTemperaturePosition, currentValue/ maximumValue);
			smoothedTemperatureNeedlePosition = Mathf.Lerp (smoothedTemperatureNeedlePosition, TemperatureNeedlePosition, Time.deltaTime * 5);
			TemperatureNeedle.transform.localPosition = new Vector3(TemperatureNeedle.transform.eulerAngles.x ,smoothedTemperatureNeedlePosition,TemperatureNeedle.transform.eulerAngles.z );
			valueOutput.text = currentValue.ToString ("0.0") + " °C";
		}
		if (displayType == DisplayType.Horizon) {
			pitchValue = Mathf.DeltaAngle (0, -connectedAircraft.aircraft.transform.rotation.eulerAngles.x);
			float extension = minimumPosition + movementFactor * Mathf.Clamp (pitchValue, minimumPitch, maximumPitch) / 10f;
			pitchTape.anchoredPosition3D = new Vector3 (pitchTape.anchoredPosition3D.x, extension, pitchTape.anchoredPosition3D.z);
			//
			rollValue = Mathf.DeltaAngle(0,-connectedAircraft.aircraft.transform.eulerAngles.z);
			float rotation = Mathf.Clamp (rollValue, minimumRoll, maximumRoll);
			rollAnchor.localEulerAngles = new Vector3 (rollAnchor.localEulerAngles.x, rollAnchor.localEulerAngles.y, rotation);
		}
		if (displayType != DisplayType.Variometer && displayType != DisplayType.Temperature) {
			//CONVERT
			float dataValue = currentValue * inputFactor;
			//ROTATE
			if (needle != null) {
				needleRotation = Mathf.Lerp (minimumRotation, (maximumRotation * rotationFactor), dataValue / (maximumValue * rotationFactor));
				smoothRotation = Mathf.Lerp (smoothRotation, needleRotation, Time.deltaTime * 5);
				needle.transform.eulerAngles = new Vector3 (needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -smoothRotation);
			}
		}
		//DISPLAY VALUE
		if (valueOutput != null) {
			float dataValue = currentValue * inputFactor;
			valueOutput.text = dataValue.ToString ("0.0");
		}
		
	}
}
