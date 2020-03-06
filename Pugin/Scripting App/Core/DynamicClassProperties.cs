namespace ScriptingApp.Core
{
    public class DynamicClass
    {
        private string propertyName;
        public string PropertyName { get { return propertyName.Replace('-', '_'); } set { propertyName = value; } }

        public object PropertyType { get; set; }

        public string ClassName { get; set; }

        public DynamicClass ParentNode { get; set; }

        public bool IsParent { get; set; }
        public XMLType XmlType { get; set; }

        public string GetFullClassName()
        {
            return (ParentNode != null ? $"{ParentNode.GetFullClassName()}_{ClassName}" : ClassName).Replace('-', '_');
        }
    }

    public enum XMLType
    {
        Element,
        Attribute,
        PCData
    }
}
