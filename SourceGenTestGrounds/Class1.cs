using System;

using Latios;

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
}

namespace Latios
{
    public interface ICollectionComponent
    {
    }
}

