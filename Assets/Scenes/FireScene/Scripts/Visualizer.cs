//
// Copyright © Daniel Shervheim, 2019
// www.danielshervheim.com
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour {

	public GameObject flames;
	public GameObject embers;
	public GameObject smoke;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp("1")) {
            flames.SetActive(!flames.activeInHierarchy);
        }
        if (Input.GetKeyUp("2")) {
            embers.SetActive(!embers.activeInHierarchy);
        }
        if (Input.GetKeyUp("3")) {
            smoke.SetActive(!smoke.activeInHierarchy);
        }
	}
}
