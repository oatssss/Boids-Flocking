using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour {

	[Range(0f,10f)] public float Rate = 1f;

	// Update is called once per frame
	void Update () {

		this.transform.Rotate(new Vector3(0,1,0), this.Rate*10*Time.deltaTime);
	}
}
