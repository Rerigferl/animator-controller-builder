
namespace Numeira.Animation;

internal sealed class AnimatorControllerLayerBuilder
{
    public AnimatorControllerLayerBuilder()
    {
    }

    public AnimatorControllerLayerBuilder(string name) : this()
    {
        Name = name;
        StateMachine.Name = name;
    }

    public string Name { get; set; } = "";

    public StateMachineBuilder StateMachine { get; set; } = new();

    public float Weight { get; set; } = 1;

    public AnimatorControllerLayer ToAnimatorControllerLayer(IAssetContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorControllerLayer? layer))
        {
            layer = new AnimatorControllerLayer();
            container.Register(this, layer);
            layer.name = Name;
            layer.defaultWeight = Weight;
            layer.stateMachine = StateMachine.ToAnimatorStateMachine(container);
        }
        return layer;
    }
}