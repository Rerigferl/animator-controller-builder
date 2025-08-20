namespace Numeira.AnimatorController;

internal sealed class StateBuilder
{
    public string Name { get; set; }
    public Vector2? Position { get; set; }
    public Motion Motion { get; set; }
    public bool WriteDefaults { get; set; }

    public List<TransitionBuilder> Transitions = new();
    public List<StateMachineBehaviourBuilder> Behaviours = new();

    public TransitionBuilder AddTransition(StateBuilder destination)
    {
        var t = new TransitionBuilder()
        {
            Destination = destination
        };
        Transitions.Add(t);
        return t;
    }
    public TransitionBuilder AddExitTransition()
    {
        var t = new TransitionBuilder(true);
        Transitions.Add(t);
        return t;
    }

    public AvatarParameterDriverBuilder AddAvatarParameterDriver()
    {
        var b = new AvatarParameterDriverBuilder();
        Behaviours.Add(b);
        return b;
    }

    public ChildAnimatorState Build(AssetCacheContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorState state))
        {
            state = new AnimatorState();
            container.Register(this, state);
            state.name = Name;
            state.motion = Motion;
            state.writeDefaultValues = WriteDefaults;
            state.transitions = Transitions.Select(x => x.Build(container)).ToArray();
            var behaviours = Behaviours.Select(x => x.Build(container)).ToArray();
            StateMachineBehaviourBuilder.SetBehaviours(state, behaviours);
        }
        return new()
        {
            position = Position ?? Vector3.zero,
            state = state,
        };
    }
}
