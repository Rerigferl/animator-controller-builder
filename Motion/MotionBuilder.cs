using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Numeira.Animation;

internal abstract class MotionBuilder
{
    public virtual string Name { get; set; } = "";

    public string? DefaultDirectBlendParameter { get; set; } = null;
    public float? DefaultThreshold { get; set; } = null;
    public Vector2? DefaultPosition { get; set; } = null;

    public abstract Motion ToMotion(IAssetContainer container);

    public static MotionBuilder FromBlendTree(BlendTree blendTree) => new UnityMotion(blendTree);
    public static MotionBuilder FromAnimationClip(AnimationClip animationClip) => new UnityMotion(animationClip);
    public static implicit operator MotionBuilder(AnimationClip animationClip) => new UnityMotion(animationClip);
    public static implicit operator MotionBuilder(BlendTree blendTree) => new UnityMotion(blendTree);

    private sealed class UnityMotion : MotionBuilder
    {
        public UnityMotion(Motion motion)
        {
            Motion = motion;
        }
        public override string Name => Motion == null ? "(null)" : Motion.name;

        public Motion? Motion { get; }

        public override Motion ToMotion(IAssetContainer container)
        {
            if (Motion == null)
                return null!;
            if (container.TryGetValue(this, out Motion? motion))
                return motion;
            container.Register(this, Motion);
            return Motion;
        }
    }
}

internal abstract class MotionBuilder<T> : MotionBuilder where T : Motion
{
    protected abstract T CreateInstance();

    public override Motion ToMotion(IAssetContainer container)
    {
        if (container.TryGetValue(this, out Motion? motion))
            return motion;

        var result = CreateInstance();
        container.Register(this, result);

        result.name = Name;
        ConfigureMotion(result, container);
        return result;
    }

    protected abstract void ConfigureMotion(T value, IAssetContainer container);
}

internal abstract class ChildMotionBuilder
{
    internal abstract MotionBuilder GetMotion();

    public float? Threshold { get; set; }
    public Vector2? Position { get; set; }
    public string? DirectBlendParameter { get; set; }
}

internal sealed class ChildMotionBuilder<T> : ChildMotionBuilder where T : MotionBuilder
{
    public ChildMotionBuilder(T motion)
    {
        Motion = motion;
    }

    public T Motion { get; }

    internal override MotionBuilder GetMotion() => Motion;

    public static implicit operator T(ChildMotionBuilder<T> builder) => builder.Motion;
}

internal static partial class MotionBuilderExt
{
    public static T WithName<T>(this T motion, string name) where T : MotionBuilder
    {
        motion.Name = name;
        return motion;
    }

    public static T WithDefaultDirectBlendParameter<T>(this T blendTree, string value) where T : BlendTreeBuilder
    {
        blendTree.DefaultDirectBlendParameter = value;
        return blendTree;
    }

    public static T WithDefaultThreshold<T>(this T motion, float value) where T : MotionBuilder
    {
        motion.DefaultThreshold = value;
        return motion;
    }

    public static T WithDefaultPosition<T>(this T motion, float x, float y) where T : MotionBuilder
    {
        motion.DefaultPosition = new(x, y);
        return motion;
    }

    public static T WithThreshold<T>(this T childMotion, float threshold) where T : ChildMotionBuilder
    {
        childMotion.Threshold = threshold;
        return childMotion;
    }

    public static T WithPosition<T>(this T childMotion, Vector2 position) where T : ChildMotionBuilder
    {
        childMotion.Position = position;
        return childMotion;
    }

    public static T WithDirectBlendParameter<T>(this T childMotion, string value) where T : ChildMotionBuilder
    {
        childMotion.DirectBlendParameter = value;
        return childMotion;
    }
}