using UnityEngine;
using ScriptableObjectMultiSelectDropdown;
using Example.Blocks;

namespace Example
{
    [CreateAssetMenu(menuName = "Create Block Manager Settings")]
    public class BlockManagerSettings : ScriptableObject
    {
        // Without grouping (default is None)
        [ScriptableObjectMultiSelectDropdown(typeof(Block))]
        public ScriptableObjectReference targetBlocks;
        // By grouping
        [ScriptableObjectMultiSelectDropdown(typeof(Block), grouping = ScriptableObjectGrouping.ByFolderFlat)]
        public ScriptableObjectReference targetBlocksByGrouping;
        // Derived class
        [ScriptableObjectMultiSelectDropdown(typeof(SandBlock))]
        public ScriptableObjectReference derivedClassTargetBlock;
        // Derived abstract class
        [ScriptableObjectMultiSelectDropdown(typeof(AbstarctBlock))]
        public ScriptableObjectReference derivedAbstractClassTargetBlock;
        // Interface
        [ScriptableObjectMultiSelectDropdown(typeof(IBlock))]
        public ScriptableObjectReference interfaceTargetBlock;
    }
}