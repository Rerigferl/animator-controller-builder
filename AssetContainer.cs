using System.Diagnostics.CodeAnalysis;

namespace Numeira.Animation;

internal sealed class AssetContainer : IAssetContainer
{
    public Dictionary<object, object> Items { get; } = new();

    public IEnumerable<Object> Assets => Items.Values.Select(x => (x as Object)!).Where(x => x != null);

    public void Register(object key, object value)
        => Items.TryAdd(key, value);

    public bool TryGetValue<T>(object key, [NotNullWhen(true)] out T? value) where T : class
    {
        if (Items.TryGetValue(key, out var v) && v is T val)
        {
            value = val;
            return true;
        }
        value = default!;
        return false;
    }
}

internal interface IAssetContainer
{
    public void Register(object key, object value);

    public bool TryGetValue<T>(object key, [NotNullWhen(true)] out T? value) where T : class;
}
