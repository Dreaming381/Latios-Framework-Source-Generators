# Latios Framework Source Generators

Beginning with 0.7, the Latios Framework began using source generators to
provide a better user experience while maintaining runtime performance. This
repository contains the source code for those source generators.

## Current Generators

### ICollectionComponent and IManagedStructComponent

These two interface types are extended to have generated an `ExistComponent :
IComponentData` and a `CleanupComponent : ICleanupComponentData`. These allow
reactive systems to process and manage these special types. In addition,
`ICollectionComponent` generates a Burst function pointer that forwards a
general-purpose context pointer to a generic static method of the collection
component type. This generalized interface allows for new Burst-compiled methods
to operate on type-punned collection components without the need for updating
the source generators.

## Future Generators

### Generic Job Registration

This generator will allow for specifying all invocations of a method decorated
with an attribute to generate the registration of a generic job based on the
generic type of that method.

### Audio Effects

These generators will create the function pointers used to execute effects on
the DSP thread.

### Unika

The entire Unika scripting suite will heavily rely on source generators to
enable a friendly scripting interface for designers in a Burst-compiled
environment.

## License

This project is licensed under the same license as the Latios Framework, that is
the Unity Companion License.

## Contributing

Want to make this repo better? Feel free to submit pull requests or post
suggestions on the official Latios Framework Discord. A great improvement to
this repository would be making it build with GitHub actions. Another would be
adding analyzers to spit out proper errors when a user forgets the `partial`
keyword.
