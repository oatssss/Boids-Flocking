using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public abstract class BoidsPartitioner : MonoBehaviour
{
    [SerializeField] protected BoidsManager BoidsManager { get { return BoidsManager.Instance; } }

    public Dictionary<Boid.TYPE,HashSet<Boid>> FindNeighbours(Boid boid)
    {
        // Get the friendly types that 'boid' flocks with
        Boid.TYPE[] neighbourTypes = boid.NeighbourTypes;
        Dictionary<Boid.TYPE,HashSet<Boid>> potentialNeighbours = this.FindTypesWithinRadius(neighbourTypes, boid.NeighbourRadius, boid, this.BoidsManager.MaxFishFlockSize);
        return potentialNeighbours;
    }

    public Dictionary<Boid.TYPE, HashSet<Boid>> FindRepellants(Boid boid)
    {
        // Get all the boids within the repellant radius that aren't predators or prey
        Boid.TYPE[] repellantTypes = this.BoidsManager.AllBoids.Keys.Except(boid.PredatorTypes).Except(boid.PreyTypes).ToArray();
        return this.FindTypesWithinRadius(repellantTypes, boid.RepellantRadius, boid, 15);
    }

    public Dictionary<Boid.TYPE, HashSet<Boid>> FindPrey(Boid boid)
    {
        // Get the prey types that 'boid' hunts
        Boid.TYPE[] preyTypes = boid.PreyTypes;
        return this.FindTypesWithinRadius(preyTypes, boid.PreyRadius, boid);
    }

    public Dictionary<Boid.TYPE, HashSet<Boid>> FindPredators(Boid boid)
    {
        // Get the predator types that 'boid' avoids
        Boid.TYPE[] predatorTypes = boid.PredatorTypes;
        return this.FindTypesWithinRadius(predatorTypes, boid.PredatorRadius, boid);
    }

    protected abstract Dictionary<Boid.TYPE,HashSet<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Boid originBoid, int maximum = int.MaxValue);
    public abstract HashSet<ProximityTarget> FindTargetsNearBoid(Boid boid);
    public abstract HardBoundary FindClosestHardBound(Boid boid);
}
