namespace Numeira.Animation;

internal sealed class StateMachineBuilder : IStateMachineItem
{
    public StateMachineBuilder() { }
    public StateMachineBuilder(StateMachineBuilder parent) : this()
    {
        Parent = parent;
    }

    public StateMachineBuilder? Parent { get; }

    public List<StateBuilder> States { get; } = new();

    public List<StateMachineBuilder> StateMachines => _stateMachines ??= new();
    private List<StateMachineBuilder>? _stateMachines;

    public List<TransitionBuilder> AnyStateTransitions => _anyStateTransitions ??= new();
    private List<TransitionBuilder>? _anyStateTransitions;

    public List<TransitionBuilder> EntryTransitions => _entryTransitions ??= new();
    private List<TransitionBuilder>? _entryTransitions;

    public List<TransitionBuilder> StateMachineTransitions => _stateMachineTransitions ??= new();
    private List<TransitionBuilder>? _stateMachineTransitions;

    public string Name { get; set; } = "";
    public bool DefaultWriteDefaults { get; set; } = true;
    public MotionBuilder? DefaultMotion { get; set; }

    public Vector2 EntryPosition { get; set; } = new(50, 120);
    public Vector2 AnyStatePosition { get; set; } = new(50, 20);
    public Vector2 ExitPosition { get; set; } = new(800, 120);
    public Vector2? Position { get; set; }

    public StateMachineBuilder WithDefaultWriteDefaults(bool value)
    {
        DefaultWriteDefaults = value;
        return this;
    }

    public StateMachineBuilder WithDefaultMotion(MotionBuilder motion)
    {
        DefaultMotion = motion;
        return this;
    }

    public StateMachineBuilder WithDefaultMotion(BlendTree motion)
    {
        DefaultMotion = MotionBuilder.FromBlendTree(motion);
        return this;
    }

    public StateMachineBuilder WithDefaultMotion(AnimationClip motion)
    {
        DefaultMotion = MotionBuilder.FromAnimationClip(motion);
        return this;
    }

    public StateBuilder AddState(string name, Vector2? position = null)
    {
        position ??= (States.Count == 0 ? new Vector2(200f, 0) : States[^1].Position + new Vector2(35f, 65f));
        var state = new StateBuilder() { Name = name, Position = position, WriteDefaults = DefaultWriteDefaults, Motion = DefaultMotion };
        States.Add(state);
        return state;
    }

    public StateMachineBuilder AddStateMachine(string name, Vector2? position = null)
    {
        var stateMachine = new StateMachineBuilder(this) { Name = name, Position = position };
        StateMachines.Add(stateMachine);
        return stateMachine;
    }

    public TransitionBuilder AddAnyStateTransition(IStateMachineItem destination)
    {
        var t = new TransitionBuilder()
        {
            Destination = destination
        };
        AnyStateTransitions.Add(t);
        return t;
    }

    public TransitionBuilder AddEntryTransition(IStateMachineItem destination)
    {
        var t = new TransitionBuilder()
        {
            Destination = destination
        };
        EntryTransitions.Add(t);
        return t;
    }

    public TransitionBuilder AddOutgoingTransition(IStateMachineItem destination)
    {
        var t = new TransitionBuilder()
        {
            Destination = destination
        };
        StateMachineTransitions.Add(t);
        return t;
    }

    public TransitionBuilder AddExitTransition()
    {
        var t = new TransitionBuilder(true);
        StateMachineTransitions.Add(t);
        return t;
    }

    public AnimatorStateMachine ToAnimatorStateMachine(IAssetContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorStateMachine? stateMachine))
        {
            stateMachine = new AnimatorStateMachine();
            container.Register(this, stateMachine);
            stateMachine.name = Name;
            stateMachine.entryPosition = EntryPosition;
            stateMachine.exitPosition = ExitPosition;

            if (States.Count != 0)
                stateMachine.states = States.Select(x => x.ToAnimatorState(container)).ToArray();
            if ((_stateMachines?.Count ?? 0) != 0)
                stateMachine.stateMachines = StateMachines.Select(x => x.ToChildAnimatorStateMachine(container)).ToArray();
            if ((_entryTransitions?.Count ?? 0) != 0)
                stateMachine.entryTransitions = EntryTransitions.Select(x => x.ToAnimatorTransition(container)).ToArray();
            if ((_anyStateTransitions?.Count ?? 0) != 0)
                stateMachine.anyStateTransitions = AnyStateTransitions.Select(x => x.ToAnimatorStateTransition(container)).ToArray();

            if (_stateMachines is {} stateMachines)
            {
                foreach (var x in stateMachines)
                {
                    if (x._stateMachineTransitions is { } trs)
                    {
                        stateMachine.SetStateMachineTransitions(x.ToAnimatorStateMachine(container), trs.Select(x => x.ToAnimatorTransition(container)).ToArray());
                    }
                }
            }
        }
        return stateMachine;
    }

    public ChildAnimatorStateMachine ToChildAnimatorStateMachine(IAssetContainer container)
    {
        return new ChildAnimatorStateMachine()
        {
            stateMachine = ToAnimatorStateMachine(container),
            position = Position ?? Vector2.zero,
        };
    }

}
