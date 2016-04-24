// #define DEBUG

using UnityEngine;
using System.Collections.Generic;
using ExtensionMethods;
using System.Linq;
using System.Diagnostics;

public abstract class Boid : MonoBehaviour {

    [SerializeField] private Rigidbody Rigidbody;
    public bool DebugFocus = false;
    [SerializeField] protected BoidsManager BoidsManager { get { return BoidsManager.Instance; } }
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
    protected abstract float TurnRadius { get; }

    // Weights
    protected abstract float CohesionWeight { get; }
    protected abstract float SeparationWeight { get; }
    protected abstract float AlignmentWeight { get; }
    protected abstract float PredatorWeight { get; }
    protected abstract float GoalWeight { get; }
    protected abstract float BoundaryWeight { get; }

    // Boundaries
    protected HashSet<HardBoundary> EncapsulatingHardBounds = new HashSet<HardBoundary>();
    protected HashSet<SoftBoundary> EncapsulatingSoftBounds = new HashSet<SoftBoundary>();
    protected HardBoundary LastEncounteredHardBound;
    protected SoftBoundary LastEncounteredSoftBound;
    [ReadOnly] [SerializeField] protected bool OutOfHardBounds = true;
    [ReadOnly] [SerializeField] protected bool OutOfSoftBounds;

    // Flock Capping
    [ReadOnly] public Flock Flock;

#if (!SKIP_DEBUG && UNITY_EDITOR)
    #pragma warning disable 0414    // Private field assigned but not used.
    // Sets
    [ReadOnly] [SerializeField] private List<Boid> Neighbours = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Repellants = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Predators = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> Prey = new List<Boid>();
    [ReadOnly] [SerializeField] private List<Boid> FlockList = new List<Boid>();
    private SphereCollider NeighbourCollider;
    private SphereCollider RepellantCollider;
    private SphereCollider PredatorCollider;

    void Awake()
    {
        // DISCLAIMER: METHOD IS WITHIN PRECOMPILER BLOCK
        this.EnforceLayerMembership("Boids");

        GameObject colliderContainer = new GameObject();
        colliderContainer.name = "Colliders";
        colliderContainer.transform.SetParent(this.transform);
        colliderContainer.transform.localPosition = Vector3.zero;
        colliderContainer.layer = 0;

        this.NeighbourCollider  = colliderContainer.AddComponent<SphereCollider>();
        this.RepellantCollider  = colliderContainer.AddComponent<SphereCollider>();
        this.PredatorCollider   = colliderContainer.AddComponent<SphereCollider>();

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
    // Unlike the above sets, the boid does need to keep track of targets for the calculation since the target calculation occurs in stages
    [ReadOnly] [SerializeField] private HashSet<ProximityTarget> TargetsWithinRange = new HashSet<ProximityTarget>();
    [ReadOnly] [SerializeField] protected Transform Goal;
    [ReadOnly] public bool Fleeing;
    [ReadOnly] public bool HasPredators;

	protected virtual void Start()
    {
        this.BoidsManager.RegisterBoid(this);
    }

    protected virtual void OnDestroy()
    {
        if (this.Flock != null)
            { this.Flock.RemoveMember(this); }
        this.BoidsManager.DeregisterBoid(this);
    }

    public void EnterHardBound(HardBoundary hardBound)
    {
        this.EncapsulatingHardBounds.Add(hardBound);
        this.OutOfHardBounds = false;
    }

    public void EnterSoftBound(SoftBoundary softBound)
    {
        this.EncapsulatingSoftBounds.Add(softBound);
        this.OutOfSoftBounds = false;
    }

    public void ExitHardBound(HardBoundary hardBound)
    {
        this.EncapsulatingHardBounds.Remove(hardBound);
        if (this.EncapsulatingHardBounds.Count() <= 0)
        {
            this.LastEncounteredHardBound = hardBound;
            this.OutOfHardBounds = true;
        }
    }

    public void ExitSoftBound(SoftBoundary softBound)
    {
        this.EncapsulatingSoftBounds.Remove(softBound);
        if (this.EncapsulatingSoftBounds.Count() <= 0)
        {
            this.LastEncounteredSoftBound = softBound;
            this.OutOfSoftBounds = true;
        }
    }

    protected virtual void FixedUpdate()
    {
        Dictionary<Boid.TYPE,HashSet<Boid>> neighbours = this.BoidsManager.SpatialPartitioner.FindNeighbours(this);
        // Dictionary<Boid.TYPE,HashSet<Boid>> repellants = this.BoidsManager.SpatialPartitioner.FindRepellants(this);
        Dictionary<Boid.TYPE,HashSet<Boid>> repellants = neighbours;
        Dictionary<Boid.TYPE,HashSet<Boid>> predators  = this.BoidsManager.SpatialPartitioner.FindPredators(this);
        Dictionary<Boid.TYPE,HashSet<Boid>> prey       = this.BoidsManager.SpatialPartitioner.FindPrey(this);
        this.TargetsWithinRange                        = this.BoidsManager.SpatialPartitioner.FindTargetsNearBoid(this);
        this.Goal                                      = this.GetBestGoal(this.TargetsWithinRange, prey);

        if (this.Flock != null && (this.Flock.transform.position - this.transform.position).magnitude > 3)
            { this.Flock.RemoveMember(this); }

        this.HasPredators = predators.Count > 0 ? true : false;

#if (!SKIP_DEBUG && UNITY_EDITOR)
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
        Vector3 wander          = this.CalculateWander();
        Vector3 boundary        = this.CalculateBoundary();

        if (!this.Goal || this.HasPredators) { separation *= 0.3f; }
        // UnityEngine.Debug.LogWarningFormat("COHESION: {0}\nSEPARATION: {1}\nALIGNMENT: {2}\nPREDATORS: {3}\nGOAL: {4}\nWANDER: {5}\nBOUNDARY: {6}", cohesion, separation, alignment, avoidPredators, goalSeeking, wander, boundary);
        Vector3 updateVelocity = cohesion + separation + alignment + avoidPredators + goalSeeking + wander + boundary;

        // Update rotation
        Quaternion updateQuaternion = Quaternion.LookRotation(updateVelocity);
        // this.transform.rotation = Quaternion.Slerp(this.transform.rotation, updateQuaternion, 2*Time.fixedDeltaTime);
        this.Rigidbody.MoveRotation(Quaternion.Slerp(this.transform.rotation, updateQuaternion, 2*Time.fixedDeltaTime/this.TurnRadius));

        // Restrict new speed to be within acceleration range
        float minSpeed = this.Speed - this.MaxAcceleration;
        float maxSpeed = this.Speed + this.MaxAcceleration;
        float mag = updateVelocity.magnitude;
        mag = Mathf.Clamp(mag, this.MinSpeed, this.MaxSpeed);
        // Use new orientation to propel forward
        updateVelocity = (this.transform.forward*mag*Time.fixedDeltaTime).ClampMagnitudeToRange(minSpeed, maxSpeed);
        updateVelocity = updateVelocity.ClampMagnitudeToRange(this.MinSpeed, this.MaxSpeed);
        this.Speed = updateVelocity.magnitude;
        // this.transform.position = this.transform.position + updateVelocity;
        this.Rigidbody.MovePosition(this.transform.position + this.transform.forward*this.Speed);
    }

    protected virtual Vector3 CalculateCohesion(Dictionary<TYPE, HashSet<Boid>> neighbours)
    {
        Vector3 centerOfMass = Vector3.zero;
        int neighbourCount = 0;
        // Iterate over all neighbour types
        foreach (HashSet<Boid> typeSection in neighbours.Values)
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
        return toCoM.normalized * this.CohesionWeight;
    }

    protected virtual Vector3 CalculateAlignment(Dictionary<TYPE, HashSet<Boid>> neighbours)
    {
        Vector3 averageForward = Vector3.zero;
        int neighbourCount = 0;
        foreach (HashSet<Boid> typeSection in neighbours.Values)
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

    protected virtual Vector3 CalculateSeparation(Dictionary<TYPE, HashSet<Boid>> repellants)
    {
        Vector3 separation = Vector3.zero;
        int repellantCount = 0;
        foreach (HashSet<Boid> typeSection in repellants.Values)
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

    protected virtual Vector3 CalculatePredators(Dictionary<TYPE, HashSet<Boid>> predators)
    {
        Vector3 away = Vector3.zero;
        bool fleeing = false;
        int predatorCount = 0;
        foreach (TYPE type in predators.Keys)
        {
            HashSet<Boid> typedPredators = predators[type];
            foreach (Boid predator in typedPredators)
            {
                Vector3 awayFromPredator = this.transform.position - predator.transform.position;
                float distanceToPredator = awayFromPredator.magnitude;

                if (this.FleeTypes.Contains(predator.Type) && distanceToPredator < this.FleeRadius)
                    { fleeing = true;}

                away += awayFromPredator.normalized;
                predatorCount++;
            }
        }

        if (predatorCount == 0)
            { return Vector3.zero; }

        away /= predatorCount;
        away *= 4.75f;
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

    protected virtual Vector3 CalculateWander()
    {
        return this.transform.forward * 0.1f;
    }

    protected virtual Vector3 CalculateBoundary()
    {
        if (this.LastEncounteredHardBound == null)
            { this.LastEncounteredHardBound = this.BoidsManager.SpatialPartitioner.FindClosestHardBound(this); }

        // Do the same for soft bound?

        Vector3 towardsBound = Vector3.zero;

        if (this.OutOfHardBounds)
            { towardsBound = this.LastEncounteredHardBound.transform.position - this.transform.position; }

        if (this.OutOfSoftBounds && this.Goal == null)
            { towardsBound = this.LastEncounteredSoftBound.transform.position - this.transform.position; }

        return towardsBound * this.BoundaryWeight/2.5f;
    }

    protected Transform GetBestGoal(HashSet<ProximityTarget> targets, Dictionary<TYPE, HashSet<Boid>> prey)
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
        Boid GetBestPrey(Boid predator, Dictionary<TYPE, HashSet<Boid>> prey);
    }

    protected interface ITargetSelector
    {
        BoidsTarget GetBestTarget(Boid originBoid, HashSet<ProximityTarget> targets);
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

        public abstract Transform GetBestGoal(Boid originBoid, HashSet<ProximityTarget> targets, Dictionary<TYPE, HashSet<Boid>> prey);
    }

    protected class PreyFirstSelector : GoalSelector
    {
        public PreyFirstSelector(ITargetSelector targetSelector, IPreySelector preySelector) : base(targetSelector, preySelector) {}
        public override Transform GetBestGoal(Boid originBoid, HashSet<ProximityTarget> targets, Dictionary<TYPE, HashSet<Boid>> prey)
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
        public Boid GetBestPrey(Boid predator, Dictionary<TYPE, HashSet<Boid>> prey)
        {
            Boid best = null;
            float sqrDistanceToBest = 0f;
            foreach (TYPE type in prey.Keys)
            {
                HashSet<Boid> typedPrey = prey[type];
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
        public BoidsTarget GetBestTarget(Boid originBoid, HashSet<ProximityTarget> targets)
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
        public BoidsTarget GetBestTarget(Boid originBoid, HashSet<ProximityTarget> targets)
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
