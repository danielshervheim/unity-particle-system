//
// Copyright © Daniel Shervheim, 2019
// www.danielshervheim.com
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSDisplay : MonoBehaviour {

	public int numParticles = 0;

	bool show = true;

	// Use this for initialization
	void Start () {
		Emitter[] emitters = GameObject.FindObjectsOfType<Emitter>();

		foreach (Emitter e in emitters) {
			numParticles += e.particleCount;
		}
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.RightShift)) {
            show = !show;
        }
	}

	void OnGUI() {
		if (show) {
			GUI.Label(new Rect(25, 25, 100, 25), numParticles + " particles");
        	GUI.Label(new Rect(25, 50, 100, 25), 1.0f/Time.smoothDeltaTime + " fps");
		}  
    }
}
