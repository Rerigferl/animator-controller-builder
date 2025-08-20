namespace Numeira.AnimatorController;

internal sealed class AssetCacheContainer
{
    public Dictionary<object, object> Items { get; } = new();

    public IEnumerable<Object> GeneratedAssets => Items.Values.Select(x => x as Object).Where(x => x != null);

    public void Register(object key, object value)
        => Items.TryAdd(key, value);

    public bool TryGetValue<T>(object key, out T value) where T : class
    {
        if (Items.TryGetValue(key, out var v) && v is T val)
        {
            value = val;
            return true;
        }
        value = default;
        return false;
    }
}
