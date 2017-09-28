using System;
using UnityEditor;

namespace RetroLooks
{
    public class RetroLooksModelEditorAttribute : Attribute
    {
        public readonly Type type;
        public readonly bool alwaysEnabled;

        public RetroLooksModelEditorAttribute(Type type, bool alwaysEnabled = false)
        {
            this.type = type;
            this.alwaysEnabled = alwaysEnabled;
        }
    }
}
