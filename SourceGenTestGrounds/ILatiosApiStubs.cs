// This file was originally written with Claude
// Minimal stand-ins for the real Unity.Entities / Latios types, matching this file's
// fully-qualified names so ILatiosApiGenerator's semantic matching resolves against them.
// Not a functional implementation - compile-testing only.
namespace Unity.Entities
{
    public interface ISystem
    {
    }

    public interface IComponentData
    {
    }

    public interface IBufferElementData
    {
    }

    public struct SystemState
    {
        public ComponentTypeHandle<T> GetComponentTypeHandle<T>(bool isReadOnly = false) where T : unmanaged, IComponentData => default;
        public ComponentLookup<T> GetComponentLookup<T>(bool isReadOnly         = false) where T : unmanaged, IComponentData => default;
        public BufferTypeHandle<T> GetBufferTypeHandle<T>(bool isReadOnly       = false) where T : unmanaged, IBufferElementData => default;
        public BufferLookup<T> GetBufferLookup<T>(bool isReadOnly               = false) where T : unmanaged, IBufferElementData => default;
        public EntityTypeHandle GetEntityTypeHandle() => default;
        public EntityStorageInfoLookup GetEntityStorageInfoLookup() => default;
    }

    public struct ComponentTypeHandle<T> where T : unmanaged, IComponentData
    {
        public void Update(ref SystemState state)
        {
        }
    }

    public struct ComponentLookup<T> where T : unmanaged, IComponentData
    {
        public void Update(ref SystemState state)
        {
        }
    }

    public struct BufferTypeHandle<T> where T : unmanaged, IBufferElementData
    {
        public void Update(ref SystemState state)
        {
        }
    }

    public struct BufferLookup<T> where T : unmanaged, IBufferElementData
    {
        public void Update(ref SystemState state)
        {
        }
    }

    public struct EntityTypeHandle
    {
        public void Update(ref SystemState state)
        {
        }
    }

    public struct EntityStorageInfoLookup
    {
        public void Update(ref SystemState state)
        {
        }
    }
}

namespace Latios
{
    using Unity.Entities;

    public interface ILatiosApi
    {
    }

    public interface ILatiosApiGettable
    {
        void CreateForApi(ref SystemState state);
        void UpdateForApi(ref SystemState state);
    }

    public interface ILatiosApiGettableBool
    {
        void CreateForApi(ref SystemState state, bool b);
        void UpdateForApi(ref SystemState state);
    }

    public struct LatiosWorldUnmanaged
    {
    }

    public static class LatiosWorldUnmanagedRetrieveExtensions
    {
        public static LatiosWorldUnmanaged GetLatiosWorldUnmanaged(this ref SystemState state) => default;
    }

    public struct LatiosApiInvoker<TSystem> where TSystem : unmanaged, ISystem, ILatiosApi
    {
        public TGettable Get<TGettable>() where TGettable : unmanaged, ILatiosApiGettable => default;
        public TGettable Get<TGettable>(bool b) where TGettable : unmanaged, ILatiosApiGettableBool => default;
        public ComponentTypeHandle<T> GetComponentHandle<T>(bool readOnly) where T : unmanaged, IComponentData => default;
        public ComponentLookup<T> GetComponentLookup<T>(bool readOnly) where T : unmanaged, IComponentData => default;
        public BufferTypeHandle<T> GetBufferHandle<T>(bool readOnly) where T : unmanaged, IBufferElementData => default;
        public BufferLookup<T> GetBufferLookup<T>(bool readOnly) where T : unmanaged, IBufferElementData => default;
        public EntityTypeHandle GetEntityHandle() => default;
        public EntityStorageInfoLookup GetEntityStorageInfoLookup() => default;
    }

    public static class LatiosApiCreateExtensions
    {
        public static LatiosApiInvoker<T> GetApi<T>(ref this T system, ref SystemState state) where T : unmanaged, ISystem, ILatiosApi => default;
        public static void OnCreateForLatios<T>(ref this T system, ref SystemState state) where T : unmanaged, ISystem, ILatiosApi
        {
        }
    }

    namespace InternalSourceGen
    {
        public static class StaticAPI
        {
            public static LatiosWorldUnmanaged GetLatiosWorldUnmanaged(ref SystemState state) => state.GetLatiosWorldUnmanaged();

            public static T Create<T>(ref SystemState state) where T : unmanaged, ILatiosApiGettable
            {
                T result = default;
                result.CreateForApi(ref state);
                return result;
            }

            public static T Create<T>(ref SystemState state, bool b) where T : unmanaged, ILatiosApiGettableBool
            {
                T result = default;
                result.CreateForApi(ref state, b);
                return result;
            }
        }
    }
}

