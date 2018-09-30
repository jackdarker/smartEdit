using System;
using System.Collections.Generic;
using System.Text;

namespace smartEdit.PropertyBrowser {
    public class PropertyField {
        public PropertyField() { }
        public virtual string GetName() { return ""; }
        public virtual void SetName(string Name) { }
        public virtual string GetGroup() { return ""; }
        public virtual void SetGroup(string Name) { }
        public virtual void ApplyChanges() { }
        public virtual void UndoChanges() { }

    }
}
