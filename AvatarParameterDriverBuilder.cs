#if VRC_SDK_VRCSDK3
using VRC.SDKBase;
using AvatarParameterDriver = VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver;
using AvatarParameterDriverParameter = VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver.Parameter;

namespace Numeira.Animation;

internal sealed class AvatarParameterDriverBuilder : StateMachineBehaviourBuilder
{
    public List<AvatarParameterDriverParameter> Parameters { get; } = new();

    public AvatarParameterDriverBuilder AddParameter(AvatarParameterDriverParameter parameter)
    {
        Parameters.Add(parameter);
        return this;
    }

    public AvatarParameterDriverBuilder Copy(string source, string destination)
        => AddParameter(new() { type = VRC_AvatarParameterDriver.ChangeType.Copy, source = source, name = destination });


    public AvatarParameterDriverBuilder Set(string name, float value)
        => AddParameter(new() { type = VRC_AvatarParameterDriver.ChangeType.Set, name = name, value = value });

    protected override StateMachineBehaviour CreateInstance()
    {
        return ScriptableObject.CreateInstance<AvatarParameterDriver>();
    }

    protected override void SetupBehaviour(StateMachineBehaviour behaviour)
    {
        var dr = (behaviour as AvatarParameterDriver)!;

        dr.parameters = Parameters;
    }
}

static partial class StateBuilderExt
{
    public static AvatarParameterDriverBuilder AddAvatarParameterDriver(this StateBuilder state)
    {
        var b = new AvatarParameterDriverBuilder();
        state.Behaviours.Add(b);
        return b;
    }
}
#endif