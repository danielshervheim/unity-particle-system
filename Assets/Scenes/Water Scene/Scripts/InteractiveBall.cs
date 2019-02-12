using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveBall : MonoBehaviour {

	public Emitter[] emitters;
	public Transform camera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		foreach (Emitter emitter in emitters) {
			emitter.sphereColliders[emitter.sphereColliders.Length-1].center = this.transform.position;
			emitter.sphereColliders[emitter.sphereColliders.Length-1].radius = this.transform.lossyScale.x/2.0f;
		}

		if (Physics.Raycast(camera.position, camera.forward, 10f) && Input.GetKeyDown("p")) {
			this.transform.parent = camera;
		}

		if (Input.GetKeyDown("o")) {
			this.transform.parent = null;
		}
	}
}
