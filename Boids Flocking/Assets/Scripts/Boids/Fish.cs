using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Fish : Boid
{
    [Header("Types")]
    [SerializeField] private TYPE _type = TYPE.FISH;
    public override TYPE Type { get { return this._type; } }
    [SerializeField] private TYPE[] _neighbourTypes = new TYPE[] { TYPE.FISH };
    public override TYPE[] NeighbourTypes {
        get { return this._neighbourTypes; }
    }
    [SerializeField] private TYPE[] _predatorTypes = new TYPE[] { TYPE.SHARK };
    public override TYPE[] PredatorTypes {
        get { return this._predatorTypes; }
    }

    // Radii
    public override float NeighbourRadius {
        get { return this.BoidsManager.FishNeighbourRadius; }
    }
    public override float RepellantRadius {
        get { return this.BoidsManager.FishRepellantRadius; }
    }
    public override float PredatorRadius {
        get { return this.BoidsManager.FishPredatorRadius; }
    }
    public override float CohesiveSwitchDistance {
        get { return this.BoidsManager.FishCohesiveSwitchDistance; }
    }

    // Speeds
    protected override float MaxAcceleration {
        get { return this.BoidsManager.FishMaxAcceleration; }
    }
    protected override float MaxSpeed {
        get { return this.BoidsManager.FishMaxSpeed; }
    }
    protected override float MinSpeed {
        get { return this.BoidsManager.FishMinSpeed; }
    }

    // Weights
    protected override float CohesionWeight {
        get { return this.BoidsManager.FishCohesionWeight; }
    }
    protected override float SeparationWeight {
        get { return this.BoidsManager.FishSeparationWeight; }
    }
    protected override float AlignmentWeight {
        get { return this.BoidsManager.FishAlignmentWeight; }
    }
    protected override float PredatorWeight {
        get { return this.BoidsManager.FishPredatorWeight; }
    }
}
