using System.Reflection.Emit;

namespace Numeira.Animation;

internal abstract class StateMachineBehaviourBuilder
{
    static StateMachineBehaviourBuilder()
    {
        var method = new DynamicMethod("SetBehaviours", null, new[] { typeof(AnimatorState), typeof(ScriptableObject[]) }, typeof(AnimatorState), true);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, typeof(AnimatorState).GetProperty("behaviours_Internal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetMethod);
        il.Emit(OpCodes.Ret);
        setBehaviours = (method.CreateDelegate(typeof(Action<AnimatorState, ScriptableObject[]>)) as Action<AnimatorState, ScriptableObject[]>)!;
    }

    private static readonly Action<AnimatorState, ScriptableObject[]> setBehaviours;

    public StateMachineBehaviour Build(IAssetContainer container)
    {
        if (!container.TryGetValue(this, out StateMachineBehaviour behaviour))
        {
            behaviour = CreateInstance();
            container.Register(this, behaviour);
            SetupBehaviour(behaviour);
        }
        return behaviour;
    }

    protected abstract StateMachineBehaviour CreateInstance();

    protected abstract void SetupBehaviour(StateMachineBehaviour behaviour);

    public static void SetBehaviours(AnimatorState state, StateMachineBehaviour[] behaviours) => setBehaviours(state, System.Runtime.CompilerServices.Unsafe.As<ScriptableObject[]>(behaviours));
}
