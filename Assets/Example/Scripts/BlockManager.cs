using ScriptableObjectMultiSelectDropdown;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    // Without grouping (default is None)
    [ScriptableObjectMultiSelectDropdown(typeof(Block))]
    public ScriptableObjectReference firstTargetBlocks;
    // By grouping
    [ScriptableObjectMultiSelectDropdown(typeof(Block), grouping = ScriptableObjectGrouping.ByFolder)]
    public ScriptableObjectReference secondTargetBlocks;
}