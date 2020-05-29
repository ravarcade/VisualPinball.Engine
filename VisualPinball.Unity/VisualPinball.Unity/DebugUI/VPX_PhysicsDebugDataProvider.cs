
using Unity.Entities;

namespace VisualPinball.Unity.DebugUI_Interfaces
{
    public class VPX_PhysicsDebugDataProvider  : IPhysicsDebugDataProvider
    {
        public void OnRegisterFlipper(Entity entity, string name)
        {

        }

		public void OnPhysicsUpdate()
        {

        }

		public float GetFloat(Params param)
		{
			return 0;
		}

		public void SetFloat(Params param, float val)
		{
			
		}
	}
}
