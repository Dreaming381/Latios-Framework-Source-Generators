// This file was originally written with Claude
// Minimal stand-ins for the real Unity.Entities / Latios types, matching this file's
// fully-qualified names so ILatiosApiGenerator's semantic matching resolves against them.
// Not a functional implementation - compile-testing only.
namespace Unity.Collections
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ReadOnlyAttribute : Attribute
    {
    }
}

namespace Unity.Collections.LowLevel.Unsafe
{
    // Pre-existing gap (predates IInjectable): ILatiosApiCodeWriter's __Get<T>() dispatch already used
    // UnsafeUtility.As, but this project never stubbed it, so it never actually compiled cleanly.
    public static class UnsafeUtility
    {
        public static ref TTo As<TFrom, TTo>(ref TFrom source) => throw new System.NotImplementedException();
    }
}

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
    using System;
    using Unity.Entities;

    // No default bodies here (unlike the real ILatiosApi in ILatiosApi.cs) - see the IInjectable stub
    // below for why. Previously this stub declared no members at all, which meant the generator's
    // emitted explicit interface implementations (__OnCreateForLatios/__Get<T>/etc.) had nothing to
    // bind to and this project never actually compiled cleanly; declaring the members (without bodies)
    // fixes that pre-existing gap.
    public interface ILatiosApi
    {
        void __OnCreateForLatios(ref SystemState state);
        T __Get<T>() where T : unmanaged;
        T __Get<T>(bool b) where T : unmanaged;
        LatiosWorldUnmanaged __GetLatiosWorldUnmanaged();
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

    // No default bodies here (unlike the real IInjectable in ILatiosApi.cs) - this netstandard2.0
    // compile-test project doesn't target a runtime that supports default interface implementations (CS8701).
    public interface IInjectable
    {
        void __CreateForApi(ref SystemState state);
        void __Inject<T>(ref SystemState state, ref T target) where T : unmanaged, IInjectable;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
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

        public static TInject Inject<TInject, TSystem>(this TInject injectable, LatiosApiInvoker<TSystem> api) where TInject : unmanaged,
        IInjectable where TSystem : unmanaged, ISystem, ILatiosApi => injectable;

        public static ref TInject InjectByRef<TInject, TSystem>(ref this TInject injectable, LatiosApiInvoker<TSystem> api) where TInject : unmanaged,
        IInjectable where TSystem : unmanaged, ISystem, ILatiosApi => ref injectable;
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

            public static T CreateInjectable<T>(ref SystemState state) where T : unmanaged, IInjectable
            {
                T result = default;
                result.__CreateForApi(ref state);
                return result;
            }
        }
    }
}

