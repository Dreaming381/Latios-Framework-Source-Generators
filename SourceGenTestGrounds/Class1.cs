using System;

using Latios;
using Latios.Unika;
using Latios.Unika.Authoring;

namespace TestLibrary.ActionableNamespaces.TotalChaos
{
    public partial class Class1
    {
        internal partial struct NestedStruct
        {
            static partial class NestedStaticClass
            {
                public partial struct MyCollectionComponent3 : ICollectionComponent { }

                public partial struct MyCollectionComponent2 : Latios.ICollectionComponent { }
            }
        }
    }

    public partial interface ITimer : IUnikaInterface
    {
    }
    public partial struct Timer : IUnikaScript, ITimer { }
    public partial class TimerAuthoring : UnikaScriptAuthoring<Timer>
    {
    }
    public partial class TimerAuthoring2 : Latios.Unika.Authoring.UnikaScriptAuthoring<Timer>
    {
    }
}

namespace Latios
{
    public interface ICollectionComponent
    {
    }
}

namespace Latios.Unika
{
    public interface IUnikaScript
    {
    }
    public interface IUnikaInterface
    {
    }
}

namespace Latios.Unika.Authoring
{
    public class UnikaScriptAuthoringBase
    {
    }

    public class UnikaScriptAuthoring<T> : UnikaScriptAuthoringBase where T : unmanaged, IUnikaScript
    {
    }
}

