namespace ScriptingApp.Core
{
    public class DynamicClassProperties
    {
        private string propertyName;
        public string PropertyName { get { return propertyName.Replace('-', '_'); } set { propertyName = value; } }

        public object PropertyType { get; set; }

        public string ClassName { get; set; }

        public DynamicClassProperties ParentNode { get; set; }

        public bool IsParent { get; set; }

        public string GetFullClassName()
        {
            return (ParentNode != null ? $"{ParentNode.GetFullClassName()}_{ClassName}" : ClassName).Replace('-', '_');
        }
    }
}
