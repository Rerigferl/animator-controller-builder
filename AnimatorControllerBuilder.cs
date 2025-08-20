namespace Numeira.Animation;

internal sealed class AnimatorControllerBuilder
{
    public string Name { get; set; } = "";

    public List<AnimatorControllerLayerBuilder> Layers { get; } = new();

    public HashSet<AnimatorControllerParameter> Parameters { get; } = new(new NameEqualityComparer());

    public AnimatorControllerLayerBuilder AddLayer(string name)
    {
        var layer = new AnimatorControllerLayerBuilder();
        Layers.Add(layer);
        layer.Name = name;
        return layer;
    }

    public AnimatorControllerBuilder AddParameter(string name, AnimatorControllerParameterType type)
    {
        Parameters.Add(new() { name = name, type = type });
        return this;
    }

    public AnimatorController ToAnimatorController(IAssetContainer container)
    {
        if (container.TryGetValue(this, out AnimatorController controller))
        {
            return controller;
        }
        controller = new AnimatorController();
        container.Register(this, controller);
        controller.name = Name;
        controller.parameters = Parameters.ToArray();
        controller.layers = Layers.Select(layer => layer.Build(container)).ToArray();
        return controller;
    }

    private sealed class NameEqualityComparer : IEqualityComparer<AnimatorControllerParameter>
    {
        public bool Equals(AnimatorControllerParameter x, AnimatorControllerParameter y)
            => x.name.Equals(y.name, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode(AnimatorControllerParameter obj)
            => obj.name.GetHashCode(StringComparison.OrdinalIgnoreCase);
    }
}

internal static partial class AnimatorControllerBuilderExt
{
    private static HashSet<AnimatorControllerParameter> AddParameter(this HashSet<AnimatorControllerParameter> parameters, AnimatorControllerParameter parameter)
    {
        parameters.Add(parameter);
        return parameters;
    }

    public static HashSet<AnimatorControllerParameter> AddInt(this HashSet<AnimatorControllerParameter> parameters, string name, int defaultValue = 0) 
        => parameters.AddParameter(new() { name = name, type = AnimatorControllerParameterType.Int, defaultInt = defaultValue });

    public static HashSet<AnimatorControllerParameter> AddFloat(this HashSet<AnimatorControllerParameter> parameters, string name, float defaultValue = 0)
        => parameters.AddParameter(new() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });

    public static HashSet<AnimatorControllerParameter> AddBool(this HashSet<AnimatorControllerParameter> parameters, string name, bool defaultValue = false)
        => parameters.AddParameter(new() { name = name, type = AnimatorControllerParameterType.Bool, defaultBool = defaultValue });
}