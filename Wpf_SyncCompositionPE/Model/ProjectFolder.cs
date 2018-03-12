using System;
using TFlex.DOCs.Model.References;

namespace Wpf_SyncCompositionPE.Model
{
    public class ProjectFolder : ProjectTreeItem
    {
        public ProjectFolder(ReferenceObject referenceObject)
        {
            this.ReferenceObject = referenceObject;
        }
  
        //public bool IsNull { get; private set; }

        public static Guid PM_class_ProjectFolder_Guid = new Guid("ae994d8a-7193-4ee3-8ef6-e443b23f7f54"); //тип - "Папка"
    }
}
