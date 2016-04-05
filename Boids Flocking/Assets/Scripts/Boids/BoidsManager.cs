﻿using UnityEngine;
using System.Collections.Generic;

public class BoidsManager : UnitySingletonPersistent<BoidsManager> {

    public BoidsPartitioner SpatialPartitioner;
	[ReadOnly] public Dictionary<Boid.TYPE,List<Boid>> AllBoids = new Dictionary<Boid.TYPE,List<Boid>>();
    [ReadOnly] public List<BoidsTarget> AllTargets = new List<BoidsTarget>();
    [ReadOnly] public List<ProximityTarget> AllProximityTargets = new List<ProximityTarget>();
    [Space(10)]

    [Header("Fish Settings")]
    [Header("Radii")]
    [Range(0.1f,32f)]   public float FishNeighbourRadius        = 3f;
    [Range(0.1f,8f)]    public float FishRepellantRadius        = 1.5f;
    [Range(0.1f,64f)]   public float FishPredatorRadius         = 8f;
    [Range(0.1f,64f)]   public float FishFleeRadius            = 2f;
    [Range(0.01f,1f)]   public float FishCohesiveSwitchRatio    = 0.75f;
    [ReadOnly] [SerializeField] private float _fishCohesiveSwitchDistance;

    [Header("Speeds")]
    [Range(0.0001f,0.001f)] public float FishMaxAcceleration = 0.0004f;
    [Range(0.0001f,0.001f)] public float FishMaxDeceleration = 0.0005f;
    [Range(0f,0.1f)] public float FishMinSpeed = 0.01f;
    [Range(0f,20f)] public float FishMaxSpeed = 2f;

    [Header("Weights")]
    [Range(0f,5f)] public float FishCohesionWeight = 1f;
    [Range(0f,5f)] public float FishSeparationWeight = 2f;
    [Range(0f,5f)] public float FishAlignmentWeight = 2.25f;
    [Range(0f,5f)] public float FishPredatorWeight = 1f;
    [Range(0f,5f)] public float FishGoalWeight = 1f;
    [Space(20)]

    [Header("Shark Settings")]
    [Header("Radii")]
    [Range(0.1f,32f)]   public float SharkNeighbourRadius       = 7f;
    [Range(0.1f,8f)]    public float SharkRepellantRadius       = 3f;
    [Range(0.1f,64f)]   public float SharkPredatorRadius        = 15f;
    [Range(0.1f,64f)]   public float SharkFleeRadius           = 6f;
    [Range(0.01f,1f)]   public float SharkCohesiveSwitchRatio   = 0.75f;
    [ReadOnly] [SerializeField] private float _sharkCohesiveSwitchDistance;

    [Header("Speeds")]
    [Range(0.0001f,0.001f)] public float SharkMaxAcceleration = 0.0006f;
    [Range(0.0001f,0.001f)] public float SharkMaxDeceleration = 0.0007f;
    [Range(0f,0.1f)] public float SharkMinSpeed = 0.015f;
    [Range(0f,20f)] public float SharkMaxSpeed = 4f;

    [Header("Weights")]
    [Range(0f,5f)] public float SharkCohesionWeight = 1f;
    [Range(0f,5f)] public float SharkSeparationWeight = 2f;
    [Range(0f,5f)] public float SharkAlignmentWeight = 2.25f;
    [Range(0f,5f)] public float SharkPredatorWeight = 1f;
    [Range(0f,5f)] public float SharkGoalWeight = 1f;

    // Cached items
    public float FishCohesiveSwitchDistance {
        get         { return this._fishCohesiveSwitchDistance; }
        private set { this._fishCohesiveSwitchDistance = value; }
    }
    public float SharkCohesiveSwitchDistance {
        get         { return this._sharkCohesiveSwitchDistance; }
        private set { this._sharkCohesiveSwitchDistance = value; }
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

    private void CalculateValidSettings()
    {
        // Find the current radius based on ratio
        this.FishCohesiveSwitchDistance = this.FishNeighbourRadius * this.FishCohesiveSwitchRatio;

        // Repellant radius should not be greater than neighbour radius
        this.FishRepellantRadius = Mathf.Clamp(this.FishRepellantRadius, 0f, this.FishNeighbourRadius);

        // Min speed should always be less than max speed
        this.FishMinSpeed = Mathf.Clamp(this.FishMinSpeed, 0f, this.FishMaxSpeed);
    }

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

    public void RegisterTarget(BoidsTarget target)
    {
        this.AllTargets.Add(target);
    }

    public void DeregisterTarget(BoidsTarget target)
    {
        this.AllTargets.Remove(target);
    }

    public void RegisterProximityTarget(ProximityTarget target)
    {
        this.AllProximityTargets.Add(target);
    }

    public void DeregisterProximityTarget(ProximityTarget target)
    {
        this.AllProximityTargets.Remove(target);
    }
}