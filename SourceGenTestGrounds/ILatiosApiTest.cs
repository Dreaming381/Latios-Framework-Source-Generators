// This file was originally written with Claude
using Latios;
using Unity.Entities;

namespace SourceGenTestGrounds
{
    public struct MyComponent : IComponentData
    {
    }

    public struct MyBufferElement : IBufferElementData
    {
    }

    public struct MyGettable : ILatiosApiGettable
    {
        public void CreateForApi(ref SystemState state)
        {
        }

        public void UpdateForApi(ref SystemState state)
        {
        }
    }

    public struct MyGettableBool : ILatiosApiGettableBool
    {
        public void CreateForApi(ref SystemState state, bool b)
        {
        }

        public void UpdateForApi(ref SystemState state)
        {
        }
    }

    public partial struct MyInjectable : IInjectable
    {
        [Inject]
        [Unity.Collections.ReadOnly]
        public ComponentTypeHandle<MyComponent> roHandle;
        [Inject]
        public ComponentTypeHandle<MyComponent> rwHandle;
        [Inject]
        public MyGettable gettable;
        [Inject]
        [Unity.Collections.ReadOnly]
        public MyGettableBool gettableBool;
        // Not [Inject]-attributed, so left alone by the generator.
        public int notInjected;
        // [Inject]-attributed but an unsupported type - silently skipped by the generator, not an error.
        [Inject]
        public float unsupported;
    }

    public partial struct MyLatiosApiSystem : ISystem, ILatiosApi
    {
        public void OnUpdate(ref SystemState state)
        {
            var api = this.GetApi(ref state);

            var roHandle     = api.GetComponentHandle<MyComponent>(true);
            var rwHandle     = api.GetComponentHandle<MyComponent>(false);
            var lookup       = api.GetComponentLookup<MyComponent>(true);
            var bufferHandle = api.GetBufferHandle<MyBufferElement>(true);
            var bufferLookup = api.GetBufferLookup<MyBufferElement>(false);
            var entityHandle = api.GetEntityHandle();
            var storageInfo  = api.GetEntityStorageInfoLookup();
            var gettable     = api.Get<MyGettable>();
            var gettableBool = api.Get<MyGettableBool>(true);

            var byValueInjected = default(MyInjectable).Inject(api);

            var byRefInjected = default(MyInjectable);
            byRefInjected.InjectByRef(api);
        }
    }
}

