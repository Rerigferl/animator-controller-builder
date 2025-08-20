namespace Numeira.AnimatorController;

internal sealed class TransitionBuilder
{
    public TransitionBuilder(bool isExitTransition = false)
    {
        IsExitTransition = isExitTransition;
    }

    private bool IsExitTransition { get; set; }

    public StateBuilder Destination { get; set; }
    public float? ExitTime { get; set; }
    public float Duration { get; set; }
    public bool FixedDuration { get; set; } = true;

    public List<AnimatorCondition> Conditions { get; } = new();

    public TransitionBuilder If(string parameter) => AddCondition(AnimatorConditionMode.If, parameter, 0);
    public TransitionBuilder IfNot(string parameter) => AddCondition(AnimatorConditionMode.IfNot, parameter, 0);
    public TransitionBuilder Equals(string parameter, float value) => AddCondition(AnimatorConditionMode.Equals, parameter, value);
    public TransitionBuilder NotEqual(string parameter, float value) => AddCondition(AnimatorConditionMode.NotEqual, parameter, value);
    public TransitionBuilder Greater(string parameter, float value) => AddCondition(AnimatorConditionMode.Greater, parameter, value);
    public TransitionBuilder Less(string parameter, float value) => AddCondition(AnimatorConditionMode.Less, parameter, value);

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

    internal AnimatorStateTransition Build(AssetCacheContainer container)
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
