
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VisualPinball.Unity.VPT.Flipper;

namespace VisualPinball.Unity.DebugAndPhysicsComunicationProxy
{
    public class VPX_PhysicsEngineProxyClient : IPhysicsEngine
    {
        protected readonly EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        Dictionary<Entity, FlipperState> _flippers = new Dictionary<Entity, FlipperState>();

        public void OnRegisterFlipper(Entity entity, string name) { _flippers[entity] = new FlipperState(); }

        public void OnPhysicsUpdate(int numSteps, float processingTime)
        {
            // store state of flippers
            var keys = _flippers.Keys.ToList();
            foreach (var entity in keys)
            {
                var fmd = EntityManager.GetComponentData<FlipperMovementData>(entity);
                var fsd = EntityManager.GetComponentData<FlipperStaticData>(entity);
                var fss = EntityManager.GetComponentData<SolenoidStateData>(entity);
                _flippers[entity] = new FlipperState(
                    math.degrees(math.abs(fmd.Angle - fsd.AngleStart)),
                    fss.Value);
            }
        }

        public void OnCreateBall(Entity entity, float3 position, float3 velocity, float radius, float mass) { }
        public void OnRotateToEnd(Entity entity) { }
        public void OnRotateToStart(Entity entity) { }
        public bool UsePureEntity() { return false; }

        public bool GetFlipperState(Entity entity, out FlipperState flipperState)
        {
            return _flippers.TryGetValue(entity, out flipperState);
        }

        public float GetFloat(Params param) { return 0; }
        public void SetFloat(Params param, float val) { }
        public void ManualBallRoller(Entity entity, float3 targetPosition) { }
    }
}
