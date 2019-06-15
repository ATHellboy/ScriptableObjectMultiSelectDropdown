using UnityEngine;
using ScriptableObjectMultiSelectDropdown;

[CreateAssetMenu(menuName = "Create Block Manager Settings")]
public class BlockManagerSettings : ScriptableObject
{
    // Without grouping (default is None)
    [ScriptableObjectMultiSelectDropdown(typeof(Block))]
    public ScriptableObjectReference firstTargetBlocks;
    // By grouping
    [ScriptableObjectMultiSelectDropdown(typeof(Block), grouping = ScriptableObjectGrouping.ByFolderFlat)]
    public ScriptableObjectReference secondTargetBlocks;
}