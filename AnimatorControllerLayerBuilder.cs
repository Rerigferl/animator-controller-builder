namespace Numeira.Animation;

internal sealed class AnimatorControllerLayerBuilder
{
    public string Name { get; set; } = "";

    public StateMachineBuilder StateMachine { get; set; } = new();

    public float Weight { get; set; } = 1;

    public AnimatorControllerLayer Build(IAssetContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorControllerLayer layer))
        {
            layer = new AnimatorControllerLayer();
            container.Register(this, layer);
            layer.name = Name;
            layer.defaultWeight = Weight;
            layer.stateMachine = StateMachine.Build(container);
        }
        return layer;
    }
}
