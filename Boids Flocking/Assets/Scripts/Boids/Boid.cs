using UnityEngine;
using System.Collections.Generic;
using ExtensionMethods;
using System.Linq;

public abstract class Boid : MonoBehaviour {

    [ReadOnly] [SerializeField] protected BoidsManager BoidsManager;
    protected abstract bool Flocks { get; }
    protected abstract ITargetSelector TargetSelector { get; }
    protected abstract IPreySelector PreySelector { get; }
    protected abstract GoalSelector GoalSelectorStrategy { get; }

    // Types
    public enum TYPE { FISH, SHARK }
    public abstract TYPE Type { get; }
    public abstract TYPE[] NeighbourTypes { get; }
    public abstract TYPE[] PreyTypes { get; }
    public abstract TYPE[] PredatorTypes { get; }
    public abstract TYPE[] FleeTypes { get; }

    // Radii
    public abstract int   MaxFlockSize    { get; }
    public abstract float NeighbourRadius { get; }
    public abstract float RepellantRadius { get; }
    public abstract float PredatorRadius { get; }
    public abstract float PreyRadius { get; }
    public abstract float FleeRadius { get; }
    ///<summary>If the distance between this boid and the center of mass is less than this distance, the flock's attractive strength increases with distance. This will be a portion of the neighbour radius.</summary>
    public abstract float CohesiveSwitchDistance { get; }

    // Speeds
    [ReadOnly] [SerializeField] private float Speed;
    protected abstract float MaxAcceleration { get; }
    protected abstract float MaxDeceleration { get; }
    protected abstract float MaxSpeed { get; }
    protected abstract float MinSpeed { get; }

    // Weights
    protected abstract float CohesionWeight { get; }
    protected abstract float SeparationWeight { get; }
    protected abstract float AlignmentWeight { get; }
    protected abstract float PredatorWeight { get; }
    protected abstract float GoalWeight { get; }

#if UNITY_EDITOR
    #pragma warning disable 0414    // Private field assigned but not used.
    // Lists
    [ReadOnly] [SerializeField] private List<Boid> Neighbours = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Repellants = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Predators = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Prey = new List<Boid>();
    private SphereCollider NeighbourCollider;
    private SphereCollider RepellantCollider;
    private SphereCollider PredatorCollider;

    void Awake()
    {
        this.NeighbourCollider  = this.gameObject.AddComponent<SphereCollider>();
        this.RepellantCollider  = this.gameObject.AddComponent<SphereCollider>();
        this.PredatorCollider   = this.gameObject.AddComponent<SphereCollider>();

        this.NeighbourCollider.enabled  = false;
        this.RepellantCollider.enabled  = false;
        this.PredatorCollider.enabled   = false;
    }

    void Update()
    {
        this.NeighbourCollider.radius   = this.NeighbourRadius;
        this.RepellantCollider.radius   = this.RepellantRadius;
        this.PredatorCollider.radius    = this.PredatorRadius;
    }
#endif
    // Unlike the above lists, the boid does need to keep track of targets for the calculation since the target calculation occurs in stages
    [ReadOnly] [SerializeField] private List<BoidsTarget> TargetsWithinRange = new List<BoidsTarget>();
    [SerializeField] private Transform LeaderTrail = null;  // The transform that following boids should aim for if this boid is a leader
    [ReadOnly] [SerializeField] private Transform Goal;
    [ReadOnly] public bool Fleeing;
    [ReadOnly] public bool HasPredators;

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
        Dictionary<Boid.TYPE,List<Boid>> neighbours = this.BoidsManager.SpatialPartitioner.FindNeighbours(this);
        Dictionary<Boid.TYPE,List<Boid>> repellants = this.BoidsManager.SpatialPartitioner.FindRepellants(this);
        Dictionary<Boid.TYPE,List<Boid>> predators  = this.BoidsManager.SpatialPartitioner.FindPredators(this);
        Dictionary<Boid.TYPE,List<Boid>> prey       = this.BoidsManager.SpatialPartitioner.FindPrey(this);
        this.TargetsWithinRange                     = this.BoidsManager.SpatialPartitioner.FindTargetsNearBoid(this);
        this.Goal                                   = this.GetBestGoal(this.TargetsWithinRange, prey);

        this.HasPredators = predators.Count > 0 ? true : false;

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

        List<Boid> tmpPrey = new List<Boid>();
        foreach (Boid.TYPE type in prey.Keys)
            { tmpPrey.AddRange(prey[type]); }
        this.Prey = tmpPrey;

        List<Boid> tmpPredators = new List<Boid>();
        foreach (Boid.TYPE type in predators.Keys)
            { tmpPredators.AddRange(predators[type]); }
        this.Predators = tmpPredators;
#endif

        Vector3 cohesion        = this.Flocks ? (this.Fleeing ? -this.CalculateCohesion(neighbours) : this.CalculateCohesion(neighbours)) : Vector3.zero;
        Vector3 separation      = this.CalculateSeparation(repellants);
        Vector3 alignment       = (this.Flocks && !this.Fleeing) ? this.CalculateAlignment(neighbours) : Vector3.zero;
        Vector3 avoidPredators  = this.CalculatePredators(predators);
        Vector3 goalSeeking     = this.CalculateGoal();

        if (!this.Goal) { separation *= 0.3f; }
        if (this.HasPredators) { separation *= 0.5f; }

        Vector3 updateVelocity = cohesion + separation + alignment + avoidPredators + goalSeeking;

        // Update rotation
        Quaternion updateQuaternion = Quaternion.LookRotation(updateVelocity);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, updateQuaternion, 2*Time.fixedDeltaTime);

        // Restrict new speed to be within acceleration range
        float minSpeed = this.Speed - this.MaxAcceleration;
        float maxSpeed = this.Speed + this.MaxAcceleration;
        float mag = updateVelocity.magnitude;
        mag = Mathf.Clamp(mag, this.MinSpeed, this.MaxSpeed);
        // Use new orientation to propel forward
        updateVelocity = (this.transform.forward*mag*Time.fixedDeltaTime).ClampMagnitudeToRange(minSpeed, maxSpeed);
        updateVelocity = updateVelocity.ClampMagnitudeToRange(this.MinSpeed, this.MaxSpeed);
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
            { toCoM = toCoM.NormalizeMagnitudeToRange(this.CohesiveSwitchDistance, 0f, this.MaxSpeed); }

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

        return averageForward * this.AlignmentWeight * (this.NeighbourRadius/4f);
    }

    protected virtual Vector3 CalculateSeparation(Dictionary<TYPE, List<Boid>> repellants)
    {
        Vector3 separation = Vector3.zero;
        int repellantCount = 0;
        foreach (List<Boid> typeSection in repellants.Values)
        {
            foreach (Boid repellant in typeSection)
            {
                // Make sure to skip itself
                if (repellant == this)
                    { continue; }

                Vector3 difference = (this.transform.position - repellant.transform.position);
                float diffMag = difference.magnitude;
                diffMag = diffMag < 0.0001f ? 0.0001f : diffMag;
                // We want boids farther away to have less separation effect
                separation += difference.normalized * (((this.RepellantRadius/diffMag) - 1) * (4f*this.SeparationWeight*(this.RepellantRadius)));
                repellantCount++;
            }
        }

        if (repellantCount > 0)
            { separation /= repellantCount; }

        return separation * this.SeparationWeight;
    }

    protected virtual Vector3 CalculatePredators(Dictionary<TYPE, List<Boid>> predators)
    {
        Vector3 away = Vector3.zero;
        bool fleeing = false;
        foreach (TYPE type in predators.Keys)
        {
            List<Boid> typedPredators = predators[type];
            foreach (Boid predator in typedPredators)
            {
                Vector3 awayFromPredator = this.transform.position - predator.transform.position;
                float distanceToPredator = awayFromPredator.magnitude;

                if (this.FleeTypes.Contains(predator.Type) && distanceToPredator < this.FleeRadius)
                    { fleeing = true;}

                away += awayFromPredator.normalized;
                away *= 4.75f;
            }
        }

        this.Fleeing = fleeing;
        return away * this.PredatorWeight;
    }

    protected virtual Vector3 CalculateGoal()
    {
        if (this.Goal == null)
            { return Vector3.zero; }

        Vector3 toGoal = (this.Goal.transform.position - this.transform.position).normalized;
        return toGoal * this.GoalWeight * (this.NeighbourRadius/2f);
    }

    protected Transform GetBestGoal(List<BoidsTarget> targets, Dictionary<TYPE, List<Boid>> prey)
    {
        return this.GoalSelectorStrategy.GetBestGoal(this, targets, prey);
    }

    /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     *                                                                 *
     *  GOAL SELECTOR DEFINITIONS                                      *
     *                                                                 *
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

    protected interface IPreySelector
    {
        Boid GetBestPrey(Boid predator, Dictionary<TYPE, List<Boid>> prey);
    }

    protected interface ITargetSelector
    {
        BoidsTarget GetBestTarget(Boid originBoid, List<BoidsTarget> targets);
    }

    protected abstract class GoalSelector
    {
        protected ITargetSelector TargetSelector;
        protected IPreySelector PreySelector;

        public GoalSelector(ITargetSelector targetSelector, IPreySelector preySelector)
        {
            this.TargetSelector = targetSelector;
            this.PreySelector = preySelector;
        }

        public abstract Transform GetBestGoal(Boid originBoid, List<BoidsTarget> targets, Dictionary<TYPE, List<Boid>> prey);
    }

    protected class PreyFirstSelector : GoalSelector
    {
        public PreyFirstSelector(ITargetSelector targetSelector, IPreySelector preySelector) : base(targetSelector, preySelector) {}
        public override Transform GetBestGoal(Boid originBoid, List<BoidsTarget> targets, Dictionary<TYPE, List<Boid>> prey)
        {
            Boid bestPrey = this.PreySelector.GetBestPrey(originBoid, prey);

            if (bestPrey != null)
                { return bestPrey.transform; }

            // Otherwise continue looking through targets
            BoidsTarget bestTarget = this.TargetSelector.GetBestTarget(originBoid, targets);
            return (bestTarget != null) ? bestTarget.transform : null;
        }
    }

    protected class ClosestPreySelector : IPreySelector
    {
        public Boid GetBestPrey(Boid predator, Dictionary<TYPE, List<Boid>> prey)
        {
            Boid best = null;
            float sqrDistanceToBest = 0f;
            foreach (TYPE type in prey.Keys)
            {
                List<Boid> typedPrey = prey[type];
                foreach (Boid potential in typedPrey)
                {
                    float sqrDistanceToPotential = (potential.transform.position - predator.transform.position).sqrMagnitude;

                    if (best == null)
                    {
                        best = potential;
                        sqrDistanceToBest = sqrDistanceToPotential;
                        continue;
                    }

                    // Best is chosen based on closest distance
                    if (sqrDistanceToPotential < sqrDistanceToBest)
                    {
                        best = potential;
                        sqrDistanceToBest = sqrDistanceToPotential;
                    }
                }
            }

            return best;
        }
    }

    protected class ClosestTargetSelector : ITargetSelector
    {
        public BoidsTarget GetBestTarget(Boid originBoid, List<BoidsTarget> targets)
        {
            BoidsTarget best = null;
            float sqrDistanceToBest = 0f;
            foreach (BoidsTarget target in targets)
            {
                float sqrDistanceToTarget = (target.transform.position - originBoid.transform.position).sqrMagnitude;

                if (best == null)
                {
                    best = target;
                    sqrDistanceToBest = sqrDistanceToTarget;
                    continue;
                }

                // Best is chosen based on closest distance
                if (sqrDistanceToTarget < sqrDistanceToBest)
                {
                    best = target;
                    sqrDistanceToBest = sqrDistanceToTarget;
                }
            }

            return best;
        }
    }

    protected class MostRecentSelector : ITargetSelector
    {
        public BoidsTarget GetBestTarget(Boid originBoid, List<BoidsTarget> targets)
        {
            BoidsTarget best = null;
            float bestCreationTime = float.PositiveInfinity;
            foreach (BoidsTarget target in targets)
            {
                float targetCreationTime = target.CreationTime;

                if (best == null)
                {
                    best = target;
                    bestCreationTime = targetCreationTime;
                    continue;
                }

                // Best is chosen based on most recent creation
                if (targetCreationTime < bestCreationTime)
                {
                    best = target;
                    bestCreationTime = targetCreationTime;
                }
            }

            return best;
        }
    }
}
