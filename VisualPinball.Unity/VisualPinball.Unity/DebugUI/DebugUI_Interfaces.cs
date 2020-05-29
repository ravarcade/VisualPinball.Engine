using Unity.Entities;

namespace VisualPinball.Unity.DebugUI_Interfaces
{
	public enum Params
	{
		Physics_FlipperAcc = 1,
		Physics_FlipperOffScale = 2,
		Physics_FlipperOnNearEndScale = 3,
		Physics_FlipperNumOfDegreeNearEnd = 4,
		Physics_FlipperMass = 5,
	}

    /// <summary>
    /// Comunication interface to VisualPinball.Engine.Unity.ImgGUI	
    /// </summary>
    public interface IDebugUI
    {
        void OnRegisterFlipper(Entity entity, string name);
        void OnPhysicsUpdate();
		void PhysicsFrameProcessingTime(float t);
	}

	/// <summary>
	/// Comunication interface to PhysicsEngine.
	/// For VPX-Physics see VPX_PhysicsDebugDataProvider
	/// </summary>
	public interface IPhysicsDebugDataProvider
	{
        void OnRegisterFlipper(Entity entity, string name);
        void OnPhysicsUpdate();

		float GetFloat(Params param);
		void SetFloat(Params param, float val);
	}
}

