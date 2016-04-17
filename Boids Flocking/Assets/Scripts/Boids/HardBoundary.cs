using UnityEngine;
using ExtensionMethods;

[RequireComponent(typeof(Collider))]
public class HardBoundary : Boundary {

	protected override void Start()
	{
		base.Start();
		this.EnforceLayerMembership("HardBoundaries");
	}

	void OnTriggerEnter(Collider other)
	{
		Boid boid = other.GetComponent<Boid>();
		boid.EnterHardBound(this);
	}

	void OnTriggerExit(Collider other)
	{
		Boid boid = other.GetComponent<Boid>();
		boid.ExitHardBound(this);
	}

    protected override void RegisterBoundary()
    {
        this.BoidsManager.RegisterHardBoundary(this);
    }

    protected override void DeregisterBoundary()
    {
        this.BoidsManager.DeregisterHardBoundary(this);
    }
}
