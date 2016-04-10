using UnityEngine;
using System.Collections.Generic;

public class Fish : Boid
{
    private ITargetSelector _targetSelector = new MostRecentSelector();
    private IPreySelector _preySelector = new ClosestPreySelector();
    private GoalSelector _goalSelectorStrategy;
    protected override GoalSelector GoalSelectorStrategy    { get { return this._goalSelectorStrategy; } }
    protected override IPreySelector PreySelector           { get { return this._preySelector; } }
    protected override ITargetSelector TargetSelector       { get { return this._targetSelector; } }

    protected override void Start()
    {
        this._goalSelectorStrategy = new PreyFirstSelector(this.TargetSelector, this.PreySelector);
        base.Start();
    }

    [Header("Types")]
    [SerializeField] private TYPE _type = TYPE.FISH;
    public override TYPE Type { get { return this._type; } }
    [SerializeField] private TYPE[] _neighbourTypes = new TYPE[] { TYPE.FISH };
    public override TYPE[] NeighbourTypes {
        get { return this._neighbourTypes; }
    }
    [SerializeField] private TYPE[] _preyTypes = new TYPE[0];
    public override TYPE[] PreyTypes {
        get { return this._preyTypes; }
    }
    [SerializeField] private TYPE[] _predatorTypes = new TYPE[] { TYPE.SHARK };
    public override TYPE[] PredatorTypes {
        get { return this._predatorTypes; }
    }
    [SerializeField] private TYPE[] _fleeTypes = new TYPE[] { TYPE.SHARK };
    public override TYPE[] FleeTypes {
        get { return this._fleeTypes; }
    }
    [SerializeField] private bool _flocks = true;
    protected override bool Flocks { get { return this._flocks; } }

    // Radii
    public override int MaxFlockSize {
        get { return this.BoidsManager.MaxFishFlockSize; }
    }
    public override float NeighbourRadius {
        get { return this.BoidsManager.FishNeighbourRadius; }
    }
    public override float RepellantRadius {
        get { return this.BoidsManager.FishRepellantRadius; }
    }
    public override float PreyRadius {
        get { return this.NeighbourRadius; }
    }
    public override float PredatorRadius {
        get { return this.BoidsManager.FishPredatorRadius; }
    }
    public override float FleeRadius {
        get { return this.BoidsManager.FishFleeRadius; }
    }
    public override float CohesiveSwitchDistance {
        get { return this.BoidsManager.FishCohesiveSwitchDistance; }
    }

    // Speeds
    protected override float MaxAcceleration {
        get { return this.BoidsManager.FishMaxAcceleration; }
    }
    protected override float MaxDeceleration {
        get { return this.BoidsManager.FishMaxDeceleration; }
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
    protected override float GoalWeight {
        get { return this.BoidsManager.FishGoalWeight; }
    }
}
