using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class SpatialHashPartitioner : BoidsPartitioner
{
    /// <summary>Performs a check based on spacial hashing for boids neighbouring <paramref name="boid"/> within <paramref name="radius"/>.</summary>
    /// <param name="boid">The boid to use as origin of the radius.</param>
    /// <returns>A dictionary keyed by boid-type of the boids within the radius.</returns>
    protected override Dictionary<Boid.TYPE, List<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Boid boid)
    {
        throw new NotImplementedException();
    }
}
