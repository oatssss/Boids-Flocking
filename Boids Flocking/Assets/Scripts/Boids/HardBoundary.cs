using UnityEngine;
using ExtensionMethods;

[RequireComponent(typeof(Collider))]
public class HardBoundary : Boundary {

#if !SKIP_BENCHMARK
	public bool Pulsate = true;
	private Vector3 originalLocalScale;

	private void Contract()
	{
		this.transform.localScale = Vector3.zero;
	}
	private void Expand()
	{
		this.transform.localScale = this.originalLocalScale;
	}
#endif

	protected override void Start()
	{
		base.Start();
		this.EnforceLayerMembership("HardBoundaries");

// #if !SKIP_BENCHMARK
// 	this.originalLocalScale = this.transform.localScale;
// 	InvokeRepeating("Contract", 0, 16);
// 	InvokeRepeating("Expand", 3, 16);
// #endif
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
