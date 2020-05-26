using System;
using System.Collections.Generic;
using UnityEngine;
using Player = VisualPinball.Unity.Game.Player;

namespace VisualPinball.Unity.Physics
{
	public struct DebugFlipperData
	{
		public string Name;
		public List<float> SolenoidOnAngles;
		public List<float> SolenoidOffAngles;
		public int SolenoidSate;
	};

	/// <summary>
	/// All game events related to physics simulation goes thru this interface.	
	/// </summary>
	public interface IPhysicsEngine
    {
		// Events
		void OnCreateBall(GameObject go, float radius, float mass);
		void OnRotateToEnd(int flipperId);
		void OnRotateToStart(int flipperId);
		void OnRegisterFlipper(GameObject flipperGameObject, int flipperId);

		void Initialize(Player player);

		int GetFrameCount();

		void ManualBallRoller(Vector3 cursor);

		// Called by debug UI (ImGui) every frame
		void OnDebugDraw();
		
		// events in debug UI
		event Action<float> PushUI_PhysicsProcessingTime;
		event Action<DebugFlipperData> PushUI_DebugFlipperData;

		// Callback functions in debug UI used to get params
		// First arg is int (index)
		// Second current value
		// Return: new value
		event Func<int, int, int> GetUI_Int;
		event Func<int, float, float> GetUI_Float;
	}
}
