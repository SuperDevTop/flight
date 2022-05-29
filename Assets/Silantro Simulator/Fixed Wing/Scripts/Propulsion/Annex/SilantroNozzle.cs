using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilantroNozzle : MonoBehaviour {
	//COLLECT SYSTEM INPUTS
	[HideInInspector]public float aileronInput;
	[HideInInspector]public float elevatorInput;
	[HideInInspector]public float rudderInput;
	[HideInInspector]public float throttleInput;
	//
	public Transform ExhaustPoint;
	[HideInInspector]public Quaternion initialExhaustPointRotation;
	public enum RotationAxis
	{
		X,Y,Z
	}
	[Header("Axis Rotation")]
	public RotationAxis rotationAxis = RotationAxis.X;
	[HideInInspector]public Vector3 axisRotation;
	//DEFLECTION SETTINGS
	public float maximumDeflection;
	[HideInInspector]public float currentDeflection;
	public bool negativeDeflection;
	[HideInInspector]public bool isControllable = true;
	[HideInInspector]public float nozzleInput;
	//
	public List<NozzleSystem> nozzleSystem = new List<NozzleSystem>();
	[System.Serializable]
	public class NozzleSystem
	{
		[Header("Connections")]
		public string Identifier;
		public Transform nozzleModel;
		public enum RotationAxis
		{
			X,Y,Z
		}
		[Header("Axis Rotation")]
		public RotationAxis rotationAxis = RotationAxis.X;
		[HideInInspector]public Vector3 axisRotation;
		public bool negativeRotation = false;
		[HideInInspector]public Quaternion initalRotation;
	}
	[HideInInspector]public enum ControlBinding
	{
		//Aileron,
		Elevator,
		//Rudder,
		Engine
	}
	public ControlBinding controlBinding = ControlBinding.Elevator;









	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeNozzle()
	{
		if (rotationAxis == RotationAxis.X) {
			axisRotation = new Vector3 (1, 0, 0);
		} else if (rotationAxis == RotationAxis.Y) {
			axisRotation = new Vector3 (0, 1, 0);
		} else if (rotationAxis == RotationAxis.Z) {
			axisRotation = new Vector3 (0, 0, 1);
		}
		//STORE ExhaustPoint ROTATION
		if (ExhaustPoint != null)
		{
			initialExhaustPointRotation = ExhaustPoint.localRotation;
		}
		axisRotation.Normalize();
		//STORE PLATES ROTATION
		foreach (NozzleSystem nozzle in nozzleSystem) {
			//
			if (nozzle.negativeRotation) {
				if (nozzle.rotationAxis == NozzleSystem.RotationAxis.X) {nozzle.axisRotation = new Vector3 (-1, 0, 0);
				} else if (nozzle.rotationAxis == NozzleSystem.RotationAxis.Y) {nozzle.axisRotation = new Vector3 (0, -1, 0);
				} else if (nozzle.rotationAxis == NozzleSystem.RotationAxis.Z) {nozzle.axisRotation = new Vector3 (0, 0, -1);}
			} else {
				if (nozzle.rotationAxis == NozzleSystem.RotationAxis.X) {nozzle.axisRotation = new Vector3 (1, 0, 0);
				} else if (nozzle.rotationAxis == NozzleSystem.RotationAxis.Y) {nozzle.axisRotation = new Vector3 (0, 1, 0);
				} else if (nozzle.rotationAxis == NozzleSystem.RotationAxis.Z) {nozzle.axisRotation = new Vector3 (0, 0, 1);}
			}
			//
			nozzle.axisRotation.Normalize();
			//
			if (nozzle.nozzleModel != null) {
				nozzle.initalRotation = nozzle.nozzleModel.localRotation;
			}
			//
		}
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (isControllable) {
			//SELECT CONTROL TYPE
			 if (controlBinding == ControlBinding.Elevator) {
				nozzleInput = elevatorInput;
			}  else if (controlBinding == ControlBinding.Engine) {
				nozzleInput = throttleInput;
			}
			//
			if (negativeDeflection) {
				nozzleInput *= -1f;
			}
			//
			//DECODE CONTROL INPUT
			currentDeflection = nozzleInput * maximumDeflection;
			//
			foreach (NozzleSystem nozzle in nozzleSystem) {
				nozzle.nozzleModel.transform.localRotation = nozzle.initalRotation;
				nozzle.nozzleModel.transform.Rotate (nozzle.axisRotation, currentDeflection);
			}
			//
			if (ExhaustPoint != null) {
				ExhaustPoint.localRotation = initialExhaustPointRotation;
				ExhaustPoint.Rotate (axisRotation, currentDeflection);
			}
		}
	}
}
