#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRCTrackingType = VRC.SDK3.Avatars.Components.VRCAnimatorTrackingControl.TrackingType;
#endif

namespace Numeira.Animation;

public enum TrackingType
{
    NoChange,
    Tracking,
    Animation
}

public sealed class AnimatorTrackingControlBuilder
#if VRC_SDK_VRCSDK3 
    : StateMachineBehaviourBuilder
#endif
{
    public TrackingType Head { get; set; } = TrackingType.NoChange;
    public TrackingType LeftHand { get; set; } = TrackingType.NoChange;
    public TrackingType RightHand { get; set; } = TrackingType.NoChange;
    public TrackingType Hip { get; set; } = TrackingType.NoChange;
    public TrackingType LeftFoot { get; set; } = TrackingType.NoChange;
    public TrackingType RightFoot { get; set; } = TrackingType.NoChange;
    public TrackingType LeftFingers { get; set; } = TrackingType.NoChange;
    public TrackingType RightFingers { get; set; } = TrackingType.NoChange;
    public TrackingType Eyes { get; set; } = TrackingType.NoChange;
    public TrackingType Mouth { get; set; } = TrackingType.NoChange;

#if VRC_SDK_VRCSDK3
    protected override StateMachineBehaviour CreateInstance() => ScriptableObject.CreateInstance<VRCAnimatorTrackingControl>();

    protected override void SetupBehaviour(StateMachineBehaviour behaviour)
    {
        var trackingControl = (behaviour as VRCAnimatorTrackingControl)!;

        trackingControl.trackingHead = (VRCTrackingType)Head;
        trackingControl.trackingLeftHand = (VRCTrackingType)LeftHand;
        trackingControl.trackingRightHand = (VRCTrackingType)RightHand;
        trackingControl.trackingHip = (VRCTrackingType)Hip;
        trackingControl.trackingLeftFoot = (VRCTrackingType)LeftFoot;
        trackingControl.trackingRightFoot = (VRCTrackingType)RightFoot;
        trackingControl.trackingLeftFingers = (VRCTrackingType)LeftFingers;
        trackingControl.trackingRightFingers = (VRCTrackingType)RightFingers;
        trackingControl.trackingEyes = (VRCTrackingType)Eyes;
        trackingControl.trackingMouth = (VRCTrackingType)Mouth;
    }
#endif
}

static partial class StateBuilderExt
{
    public static AnimatorTrackingControlBuilder AddTrackingControl(this StateBuilder state)
    {
        var b = new AnimatorTrackingControlBuilder();
#if VRC_SDK_VRCSDK3
        state.Behaviours.Add(b);
#endif
        return b;
    }
}