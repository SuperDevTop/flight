using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroAirstripLights : MonoBehaviour {
	public SilantroLight[] lights;
	public float[] intensity;
	public int TotalLoop;
	int currentLoop;
	public SilantroLight currentLight;
	public float currentIntensity;
	//
	public float timer;
	public float counter;
	public float interval;
	public Light currentBulb;
	// Use this for initialization
	void Start () {
		//
		lights = GetComponentsInChildren<SilantroLight>();
		//
		TotalLoop = lights.Length;
		//
		interval = (1/77f);
		for(int i=0; i<lights.Length-1; i++)
		{
			intensity [i] = (i * interval);
		}
		currentLoop = 0;
		NextIteration ();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		counter += Time.deltaTime;
		if (counter > timer) {
			NextIteration ();
		}
	}
	//
	void NextIteration()
	{
		counter = 0;

		for (int i = 0; i < lights.Length; i++) {
			currentLoop += 1;
			if (currentLoop > TotalLoop) {
				currentLoop = 0;
			}
			currentLight = lights [i];
			if (currentLight.bulb != null) {
				currentBulb = currentLight.bulb.GetComponent<Light> ();
				int lightLoop = currentLoop + 1;
				if (lightLoop < lights.Length - 1) {
					currentIntensity = intensity [lightLoop];
				}
				//
				currentBulb.intensity = currentIntensity * 8;
				currentBulb.range = currentIntensity * 10;
			}
		}
		//

	}
}
