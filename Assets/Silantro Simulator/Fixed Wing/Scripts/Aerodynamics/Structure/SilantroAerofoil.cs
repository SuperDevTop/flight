// Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[RequireComponent(typeof(BoxCollider))]
public class SilantroAerofoil : MonoBehaviour {

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//                                                       -------FOIL SELECTIBLES---------
	public enum AerofoilType{Wing,Stabilizer,Canard}
	[HideInInspector]public AerofoilType aerofoilType = AerofoilType.Wing;
	public enum ControType{Stationary,Controllable}
	[HideInInspector]public ControType controlState = ControType.Controllable;
	public enum SurfaceType{Inactive,Elevator,Rudder,Aileron,Ruddervator,Elevon}
	[HideInInspector]public SurfaceType surfaceType = SurfaceType.Aileron;

	public enum SweepDirection{Unswept,Forward,Backward}
	[HideInInspector]public SweepDirection sweepDirection = SweepDirection.Unswept;
	public enum TwistDirection{Untwisted,Upwards,Downwards}
	[HideInInspector]public TwistDirection twistDirection = TwistDirection.Untwisted;
	public enum AvailableControls{PrimaryOnly,PrimaryPlusSecondary,SecondaryOnly}
	[HideInInspector]public AvailableControls availableControls = AvailableControls.PrimaryOnly;
	public enum SurfaceFinish{SmoothPaint,PolishedMetal,ProductionSheetMetal,MoldedComposite,PaintedAluminium}
	[HideInInspector]public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;
	public enum WingtipDesign{Round,Spherical,Square,Endplate,Winglet}
	[HideInInspector]public WingtipDesign tipDesign = WingtipDesign.Square;
	[HideInInspector]public float effectiveChange,spanEfficiency =0.9f,angleOfAttack;
	public enum ForceSystem{AerodynamicCenter,CenterOfPressure}
	[HideInInspector]public ForceSystem forceSystem = ForceSystem.AerodynamicCenter;

	//POSITIONS
	public enum WingPosition{Top,Bottom,Monoplane}
	[HideInInspector]public WingPosition wingPosition = WingPosition.Monoplane;
	public enum CenterSide{Right,Left}
	[HideInInspector]public CenterSide centerSide = CenterSide.Right;
	public enum SlatMovement{Deflection,Extension}[HideInInspector]public SlatMovement slatMovement = SlatMovement.Deflection;
	public enum StabilizerOrientation{Vertical,Horizontal}
	[HideInInspector]public StabilizerOrientation stabOrientation = StabilizerOrientation.Horizontal;
	public enum StabilizerPosition{Left,Right,Center}
	[HideInInspector]public StabilizerPosition stabilizerPosition = StabilizerPosition.Center;
	public enum CanardPosition{Left,Right}
	[HideInInspector]public CanardPosition canardPosition = CanardPosition.Left;
	public enum HorizontalPosition{Left,Right,Center}
	[HideInInspector]public HorizontalPosition horizontalPosition = HorizontalPosition.Center;
	public enum RudderPosition{Left,Center,Right}
	[HideInInspector]public RudderPosition rudderPosition = RudderPosition.Center;

	public enum SpoilerType{Plain,Spoileron}
	[HideInInspector]public SpoilerType spoilerType = SpoilerType.Plain;
	public enum FlapType{Plain,Split,Flaperon}
	[HideInInspector]public FlapType flapType = FlapType.Plain;


	public enum GroundEffectState{Neglect,Consider}
	[HideInInspector]public GroundEffectState effectState = GroundEffectState.Neglect;
	[HideInInspector]public Vector3 groundAxis;
	[HideInInspector]public LayerMask groundLayer;
	[HideInInspector]public float groundInfluenceFactor;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//FUNCTIONS
	public enum SweepCorrection{RaymerJenkinson,YoungLE,None}
	[HideInInspector]public SweepCorrection sweepCorrectionMethod = SweepCorrection.YoungLE;
	[HideInInspector]public float sweepCorrectionFactor = 1f;
	//GROUND EFFECT
	public enum GroundEffectMethod{Weiselburger,McCormick,Asselin}
	[HideInInspector]public GroundEffectMethod groundInfluenceMethod = GroundEffectMethod.Weiselburger;

	public enum ElevonPosition{Right,Left}
	[HideInInspector]public ElevonPosition elevonPosition = ElevonPosition.Right;
	public enum FlaperonPosition{Right,Left}
	[HideInInspector]public FlaperonPosition flaperonPosition = FlaperonPosition.Right;
	[HideInInspector]public List<float> flapAngles;bool invertFlap;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CONNECTIONS
	[HideInInspector]public Rigidbody connectedAircraft;BoxCollider foilCollider;
	[HideInInspector]public SilantroAirfoil rootAirfoil,tipAirfoil;
	[HideInInspector]public SilantroController calculator;
	[HideInInspector]public SilantroCore coreSystem;


	//VECTOR POINTS
	[HideInInspector]public Vector3 RootChordLeading,RootChordTrailing,TipChordLeading,TipChordTrailing,quaterRootChordPoint,quaterTipChordPoint;
	[HideInInspector]public Vector3 TipChordCenter,RootChordCenter,airflow,wingTipLeading,wingTipTrailing,wingTipPivot,wingLetCenter;
	List<Vector3> tipPoints;List<Vector3> rootPoints;List<Vector3> points;Vector3 PointA,PointB;Vector3 PointXA,PointXB;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//GEOMETRY
	[HideInInspector]public float foilTipChord,foilRootChord,foilMeanChord,AspectRatio,foilSpan,foilArea,foilWettedArea;
	[HideInInspector]public float tipCenterSweep,leadingEdgeSweep,quaterSweep,taperPercentage = 0f,wingTwist,aerofoilSweepAngle = 0f,rootAirfoilArea,tipAirfoilArea;
	[HideInInspector]public int foilSubdivisions = 5;float sweepangle,actualTwist;[HideInInspector]public Color controlColor = Color.green;


	[HideInInspector]public float TipTaperPercentage = 100f;
	[HideInInspector]public float wingTipHeight = 5f;
	[HideInInspector]public float wingTipBend = 20f;
	[HideInInspector]public float wingTipSweep = 20f;
	[HideInInspector]public float biplaneLiftFactor = 1f,biplaneDragFactor = 1f;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CONTROL FACTORS
	[HideInInspector]public bool showPanel,drawSectionRibs = true,drawRibSplits = true;
	[HideInInspector]public List<Vector3> panelLiftForces = new List<Vector3> ();
	[HideInInspector]public List<Vector3> panelDragForces = new List<Vector3> ();
	[HideInInspector]public List<Vector3> panelMoments = new List<Vector3> ();
	[HideInInspector]public List<Vector3> CenterOfPressures = new List<Vector3> ();

	//OUTPUT
	[HideInInspector]public float TotalLift,TotalDrag,TotalMoment,airDensity = 1.225f,airSpeed;

	[HideInInspector]public bool[] spoilerSections;
	[HideInInspector]public float spoilerChordFactor = 10f,targetSpoilerAngle,spoilerAngle;
	[HideInInspector]public float spoilerArea,spoilerHinge = 10f,spoilerDragFactor,spoilerDeflection,maximumSpoilerDeflection = 60f,spoilerActuationTime = 5f,slatActuationTime = 5f;
	public enum SpoilerDeflectionDirection{CW,CCW}
	[HideInInspector]public SpoilerDeflectionDirection spoilerDeflectionDirection = SpoilerDeflectionDirection.CW;
	public enum SpoilerRotationAxis{X,Y,Z}[HideInInspector]public SpoilerRotationAxis spoilerRotationAxis = SpoilerRotationAxis.X;
	Vector3 spoilerDeflectionAxis;Quaternion baseSpoilerRotation;[HideInInspector]public bool spoilerMoving;


	[HideInInspector]public bool[] controlSections;
	[HideInInspector]public float controlRootChord = 10f,controlTipChord = 10f,controlArea;
	public enum DeflectionType{Symmetric,Asymmetric}
	[HideInInspector]public DeflectionType controlDeflectionType = DeflectionType.Symmetric;
	[HideInInspector]public float positveLimit = 30f,negativeLimit = 20f,controlDeflection;
	public enum BaseDeflectionDirection{CW,CCW}
	[HideInInspector]public BaseDeflectionDirection baseDeflectionDirection = BaseDeflectionDirection.CW;
	public enum RotationAxis{X,Y,Z}[HideInInspector]public RotationAxis rotationAxis = RotationAxis.X;
	Vector3 controlDeflectionAxis;Quaternion baseModelRotation;
	[HideInInspector]public Transform controlSurfaceModel,flapSurfaceModel,slatSurfaceModel,spoilerSurfaceModel;


	[HideInInspector]public bool[] flapSections;
	[HideInInspector]public float flapRootChord = 20f,flapTipChord = 15f,flapArea,flapDeflection,flapAngle,baseFlap;
	[HideInInspector]public float flap1 = 8f,flap2 = 17f,flap3 = 26f,flapsfull = 32f;
	[HideInInspector]public int flapSelection = 0;public enum FlapDeflectionDirection{CW,CCW}
	[HideInInspector]public FlapDeflectionDirection flapDeflectionDirection = FlapDeflectionDirection.CW;
	public enum FlapRotationAxis{X,Y,Z}[HideInInspector]public FlapRotationAxis flapRotationAxis = FlapRotationAxis.X;
	Vector3 flapDeflectionAxis;Quaternion baseFlapRotation;
	[HideInInspector]public AudioClip flapUp, flapDown;AudioSource foilSounds;


	[HideInInspector]public bool[] slatSections;
	[HideInInspector]public float slatRootChord = 20f,slatTipChord = 20f;
	[HideInInspector]public float slatArea;public enum SlatDeflectionDirection{CW,CCW}
	[HideInInspector]public SlatDeflectionDirection slatDeflectionDirection = SlatDeflectionDirection.CW;
	public enum SlatRotationAxis{X,Y,Z}[HideInInspector]public SlatRotationAxis slatRotationAxis = SlatRotationAxis.X;
	Vector3 slatDeflectionAxis;Quaternion baseSlatRotation;
	[HideInInspector]public float slatDistance = 10f;Vector3 baseSlatPosition;float slatPosition;
	[HideInInspector]public float targetSlatPosition,currentSlatPosition;

	[HideInInspector]public bool isControllable = true,showAdvanced;
	[HideInInspector]public bool canUseFlap, canUseSlat, canUseSpoilers, canUseTrim,spoilerExtended,slatExtended,slatMoving;
	public enum SlatType{FixedSlot,MaxwellFlap,KrugerFlap}[HideInInspector]public SlatType slatType = SlatType.FixedSlot;
	[HideInInspector]public float Clmax = 0.45f,Cdmax = 0.0149f,Cmmax = 0.0008f,slatLiftFactor,slatDragFactor,slatMomentFactor;
	[HideInInspector]public float maximumSlatDeflection = 30f,slatDeflection,targetSlatAngle,TotalSkinDrag,CF,TotalBaseDrag,TotalSpoilerDrag,TotalControlDrag,TotalFlapDrag;
	[HideInInspector]public float viscocity = 0.000014607f,k,machSpeed = 0f,Asf,slatActuationSpeed = 5f;
	[HideInInspector]public float pressureCenter,dragCoefficient,factor1,factor2;

	public enum TrimState{Absent,Available}[HideInInspector]public TrimState trimState = TrimState.Absent;
	[HideInInspector]public float maximumPitchTrim = 50f,currentPitchTrim,targetPitchTrim,trimSpeed = 0.005f,currentRollTrim,targetRollTrim,currentYawTrim,targetYawTrim;

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CONTROL INPUTS
	[HideInInspector]public float pitchInput,rollInput,yawInput;



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//TOGGLE SPOILER
	public void ActuateSpoilers()
	{
		if (controlState == ControType.Controllable && canUseSpoilers && aerofoilType == AerofoilType.Wing && isControllable) {
			if (!spoilerExtended) {
				targetSpoilerAngle = maximumSpoilerDeflection;StartCoroutine (ExtendSpoilers ());
			} else {
				targetSpoilerAngle = 0f;StartCoroutine (RetractSpoilers ());
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//TOGGLE SLAT
	public void ActuateSlat()
	{
		if (controlState == ControType.Controllable && canUseSlat && aerofoilType == AerofoilType.Wing && isControllable) {
			if (!slatExtended) {
				targetSlatAngle = maximumSlatDeflection;targetSlatPosition = slatDistance;StartCoroutine (ExtendSlats ());
			} else {
				targetSlatAngle = 0f;targetSlatPosition = 0f;StartCoroutine (RetractSlats ());
			}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//LOWER FLAP
	public void LowerFlap()
	{
		if (controlState == ControType.Controllable && canUseFlap && aerofoilType == AerofoilType.Wing && isControllable) {
			//INTERNAL MOVEMENT SYSTEM
			IncreaseFlap ();
			if (flapDown != null) {foilSounds.clip = flapDown;
			if (flapSelection < (flapAngles.Count - 1)) {foilSounds.Play ();}}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RAISE FLAP
	public void RaiseFlap()
	{
		if (controlState == ControType.Controllable && canUseFlap && aerofoilType == AerofoilType.Wing && isControllable) {
			//INTERNAL MOVEMENT SYSTEM
			DecreaseFlap ();
			if (flapUp != null) {foilSounds.clip = flapUp;
			if (flapSelection > 0) {foilSounds.Play ();}}
		}
	}


	//TRIM SETTINGS
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void RaiseTrimTab(int state){if (controlState == ControType.Controllable && isControllable && surfaceType != SurfaceType.Inactive) {
			if (state == 1) {targetPitchTrim += 0.05f;}float pitchlimit = maximumPitchTrim / 100;targetPitchTrim = Mathf.Clamp (targetPitchTrim, -pitchlimit, pitchlimit);
			if (state == 2) {targetRollTrim += 0.05f;}
			if (state == 3) {targetYawTrim += 0.05f;}
		}
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void LowerTrimTab(int state){if (controlState == ControType.Controllable && isControllable && surfaceType != SurfaceType.Inactive) {
			if (state == 1) {targetPitchTrim -= 0.05f;}if (state == 1) {targetRollTrim -= 0.05f;}if (state == 1) {targetYawTrim -= 0.05f;}
			targetPitchTrim = Mathf.Clamp (targetPitchTrim, -maximumPitchTrim / 100, maximumPitchTrim / 100);
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (connectedAircraft != null &&  connectedAircraft != null && rootAirfoil != null && tipAirfoil != null) {
			allOk = true;
		} else if (connectedAircraft == null) {
			Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Aircraft rigidbody not assigned");
			allOk = false;
		}
		else if (connectedAircraft == null) {
			Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Core system not assigned");
			allOk = false;
		}
		else if (rootAirfoil == null) {
			Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Root airfoil has not been assigned");
			allOk = false;
		}
		else if (tipAirfoil == null) {
			Debug.LogError("Prerequisites not met on " + aerofoilType.ToString() + " " + transform.name + "....Tip airfoil has not been assigned");
			allOk = false;
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeFoil()
	{

		//----------------------------
		_checkPrerequisites ();


		if(allOk){
			
			if (controlState == ControType.Controllable) {
				//1. BASE CONTROL
				controlDeflectionAxis = EstimateModelProperties(baseDeflectionDirection.ToString(),rotationAxis.ToString());
				if (controlSurfaceModel != null) {baseModelRotation = controlSurfaceModel.transform.localRotation;}
				groundAxis = new Vector3 (0.0f, -1.0f, 0.0f); groundAxis.Normalize();
				//2. SPOILER CONTROL
				spoilerDeflectionAxis = EstimateModelProperties(spoilerDeflectionDirection.ToString(),spoilerRotationAxis.ToString());
				if (spoilerSurfaceModel != null) {baseSpoilerRotation = spoilerSurfaceModel.transform.localRotation;}
				//3. FLAP CONTROL
				flapAngles = new List<float> ();
				flapAngles.Add (0);flapAngles.Add (flap1);flapAngles.Add (flap2);flapAngles.Add (flap3);flapAngles.Add (flapsfull);
				flapDeflectionAxis = EstimateModelProperties(flapDeflectionDirection.ToString(),flapRotationAxis.ToString());
				if (flapSurfaceModel != null) {baseFlapRotation = flapSurfaceModel.transform.localRotation;}
				//4. SLAT CONTROL
				slatDeflectionAxis = EstimateModelProperties(slatDeflectionDirection.ToString(),slatRotationAxis.ToString());
				if (slatSurfaceModel != null) {baseSlatRotation = slatSurfaceModel.transform.localRotation;baseSlatPosition = slatSurfaceModel.transform.localPosition;}
				//SETUP SOUND SYSTEM
				if (controlState == ControType.Controllable && aerofoilType == AerofoilType.Wing) {
					if (canUseFlap || canUseSlat) {
						GameObject soundPoint = new GameObject ();
						soundPoint.transform.parent = transform;soundPoint.transform.localPosition = new Vector3 (0, 0, 0);soundPoint.name = this.name + "Aerofoil Sound Point";
						//ASSIGN VARIABLES
						foilSounds = soundPoint.gameObject.AddComponent<AudioSource> ();foilSounds.loop = false;foilSounds.volume = 0.6f;
					}
				}
				//SET SLAT DATA
				if (slatType == SilantroAerofoil.SlatType.FixedSlot) {Cdmax = 0.0075f;Clmax = 0.45f;Cmmax = 0.0008f;}
				if (slatType == SilantroAerofoil.SlatType.MaxwellFlap) {Cdmax = 0.0156f;Clmax = 0.55f;Cmmax = 0.0008f;}
				if (slatType == SilantroAerofoil.SlatType.KrugerFlap) {Cdmax = 0.0149f;Clmax = 0.30f;Cmmax = -0.027f;}

			}

				//SET FINISH FACTOR
				if (surfaceFinish == SurfaceFinish.MoldedComposite) {k = 0.17f;}
				if (surfaceFinish == SurfaceFinish.PaintedAluminium) {k = 3.33f;}
				if (surfaceFinish == SurfaceFinish.PolishedMetal) {k = 0.50f;}
				if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) {k = 1.33f;}
				if (surfaceFinish == SurfaceFinish.SmoothPaint) {k = 2.08f;}
		}
	}




	float baseInput = 0f;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Update()
	{
		if (allOk && isControllable && controlState == ControType.Controllable) {

		//COLLECT DATA
		if (coreSystem != null) {airDensity = coreSystem.airDensity;	machSpeed = coreSystem.machSpeed;	viscocity = coreSystem.viscocity;}
		//-----------------------------------------------------------------------------------------------------------------------
		//PRE-PROCESS TRIM
		if (trimState == TrimState.Available) {
			float maximumPitchTrimInput = maximumPitchTrim / 100f;
			currentPitchTrim = Mathf.MoveTowards (currentPitchTrim, targetPitchTrim, Time.deltaTime * trimSpeed);currentPitchTrim = Mathf.Clamp (currentPitchTrim, -maximumPitchTrimInput, maximumPitchTrimInput);
			currentRollTrim = Mathf.MoveTowards (currentRollTrim, targetRollTrim, Time.deltaTime * trimSpeed);currentRollTrim = Mathf.Clamp (currentRollTrim, -maximumPitchTrimInput, maximumPitchTrimInput);
			currentYawTrim = Mathf.MoveTowards (currentYawTrim, targetYawTrim, Time.deltaTime * trimSpeed);currentYawTrim = Mathf.Clamp (currentYawTrim, -maximumPitchTrimInput, maximumPitchTrimInput);
		}
		//PROCESS CONTROLS
		//----------------------------//PRIMARY
		if(availableControls != AvailableControls.SecondaryOnly){
		if(surfaceType != SurfaceType.Inactive){
		if (surfaceType == SurfaceType.Aileron) {baseInput = rollInput;}
		if (surfaceType == SurfaceType.Elevator) {baseInput = (pitchInput+currentPitchTrim);if (transform.localScale.x < 0) {baseInput = -(pitchInput+currentPitchTrim);}}
		if (surfaceType == SurfaceType.Rudder) {baseInput = -yawInput;}
		if (surfaceType == SurfaceType.Ruddervator) {
		float PitchInput = (pitchInput+currentPitchTrim);float YawInput = -yawInput;
			//MIX
			if(transform.localScale.x < 0){PitchInput *= -1;}
			baseInput = ((YawInput*2f)+(PitchInput*2f))/2f;
		}
		if (surfaceType == SurfaceType.Elevon) {
			float PitchInput = (pitchInput+currentPitchTrim);float RollInput = rollInput;
			//MIX
			if(transform.localScale.x < 0){PitchInput *= -1;}
			baseInput = ((RollInput*2f)+(PitchInput*2f))/2f;
		}

				//PROCESS DEFLECTION
				if (controlDeflectionType == DeflectionType.Symmetric) {controlDeflection = positveLimit * baseInput;
					controlDeflection = Mathf.Clamp (controlDeflection, -positveLimit, positveLimit);}
				if (controlDeflectionType == DeflectionType.Asymmetric) {controlDeflection = Mathf.Max(Mathf.Abs(positveLimit),Mathf.Abs(negativeLimit)) * baseInput;
				//DEFLECTION CONSTRAINT
				if (transform.localScale.x < 0) {controlDeflection = Mathf.Clamp (controlDeflection, -positveLimit, negativeLimit);} //LEFT SIDE
				else {controlDeflection = Mathf.Clamp (controlDeflection, -negativeLimit, positveLimit);}}//RIGHT SIDE
			}
		}

		//CANARD
		if (aerofoilType == AerofoilType.Canard) {controlDeflection *= -1f;}

		//----------------------------//FLAP
		if (aerofoilType == AerofoilType.Wing && canUseFlap && availableControls != AvailableControls.PrimaryOnly) {
			flapAngle = Mathf.Lerp (flapAngle, flapAngles[flapSelection], Time.deltaTime * 0.7f);

			//FLAPERON
			if (flapType == FlapType.Flaperon) {
				float baseRoll = rollInput * positveLimit;
				if(transform.localScale.x < 0){baseRoll = Mathf.Clamp (baseRoll, -positveLimit, negativeLimit);}
				else{baseRoll = Mathf.Clamp (baseRoll, -negativeLimit, positveLimit);}

				if (flaperonPosition == FlaperonPosition.Right) {baseFlap = -flapAngle;} 
				else {baseFlap = flapAngle;}
				//MIX
				flapDeflection = ((baseFlap*2f) + (baseRoll*2f))/2f;
				flapDeflection = Mathf.Clamp (flapDeflection, -flapAngles.Last (), flapAngles.Last ());
			} 
			else {
					if (flapType == FlapType.Flaperon) {
						if (flaperonPosition == FlaperonPosition.Right) {flapDeflection = -flapAngle;} else {flapDeflection = flapAngle;}
					} else {
						if (horizontalPosition == HorizontalPosition.Right) {flapDeflection = -flapAngle;}if (horizontalPosition == HorizontalPosition.Left) {flapDeflection = flapAngle;}
						if (horizontalPosition == HorizontalPosition.Center) {if (centerSide == CenterSide.Right) {flapDeflection = -flapAngle;}if (centerSide == CenterSide.Left) {flapDeflection = flapAngle;}}
					}
			}
		}

		//----------------------------//SPOILERS
		//5. SPOILERS
		if (controlState == ControType.Controllable && canUseSpoilers && aerofoilType == AerofoilType.Wing && availableControls != AvailableControls.PrimaryOnly) {
			if(spoilerType == SpoilerType.Plain){
				if (spoilerMoving) {spoilerAngle = Mathf.MoveTowards (spoilerAngle, targetSpoilerAngle, Time.deltaTime * 0.9f);}
				spoilerDeflection = spoilerAngle;if (transform.localScale.x < 0) {spoilerDeflection *= -1;}}
			else
			{
				
				if (spoilerMoving && transform.localScale.x > 0) {spoilerAngle = Mathf.MoveTowards (spoilerAngle, targetSpoilerAngle, Time.deltaTime * 0.9f);}
				if (spoilerMoving && transform.localScale.x < 0) {spoilerAngle = Mathf.MoveTowards (spoilerAngle, -targetSpoilerAngle, Time.deltaTime * 0.9f);}
				float baseRoll = (rollInput * maximumSpoilerDeflection);

				spoilerDeflection = ((spoilerAngle*2f) + (baseRoll*2f))/2f;
				if (transform.localScale.x < 0) {
				spoilerDeflection = Mathf.Clamp (spoilerDeflection, -maximumSpoilerDeflection, 0);} 
				else {spoilerDeflection = Mathf.Clamp (spoilerDeflection, 0, maximumSpoilerDeflection);}
			}
		}

			//----------------------------//SLATS
			if (controlState == ControType.Controllable && canUseSlat && aerofoilType == AerofoilType.Wing && availableControls != AvailableControls.PrimaryOnly) {
				if (slatMoving) {
					if (slatMovement == SlatMovement.Deflection) {slatDeflection = Mathf.MoveTowards (slatDeflection, targetSlatAngle, Time.deltaTime* slatActuationSpeed);}
					if (slatMovement == SlatMovement.Extension) {slatPosition = Mathf.Lerp(slatPosition,targetSlatPosition, Time.deltaTime * slatActuationSpeed);}
				}
			}

			//----------------------SEND ROTATION DATA
			if (controlSurfaceModel) {controlSurfaceModel.transform.localRotation = baseModelRotation;controlSurfaceModel.transform.Rotate (controlDeflectionAxis, controlDeflection);}//1. MAIN CONTROL SURFACE
			if (spoilerSurfaceModel) {spoilerSurfaceModel.transform.localRotation = baseSpoilerRotation;spoilerSurfaceModel.transform.Rotate (spoilerDeflectionAxis, spoilerDeflection);}//2. SPOILER
			if (flapSurfaceModel) {flapSurfaceModel.transform.localRotation = baseFlapRotation;flapSurfaceModel.transform.Rotate (flapDeflectionAxis, flapDeflection);}//3. FLAP
			if (slatSurfaceModel) {
				//DEFLECTION
				if (slatMovement == SlatMovement.Deflection) {slatSurfaceModel.transform.localRotation = baseSlatRotation;slatSurfaceModel.transform.Rotate (slatDeflectionAxis, slatDeflection);}
				//SLIDING
				if (slatMovement == SlatMovement.Extension) {slatSurfaceModel.transform.localPosition = baseSlatPosition;slatSurfaceModel.transform.localPosition += slatDeflectionAxis * (slatPosition/100);}currentSlatPosition = slatPosition;
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate () {
		
		if (isControllable) {
			//SEND BUILD DATA
			BuildFoilStructure (false);

			//ANALYSE FORCES
			if (rootAirfoil && tipAirfoil && connectedAircraft != null) {
				BuildFoilForces ();
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void BuildFoilForces()
	{

		//BUILD FORCE CONTAINERS
		panelLiftForces = new List<Vector3> (foilSubdivisions);
		panelDragForces = new List<Vector3> (foilSubdivisions);CenterOfPressures = new List<Vector3> (foilSubdivisions);
		panelMoments = new List<Vector3> (foilSubdivisions);
		TotalDrag = 0; TotalLift = 0; TotalMoment = 0; TotalSkinDrag= 0; TotalSpoilerDrag = 0; TotalBaseDrag = 0;
		TotalFlapDrag = 0f;TotalControlDrag = 0f;int flapFactor = 1;


		for (int p = 0; p < foilSubdivisions; p++) {
			float currentSection = (float)p, nextSection = (float)(p + 1), sectionLength = (float)foilSubdivisions, sectionTipChord, sectionRootChord;
			float sectionFactor = currentSection / sectionLength;float sectionPlus = nextSection / sectionLength,panelWettedArea;
			//MARK PANEL CORNERS
			Vector3 LeadEdgeRight,LeadEdgeLeft,TrailEdgeRight,TrailEdgeLeft,geometricCenter,LeadCenter,TrailCenter,PanelCenter;
			Vector3 sectionRootCenter, sectionTipCenter,parallelFlow,normalWind,liftDirection,panelLiftForce,panelPitchTorque,panelDragForce;
			LeadEdgeRight = EstimateSectionPosition (RootChordLeading, TipChordLeading, sectionPlus);LeadEdgeLeft = EstimateSectionPosition (RootChordLeading, TipChordLeading, sectionFactor);
			TrailEdgeLeft = EstimateSectionPosition (RootChordTrailing, TipChordTrailing, sectionFactor);TrailEdgeRight = EstimateSectionPosition (RootChordTrailing, TipChordTrailing, sectionPlus);
			float panelArea = EstimatePanelSectionArea(LeadEdgeLeft,LeadEdgeRight,TrailEdgeLeft,TrailEdgeRight);

			//READJUST CAMBER IF FLAP SURFACE IS ACTIVE
			if (flapSections [p] == true && canUseFlap && aerofoilType == AerofoilType.Wing) {
				//ADD EXTENSION LINE
				Vector3 rightFlapExtension,leftFlapExtension;
				EstimateControlExtension(LeadEdgeLeft,TrailEdgeRight,TrailEdgeLeft,LeadEdgeRight,flapRootChord,flapTipChord,flapDeflection,out leftFlapExtension,out rightFlapExtension);
				TrailEdgeLeft = EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (flapRootChord * 0.01f)) + leftFlapExtension;
				TrailEdgeRight = EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (flapTipChord * 0.01f)) + rightFlapExtension;

				//FLAP DRAG
				if(flapType == FlapType.Plain){flapFactor = 1;}if(flapType == FlapType.Split){flapFactor = 2;}
				float flapDragCoefficient = EstimateDeflectionDrag(flapRootChord,Mathf.Abs(flapDeflection),flapFactor,flapArea);
				float flapDrag = (flapArea/flapSections.Length) * flapDragCoefficient * 0.5f * airDensity * Mathf.Pow (airflow.magnitude, 2);;
				TotalFlapDrag += flapDrag;
			}

			//READJUST CAMBER IF CONTROL SURFACE IS ACTIVE
			if (controlSections [p] == true && surfaceType != SurfaceType.Inactive && availableControls != AvailableControls.SecondaryOnly) {
				//ADD EXTENSION LINE
				Vector3 rightExtension,leftExtension;
				EstimateControlExtension(LeadEdgeLeft,TrailEdgeRight,TrailEdgeLeft,LeadEdgeRight,controlRootChord,controlTipChord,controlDeflection,out leftExtension,out rightExtension);
				TrailEdgeLeft = EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (controlRootChord * 0.01f)) + leftExtension;
				TrailEdgeRight = EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (controlTipChord * 0.01f)) + rightExtension;
			}

			//CALCULATE CENTER OF PANEL
			geometricCenter = EstimateGeometricCenter(LeadEdgeRight,TrailEdgeRight,LeadEdgeLeft,TrailEdgeLeft);
			LeadCenter = EstimateSectionPosition(LeadEdgeLeft,LeadEdgeRight,0.5f);TrailCenter = EstimateSectionPosition (TrailEdgeLeft, TrailEdgeRight, 0.5f);
			PanelCenter = LeadCenter - TrailCenter;PanelCenter.Normalize ();
			sectionTipChord = Vector3.Distance (LeadEdgeRight,TrailEdgeRight);sectionRootChord = Vector3.Distance (LeadEdgeLeft, TrailEdgeLeft);
			float sectionTaper = sectionTipChord / sectionRootChord;

			sectionRootCenter = EstimateSectionPosition (LeadEdgeLeft, TrailEdgeLeft, 0.5f);sectionTipCenter = EstimateSectionPosition (LeadEdgeRight, TrailEdgeRight, 0.5f);
			float sectionSpan = Vector3.Distance (sectionRootCenter, sectionTipCenter);float yM = (sectionSpan / 6) * ((1 + (2 * sectionTaper)) / (1 + sectionTaper));
			float factor = yM / sectionSpan;

			Vector3 yMGCTop = EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (1 - factor));
			Vector3 yMGCBottom = EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (1 - factor));

			airflow = -connectedAircraft.velocity;
			Vector3 circulation = Vector3.Cross (connectedAircraft.angularVelocity.normalized, (geometricCenter - connectedAircraft.worldCenterOfMass).normalized);
			circulation *= -((connectedAircraft.angularVelocity.magnitude) * (geometricCenter - connectedAircraft.worldCenterOfMass).magnitude);
			airflow += circulation;
			//CONSIDER FLOW ONLY PARALLEL TO THE TRANFORM
			parallelFlow = (transform.right*(Vector3.Dot(transform.right, airflow)));airflow -= parallelFlow;
			normalWind = airflow.normalized;
			angleOfAttack = Mathf.Acos (Vector3.Dot (PanelCenter, -normalWind))* Mathf.Rad2Deg;
			liftDirection = Vector3.Cross (PanelCenter, (yMGCBottom - yMGCTop).normalized);liftDirection.Normalize ();
			if (transform.localScale.x < 0.0f) {liftDirection = -liftDirection;}
			//DETERMINE IF AIR IS FLOWING ABOVE OR BELOW
			if (Vector3.Dot (liftDirection, normalWind) < 0.0f) {angleOfAttack = -angleOfAttack;}

			if (sweepCorrectionMethod == SweepCorrection.RaymerJenkinson) {sweepCorrectionFactor = Mathf.Pow ((Mathf.Cos (leadingEdgeSweep * Mathf.Deg2Rad)), 3);} 
			else if (sweepCorrectionMethod == SweepCorrection.YoungLE){sweepCorrectionFactor = Mathf.Cos (quaterSweep * Mathf.Deg2Rad);}
			else if (sweepCorrectionMethod == SweepCorrection.None){sweepCorrectionFactor = 1;}
			//GROUND EFFECT CONSIDER
			groundInfluenceFactor = 1f;
			if (effectState == GroundEffectState.Consider) {
				Vector3 baseDirection = transform.rotation * groundAxis;
				Ray groundCheck = new Ray (geometricCenter, baseDirection);RaycastHit groundHit = new RaycastHit ();
				if (Physics.Raycast (groundCheck, out groundHit, foilSpan, groundLayer)) {
					float spanRatio = groundHit.distance / foilSpan;
					//1. WEISELBURGER
					if (groundInfluenceMethod == GroundEffectMethod.Weiselburger) {
					float GA = 1 - (1.32f * spanRatio);float GB = 1.05f + (7.4f * spanRatio);groundInfluenceFactor = 1 - (GA / GB);}
					//2. McCOEMICK
					if (groundInfluenceMethod == GroundEffectMethod.McCormick) {
					float GA = Mathf.Pow ((16f * spanRatio), 2);groundInfluenceFactor = GA / (1 + GA);}
					//3. ASSELIN
					if (groundInfluenceMethod == GroundEffectMethod.Asselin) {
					float GA = 1 + (3.142f / (8f * spanRatio));float GB = (2 / (3.142f * 3.142f)) * Mathf.Log (GA);groundInfluenceFactor = 1 - GB;}
					
				}
			}


			//ANALYSE FORCES
			float dynamicPressure = 0.5f * airDensity * Mathf.Pow (airflow.magnitude, 2);airSpeed = connectedAircraft.velocity.magnitude*1.94f;
			float panelDistance = Vector3.Distance (RootChordTrailing, TrailEdgeLeft);


			if (angleOfAttack > -180f || angleOfAttack < 180) {


				Vector3 centerOfPressure = new Vector3();
				//ESTIMATE CENTER OF PRESSURE
				if (forceSystem == ForceSystem.CenterOfPressure) {
					if (rootAirfoil.pressureCurve != null && tipAirfoil.pressureCurve != null) {
						float rootCenterOfPressure = rootAirfoil.pressureCurve.Evaluate (angleOfAttack);
						float tipCenterOfPressure = tipAirfoil.pressureCurve.Evaluate (angleOfAttack);
						float effectiveCenterOfPressure = EstimateEffectiveValue (rootCenterOfPressure, tipCenterOfPressure, panelDistance, 0);
						centerOfPressure = EstimateSectionPosition (LeadCenter, TrailCenter, effectiveCenterOfPressure);
						CenterOfPressures.Add (centerOfPressure);
					} else {
						//USE AERODYNMAIC CENTER
						centerOfPressure = geometricCenter;
					}
				} else {
					//USE AERODYNMAIC CENTER
					centerOfPressure = geometricCenter;
				}


				//$$$ LIFT
				float panelLift = 0;
				float rootLiftCoefficient = rootAirfoil.liftCurve.Evaluate (angleOfAttack);
				float tipLiftCoefficient = tipAirfoil.liftCurve.Evaluate (angleOfAttack);
				float effectiveLiftCoefficient = EstimateEffectiveValue (rootLiftCoefficient, tipLiftCoefficient, panelDistance, 0);
				//SLAT CORRECTION
				if (slatSections [p] == true) {effectiveLiftCoefficient += slatLiftFactor;}
				//GROUND EFFECT CORRECTION
				if (effectState == GroundEffectState.Consider) {effectiveLiftCoefficient /= Mathf.Sqrt (groundInfluenceFactor);}
				//FINALIZE
				panelLift = sweepCorrectionFactor * panelArea * effectiveLiftCoefficient * dynamicPressure;

				//CONSIDER SPOILER EFFECTS
				float panelSpoilerDrag = 0f;
				if (spoilerSections [p] == true && canUseSpoilers && aerofoilType == AerofoilType.Wing) {
					float actualSpoilerDeflection = spoilerDeflection;if(actualSpoilerDeflection < 0){actualSpoilerDeflection *= -1f;}
					spoilerFactor = (maximumSpoilerDeflection -  actualSpoilerDeflection)/ maximumSpoilerDeflection;spoilerFactor = Mathf.Clamp (spoilerFactor, 0, 1);panelLift *= spoilerFactor;
					float spoilerAngle = 90 - actualSpoilerDeflection;
					panelSpoilerDrag = ((spoilerArea) / spoilerSections.Length) * spoilerDragFactor * dynamicPressure * Mathf.Cos (spoilerAngle * Mathf.Deg2Rad);
					TotalSpoilerDrag += panelSpoilerDrag;
				}
				TotalLift += panelLift;

				//CONSIDER SLAT EFFECTS
				if (slatSections [p] == true) {
					slatLiftFactor =  (Clmax * ((slatDeflection / 25.6f) * (slatRootChord / 17f)));
					slatDragFactor = (Cdmax * ((slatDeflection / 25.6f) * (slatRootChord / 17f)));
					slatMomentFactor = (Cmmax * ((slatDeflection / 25.6f) * (slatRootChord / 17f)));
				}

				//BUILD LIFT FORCE
				panelLiftForce = Vector3.Cross (transform.right, airflow);panelLiftForce.Normalize ();panelLiftForce *= panelLift;
				panelLiftForces.Add (panelLiftForce);connectedAircraft.AddForceAtPosition(panelLiftForce,centerOfPressure,ForceMode.Force);

				//$$$ PITCH MOMENT
				float panelPitchMoment = 0f;
				float sectionMeanChord = EstimateMeanChord (sectionRootChord, sectionTipChord);
				float rootMomentCoefficient = rootAirfoil.momentCurve.Evaluate (angleOfAttack);
				float tipMomentCoefficient = tipAirfoil.momentCurve.Evaluate (angleOfAttack);
				float effectiveMomentCoefficient = EstimateEffectiveValue (rootMomentCoefficient, tipMomentCoefficient, panelDistance, 0);if (slatSections [p] == true) {effectiveMomentCoefficient += slatMomentFactor;}
				//ASPECT RATIO CORRECTION
				effectiveMomentCoefficient *= AspectRatio/(AspectRatio+4f);

				panelPitchMoment = dynamicPressure * panelArea * sectionMeanChord*sectionMeanChord * effectiveMomentCoefficient;TotalMoment += panelPitchMoment;
				panelPitchTorque = Vector3.Cross (PanelCenter, panelLiftForce.normalized);panelPitchTorque.Normalize ();panelPitchTorque *= panelPitchMoment;
				panelMoments.Add (panelPitchTorque);connectedAircraft.AddTorque (panelPitchTorque, ForceMode.Force);

				//$$ BASE DRAG
				float panelDrag = 0f;
				float rootDragCoefficient = rootAirfoil.dragCurve.Evaluate (angleOfAttack);
				float tipDragCoefficient = tipAirfoil.dragCurve.Evaluate (angleOfAttack);
				float effectiveDragCoefficient = EstimateEffectiveValue (rootDragCoefficient, tipDragCoefficient, panelDistance, 0);if (slatSections [p] == true) {effectiveDragCoefficient += slatDragFactor;}
				if (effectState == GroundEffectState.Consider) {effectiveDragCoefficient *= groundInfluenceFactor;}

				//ASPECT RATIO CORRECTION
				float piFactor = (6.28f/(AspectRatio*3.142f));
				if (AspectRatio < 4) {Asf = (Mathf.Sqrt ((1 - Mathf.Pow (machSpeed, 2)) + Mathf.Pow (piFactor, 2)) + piFactor);} 
				else {Asf = (Mathf.Sqrt ((1 - Mathf.Pow (machSpeed, 2))) + piFactor);}
				float correctedLift = (effectiveLiftCoefficient / Asf);
				float cdi = (correctedLift*correctedLift)/(3.142f*(AspectRatio+effectiveChange)*spanEfficiency);
				effectiveDragCoefficient += cdi;
				panelDrag = panelArea * effectiveDragCoefficient * dynamicPressure* Mathf.Cos (leadingEdgeSweep * Mathf.Deg2Rad);TotalBaseDrag += panelDrag;
				if (spoilerSections [p] == true) {panelDrag += panelSpoilerDrag;}panelDragForce = airflow;panelDragForce.Normalize ();

				//SKIN DRAG
				float Cf = EstimateSkinDragCoefficient (sectionRootChord, sectionTipChord, airSpeed);CF = Cf;
				float effectiveThickness = EstimateEffectiveValue (rootAirfoil.maximumThickness, tipAirfoil.maximumThickness, panelDistance, 0);
				panelWettedArea = panelArea * (1.977f + (0.52f * effectiveThickness));
				float panelSkinDrag = dynamicPressure * panelWettedArea * Cf;
				//DRAG SUM
				float panelTotalDrag = (panelDrag+panelSpoilerDrag);if(panelSkinDrag > 0){panelTotalDrag += panelSkinDrag;}TotalSkinDrag += panelSkinDrag;

				//CONTROL DRAG
				float controlDragCoefficient = EstimateDeflectionDrag(controlRootChord,Mathf.Abs(controlDeflection),1,controlArea);
				float controlDrag = (controlArea/controlSections.Length) * controlDragCoefficient * dynamicPressure;
				TotalControlDrag += controlDrag;

				//BUILD FORCE
				panelDragForce *= (panelTotalDrag+controlDrag);TotalDrag += (panelTotalDrag+controlDrag);
				if ((panelTotalDrag + controlDrag) > 0) {
					panelDragForces.Add (panelDragForce);connectedAircraft.AddForce (panelDragForce, ForceMode.Force);
				}
			}
		}
	}
		

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private Vector3 EstimateModelProperties(string deflectionDirection,string axis)
	{
		Vector3 defaultAxis = new Vector3 (0, 0, 0);if (deflectionDirection == "CCW") {
			if (axis == "X") {defaultAxis = new Vector3 (-1, 0, 0);} else if (axis == "Y") {defaultAxis = new Vector3 (0, -1, 0);} else if (axis == "Z") {defaultAxis = new Vector3 (0, 0, -1);}} 
		else {if (axis == "X") {defaultAxis = new Vector3 (1, 0, 0);} else if (axis == "Y") {defaultAxis = new Vector3 (0, 1, 0);} else if (axis == "Z") {defaultAxis = new Vector3 (0, 0, 1);}}
		//RETURN
		defaultAxis.Normalize();return defaultAxis;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateDeflectionDrag(float Rf,float deflection,int factor,float area)
	{
		Rf /= 100f;
		//PLAIN
		if (factor == 1) {
			factor1 = (4.6945f * Rf * Rf) + (4.3721f * Rf) - 0.031f;
			factor2 = ((-3.795f / 10000000) * Mathf.Pow (deflection, 3)) +
				((5.387f / 100000) * Mathf.Pow (deflection, 2)) +
				((6.843f / 10000) * deflection) - (1.4729f / 1000);
		}
		//SPLIT
		if (factor == 2) {
			factor1 = (4.6945f * Rf * Rf) + (4.3721f * Rf) - 0.031f;
			factor2 = ((-3.2740f / 10000000) * Mathf.Pow (deflection, 3)) +
				((5.598f / 100000) * Mathf.Pow (deflection, 2)) -
				((1.2443f / 10000) * deflection) + (5.1647f / 10000);
		}
		//SLOTTED
		if (factor == 3) {
			factor1 = (8.2658f * Rf * Rf) + (3.4564f * Rf) +0.0054f;
			factor2 = ((-3.681f / 10000000) * Mathf.Pow (deflection, 3)) +
				((5.3342f / 100000) * Mathf.Pow (deflection, 2)) -
				((4.1677f / 1000) * deflection) + (6.749f / 10000);
		}
		//RETURN
		dragCoefficient = factor1*factor2*(area/foilArea);
		return dragCoefficient;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void BuildFoilStructure(bool draw)
	{
		//ASPECT RATIO CHANGE
		if (tipDesign == WingtipDesign.Endplate) {effectiveChange = (wingTipHeight/foilSpan);}
		if (tipDesign == WingtipDesign.Square) {effectiveChange = 0.004f;}
		if (tipDesign == WingtipDesign.Spherical) {effectiveChange = -0.18f;}
		if (tipDesign == WingtipDesign.Winglet) {effectiveChange = (wingTipHeight/foilSpan);}
		if (tipDesign == WingtipDesign.Round) {effectiveChange = -0.19f;}

		//ESTIMATE EFFICIENCY
		float eA = Mathf.Pow((Mathf.Tan(quaterSweep*Mathf.Deg2Rad)),2);
		float eB = 4f + ((AspectRatio * AspectRatio) * (1 + eA));
		spanEfficiency = 2/(2-AspectRatio+Mathf.Sqrt(eB));

		//BUILD POINT MAPPING SCALES
		Vector3 scaleFactor,supremeFactor,baseTip,TipPivot,RootScale;
		float foilLength, foilWidth, parentLength, parentWidth, taperRatio, combinedScale,wingTaper;
		//BUILD VARIABLES
		#if UNITY_EDITOR
		RootScale = transform.root.localScale;//Only do this in editor view for setup
		#else
		if (connectedAircraft != null) {RootScale = connectedAircraft.transform.localScale;} else {RootScale = new Vector3 (1, 1, 1);}
		#endif
		foilLength = transform.localScale.x;foilWidth = transform.localScale.z;wingTaper = 100 - taperPercentage;
		parentLength = RootScale.x;parentWidth = RootScale.z;
		scaleFactor = transform.forward * (parentWidth * foilWidth * 0.5f);taperRatio = wingTaper / 100f;
		supremeFactor = transform.forward * ((parentWidth * foilWidth * 0.5f)*(wingTaper/100));
		combinedScale = RootScale.magnitude * transform.localScale.magnitude;
		if (sweepDirection == SweepDirection.Unswept) {sweepangle = aerofoilSweepAngle = 0;}
		else if (sweepDirection == SweepDirection.Forward){sweepangle = aerofoilSweepAngle;} 
		else if (sweepDirection == SweepDirection.Backward){sweepangle = -aerofoilSweepAngle;}

		if (twistDirection == TwistDirection.Untwisted) {actualTwist = wingTwist = 0;} 
		else if (twistDirection == TwistDirection.Downwards) {actualTwist = wingTwist;} 
		else if (twistDirection == TwistDirection.Upwards) {actualTwist = -wingTwist;}

		//WING SWEEP
		float tanc4 = Mathf.Tan((leadingEdgeSweep*Mathf.Deg2Rad)) + ((foilRootChord/(4*foilSpan))*(taperRatio-1));
		quaterSweep = Mathf.Atan (tanc4) * Mathf.Rad2Deg;
		float tanc2 = tanc4 - ((2 / AspectRatio) * (0.25f * ((1 - taperRatio) / (1 + taperRatio))));
		tipCenterSweep = Mathf.Atan (tanc2) * Mathf.Rad2Deg;

		//MARK CHORD CENTER POINTS
		RootChordCenter = transform.position - (transform.right * (parentLength*foilLength * 0.5f));
		TipChordCenter = (transform.position + (transform.right * (parentLength*foilLength * 0.5f)))+(transform.forward*(sweepangle/90)*combinedScale);
		foilSpan = Vector3.Distance (RootChordCenter, TipChordCenter);

		//MARK FOIL BASE POINTS
		RootChordLeading = RootChordCenter + scaleFactor;RootChordTrailing = RootChordCenter - scaleFactor;
		TipChordLeading = TipChordCenter + supremeFactor;TipChordTrailing = TipChordCenter - supremeFactor;
		baseTip = (transform.position + (transform.right * (parentLength*foilLength * 0.5f))) + transform.forward * ((parentWidth * foilWidth * 0.5f));

		float tipDistance = Vector3.Distance (baseTip, TipChordLeading);
		leadingEdgeSweep = Mathf.Abs(Mathf.Atan (tipDistance/foilLength) * Mathf.Rad2Deg);
		//REPOSITION TIP POINTS BASED ON TWIST 
		TipPivot = (Quaternion.AngleAxis(actualTwist,transform.rotation* transform.right.normalized) * (TipChordLeading - TipChordTrailing))*0.5f;
		TipChordTrailing = TipChordCenter - TipPivot;TipChordLeading = TipChordCenter + TipPivot;
		TipChordCenter = EstimateSectionPosition (TipChordLeading, TipChordTrailing, 0.5f);

		//DRAW GROUND EFFECT
		if(effectState == GroundEffectState.Consider && draw){
			for (int p = 1; p < foilSubdivisions; p++) {
			float currentSection = (float)p,nextSection = (float)(p + 1);float sectionLength = (float)foilSubdivisions;float sectionFactor = currentSection / sectionLength;
			float sectionPlus = nextSection / sectionLength;Vector3 LeadingPointA,TrailingPointA,LeadingPointB,TrailingPointB;
			TrailingPointA = EstimateSectionPosition (RootChordTrailing, TipChordTrailing, sectionFactor);LeadingPointA = EstimateSectionPosition (RootChordLeading, TipChordLeading, sectionFactor);
			TrailingPointB = EstimateSectionPosition (RootChordTrailing, TipChordTrailing, sectionPlus);LeadingPointB = EstimateSectionPosition (RootChordLeading, TipChordLeading, sectionPlus);
			//GROUND EFFECT CORRECTION
			Vector3 geometricCenter = EstimateGeometricCenter(LeadingPointA,TrailingPointA,LeadingPointB,TrailingPointB);Vector3 baseDirection = transform.rotation * groundAxis;
			Ray groundCheck = new Ray (geometricCenter, baseDirection);RaycastHit groundHit = new RaycastHit ();
			if (Physics.Raycast (groundCheck, out groundHit, foilSpan, groundLayer)) {
					float spanRatio = groundHit.distance / foilSpan;float blue = spanRatio;float red = spanRatio;spanRatio = Mathf.Clamp (spanRatio, 0.0f, 1.0f);Color lineColor = new Color (red, 1, blue);
					Debug.DrawLine (geometricCenter, groundHit.point, lineColor);
				}
			}
		}


		//CALCULATE FOIL DIMENSIONS
		foilRootChord = Vector3.Distance(RootChordTrailing, RootChordLeading);
		foilTipChord = Vector3.Distance(TipChordLeading, TipChordTrailing);
		foilMeanChord = EstimateMeanChord (foilRootChord, foilTipChord);
		foilArea = EstimatePanelArea (foilSpan, foilRootChord, foilTipChord);AspectRatio = (foilSpan*foilSpan) / (foilArea);
		if (rootAirfoil && tipAirfoil) {
			float meanThickness = (rootAirfoil.maximumThickness + tipAirfoil.maximumThickness)/2f;
			foilWettedArea = foilArea * (1.977f + (0.52f * meanThickness));
		}
		quaterRootChordPoint = EstimateSectionPosition (RootChordLeading, RootChordTrailing, 0.25f);
		quaterTipChordPoint = EstimateSectionPosition (TipChordLeading, TipChordTrailing, 0.25f);

		if (tipDesign == WingtipDesign.Winglet || tipDesign == WingtipDesign.Endplate) {
			//WINGLET
			Vector3 wingletDirection = transform.up.normalized;float actualSweep = wingTipSweep;if(transform.localScale.x < 0){actualSweep *= -1;}
			wingLetCenter = (EstimateSectionPosition (TipChordLeading, TipChordTrailing, 0.5f) + (transform.right * (wingTipHeight * 0.5f))) + (transform.forward * (actualSweep / 90) * 1);

			Vector3 tipDirection =  wingLetCenter - TipChordCenter;
			float actualBend = wingTipBend;if(transform.localScale.x < 0){tipDirection *= -1; actualBend *= -1;}
			tipDirection = Quaternion.Euler (transform.forward.normalized * actualBend) * tipDirection;
			wingLetCenter = tipDirection + TipChordCenter;


			Vector3 tipFactor = transform.forward * ((foilTipChord * 0.5f) * (TipTaperPercentage / 100));
			wingTipLeading = (wingLetCenter + tipFactor);
			wingTipTrailing = wingLetCenter - tipFactor;
			wingTipPivot = (Quaternion.AngleAxis (actualTwist, transform.rotation * transform.right.normalized) * (wingTipLeading - wingTipTrailing)) * 0.5f;
			wingTipLeading = wingLetCenter + wingTipPivot;wingTipTrailing = wingLetCenter - wingTipPivot;
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private Vector3 EstimateGeometricCenter(Vector3 LeadEdgeRight,Vector3 TrailEdgeRight, Vector3 LeadEdgeLeft, Vector3 TrailEdgeLeft)
	{
		Vector3 center,sectionRootCenter,sectionTipCenter,yMGCTop,yMGCBottom;
		float sectionTipChord, sectionRootChord,sectionTaper;

		//CALCULATE GEOMETRIC CENTER FACTOR
		sectionTipChord = Vector3.Distance (LeadEdgeRight,TrailEdgeRight);
		sectionRootChord = Vector3.Distance (LeadEdgeLeft, TrailEdgeLeft);
		sectionTaper = sectionTipChord / sectionRootChord;

		sectionRootCenter = EstimateSectionPosition (LeadEdgeLeft, TrailEdgeLeft, 0.5f);
		sectionTipCenter = EstimateSectionPosition (LeadEdgeRight, TrailEdgeRight, 0.5f);
		float sectionSpan = Vector3.Distance (sectionRootCenter, sectionTipCenter);
		float yM = (sectionSpan / 6) * ((1 + (2 * sectionTaper)) / (1 + sectionTaper));
		float factor = yM / sectionSpan;

		yMGCTop = EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (1 - factor));
		yMGCBottom = EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (1 - factor));
		center = EstimateSectionPosition (yMGCTop, yMGCBottom, 0.5f);

		return center;
	}



	#if UNITY_EDITOR
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnDrawGizmos()
	{
		//BASE FUNCTIONS
		BuildFoilStructure (true);PlotSpoilerData ();
		//SWEEP CORRECTION
		if (sweepCorrectionMethod == SweepCorrection.RaymerJenkinson) {sweepCorrectionFactor = Mathf.Pow ((Mathf.Cos (leadingEdgeSweep * Mathf.Deg2Rad)), 3);} 
		else if (sweepCorrectionMethod == SweepCorrection.YoungLE){sweepCorrectionFactor = Mathf.Cos (quaterSweep * Mathf.Deg2Rad);}
		else if (sweepCorrectionMethod == SweepCorrection.None){sweepCorrectionFactor = 1;}
		if(controlState == ControType.Controllable){
		//BASE CONTROL
		if (controlSections == null || foilSubdivisions != controlSections.Length) {controlSections = new bool[foilSubdivisions];}

			if (surfaceType == SurfaceType.Aileron) {controlColor = Color.green;}
			if (surfaceType == SurfaceType.Elevon) {controlColor = new Color(0,0,0.5f);}
			if (surfaceType == SurfaceType.Elevator) {controlColor = Color.blue;}
			if (surfaceType == SurfaceType.Rudder) {controlColor = Color.red;}
			if (surfaceType == SurfaceType.Ruddervator) {controlColor = new Color(0.5f,0,0f);}

			if(surfaceType != SurfaceType.Inactive && availableControls != AvailableControls.SecondaryOnly){controlArea = EstimateControlSurface (controlTipChord, controlRootChord,controlSections,  controlColor,false,false,controlDeflection,true);}
		//FLAP
		if (flapSections == null || foilSubdivisions != flapSections.Length) {flapSections = new bool[foilSubdivisions];}
			if(canUseFlap && availableControls != AvailableControls.PrimaryOnly){flapArea = EstimateControlSurface (flapTipChord, flapRootChord,flapSections,  Color.yellow,false,false,flapDeflection,true);}
		//SLAT
		if (slatSections == null || foilSubdivisions != slatSections.Length) {slatSections = new bool[foilSubdivisions];}
			if(canUseSlat && availableControls != AvailableControls.PrimaryOnly){slatArea = EstimateControlSurface (slatTipChord, slatRootChord,slatSections, Color.magenta,true,false,0,false);}
		//SPOILERS
		if (spoilerSections == null || foilSubdivisions != spoilerSections.Length) {spoilerSections = new bool[foilSubdivisions];}
			if(canUseSpoilers && availableControls != AvailableControls.PrimaryOnly){spoilerArea = EstimateControlSurface (spoilerChordFactor, spoilerChordFactor,spoilerSections, Color.cyan,false,true,spoilerDeflection,true);}
		}
		//MAKE SURE COLLIDER IS ALWAYS PRESENT
		foilCollider = gameObject.GetComponent<BoxCollider> ();
		if (foilCollider == null) {foilCollider = gameObject.AddComponent<BoxCollider> ();}
		if (foilCollider != null) {foilCollider.size = new Vector3 (1.0f, 0.1f, 1.0f);}
		//MAKE SURE FLAPS ARNT ON BASE CONTROL PANELS


		if (tipDesign == WingtipDesign.Winglet || tipDesign == WingtipDesign.Endplate) {
			float yMt = Vector3.Distance (RootChordTrailing, wingTipTrailing);
			if (rootAirfoil != null && tipAirfoil != null) {
				PlotRibAirfoil (wingTipLeading, wingTipTrailing, yMt, wingTipHeight, Color.yellow);
			}
			//DRAW WINGLET OUTLINE
			Gizmos.color = Color.yellow;Gizmos.DrawLine (wingTipLeading, wingTipTrailing);
			Gizmos.color = Color.yellow;Gizmos.DrawLine (wingTipLeading, TipChordLeading);
			Gizmos.color = Color.yellow;Gizmos.DrawLine (wingTipTrailing, TipChordTrailing);
			Gizmos.color = Color.red;Gizmos.DrawLine (wingLetCenter, TipChordCenter);
		}

		//DRAW CONNECTION POINTS
		Handles.DrawDottedLine(RootChordCenter,TipChordCenter,4f);
		Handles.color = Color.yellow;
		Handles.DrawDottedLine(quaterRootChordPoint,quaterTipChordPoint,4f);
		GUIStyle style = new GUIStyle ();style.normal.textColor = Color.yellow;

		//DRAW AIRFOIL LAYOUT
		if(drawSectionRibs){
		if (rootAirfoil != null) {PlotAirfoil (RootChordLeading, RootChordTrailing, rootAirfoil, 2);}
		if (tipAirfoil != null) {PlotAirfoil (TipChordLeading, TipChordTrailing, tipAirfoil, 1);}}

		//DIVIDE WING INTO PANELS (WITHOUT THE FIRST AND LAST SECTION LINE)
		for (int p = 1; p < foilSubdivisions; p++) {
			float currentSection = (float)p,nextSection = (float)(p + 1);float sectionLength = (float)foilSubdivisions;float sectionFactor = currentSection / sectionLength;
			float sectionPlus = nextSection / sectionLength;Vector3 LeadingPointA,TrailingPointA,LeadingPointB,TrailingPointB;
			TrailingPointA = EstimateSectionPosition (RootChordTrailing, TipChordTrailing, sectionFactor);LeadingPointA = EstimateSectionPosition (RootChordLeading, TipChordLeading, sectionFactor);
			TrailingPointB = EstimateSectionPosition (RootChordTrailing, TipChordTrailing, sectionPlus);LeadingPointB = EstimateSectionPosition (RootChordLeading, TipChordLeading, sectionPlus);
			//MARK CENTER AND DRAW
			Gizmos.color = Color.yellow;Gizmos.DrawLine (LeadingPointA, TrailingPointA);
			float yM = Vector3.Distance (RootChordTrailing, TrailingPointA);
			if (drawSectionRibs && rootAirfoil != null && tipAirfoil != null) {PlotRibAirfoil (LeadingPointA,TrailingPointA, yM,0,Color.yellow);}

			Vector3 AC = EstimateGeometricCenter (LeadingPointB,TrailingPointB,LeadingPointA,TrailingPointA);
			Handles.Label (AC, "AC: "+(p+1).ToString (),style);Gizmos.color = Color.red;Gizmos.DrawSphere(AC,0.02f);

			if(forceSystem == ForceSystem.CenterOfPressure){
			if(CenterOfPressures.Count == foilSubdivisions){Vector3 CP = CenterOfPressures [p];
			Handles.Label (CP, "CP: "+(p+1).ToString (),style);Gizmos.color = Color.blue;Gizmos.DrawSphere(CP,0.02f);}
			}
		}

		//DRAW END POINTS
		Gizmos.color = Color.blue;Gizmos.DrawSphere(RootChordLeading,0.07f);
		Gizmos.color = Color.red;Gizmos.DrawSphere(RootChordTrailing,0.07f);
		Gizmos.color = Color.green;Gizmos.DrawSphere(TipChordLeading,0.07f);
		Gizmos.color = Color.grey;Gizmos.DrawSphere(TipChordTrailing,0.07f);
		//DRAW EDGE LINES
		Gizmos.color = Color.yellow;Gizmos.DrawLine (RootChordLeading, TipChordLeading);//LEADING SPAN
		Gizmos.color = Color.red;Gizmos.DrawLine (TipChordTrailing, RootChordTrailing);//TRAILING SPAN

		//DRAW PANEL
		if (showPanel) {
			if(tipPoints != null && rootPoints != null){
			for (int j = 0; (j < tipPoints.Count / 2); j++) {Vector3 PR = rootPoints [j];Vector3 PT = tipPoints [j];Gizmos.color = Color.yellow;Gizmos.DrawLine (PR, PT);}}
			else{Debug.LogError ("Airfoil for wing " + transform.name + " not plotted properly");}
		}
	}
	#endif



	//DRAW FOIL RIBS
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PlotRibAirfoil(Vector3 leadingPoint,Vector3 trailingPoint,float distance,float wingTip,Color ribColor)
	{
		points = new List<Vector3> ();List<float> xt = new List<float> ();float chordDistance = Vector3.Distance(leadingPoint,trailingPoint);
		//FIND POINTS
		if (rootAirfoil.x.Count > 0) {for (int j = 0; (j < rootAirfoil.x.Count); j++) {
				float xi = EstimateEffectiveValue (rootAirfoil.x [j], tipAirfoil.x [j], distance,wingTip);float yi = EstimateEffectiveValue (rootAirfoil.y [j], tipAirfoil.y [j], distance,wingTip);
				//BASE POINT
				Vector3 XA = leadingPoint - ((leadingPoint - trailingPoint).normalized * (xi * chordDistance));Vector3 liftDirection = transform.up;PointXA = XA + (liftDirection * yi * chordDistance);points.Add (PointXA);
				if ((j + 1) < rootAirfoil.x.Count) {
					float xii = EstimateEffectiveValue (rootAirfoil.x [j + 1], tipAirfoil.x [j + 1], distance,wingTip);float yii = EstimateEffectiveValue (rootAirfoil.y [j + 1], tipAirfoil.y [j + 1], distance,wingTip);
					Vector3 XB = (leadingPoint - ((leadingPoint - trailingPoint).normalized * (xii * chordDistance)));PointXB = XB + (liftDirection.normalized * (yii * chordDistance));
				}
				//CONNECT
				Gizmos.color = ribColor;Gizmos.DrawLine (PointXA, PointXB);
			}
		}
		if(drawRibSplits){xt = new List<float> ();for (int jx = 0; (jx < points.Count); jx++) {xt.Add (Vector3.Distance (points [jx], points [(points.Count - jx - 1)]));Gizmos.color = ribColor;Gizmos.DrawLine (points [jx], points [(points.Count - jx - 1)]);}}
	}


	//DRAW FOIL END POINTS
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PlotAirfoil(Vector3 leadingPoint,Vector3 trailingPoint, SilantroAirfoil foil,int point)
	{
		points = new List<Vector3> ();List<float> xt = new List<float> ();float chordDistance = Vector3.Distance(leadingPoint,trailingPoint);
		//FIND POINTS
		if (foil.x.Count > 0) {for (int j = 0; (j < foil.x.Count); j++) {
				//BASE POINT
				Vector3 XA = leadingPoint - ((leadingPoint-trailingPoint).normalized * (foil.x [j] * chordDistance));Vector3 liftDirection = transform.up.normalized;
				PointA = XA+(liftDirection*((foil.y [j]) * chordDistance));points.Add (PointA);if ((j + 1) < foil.x.Count) {Vector3 XB = (leadingPoint - ((leadingPoint-trailingPoint).normalized * (foil.x [j + 1] * chordDistance)));PointB = XB + (liftDirection.normalized*((foil.y [j + 1]) * chordDistance));}
				//CONNECT
				Gizmos.color = Color.white;Gizmos.DrawLine(PointA,PointB);
			}
		}
		//PERFORM CALCULATIONS
		xt = new List<float> ();
		for (int j = 0; (j < points.Count); j++) {xt.Add (Vector3.Distance (points [j], points [(points.Count - j - 1)]));Gizmos.DrawLine (points [j], points [(points.Count - j - 1)]);}
		if (point == 1) {tipPoints = points;tipAirfoilArea = Mathf.Pow (chordDistance, 2f) * (((foil.xtc * 0.01f) + 3) / 6f) * (foil.tc * 0.01f);}
		if (point == 2) {rootPoints = points;rootAirfoilArea = Mathf.Pow (chordDistance, 2f) * (((foil.xtc * 0.01f) + 3) / 6f) * (foil.tc * 0.01f);}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimatePanelSectionArea(Vector3 panelLeadingLeft, Vector3 panelLeadingRight, Vector3 panelTrailingLeft, Vector3 panelTrailingRight)
	{
		//BUILD TRAPEZOID VARIABLES
		float panelArea,panelLeadingEdge,panelTipEdge,panalTrailingEdge,paneRootEdge,panelDiagonal;
		//SOLVE
		panelLeadingEdge = (panelTrailingLeft - panelLeadingLeft).magnitude;panelTipEdge = (panelTrailingRight - panelTrailingLeft).magnitude;panalTrailingEdge = (panelLeadingRight - panelTrailingRight).magnitude;paneRootEdge = (panelLeadingLeft - panelLeadingRight).magnitude;
		panelDiagonal = 0.5f * (panelLeadingEdge + panelTipEdge + panalTrailingEdge + paneRootEdge);
		panelArea = Mathf.Sqrt(((panelDiagonal-panelLeadingEdge) * (panelDiagonal-panelTipEdge) * (panelDiagonal-panalTrailingEdge) * (panelDiagonal-paneRootEdge)));
		return panelArea;
	}
		

	//https://forum.kerbalspaceprogram.com/index.php?/topic/46067-control-surfaces-and-supersonic-flight-far/&do=findComment&comment=645011
	//https://i.ibb.co/TPRGKbp/20191107-135810.jpg
	public void EstimateControlExtension(Vector3 LeadEdgeLeft, Vector3 TrailEdgeRight, Vector3 TrailEdgeLeft, Vector3 LeadEdgeRight, float inputRootChord,float inputTipChord,float deflection, out Vector3 leftControlExtension,out Vector3 rightControlExtension)
	{
		leftControlExtension = (Quaternion.AngleAxis(deflection, ((EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)) - 
			EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f))).normalized))) * (TrailEdgeLeft - EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f)));
		rightControlExtension = (Quaternion.AngleAxis(deflection, ((EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)) -
			EstimateSectionPosition (TrailEdgeLeft, LeadEdgeLeft, (inputRootChord * 0.01f))).normalized))) * (TrailEdgeRight - EstimateSectionPosition (TrailEdgeRight, LeadEdgeRight, (inputTipChord * 0.01f)));
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimateEffectiveValue(float rootValue,float tipValue,float yM,float wingTip)
	{
		float baseValue = rootValue + ((yM/(foilSpan+wingTip))*(tipValue-rootValue));return baseValue;
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE RECTANGULAR AREA
	private float EstimatePanelArea(float span,float rootChord,float tipChord){float area = 0.5f * span * (rootChord + tipChord);return area;}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE FACTOR POSITION
	public Vector3 EstimateSectionPosition(Vector3 lhs,Vector3 rhs,float factor)
	{
		Vector3 estimatedPosition = lhs + ((rhs - lhs) * factor);return estimatedPosition;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimateMeanChord(float setionRootChord,float sectionTipChord)
	{
		float taperRatio = sectionTipChord / setionRootChord;
		float lamdaChord = (1 + taperRatio + (taperRatio * taperRatio))/(1+taperRatio);
		float sectionMeanChord = 0.66667f * setionRootChord*lamdaChord;
		return sectionMeanChord;
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateRe(float inputSpeed,float charLength)
	{
		float Re1 = (airDensity * inputSpeed * charLength) / viscocity;float Re2;
		if (machSpeed < 0.9f) {Re2 = 38.21f * Mathf.Pow (((charLength *3.28f)/ (k/100000)), 1.053f);} 
		else {Re2 = 44.62f * Mathf.Pow (((charLength*3.28f) / (k/100000)), 1.053f) * Mathf.Pow (machSpeed, 1.16f);}
		return Mathf.Min (Re1, Re2);
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateSkinDragCoefficient(float Cr,float Ct,float velocity)
	{
		float Recr = EstimateRe (velocity, Cr);float Rect = EstimateRe (velocity, Ct);
		float X0Cr1 = 36.9f * 0.366f * Mathf.Pow ((1 / Recr), 0.375f);//Root Upper
		float X0Ct1 = 36.9f*0.366f* Mathf.Pow ((1 / Rect), 0.375f);//Tip Upper
		float X0Ct2 = 36.9f*0.237f* Mathf.Pow ((1 / Rect), 0.375f);//Tip Upper

		float Cf1 = (0.074f/Mathf.Pow(Recr,0.2f))*Mathf.Pow((1-(0.2f-X0Cr1)),0.8f);
		float Cfu2 = (0.074f/Mathf.Pow(Rect,0.2f))*Mathf.Pow((1-(0.2f-X0Ct1)),0.8f);
		float Cfl2 = (0.074f/Mathf.Pow(Rect,0.2f))*Mathf.Pow((1-(0.1f-X0Ct2)),0.8f);//X0Ct1 TO Reduce...Check Later
		float Cf2 = (Cfu2 + Cfl2) / 2f;
		return (Cf1 + Cf2) / 2f;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateControlSurface(float inputTipChord,float inputRootChord,bool[] sections,Color surfaceColor,bool leading,bool floating,float surfaceDeflection,bool drawSections)
	{
		//COLLECTION
		float controlSurfaceArea = 0,controlSurfaceSpan = 0f;

		if (sections != null) {for (int i = 0; i < sections.Length; i++) {if (sections [i] == true) {
					//BUILD VARIABLES
					float currentSelection = (float)i;float nextSection = (float)(i + 1);float sectionLength = (float)sections.Length;
					float baseFactorA = currentSelection/sectionLength;float baseFactorB = nextSection / sectionLength;
					//DRAW CONTROL SURFACE
					Vector3[] rects = new Vector3[4];
					if (floating) {
						rects [0] = EstimateSectionPosition (EstimateSectionPosition(RootChordLeading,RootChordTrailing,(spoilerHinge*0.01f)), 
							EstimateSectionPosition(TipChordLeading,TipChordTrailing,(spoilerHinge*0.01f)), baseFactorA);
						rects [1] = EstimateSectionPosition (EstimateSectionPosition(EstimateSectionPosition(RootChordLeading,RootChordTrailing,(spoilerHinge*0.01f)),RootChordTrailing,(inputRootChord*0.01f)),
							EstimateSectionPosition(EstimateSectionPosition(TipChordLeading,TipChordTrailing,(spoilerHinge*0.01f)),TipChordTrailing,(inputTipChord*0.01f)), baseFactorA);
						rects [2] = EstimateSectionPosition (EstimateSectionPosition(EstimateSectionPosition(RootChordLeading,RootChordTrailing,(spoilerHinge*0.01f)),RootChordTrailing,(inputRootChord*0.01f)), 
							EstimateSectionPosition(EstimateSectionPosition(TipChordLeading,TipChordTrailing,(spoilerHinge*0.01f)),TipChordTrailing,(inputTipChord*0.01f)), baseFactorB);
						rects [3] = EstimateSectionPosition (EstimateSectionPosition(RootChordLeading,RootChordTrailing,(spoilerHinge*0.01f)), 
							EstimateSectionPosition(TipChordLeading,TipChordTrailing,(spoilerHinge*0.01f)), baseFactorB);
					} else {
						if (!leading) {
							//TRAILING  EDGE CONTROLS
							rects [0] = (EstimateSectionPosition (EstimateSectionPosition (RootChordTrailing, RootChordLeading, (inputRootChord * 0.01f)),
								EstimateSectionPosition (TipChordTrailing, TipChordLeading, (inputTipChord * 0.01f)), baseFactorA));
							rects [1] = (EstimateSectionPosition (RootChordTrailing, TipChordTrailing, baseFactorA));
							rects [2] = (EstimateSectionPosition (RootChordTrailing, TipChordTrailing, baseFactorB));
							rects [3] = (EstimateSectionPosition (EstimateSectionPosition (RootChordTrailing, RootChordLeading, (inputRootChord * 0.01f)),
								EstimateSectionPosition (TipChordTrailing, TipChordLeading, (inputTipChord * 0.01f)), baseFactorB));
						} else {
							//LEADING EDGE CONTROLS
							rects [0] = EstimateSectionPosition (EstimateSectionPosition (RootChordLeading, RootChordTrailing, (inputRootChord * 0.01f)),
								EstimateSectionPosition (TipChordLeading, TipChordTrailing, (inputTipChord * 0.01f)), baseFactorA);
							rects [1] = (EstimateSectionPosition (RootChordLeading, TipChordLeading, baseFactorA));
							rects [2] = (EstimateSectionPosition (RootChordLeading, TipChordLeading, baseFactorB));
							rects [3] = EstimateSectionPosition (EstimateSectionPosition (RootChordLeading, RootChordTrailing, (inputRootChord * 0.01f)),
								EstimateSectionPosition (TipChordLeading, TipChordTrailing, (inputTipChord * 0.01f)), baseFactorB);
						}
					}

					//DEFLECT SURFACE
					rects [1] = rects [0] + Quaternion.AngleAxis (surfaceDeflection, (rects [3] - rects [0]).normalized) * (rects [1] - rects [0]);
					rects [2] = rects [3] + (Quaternion.AngleAxis (surfaceDeflection, (rects [3] - rects [0]).normalized)) * (rects [2] - rects [3]);

					#if UNITY_EDITOR
					//DRAW CONTROL AIRFOILS
					if(drawSections && (rootAirfoil != null && tipAirfoil != null)){float yM = Vector3.Distance (RootChordTrailing, rects [1]);
					if (drawSectionRibs && (rootAirfoil != null && tipAirfoil != null)) {PlotRibAirfoil (rects [0], rects [1], yM,0,Color.white);}}
					//DRAW CONTROLS
					Handles.DrawSolidRectangleWithOutline (rects, surfaceColor, surfaceColor);
					#endif
					//ANALYSIS
					controlSurfaceArea += EstimatePanelSectionArea (rects [0], rects [3], rects [1], rects [2]);
					Vector3 rootCenter = EstimateSectionPosition(rects [0],rects [1],0.5f);Vector3 tipCenter = EstimateSectionPosition(rects [2],rects [3],0.5f);
					float panelSpan = Vector3.Distance (rootCenter, tipCenter);
					float Cr = Vector3.Distance (rects [0], rects [1]);float Ct = Vector3.Distance (rects [2], rects [3]);controlSurfaceSpan += panelSpan;
					if (spoilerSections [i] == true) {spoilerDragFactor = spoilerDragCurve.Evaluate(panelSpan / ((Cr + Ct) / 2));}
				}
			}
		}
		return controlSurfaceArea;
	}


	[HideInInspector]public float spoilerFactor;[HideInInspector]public AnimationCurve spoilerDragCurve;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void PlotSpoilerData()
	{
		spoilerDragCurve = new AnimationCurve ();spoilerDragCurve.AddKey (new Keyframe (1, 1.15f));
		spoilerDragCurve.AddKey (new Keyframe (2, 1.16f));spoilerDragCurve.AddKey (new Keyframe (5, 1.20f));
		spoilerDragCurve.AddKey (new Keyframe (10, 1.22f));spoilerDragCurve.AddKey (new Keyframe (30, 1.62f));
	}



	//FLAP MOVEMENT
	private void IncreaseFlap()
	{
		flapSelection += 1;
		if (flapSelection > (flapAngles.Count - 1)) {
			flapSelection = flapAngles.Count - 1;
		}
	}
	//
	private void DecreaseFlap()
	{
		flapSelection -= 1;
		if (flapSelection < 0) {flapSelection = 0;}
	}
	//SPOILER MOVEMENT
	public IEnumerator ExtendSpoilers()
	{
		spoilerMoving = true;
		yield return new WaitForSeconds (spoilerActuationTime);
		spoilerExtended = true;spoilerMoving = false;
	}
	public IEnumerator RetractSpoilers()
	{
		spoilerMoving = true;
		yield return new WaitForSeconds (spoilerActuationTime);
		spoilerExtended = false;spoilerMoving = false;
	}

	//SLAT MOVEMENT
	public IEnumerator ExtendSlats()
	{
		slatMoving = true;
		yield return new WaitForSeconds (slatActuationTime);
		slatExtended = true;slatMoving = false;
	}
	public IEnumerator RetractSlats()
	{
		slatMoving = true;
		yield return new WaitForSeconds (slatActuationTime);
		slatExtended = false;slatMoving = false;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(SilantroAerofoil))]
public class AerofoilEditor: Editor
{
	Color backgroundColor;Color silantroColor = new Color(1,0.4f,0);float tipSweep;
	SilantroAerofoil aerofoil;SerializedObject aerofoilObject;GUIContent label;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		aerofoil = (SilantroAerofoil)target;
		aerofoilObject = new SerializedObject (aerofoil);
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;DrawDefaultInspector ();
		EditorGUI.BeginChangeCheck();aerofoilObject.UpdateIfRequiredOrScript();

		GUILayout.Space (3f);GUI.color = silantroColor;EditorGUILayout.HelpBox ("Aerofoil Type", MessageType.None);GUI.color = backgroundColor;GUILayout.Space (3f);
		aerofoil.aerofoilType = (SilantroAerofoil.AerofoilType)EditorGUILayout.EnumPopup ("Aerofoil Type", aerofoil.aerofoilType);GUILayout.Space (5f);
		aerofoil.surfaceFinish = (SilantroAerofoil.SurfaceFinish)EditorGUILayout.EnumPopup ("Surface Finish", aerofoil.surfaceFinish);
		//SELECT TYPE
		GUILayout.Space (10f);
		if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
			aerofoil.wingPosition = (SilantroAerofoil.WingPosition)EditorGUILayout.EnumPopup ("Alignment", aerofoil.wingPosition);
			GUILayout.Space (3f);
			aerofoil.horizontalPosition = (SilantroAerofoil.HorizontalPosition)EditorGUILayout.EnumPopup (" ", aerofoil.horizontalPosition);
			//
		} else if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Stabilizer) {
			aerofoil.stabOrientation = (SilantroAerofoil.StabilizerOrientation)EditorGUILayout.EnumPopup ("Orientation", aerofoil.stabOrientation);
			if (aerofoil.stabOrientation == SilantroAerofoil.StabilizerOrientation.Horizontal) {
				GUILayout.Space (3f);
				aerofoil.stabilizerPosition = (SilantroAerofoil.StabilizerPosition)EditorGUILayout.EnumPopup (" ", aerofoil.stabilizerPosition);
			}
			if (aerofoil.stabOrientation == SilantroAerofoil.StabilizerOrientation.Vertical) {
				GUILayout.Space (3f);
				aerofoil.rudderPosition = (SilantroAerofoil.RudderPosition)EditorGUILayout.EnumPopup (" ", aerofoil.rudderPosition);
			}
		} else if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Canard) {
			aerofoil.canardPosition = (SilantroAerofoil.CanardPosition)EditorGUILayout.EnumPopup ("Position", aerofoil.canardPosition);
		}

		//AIRFOILS
		GUILayout.Space (10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Airfoil Component", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		aerofoil.rootAirfoil = EditorGUILayout.ObjectField ("Root Airfoil", aerofoil.rootAirfoil, typeof(SilantroAirfoil), true) as SilantroAirfoil;
		GUILayout.Space (5f);
		aerofoil.tipAirfoil = EditorGUILayout.ObjectField ("Tip Airfoil", aerofoil.tipAirfoil, typeof(SilantroAirfoil), true) as SilantroAirfoil;
		GUILayout.Space (3f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Force Application Point", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		aerofoil.forceSystem = (SilantroAerofoil.ForceSystem)EditorGUILayout.EnumPopup(" ",aerofoil.forceSystem);
	
		GUILayout.Space (10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Ground Effect Component", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		aerofoil.effectState = (SilantroAerofoil.GroundEffectState)EditorGUILayout.EnumPopup(" ",aerofoil.effectState);
		if (aerofoil.effectState == SilantroAerofoil.GroundEffectState.Consider) {
			GUILayout.Space (10f);
			aerofoil.groundInfluenceMethod = (SilantroAerofoil.GroundEffectMethod)EditorGUILayout.EnumPopup("Analysis Method",aerofoil.groundInfluenceMethod);
			GUILayout.Space (5f);
			SerializedProperty layerMask = aerofoilObject.FindProperty ("groundLayer");
			EditorGUILayout.PropertyField (layerMask);
			GUILayout.Space (5f);
			EditorGUILayout.LabelField ("Lift Factor", (1/(Mathf.Sqrt(aerofoil.groundInfluenceFactor))*100f).ToString ("0.00")+ " %");
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Drag Factor", (aerofoil.groundInfluenceFactor*100f).ToString ("0.00") + " %");
		}


		GUILayout.Space (20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Aerofoil Dimensions", MessageType.None);GUI.color = backgroundColor;
		GUILayout.Space (7f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Sweep", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		aerofoil.sweepDirection = (SilantroAerofoil.SweepDirection)EditorGUILayout.EnumPopup ("Sweep Direction", aerofoil.sweepDirection);
		if (aerofoil.sweepDirection != SilantroAerofoil.SweepDirection.Unswept) {
			GUILayout.Space (2f);
			aerofoil.aerofoilSweepAngle = EditorGUILayout.Slider ("Sweep Angle", aerofoil.aerofoilSweepAngle, 0f, 90f);
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("ɅLE", aerofoil.leadingEdgeSweep.ToString ("0.00") + " °");
			GUILayout.Space (2f);
			EditorGUILayout.LabelField ("Ʌc/4", aerofoil.quaterSweep.ToString ("0.00") + " °");
			GUILayout.Space (5f);
			aerofoil.sweepCorrectionMethod = (SilantroAerofoil.SweepCorrection)EditorGUILayout.EnumPopup ("Correction Method", aerofoil.sweepCorrectionMethod);
			GUILayout.Space (3f);
			EditorGUILayout.LabelField ("Correction Factor", aerofoil.sweepCorrectionFactor.ToString ("0.000"));

		}

		GUILayout.Space (10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Twist", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		aerofoil.twistDirection = (SilantroAerofoil.TwistDirection)EditorGUILayout.EnumPopup ("Twist Direction", aerofoil.twistDirection);
		if (aerofoil.twistDirection != SilantroAerofoil.TwistDirection.Untwisted) {
			GUILayout.Space (2f);
			aerofoil.wingTwist = EditorGUILayout.Slider ("Twist Angle", aerofoil.wingTwist, 0f, 90f);
		}

		GUILayout.Space (10f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Structure", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		aerofoil.taperPercentage = EditorGUILayout.Slider ("Taper Percentage", aerofoil.taperPercentage, 0f, 90f);
		GUILayout.Space (5f);
		aerofoil.foilSubdivisions = EditorGUILayout.IntSlider ("Panel Subdivisions", aerofoil.foilSubdivisions, 3, 15);
		GUILayout.Space (5f);

		aerofoil.tipDesign = (SilantroAerofoil.WingtipDesign)EditorGUILayout.EnumPopup("Wingtip Design",aerofoil.tipDesign);
		if (aerofoil.tipDesign == SilantroAerofoil.WingtipDesign.Endplate) {
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("EndPlate Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			aerofoil.wingTipHeight = EditorGUILayout.Slider ("Plate Height", aerofoil.wingTipHeight, 0, aerofoil.foilSpan * 0.5f);
			GUILayout.Space (3f);
			aerofoil.wingTipBend = EditorGUILayout.Slider ("Plate Deflection", aerofoil.wingTipBend, -90, 90);
			aerofoil.wingTipSweep = 0f;
		}
		if (aerofoil.tipDesign == SilantroAerofoil.WingtipDesign.Winglet) {
			GUILayout.Space (5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Winglet Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);
			aerofoil.wingTipHeight = EditorGUILayout.Slider ("Winglet Height", aerofoil.wingTipHeight, 0, aerofoil.foilSpan * 0.5f);
			GUILayout.Space (3f);
			aerofoil.wingTipBend = EditorGUILayout.Slider ("Winglet Deflection", aerofoil.wingTipBend, -70, 70);
			GUILayout.Space (3f);
		
			tipSweep = EditorGUILayout.Slider ("Winglet Sweep", tipSweep, 0, 50);
			aerofoil.wingTipSweep = tipSweep * -1f;

		}
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("ΔAR", aerofoil.effectiveChange.ToString ("0.000"));


		GUILayout.Space (25f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (1f);
		EditorGUILayout.LabelField ("Root Chord", aerofoil.foilRootChord.ToString ("0.00") + " m");
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Tip Chord", aerofoil.foilTipChord.ToString ("0.00") + " m");
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Aspect Ratio", aerofoil.AspectRatio.ToString ("0.000"));
		GUILayout.Space (2f);
		EditorGUILayout.LabelField ("Oswald Efficiency", (aerofoil.spanEfficiency*100f).ToString ("0.00") + " %");
		GUILayout.Space (5f);
		EditorGUILayout.LabelField ("Wetted Area", aerofoil.foilWettedArea.ToString ("0.00")+ " m2");
		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Draw Options", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.BeginVertical (GUILayout.Width(110));
		aerofoil.drawSectionRibs = EditorGUILayout.Toggle ("Show Ribs", aerofoil.drawSectionRibs);GUILayout.Space (2f);
		if (aerofoil.drawSectionRibs) {aerofoil.drawRibSplits = EditorGUILayout.Toggle ("Show Rib Sections", aerofoil.drawRibSplits);}GUILayout.Space (2f);
		aerofoil.showPanel = EditorGUILayout.Toggle ("Show Panel Mesh", aerofoil.showPanel);
		GUILayout.EndVertical ();


		GUILayout.Space (25f);
		aerofoil.controlState = (SilantroAerofoil.ControType)EditorGUILayout.EnumPopup ("State", aerofoil.controlState);
		if (aerofoil.controlState == SilantroAerofoil.ControType.Controllable) {
			GUILayout.Space (5f);
			aerofoil.availableControls = (SilantroAerofoil.AvailableControls)EditorGUILayout.EnumPopup(" ",aerofoil.availableControls);

			if (aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing && aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly) {
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Available Secondary Controls", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.BeginVertical (GUILayout.Width(110));
				aerofoil.canUseFlap = EditorGUILayout.Toggle ("Flaps", aerofoil.canUseFlap);
				aerofoil.canUseSlat = EditorGUILayout.Toggle ("Slats", aerofoil.canUseSlat);
				aerofoil.canUseSpoilers = EditorGUILayout.Toggle ("Spoiler", aerofoil.canUseSpoilers);
				GUILayout.EndVertical ();
			}
			GUILayout.BeginHorizontal ();

			if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly) {
				GUILayout.BeginVertical ();
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Primary", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				aerofoil.surfaceType = (SilantroAerofoil.SurfaceType)EditorGUILayout.EnumPopup ("Surface Type", aerofoil.surfaceType);
				GUILayout.Space (3f);
				if (aerofoil.surfaceType == SilantroAerofoil.SurfaceType.Elevon) {
					aerofoil.elevonPosition = (SilantroAerofoil.ElevonPosition)EditorGUILayout.EnumPopup (" ", aerofoil.elevonPosition);
				}

				GUILayout.Space (5f);
				if (aerofoil.surfaceType != SilantroAerofoil.SurfaceType.Inactive) {
					GUILayout.Space (3f);
					GUI.color = aerofoil.controlColor;
					EditorGUILayout.HelpBox ("Control Chord Ratios (xc/c)", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (5f);
					GUI.color = backgroundColor;
					if (aerofoil.availableControls == SilantroAerofoil.AvailableControls.PrimaryPlusSecondary && aerofoil.canUseFlap || aerofoil.canUseSlat || aerofoil.canUseSpoilers) {
						EditorGUILayout.LabelField (aerofoil.surfaceType.ToString () + " Root Chord :", aerofoil.controlRootChord.ToString ("0.0") + " %");
						aerofoil.controlRootChord = GUILayout.HorizontalSlider (aerofoil.controlRootChord, 0f, 100f);
						EditorGUILayout.LabelField (aerofoil.surfaceType.ToString () + " Tip Chord :", aerofoil.controlTipChord.ToString ("0.0") + " %");
						aerofoil.controlTipChord = GUILayout.HorizontalSlider (aerofoil.controlTipChord, 0f, 100f);
					} else {
						aerofoil.controlRootChord = EditorGUILayout.Slider (aerofoil.surfaceType.ToString () + " Root Chord", aerofoil.controlRootChord, 0f, 100f);
						GUILayout.Space (5f);
						aerofoil.controlTipChord = EditorGUILayout.Slider (aerofoil.surfaceType.ToString () + " Tip Chord", aerofoil.controlTipChord, 0f, 100f);
					}
					GUILayout.Space (5f);
					EditorGUILayout.LabelField (aerofoil.surfaceType.ToString () + " Area", aerofoil.controlArea.ToString ("0.000") + " m2");
					GUILayout.Space (3f);
					GUI.color = aerofoil.controlColor;
					EditorGUILayout.HelpBox (aerofoil.surfaceType.ToString () + " Panels", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (5f);
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.BeginVertical ();
					SerializedProperty boolsControl = aerofoilObject.FindProperty ("controlSections");
					for (int ci = 0; ci < boolsControl.arraySize; ci++) {
						GUIContent labelControl = new GUIContent ();
						if (ci == 0) {
							labelControl = new GUIContent ("Root Panel: ");
						} else if (ci == boolsControl.arraySize - 1) {
							labelControl = new GUIContent ("Tip Panel: ");
						} else {
							labelControl = new GUIContent ("Panel: " + (ci + 1).ToString ());
						}
						EditorGUILayout.PropertyField (boolsControl.GetArrayElementAtIndex (ci), labelControl);
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.EndVertical ();

					GUILayout.Space (10f);
					GUI.color = aerofoil.controlColor;
					EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (5f);
					aerofoil.controlDeflectionType = (SilantroAerofoil.DeflectionType)EditorGUILayout.EnumPopup ("Deflection Type", aerofoil.controlDeflectionType);
					if (aerofoil.controlDeflectionType == SilantroAerofoil.DeflectionType.Symmetric) {
						GUILayout.Space (5f);
						aerofoil.positveLimit = EditorGUILayout.FloatField ("Deflection Limit", aerofoil.positveLimit);
					} else {
						GUILayout.Space (5f);
						aerofoil.positveLimit = EditorGUILayout.FloatField ("Positive Deflection Limit", aerofoil.positveLimit);
						GUILayout.Space (3f);
						aerofoil.negativeLimit = EditorGUILayout.FloatField ("Negative Deflection Limit", aerofoil.negativeLimit);
					}
					GUILayout.Space (3f);
					EditorGUILayout.LabelField ("Current Deflection", aerofoil.controlDeflection.ToString ("0.00") + " °");


					GUILayout.Space (15f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Trim Configuration", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (3f);
					aerofoil.trimState = (SilantroAerofoil.TrimState)EditorGUILayout.EnumPopup("Trim State",aerofoil.trimState);
					if (aerofoil.trimState == SilantroAerofoil.TrimState.Available) {
						GUILayout.Space (3f);
						aerofoil.maximumPitchTrim = EditorGUILayout.Slider ("Deflection Limit (%)", aerofoil.maximumPitchTrim,5f,50f);
						GUILayout.Space (3f);
						aerofoil.trimSpeed = EditorGUILayout.Slider ("Wheel Speed",aerofoil.trimSpeed, 0.035f, 0.095f);
					}


					GUILayout.Space (15f);
					GUI.color = aerofoil.controlColor;
					EditorGUILayout.HelpBox ("Model Configuration", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (5f);
					EditorGUILayout.BeginHorizontal ();
					aerofoil.controlSurfaceModel = EditorGUILayout.ObjectField (aerofoil.controlSurfaceModel, typeof(Transform), true)as Transform;
					aerofoil.rotationAxis = (SilantroAerofoil.RotationAxis)EditorGUILayout.EnumPopup (aerofoil.rotationAxis);
					aerofoil.baseDeflectionDirection = (SilantroAerofoil.BaseDeflectionDirection)EditorGUILayout.EnumPopup (aerofoil.baseDeflectionDirection);
					EditorGUILayout.EndHorizontal ();

				}
				//END OF CONTROLS
				GUILayout.EndVertical ();

			}


			if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.canUseFlap) {
				if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.SecondaryOnly) {
					GUILayout.Space (30f);
				}
				GUILayout.BeginVertical ();
				GUI.color = Color.yellow;
				EditorGUILayout.HelpBox ("Flap", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				aerofoil.flapType = (SilantroAerofoil.FlapType)EditorGUILayout.EnumPopup ("Flap Type", aerofoil.flapType);
				GUILayout.Space (3f);
				if (aerofoil.flapType == SilantroAerofoil.FlapType.Flaperon) {
					aerofoil.flaperonPosition = (SilantroAerofoil.FlaperonPosition)EditorGUILayout.EnumPopup (" ", aerofoil.flaperonPosition);
					GUILayout.Space (5f);
				}
				if (aerofoil.horizontalPosition == SilantroAerofoil.HorizontalPosition.Center && aerofoil.flapType != SilantroAerofoil.FlapType.Flaperon) {
					GUILayout.Space (3f);
					aerofoil.centerSide = (SilantroAerofoil.CenterSide)EditorGUILayout.EnumPopup (" ", aerofoil.centerSide);
					GUILayout.Space (5f);
				}
				GUI.color = Color.yellow;
				EditorGUILayout.HelpBox ("Flap Chord Ratios (xf/c)", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				GUI.color = backgroundColor;
				EditorGUILayout.LabelField ("Flap Root Chord :", aerofoil.flapRootChord.ToString ("0.0") + " %");
				aerofoil.flapRootChord = GUILayout.HorizontalSlider (aerofoil.flapRootChord, 0f, 100f);
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("Flap Tip Chord :", aerofoil.flapTipChord.ToString ("0.0") + " %");
				aerofoil.flapTipChord = GUILayout.HorizontalSlider (aerofoil.flapTipChord, 0f, 100f);

				GUILayout.Space (5f);
				EditorGUILayout.LabelField ("Flap Area", aerofoil.flapArea.ToString ("0.000") + " m2");
				GUILayout.Space (3f);
				GUI.color = Color.yellow;
				EditorGUILayout.HelpBox ("Flap Panels", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.BeginVertical ();
				SerializedProperty boolsflap = aerofoilObject.FindProperty ("flapSections");
				for (int i = 0; i < boolsflap.arraySize; i++) {
					if (aerofoil.controlSections [i] != true) {
						GUIContent labelFlap = new GUIContent ();
						if (i == 0) {
							labelFlap = new GUIContent ("Root Panel: ");
						} else if (i == boolsflap.arraySize - 1) {
							labelFlap = new GUIContent ("Tip Panel: ");
						} else {
							labelFlap = new GUIContent ("Panel: " + (i + 1).ToString ());
						}
						EditorGUILayout.PropertyField (boolsflap.GetArrayElementAtIndex (i), labelFlap);
					} else {
						if (aerofoil.surfaceType != SilantroAerofoil.SurfaceType.Inactive) {
							string labelFlapNote;
							if (i == 0) {
								labelFlapNote = ("Root Panel: ");
							} else if (i == boolsflap.arraySize - 1) {
								labelFlapNote = ("Tip Panel: ");
							} else {
								labelFlapNote = ("Panel: " + (i + 1).ToString ());
							}
							EditorGUILayout.LabelField (labelFlapNote, aerofoil.surfaceType.ToString ());
						}

					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();

				GUILayout.Space (10f);
				GUI.color = Color.yellow;
				EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				if (aerofoil.flapType == SilantroAerofoil.FlapType.Flaperon) {
					GUILayout.Space (5f);
					aerofoil.positveLimit = EditorGUILayout.FloatField ("Positive Deflection Limit", aerofoil.positveLimit);
					GUILayout.Space (3f);
					aerofoil.negativeLimit = EditorGUILayout.FloatField ("Negative Deflection Limit", aerofoil.negativeLimit);
				
				} else {
					GUILayout.Space (5f);
					aerofoil.flap1 = EditorGUILayout.FloatField ("Flap Level 1", aerofoil.flap1);
					GUILayout.Space (3f);
					aerofoil.flap2 = EditorGUILayout.FloatField ("Flap Level 2", aerofoil.flap2);
					GUILayout.Space (5f);
					aerofoil.flap3 = EditorGUILayout.FloatField ("Flap Level 3", aerofoil.flap3);
					GUILayout.Space (3f);
					aerofoil.flapsfull = EditorGUILayout.FloatField ("Flaps Full", aerofoil.flapsfull);
				}
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Flap Deflection", aerofoil.flapDeflection.ToString ("0.00") + " °");

				if (aerofoil.flapType == SilantroAerofoil.FlapType.Flaperon) {
					GUILayout.Space (33f);
				} else {
					GUILayout.Space (3f);
				}
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Model Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				EditorGUILayout.BeginHorizontal ();
				aerofoil.flapSurfaceModel = EditorGUILayout.ObjectField (aerofoil.flapSurfaceModel, typeof(Transform), true)as Transform;
				aerofoil.flapRotationAxis = (SilantroAerofoil.FlapRotationAxis)EditorGUILayout.EnumPopup(aerofoil.flapRotationAxis);
				aerofoil.flapDeflectionDirection = (SilantroAerofoil.FlapDeflectionDirection)EditorGUILayout.EnumPopup(aerofoil.flapDeflectionDirection);
				EditorGUILayout.EndHorizontal ();

				GUILayout.Space (3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Sound Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				aerofoil.flapDown = EditorGUILayout.ObjectField ("Lower Flaps", aerofoil.flapDown, typeof(AudioClip), true) as AudioClip;
				GUILayout.Space (3f);
				aerofoil.flapUp = EditorGUILayout.ObjectField ("Raise Flaps", aerofoil.flapUp, typeof(AudioClip), true) as AudioClip;


				//END OF FLAPS
				GUILayout.EndVertical ();
			}

			if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.canUseSpoilers) {
				if (aerofoil.canUseFlap) {
					GUILayout.Space (30f);
				}
				GUILayout.BeginVertical ();
				GUI.color = Color.cyan;
				EditorGUILayout.HelpBox ("Spoiler", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				aerofoil.spoilerType = (SilantroAerofoil.SpoilerType)EditorGUILayout.EnumPopup ("Spoiler Type", aerofoil.spoilerType);
				GUILayout.Space (3f);
				GUI.color = Color.cyan;
				EditorGUILayout.HelpBox ("Spoiler Dimensions", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				GUI.color = backgroundColor;
				EditorGUILayout.LabelField ("Spoiler Chord (xst/c):", aerofoil.spoilerChordFactor.ToString ("0.0") + " %");
				aerofoil.spoilerChordFactor = GUILayout.HorizontalSlider (aerofoil.spoilerChordFactor, 0f, 60f);
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("Spoiler Hinge:", aerofoil.spoilerHinge.ToString ("0.0") + " %");
				aerofoil.spoilerHinge = GUILayout.HorizontalSlider (aerofoil.spoilerHinge, 0f, 100f);

				GUILayout.Space (5f);
				EditorGUILayout.LabelField ("Spoiler Area", aerofoil.spoilerArea.ToString ("0.000") + " m2");
				GUILayout.Space (3f);
				GUI.color = Color.cyan;
				EditorGUILayout.HelpBox ("Spoiler Panels", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.BeginVertical ();
				SerializedProperty boolspoilers = aerofoilObject.FindProperty ("spoilerSections");
				//
				for (int i = 0; i < boolspoilers.arraySize; i++) {
					GUIContent labelSpoiler = new GUIContent ();
					if (i == 0) {
						labelSpoiler = new GUIContent ("Root Panel: ");
					} else if (i == boolspoilers.arraySize - 1) {
						labelSpoiler = new GUIContent ("Tip Panel: ");
					} else {
						labelSpoiler = new GUIContent ("Panel: " + (i + 1).ToString ());
					}
					EditorGUILayout.PropertyField (boolspoilers.GetArrayElementAtIndex (i), labelSpoiler);
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();

				GUILayout.Space (10f);
				GUI.color = Color.cyan;
				EditorGUILayout.HelpBox ("Deflection Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				aerofoil.maximumSpoilerDeflection = EditorGUILayout.FloatField ("Deflection Limit", aerofoil.maximumSpoilerDeflection);
				GUILayout.Space (5f);
				aerofoil.spoilerActuationTime = EditorGUILayout.FloatField ("Actuation Time", aerofoil.spoilerActuationTime);
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Spoiler Deflection", aerofoil.spoilerDeflection.ToString ("0.00") + " °");

				GUILayout.Space (33f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Model Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				EditorGUILayout.BeginHorizontal ();
				aerofoil.spoilerSurfaceModel = EditorGUILayout.ObjectField (aerofoil.spoilerSurfaceModel, typeof(Transform), true)as Transform;
				aerofoil.spoilerRotationAxis = (SilantroAerofoil.SpoilerRotationAxis)EditorGUILayout.EnumPopup(aerofoil.spoilerRotationAxis);
				aerofoil.spoilerDeflectionDirection = (SilantroAerofoil.SpoilerDeflectionDirection)EditorGUILayout.EnumPopup(aerofoil.spoilerDeflectionDirection);
				EditorGUILayout.EndHorizontal ();

				//END OF SPOILERS
				GUILayout.EndVertical ();
			}


			if (aerofoil.availableControls != SilantroAerofoil.AvailableControls.PrimaryOnly && aerofoil.canUseSlat) {
				if (aerofoil.canUseSlat) {
					GUILayout.Space (30f);
				}
				GUILayout.BeginVertical ();
				GUI.color = Color.magenta;
				EditorGUILayout.HelpBox ("Slat", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (2f);
				aerofoil.slatType = (SilantroAerofoil.SlatType)EditorGUILayout.EnumPopup ("Slat Type", aerofoil.slatType);
				GUILayout.Space (3f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Aerodynamic Properties", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (3f);
				if (aerofoil.slatType == SilantroAerofoil.SlatType.FixedSlot) {aerofoil.Cdmax = 0.0075f;aerofoil.Clmax = 0.45f;aerofoil.Cmmax = 0.0008f;}
				if (aerofoil.slatType == SilantroAerofoil.SlatType.MaxwellFlap) {aerofoil.Cdmax = 0.0156f;aerofoil.Clmax = 0.55f;aerofoil.Cmmax = 0.0008f;}
				if (aerofoil.slatType == SilantroAerofoil.SlatType.KrugerFlap) {aerofoil.Cdmax = 0.0149f;aerofoil.Clmax = 0.30f;aerofoil.Cmmax = -0.027f;}
				EditorGUILayout.LabelField ("λCl:", aerofoil.Clmax.ToString ("0.0000"));
				EditorGUILayout.LabelField ("λCd:", aerofoil.Cdmax.ToString ("0.0000"));
				EditorGUILayout.LabelField ("λCm:", aerofoil.Cmmax.ToString ("0.0000"));

				GUI.color = Color.magenta;
				EditorGUILayout.HelpBox ("Slat Chord Ratios (xs/c)", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				GUI.color = backgroundColor;
				EditorGUILayout.LabelField ("Slat Root Chord :", aerofoil.slatRootChord.ToString ("0.0") + " %");
				aerofoil.slatRootChord = GUILayout.HorizontalSlider (aerofoil.slatRootChord, 0f, 100f);
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("Slat Tip Chord :", aerofoil.slatTipChord.ToString ("0.0") + " %");
				aerofoil.slatTipChord = GUILayout.HorizontalSlider (aerofoil.slatTipChord, 0f, 100f);

				GUILayout.Space (5f);
				EditorGUILayout.LabelField ("Slat Area", aerofoil.slatArea.ToString ("0.000") + " m2");
				GUILayout.Space (3f);
				GUI.color = Color.magenta;
				EditorGUILayout.HelpBox ("Slat Panels", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.BeginVertical ();
				SerializedProperty boolsSlat = aerofoilObject.FindProperty ("slatSections");
				for (int i = 0; i < boolsSlat.arraySize; i++) {
					GUIContent labelSlat = new GUIContent ();
					if (i == 0) {
						labelSlat = new GUIContent ("Root Panel: ");
					} else if (i == boolsSlat.arraySize - 1) {
						labelSlat = new GUIContent ("Tip Panel: ");
					} else {
						labelSlat = new GUIContent ("Panel: " + (i + 1).ToString ());
					}
					EditorGUILayout.PropertyField (boolsSlat.GetArrayElementAtIndex (i), labelSlat);
				}
				EditorGUILayout.EndVertical ();
				EditorGUILayout.EndHorizontal ();
				GUILayout.Space (10f);
				GUI.color = Color.magenta;
				EditorGUILayout.HelpBox ("Movement Configuration", MessageType.None);
				GUI.color = backgroundColor;
				aerofoil.slatMovement = (SilantroAerofoil.SlatMovement)EditorGUILayout.EnumPopup("Movement Type",aerofoil.slatMovement);
				if (aerofoil.slatMovement == SilantroAerofoil.SlatMovement.Deflection) {
					GUILayout.Space (6f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Deflection Settings", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (4f);
					aerofoil.maximumSlatDeflection = EditorGUILayout.FloatField ("Deflection Limit", aerofoil.maximumSlatDeflection);
					GUILayout.Space (3f);
					EditorGUILayout.LabelField ("Actuation Speed", aerofoil.slatActuationSpeed.ToString ("0.0") + " °/s");
					aerofoil.slatActuationSpeed = GUILayout.HorizontalSlider (aerofoil.slatActuationSpeed,10,50);
					GUILayout.Space (3f);
					aerofoil.slatActuationTime = EditorGUILayout.FloatField ("Actuation Time", aerofoil.slatActuationTime);
					GUILayout.Space (3f);
					EditorGUILayout.LabelField ("Current Deflection", aerofoil.slatDeflection.ToString ("0.00") + " °");
				}
				//SLIDING
				if (aerofoil.slatMovement == SilantroAerofoil.SlatMovement.Extension) {
					GUILayout.Space (6f);
					GUI.color = Color.white;
					EditorGUILayout.HelpBox ("Extension Settings", MessageType.None);
					GUI.color = backgroundColor;
					GUILayout.Space (4f);
					aerofoil.slatDistance = EditorGUILayout.FloatField ("Maximum Extension", aerofoil.slatDistance);
					GUILayout.Space (3f);
					EditorGUILayout.LabelField ("Actuation Speed", aerofoil.slatActuationSpeed.ToString ("0.0") + " m/s");
					aerofoil.slatActuationSpeed = GUILayout.HorizontalSlider (aerofoil.slatActuationSpeed,1,10);
					GUILayout.Space (3f);
					aerofoil.slatActuationTime = EditorGUILayout.FloatField ("Actuation Time", aerofoil.slatActuationTime);
					GUILayout.Space (3f);
					EditorGUILayout.LabelField ("Current Extension", (aerofoil.currentSlatPosition).ToString ("0.00") + " cm");
				}
				GUILayout.Space (5f);
				GUI.color = Color.white;
				EditorGUILayout.HelpBox ("Model Configuration", MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (5f);
				EditorGUILayout.BeginHorizontal ();
				aerofoil.slatSurfaceModel = EditorGUILayout.ObjectField (aerofoil.slatSurfaceModel, typeof(Transform), true)as Transform;
				aerofoil.slatRotationAxis = (SilantroAerofoil.SlatRotationAxis)EditorGUILayout.EnumPopup(aerofoil.slatRotationAxis);
				aerofoil.slatDeflectionDirection = (SilantroAerofoil.SlatDeflectionDirection)EditorGUILayout.EnumPopup(aerofoil.slatDeflectionDirection);
				EditorGUILayout.EndHorizontal ();
				//END OF SLATS
				GUILayout.EndVertical ();
			}
			GUILayout.EndHorizontal ();
		}


		GUILayout.Space(40f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Output Data", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField ("AOA", aerofoil.angleOfAttack.ToString ("0.00") + " °");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Lift", aerofoil.TotalLift.ToString ("0.0") + " N");
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Drag", aerofoil.TotalDrag.ToString ("0.0") + " N");
		GUILayout.Space(2f);
		if (!aerofoil.showAdvanced) {
			aerofoil.showAdvanced = EditorGUILayout.Toggle ("Show Drag Details", aerofoil.showAdvanced);
		}
		if (aerofoil.showAdvanced) {
			GUILayout.Space(2f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox ("Drag Details", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space (3f);

			EditorGUILayout.LabelField ("Induced Drag", aerofoil.TotalBaseDrag.ToString ("0.0") + " N");
			GUILayout.Space(2f);
			EditorGUILayout.LabelField ("Parasite Drag", aerofoil.TotalSkinDrag.ToString ("0.0") + " N");
			if (aerofoil.canUseSpoilers && aerofoil.aerofoilType == SilantroAerofoil.AerofoilType.Wing) {
				GUILayout.Space (2f);
				EditorGUILayout.LabelField ("Spoiler Drag", aerofoil.TotalSpoilerDrag.ToString ("0.0") + " N");
			}
			GUILayout.Space(2f);
			aerofoil.showAdvanced = EditorGUILayout.Toggle ("Close", aerofoil.showAdvanced);
		}

		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (aerofoilObject.targetObject, "Aerofoil Change");}
		if (GUI.changed) {
			EditorUtility.SetDirty (aerofoil);
			EditorSceneManager.MarkSceneDirty (aerofoil.gameObject.scene);
		}
		aerofoilObject.ApplyModifiedProperties();
	}

	//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	public override bool RequiresConstantRepaint ()
	{
		return true;
	}
}
#endif