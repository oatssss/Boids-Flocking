using UnityEngine;

public class BoidsTarget : MonoBehaviour {

    [ReadOnly] [SerializeField] protected BoidsManager BoidsManager;
    [SerializeField] private bool _easeIn;
	public bool EaseIn { get { return this._easeIn; } }

    [SerializeField] private Boid.TYPE[] _attractedTypes;
    public Boid.TYPE[] AttractedTypes { get { return this._attractedTypes; } }

    [SerializeField] private float _creationTime;
    public float CreationTime { get { return this._creationTime; } private set { this._creationTime = value; } }

    protected virtual void Start()
    {
        this.BoidsManager = BoidsManager.Instance;
        this.BoidsManager.RegisterTarget(this);
        this.CreationTime = Time.realtimeSinceStartup;
    }

    protected virtual void OnDestroy()
    {
        this.BoidsManager.DeregisterTarget(this);
    }
}
