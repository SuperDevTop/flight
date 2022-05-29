using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroLightControl : MonoBehaviour {
	//
	[HideInInspector]public List<SilantroLight> navigationLight;
	[HideInInspector]public List<SilantroLight> strobeLight;
	[HideInInspector]public List<SilantroLight> beaconLight;
	[HideInInspector]public List<SilantroLight> landingLight;
	[HideInInspector]public bool isControllable = true;
	[HideInInspector]public SilantroLight[] lights;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//TURN ON LIGHT
	public void TurnOnLight()
	{
		if (isControllable) {
			foreach (SilantroLight light in lights) {
				if (light.state == SilantroLight.CurrentState.Off) {
					light.TurnOn ();	
				}
			}
		}
	}
	//TURN OFF LIGHT
	public void TurnOffLight()
	{
		if (isControllable) {
			foreach (SilantroLight light in lights) {
				if (light.state == SilantroLight.CurrentState.On) {
					light.TurnOff ();	
				}
			}
		}
	}
	//TOGGLE LIGHT
	public void ToggleLight()
	{
		if(isControllable){
				foreach (SilantroLight light in lights) {
					if (light.state == SilantroLight.CurrentState.On) {
						light.TurnOff ();	
					} else {
						light.TurnOn ();
					}
				}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeSwitch()
	{
		foreach (SilantroLight light in lights) {
			if (light.lightType == SilantroLight.LightType.Navigation) {
				navigationLight.Add (light);
			}
			if (light.lightType == SilantroLight.LightType.Strobe) {
				strobeLight.Add (light);
			}
			if (light.lightType == SilantroLight.LightType.Beacon) {
				beaconLight.Add (light);
			}
			if (light.lightType == SilantroLight.LightType.Landing) {
				landingLight.Add (light);
			}
		}
	}
}
