using UnityEngine;
using System.Collections.Generic;
using ExtensionMethods;

public abstract class Boid : MonoBehaviour {

    public enum PARTITIONING { NONE, KD_TREE, SPATIAL_HASH }
    private static readonly PARTITIONING PartitionType = PARTITIONING.NONE;
    [ReadOnly] [SerializeField] protected BoidsManager BoidsManager;
    [ReadOnly] [SerializeField] private float Speed;

#if UNITY_EDITOR
    // Lists
    [ReadOnly] [SerializeField] private List<Boid> Neighbours = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Repellants = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Predators = new List<Boid>();
#endif

    // Types
    public enum TYPE { FISH, SHARK }
    public abstract TYPE Type { get; }
    public abstract TYPE[] NeighbourTypes { get; }
    public abstract TYPE[] PredatorTypes { get; }

    // Radii
    public abstract float NeighbourRadius { get; }
    public abstract float RepellantRadius { get; }
    public abstract float PredatorRadius { get; }
    ///<summary>If the distance between this boid and the center of mass is less than this distance, the flock's attractive strength increases with distance. This will be a portion of the neighbour radius.</summary>
    public abstract float CohesiveSwitchDistance { get; }

    // Speeds
    protected abstract float MaxAcceleration { get; }
    protected abstract float MaxSpeed { get; }
    protected abstract float MinSpeed { get; }

    // Weights
    protected abstract float CohesionWeight { get; }
    protected abstract float SeparationWeight { get; }
    protected abstract float AlignmentWeight { get; }
    protected abstract float PredatorWeight { get; }
    // protected abstract float TargetWeight { get;}

	protected virtual void Start()
    {
        this.BoidsManager = BoidsManager.Instance;
        this.BoidsManager.RegisterBoid(this);
    }

    protected virtual void OnDestroy()
    {
        this.BoidsManager.DeregisterBoid(this);
    }

    protected virtual void FixedUpdate()
    {
        System.DateTime start = System.DateTime.Now;

        Dictionary<Boid.TYPE,List<Boid>> neighbours = this.BoidsManager.SpatialPartitioner.FindNeighbours(this);
        Dictionary<Boid.TYPE,List<Boid>> repellants = this.BoidsManager.SpatialPartitioner.FindRepellants(this);
        Dictionary<Boid.TYPE,List<Boid>> predators  = this.BoidsManager.SpatialPartitioner.FindPredators(this);

#if UNITY_EDITOR
        // Update inspector lists
        List<Boid> tmpNeighbours = new List<Boid>();
        foreach (Boid.TYPE type in neighbours.Keys)
            { tmpNeighbours.AddRange(neighbours[type]); }
        this.Neighbours = tmpNeighbours;

        List<Boid> tmpRepellants = new List<Boid>();
        foreach (Boid.TYPE type in repellants.Keys)
            { tmpRepellants.AddRange(repellants[type]); }
        this.Repellants = tmpRepellants;

        List<Boid> tmpPredators = new List<Boid>();
        foreach (Boid.TYPE type in predators.Keys)
            { tmpPredators.AddRange(predators[type]); }
        this.Predators = tmpPredators;
#endif

        Vector3 cohesion        = this.CalculateCohesion(neighbours);
        Vector3 separation      = this.CalculateSeparation(repellants);
        Vector3 alignment       = this.CalculateAlignment(neighbours);
        Vector3 avoidPredators  = this.CalculatePredators(predators);

        Vector3 updateVelocity = cohesion + separation + alignment + avoidPredators;

        Quaternion updateQuaternion = Quaternion.LookRotation(updateVelocity);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, updateQuaternion, 2*Time.fixedDeltaTime);

        float minSpeed = this.Speed - this.MaxAcceleration;
        float maxSpeed = this.Speed + this.MaxAcceleration;
        float mag = updateVelocity.magnitude;
        mag = Mathf.Clamp(mag, this.MinSpeed, this.MaxSpeed);
        updateVelocity = (this.transform.forward*mag*Time.fixedDeltaTime).ClampMagnitudeToRange(minSpeed, maxSpeed);
        this.Speed = updateVelocity.magnitude;
        this.transform.position = this.transform.position + updateVelocity;
    }

    protected virtual Vector3 CalculateCohesion(Dictionary<TYPE, List<Boid>> neighbours)
    {
        Vector3 centerOfMass = Vector3.zero;
        int neighbourCount = 0;
        // Iterate over all neighbour types
        foreach (List<Boid> typeSection in neighbours.Values)
        {
            // Iterate over all neighbours of that type
            foreach (Boid neighbour in typeSection)
            {
                // Should we skip itself in the CoM calculation?
                centerOfMass += neighbour.transform.position;
                neighbourCount++;
            }
        }

        // Return 0 if there were no neighbours
        if (neighbourCount <= 0)
            { return Vector3.zero; }

        centerOfMass /= neighbourCount;
        Vector3 toCoM = centerOfMass - this.transform.position;

        // If within the cohesive switch distance, attractive force increases with distance
        if (toCoM.magnitude <= this.CohesiveSwitchDistance)
            { toCoM = toCoM.NormalizeMagnitudeToRange(this.CohesiveSwitchDistance, this.MaxSpeed/3f, this.MaxSpeed); }

        // Otherwise, attractive force decreases with distance
        else
            { toCoM = toCoM.NormalizeMagnitudeToRange(this.CohesiveSwitchDistance, this.NeighbourRadius, this.MaxSpeed, 0f); }

        // Return a vector from this boid's position to the center of mass
        return toCoM * this.CohesionWeight;
    }

    protected virtual Vector3 CalculateAlignment(Dictionary<TYPE, List<Boid>> neighbours)
    {
        Vector3 averageForward = Vector3.zero;
        int neighbourCount = 0;
        foreach (List<Boid> typeSection in neighbours.Values)
        {
            foreach (Boid neighbour in typeSection)
            {
                // Make sure to skip itself
                if (neighbour == this)
                    { continue; }

                averageForward += neighbour.transform.forward;
                neighbourCount++;
            }
        }

        if (neighbourCount > 0)
            { averageForward /= neighbourCount; }

        return averageForward * this.AlignmentWeight * (this.NeighbourRadius/8f);
    }

    protected virtual Vector3 CalculateSeparation(Dictionary<TYPE, List<Boid>> repellants)
    {
        Vector3 separation = Vector3.zero;
        int repellantCount = 0;
        foreach (List<Boid> typeSection in repellants.Values)
        {
            // Debug.LogFormat("Time to get dictionary value: {0}", System.DateTime.Now-start);
            foreach (Boid repellant in typeSection)
            {
                // Make sure to skip itself
                if (repellant == this)
                    { continue; }

                Vector3 difference = (this.transform.position - repellant.transform.position);
                float diffMag = difference.magnitude;
                diffMag = diffMag < 0.0001f ? 0.0001f : diffMag;
                // We want boids farther away to have less separation effect
                separation += difference.normalized * (((this.RepellantRadius/diffMag) - 1) * (2*this.SeparationWeight*(this.RepellantRadius)));
                repellantCount++;
            }
        }

        if (repellantCount > 0)
            { separation /= repellantCount; }

        return separation * this.SeparationWeight;
    }

    protected virtual Vector3 CalculatePredators(Dictionary<TYPE, List<Boid>> predators)
    {
        return Vector3.zero;
    }
}
