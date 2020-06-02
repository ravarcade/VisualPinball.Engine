﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using VisualPinball.Engine.Common;
using VisualPinball.Engine.Game;
using VisualPinball.Engine.Resources;
using VisualPinball.Unity.Extensions;
using VisualPinball.Unity.Game;
using VisualPinball.Unity.Physics.Engine;
using Player = VisualPinball.Unity.Game.Player;

namespace VisualPinball.Unity.VPT.Ball
{
	public class BallManager
	{
		private int _id;

		private readonly Engine.VPT.Table.Table _table;

		private static readonly int MainTex = Shader.PropertyToID("_MainTex");
		private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
		private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
		private static readonly int Metallic = Shader.PropertyToID("_Metallic");
		private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
		private static Mesh _unitySphereMesh; // used to cache ball mesh from GameObject

		public BallManager(Engine.VPT.Table.Table table)
		{
			_table = table;
		}

		public BallApi CreateBall(Player player, IBallCreationPosition ballCreator, float radius, float mass)
		{
			// calculate mass and scale
			var m = player.TableToWorld;

			var localPos = ballCreator.GetBallCreationPosition(_table).ToUnityFloat3();
			var localVel = ballCreator.GetBallCreationVelocity(_table).ToUnityFloat3();
			localPos.z += radius;
			//float4x4 model = player.TableToWorld * Matrix4x4.TRS(localPos, Quaternion.identity, new float3(radius));

			var worldPos = m.MultiplyPoint(localPos);
			var scale3 = new Vector3(
				m.GetColumn(0).magnitude,
				m.GetColumn(1).magnitude,
				m.GetColumn(2).magnitude
			);
			var scale = (scale3.x + scale3.y + scale3.z) / 3.0f; // scale is only scale (without radiusfloat now, not vector.
			var material = CreateMaterial();

			// go will be converted automatically to entity
			//CreateViaGameObject(worldPos, localPos, localVel, scale * radius * 2, mass, radius, material);
			var physicsEngine = EngineProvider<IPhysicsEngineNew>.Instance.Get();
			var ballEntity = physicsEngine.UsePureEntity
				? CreatePureEntity(worldPos, scale * radius * 2, GetSphereMesh(), material)
				: CreateEntity(worldPos, localPos, localVel, scale * radius * 2, mass, radius, material);

			physicsEngine.OnCreateBall( ballEntity, localPos, localVel, mass, radius);

			return null;
		}

		private Entity CreatePureEntity(float3 position, float scale, Mesh mesh, Material material)
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

			Entity entity = entityManager.CreateEntity(
				typeof(Translation),
				typeof(Scale),
				typeof(Rotation),
				typeof(RenderMesh),
				typeof(LocalToWorld),
				typeof(RenderBounds));

			entityManager.SetSharedComponentData(entity, new RenderMesh
			{
				mesh = mesh,
				material = material
			});

			entityManager.SetComponentData(entity, new RenderBounds
			{
				Value = mesh.bounds.ToAABB()
			});

			entityManager.SetComponentData(entity, new Translation
			{
				Value = position
			});

			entityManager.SetComponentData(entity, new Scale
			{
				Value = scale
			});

			return entity;
		}

		private Entity CreateEntity(Vector3 worldPos, float3 localPos, float3 localVel, float scale, float mass,
			float radius, Material material)
		{
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			return BallBehavior.CreateEntity(entityManager,
					GetSphereMesh(), material, worldPos, scale, localPos,
					localVel, radius, mass);
		}

		/// <summary>
		/// Dirty way to get SphereMesh from Unity.
		/// ToDo: Get Mesh from our resources
		/// </summary>
		/// <returns>Sphere Mesh</returns>
		private static Mesh GetSphereMesh()
		{
			if (!_unitySphereMesh)
			{
				var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				_unitySphereMesh = go.GetComponent<MeshFilter>().sharedMesh;
				GameObject.Destroy(go);
			}

			return _unitySphereMesh;
		}

		private GameObject CreateSphere(Material material, float3 pos, float3 scale)
		{
			// create go
			var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.name = $"Ball{++_id}";

			// set material
			go.GetComponent<Renderer>().material = material;

			// set position and scale
			go.transform.localPosition = pos;
			go.transform.localScale = scale;

			// mark to convert
			go.AddComponent<ConvertToEntity>();

			return go;
		}

		#region Material

		private static Material CreateMaterial()
		{
			if (GraphicsSettings.renderPipelineAsset != null) {
				if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("UniversalRenderPipelineAsset")) {
					return CreateUniversalMaterial();
				}

				if (GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("HDRenderPipelineAsset")) {
					return CreateHDMaterial();
				}
			}

			return CreateStandardMaterial();
		}

		private static Material CreateStandardMaterial()
		{
			var material = new Material(Shader.Find("Standard"));
			var texture = new Texture2D(512, 512, TextureFormat.RGBA32, true) {name = "BallDebugTexture"};
			texture.LoadImage(Resource.BallDebug.Data);
			material.SetTexture(MainTex, texture);
			material.SetFloat(Metallic, 1f);
			material.SetFloat(Glossiness, 1f);
			return material;
		}

		private static Material CreateHDMaterial()
		{
			return CreateScriptableMaterial("High Definition Render Pipeline/Lit");
		}

		private static Material CreateUniversalMaterial()
		{
			return CreateScriptableMaterial("Universal Render Pipeline/Lit");
		}

		private static Material CreateScriptableMaterial(string shaderName)
		{
			var material = new Material(Shader.Find(shaderName));
			var texture = new Texture2D(512, 512, TextureFormat.RGBA32, true) {name = "BallDebugTexture"};
			texture.LoadImage(Resource.BallDebug.Data);
			material.SetTexture(BaseMap, texture);
			material.SetColor(BaseColor, Color.white);
			material.SetFloat(Metallic, 0.85f);
			material.SetFloat(Glossiness, 0.75f);
			return material;
		}

		#endregion

	}
}
