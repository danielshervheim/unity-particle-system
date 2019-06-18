//
// Copyright © Daniel Shervheim, 2019
// www.danielshervheim.com
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emitter : MonoBehaviour {
	public enum EmitterShape {Rectangle, Circle, Sphere, Cone};
	
	private struct Particle {
		public Vector3 position;
		public Vector3 originalPosition;
		public Vector3 velocity;
		public Vector3 originalVelocity;
		public float lifetime;
		public float age;
	}
	private const int PARTICLE_SIZE = 56;  // (4*3 + 2)*sizeof(float)

	public Material particleMaterial;
	private Mesh quad;
	private ComputeShader compute;
	private int computeKernel;
	private const int THREAD_GROUPS = 256;

	// Emitter Viz
	public bool visualizeEmitter = true;
	public bool visualizeVelocity = true;

	// Emitter Shape, Transform, Viz
	public EmitterShape emitterShape = EmitterShape.Rectangle;
	public Vector3 emitterPosition = Vector3.zero;
	public Vector3 emitterRotation = Vector3.zero;

	// Rectangle
	public float rectangleWidth = 1;
	public float rectangleLength = 2;

	// Circle
	public float circleRadius = 1;

	// Sphere
	public float sphereRadius = 1;

	// Cone
	public float coneRadius = 1;
	public float coneAngle = 45f;

	// Velocity
	public float minimumVelocity = 1;
	public float maximumVelocity = 5;
	[Range(0, 1)]
	public float randomness = 0.0f;
	[Range(0, 1)]
	public float percentageAtDeath = 1.0f;

	// Lifetime
	public float minimumLifetime = 1;
	public float maximumLifetime = 10;
	
	// Spawn Rate, Count
	public int particlesPer = 100;
	public float timeStep = 1;
	public int particleCount = 10000;

	// Simulation Settings
	[Range(0f, 1f)]
	public float coefficientOfRestitution = 0.75f;
	public bool randomizeCOR = true; 
	public Vector3 gravity = new Vector4(0, -9.8f, 0); 

	private ComputeBuffer argsBuffer;
	private ComputeBuffer particleBuffer;

	// Sphere Colliders
	[System.Serializable]
	public struct SphereCollider {
		public Vector3 center;
		public float radius;
	}
	private const int SPHERE_COLLIDER_SIZE = 16;
	public SphereCollider[] sphereColliders;
	private ComputeBuffer sphereColliderBuffer;

	public bool updateEachFrame = false;

	// Box Colliders
	[System.Serializable]
	public struct BoxCollider {
		public Vector3 center;
		public Vector3 extents;
	}
	private const int BOX_COLLIDER_SIZE = 24;
	public BoxCollider[] boxColliders;
	private ComputeBuffer boxColliderBuffer;



	public void Start () {
		// Verify that the particleCount is valid.
		if (particleCount < THREAD_GROUPS) {
			Debug.LogWarning("The GPU will spawn more thread groups than needed due to low particle count.");
		}

		if (particleCount < 1) {
			Debug.LogError("Particle count must be greater than zero.");
			return;
		}

		/* Instantiate the compute shader and find the kernel. Due to a Unity 2017.x bug, we must
		instantiate the compute shader as a resource if we want to use multiple instances of
		it in the same scene. */
		compute = (ComputeShader)Instantiate(Resources.Load("particleCompute"));
		computeKernel = compute.FindKernel("CSMain");

		// Create a quad mesh to render our particles
		GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Quad);
		quad = tmp.GetComponent<MeshFilter>().mesh;
		Destroy(tmp);

		// create the Particle array, load it into the particle buffer and upload it to the GPU
		Particle[] particles = new Particle[particleCount];
		for (int i = 0; i < particleCount; i++) {
			Vector3 pos = Vector3.zero;
			Vector3 vel = Vector3.zero;
			GenerateRandomParameters(ref pos, ref vel);
			particles[i].position = pos;
			particles[i].originalPosition = pos;
			particles[i].velocity = vel;
			particles[i].originalVelocity = vel;
			particles[i].lifetime = Random.Range(minimumLifetime, maximumLifetime);

			/* the initial age is set < 0 to allow us to set the particle rate, rather than
			each particle spawning at the same time. this allows us to "reuse" dead particles
			by resetting them, rather than removing them and adding new ones. */
			particles[i].age = -1f * Mathf.Floor(i/(float)particlesPer) * timeStep;
		}
		particleBuffer = new ComputeBuffer(particleCount, PARTICLE_SIZE);
		particleBuffer.SetData(particles);
		compute.SetBuffer(computeKernel, "particleBuffer", particleBuffer);
		particleMaterial.SetBuffer("particleBuffer", particleBuffer);

		// create the sphere collider array, load it into the buffer and upload it to the GPU
		if (sphereColliders.Length > 0) {
			sphereColliderBuffer = new ComputeBuffer(sphereColliders.Length, SPHERE_COLLIDER_SIZE);
			sphereColliderBuffer.SetData(sphereColliders);
			compute.SetBuffer(computeKernel, "sphereColliderBuffer", sphereColliderBuffer);
		}
        compute.SetInt("sphereColliderCount", sphereColliders.Length);



        // create the box collider array, load it into the buffer and upload it to the GPU
        if (boxColliders.Length > 0) {
			boxColliderBuffer = new ComputeBuffer(boxColliders.Length, BOX_COLLIDER_SIZE);
			boxColliderBuffer.SetData(boxColliders);
			compute.SetBuffer(computeKernel, "boxColliderBuffer", boxColliderBuffer);
        }
        compute.SetInt("boxColliderCount", boxColliders.Length);

        // warmup the shaders
        Shader.WarmupAllShaders();


		// create the Args array, and load it into the Arguments buffer, and save it for the Update() loop
		uint[] args = new uint[5] {(uint)quad.GetIndexCount(0), (uint)particleCount,
			(uint)quad.GetIndexStart(0), (uint)quad.GetBaseVertex(0), 0};
		argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		argsBuffer.SetData(args);
	}
	


	public void Update () {
		if (argsBuffer != null && particleBuffer != null) {
			if (updateEachFrame) {
				if (sphereColliders.Length > 0) {
					sphereColliderBuffer.Release();
					sphereColliderBuffer = new ComputeBuffer(sphereColliders.Length, SPHERE_COLLIDER_SIZE);
					sphereColliderBuffer.SetData(sphereColliders);
					compute.SetBuffer(computeKernel, "sphereColliderBuffer", sphereColliderBuffer);
                }
                compute.SetInt("sphereColliderCount", sphereColliders.Length);

                // create the box collider array, load it into the buffer and upload it to the GPU
                if (boxColliders.Length > 0) {
					boxColliderBuffer.Release();
					boxColliderBuffer = new ComputeBuffer(boxColliders.Length, BOX_COLLIDER_SIZE);
					boxColliderBuffer.SetData(boxColliders);
					compute.SetBuffer(computeKernel, "boxColliderBuffer", boxColliderBuffer);
                }
                compute.SetInt("boxColliderCount", boxColliders.Length);
            }

			// update simulation parameters
			compute.SetFloat("percentageAtDeath", percentageAtDeath);
			compute.SetFloat("coefficientOfRestitution", coefficientOfRestitution);
			compute.SetInt("randomizeCOR", randomizeCOR ? 1 : 0);
			compute.SetVector("gravity", gravity);
			compute.SetFloat("dt", Time.deltaTime);

			// rerun simulation
			compute.Dispatch(computeKernel, particleCount/THREAD_GROUPS + 1, 1, 1);

			// draw the instanced meshes
			particleMaterial.SetPass(0);
			Graphics.DrawMeshInstancedIndirect(quad, 0, particleMaterial,
				new Bounds(transform.position, Vector3.one * 2f * 500), argsBuffer);
		}
	}



	/* Frees the compute buffers when the emitter is destroyed. */
	void OnDestroy () {
		if (argsBuffer != null) {
			argsBuffer.Release();
		}

		if (particleBuffer != null) {
			particleBuffer.Release();
		}

		if (sphereColliderBuffer != null) {
			sphereColliderBuffer.Release();
		}

		if (boxColliderBuffer != null) {
			boxColliderBuffer.Release();
		}
	}



	/* Visualizes the various geometric components of the particle emitter. */
	void OnDrawGizmosSelected () {
		
		/* Gizmos are drawn in world space by default, so we offset them by the transform
		position to draw them in object space. */
		Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one); 

		// Draw an approximation of the velocities.
		if (visualizeVelocity) {
			Gizmos.color = Color.gray;

			for (int i = 0; i < 50; i++) {
				Vector3 pos = Vector3.zero;
				Vector3 vel = Vector3.zero;
				GenerateRandomParameters(ref pos, ref vel);
				Gizmos.DrawLine(pos, pos+vel);
			}
		}

		// Draw the colliders.
		Gizmos.color = Color.green;
		foreach(SphereCollider sphere in sphereColliders) {
			Gizmos.DrawWireSphere(sphere.center, sphere.radius);
		}

		foreach(BoxCollider box in boxColliders) {
			Gizmos.DrawWireCube(box.center, box.extents*2f);
		}

		// Draw the emitter.
		if (visualizeEmitter) {
			Gizmos.color = Color.white;

			/* The emitter must be drawn relative to the transform position AND the emitter position and rotation. */
			Gizmos.matrix = Matrix4x4.TRS(transform.position + emitterPosition, Quaternion.Euler(emitterRotation), Vector3.one);

			if (emitterShape == EmitterShape.Rectangle) {
				Gizmos.DrawWireCube(Vector3.zero, new Vector3(rectangleLength, 0, rectangleWidth));
			}
			else if (emitterShape == EmitterShape.Circle) {
				drawCircleGizmo(circleRadius, Vector3.zero);
			}
			else if (emitterShape == EmitterShape.Sphere) {
        		Gizmos.DrawWireSphere(Vector3.zero, sphereRadius);
			}
			else if (emitterShape == EmitterShape.Cone) {
				drawCircleGizmo(coneRadius, Vector3.zero);
				float upperRadius = coneRadius + Mathf.Tan(Mathf.Deg2Rad*Mathf.Max(0.001f, coneAngle));
				drawCircleGizmo(upperRadius, Vector3.up);
				Gizmos.DrawLine(Vector3.right*coneRadius, Vector3.up + Vector3.right*upperRadius);
				Gizmos.DrawLine(-Vector3.right*coneRadius, Vector3.up - Vector3.right*upperRadius);
				Gizmos.DrawLine(Vector3.forward*coneRadius, Vector3.up + Vector3.forward*upperRadius);
				Gizmos.DrawLine(-Vector3.forward*coneRadius, Vector3.up - Vector3.forward*upperRadius);
			}
		}
	}



	/* Draws a circle approximated by line segments. */
	void drawCircleGizmo(float radius, Vector3 center) {
		for (int i = 0; i < 360; i++) {
			Vector3 a = new Vector3(radius * Mathf.Sin(Mathf.Deg2Rad*i), 0, radius*Mathf.Cos(Mathf.Deg2Rad*i)) + center;
			Vector3 b = new Vector3(radius * Mathf.Sin(Mathf.Deg2Rad*(i+1)), 0, radius*Mathf.Cos(Mathf.Deg2Rad*(i+1))) + center;
			Gizmos.DrawLine(a, b);
		}
	}



	/* Returns a random position and velocity vector according to the current EmitterShape, relative to the
	emitter position and rotation. */
	void GenerateRandomParameters(ref Vector3 pos, ref Vector3 vel) {
		if (emitterShape == EmitterShape.Rectangle) {
			pos = new Vector3(Random.Range(-rectangleLength/2, rectangleLength/2), 0f, Random.Range(-rectangleWidth/2, rectangleWidth/2));
			vel = Vector3.up * Random.Range(minimumVelocity, maximumVelocity);
		}
		else if (emitterShape == EmitterShape.Circle) {
			Vector3 circ = Random.insideUnitCircle;
			pos = new Vector3(circ.x, 0f, circ.y) * circleRadius;
			vel = Vector3.up * Random.Range(minimumVelocity, maximumVelocity);
		}
		else if (emitterShape == EmitterShape.Sphere) {
			vel = Random.onUnitSphere * Random.Range(minimumVelocity, maximumVelocity);
			pos = Vector3.Normalize(vel) * sphereRadius;
		}
		else if (emitterShape == EmitterShape.Cone) {
			float upperRadius = coneRadius + Mathf.Tan(Mathf.Deg2Rad*Mathf.Max(0.001f, coneAngle));
			float dr = upperRadius - coneRadius;
			Vector2 circ = Random.insideUnitCircle * dr;
			vel = new Vector3(circ.x, 1f, circ.y);
			vel = Vector3.Normalize(vel) * Random.Range(minimumVelocity, maximumVelocity);
			pos = Vector3.Normalize(new Vector3(vel.x, 0, vel.z)) * Random.Range(0, coneRadius);	
		}

		/* Rotate the velocity vector by a random quaternion to introduce some visual variation
		in the particle trajectories. */
		vel = Quaternion.Lerp(Quaternion.identity, Random.rotation, randomness) * vel;

		// Rotate the position and velocity vectors by the emitter rotation.
		pos = Quaternion.Euler(emitterRotation)*pos;
		vel = Quaternion.Euler(emitterRotation)*vel;

		// Add the emitter position to the position vector.
		pos += emitterPosition;
	}

	public void TransferSphereColliders() {
		UnityEngine.SphereCollider[] spheres = GameObject.FindObjectsOfType<UnityEngine.SphereCollider>();
		sphereColliders = new SphereCollider[spheres.Length];
		for (int s = 0; s < spheres.Length; s++) {
			sphereColliders[s].center = spheres[s].transform.position;
			sphereColliders[s].radius = spheres[s].transform.lossyScale.x/2.0f;
		}
	}

	public void TransferBoxColliders() {
		UnityEngine.BoxCollider[] boxes = GameObject.FindObjectsOfType<UnityEngine.BoxCollider>();
		boxColliders = new BoxCollider[boxes.Length];
		for (int b = 0; b < boxes.Length; b++) {
			boxColliders[b].center = boxes[b].transform.position;
			boxColliders[b].extents = boxes[b].transform.lossyScale/2.0f;
		}
	}
}