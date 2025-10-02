namespace Numeira.Animation;

public sealed class StateBuilder : IStateMachineItem
{
    public string Name { get; set; } = "";
    public Vector2? Position { get; set; }
    public MotionBuilder? Motion { get; set; }
    public bool WriteDefaults { get; set; }
    public string? MotionTime { get; set; } = null;

    public List<TransitionBuilder> Transitions { get; } = new();

    public List<StateMachineBehaviourBuilder> Behaviours { get; } = new();

    public TransitionBuilder AddTransition(IStateMachineItem destination)
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
        if (!container.TryGetValue(this, out AnimatorState? state))
        {
            state = new AnimatorState();
            container.Register(this, state);
            state.name = Name;
            state.motion = Motion?.ToMotion(container);
            state.timeParameter = MotionTime;
            state.timeParameterActive = MotionTime is not null;
            state.writeDefaultValues = WriteDefaults;
            state.transitions = Transitions.Select(x => x.ToAnimatorStateTransition(container)).ToArray();
            var behaviours = Behaviours.Select(x => x.ToStateMachineBehaviour(container)).ToArray();
            StateMachineBehaviourBuilder.SetBehaviours(state, behaviours);
        }
        return new()
        {
            position = Position ?? new Vector2(200f, 0),
            state = state,
        };
    }
}

public static partial class StateBuilderExt { }