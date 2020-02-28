using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension.Standalone;

namespace CommunityLauncher
{
    internal class StandaloneInputService : IInputService
    {
        public StandaloneInputService(GraphicsForm graphicsForm)
        {
        }

        bool IInputService.MouseEnabled
        {
            get
            {
                return true;
            }
        }

        bool IInputService.KeyboardEnabled
        {
            get
            {
                return true;
            }
        }

        bool IInputService.GamepadEnabled
        {
            get
            {
                return false;
            }
        }
    }
}