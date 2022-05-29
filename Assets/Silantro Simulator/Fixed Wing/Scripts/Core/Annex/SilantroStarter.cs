using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroStarter : MonoBehaviour {
	[Header("Connected Aircraft")]
	public SilantroController aircraft;
	//
	void Start()
	{
		StartCoroutine (StartUpAircraft ());
	}
	//SETUP
	IEnumerator StartUpAircraft () {
		yield return new WaitForSeconds (0.002f);//JUST LAG A BIT BEHIND CONTROLLER SCRIPT
		//STARTUP AIRCRAFT	
		aircraft.StartAircraft();
		//RAISE GEAR
		if (aircraft.gearHelper != null) {aircraft.gearHelper.RaiseGear ();}
		//TURN ON LIGHTS
		if (aircraft.lightControl != null) {aircraft.lightControl.TurnOnLight ();}
	}
}
