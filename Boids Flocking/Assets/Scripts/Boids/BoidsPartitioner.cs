using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public abstract class BoidsPartitioner : MonoBehaviour
{
    [ReadOnly] [SerializeField] protected BoidsManager BoidsManager;

    void Start()
    {
        this.BoidsManager = BoidsManager.Instance;
    }

    public Dictionary<Boid.TYPE,List<Boid>> FindNeighbours(Boid boid)
    {
        // Get the friendly types that 'boid' flocks with
        Boid.TYPE[] neighbourTypes = boid.NeighbourTypes;
        Dictionary<Boid.TYPE,List<Boid>> potentialNeighbours = this.FindTypesWithinRadius(neighbourTypes, boid.NeighbourRadius, boid.transform.position);

        foreach (Boid.TYPE type in potentialNeighbours.Keys)
        {
            List<Boid> typedNeighbours = potentialNeighbours[type];
            foreach (Boid neighbour in typedNeighbours.ToArray())
            {

            }
        }

        return potentialNeighbours;
    }

    public Dictionary<Boid.TYPE, List<Boid>> FindRepellants(Boid boid)
    {
        // Get all the boids within the repellant radius that aren't predators or prey
        Boid.TYPE[] repellantTypes = this.BoidsManager.AllBoids.Keys.Except(boid.PredatorTypes).Except(boid.PreyTypes).ToArray();
        return this.FindTypesWithinRadius(repellantTypes, boid.RepellantRadius, boid.transform.position);
    }

    public Dictionary<Boid.TYPE, List<Boid>> FindPrey(Boid boid)
    {
        // Get the prey types that 'boid' hunts
        Boid.TYPE[] preyTypes = boid.PreyTypes;
        return this.FindTypesWithinRadius(preyTypes, boid.PreyRadius, boid.transform.position);
    }

    public Dictionary<Boid.TYPE, List<Boid>> FindPredators(Boid boid)
    {
        // Get the predator types that 'boid' avoids
        Boid.TYPE[] predatorTypes = boid.PredatorTypes;
        return this.FindTypesWithinRadius(predatorTypes, boid.PredatorRadius, boid.transform.position);
    }

    protected abstract Dictionary<Boid.TYPE,List<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Vector3 center);
    public abstract List<BoidsTarget> FindTargetsNearBoid(Boid boid);
}
