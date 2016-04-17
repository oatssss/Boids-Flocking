using UnityEngine;

public class Shark : Boid
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
    [SerializeField] private TYPE _type = TYPE.SHARK;
    public override TYPE Type { get { return this._type; } }
    [SerializeField] private TYPE[] _neighbourTypes = new TYPE[] { TYPE.SHARK };
    public override TYPE[] NeighbourTypes {
        get { return this._neighbourTypes; }
    }
    [SerializeField] private TYPE[] _preyTypes = new TYPE[] { TYPE.FISH };
    public override TYPE[] PreyTypes {
        get { return this._preyTypes; }
    }
    [SerializeField] private TYPE[] _predatorTypes = new TYPE[0];
    public override TYPE[] PredatorTypes {
        get { return this._predatorTypes; }
    }
    [SerializeField] private TYPE[] _fleeTypes = new TYPE[0];
    public override TYPE[] FleeTypes {
        get { return this._fleeTypes; }
    }
    [SerializeField] private bool _flocks = false;
    protected override bool Flocks { get { return this._flocks; } }

    // Radii
    public override int MaxFlockSize {
        get { return this.BoidsManager.MaxSharkFlockSize; }
    }
    public override float NeighbourRadius {
        get { return this.BoidsManager.SharkNeighbourRadius; }
    }
    public override float RepellantRadius {
        get { return this.BoidsManager.SharkRepellantRadius; }
    }
    public override float PreyRadius {
        get { return this.NeighbourRadius; }
    }
    public override float PredatorRadius {
        get { return this.BoidsManager.SharkPredatorRadius; }
    }
    public override float FleeRadius {
        get { return this.BoidsManager.SharkFleeRadius; }
    }
    public override float CohesiveSwitchDistance {
        get { return this.BoidsManager.SharkCohesiveSwitchDistance; }
    }

    // Speeds
    protected override float MaxAcceleration {
        get { return this.BoidsManager.SharkMaxAcceleration; }
    }
    protected override float MaxDeceleration {
        get { return this.BoidsManager.SharkMaxDeceleration; }
    }
    protected override float MaxSpeed {
        get { return this.BoidsManager.SharkMaxSpeed; }
    }
    protected override float MinSpeed {
        get { return this.BoidsManager.SharkMinSpeed; }
    }

    // Weights
    protected override float CohesionWeight {
        get { return this.BoidsManager.SharkCohesionWeight; }
    }
    protected override float SeparationWeight {
        get { return this.BoidsManager.SharkSeparationWeight; }
    }
    protected override float AlignmentWeight {
        get { return this.BoidsManager.SharkAlignmentWeight; }
    }
    protected override float PredatorWeight {
        get { return this.BoidsManager.SharkPredatorWeight; }
    }
    protected override float GoalWeight {
        get { return this.BoidsManager.SharkGoalWeight; }
    }
    protected override float BoundaryWeight {
        get { return this.BoidsManager.SharkBoundaryWeight; }
    }
}
