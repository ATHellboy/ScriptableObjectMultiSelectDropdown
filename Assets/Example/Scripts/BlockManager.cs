using Example.Blocks;
using ScriptableObjectMultiSelectDropdown;
using UnityEngine;

namespace Example
{
    public class BlockManager : MonoBehaviour
    {
        [SerializeField] private bool _debug = true;
        [SerializeField] private BlockManagerSettings _blockManagerSettings = default;
        // Without grouping (default is None)
        [ScriptableObjectMultiSelectDropdown(typeof(Block))]
        public ScriptableObjectReference targetBlocks;
        // By grouping
        [ScriptableObjectMultiSelectDropdown(typeof(Block), grouping = ScriptableObjectGrouping.ByFolder)]
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

        void Start()
        {
            if (_debug)
            {
                PrintValues(targetBlocks);
                PrintValues(targetBlocksByGrouping);
                PrintValues(derivedClassTargetBlock);
                PrintValues(derivedAbstractClassTargetBlock);
                PrintValues(interfaceTargetBlock);

                PrintValues(_blockManagerSettings.targetBlocks);
                PrintValues(_blockManagerSettings.targetBlocksByGrouping);
                PrintValues(_blockManagerSettings.derivedClassTargetBlock);
                PrintValues(_blockManagerSettings.derivedAbstractClassTargetBlock);
                PrintValues(_blockManagerSettings.interfaceTargetBlock);
            }
        }

        private void PrintValues(ScriptableObjectReference scriptableObjectReference)
        {
            for (int i = 0; i < scriptableObjectReference.values.Length; i++)
            {
                print(scriptableObjectReference.values[i]);
            }
        }
    }
}