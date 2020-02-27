namespace ScriptingApp.Core
{
    public class DynamicClassProperties
    {
        public string PropertyName { get; set; }

        public object PropertyType { get; set; }

        public string ClassName { get; set; }

        public DynamicClassProperties ParentNode { get; set; }

        public bool IsParent { get; set; }

        public string GetFullClassName()
        {
            return ParentNode != null ? $"{ParentNode.GetFullClassName()}_{ClassName}" : ClassName;
        }
    }
}
