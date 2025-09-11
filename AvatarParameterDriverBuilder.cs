#if VRC_SDK_VRCSDK3
using VRC.SDKBase;
using AvatarParameterDriver = VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver;
using AvatarParameterDriverParameter = VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver.Parameter;
using ChangeType = VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver.ChangeType;
#endif

namespace Numeira.Animation;

#if !VRC_SDK_VRCSDK3
internal sealed class AvatarParameterDriverParameter
{
    public ChangeType type;
    public string? name;
    public string? source;
    public float value;
    public float valueMin;
    public float valueMax = 1f;
    public float chance = 1f;
    public bool convertRange;
    public float sourceMin;
    public float sourceMax;
    public float destMin;
    public float destMax;
    public object? sourceParam;
    public object? destParam;
}

internal enum ChangeType
{
    Set,
    Add,
    Random,
    Copy
}
#endif

internal sealed class AvatarParameterDriverBuilder
#if VRC_SDK_VRCSDK3
    : StateMachineBehaviourBuilder
#endif
{
    public List<AvatarParameterDriverParameter> Parameters { get; } = new();

    public AvatarParameterDriverBuilder AddParameter(AvatarParameterDriverParameter parameter)
    {
        Parameters.Add(parameter);
        return this;
    }

    public AvatarParameterDriverBuilder Copy(string source, string destination)
        => AddParameter(new() { type = ChangeType.Copy, source = source, name = destination });


    public AvatarParameterDriverBuilder Set(string name, float value)
        => AddParameter(new() { type = ChangeType.Set, name = name, value = value });

#if VRC_SDK_VRCSDK3
    protected override StateMachineBehaviour CreateInstance()
    {
        return ScriptableObject.CreateInstance<AvatarParameterDriver>();
    }

    protected override void SetupBehaviour(StateMachineBehaviour behaviour)
    {
        var dr = (behaviour as AvatarParameterDriver)!;

        dr.parameters = Parameters;
    }
#endif
}

static partial class StateBuilderExt
{
    public static AvatarParameterDriverBuilder AddAvatarParameterDriver(this StateBuilder state)
    {
        var b = new AvatarParameterDriverBuilder();
#if VRC_SDK_VRCSDK3
        state.Behaviours.Add(b);
#endif
        return b;
    }
}