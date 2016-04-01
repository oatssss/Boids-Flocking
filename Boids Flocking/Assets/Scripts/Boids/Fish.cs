using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Fish : Boid
{
    [SerializeField] private TYPE type = TYPE.FISH;
    public override TYPE Type { get { return this.type; } }

    [SerializeField] private TYPE[] neighbourTypes = new TYPE[] { TYPE.FISH };
    public override TYPE[] NeighbourTypes {
        get { return this.neighbourTypes; }
    }

    [SerializeField] private TYPE[] predatorTypes = new TYPE[] { TYPE.SHARK };
    public override TYPE[] PredatorTypes {
        get { return this.predatorTypes; }
    }

    protected override Vector3 CalculateCohesion(Dictionary<TYPE, List<Boid>> neighbours)
    {
        Vector3 centerOfMass = Vector3.zero;
        int neighbourCount = 0;
        // Iterate over all neighbour types
        foreach (List<Boid> typeSection in neighbours.Values)
        {
            // Iterate over all neighbours of that type
            foreach (Boid neighbour in typeSection)
            {
                centerOfMass += neighbour.transform.position;
                neighbourCount++;
            }
        }
        centerOfMass /= neighbourCount;

        // Return a vector from this boid's position to the center of mass
        return centerOfMass - this.transform.position;
    }

    protected override Vector3 CalculateAlignment(Dictionary<TYPE, List<Boid>> neighbours)
    {
        Vector3 averageForward = Vector3.zero;
        int neighbourCount = 0;
        foreach(List<Boid> typeSection in neighbours.Values)
        {
            foreach (Boid neighbour in typeSection)
            {
                averageForward += neighbour.transform.forward;
                neighbourCount++;
            }
        }
        averageForward /= neighbourCount;

        return averageForward;
    }

    protected override Vector3 CalculateSeparation(Dictionary<TYPE, List<Boid>> repellants)
    {
        Vector3 separation = Vector3.zero;
        int repellantCount = 0;
        foreach (List<Boid> typeSection in repellants.Values)
        {
            foreach (Boid repellant in typeSection)
            {
                Vector3 toSelf = (this.transform.position - repellant.transform.position);
                // We want boids farther away to have less separation effect
                separation += toSelf * ((this.RepellantRadius/toSelf.magnitude) - 1);
                repellantCount++;
            }
        }
        separation /= repellantCount;

        return separation;
    }

    protected override Vector3 CalculatePredators(Dictionary<TYPE, List<Boid>> predators)
    {
        throw new NotImplementedException();
    }
}
