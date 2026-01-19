using System;
using System.Collections.Generic;

namespace Fougerite
{
    /// <summary>
    /// A specialized collection for ItemDataBlock objects. 
    /// This class provides a centralized way to store and search for item blueprints 
    /// within the game's data dictionary.
    /// </summary>
    public class ItemsBlocks : List<ItemDataBlock>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsBlocks"/> class 
        /// and populates it with a provided list of ItemDataBlocks.
        /// </summary>
        /// <param name="items">The initial list of item datablocks to add to the collection.</param>
        public ItemsBlocks(List<ItemDataBlock> items)
        {
            foreach (ItemDataBlock block in items)
            {
                Add(block);
            }
        }

        /// <summary>
        /// Searches for a specific <see cref="ItemDataBlock"/> by its name.
        /// This search is case-insensitive and uses the invariant culture to ensure 
        /// consistent results across different system locales.
        /// </summary>
        /// <param name="str">The name of the item to find ("M4", "Large Medkit").</param>
        /// <returns>The found <see cref="ItemDataBlock"/>, or null if no match is found.</returns>
        public ItemDataBlock Find(string str)
        {
            foreach (ItemDataBlock block in this)
            {
                if (string.Equals(block.name, str, StringComparison.InvariantCultureIgnoreCase))
                {
                    return block;
                }
            }
            return null;
        }
    }
}