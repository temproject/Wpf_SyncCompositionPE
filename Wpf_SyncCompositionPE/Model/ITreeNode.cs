using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WpfApp_TreeSyncCompositionWork.Model
{
    public interface ITreeNode
    {
        ITreeNode Parent { get; }

        IEnumerable<ITreeNode> Children { get; }
    }
}
