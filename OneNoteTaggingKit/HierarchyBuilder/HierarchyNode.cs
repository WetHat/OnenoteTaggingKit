using Microsoft.Office.Interop.OneNote;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.HierarchyBuilder
{
    /// <summary>
    /// Representation of an element in the hierarchy of the OneNote page tree.
    /// </summary>
    /// <remarks>
    /// Chains of instances of this class are typically used to describe a path to a
    /// OneNote page
    /// </remarks>
    public class HierarchyNode : IKeyedItem<string>
    {
        /// <summary>
        /// Get the node type.
        /// </summary>
        public HierarchyElement NodeType { get; }

        /// <summary>
        /// Get the parent node.
        /// </summary>
        /// <value>`null` if the node is a root node or the parent node is unknown.</value>
        public HierarchyNode Parent { get; }

        /// <summary>
        /// create a new instance of an element in the OneNote object hierarchy.
        /// </summary>
        /// <param name="hierarchyNode">A node in the OneNote page hierarchy.</param>
        /// <param name="parent">
        ///     The parent node in the page hierarchy.
        ///     `null` if this is a root node (`one:Notebook``) or the parent
        ///     node is unknown.
        /// </param>
        /// <param name="type">The element type, if kmown</param>
        public HierarchyNode(XElement hierarchyNode,
                             HierarchyNode parent,
                             HierarchyElement type = HierarchyElement.heNone ) {
            ID = (string)hierarchyNode.Attribute("ID");
            Name = (string)hierarchyNode.Attribute("name");
            Parent = parent;
            NodeType = type;
        }

        /// <summary>
        /// Get the unique id of the node in the OneNote page hierarchy tree.
        /// </summary>
        public string ID { get; }
        /// <summary>
        /// Get the user-friendly name of this element in the OneNote hierachy
        /// </summary>
        public string Name { get; }

        #region IKeyedItem<string>

        /// <summary>
        /// get the unique key of this item
        /// </summary>
        public string Key { get => ID; }

        #endregion IKeyedItem<string>
    }

}
