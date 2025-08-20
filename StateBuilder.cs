namespace Numeira.Animation;

internal sealed class StateBuilder
{
    public string Name { get; set; } = "";
    public Vector2? Position { get; set; }
    public Motion? Motion { get; set; }
    public bool WriteDefaults { get; set; }

    public List<TransitionBuilder> Transitions { get; } = new();

    public List<StateMachineBehaviourBuilder> Behaviours { get; } = new();

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

    public ChildAnimatorState ToAnimatorState(IAssetContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorState state))
        {
            state = new AnimatorState();
            container.Register(this, state);
            state.name = Name;
            state.motion = Motion;
            state.writeDefaultValues = WriteDefaults;
            state.transitions = Transitions.Select(x => x.ToAnimatorStateTransition(container)).ToArray();
            var behaviours = Behaviours.Select(x => x.ToStateMachineBehaviour(container)).ToArray();
            StateMachineBehaviourBuilder.SetBehaviours(state, behaviours);
        }
        return new()
        {
            position = Position ?? Vector3.zero,
            state = state,
        };
    }
}

internal static partial class StateBuilderExt { }