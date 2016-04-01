using UnityEngine;
using System;
using System.Collections.Generic;

public class NoPartitioner : BoidsPartitioner
{
    /// <summary>Performs an n-squared check for boids neighbouring <paramref name="boid"/> within <paramref name="radius"/>.</summary>
    /// <param name="boid">The boid to use as origin of the radius.</param>
    /// <returns>A dictionary keyed by boid-type of the boids within the radius.</returns>
    protected override Dictionary<Boid.TYPE,List<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Boid boid)
    {
        // A dictionary that will hold the boids from each type within the radius
        Dictionary<Boid.TYPE,List<Boid>> neighbourLists = new Dictionary<Boid.TYPE,List<Boid>>();
        foreach (Boid.TYPE type in types)
        {
            // If no boids exist in the scene for this type, skip
            if (!this.AllBoids.ContainsKey(type))
                { continue; }

            // Otherwise, get all the boids in the scene of this type
            List<Boid> boids = this.AllBoids[type];

            List<Boid> neighbours = new List<Boid>();   // Need a list to contain the boids that are found within radius
            foreach (Boid potential in boids)
            {
                // Is the boid within radius? Then add to list
                if ((potential.transform.position - boid.transform.position).magnitude < radius)
                    { neighbours.Add(potential); }
            }

            // Add the found neighbours of this type to the dictionary
            if (neighbours.Count > 0)
                { neighbourLists.Add(type, neighbours); }
        }

        return neighbourLists;
    }
}
