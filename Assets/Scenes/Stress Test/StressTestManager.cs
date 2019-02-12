using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FPSDisplay))]
public class StressTestManager : MonoBehaviour {

	public Emitter[] emitters;

	private Emitter currentEmitter;

	private FPSDisplay fps;

	// Use this for initialization
	void Start () {
		currentEmitter = null;
		fps = GetComponent<FPSDisplay>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp("1")) {
			if (currentEmitter != null) {
				Destroy(currentEmitter);
				currentEmitter = null;
			}

			currentEmitter = Instantiate(emitters[0], Vector3.zero, Quaternion.identity, this.transform);
			fps.numParticles = currentEmitter.particleCount;
		}

		if (Input.GetKeyUp("2")) {
			if (currentEmitter != null) {
				Destroy(currentEmitter);
				currentEmitter = null;
			}

			currentEmitter = Instantiate(emitters[1], Vector3.zero, Quaternion.identity, this.transform);
			fps.numParticles = currentEmitter.particleCount;
		}

		if (Input.GetKeyUp("3")) {
			if (currentEmitter != null) {
				Destroy(currentEmitter);
				currentEmitter = null;
			}

			currentEmitter = Instantiate(emitters[2], Vector3.zero, Quaternion.identity, this.transform);
			fps.numParticles = currentEmitter.particleCount;
		}

		if (Input.GetKeyUp("4")) {
			if (currentEmitter != null) {
				Destroy(currentEmitter);
				currentEmitter = null;
			}

			currentEmitter = Instantiate(emitters[3], Vector3.zero, Quaternion.identity, this.transform);
			fps.numParticles = currentEmitter.particleCount;
		}

		if (Input.GetKeyUp("5")) {
			if (currentEmitter != null) {
				Destroy(currentEmitter);
				currentEmitter = null;
			}

			currentEmitter = Instantiate(emitters[4], Vector3.zero, Quaternion.identity, this.transform);
			fps.numParticles = currentEmitter.particleCount;
		}

		if (Input.GetKeyUp("6")) {
			if (currentEmitter != null) {
				Destroy(currentEmitter);
				currentEmitter = null;
			}

			currentEmitter = Instantiate(emitters[5], Vector3.zero, Quaternion.identity, this.transform);
			fps.numParticles = currentEmitter.particleCount;
		}
	}
}
