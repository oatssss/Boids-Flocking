using UnityEngine;
using System.Collections.Generic;

public class BoidsManager : UnitySingletonPersistent<BoidsManager> {

	[ReadOnly] public Dictionary<Boid.TYPE,List<Boid>> AllBoids = new Dictionary<Boid.TYPE,List<Boid>>();
    public BoidsPartitioner SpatialPartitioner;
    [Space(10)]

    [Header("Fish Settings")]
    [Header("Radii")]
    [Range(0.1f,256f)]  public float FishNeighbourRadius        = 4f;
    [Range(0.1f,32f)]   public float FishRepellantRadius        = 1f;
    [Range(0.1f,1024f)] public float FishPredatorRadius         = 8f;
    [Range(0.01f,1f)]   public float FishCohesiveSwitchRatio    = 0.75f;
    [ReadOnly] [SerializeField] private float _fishCohesiveSwitchDistance;

    [Header("Speeds")]
    [Range(0.0001f,0.001f)] public float FishMaxAcceleration = 0.5f;
    [Range(0f,10f)] public float FishMinSpeed = 0f;
    [Range(0f,20f)] public float FishMaxSpeed = 1f;

    [Header("Weights")]
    [Range(0f,5f)] public float FishCohesionWeight = 1f;
    [Range(0f,5f)] public float FishSeparationWeight = 1f;
    [Range(0f,5f)] public float FishAlignmentWeight = 1f;
    [Range(0f,5f)] public float FishPredatorWeight = 1f;

    // Cached items
    public float FishCohesiveSwitchDistance {
        get         { return this._fishCohesiveSwitchDistance; }
        private set { this._fishCohesiveSwitchDistance = value; }
    }

    protected override void Awake()
    {
        base.Awake();

        // If no partitioner was set, use NoPartitioner as default
        if (!this.SpatialPartitioner)
        {
            Debug.LogErrorFormat("A spatial partitioner was not set for {0}, using no partitioner as default.", this);
            this.SpatialPartitioner = gameObject.AddComponent<NoPartitioner>();
        }

        // Cache items
        this.CalculateValidSettings();
    }

#if UNITY_EDITOR
    void Update()
    {
        // Update cached items
        this.CalculateValidSettings();
    }
#endif

    public void RegisterBoid(Boid boid)
    {
        List<Boid> boids;
        bool exists = this.AllBoids.TryGetValue(boid.Type, out boids);

        if (!exists)
            { boids = new List<Boid>(); }

        boids.Add(boid);
        this.AllBoids[boid.Type] = boids;
    }

    public void DeregisterBoid(Boid boid)
    {
        List<Boid> boids = this.AllBoids[boid.Type];
        boids.Remove(boid);
    }

    private void CalculateValidSettings()
    {
        // Find the current radius based on ratio
        this.FishCohesiveSwitchDistance = this.FishNeighbourRadius * this.FishCohesiveSwitchRatio;

        // Repellant radius should not be greater than neighbour radius
        this.FishRepellantRadius = Mathf.Clamp(this.FishRepellantRadius, 0f, this.FishNeighbourRadius);

        // Min speed should always be less than max speed
        this.FishMinSpeed = Mathf.Clamp(this.FishMinSpeed, 0f, this.FishMaxSpeed);
    }
}
