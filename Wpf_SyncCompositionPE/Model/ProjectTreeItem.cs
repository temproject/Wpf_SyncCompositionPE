using System.Collections.Generic;
using System.Collections.ObjectModel;
using TFlex.DOCs.Model.References;

namespace WpfApp_TreeSyncCompositionWork.Model
{
    public class ProjectTreeItem : ITreeNode
    {

        public ReferenceObject ReferenceObject;
        string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    if (ReferenceObject != null)
                        _name = ReferenceObject[ProjectManagementWork.PM_param_Name_GUID].GetString();
                    else _name = "";
                }
                return _name;
            }
            set { }
        }

        public ProjectTreeItem()
        {

        }
        public ProjectTreeItem(ReferenceObject referenceObject, bool IsForAdd)
        {
            this.ReferenceObject = referenceObject;
            this.IsForAdd = IsForAdd;
        }

        public bool IsForAdd { get; set; }


        ITreeNode _Parent;
        public ITreeNode Parent
        {
            get
            {
                if (_Parent == null)
                {
                    ReferenceObject parent = this.ReferenceObject.Parent;
                    if (parent != null) _Parent = Factory.CreateProjectTreeItem(parent);
                }
                return _Parent;
            }
        }

        IEnumerable<ITreeNode> _Children;
        public IEnumerable<ITreeNode> Children
        {
            get
            {
                if (_Children == null)
                {
                    List<ITreeNode> temp = new List<ITreeNode>();
                    foreach (var item in this.ReferenceObject.Children)
                    {
                        ITreeNode node = Factory.CreateProjectTreeItem(item);
                        if (node != null) temp.Add(node);
                    }
                    _Children = temp;
                }
                return _Children;
            }
        }
    }
}

