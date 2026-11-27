using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;

public sealed class VersionedAddressablesBuild : BuildScriptPackedMode
{
    protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
    {
        // The base builder clears cached profile properties before calling this method.
        if (builderInput.PreviousContentState == null)
            builderInput.PlayerVersion = builderInput.AddressableSettings.PlayerBuildVersion;

        return base.BuildDataImplementation<TResult>(builderInput);
    }
}
