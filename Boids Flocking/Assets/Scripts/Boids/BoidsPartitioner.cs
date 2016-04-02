using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public abstract class BoidsPartitioner : MonoBehaviour
{
    [ReadOnly] [SerializeField] protected Dictionary<Boid.TYPE,List<Boid>> AllBoids;

    void Start()
    {
        this.AllBoids = BoidsManager.Instance.AllBoids;
    }

    public Dictionary<Boid.TYPE,List<Boid>> FindNeighbours(Boid boid)
    {
        // Get the friendly types that 'boid' flocks with
        Boid.TYPE[] neighbourTypes = boid.NeighbourTypes;
        return this.FindTypesWithinRadius(neighbourTypes, boid.NeighbourRadius, boid);
    }

    public Dictionary<Boid.TYPE, List<Boid>> FindRepellants(Boid boid)
    {
        // Get all the boids within the repellant radius of the boid
        Boid.TYPE[] allTypes = this.AllBoids.Keys.ToArray();
        return this.FindTypesWithinRadius(allTypes, boid.RepellantRadius, boid);
    }

    public Dictionary<Boid.TYPE, List<Boid>> FindPredators(Boid boid)
    {
        // Get the predator types that 'boid' avoids
        Boid.TYPE[] predatorTypes = boid.PredatorTypes;
        return this.FindTypesWithinRadius(predatorTypes, boid.PredatorRadius, boid);
    }

    protected abstract Dictionary<Boid.TYPE,List<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Boid boid);
}
