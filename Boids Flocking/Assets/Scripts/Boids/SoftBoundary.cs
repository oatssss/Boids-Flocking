using UnityEngine;
using System.Collections;
using ExtensionMethods;
using System;

[RequireComponent(typeof(Collider))]
public class SoftBoundary : Boundary {

	protected override void Start()
	{
		base.Start();
		this.EnforceLayerMembership("SoftBoundaries");
	}

	void OnTriggerEnter(Collider other)
	{
		Boid boid = other.GetComponent<Boid>();
		boid.EnterSoftBound(this);
	}

	void OnTriggerExit(Collider other)
	{
		Boid boid = other.GetComponent<Boid>();
		boid.ExitSoftBound(this);
	}

    protected override void RegisterBoundary()
    {
        this.BoidsManager.RegisterSoftBoundary(this);
    }

    protected override void DeregisterBoundary()
    {
        this.BoidsManager.DeregisterSoftBoundary(this);
    }
}
