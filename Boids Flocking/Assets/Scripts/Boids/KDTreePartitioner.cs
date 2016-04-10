using UnityEngine;
using System.Collections.Generic;
using System;

public class KDTreePartitioner : BoidsPartitioner
{
    [ReadOnly] [SerializeField] private Dictionary<Type,BoidsKDTree> Boids;

    /// <summary>Performs a check based on K-Dimensional trees for boids neighbouring <paramref name="boid"/> within <paramref name="radius"/>.</summary>
    /// <param name="boid">The boid to use as origin of the radius.</param>
    /// <returns>A dictionary keyed by boid-type of the boids within the radius.</returns>
    protected override Dictionary<Boid.TYPE, List<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Vector3 center)
    {
        throw new NotImplementedException();
    }

    public override List<BoidsTarget> FindTargetsNearBoid(Boid boid)
    {
        throw new NotImplementedException();
    }

    // void FixedUpdate()
}
