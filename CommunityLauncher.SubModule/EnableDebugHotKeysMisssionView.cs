using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.View.Missions;

namespace CommunityLauncher.Submodule
{
    public class EnableDebugHotkeysMissionView : MissionView
    {
        public override void OnMissionTick(float dt)
        {
            if (!MissionScreen.SceneLayer.Input.IsCategoryRegistered(HotKeyManager.GetCategory("Debug")))
            {

                MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("Debug"));
            }
            base.OnMissionTick(dt);
        }
    }
}
