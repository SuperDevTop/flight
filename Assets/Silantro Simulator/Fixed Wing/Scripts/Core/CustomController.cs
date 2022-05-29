using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomController : MonoBehaviour {

	//-------------------------------------------------------------------------
	//------------------------------CUSTOM AIRCRAFT CONTROLLER---------------
	//-------------------------------------------------------------------------
	//Use this script to extend or connect to the main aircraft controller to send or recieve commands and variables
	//-------------------------------------------------------------------------
	public SilantroController connectedAircraft;

	//SAMPLE INPUT VARIABLE
	[Range(-1,1)]public float sampleRollInput;


	//---------------------------------//SWITCH CONTROLLER TYPE TO CUSTOM----------------------------------------
	void Awake () {

		if (connectedAircraft != null) {
			connectedAircraft.inputType = SilantroController.InputType.Custom;
		}
	}

	//---------------------------------SEND VARIABLES----------------------------------------
	void Update () {
		//SEND SAMPLE VARIABLE TO AIRCRAFT
		float yourRollInput;
		float yourPitchInput;
		if (connectedAircraft != null) {
			connectedAircraft.rollInput = sampleRollInput;
		}
	}

	//-------------------------------CALL SAMPLE COMMAND (START ENGINES)------------------------------------------
	public void SampleCommandA()
	{
		if (connectedAircraft != null) {
			connectedAircraft.TurnOnEngines ();
		}
	}

	//----------------------------------CALL SAMPLE COMMAND (SWITCH LIGHTS)---------------------------------------
	public void SampleCommandB()
	{
		if (connectedAircraft != null) {
			if(connectedAircraft.lightControl != null)
			{
				connectedAircraft.lightControl.ToggleLight();
			}
		}
	}
}
