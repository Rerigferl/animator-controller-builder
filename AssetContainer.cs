using System.Diagnostics.CodeAnalysis;

namespace Numeira.Animation;

public sealed class AssetContainer : IAssetContainer
{
    public static IAssetContainer Empty { get; } = new NullAssetContainer();

    public static IAssetContainer Current { get; set; } = new AssetContainer();

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

    private sealed class NullAssetContainer : IAssetContainer
    {
        public void Register(object key, object value)
        {
        }

        public bool TryGetValue<T>(object key, [NotNullWhen(true)] out T? value) where T : class
        {
            value = default;
            return false;
        }
    }
}


public interface IAssetContainer
{
    public void Register(object key, object value);

    public bool TryGetValue<T>(object key, [NotNullWhen(true)] out T? value) where T : class;
}
