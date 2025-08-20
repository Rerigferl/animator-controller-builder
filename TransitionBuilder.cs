namespace Numeira.Animation;

internal sealed class TransitionBuilder
{
    public TransitionBuilder(bool isExitTransition = false)
    {
        IsExitTransition = isExitTransition;
    }

    private bool IsExitTransition { get; set; }

    public StateBuilder? Destination { get; set; }

    public float? ExitTime { get; set; }

    public float Duration { get; set; }

    public bool FixedDuration { get; set; } = true;

    public List<AnimatorCondition> Conditions { get; } = new();

    public TransitionBuilder AddCondition(AnimatorConditionMode mode, string parameter, float threshold)
    {
        var c = new AnimatorCondition() { mode = mode, parameter = parameter, threshold = threshold }; ;
        Conditions.Add(c);
        return this;
    }

    public TransitionBuilder WithDuration(float value)
    {
        Duration = value;
        return this;
    }

    public TransitionBuilder WithExitTime(float value)
    {
        ExitTime = value;
        return this;
    }

    internal AnimatorStateTransition Build(IAssetContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorStateTransition tr))
        {
            tr = new AnimatorStateTransition();
            container.Register(this, tr);
            tr.canTransitionToSelf = false;
            tr.destinationState = Destination?.Build(container).state;
            tr.duration = Duration;
            tr.hasExitTime = ExitTime.HasValue;
            tr.exitTime = ExitTime ?? 0;
            tr.hasFixedDuration = FixedDuration;
            tr.conditions = Conditions.ToArray();
            tr.isExit = IsExitTransition;
        }
        return tr;
    }
}

internal static partial class TransitionBuilderExt
{
    public static TransitionBuilder If(this TransitionBuilder transition, string parameter) => transition.AddCondition(AnimatorConditionMode.If, parameter, 0);

    public static TransitionBuilder IfNot(this TransitionBuilder transition, string parameter) => transition.AddCondition(AnimatorConditionMode.IfNot, parameter, 0);

    public static TransitionBuilder Equals(this TransitionBuilder transition, string parameter, float value) => transition.AddCondition(AnimatorConditionMode.Equals, parameter, value);

    public static TransitionBuilder NotEqual(this TransitionBuilder transition, string parameter, float value) => transition.AddCondition(AnimatorConditionMode.NotEqual, parameter, value);

    public static TransitionBuilder Greater(this TransitionBuilder transition, string parameter, float value) => transition.AddCondition(AnimatorConditionMode.Greater, parameter, value);

    public static TransitionBuilder Less(this TransitionBuilder transition, string parameter, float value) => transition.AddCondition(AnimatorConditionMode.Less, parameter, value);
}