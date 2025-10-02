using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Numeira.Animation;

public abstract class BlendTreeBuilder : MotionBuilder<BlendTree>
{
    public abstract BlendTreeType Type { get; }

    public List<ChildMotionBuilder> Children => children ??= new();
    private List<ChildMotionBuilder>? children = null;
    public int Count => children?.Count ?? 0;

    public ChildMotionBuilder<T> Append<T>(T item, float? threshold = null, Vector2? position = null, string? directBlendParameter = null) where T : MotionBuilder
    {
        var child = new ChildMotionBuilder<T>(item)
        {
            Threshold = threshold ?? item.DefaultThreshold,
            Position = position ?? item.DefaultPosition,
            DirectBlendParameter = directBlendParameter ?? item.DefaultDirectBlendParameter,
        };

        item.DefaultDirectBlendParameter ??= DefaultDirectBlendParameter;
        item.DefaultThreshold ??= DefaultThreshold;
        item.DefaultPosition ??= DefaultPosition;

        Children.Add(child);
        return child;
    }

    protected override BlendTree CreateInstance() => new();

    protected override void ConfigureMotion(BlendTree value, IAssetContainer container)
    {
        value.blendType = Type;
        value.useAutomaticThresholds = false;
        ConfigureBlendTree(value, container);

        if (Count == 0)
            return;

        Children.Sort((x, y) => (x.Threshold ?? 0f).CompareTo(y.Threshold ?? 0));
        var span = AsSpan(Children);
        var children = new ChildMotion[span.Length];
        for (int i = 0; i < children.Length; i++)
        {
            var x = span[i];
            children[i] = new()
            {
                directBlendParameter = x.DirectBlendParameter ?? DefaultDirectBlendParameter ?? "",
                threshold = x.Threshold ?? DefaultThreshold ?? (Count < 2 ? 0 : (i / (Count - 1))),
                position = x.Position ?? DefaultPosition ?? Vector2.zero,
                motion = x.Motion.ToMotion(container)
            };
        }

        value.children = children;
    }

    protected abstract void ConfigureBlendTree(BlendTree value, IAssetContainer container);

    public override string ToString()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        using var sw = new IndentedTextWriter(writer, "  ");
        ToString(sw);
        return sb.ToString();
    }

    private void ToString(IndentedTextWriter writer)
    {
        writer.WriteLine($"{Name} ({GetType().Name})");
        writer.Indent++;
        foreach (var item in Children)
        {
            var motion = item.Motion;
            if (motion is BlendTreeBuilder b)
                b.ToString(writer);
            else
                writer.WriteLine($"{motion.Name} ({motion.GetType().Name})");
        }
        writer.Indent--;
    }

    protected static Span<T> AsSpan<T>(List<T> list) => Unsafe.As<Tuple<T[], int>>(list) is { } tuple ? tuple.Item1.AsSpan(0, tuple.Item2) : default;
    protected static Span<(int HashCode, int Next, T Value)> AsSpan<T>(HashSet<T> hashSet) => Unsafe.As<Tuple<int[], ValueTuple<int, int, T>[]>>(hashSet) is { } tuple ? tuple.Item2.AsSpan() : default;
}

public sealed class OneDirectionBlendTreeBuilder : BlendTreeBuilder
{
    public override BlendTreeType Type => BlendTreeType.Simple1D;

    public string BlendParameter { get; set; } = "";

    protected override void ConfigureBlendTree(BlendTree value, IAssetContainer container)
    {
        value.blendParameter = BlendParameter;
    }
}

public sealed class TwoDirectionBlendTreeBuilder : BlendTreeBuilder
{
    public override BlendTreeType Type => (IsFreeform, IsCertein) switch
    {
        (true, true) => BlendTreeType.FreeformCartesian2D,
        (true, false) => BlendTreeType.FreeformDirectional2D,
        _ => BlendTreeType.SimpleDirectional2D,
    };

    public bool IsFreeform { get; set; } = false;

    public bool IsCertein { get; set; } = false;

    public string BlendParameter { get; set; } = "";

    protected override void ConfigureBlendTree(BlendTree value, IAssetContainer container)
    {
        value.blendParameter = BlendParameter;
    }
}

public sealed partial class DirectBlendTreeBuilder : BlendTreeBuilder
{
    public override BlendTreeType Type => BlendTreeType.Direct;

    public bool NormalizedBlendValues { get; set; } = false;

    protected override void ConfigureBlendTree(BlendTree value, IAssetContainer container)
    {
        if (NormalizedBlendValues)
            SetNormalizedBlendValues(value, NormalizedBlendValues);
    }

    private static void SetNormalizedBlendValues(BlendTree blendTree, bool value)
    {
#if !DISABLE_UNSAFE_CODE
        if (normalizedBlendValuesOffset is { } offset)
        {
            var pointer = Unsafe.As<Tuple<IntPtr, int>>(blendTree).Item1;
            unsafe
            {
                *(bool*)((byte*)pointer.ToPointer() + offset) = value;
            }
        }
#else
        using var so = new SerializedObject(blendTree);
        so.FindProperty("m_NormalizedBlendValues").boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
#endif
    }

    private static readonly long? normalizedBlendValuesOffset = FindNormalizedBlendValuesOffset();

    private unsafe static long? FindNormalizedBlendValuesOffset()
    {
        var blendTree = new BlendTree();
        try
        {
            var threshold = float.Epsilon;
            blendTree.maxThreshold = float.Epsilon;
            var pointer = Unsafe.As<Tuple<IntPtr, int>>(blendTree).Item1;
            var p = (byte*)pointer.ToPointer();
            var s = p;

            int limit = 512;
            while (limit > 0)
            {
                if (*(float*)p == threshold)
                    return (p - s) + sizeof(float) + sizeof(bool);
                p++;
                limit--;
            }
            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            Object.DestroyImmediate(blendTree);
        }
    }
}

static partial class MotionBuilderExt
{
    private static ChildMotionBuilder<R> AddMotion<T, R>(this T blendTree, R item) where T : BlendTreeBuilder where R : MotionBuilder
    {
        item.DefaultDirectBlendParameter ??= blendTree.DefaultDirectBlendParameter;
        item.DefaultThreshold ??= blendTree.DefaultThreshold;
        item.DefaultPosition ??= blendTree.DefaultPosition;
        return blendTree.Append(item);
    }

    public static ChildMotionBuilder AddMotion<T>(this T blendTree, AnimationClip motion) where T : BlendTreeBuilder
        => blendTree.AddMotion(MotionBuilder.FromAnimationClip(motion));

    public static ChildMotionBuilder<OneDirectionBlendTreeBuilder> AddBlendTree<T>(this T blendTree, string? name = null) where T : BlendTreeBuilder
        => blendTree.AddMotion(new OneDirectionBlendTreeBuilder() { Name = name ?? "" });

    public static ChildMotionBuilder<DirectBlendTreeBuilder> AddDirectBlendTree<T>(this T blendTree, string? name = null) where T : BlendTreeBuilder
        => blendTree.AddMotion(new DirectBlendTreeBuilder() { Name = name ?? "" });

    public static ChildMotionBuilder<AnimationClipBuilder> AddAnimationClip<T>(this T blendTree, string? name = null) where T : BlendTreeBuilder
        => blendTree.AddMotion(new AnimationClipBuilder() { Name = name ?? "" });
}
