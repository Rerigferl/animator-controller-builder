namespace Numeira.AnimatorController;

internal sealed class StateMachineBuilder
{
    public List<StateBuilder> States { get; } = new();
    public List<TransitionBuilder> AnyStateTransitions { get; } = new();

    public string Name { get; set; }
    public bool DefaultWriteDefaults { get; set; } = true;
    public Motion DefaultMotion { get; set; }

    public Vector2 EntryPosition { get; set; } = new(50, 120);
    public Vector2 ExitPosition { get; set; } = new(800, 120);

    public StateMachineBuilder WithDefaultWriteDefaults(bool value)
    {
        DefaultWriteDefaults = value;
        return this;
    }

    public StateMachineBuilder WithDefaultMotion(Motion motion)
    {
        DefaultMotion = motion;
        return this;
    }

    public StateBuilder AddState(string name, Vector2 position)
    {
        var state = new StateBuilder() { Name = name, Position = position, WriteDefaults = DefaultWriteDefaults, Motion = DefaultMotion };
        States.Add(state);
        return state;
    }

    public TransitionBuilder AddAnyStateTransition(StateBuilder destination)
    {
        var t = new TransitionBuilder()
        {
            Destination = destination
        };
        AnyStateTransitions.Add(t);
        return t;
    }

    public AnimatorStateMachine Build(AssetCacheContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorStateMachine stateMachine))
        {
            stateMachine = new AnimatorStateMachine();
            container.Register(this, stateMachine);
            stateMachine.name = Name;
            stateMachine.states = States.Select(x => x.Build(container)).ToArray();
            stateMachine.entryPosition = EntryPosition;
            stateMachine.exitPosition = ExitPosition;
            stateMachine.anyStateTransitions = AnyStateTransitions.Select(x => x.Build(container)).ToArray();
        }
        return stateMachine;
    }

}
