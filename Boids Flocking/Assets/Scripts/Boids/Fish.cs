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

    protected override Vector3 CalculateAlignment(Dictionary<TYPE, List<Boid>> neighbours)
    {
        throw new NotImplementedException();
    }

    protected override Vector3 CalculateCohesion(Dictionary<TYPE, List<Boid>> neighbours)
    {
        throw new NotImplementedException();
    }

    protected override Vector3 CalculateSeparation(Dictionary<TYPE, List<Boid>> repellants)
    {
        throw new NotImplementedException();
    }

    protected override Vector3 CalculatePredators(Dictionary<TYPE, List<Boid>> predators)
    {
        throw new NotImplementedException();
    }
}
