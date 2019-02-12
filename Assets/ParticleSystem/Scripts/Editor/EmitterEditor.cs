//
// Copyright © Daniel Shervheim, 2019
// www.danielshervheim.com
//

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Emitter))]
public class EmitterEditor : Editor {
	private bool showSimulationProperties = false;
	private bool showEmitterProperties = false;
	private bool showSpawnRateProperties = false;
	private bool showVelocityProperties = false;
	private bool showLifetimeProperties = false;
	private bool showColliders = false;

    private SerializedProperty particleMaterial;
	private SerializedProperty sphereColliders;
	private SerializedProperty boxColliders;

	void OnEnable() {
        particleMaterial = serializedObject.FindProperty("particleMaterial");
		sphereColliders = serializedObject.FindProperty("sphereColliders");
		boxColliders = serializedObject.FindProperty("boxColliders");
	}

    public override void OnInspectorGUI() {
        Emitter emitter = (Emitter)target;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(particleMaterial, true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        showSimulationProperties = EditorGUILayout.Foldout(showSimulationProperties, "Simulation Properties", true);
        if (showSimulationProperties) {
        	emitter.coefficientOfRestitution = EditorGUILayout.Slider("Coefficient of Resitution", emitter.coefficientOfRestitution, 0f, 1f);
			emitter.randomizeCOR = EditorGUILayout.Toggle("Randomize COR", emitter.randomizeCOR);
			emitter.gravity = EditorGUILayout.Vector3Field("Gravity", emitter.gravity);
        }

        EditorGUILayout.Space();
        showEmitterProperties = EditorGUILayout.Foldout(showEmitterProperties, "Emitter Properties", true);
        emitter.visualizeEmitter = showEmitterProperties;
		emitter.visualizeVelocity = showEmitterProperties;
        if (showEmitterProperties) {
        	emitter.emitterShape = (Emitter.EmitterShape)EditorGUILayout.EnumPopup("Shape", emitter.emitterShape);
	    	emitter.emitterPosition = EditorGUILayout.Vector3Field("Position", emitter.emitterPosition);
	    	emitter.emitterRotation = EditorGUILayout.Vector3Field("Rotation", emitter.emitterRotation);

	    	if (emitter.emitterShape == Emitter.EmitterShape.Rectangle) {
				emitter.rectangleWidth = EditorGUILayout.FloatField("Width", emitter.rectangleWidth);
				emitter.rectangleLength = EditorGUILayout.FloatField("Length", emitter.rectangleLength);
			}
			else if (emitter.emitterShape == Emitter.EmitterShape.Circle) {
				emitter.circleRadius = EditorGUILayout.FloatField("Radius", emitter.circleRadius);
			}
			else if (emitter.emitterShape == Emitter.EmitterShape.Sphere) {
				emitter.sphereRadius = EditorGUILayout.FloatField("Radius", emitter.sphereRadius);
			}
			else if (emitter.emitterShape == Emitter.EmitterShape.Cone) {
				emitter.coneRadius = EditorGUILayout.FloatField("Radius", emitter.coneRadius);
				emitter.coneAngle = EditorGUILayout.Slider("Angle", emitter.coneAngle, 0f, 90f);
			}
        }

		EditorGUILayout.Space();
        showSpawnRateProperties = EditorGUILayout.Foldout(showSpawnRateProperties, "Particle Spawn Rate", true);
        if (showSpawnRateProperties) {
        	EditorGUILayout.BeginHorizontal();
		        emitter.particlesPer = EditorGUILayout.IntField(emitter.particlesPer);
		        GUILayout.Label("particles per");
		        emitter.timeStep = EditorGUILayout.FloatField(emitter.timeStep);
		        GUILayout.Label("second");
	        EditorGUILayout.EndHorizontal();
	        EditorGUILayout.BeginHorizontal();
	        	GUILayout.Label("Limit to");
	        	emitter.particleCount = EditorGUILayout.IntField(emitter.particleCount);
	        	GUILayout.Label("particles");
	        EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        showVelocityProperties = EditorGUILayout.Foldout(showVelocityProperties, "Particle Velocity", true);
        if (showVelocityProperties) {
        	emitter.minimumVelocity = EditorGUILayout.FloatField("Minimum (m/s)", emitter.minimumVelocity);
        	emitter.maximumVelocity = EditorGUILayout.FloatField("Maximum (m/s)", emitter.maximumVelocity);
        	emitter.randomness = EditorGUILayout.Slider("Randomization Factor", emitter.randomness, 0f, 1f);
        	emitter.percentageAtDeath = EditorGUILayout.Slider("Percentage At Death", emitter.percentageAtDeath, 0f, 1f);
        }

        EditorGUILayout.Space();
        showLifetimeProperties = EditorGUILayout.Foldout(showLifetimeProperties, "Particle Lifetime", true);
        if (showLifetimeProperties) {
        	emitter.minimumLifetime = EditorGUILayout.FloatField("Minimum (s)", emitter.minimumLifetime);
        	emitter.maximumLifetime = EditorGUILayout.FloatField("Maximum (s)", emitter.maximumLifetime);
        }

        EditorGUILayout.Space();
        showColliders = EditorGUILayout.Foldout(showColliders, "Colliders", true);
        if (showColliders) {
        	EditorGUI.indentLevel++;
        	EditorGUILayout.PropertyField(sphereColliders, true);
            if (GUILayout.Button("Transfer sphere colliders from scene")) {
                emitter.TransferSphereColliders();
            }

        	EditorGUILayout.PropertyField(boxColliders, true);
            if (GUILayout.Button("Transfer box colliders from scene")) {
                emitter.TransferBoxColliders();
            }

            EditorGUI.indentLevel--;
            string msg = "Transfering colliders from the scene will overwrite any currently set particle colliders.";
            msg += " Collider rotation is currently not supported.";
            EditorGUILayout.HelpBox(msg, MessageType.Warning);

            emitter.updateEachFrame = EditorGUILayout.Toggle("Update Colliders in Real-time", emitter.updateEachFrame);

        	serializedObject.ApplyModifiedProperties();
        }
    }
}