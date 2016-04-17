using UnityEngine;
using System.Collections;

public abstract class Boundary : MonoBehaviour {

	[SerializeField] protected BoidsManager BoidsManager { get { return BoidsManager.Instance; } }

	protected virtual void Start()
	{
		this.RegisterBoundary();
	}

	protected virtual void OnDestroy()
	{
		this.DeregisterBoundary();
	}

	protected abstract void RegisterBoundary();
	protected abstract void DeregisterBoundary();
}
