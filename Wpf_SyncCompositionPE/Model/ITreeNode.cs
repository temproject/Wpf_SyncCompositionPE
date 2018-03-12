using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Wpf_SyncCompositionPE.Model
{
    public interface ITreeNode
    {
        ITreeNode Parent { get; }

        IEnumerable<ITreeNode> Children { get; }
    }
}
