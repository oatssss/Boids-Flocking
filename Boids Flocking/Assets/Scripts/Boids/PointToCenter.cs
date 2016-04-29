using UnityEngine;
using System.Collections;

public class PointToCenter : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
		this.transform.rotation = Quaternion.LookRotation(Vector3.zero-this.transform.position, Vector3.up);
	}
}
