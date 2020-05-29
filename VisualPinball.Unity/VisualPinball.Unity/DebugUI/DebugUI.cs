using VisualPinball.Unity.DebugUI_Interfaces;
using Unity.Entities;

namespace VisualPinball.Unity.Game
{
    /// <summary>
    /// Interface used by core Visual Pinball Engine to comunicate with DebugUI
    /// </summary>
    public static class DebugUI
    {
		private static IDebugUI _debugUI = null;
		private static IPhysicsDebugDataProvider _physicsDebugData = new VPX_PhysicsDebugDataProvider();

		public static IPhysicsDebugDataProvider physics { get => _physicsDebugData; }

		public static void RegisterDebugUI(IDebugUI dbgUI)
		{
			_debugUI = dbgUI;
		}
		public static void RegisterPhysicsDebugDataProvider(IPhysicsDebugDataProvider physicsDebugDataProvider)
		{
			_physicsDebugData = physicsDebugDataProvider;
		}

		public static void OnRegisterFlipper(Entity entity, string name)
        {
			_physicsDebugData?.OnRegisterFlipper(entity, name);
            _debugUI?.OnRegisterFlipper(entity, name);
        }

        public static void OnPhysicsUpdate()
        {
			_physicsDebugData?.OnPhysicsUpdate();
            _debugUI?.OnPhysicsUpdate();
        }

		public static void PhysicsFrameProcessingTime(float t)
		{
			_debugUI?.PhysicsFrameProcessingTime(t);
		}


	}
}

