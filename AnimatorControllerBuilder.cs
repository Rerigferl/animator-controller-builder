namespace Numeira.AnimatorController;

internal sealed class AnimatorControllerBuilder
{
    public string Name { get; set; }

    public List<AnimatorControllerLayerBuilder> Layers { get; } = new();

    public AnimatorControllerParameters Parameters { get; } = new(new(new AnimatorControllerParameters.NameEqualityComparer()));

    public AnimatorControllerLayerBuilder AddLayer(string name)
    {
        var layer = new AnimatorControllerLayerBuilder();
        Layers.Add(layer);
        layer.Name = name;
        return layer;
    }

    public void AddParameter(string name, AnimatorControllerParameterType type)
    {
        Parameters.AddParameter(new() { name = name, type = type });
    }

    public AnimatorController Build(AssetCacheContainer container)
    {
        if (!container.TryGetValue(this, out AnimatorController controller))
        {
            controller = new AnimatorController();
            container.Register(this, controller);
            controller.name = Name;
            controller.parameters = Parameters.Items.ToArray();
            controller.layers = Layers.Select(layer => layer.Build(container)).ToArray();
        }
        return controller;
    }

    public readonly struct AnimatorControllerParameters
    {
        public AnimatorControllerParameters(HashSet<AnimatorControllerParameter> parameters)
        {
            Items = parameters;
        }

        public readonly HashSet<AnimatorControllerParameter> Items { get; }

        public AnimatorControllerParameters Int(string name, int defaultValue)
            => AddParameter(new() { name = name, type = AnimatorControllerParameterType.Int, defaultInt = defaultValue });

        public AnimatorControllerParameters Float(string name, float defaultValue)
            => AddParameter(new() { name = name, type = AnimatorControllerParameterType.Float, defaultFloat = defaultValue });

        public AnimatorControllerParameters Bool(string name, bool defaultValue)
            => AddParameter(new() { name = name, type = AnimatorControllerParameterType.Bool, defaultBool = defaultValue });

        public AnimatorControllerParameters AddParameter(AnimatorControllerParameter parameter)
        {
            Items.Add(parameter);
            return this;
        }

        internal sealed class NameEqualityComparer : IEqualityComparer<AnimatorControllerParameter>
        {
            public bool Equals(AnimatorControllerParameter x, AnimatorControllerParameter y)
                => x.name.Equals(y.name, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode(AnimatorControllerParameter obj)
                => obj.name.GetHashCode();
        }
    }
}
