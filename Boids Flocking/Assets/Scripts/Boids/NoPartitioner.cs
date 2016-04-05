using UnityEngine;
using System;
using System.Collections.Generic;

public class NoPartitioner : BoidsPartitioner
{
    /// <summary>Performs an n-squared check for boids neighbouring <paramref name="boid"/> within <paramref name="radius"/>.</summary>
    /// <param name="boid">The boid to use as origin of the radius.</param>
    /// <returns>A dictionary keyed by boid-type of the boids within the radius.</returns>
    protected override Dictionary<Boid.TYPE,List<Boid>> FindTypesWithinRadius(Boid.TYPE[] types, float radius, Vector3 center)
    {
        // A dictionary that will hold the boids from each type within the radius
        Dictionary<Boid.TYPE,List<Boid>> listsByType = new Dictionary<Boid.TYPE,List<Boid>>();
        foreach (Boid.TYPE type in types)
        {
            // If no boids exist in the scene for this type, skip
            if (!this.BoidsManager.AllBoids.ContainsKey(type))
                { continue; }

            // Otherwise, get all the boids in the scene of this type
            List<Boid> boids = this.BoidsManager.AllBoids[type];

            List<Boid> withinRange = new List<Boid>();   // Need a list to contain the boids that are found within radius
            foreach (Boid potential in boids)
            {
                // Is the boid within radius? Then add to list
                if ((potential.transform.position - center).magnitude < radius)
                    { withinRange.Add(potential); }
            }

            // Add the found neighbours of this type to the dictionary
            if (withinRange.Count > 0)
                { listsByType.Add(type, withinRange); }
        }

        return listsByType;
    }

    public override List<BoidsTarget> FindTargetsNearBoid(Boid boid)
    {
        List<BoidsTarget> targets = new List<BoidsTarget>();
        foreach (ProximityTarget target in this.BoidsManager.AllProximityTargets)
        {
            float sqrDistance = (target.transform.position - boid.transform.position).sqrMagnitude;
            float sqrRadius = target.Radius * target.Radius;

            // Check if the target even attracts this type of boid
            bool attracts = false;
            foreach (Boid.TYPE type in target.AttractedTypes)
            {
                if (boid.Type == type)
                    { attracts = true; break; }
            }

            if (attracts && sqrDistance < sqrRadius)
                { targets.Add(target); }
        }

        return targets;
    }
}
