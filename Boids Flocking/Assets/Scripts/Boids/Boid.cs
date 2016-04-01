using UnityEngine;
using System.Collections.Generic;

public abstract class Boid : MonoBehaviour {

    public enum PARTITIONING { NONE, KD_TREE, SPATIAL_HASH }
    private static readonly PARTITIONING PartitionType = PARTITIONING.NONE;
    [ReadOnly] [SerializeField] private BoidsPartitioner Partitioner;

    public enum TYPE { FISH, SHARK }
    public abstract TYPE Type { get; }
    public abstract TYPE[] NeighbourTypes { get; }
    public abstract TYPE[] PredatorTypes { get; }
    [Space(10)]

    [Header("Radii")]
    [Range(0f,float.PositiveInfinity)] public float NeighbourRadius = 16f;
    [Range(0f,float.PositiveInfinity)] public float RepellantRadius = 8f;
    [Range(0f,float.PositiveInfinity)] public float PredatorRadius = 32f;

	protected virtual void Start()
    {
        switch (PartitionType)
        {
            case PARTITIONING.NONE:         this.Partitioner = NoPartitioner.Instance;          break;
            case PARTITIONING.KD_TREE:      this.Partitioner = KDTreePartitioner.Instance;      break;
            case PARTITIONING.SPATIAL_HASH: this.Partitioner = SpatialHashPartitioner.Instance; break;
        }

        this.Partitioner.RegisterBoid(this);
    }

    protected virtual void OnDestroy()
    {
        this.Partitioner.DeregisterBoid(this);
    }

    protected virtual void FixedUpdate()
    {
        Dictionary<Boid.TYPE,List<Boid>> neighbours = this.Partitioner.FindNeighbours(this);
        Dictionary<Boid.TYPE,List<Boid>> repellants = this.Partitioner.FindRepellants(this);
        Dictionary<Boid.TYPE,List<Boid>> predators = this.Partitioner.FindPredators(this);

        Vector3 cohesion = this.CalculateCohesion(neighbours);
        Vector3 separation = this.CalculateSeparation(repellants);
        Vector3 alignment = this.CalculateAlignment(neighbours);
        Vector3 avoidPredators = this.CalculatePredators(predators);

        this.transform.position += (cohesion + separation + alignment + avoidPredators);
    }

    protected abstract Vector3 CalculateCohesion(Dictionary<Boid.TYPE,List<Boid>> neighbours);
    protected abstract Vector3 CalculateSeparation(Dictionary<Boid.TYPE,List<Boid>> repellants);
    protected abstract Vector3 CalculateAlignment(Dictionary<Boid.TYPE,List<Boid>> neighbours);
    protected abstract Vector3 CalculatePredators(Dictionary<Boid.TYPE,List<Boid>> predators);
}
