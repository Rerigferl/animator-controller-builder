using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Numeira.Animation;

internal sealed class AnimationClipBuilder : MotionBuilder<AnimationClip>
{
    private static AnimationCurve? SharedAnimationCurve;

    private readonly Dictionary<EditorCurveBinding, SortedList<float, Keyframe>> keyframeMap = new();

    public IEnumerable<EditorCurveBinding> Bindings => keyframeMap.Keys;

    public bool IsLoop { get; set; } = false;

    public float Length => keyframeMap.Count == 0 ? 0 : keyframeMap.Values.Max(x => AsSpan(x)[^1].Time);

    public Span<Keyframe> this[EditorCurveBinding binding] => keyframeMap.TryGetValue(binding, out var keyframe) ? AsSpan(keyframe) : default;

    public AnimationClipBuilder Add(EditorCurveBinding key, float time, float value)
    {
        if (!keyframeMap.TryGetValue(key, out var list))
        {
            list = new();
            keyframeMap.Add(key, list);
        }

        if (list.Count > 0)
        {
            foreach (ref var x in AsSpan(list))
            {
                if (Mathf.Approximately(x.Time, time))
                {
                    x.Value = value;
                    return this;
                }
            }
        }

        list[time] = new(time, value);
        return this;
    }

    public AnimationClipBuilder AddAnimatedParameter(string name, float time, float value)
    {
        Add(new EditorCurveBinding() { path = "", propertyName = name, type = typeof(Animator) }, time, value);
        return this;
    }

    public float? Evaluate(EditorCurveBinding binding, float time)
    {
        if (!keyframeMap.TryGetValue(binding, out var keyframe))
            return default;
        return Evaluate(AsSpan(keyframe), time);
    }

    private static float Evaluate(ReadOnlySpan<Keyframe> sortedKeyframes, float time)
    {
        if (sortedKeyframes.IsEmpty)
            return 0;

        if (sortedKeyframes.Length == 1)
            return sortedKeyframes[0].Value;

        if (sortedKeyframes[0].Time > time)
            return sortedKeyframes[0].Value;

        if (sortedKeyframes[^1].Time < time)
            return sortedKeyframes[^1].Value;

        for (int i = 0; i < sortedKeyframes.Length - 1; i++)
        {
            if (time >= sortedKeyframes[i].Time && time <= sortedKeyframes[i + 1].Time)
            {
                Keyframe leftKey = sortedKeyframes[i];
                Keyframe rightKey = sortedKeyframes[i + 1];

                float t = (time - leftKey.Time) / (rightKey.Time - leftKey.Time);
                return leftKey.Value + (rightKey.Value - leftKey.Value) * t;
            }
        }

        return 0;
    }

    protected override void ConfigureMotion(AnimationClip value, IAssetContainer container)
    {
        SharedAnimationCurve ??= new AnimationCurve();

        HashSet<float> timesSet = new(FloatEqualityComparer.Default);
        var map = keyframeMap;

        foreach (var (binding, keyframes) in map)
        {
            foreach (var keyframe in keyframes)
            {
                timesSet.Add(keyframe.Key);
            }
        }

        var times = timesSet.ToArray();
        var keys = new UnityEngine.Keyframe[times.Length];
        var curve = SharedAnimationCurve;

        foreach (var (binding, keyframes) in map)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = new UnityEngine.Keyframe(times[i], Evaluate(AsSpan(keyframes), times[i]));
            }
            curve.keys = keys;
            SetEditorCurveNoSync(value, binding, curve);
        }

        SyncEditorCurves(value);

        if (IsLoop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(value);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(value, settings);
        }

    }

    private static Span<T> AsSpan<T>(List<T> list)
    {
        var tuple = Unsafe.As<Tuple<T[], int>>(list);
        return tuple.Item1.AsSpan(0, tuple.Item2);
    }

    private static Span<TValue> AsSpan<TKey, TValue>(SortedList<TKey, TValue> list)
    {
        var array = Unsafe.As<Tuple<TKey[], TValue[]>>(list).Item2;
        return array.AsSpan(0, list.Count);
    }

    private static void SetEditorCurveNoSync(AnimationClip clip, EditorCurveBinding binding, AnimationCurve curve)
    {
        _Internal_SetEditorCurve?.Invoke(clip, binding, curve, false);
    }

    private static void SyncEditorCurves(AnimationClip clip)
    {
        _SyncEditorCurves?.Invoke(clip);
    }

    private sealed class FloatEqualityComparer : IEqualityComparer<float>
    {
        public static FloatEqualityComparer Default { get; } = new();
        public bool Equals(float x, float y) => Mathf.Approximately(x, y);

        public int GetHashCode(float obj) => obj.GetHashCode();
    }

    private static float Tangent(float timeStart, float timeEnd, float valueStart, float valueEnd)
    {
        return (valueEnd - valueStart) / (timeEnd - timeStart);
    }

    public struct Keyframe
    {
        public float Time;
        public float Value;

        public Keyframe(float time, float value)
        {
            Time = time;
            Value = value;
        }

        public readonly UnityEngine.Keyframe ToUnityKeyframe() => new(Time, Value);

        public sealed class TimeEqualityComparer : IEqualityComparer<Keyframe>
        {
            public static TimeEqualityComparer Defualt { get; } = new();

            bool IEqualityComparer<Keyframe>.Equals(Keyframe x, Keyframe y) => x.Time == y.Time;
            int IEqualityComparer<Keyframe>.GetHashCode(Keyframe obj) => obj.Time.GetHashCode();
        }
    }

    private static readonly Internal_SetEditorCurve? _Internal_SetEditorCurve;
    private static readonly SyncEditorCurvesDelegate? _SyncEditorCurves;

    private delegate void Internal_SetEditorCurve(AnimationClip clip, EditorCurveBinding binding, AnimationCurve curve, bool syncEditorCurves);
    private delegate void SyncEditorCurvesDelegate(AnimationClip clip);

    static AnimationClipBuilder()
    {
        var method = new DynamicMethod(nameof(Internal_SetEditorCurve), null, new[] { typeof(AnimationClip), typeof(EditorCurveBinding), typeof(AnimationCurve), typeof(bool) }, typeof(AnimationUtility), true);
        var original = typeof(AnimationUtility).GetMethod(nameof(Internal_SetEditorCurve), BindingFlags.Static | BindingFlags.NonPublic);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Call, original);
        il.Emit(OpCodes.Ret);
        _Internal_SetEditorCurve = method.CreateDelegate(typeof(Internal_SetEditorCurve)) as Internal_SetEditorCurve;

        method = new DynamicMethod(nameof(SyncEditorCurves), null, new[] { typeof(AnimationClip) }, typeof(AnimationUtility), true);
        original = typeof(AnimationUtility).GetMethod(nameof(SyncEditorCurves), BindingFlags.Static | BindingFlags.NonPublic);
        il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, original);
        il.Emit(OpCodes.Ret);
        _SyncEditorCurves = method.CreateDelegate(typeof(SyncEditorCurvesDelegate)) as SyncEditorCurvesDelegate;

    }

    protected override AnimationClip CreateInstance() => new();
}
