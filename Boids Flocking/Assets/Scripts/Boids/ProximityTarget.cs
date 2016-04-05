using UnityEngine;

public class ProximityTarget : BoidsTarget {

	[SerializeField] private float _radius;
    public float Radius { get { return this._radius; } }

    protected override void Start()
    {
        base.Start();
        this.BoidsManager.RegisterProximityTarget(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.BoidsManager.DeregisterProximityTarget(this);
    }

#if UNITY_EDITOR
    private SphereCollider Collider;
    void Awake()
    {
        this.Collider = this.gameObject.AddComponent<SphereCollider>();
        this.Collider.enabled = false;
    }
    void Update()
    {
        this.Collider.radius = this.Radius;
    }
#endif
}
