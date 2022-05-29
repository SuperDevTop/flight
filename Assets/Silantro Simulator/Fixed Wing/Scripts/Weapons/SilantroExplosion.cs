//Oyedoyin Dada
//cc dadaoyedoyin@gmail.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
//
public class SilantroExplosion : MonoBehaviour {
	//PROPERTIES
	[HideInInspector]public float damage = 200f;
	[HideInInspector]public float explosionForce = 4000f;
	[HideInInspector]public float explosionRadius = 45f;
	[HideInInspector]float fractionalDistance;
	//LIGHT
	[HideInInspector]public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	[HideInInspector]public float exposureTime = 1;
	[HideInInspector]public float lightIntensity = 5;
	[HideInInspector]private bool canUpdate;
	[HideInInspector]private float startTime;
	[HideInInspector]public Light lightSource;







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		//EXPLOSION LIGHT
		lightSource = GetComponentInChildren<Light> ();
		if (lightSource) {
			lightSource.intensity = LightCurve.Evaluate (0);
			startTime = Time.time;
			canUpdate = true;
		}
		//EFFECT
		Explode();
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void Explode()
	{
		//AQUIRE SURROUNDING COLLIDERS
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
		for (int i = 0; i < hitColliders.Length; i++) {
			Collider hit = hitColliders[i];
			if (hit != null) {
				//DISTANCE FALLOFF
				float distanceToObject = Vector3.Distance(transform.position,hit.gameObject.transform.position);
				fractionalDistance = (1 - (distanceToObject / explosionRadius));
				Vector3 exploionPosition = transform.position;
				//ONLY AFFECT OBJECTS WITHIN RANGE
				if (distanceToObject < explosionRadius) {
					//SEND DAMAGE MESSAGE
					float actualDamage = damage * fractionalDistance;
					hit.gameObject.SendMessageUpwards("SilantroDamage",(-actualDamage),SendMessageOptions.DontRequireReceiver);
					//FORCE
					//1. OBJECT ITSELF
					if (hit.GetComponent<Rigidbody> ()) {
						float actualForce = explosionForce * fractionalDistance;
						hit.GetComponent<Rigidbody> ().AddExplosionForce (actualForce, transform.position, explosionRadius, 3f, ForceMode.Impulse);
					}
					//2. OBJECT PARENT
					else if (hit.transform.root.gameObject.GetComponent<Rigidbody> ()) {
						float actualForce = explosionForce * fractionalDistance;
						hit.transform.root.gameObject.GetComponent<Rigidbody> ().AddExplosionForce (actualForce, transform.position, explosionRadius, (3.0f ), ForceMode.Impulse);
					}
				}
			}
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void Update()
	{
		if (lightSource) {
			float time = Time.time - startTime;
			if (canUpdate) {
				float eval = LightCurve.Evaluate (time / exposureTime) * lightIntensity;
				lightSource.intensity = eval;
			}
			if (time >= exposureTime) {
				canUpdate = false;
			}
		}
	}
}






#if UNITY_EDITOR
[CustomEditor(typeof(SilantroExplosion))]
public class ExplosionEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1,0.4f,0);
	//
	SilantroExplosion effect;
	SerializedObject effectObject;
	//
	private void OnEnable()
	{
		effect = (SilantroExplosion)target;
		effectObject = new SerializedObject (effect);
	}
	//
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		DrawDefaultInspector ();EditorGUI.BeginChangeCheck ();
		serializedObject.Update ();
		//
		GUILayout.Space (3f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Properties", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		effect.damage = EditorGUILayout.FloatField ("Damage", effect.damage);
		GUILayout.Space (3f);
		effect.explosionForce = EditorGUILayout.FloatField ("Explosion Force", effect.explosionForce);
		GUILayout.Space (3f);
		effect.explosionRadius = EditorGUILayout.FloatField ("Effective Radius", effect.explosionRadius);
		//
		GUILayout.Space (15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Light Settings", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		effect.lightIntensity = EditorGUILayout.FloatField ("Maximum Intensity", effect.lightIntensity);
		GUILayout.Space (3f);
		effect.exposureTime = EditorGUILayout.FloatField ("Exposure Time", effect.exposureTime);
		GUILayout.Space (3f);
		effect.LightCurve = EditorGUILayout.CurveField ("Decay Curve", effect.LightCurve);
		//
		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (effectObject.targetObject, "Explosion Change");}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (effect);
			EditorSceneManager.MarkSceneDirty (effect.gameObject.scene);
		}
		effectObject.ApplyModifiedProperties();
	}
}
#endif