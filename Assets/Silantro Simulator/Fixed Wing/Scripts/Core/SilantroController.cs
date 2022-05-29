// Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class SilantroController : MonoBehaviour {
	//NAME
	[HideInInspector]public string aircraftName = "Default";
	public enum AircraftType{CTOL,VTOL}
	[HideInInspector]public AircraftType aircraftType = AircraftType.CTOL;
	//ENGINE
	public enum EngineType{Unpowered,Electric,Piston,TurboJet,TurboFan,TurboProp,TurboShaft,Pegasus}
	[HideInInspector]public EngineType engineType = EngineType.Piston;
	//AIRCRAFT TYPE
	public enum WingType{Biplane,Monoplane}
	[HideInInspector]public WingType wingType = WingType.Monoplane;
	//CONTROL SYSTEM
	public enum StartMode{Cold,Hot}
	[HideInInspector]public StartMode startMode = StartMode.Cold;
	[HideInInspector]public float startSpeed;
	[HideInInspector]public float startAltitude;
	[HideInInspector]public float tempRoll,tempPitch,tempYaw;
	//
	public enum InputType{Default,Custom}
	[HideInInspector]public InputType inputType = InputType.Default;
	public enum ControlType{External,Internal}
	[HideInInspector]public ControlType controlType = ControlType.External;
	[HideInInspector]public bool pilotOnboard = false;
	[HideInInspector]public bool isGrounded;
	//

	//DATA
	[HideInInspector]public float totalWingArea = 0f;
	[HideInInspector]public int AvailableEngines = 0;
	[HideInInspector]public float totalThrustGenerated = 0f;
	[HideInInspector]public float totalConsumptionRate;
	[HideInInspector]public float wingLoading;
	[HideInInspector]public float thrustToWeightRatio;
	[HideInInspector]public float initialDrag;
	//WEIGHT
	[HideInInspector]public float emptyWeight = 1000f;
	[HideInInspector]public float currentWeight;
	[HideInInspector]public float maximumWeight = 5000f;


	//SYSTEMS
	[HideInInspector]public SilantroController controller;
	[HideInInspector]public SilantroFuelDistributor fuelsystem;
	[HideInInspector]public SilantroCore coreSystem;
	[HideInInspector]public SilantroGearSystem gearHelper;
	[HideInInspector]public Rigidbody aircraft;
	[HideInInspector]public SilantroAerofoil[] wings;
	[HideInInspector]public SilantroCamera view;
	[HideInInspector]SilantroSapphire weather;//
	[HideInInspector]SilantroFuelTank[] fuelTanks;
	[HideInInspector]SilantroBlade[] blades;
	[HideInInspector]SilantroPayload[] payloadElements;
	SilantroSpeedBrakes speedBrakes;
	[HideInInspector]public SilantroLightControl lightControl;
	SilantroLight[] lightbulbs;
	SilantroWingActuator wingActuator;
	[HideInInspector]public SilantroHydraulicSystem[] hydraulics;
	[HideInInspector]public SilantroDial[] dials;[HideInInspector]public SilantroLever[] levers;
	[HideInInspector]public SilantroNozzle[] nozzles;
	[HideInInspector]public SilantroDataLogger blackBox;
	[HideInInspector]public SilantroRadar radarCore;
	[HideInInspector]public SilantroArmament Armaments;
	//ENGINES
	[HideInInspector]public SilantroPistonEngine[] pistons;
	[HideInInspector]public SilantroTurboJet[] turboJets;
	[HideInInspector]public SilantroTurboFan[] turboFans;
	[HideInInspector]public SilantroTurboProp[] turboProps;
	[HideInInspector]public SilantroTurboShaft[] turboShafts;
	[HideInInspector]public SilantroPegasusEngine pegasus;
	[HideInInspector]public SilantroElectricMotor[] motors;
	float additionalDrag,totalDrag;
	[HideInInspector]public SilantroBody[] dragBodies;



	//HORIZONTAL STABILIZER
	public enum StabilizerType{Split,Unison,WingBased,Canard}
	[HideInInspector]public StabilizerType stabilizerType = StabilizerType.Split;
	//BIPLANE
	[HideInInspector]public SilantroAerofoil leftTopWing;
	[HideInInspector]public SilantroAerofoil rightTopWing;
	[HideInInspector]public SilantroAerofoil leftBottomWing;
	[HideInInspector]public SilantroAerofoil rightBottomWing;
	SilantroAerofoil flapfoil;SilantroAerofoil slatfoil;

	[HideInInspector]public float upperWingSpan;
	[HideInInspector]public float lowerWingSpan;
	[HideInInspector]public float wingGap;
	//MONOPLANE
	[HideInInspector]public SilantroAerofoil leftWing;
	[HideInInspector]public SilantroAerofoil rightWing;
	[HideInInspector]public float mainWingSpan;
	[HideInInspector]public SilantroAerofoil leftStabilizer;
	[HideInInspector]public SilantroAerofoil rightStabilizer;
	[HideInInspector]public SilantroAerofoil centerStabilizer;
	[HideInInspector]public float stabilizerSpan;
	//CONTROLS
	[HideInInspector]public float rollSensitivity = 0.9f;
	[HideInInspector]public float pitchSensitivity = 0.9f;
	[HideInInspector]public float yawSensitivity = 0.9f;
	[HideInInspector]public float controlFactor = 10f;
	//INPUTS
	[HideInInspector]public float rollInput;
	[HideInInspector]public float pitchInput;
	[HideInInspector]public float yawInput;
	[HideInInspector]public float throttleInput;
	float brakeControl;
	//
	//ENTER_ EXIT STUFF
	[HideInInspector]public SilantroHydraulicSystem canopyActuator;
	[HideInInspector]public GameObject player;
	[HideInInspector]public GameObject interiorPilot;//Placehoder
	[HideInInspector]public SilantroDisplay canvasDisplay;
	public enum PlayerType{ThirdPerson,FirstPerson}
	[HideInInspector]public PlayerType playerType = PlayerType.ThirdPerson;
	[HideInInspector]public Transform getOutPosition;
	[HideInInspector]public GameObject ArmamentsStorage;
	//CANOPY
	private bool  opened = false;
	private bool  temp = false;
	float openTime;
	float closeTime;
	bool canExit;
	//MAIN SCENE CAMERA
	Camera mainCamera;
	//
	//ENGINE SOUNDS
	public enum SoundState{Exterior,Interior}
	[HideInInspector]public SoundState currentSoundState = SoundState.Exterior;
	bool isControllable = true;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ACTIVATE AIRCRAFT CONTROLS
	/// <summary>
	/// Sets the state of the aircraft control.
	/// </summary>
	/// <param name="state">If set to <c>true</c> aircraft is controllable.</param>
	public void SetControlState(bool state)
	{
		//ENABLE WINGS
		if (wings != null) {
			foreach (SilantroAerofoil wing in wings) {
				wing.isControllable = state;
			}
		}
		//CAMERA
		if (view != null) {
			view.isControllable = state;
			view.gameObject.SetActive (state);
			if (view.interiorcamera != null) {
				view.interiorcamera.gameObject.SetActive (state);
			}
		}
		//GEAR
		if (gearHelper != null) {
			gearHelper.isControllable = state;
		}
		//LIGHTS
		if (lightControl != null) {
			lightControl.isControllable = state;
		}
		//WING ACTUATOR
		if (wingActuator != null) {
			wingActuator.isControllable = state;
		}
		//SPEEDBRAKES
		if (speedBrakes != null) {
			speedBrakes.isControllable = state;
		}
		//FUEL SYSTEM
		if (engineType != EngineType.Electric && engineType != EngineType.Unpowered) {
			fuelsystem.isControllable = state;
		}
		//BLADES
		if (blades != null) {
			foreach (SilantroBlade blade in blades) {
				blade.isControllable = state;
			}
		}
		//ENGINES
		if (engineType == EngineType.Piston) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroPistonEngine piston in pistons) {
				piston.isControllable = state;
			}
		}
		if (engineType == EngineType.TurboJet) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboJet jet in turboJets) {
				jet.isControllable = state;
			}
		}
		if (engineType == EngineType.TurboFan) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboFan fan in turboFans) {
				fan.isControllable = state;
			}
		}
		if (engineType == EngineType.TurboProp) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboProp prop in turboProps) {
				prop.isControllable = state;
			}
		}
		if (engineType == EngineType.TurboShaft) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboShaft shaft in turboShafts) {
				shaft.isControllable = state;
			}
		}
		if (engineType == EngineType.Pegasus) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			pegasus.isControllable = state;
		}
		if (engineType == EngineType.Electric) {
			foreach (SilantroElectricMotor motor in motors) {
				motor.isControllable = state;
			}
		}
		//RADAR
		if (radarCore != null) {
			radarCore.isControllable = state;
		}
		//WEAPONS
		if (Armaments != null) {
			Armaments.isControllable = state;
		}


		//MAIN AIRCRAFT
		isControllable = state;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//TURN ENGINES ON OR OFF
	/// <summary>
	/// Sets the state of the engine.
	/// </summary>
	/// <param name="onState">If set to <c>true</c> engine starts.</param>
	/// <param name="offState">If set to <c>true</c> engine stops.</param>
	void SetEngineState(bool onState,bool offState) 
	{
		if (engineType == EngineType.Piston) {
			foreach (SilantroPistonEngine piston in pistons) {
				if(onState == true){piston.StartEngine ();}
				if(offState == true){piston.ShutDownEngine ();}
			}
		}
		if (engineType == EngineType.TurboJet) {
			foreach (SilantroTurboJet jet in turboJets) {
				if(onState == true){jet.StartEngine ();}
				if(offState == true){jet.ShutDownEngine ();}
			}
		}
		if (engineType == EngineType.TurboFan) {
			foreach (SilantroTurboFan fan in turboFans) {
				if(onState == true){fan.StartEngine ();}
				if(offState == true){fan.ShutDownEngine ();}
			}
		}
		if (engineType == EngineType.TurboProp) {
			foreach (SilantroTurboProp prop in turboProps) {
				if(onState == true){prop.StartEngine ();}
				if(offState == true){prop.ShutDownEngine ();}
			}
		}
		if (engineType == EngineType.TurboShaft) {
			foreach (SilantroTurboShaft shaft in turboShafts) {
				if(onState == true){shaft.StartEngine ();}
				if(offState == true){shaft.ShutDownEngine ();}
			}
		}
		if (engineType == EngineType.Pegasus) {
			if(onState == true){pegasus.StartEngine ();}
			if(offState == true){pegasus.ShutDownEngine ();}
		}
		if (engineType == EngineType.Electric) {
			foreach(SilantroElectricMotor motor in motors){
			if(onState == true){motor.StartEngine ();}
			if(offState == true){motor.ShutDownEngine ();}
			}
		}
	}
	//TURN ON
	public void TurnOnEngines()
	{
		SetEngineState (true, false);
	}
	//TURN OFF
	public void TurnOffEngines()
	{
		SetEngineState (false, true);
	}
	//READY AIRCRAFT FOR BATTLE :))
	public void StartAircraft()
	{
		if (aircraft != null && startMode == StartMode.Hot) {
			//POSITION AIRCRAFT
			PositionAircraft ();
			//SET ENGINE
			TurnOnEngines();throttleInput = 1f;
		}
	}

	//TURN ON
	public void TrimUp(){foreach (SilantroAerofoil foil in wings) {foil.LowerTrimTab (1);}}

	public void TrimDown(){foreach (SilantroAerofoil foil in wings) {foil.RaiseTrimTab (1);}}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENTER AIRCRAFT
	public void EnterAircraft()
	{
		if (!opened && !temp && !pilotOnboard && controlType == ControlType.Internal) {
			opened = true;temp = true;
			//OPEN CANOPY
			if (canopyActuator != null) {
				canopyActuator.open = true;
			}
			//SET THINGS UP
			pilotOnboard = true;
			StartCoroutine (EntryProcedure ());
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//EXIT AIRCRAFT
	public void ExitAircraft()
	{
		if (pilotOnboard && controlType == ControlType.Internal) {
			//EXIT CHECK LIST
			//1. SHUT ENGINES DOWN
			TurnOffEngines();
			//2. ACTIVATE BRAKE
			if (gearHelper != null) {
				gearHelper.ActivateBrake ();
			}
			//3. TURN OFF LIGHTS
			if (lightControl != null) {
				lightControl.TurnOffLight ();
			}
			//OPEN CANOPY
			if (canopyActuator != null) {
				canopyActuator.open = true;
			}
			//ACTUAL EXIT
			pilotOnboard = false;
			StartCoroutine (ExitProcedure ());
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void ResetScene()
	{
		Application.LoadLevel(this.gameObject.scene.name);
	}
	//STOVL
	//
	//TO VTOL
	public void VTOLTransition()
	{
		if (configuration == Configuration.F35Liftfan && turboFans[0].active == true) {
			//TURN OFF AFTERBURNER IF ACTIVE
			if (turboFans[0].afterburnerOperative) {turboFans[0].afterburnerOperative = false;}
			transitionToVTOL = true;
		}
		if (configuration == Configuration.AV8Harrier && pegasus.EngineOn == true) {
			transitionToVTOL = true;
		}
	}
	//TO STOL
	public void STOLTransition()
	{
		if (configuration == Configuration.F35Liftfan && turboFans[0].active == true) {
			//TURN OFF AFTERBURNER IF ACTIVE
			if (turboFans[0].afterburnerOperative) {turboFans[0].afterburnerOperative = false;}
			transitionToSTOL = true;
		}
		if (configuration == Configuration.AV8Harrier && pegasus.EngineOn == true) {
			transitionToSTOL = true;
		}
	}
	//TO NORMAL
	public void NormalTransition()
	{
		if (configuration == Configuration.F35Liftfan && turboFans[0].active == true) {
			//TURN ON AFTERBURNER TO BOOST SPEED 
			if (!turboFans[0].afterburnerOperative) {turboFans[0].afterburnerOperative = true;}
			transitionToNormal = true;
		}
		if (configuration == Configuration.AV8Harrier && pegasus.EngineOn == true) {
			transitionToNormal = true;
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PositionAircraft()
	{
		Vector3 initialPosition = aircraft.transform.position;
		aircraft.transform.position = new Vector3 (initialPosition.x, startAltitude, initialPosition.z);
		aircraft.velocity = new Vector3 (0, 0, startSpeed);
	}


	bool allOk;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (coreSystem != null && aircraft && wings.Length > 0) {
				allOk = true;
		}
		else if (aircraft == null) {
				Debug.LogError("Prerequisites not met on Aircraft "  + transform.name + ".... rigidbody not assigned");
				allOk = false;
		}
		else if (coreSystem == null) {
			Debug.LogError("Prerequisites not met on Aircraft "  + transform.name + ".... control module not assigned");
				allOk = false;
		}
		else if (engineType != EngineType.Unpowered || engineType != EngineType.Electric && fuelsystem == null) {
			Debug.LogError("Prerequisites not met on Aircraft "  + transform.name + ".... fuel System not assigned");
				allOk = false;
		}
		else if (wings.Length <= 0) {
			Debug.LogError("Prerequisites not met on Aircraft "  + transform.name + ".... aerofoil System not assigned");
			allOk = false;
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		InitializeAircraft ();
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void InitializeAircraft()
	{
		
		//--------------------------COLLECT COMPONENTS
		//1. CORE
		aircraft = GetComponent<Rigidbody> ();
		coreSystem = GetComponentInChildren<SilantroCore> ();
		controller = GetComponent<SilantroController> ();
		wings = GetComponentsInChildren<SilantroAerofoil>();
		//2. PERIPHERALS
		mainCamera = Camera.main;
		view = GetComponentInChildren<SilantroCamera> ();
		gearHelper = GetComponentInChildren<SilantroGearSystem> ();
		speedBrakes = GetComponentInChildren<SilantroSpeedBrakes> ();
		lightbulbs = GetComponentsInChildren<SilantroLight> ();
		lightControl = GetComponentInChildren<SilantroLightControl> ();
		wingActuator = GetComponentInChildren<SilantroWingActuator> ();
		hydraulics = GetComponentsInChildren<SilantroHydraulicSystem> ();
		nozzles = GetComponentsInChildren<SilantroNozzle> ();
		if (aircraftType == AircraftType.VTOL && configuration == Configuration.F35Liftfan) {liftfan = GetComponentInChildren<SilantroLiftFan> ();}
		if (engineType != EngineType.Unpowered && engineType != EngineType.Electric) {fuelsystem = gameObject.GetComponentInChildren<SilantroFuelDistributor>();
		}
		fuelTanks = GetComponentsInChildren<SilantroFuelTank> ();
		levers = GetComponentsInChildren<SilantroLever>();
		blackBox = GetComponentInChildren<SilantroDataLogger>();
		Armaments = GetComponentInChildren<SilantroArmament>();
		radarCore = GetComponentInChildren<SilantroRadar>();
		dials = GetComponentsInChildren<SilantroDial> ();
		turboProps = GetComponentsInChildren<SilantroTurboProp> ();
		motors = GetComponentsInChildren<SilantroElectricMotor> ();
		turboShafts = GetComponentsInChildren<SilantroTurboShaft> ();
		pegasus = GetComponentInChildren<SilantroPegasusEngine> ();
		boosters = GetComponentsInChildren<SilantroRocketMotor> ();
		pistons = GetComponentsInChildren<SilantroPistonEngine> ();
		blades = GetComponentsInChildren<SilantroBlade> ();
		turboJets = GetComponentsInChildren<SilantroTurboJet> ();
		turboFans = GetComponentsInChildren<SilantroTurboFan> ();
		payloadElements = GetComponentsInChildren<SilantroPayload> ();
		dragBodies = GetComponentsInChildren<SilantroBody> ();


		//--------------------------CONFIRM NEEDED COMPONENTS
		_checkPrerequisites ();


		if (allOk) {
			//--------------------------INITIALIZE COMPONENTS
			aircraftName = silantroAircraftName = transform.name;
			//0 DRAG
			initialDrag = aircraft.drag;

			//1. CORE SYSTEM
			coreSystem.aircraft = aircraft;
			coreSystem.controller = controller;
			coreSystem.InitializeCore ();
			if (coreSystem.ambientSimulation && coreSystem.weatherController != null) {
				weather = coreSystem.weatherController;
			}

			//2.WINGS
			totalWingArea = 0;List<SilantroAerofoil> baseWings = new List<SilantroAerofoil>();
			if (wings.Length > 0) {
				foreach (SilantroAerofoil wing in wings) {
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
						totalWingArea += wing.foilArea;
					}
					wing.calculator = controller;
					wing.connectedAircraft = aircraft;
					wing.coreSystem = coreSystem;
					wing.InitializeFoil ();
					if (flapfoil == null && wing.canUseFlap) {
						flapfoil = wing;
					}
					if (slatfoil == null && wing.canUseSlat) {
						slatfoil = wing;
					}
					//SEND WINGS TO CORE
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
						baseWings.Add (wing);
					}
				}
				if (baseWings.Count > 0 && coreSystem != null) {
					coreSystem.wings = baseWings;
				}
			} else {
				Debug.LogError ("No aerofoils attached to the aircraft");
				return;
			}

			//SEPARATE WINGS
			//1. BIPLANE
			if (wingType == WingType.Biplane) {
				foreach (SilantroAerofoil wing in wings) {
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
						if (wing.wingPosition == SilantroAerofoil.WingPosition.Top && wing.horizontalPosition == SilantroAerofoil.HorizontalPosition.Left) {
							leftTopWing = wing;
						}
						if (wing.wingPosition == SilantroAerofoil.WingPosition.Top && wing.horizontalPosition == SilantroAerofoil.HorizontalPosition.Right) {
							rightTopWing = wing;
						}
						if (wing.wingPosition == SilantroAerofoil.WingPosition.Bottom && wing.horizontalPosition == SilantroAerofoil.HorizontalPosition.Left) {
							leftBottomWing = wing;
						}
						if (wing.wingPosition == SilantroAerofoil.WingPosition.Bottom && wing.horizontalPosition == SilantroAerofoil.HorizontalPosition.Right) {
							rightBottomWing = wing;
						}
					}
				}
				if (leftTopWing != null && rightTopWing != null && leftBottomWing != null && rightBottomWing != null) {
					CalculateFoilSpan ();
				}
			}
			//2. MONOPLANE
			if (wingType == WingType.Monoplane) {
				foreach (SilantroAerofoil wing in wings) {
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
						if (wing.horizontalPosition == SilantroAerofoil.HorizontalPosition.Left) {
							leftWing = wing;
						}
						if (wing.horizontalPosition == SilantroAerofoil.HorizontalPosition.Right) {
							rightWing = wing;
						}
					}
				}
			}
			//
			//SEPARATE STABILIZERS
			//1.SPLIT
			if (stabilizerType == StabilizerType.Split) {
				foreach (SilantroAerofoil wing in wings) {
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
						if (wing.stabOrientation == SilantroAerofoil.StabilizerOrientation.Horizontal && wing.stabilizerPosition == SilantroAerofoil.StabilizerPosition.Left) {
							leftStabilizer = wing;
						}
						if (wing.stabOrientation == SilantroAerofoil.StabilizerOrientation.Horizontal && wing.stabilizerPosition == SilantroAerofoil.StabilizerPosition.Right) {
							rightStabilizer = wing;
						}
					}
				}
			}
			//2.SINGLE
			if (stabilizerType == StabilizerType.Unison) {
				foreach (SilantroAerofoil wing in wings) {
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
						if (wing.stabOrientation == SilantroAerofoil.StabilizerOrientation.Horizontal && wing.stabilizerPosition == SilantroAerofoil.StabilizerPosition.Center) {
							centerStabilizer = wing;
						}
					}
				}
			}
			//3. CANARDS
			if (stabilizerType == StabilizerType.Canard) {
				foreach (SilantroAerofoil wing in wings) {
					if (wing.aerofoilType == SilantroAerofoil.AerofoilType.Canard) {
						if (wing.canardPosition == SilantroAerofoil.CanardPosition.Left) {
							leftStabilizer = wing;
						}
						if (wing.canardPosition == SilantroAerofoil.CanardPosition.Right) {
							rightStabilizer = wing;
						}
					}
				}
			}
			if (slatfoil == null && rightWing != null) {
				slatfoil = rightWing;
			}
			if (flapfoil == null && rightWing != null) {
				flapfoil = rightWing;
			}
		

			//3. ENGINES
			if (engineType != EngineType.Unpowered) {
				//SETUP PISTON AIRCRAFT
				if (engineType == EngineType.Piston) {
					AvailableEngines = pistons.Length;
					silantroEngineName = pistons [0].engineIdentifier;
					foreach (SilantroPistonEngine piston in pistons) {
						piston.EMU = coreSystem;
						piston.fuelSystem = fuelsystem;
						piston.controller = controller;
						piston.connectedAircraft = aircraft;
						if (startMode == StartMode.Hot) {
							piston.engineStartMode = SilantroPistonEngine.EngineStartMode.Hot;
						} else {
							piston.engineStartMode = SilantroPistonEngine.EngineStartMode.Cold;
						}
						piston.InitializeEngine ();
					}
				}
				//SETUP TURBO JET
				if (engineType == EngineType.TurboJet) {
					AvailableEngines = turboJets.Length;
					silantroEngineName = turboJets [0].engineIdentifier;
					foreach (SilantroTurboJet jet in turboJets) {
						jet.computer = coreSystem;
						jet.fuelSystem = fuelsystem;
						jet.controller = controller;
						jet.connectedAircraft = aircraft;
						if (startMode == StartMode.Hot) {
							jet.engineStartMode = SilantroTurboJet.EngineStartMode.Hot;
						} else {
							jet.engineStartMode = SilantroTurboJet.EngineStartMode.Cold;
						}
						jet.InitializeEngine ();
					}
				}
				//SETUP TURBO FAN
				if (engineType == EngineType.TurboFan) {
					AvailableEngines = turboFans.Length;
					silantroEngineName = turboFans [0].engineIdentifier;
					foreach (SilantroTurboFan fan in turboFans) {
						fan.computer = coreSystem;
						fan.fuelSystem = fuelsystem;
						fan.controller = controller;
						fan.connectedAircraft = aircraft;
						if (startMode == StartMode.Hot) {
							fan.engineStartMode = SilantroTurboFan.EngineStartMode.Hot;
						} else {
							fan.engineStartMode = SilantroTurboFan.EngineStartMode.Cold;
						}
						fan.InitializeEngine ();
					}
				}

				//LIFTFAN
				if (liftfan != null) {
					liftfan.connectedAircraft = aircraft;
					liftfan.controller = controller;
					liftfan.InitializeEngine ();
				}

				//SETUP TUROB PROP
				if (engineType == EngineType.TurboProp) {
					AvailableEngines = turboProps.Length;
					silantroEngineName = turboProps [0].engineIdentifier;
					foreach (SilantroTurboProp prop in turboProps) {
						prop.computer = coreSystem;
						prop.fuelSystem = fuelsystem;
						prop.controller = controller;
						prop.connectedAircraft = aircraft;
						if (startMode == StartMode.Hot) {
							prop.engineStartMode = SilantroTurboProp.EngineStartMode.Hot;
						} else {
							prop.engineStartMode = SilantroTurboProp.EngineStartMode.Cold;
						}
						prop.InitializeEngine ();
					}
				}
				//SETUP ELECTRIC MOTOR
				if (engineType == EngineType.Electric) {
					AvailableEngines = motors.Length;
					//silantroEngineName = motors [0].engineIdentifier;
					foreach (SilantroElectricMotor motor in motors) {
						motor.EMU = coreSystem;
						motor.Parent = aircraft;
						motor.controller = controller;
						motor.InitializeMotor ();
					}
				}
				//SETUP TURBO SHAFT
				if (engineType == EngineType.TurboShaft) {
					AvailableEngines = turboShafts.Length;
					silantroEngineName = turboShafts [0].engineIdentifier;
					foreach (SilantroTurboShaft shaft in turboShafts) {
						shaft.EMU = coreSystem;
						shaft.fuelSystem = fuelsystem;
						shaft.controller = controller;
						shaft.connectedAircraft = aircraft;
						shaft.InitializeEngine ();
						if (startMode == StartMode.Hot) {
							shaft.engineStartMode = SilantroTurboShaft.EngineStartMode.Hot;
						} else {
							shaft.engineStartMode = SilantroTurboShaft.EngineStartMode.Cold;
						}
					}
				}
				//SETUP HARRIER
				if (engineType == EngineType.Pegasus) {
					AvailableEngines = 1;
					pegasus.computer = coreSystem;
					pegasus.InitializeEngine ();
					silantroEngineName = pegasus.engineIdentifier;
					pegasus.fuelSystem = fuelsystem;
					if (startMode == StartMode.Hot) {
						pegasus.engineStartMode = SilantroPegasusEngine.EngineStartMode.Hot;
					} else {
						pegasus.engineStartMode = SilantroPegasusEngine.EngineStartMode.Cold;
					}
				}
			}

			//JATO
			if (jatoState == JATOState.Available) {
				if (boosters.Length > 0) {
					foreach (SilantroRocketMotor rocket in boosters) {
						if (rocket.boosterType == SilantroRocketMotor.BoosterType.Aircraft) {
							rocket.InitializeRocket ();
						} else {
							Debug.Log ("Booster " + rocket.transform.name + " is not configured properly");
						}
					}
				}
			}


			//SETUP NOZZLES
			if (nozzles.Length > 0) {
				foreach (SilantroNozzle nozzle in nozzles) {
					nozzle.InitializeNozzle ();
				}
			}

			//SETUP BLADES
			if (blades.Length > 0) {
				foreach (SilantroBlade blade in blades) {
					blade.airplane = aircraft;
				}
			}

			//SETUP BODIES
			if (dragBodies.Length > 0) {
				foreach (SilantroBody body in dragBodies) {
					body.aircraft = aircraft;
					body.controller = controller;
					body.InitializeBody ();
				}
			}

			//CAMERA
			if (view != null) {
				view.controller = controller;
				view.InitializeCamera ();
				//SEND PILOT GAMEOBJECT TO CAMERA
				if (interiorPilot != null) {view.interiorPilot = interiorPilot;}
			}
				
			//BLADES
			if (blades.Length > 0) {
				foreach (SilantroBlade blade in blades) {
					blade.InitializeBlade ();
				}
			}

			//LIGHTS
			if (lightbulbs != null && lightControl != null) {
				lightControl.lights = lightbulbs;
				lightControl.InitializeSwitch ();
			}

			//SETUP DIALS
			if (dials != null) {
				foreach (SilantroDial dial in dials) {
					dial.Aircraft = aircraft.transform;
					dial.dataLog = coreSystem;
					dial.controller = controller;
					dial.InitializeDial ();
				}
			}

			//SETUP FUEL SYSTEM
			if (fuelsystem != null) {
				fuelsystem.fuelTanks = fuelTanks;
				fuelsystem.controller = controller;
				fuelsystem.InitializeDistributor ();
			}


			//SETUP HYDRAULICS
			if (hydraulics.Length > 0) {
				foreach (SilantroHydraulicSystem actuator in hydraulics) {
					actuator.InitializeActuator ();
				}
			}

			//SETUP SPEEDBRAKE
			if (speedBrakes) {
				speedBrakes.InitializeSpeedBrakes ();
			}

			//SETUP WING SWING
			if (wingActuator) {
				wingActuator.InitializeWingActuator ();
			}

			//SEND AVAILABLE WINGS TO GEAR SYSTEM
			if (gearHelper != null) {
				gearHelper.foils = wings;
				gearHelper.aircraft = aircraft;
				gearHelper.lights = lightbulbs;
				gearHelper.controller = controller;
				gearHelper.InitializeGear();
			}

			//SETUP LEVERS
			if (levers != null) {
				foreach (SilantroLever lever in levers) {
					lever.controller = controller;
					lever.gearSystem = gearHelper;
					lever.InitializeLever ();
					if (wingType == WingType.Monoplane) {
						lever.wingInput = rightWing;
					} else if (wingType == WingType.Biplane) {
						lever.wingInput = rightTopWing;
					}
				}
			}
		
			//SETUP ENTER_EXIT
			if (controlType == ControlType.Internal) {
				//SET EXIT POINT
				if (!getOutPosition) {
					GameObject getOutPos = new GameObject ("Get Out Position");
					getOutPos.transform.SetParent (transform);
					getOutPos.transform.localPosition = new Vector3 (-3f, 0f, 0f);
					getOutPos.transform.localRotation = Quaternion.identity;
					getOutPosition = getOutPos.transform;
				}
				//DISABLE PILOT
				if (interiorPilot != null) {
					interiorPilot.SetActive (false);
				}
				//COLLECT DOOR DATA
				if (canopyActuator != null) {
					openTime = canopyActuator.openTime + 1f;
					closeTime = canopyActuator.closeTime + 1f;
					;
				}//WITH ACTUATOR
				else {
					openTime = 1.5f;
					closeTime = 1.3f;
				}//WITHOUT ACTUATOR
			}

			//SETUP DATA BOX
			if (blackBox != null) {
				blackBox.source = controller;
				blackBox.InitializeLog ();
			}

			//SETUP RADAR
			if (radarCore != null) {
				radarCore.connection = this.gameObject;
				radarCore.connectedAircraft = controller;
				radarCore.InitializeRadar ();
			}

			//SETUP WEAPONS
			if (Armaments != null) {
				//STORE FOR REARMING
				GameObject armamentBox = Armaments.gameObject;
				ArmamentsStorage = GameObject.Instantiate (armamentBox, Armaments.transform.position, Armaments.transform.rotation,this.transform);
				ArmamentsStorage.SetActive (false);ArmamentsStorage.name = "Hardpoints(Storage)";
				if (radarCore != null) {
					Armaments.connectedRadar = radarCore;
				}
				if(coreSystem){coreSystem.armament = Armaments;}
				Armaments.InitializeWeapons ();
			}
		
		
			silantroEngineCount = AvailableEngines;
			silantroEngineType = engineType.ToString ();
			silantroAircraftName = aircraftName;
	
			//DETERMINE CONTROL TYPE
			if (controlType == ControlType.Internal) {
				SetControlState (false);
				//DISABLE CANVAS
				if (canvasDisplay != null) {
					canvasDisplay.gameObject.SetActive (false);
				}
			} else {
				SetControlState (true);
				//DEACTIVATE MAIN CAMERA
				if (mainCamera != null) {
					mainCamera.gameObject.SetActive (false);//REMOVE THIS IF YOU"RE USING THE MAIN CAMERA FOR SOMETHING ELSE
				}
			}
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void OnDrawGizmos()
	{
		if (wingType == WingType.Biplane) {
			Gizmos.color = Color.yellow;
			if (rightTopWing != null && rightBottomWing != null) {
				Gizmos.DrawLine (rightTopWing.transform.position, rightBottomWing.transform.position);
			}
			Gizmos.color = Color.red;
			if (leftTopWing != null && leftBottomWing != null) {
				Gizmos.DrawLine (leftTopWing.transform.position, leftBottomWing.transform.position);
			}
		}
		//SET AIRCRAFT NAME
		aircraftName = transform.name;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void CalculateFoilSpan () {
		//.BIPLANE
		if (wingType == WingType.Biplane) {
			Vector3 leftTop = leftTopWing.TipChordTrailing;
			Vector3 leftBottom = leftBottomWing.TipChordTrailing;
			Vector3 rightTop = rightTopWing.TipChordTrailing;
			Vector3 rightBottom = rightBottomWing.TipChordTrailing;
			//
			upperWingSpan = Vector3.Distance (leftTop, rightTop);
			lowerWingSpan = Vector3.Distance (leftBottom, rightBottom);
			wingGap = Vector3.Distance (leftTopWing.transform.position, leftBottomWing.transform.position);
		}
	}







	//REARM WEAPONS--------------------------------------------------------------------------------------------------------------------------------------------------
	public void RefreshWeapons()
	{
		//SNAPSHOT OF CURRENT POINT
		GameObject oldPod = Armaments.gameObject;int currentWeapon = Armaments.selectedWeapon;
		GameObject newPod = GameObject.Instantiate (ArmamentsStorage, Armaments.transform.position, Armaments.transform.rotation, this.transform);
		SilantroArmament newArmament = newPod.GetComponent<SilantroArmament> ();newPod.name = "Hardpoints";
		//REMOVE AND REPLACE
		Armaments = newArmament;newPod.SetActive(true);
		Destroy (oldPod);
		//RE-INITIALIZE
		if (radarCore != null) {
			Armaments.connectedRadar = radarCore;
		}
		if(coreSystem){coreSystem.armament = Armaments;}
		//SET VARIABLES
		Armaments.isControllable = isControllable;
		Armaments.InitializeWeapons ();
		Armaments.SelectWeapon (currentWeapon);
		//
		Debug.Log ("Rearm Complete!!");
	}





	//-------------------------------------------------------------------------
	//------------------------------COLLECT INPUTS-----------------------------
	//-------------------------------------------------------------------------
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SET INPUTS VALUES FOR THE AIRCRAFT COMPONEnt
	void CollectInputs()
	{
		if (inputType == InputType.Default) {
			//COLLECT INPUT VARIABLES
			//1. MAIN
			pitchInput = Input.GetAxis ("Pitch");
			rollInput = Input.GetAxis ("Roll");
			yawInput = Input.GetAxis ("Rudder");
			//2. ENGINES
			float baseInput = (1 + Input.GetAxis ("Throttle"));
			throttleInput = baseInput / 2f;
			//2.1 START AND STOP
			if(Input.GetButtonDown("Start Engine")){
				TurnOnEngines ();
			}
			if(Input.GetButtonDown("Stop Engine")){
				TurnOffEngines ();
			}
			//3. BRAKES
			if (gearHelper != null) {
				//1. INCREMENTAL BRAKE
				if (Input.GetButton ("Brake Lever")) {
					brakeControl = Mathf.Lerp (brakeControl, 1f, 0.1f);
				} else {
					brakeControl = Mathf.Lerp (brakeControl, 0f, 0.1f);
				}
				//2. PARKING BRAKE
				if (Input.GetButtonDown ("Parking Brake")) {
					gearHelper.ToggleBrake ();
				}
			}
			//4. SET TCS
			if (Input.GetKeyDown (KeyCode.Q)){ 
				foreach (SilantroAerofoil wing in wings) {
					//wing.ToggleTCS ();
				}
			}
			//5. FLAP INCREASE
			if (Input.GetButtonDown ("Extend Flap")){ 
				foreach (SilantroAerofoil wing in wings) {
					wing.LowerFlap ();
				}
			}
			//5.1 FLAP DECREASE
			if (Input.GetButtonDown ("Retract Flap")){ 
				foreach (SilantroAerofoil wing in wings) {
					wing.RaiseFlap ();
				}
			}
			//6. SLAT DECREASE
			if (Input.GetButtonDown ("Actuate Slat")){ 
				foreach (SilantroAerofoil wing in wings) {
					wing.ActuateSlat ();
				}
			}
			//7. SPOILER DECREASE
			if (Input.GetButtonDown ("Spoiler")){ 
				foreach (SilantroAerofoil wing in wings) {
					wing.ActuateSpoilers ();
				}
			}
			//8. CHANGE CAMERA
			if (Input.GetKeyDown (KeyCode.C)){ 
				if (view != null) {
					view.ToggleCamera ();
				}
			}
			//9. TOGGLE GEAR
			if(Input.GetButtonDown("Actuate Gear"))
			{
				if (gearHelper != null) {
					gearHelper.ToggleGear ();
				}
			}
			//10. TOGGLE SPEEDBRAKES
			if (Input.GetButtonDown("Speed Brake")) {
				if (speedBrakes != null) {
					speedBrakes.ToggleSpeedBrake ();
				}
			}
			//11. TOGGLE LIGHT
			if(Input.GetButtonDown("LightSwitch"))
			{
				if (lightControl != null) {
					lightControl.ToggleLight ();
				}
			}
			//12. TOGGLE AFTERBURNER
			if(Input.GetButtonDown("Afterburner"))
			{
				//TURBO JET
				if (engineType == EngineType.TurboJet) {
					foreach (SilantroTurboJet engine in turboJets) {
						engine.ToggleAfterburner ();
					}
				}
				//TURBO FAN
				if (engineType == EngineType.TurboFan) {
					foreach (SilantroTurboFan engine in turboFans) {
						engine.ToggleAfterburner ();
					}
				}
			}
			//12.5 TOGGLE JATO ROCKETS
			if (Input.GetButtonDown ("Afterburner")) {
				if (jatoState == JATOState.Available) {
					if (boosters.Length > 0) {
						foreach (SilantroRocketMotor motor in boosters) {
							motor.FireRocket ();
						}
					}
				}
			}
			//13. TOGGLE REVERSE THRUST
			if(Input.GetButtonDown("Reverse Thrust"))
			{
				if (engineType == EngineType.TurboFan) {
					foreach (SilantroTurboFan engine in turboFans) {
						//engine.InitiateReverseControl ();
					}
				}
			}
			//14. SWING WING
			if (Input.GetKeyDown (KeyCode.J)){ 
				if (wingActuator != null ){
					if (leftWing.flapDeflection < 1) {
						if (wingActuator.currentState == SilantroWingActuator.CurrentState.Closed) {
							wingActuator.SwingWings ();
						} else {
							wingActuator.ExtendWings ();
						}
					} else {
						Debug.Log ("Lower Flap angle to swing wings");
					}
				}
			}
			//15. RELOAD SCENE
			if (Input.GetKeyDown (KeyCode.R)) { 
				ResetScene ();
			}
			//16. EXIT AIRCRAFT
			if (controlType == ControlType.Internal && !temp && pilotOnboard && Input.GetKeyDown(KeyCode.F)) {
				//EXIT CONDITION
				if (isGrounded && coreSystem.currentSpeed < 10) {
					ExitAircraft ();opened = false;temp = false;
				}
			}
			//17. GO TO VTOL
			if(Input.GetButtonDown("Transition To VTOL"))
			{
				if (aircraftType == AircraftType.VTOL) {
					VTOLTransition ();
				}
			}
			//18. GO TO STOL
			if(Input.GetButtonDown("Transition To STOL"))
			{
				if (aircraftType == AircraftType.VTOL) {
					STOLTransition ();
				}
			}
			//19. GO TO Normal
			if(Input.GetButtonDown("Transition To Normal"))
			{
				if (aircraftType == AircraftType.VTOL) {
					NormalTransition ();
				}
			}
			//
			//WEAPONS
			//

			if (Armaments != null) {
				//20 SHOOT GUN
				if (Input.GetButton ("Fire")) {
					//FIRE GUN IF SELECTED
					if (Armaments.currentWeapon == "Gun") {
						Armaments.FireGuns ();
					}
					//SPIN BARREL
					foreach (SilantroGun gun in Armaments.attachedGuns) {
						gun.running = true;
					}
				} else {
					//STOP BARREL
					foreach (SilantroGun gun in Armaments.attachedGuns) {
						gun.running = false;
					}
				}
				//
				//21. ROCKETS
				if (Input.GetButton ("Weapon Release") && Input.GetKeyDown (KeyCode.Alpha1)) {
					Armaments.FireRocket ();
				}
				//
				//22. MISSILES
				if (Input.GetButton ("Weapon Release") && Input.GetKeyDown (KeyCode.Alpha2)) {
					Armaments.FireMissile ();
				}
				//
				//23. BOMBS
				if (Input.GetButton ("Weapon Release") && Input.GetKeyDown (KeyCode.Alpha3)) {
					Armaments.DropBomb ();
				}
				//24. FIRE SELECTED WEAPON
				if (Input.GetButtonDown ("Fire")) {
					//MISSILE
					if (Armaments.currentWeapon == "Missile") {
						Armaments.FireMissile ();
					}
					//ROCKETS
					if (Armaments.currentWeapon == "Rocket") {
						Armaments.FireRocket ();
					}
				}
				//25. SELECT WEAPON
				if (Input.GetButtonDown ("Weapon Select")) {
					Armaments.ChangeWeapon ();
				}
			}
			//
			if (radarCore != null) {
				//22. SELECT UPPER TARGET
				if (Input.GetButtonDown ("Target Up")) {
					radarCore.SelectedUpperTarget ();
				}
				//23. SELECT LOWER TARGET
				if (Input.GetButtonDown ("Target Down")) {
					radarCore.SelectLowerTarget ();
				}
				//24. LOCK SELECTED TARGET
				if (Input.GetButtonDown ("Target Lock")) {
					radarCore.LockSelectedTarget ();
				}
				//25. RELEASE LOCK
				if (Input.GetKeyDown (KeyCode.Backspace)) {
					radarCore.ReleaseLockedTarget ();
				}
			}

		} 

		//2. 
		if(inputType == InputType.Custom) {
			//PUT CUSTOM CONTROL CODE HERE

		}
	}






	//-------------------------------------------------------------------------
	//------------------------------PROCESS INPUTS-----------------------------
	//-------------------------------------------------------------------------
	[HideInInspector]public float pitchStiffness;
	[HideInInspector]public float rollStiffness;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void ProcessInputs()
	{
		//INTRODUCE STIFFNESS
		pitchStiffness = 1;//((-0.1f*coreSystem.currentSpeed)+100f)/100f;//Efficiency drops by 5% for each 50 Knots
		pitchInput *= pitchStiffness;//Reduces the maximum deflection of the surface
		//ROLL
		rollStiffness = 1;//((-0.05f*coreSystem.currentSpeed)+100f)/100f;//Efficiency drops by 5% for each 100 Knots
		rollInput  *= rollStiffness;

		//DAMPEN CONTROLS FOR A SMOOTH FEEL
		tempRoll = Mathf.Lerp(tempRoll,rollInput,(rollSensitivity/controlFactor)*rollStiffness);//Reduces the deflection speed of the surface
		tempPitch = Mathf.Lerp(tempPitch,pitchInput,(pitchSensitivity/controlFactor)*pitchStiffness) ; 
		tempYaw = Mathf.Lerp(tempYaw,yawInput,pitchSensitivity/controlFactor);

		//SEND DATA TO WINGS
		foreach (SilantroAerofoil wing in wings) {
			if (wing.controlState == SilantroAerofoil.ControType.Controllable) {
				wing.rollInput = tempRoll;
				wing.pitchInput = tempPitch;
				wing.yawInput = tempYaw;
			}
		}
		//SEND DATA TO ENGINES
		if (engineType == EngineType.Piston) {
			foreach (SilantroPistonEngine engine in pistons) {
				engine.FuelInput = throttleInput;
			}
		}
		if (engineType == EngineType.TurboJet) {
			foreach (SilantroTurboJet engine in turboJets) {
				engine.FuelInput = throttleInput;
			}
		}
		if (engineType == EngineType.TurboFan) {
			foreach (SilantroTurboFan engine in turboFans) {
				engine.FuelInput = throttleInput;
			}
		}
		if (engineType == EngineType.TurboProp) {
			foreach (SilantroTurboProp engine in turboProps) {
				engine.FuelInput = throttleInput;
			}
		}
		if (engineType == EngineType.TurboShaft) {
			foreach (SilantroTurboShaft engine in turboShafts) {
				engine.FuelInput = throttleInput;
			}
		}
		if (engineType == EngineType.Electric) {
			foreach (SilantroElectricMotor engine in motors) {
				engine.powerInput = throttleInput;
			}
		}
		if (engineType == EngineType.Pegasus) {
			pegasus.FuelInput = throttleInput;
		}
		//SEND BRAKE DATA
		if (gearHelper) {
			gearHelper.brakeInput = brakeControl;
			gearHelper.steerInput = yawInput;
		}
		//SEND NOZZLE DATA
		if (nozzles != null) {
			foreach (SilantroNozzle nozzle in nozzles) {
				nozzle.throttleInput = throttleInput;
				nozzle.elevatorInput = tempPitch;
				nozzle.aileronInput = tempRoll;
				nozzle.rudderInput = tempYaw;
			}
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CollectData()
	{
		//PROCESS ENGINES
		totalConsumptionRate = 0f;
		//TURBO JET
		if (engineType == EngineType.TurboJet) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboJet turbojet in turboJets) {
				totalThrustGenerated += turbojet.EngineThrust;
				totalConsumptionRate += turbojet.actualConsumptionrate;
			}
			//COLLECT MAIN ENGINE DATA
			SilantroTurboJet sampleEngine = turboJets [0];
			float coreFactor = sampleEngine.coreFactor;
			silantroEnginePower = sampleEngine.corePower*coreFactor;
			if (sampleEngine.active) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//TURBO FAN
		if (engineType == EngineType.TurboFan) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboFan turbofan in turboFans) {
				totalThrustGenerated += turbofan.EngineThrust;
				totalConsumptionRate += turbofan.actualConsumptionrate;
			}
			//COLLECT MAIN ENGINE DATA
			SilantroTurboFan sampleEngine = turboFans [0];
			float coreFactor = sampleEngine.coreFactor;
			silantroEnginePower = sampleEngine.corePower*coreFactor;
			if (sampleEngine.active) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//PISTON
		if (engineType == EngineType.Piston) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroPistonEngine piston in pistons) {
				totalConsumptionRate += piston.actualConsumptionrate;
			}
			foreach (SilantroBlade blade in blades) {
				totalThrustGenerated += blade.Thrust;
			}
			//COLLECT MAIN ENGINE DATA
			SilantroPistonEngine sampleEngine = pistons [0];
			float coreFactor = sampleEngine.coreRPM / sampleEngine.norminalRPM;
			silantroEnginePower = sampleEngine.corePower*coreFactor;
			silantroN1 = sampleEngine.coreRPM*coreFactor;silantroN2 = silantroN1;
			if (sampleEngine.active) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//ELECTRIC
		if (engineType == EngineType.Electric) {
			totalThrustGenerated = 0;
			foreach (SilantroBlade blade in blades) {
				totalThrustGenerated += blade.Thrust;
			}

		}
		//TURBO SHAFT
		if (engineType == EngineType.TurboShaft) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboShaft shaft in turboShafts) {
				totalConsumptionRate += shaft.actualConsumptionrate;
			}
			foreach (SilantroBlade blade in blades) {
				totalThrustGenerated += blade.Thrust;
			}
			//COLLECT MAIN ENGINE DATA
			SilantroTurboShaft sampleEngine = turboShafts [0];
			float coreFactor = sampleEngine.coreFactor;
			silantroEnginePower = sampleEngine.corePower*coreFactor;
			silantroN1 = sampleEngine.coreRPM*coreFactor;//silantroN2 = sampleEngine.HighPressureFanRPM*coreFactor;
			if (sampleEngine.active) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//PEGASUS
		if (engineType == EngineType.Pegasus) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			totalThrustGenerated += pegasus.EngineThrust;
			totalConsumptionRate += pegasus.actualConsumptionrate;
			//COLLECT MAIN ENGINE DATA
			SilantroPegasusEngine sampleEngine = pegasus;
			float coreFactor = sampleEngine.CurrentRPM / sampleEngine.LowPressureFanRPM;
			silantroEnginePower = sampleEngine.enginePower*coreFactor;
			silantroN1 = sampleEngine.LowPressureFanRPM*coreFactor;silantroN2 = sampleEngine.HighPressureFanRPM*coreFactor;
			if (sampleEngine.EngineOn) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//TURBO PROP
		if (engineType == EngineType.TurboProp) {
			totalThrustGenerated = 0;totalConsumptionRate = 0;
			foreach (SilantroTurboProp prop in turboProps) {
				totalConsumptionRate += prop.actualConsumptionrate;
			}
			foreach (SilantroBlade blade in blades) {
				totalThrustGenerated += blade.Thrust;
			}
			SilantroTurboProp sampleEngine = turboProps [0];
			float coreFactor = sampleEngine.coreFactor;
			silantroEnginePower = sampleEngine.corePower*coreFactor;
			if (sampleEngine.active) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//ELECTRIC
		if (engineType == EngineType.Electric) {
			totalThrustGenerated = 0;
			foreach (SilantroBlade blade in blades) {
				totalThrustGenerated += blade.Thrust;
			}
			SilantroElectricMotor sampleEngine = motors [0];
			//float coreFactor = sampleEngine.coreFactor;
			//silantroEnginePower = sampleEngine.enginePower*coreFactor;
			if (sampleEngine.EngineOn) {silantroActive = true;} 
			else {silantroActive = false;}
		}
		//
		if (engineType != EngineType.Electric && engineType != EngineType.Unpowered) {
			fuelsystem.totalConsumptionRate = totalConsumptionRate;
		}
		//
		//PROCESS WEIGHT
		currentWeight = 0f;
		if (engineType != EngineType.Electric && engineType != EngineType.Unpowered) {
			currentWeight = emptyWeight + fuelsystem.TotalFuelRemaining;
		} else {
			currentWeight = emptyWeight;
		}
		if (Armaments != null) {
			currentWeight += Armaments.weaponsLoad;
		}


		//-----PAYLOAD
		//1.REFRESH
		payloadElements = GetComponentsInChildren<SilantroPayload> ();
		//2. RECORD
		if (payloadElements.Length > 0) {
			foreach (SilantroPayload element in payloadElements) {
				currentWeight += element.weight;
			}
		}




		//-----JATO
		if (jatoState == JATOState.Available) {
			totalBoost = 0f;
			foreach (SilantroRocketMotor rocket in boosters) {
				totalBoost += rocket.Thrust;
			}
		}
		//
		if(aircraft != null)aircraft.mass = currentWeight;
		if (currentWeight > maximumWeight) {
			Debug.Log ("Aircraft is too Heavy for takeoff, Dump some Fuel...");
		}
		additionalDrag = 0;
		if (hydraulics != null) {
			foreach (SilantroHydraulicSystem hydraulic in hydraulics) {
				if (hydraulic.generatesDragWhenOpened && hydraulic != null) {
					additionalDrag += hydraulic.dragAmount;
				}
			}
		}
		//COLLECT DRAG
		if (additionalDrag <= 0) {
			additionalDrag = 0;
		}
		totalDrag = initialDrag + additionalDrag;
		aircraft.drag = totalDrag;
		//CALCULATE DATA
		wingLoading = currentWeight/totalWingArea;
		thrustToWeightRatio = totalThrustGenerated / (currentWeight*9.81f);
	}







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENTER AIRCRAFT
	IEnumerator EntryProcedure()
	{
		yield return new WaitForSeconds (openTime);
		//PLAYER STATE
		if (player != null) {
			player.SetActive (false);if (interiorPilot != null) {interiorPilot.SetActive (true);}
			player.transform.SetParent (transform);
			player.transform.localPosition = Vector3.zero;player.transform.localRotation = Quaternion.identity;
			//ENABLE CONTROLS
			if (playerType == PlayerType.FirstPerson) {
				EnableFPControls ();
			}
			if (playerType == PlayerType.ThirdPerson) {
				StartCoroutine (EnableTPControls ());
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//FIRST PERSON
	void EnableFPControls()
	{
		//CLOSE CANOPY
		if (canopyActuator != null) {
			canopyActuator.close = true;
		}
		SetControlState(true);
		temp = false;
		//
		//DISABLE MAIN CAMERA USED BY PLAYER NOTE: YOU MIGHT HAVE TO SET YOUR OWN CONDITION DEPENDING ON THE PLAYER CONTROLLER
		if (mainCamera != null) {
			mainCamera.gameObject.SetActive (false);
			mainCamera.enabled = false;
			mainCamera.gameObject.GetComponent<AudioListener> ().enabled = false;
		}
		//ENABLE CANVAS
		if (canvasDisplay != null) {
			canvasDisplay.gameObject.SetActive (true);
			canvasDisplay.connectedAircraft = controller;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//THIRD PERSON 
	IEnumerator EnableTPControls()
	{
		yield return new WaitForSeconds (openTime);
		//CLOSE CANOPY
		if (canopyActuator != null) {
			canopyActuator.close = true;
		}
		SetControlState(true);
		temp = false;
		//
		//DISABLE MAIN CAMERA USED BY PLAYER NOTE: YOU MIGHT HAVE TO SET YOUR OWN CONDITION DEPENDING ON THE PLAYER CONTROLLER
		if (mainCamera != null) {
			mainCamera.gameObject.SetActive (false);
			mainCamera.enabled = false;
			mainCamera.gameObject.GetComponent<AudioListener> ().enabled = false;
		}
		//ENABLE CANVAS
		if (canvasDisplay != null) {
			canvasDisplay.gameObject.SetActive (true);
			canvasDisplay.connectedAircraft = controller;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//EXIT AIRCRAFT
	IEnumerator ExitProcedure()
	{
		yield return new WaitForSeconds (closeTime);

		//PLAYER STATE
		if (player != null) {
			if (interiorPilot != null) {interiorPilot.SetActive (false);}
			player.transform.SetParent (null);
			player.transform.position = getOutPosition.position;
			player.transform.rotation = getOutPosition.rotation;
			player.transform.rotation = Quaternion.Euler (0f, player.transform.eulerAngles.y, 0f);
			player.SetActive (true);
			////SET CAMERA FOR FIRST PERson
			if (playerType == PlayerType.FirstPerson) {
				view.gameObject.SetActive(false);
				view.interiorcamera.gameObject.SetActive(false);
			}
			//DISABLE CANVAS
			if (canvasDisplay != null) {
				canvasDisplay.connectedAircraft = null;
				canvasDisplay.gameObject.SetActive (false);
			}
			//CLOSE DOOR
			if (canopyActuator != null) {
				canopyActuator.close = true;
			}
			//ENABLE MAIN CAMERA USED BY PLAYER>>> NOTE: YOU MIGHT HAVE TO SET YOUR OWN CONDITION DEPENDING ON THE PLAYER CONTROLLER
			if (mainCamera != null) {
				mainCamera.gameObject.SetActive (true);
				mainCamera.enabled = true;
				mainCamera.gameObject.GetComponent<AudioListener> ().enabled = true;
			}

			//
			//DISABLE 1. CAMERA
			if (view != null) {
				view.isControllable = false;
				view.gameObject.SetActive (false);
				if (view.interiorcamera != null) {
					view.interiorcamera.gameObject.SetActive (false);
				}
			}
			////DISABLE CONTROLS
			DelayState();
		}
	}


	//---------------CONTROL DELAY----TO WAIT FOR EXIT CHECKLIST TO BE DONE
	void DelayState(){StartCoroutine(RecieveDelay());}
	IEnumerator RecieveDelay()
	{
		yield return new WaitForSeconds (5f);
		SetControlState(false);
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CORE
	void Update()
	{
		if (allOk && isControllable) {
			//COLLECT DATA
			CollectData ();
			//COLLECT INPUT
			CollectInputs ();
			//PROCESS INPUTS
			ProcessInputs ();
			//CHECK GROUND
			if (gearHelper != null && gearHelper.wheelSystem.Count > 0) {
				WheelCollider examWheel = gearHelper.wheelSystem [0].collider;
				isGrounded = examWheel.isGrounded;
			}
			//SEND STOVL DATA
			if (aircraftType == AircraftType.VTOL) {
				//HOVER STATE
				HoverControl ();
				//BASE SYSTEM
				FreyrSystem ();
			}
			//COLLECT AIRCRAFT DATA
			CollectPerformanceData ();
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (allOk && isControllable) {
			if (aircraftType == AircraftType.VTOL) {
				//SEND HOVER DATA
				CalculateHover ();
				//BALANCE
				HoverBalance ();
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CollectPerformanceData()
	{
		silantroSpeed = aircraft.velocity.magnitude;
		silantroAltitude = aircraft.transform.position.y;
		silantroHeading = coreSystem.headingDirection;
		silantroMach = coreSystem.machSpeed;
		silantroVerticalSpeed = aircraft.velocity.y;
		//
		silantroWeight = currentWeight;
		if (fuelsystem != null) {
			silantroFuel = fuelsystem.TotalFuelRemaining;
			silantroFuelConsumption = fuelsystem.totalConsumptionRate;
		}
		silantroWingArea = totalWingArea;
		silantroThrust = totalThrustGenerated;
		if (silantroThrust > 0 && currentWeight > 0) {
			silantroThrustWeightRatio = (silantroThrust / (currentWeight * 9.81f));
		}
		silantroWingLoading = wingLoading;
		if (flapfoil != null) {
			if (flapfoil.canUseFlap) {
				silantroFlapAngle = Mathf.Abs (flapfoil.flapAngle);
			}
		}
		if (slatfoil != null) {
			if (slatfoil.canUseSlat) {
				if (slatfoil.slatExtended) {
					silantroSlatState = "Extended";
				}
				if (!slatfoil.slatExtended) {
					silantroSlatState = "Retracted";
				}
			} else {
				silantroSlatState = "N/A";
			}
		}
		//
		silantroDensity = coreSystem.airDensity;
		silantroTemperature = coreSystem.ambientTemperature;
		silantroPressure = coreSystem.ambientPressure;
		//
		if (gearHelper != null) {
			if (gearHelper.gearOpened) {
				silantroGearState = "Open";
			}
			if (gearHelper.gearClosed) {
				silantroGearState = "Closed";
			}
			//
			if (gearHelper.brakeActivated) {
				silantroBrakeState = "Engaged";
			}
			else {
				silantroBrakeState = "DisEngaged";
			}
		}
		if (lightControl != null && lightControl.lights.Length >0) {
			silantroLightState = lightControl.lights [0].state.ToString ();
		}
	}


















	//-------------------------------------------------------------------------
	//------------------------------//VTOL-STOVL SYSTEM------------------------
	//-------------------------------------------------------------------------

	public enum Configuration
	{
		F35Liftfan,
		AV8Harrier
	}
	[HideInInspector]public Configuration configuration = Configuration.F35Liftfan;
	[HideInInspector]public SilantroLiftFan liftfan;
	[HideInInspector]public float maximumTorque;//for hover yaw control
	[HideInInspector]public float LiftForce;
	[HideInInspector]public float transitionTime = 10f;
	//OPERATION MODE
	public enum Mode
	{
		Normal,VTOL,STOL
	}
	[HideInInspector]public Mode mode = Mode.Normal;
	//ACTUATORS
	[HideInInspector]public SilantroHydraulicSystem VTOLHydraulics;
	[HideInInspector]public SilantroHydraulicSystem STOLHydraulics;
	[HideInInspector]public SilantroHydraulicSystem liftSystemHydraulics;
	//CONTROL BOOLS
	[HideInInspector]public bool transitionToVTOL;
	[HideInInspector]public bool transitionToSTOL;
	[HideInInspector]public bool transitionToNormal;
	bool transitioning = false;
	//VARIABLES
	[HideInInspector]public float STOLMultiplier;
	[HideInInspector]public float VTOLMultiplier;
	float engineFactor;
	//
	public enum LiftForceMode
	{
		Linear,	Quadratic,Quatic
	}
	[HideInInspector]public LiftForceMode liftForceMode = LiftForceMode.Linear;
	[HideInInspector]private float exponent;
	[HideInInspector]public LayerMask surfaceMask;
	[HideInInspector]public float BrakingTorque = 2000f;
	[HideInInspector]public float maximumHoverHeight = 30f;
	[HideInInspector]public float hoverDamper = 9000f;
	[HideInInspector]public float hoverAngleDrift = 25f;
	[HideInInspector]public float PitchCompensationTorque = 1000;
	[HideInInspector]public float RollCompensationTorque=1000;
	//MODE
	public enum CurrentMode
	{
		Normal,STOL,VTOL,
	}
	[HideInInspector]public CurrentMode currentMode = CurrentMode.Normal;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void InitializeSTOVL()
	{
		if (liftForceMode == LiftForceMode.Linear) {exponent = 1f;}
		if (liftForceMode == LiftForceMode.Quadratic) {exponent = 2f;}
		if (liftForceMode == LiftForceMode.Quatic) {exponent = 3f;}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RUN STOVL
	void FreyrSystem()
	{
		//SWITCH MODES
		switch (mode) {
		case Mode.Normal:
			NormalState ();
			break;
		case Mode.VTOL:
			VTOLState ();
			break;
		case Mode.STOL:
			STOLState ();
			break;
		}
		//COLLECT VARIBALES
		VTOLMultiplier = VTOLHydraulics.currentDragPercentage / 100;
		STOLMultiplier = STOLHydraulics.currentDragPercentage / 100;
		//CALCULATE TOTAL LIFT
		LiftForce = 0f;
		float deteranceFactor = 1 - VTOLMultiplier;deteranceFactor = Mathf.Clamp (deteranceFactor, 0, 1f);
		if (configuration == Configuration.AV8Harrier) {
			LiftForce = pegasus.EngineThrust;
			pegasus.liftFactor = deteranceFactor;
		}
		if (configuration == Configuration.F35Liftfan) {
			LiftForce = liftfan.fanThrust + turboFans [0].EngineThrust;
			//SEND STOL DATA TO LIFTFAN
			if (liftfan != null) {
				liftfan.forceFactor = STOLMultiplier;
				liftfan.liftFactor = deteranceFactor;
			}
			turboFans [0].liftFactor = deteranceFactor;
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//HOVER YAW CONTROL
	void HoverControl()
	{
		if (mode == Mode.VTOL && aircraft.transform.position.y > 5f) {
			//COLLECT ENGINE VARIABLE
			if (configuration == Configuration.AV8Harrier) {
				engineFactor = pegasus.CurrentRPM / pegasus.LowPressureFanRPM;
			}
			if (configuration == Configuration.F35Liftfan) {
				engineFactor = turboFans [0].coreFactor;
			}
			//
			float yawControl = tempYaw * engineFactor;
			float rollControl = tempRoll * engineFactor;
			float pitchControl = tempPitch * engineFactor;
			//BALANCE AIRCRAFT IS YAW INPUT IS NEAR ZERO
			if (Mathf.Approximately (yawControl, 0)) {
				var ControlAxis = Vector3.Dot (aircraft.angularVelocity, transform.up);
				aircraft.AddRelativeTorque (0, -ControlAxis * BrakingTorque, 0);
			} else {
				aircraft.AddRelativeTorque (0, (yawControl * maximumTorque), (-rollControl * 0.5f*maximumTorque));
			}
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//STATES
	private void NormalState()
	{
		//FOR F-35 LIGHTNING
		if (configuration == Configuration.F35Liftfan) {
			if (transitionToVTOL && !transitioning) {
				liftSystemHydraulics.open = true;
				transitioning = true;
				StartCoroutine (OpenEngineDoors(1));
			}
			if (transitionToSTOL && !transitioning) {
				liftSystemHydraulics.open = true;
				transitioning = true;
				StartCoroutine (OpenEngineDoors(2));
			}
		}
		//FOR AV8 HARRIER
		if (configuration == Configuration.AV8Harrier) {
			if (transitionToVTOL && !transitioning) {
				VTOLHydraulics.open = true;
				transitioning = true;
				StartCoroutine (TransitionToVTOL ());
			}
			if (transitionToSTOL && !transitioning) {
				STOLHydraulics.open = true;
				transitioning = true;
				StartCoroutine (TransitionToSTOL ());
			}
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void VTOLState()
	{
		if (configuration == Configuration.F35Liftfan) {
			if (transitionToNormal && !transitioning) {
				VTOLHydraulics.close = true;
				liftfan.stop = true;
				transitioning = true;
				StartCoroutine (CloseF35Doors ());
			}
			if (transitionToSTOL && !transitioning) {
				STOLHydraulics.open = true;
				transitioning = true;
				StartCoroutine (TransitionToSTOL ());
			}
		} 
		else if (configuration == Configuration.AV8Harrier) {
			if (transitionToNormal && !transitioning) {
				VTOLHydraulics.close = true;
				transitioning = true;
				StartCoroutine (TransitionToNormal ());
			}
			if (transitionToSTOL && !transitioning) {
				STOLHydraulics.open = true;
				transitioning = true;
				StartCoroutine (TransitionToSTOL ());
			}
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void STOLState()
	{
		if (configuration == Configuration.F35Liftfan) {
			if (transitionToNormal && !transitioning) {
				STOLHydraulics.close = true;liftfan.stop = true;
				transitioning = true;
				StartCoroutine (CloseF35Doors());
			}
			if (transitionToVTOL && !transitioning) {
				VTOLHydraulics.open = true;
				transitioning = true;
				StartCoroutine (TransitionToVTOL ());
			}
		}
		else if (configuration == Configuration.AV8Harrier) {
			if (transitionToNormal && !transitioning) {
				VTOLHydraulics.close = true;
				transitioning = true;
				StartCoroutine (TransitionToNormal ());
			}
			if (transitionToVTOL && !transitioning) {
				STOLHydraulics.close = true;
				transitioning = true;
				StartCoroutine (TransitionToVTOL ());
			}
		}
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//MAIN HYDRAULIC CONTROLS
	IEnumerator OpenEngineDoors(int level)
	{
		yield return new WaitForSeconds (liftSystemHydraulics.openTime);
		if (level == 1) {OpenF35VTOLDoors();} 
		else if (level == 2) {STOLHydraulics.open = true;OpenF35STOLDoors();}
	}
	void OpenF35VTOLDoors()
	{
		VTOLHydraulics.open = true;StartCoroutine(TransitionToVTOL ());
	}
	void OpenF35STOLDoors()
	{
		StartCoroutine(TransitionToSTOL ());
	}
	IEnumerator CloseF35Doors()
	{
		yield return new WaitForSeconds (liftSystemHydraulics.closeTime);
		liftSystemHydraulics.close = true;StartCoroutine (TransitionToNormal ());
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//ENUMERATORS
	private IEnumerator TransitionToVTOL ()
	{
		if (configuration == Configuration.F35Liftfan) {
			liftfan.start = true;
		}
		yield return new WaitForSeconds (transitionTime);
		mode = Mode.VTOL;transitioning = false;ResetControlKeys ();
		currentMode = SilantroController.CurrentMode.VTOL;
	}
	private IEnumerator TransitionToNormal ()
	{
		yield return new WaitForSeconds (transitionTime);
		mode = Mode.Normal;transitioning = false;ResetControlKeys ();
		currentMode = SilantroController.CurrentMode.Normal;
	}
	private IEnumerator TransitionToSTOL ()
	{
		if (configuration == Configuration.F35Liftfan) {
			liftfan.start = true;
		}
		yield return new WaitForSeconds (transitionTime);
		mode = Mode.STOL;transitioning = false;ResetControlKeys ();
		currentMode = SilantroController.CurrentMode.STOL;
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void ResetControlKeys()
	{
		transitionToSTOL = false;
		transitionToVTOL = false;
		transitionToNormal = false;
	}
	float powerFactor,basePower;float drag;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void CalculateHover()
	{
		if (mode == Mode.VTOL) {
			RaycastHit groundHit;
			//CALCULATE DIRECTION OF FORCE
			Vector3 up = aircraft.transform.up;
			Vector3 gravity = Physics.gravity.normalized;
			up = Vector3.RotateTowards (up, -gravity, hoverAngleDrift * Mathf.Deg2Rad, 1);
			powerFactor = 0;
			if (!Physics.Raycast (transform.position, -up, out groundHit, maximumHoverHeight, surfaceMask)) {
			}
			//CALCULATE POWER FALLOFF
			powerFactor = Mathf.Pow ((maximumHoverHeight - (groundHit.distance * 3.28f)) / maximumHoverHeight, exponent);
			if (powerFactor < 0) {
				powerFactor = 0;
			}
			basePower = Mathf.Lerp (basePower, powerFactor, Time.deltaTime * 0.5f);
			float liftForce = basePower * LiftForce;
			//CALCULATE DAMPING
			float velocity = Vector3.Dot (aircraft.GetPointVelocity (transform.position), up);
			drag = -velocity * Mathf.Abs (velocity) * hoverDamper;
			//
			if ((transform.position.y * 3.28f) < maximumHoverHeight) {
				if (mode == Mode.VTOL) {
					aircraft.AddForce (up * (liftForce + drag), ForceMode.Force);
				}
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void HoverBalance()
	{
		if (mode == Mode.VTOL) {
			float yawFactor = Vector3.Dot (aircraft.angularVelocity, transform.up);
			aircraft.AddRelativeTorque (0, (-yawFactor * BrakingTorque), 0);
			//PITCH AND ROLL BALALNCE
			if (transform.position.y > 5f) {
				Vector3 gravity = -Physics.gravity.normalized;
				float pitch = Mathf.Asin (Vector3.Dot (transform.forward, gravity)) * Mathf.Rad2Deg;
				var roll = Mathf.Asin (Vector3.Dot (transform.right, gravity)) * Mathf.Rad2Deg;
				pitch = Mathf.DeltaAngle (pitch, 0); 
				roll = Mathf.DeltaAngle (roll, 0);
				//TORQUES
				float pitchTorque = -pitch * PitchCompensationTorque;
				float rollTorque = roll * RollCompensationTorque;
				aircraft.AddRelativeTorque (pitchTorque, 0, rollTorque);
			}
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//JATO SYSTEM
	public enum JATOState
	{
		Available,
		Absent
	}
	[HideInInspector]public JATOState jatoState = JATOState.Absent;
	[HideInInspector]public SilantroRocketMotor[] boosters;
	[HideInInspector]public float totalBoost;


	//
	[HideInInspector]public string silantroAircraftName;
	//AIRCRAFT DATA
	[HideInInspector]public float silantroSpeed;
	[HideInInspector]public float silantroAltitude;
	[HideInInspector]public float silantroHeading;
	[HideInInspector]public float silantroMach;
	[HideInInspector]public float silantroVerticalSpeed;
	//
	[HideInInspector]public float silantroWeight;
	[HideInInspector]public float silantroFuel;
	[HideInInspector]public float silantroWingArea;
	[HideInInspector]public float silantroWingLoading;
	[HideInInspector]public float silantroFlapAngle;
	[HideInInspector]public float silantroSlatLevel;
	//
	[HideInInspector]public float silantroEnginePower;
	[HideInInspector]public float silantroN1;
	[HideInInspector]public float silantroN2;
	//AMBIENT
	[HideInInspector]public float silantroDensity;
	[HideInInspector]public float silantroTemperature;
	[HideInInspector]public float silantroPressure;
	//ELEMENTS
	[HideInInspector]public string silantroBrakeState;
	[HideInInspector]public string silantroGearState;
	[HideInInspector]public string silantroLightState;
	[HideInInspector]public string silantroSlatState;
	//POWER
	[HideInInspector]public int silantroEngineCount;
	[HideInInspector]public string silantroEngineName;
	[HideInInspector]public string silantroEngineType;
	[HideInInspector]public float silantroThrust;
	[HideInInspector]public float silantroFuelConsumption;
	[HideInInspector]public float silantroThrustWeightRatio;
	[HideInInspector]public bool silantroActive;//Engines On
}













#if UNITY_EDITOR
[CustomEditor(typeof(SilantroController))]
public class ControllerEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1.0f,0.40f,0f);
	//
	SilantroController controller;
	SerializedObject controllerObject;
	//
	private void OnEnable()
	{
		controller = (SilantroController)target;
		controllerObject = new SerializedObject (controller);
	}
	override public void OnInspectorGUI ()
	{
		
		backgroundColor = GUI.backgroundColor;
		//
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck();
		controllerObject.UpdateIfRequiredOrScript();
		//
		//
		GUILayout.Space(1f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Aircraft Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		EditorGUILayout.LabelField ("Identifier", controller.aircraftName);
		GUILayout.Space(3f);
		controller.engineType = (SilantroController.EngineType)EditorGUILayout.EnumPopup ("Engine Type", controller.engineType);
		GUILayout.Space (3f);
		controller.aircraftType = (SilantroController.AircraftType)EditorGUILayout.EnumPopup ("Aircraft Type", controller.aircraftType);
		//
		if (controller.aircraftType == SilantroController.AircraftType.VTOL) {
			GUILayout.Space (10f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("STOVL Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			controller.configuration = (SilantroController.Configuration)EditorGUILayout.EnumPopup ("Configuration", controller.configuration);
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Hydraulic Actuators", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			controller.VTOLHydraulics = EditorGUILayout.ObjectField ("VTOL Actuator", controller.VTOLHydraulics, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
			GUILayout.Space (3f);
			controller.STOLHydraulics = EditorGUILayout.ObjectField ("STOL Actuator", controller.STOLHydraulics, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
			//
			if (controller.configuration == SilantroController.Configuration.F35Liftfan) {
				GUILayout.Space (3f);
				controller.liftSystemHydraulics = EditorGUILayout.ObjectField ("LiftFan Actuator", controller.liftSystemHydraulics, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
			}
			//
			GUILayout.Space (3f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Balance Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			controller.BrakingTorque = EditorGUILayout.FloatField ("Braking Torque", controller.BrakingTorque);
			GUILayout.Space (2f);
			controller.RollCompensationTorque = EditorGUILayout.FloatField ("Longitudinal Balance Torque", controller.RollCompensationTorque);
			GUILayout.Space (2f);
			controller.PitchCompensationTorque = EditorGUILayout.FloatField ("Lateral Balance Torque", controller.PitchCompensationTorque);
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Transition Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			controller.transitionTime = EditorGUILayout.FloatField ("Transition Time", controller.transitionTime);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Current Mode", controller.mode.ToString ());
			//
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox ("Hover Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			SerializedProperty layerMask = controllerObject.FindProperty ("surfaceMask");
			EditorGUILayout.PropertyField (layerMask);
			GUILayout.Space(3f);
			controller.maximumHoverHeight = EditorGUILayout.FloatField ("Maximum Hover Height", controller.maximumHoverHeight);
			GUILayout.Space (2f);
			controller.hoverDamper = EditorGUILayout.FloatField ("Hover Damper", controller.hoverDamper);
			GUILayout.Space (2f);
			controller.hoverAngleDrift = EditorGUILayout.FloatField ("Hover Angle Drift", controller.hoverAngleDrift);
			GUILayout.Space (4f);
			controller.maximumTorque = EditorGUILayout.FloatField("Control Torque",controller.maximumTorque);
			GUILayout.Space (4f);
			controller.liftForceMode = (SilantroController.LiftForceMode)EditorGUILayout.EnumPopup ("Liftforce Mode", controller.liftForceMode);
		}
		//
		GUILayout.Space (10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Control Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		controller.controlType = (SilantroController.ControlType)EditorGUILayout.EnumPopup ("Control", controller.controlType);
		if (controller.controlType == SilantroController.ControlType.Internal) {
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Cockpit Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Pilot Onboard", controller.pilotOnboard.ToString ());
			GUILayout.Space (3f);
			controller.canvasDisplay = EditorGUILayout.ObjectField ("Display Canvas", controller.canvasDisplay, typeof(SilantroDisplay), true) as SilantroDisplay;
			GUILayout.Space (3f);
			controller.canopyActuator = EditorGUILayout.ObjectField ("Canopy Actuator", controller.canopyActuator, typeof(SilantroHydraulicSystem), true) as SilantroHydraulicSystem;
			GUILayout.Space (3f);
			controller.interiorPilot = EditorGUILayout.ObjectField ("Interior Pilot", controller.interiorPilot, typeof(GameObject), true) as GameObject;
			GUILayout.Space (3f);
			controller.getOutPosition = EditorGUILayout.ObjectField ("Exit Point", controller.getOutPosition, typeof(Transform), true) as Transform;
			GUILayout.Space (10f);
		}
		GUILayout.Space (3f);
		controller.inputType = (SilantroController.InputType)EditorGUILayout.EnumPopup ("Input", controller.inputType);
		GUILayout.Space (3f);
		controller.startMode = (SilantroController.StartMode)EditorGUILayout.EnumPopup ("Start Mode", controller.startMode);
		if (controller.startMode == SilantroController.StartMode.Hot) {
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Instantenous Start (Speed = m/s, Altitude = m)", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			controller.startSpeed = EditorGUILayout.FloatField ("Start Speed", controller.startSpeed);
			GUILayout.Space (3f);
			controller.startAltitude = EditorGUILayout.FloatField ("Start Altitude", controller.startAltitude);
		}
		//
		GUILayout.Space(10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Control Effectiveness", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		controller.rollSensitivity = EditorGUILayout.Slider ("Roll Sensitivity", controller.rollSensitivity, 0f, 1f);
		GUILayout.Space (3f);
		controller.pitchSensitivity = EditorGUILayout.Slider ("Pitch Sensitivity", controller.pitchSensitivity, 0f, 1f);
		GUILayout.Space (3f);
		controller.yawSensitivity = EditorGUILayout.Slider ("Yaw Sensitivity", controller.yawSensitivity, 0f, 1f);
		GUILayout.Space (5f);
		controller.controlFactor = EditorGUILayout.Slider ("Sensitivity Factor", controller.controlFactor, 1f, 10f);
		//
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("System Type", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		controller.wingType = (SilantroController.WingType)EditorGUILayout.EnumPopup ("Wing", controller.wingType);
		GUILayout.Space (3f);
		controller.stabilizerType = (SilantroController.StabilizerType)EditorGUILayout.EnumPopup ("Stabilizer", controller.stabilizerType);
		GUILayout.Space (3f);
		//
		GUILayout.Space (15f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Aircraft Weight Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		//
		controller.emptyWeight = EditorGUILayout.FloatField ("Empty Weight", controller.emptyWeight);
		GUILayout.Space (2f);
		controller.maximumWeight = EditorGUILayout.FloatField ("Maximum Weight", controller.maximumWeight);
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Current Weight", controller.currentWeight.ToString ("0.00") + " kg");
		//
		GUILayout.Space (10f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Appendages", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("JATO System", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		controller.jatoState = (SilantroController.JATOState)EditorGUILayout.EnumPopup(" ",controller.jatoState);
		if (controller.jatoState == SilantroController.JATOState.Available) {
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Boost Force", controller.totalBoost.ToString ("0.00") + " N");
		}
		//
		GUILayout.Space (10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Aircraft Data Display", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		if (controller.wingType == SilantroController.WingType.Biplane) {
			EditorGUILayout.LabelField ("Upper Wing Span", controller.upperWingSpan.ToString ("0.00") + " m");
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Upper Wing Span", controller.lowerWingSpan.ToString ("0.00") + " m");
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Wing Gap", controller.wingGap.ToString ("0.00") + " m");
		} 
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Available Engines", controller.AvailableEngines.ToString ());
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Total Thrust", controller.totalThrustGenerated.ToString ("0.00") + " N");
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Wing Loading", controller.wingLoading.ToString ("0.00") + " kg/m2");
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Thrust/Weight Ratio", controller.thrustToWeightRatio.ToString ("0.00"));
		//
		if (EditorGUI.EndChangeCheck ()) {
			Undo.RegisterCompleteObjectUndo (controllerObject.targetObject, "Controller Change");
		}
		////
		if (GUI.changed) {
			EditorUtility.SetDirty (controller);
			EditorSceneManager.MarkSceneDirty (controller.gameObject.scene);
		}
		controllerObject.ApplyModifiedProperties();
	}
}
#endif