using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using JdSuite.Common.Module;

namespace JdSuite.Common
{
    /// <summary>
    /// Instances of this class are used to hold data from xml data file for tree list control nodes. Each node is binded with one DataItem object.
    /// </summary>
    /// 

    public class DataProperty : ViewModelBase
    {
        private string _name = "";
        private string _type = "";
        private string _value = "";
        private int _level = 0;
        private int _lineNo = 0;
        private int _position = 0;


        public int LineNo
        {
            get { return _lineNo; }
            set { SetPropertry(ref _lineNo, value); }
        }

        public int Position
        {
            get { return _position; }
            set { SetPropertry(ref _position, value); }
        }

        public int Level
        {
            get { return _level; }
            set { SetPropertry(ref _level, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetPropertry(ref _name, value); }
        }


        public string Type
        {
            get { return _type; }
            set { SetPropertry(ref _type, value); }
        }


        public string Value
        {
            get { return _value; }
            set { SetPropertry(ref _value, value); }
        }


        public DataProperty(string nodeName, string targetType, string nodeValue)
        {
            this._name = nodeName;
            this._type = targetType;
            this._value = nodeValue;
        }

        public override string ToString()
        {
            return $"Prop[Name:{Name}, Type:{Type}, Value:{Value}]";
        }
    }


    public class DataItem : ViewModelBase
    {
        static readonly NLog.ILogger logger = NLog.LogManager.GetLogger("DataItem");

        private string _name;
        private string _type;
        private string _value;
        private string _optionality;

        private int _level;
        private int _reverseLevel;
        private int _id;

        public int LineNo;
        public int Position;
        // private ObservableCollection<DataProperty> _props;
        private ObservableCollection<DataItem> _children;
        private ObservableCollection<DataProperty> _props;

        public TextBlock NameBlock { get; set; }
        public TextBlock TypeBlock { get; set; }
        public TextBlock ValueBlock { get; set; }

        public Grid ParentGrid { get; set; }

        public Object VisualTreeNode { get; set; }

        public Field Schema { get; set; }
        public int RecordNo { get; set; }

        private int _totalRecordCount = 0;

        public int TotalRecordCount { get { return _totalRecordCount; } set { SetPropertry(ref _totalRecordCount, value); } }

        public DataItem()
        {

        }


        public bool IsDataNodeMissing { get; set; } = false;
        public bool IsProperty { get; set; } = true;

        public DataItem(string name, string optionality, string type, string datatype, string svalue)
        {
            _name = name;
            _type = type;
            _value = svalue;
            _dataType = datatype;
        }

        public int Id
        {
            get { return _id; }
            set { SetPropertry(ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetPropertry(ref _name, value); }
        }


        public string Type
        {
            get { return _type; }
            set { SetPropertry(ref _type, value); }
        }

        private string _dataType = "String";
        public string DataType
        {
            get { return _dataType; }
            set { SetPropertry(ref _dataType, value); }
        }


        public string Value
        {
            get { return _value; }
            set { SetPropertry(ref _value, value); }
        }

        public string Optionality
        {
            get { return _optionality; }
            set { SetPropertry(ref _optionality, value); }
        }


        public int Level
        {
            get { return _level; }
            set { SetPropertry(ref _level, value); }
        }


        public int ReverseLevel
        {
            get { return _reverseLevel; }
            set { SetPropertry(ref _reverseLevel, value); }
        }

        /// <summary>
        /// Number of child nodes
        /// </summary>
        public int ChildCount
        {
            get { return _children?.Count ?? 0; }
        }

        /// <summary>
        /// Number of properties
        /// </summary>
        public int PropCount
        {
            get { return _props?.Count ?? 0; }
        }


        public ObservableCollection<DataProperty> Props
        {
            get { return _props; }
            set
            {
                SetPropertry(ref _props, value);
            }
        }



        public ObservableCollection<DataItem> Children
        {
            get { return _children; }
            set
            {

                SetPropertry(ref _children, value);
            }
        }


        /// <summary>
        /// Sets child level=Parent level +1
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public DataItem AddChild(DataItem child)
        {
            if (_children == null)
            {
                _children = new ObservableCollection<DataItem>();
            }
            child.Level = this.Level + 1;
            Children.Add(child);

            return child;
        }

        public void Load(XElement xElement, Field schema, ref int maxLevel)
        {

            this.Schema = schema;
            this.Name = schema.Name;
            this.Type = schema.Type;
            this.DataType = schema.DataType;


            logger.Trace("Processing_Schema [{0}]", schema.ToString());

            foreach (var schemaChild in schema.ChildNodes)
            {
                var xDataChildren = xElement.Elements().Where(x => string.Compare(x.Name.LocalName, schemaChild.Name, true) == 0);

                logger.Trace("Processing_Child_Schema [{0}] DataCount [{1}]", schemaChild.ToString(), xDataChildren.Count());

                if (xDataChildren.Count() == 0)
                {
                    DataItem child = AddChild(new DataItem(schemaChild.Name, schemaChild.Optionality, schemaChild.Type,schemaChild.DataType, "N/A"));

                    child.Load(new XElement(schemaChild.Name), schemaChild, ref maxLevel);
                    child.IsDataNodeMissing = true;
                    child.IsProperty = !schemaChild.HasChildNodes();
                    child.Value = "";
                    child.TotalRecordCount = 0;
                    logger.Trace("Added_Child_DataItem Schema: [{0}] DataItem: [{1}]", schemaChild.ToString(), child.ToString());
                }
                else
                {
                    bool isFirst = true;

                    foreach (var xDataNode in xDataChildren)
                    {
                        DataItem child = AddChild(new DataItem(schemaChild.Name, schemaChild.Optionality, schemaChild.Type,schemaChild.DataType, ""));
                        if(isFirst)
                        {
                            child.TotalRecordCount = xDataChildren.Count();
                            isFirst = false;
                        }
                        child.Load(xDataNode, schemaChild, ref maxLevel);
                        child.IsDataNodeMissing = false;
                        child.IsProperty = !schemaChild.HasChildNodes();
                        if (child.IsProperty)
                        {
                            child.Value = xDataNode.Value;
                        }
                        logger.Trace("Added_Child_DataItem2 Schema: [{0}] DataItem: [{1}]", schemaChild.ToString(), child.ToString());
                    }
                }
            }

        }

        public bool ValidateNodeDataType(string nodeName, string targetType, string nodeValue)
        {


            //Validate xml element value
            bool isValid = true;

            if (targetType == "Int16")
            {
                if (!Int16.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Int32")
            {
                if (!int.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }

            else if (targetType == "Int64")
            {
                if (!Int64.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Boolean" || targetType == "bool")
            {
                if (!bool.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Double")
            {
                if (!double.TryParse(nodeValue, out var intVal))
                {
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Single")
            {
                if (!Single.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }
            else if (targetType == "Date/Time")
            {
                if (!DateTime.TryParse(nodeValue, out var intVal))
                {
                    isValid = false;
                    logger.Error($"Node {nodeName} Value {nodeValue} is invalid Required {targetType} ");
                }
            }

            return isValid;
        }

        public override string ToString()
        {
            return $"DataItem[Name:{Name}, Value:{Value}, IsProp:{IsProperty}, Id:{Id}, ChildCount:{ChildCount}]";
        }



        /*
        public void Load(XElement xElement, Field schema, ref int maxLevel)
        {

            try
            {

                if (xElement == null)
                {
                    logger.Error("XElement is null");
                    return;
                }


                var nodeName = xElement.Name.LocalName;

                if (schema == null)
                {
                    logger.Error($"Schema node for xml data node {nodeName} is null");
                    return;
                }

                var nextSchemaNode = schema;
                if (!nextSchemaNode.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    nextSchemaNode = schema.ChildNodes.FirstOrDefault(x => x.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
                }

                if (nextSchemaNode == null)
                {
                    logger.Error($"Schema node for xml data node {nodeName} is null");
                    return;
                }


                var targetType = nextSchemaNode.DataType;
                targetType = targetType.Replace("xs:", "");

                string nodeValue = "";
                if (xElement.HasElements)
                    nodeValue = "Array";
                else
                    nodeValue = xElement.Value;

                //Validate xml element value
                if (nodeValue != "Array")
                    ValidateNodeDataType(nodeName, targetType, nodeValue);


                IXmlLineInfo lineInfo = xElement as IXmlLineInfo;
                if (lineInfo != null)
                {
                    LineNo = lineInfo.LineNumber;
                    Position = lineInfo.LinePosition;
                }
                Name = nodeName;
                Type = targetType;
                Value = "Array";// element.Value;


                foreach (var childXelement in xElement.Elements())
                {
                    if (childXelement.HasElements)
                    {
                        var childNode = AddChild(new DataItem());



                        childNode.Load(childXelement, nextSchemaNode, ref maxLevel);

                        lineInfo = childXelement as IXmlLineInfo;
                        if (lineInfo != null)
                        {
                            childNode.LineNo = lineInfo.LineNumber;
                            childNode.Position = lineInfo.LinePosition;
                        }

                        if (maxLevel < childNode.Level)
                        {
                            maxLevel = childNode.Level;
                        }
                    }
                    else
                    {
                        AddProp(childXelement, nextSchemaNode);
                    }
                }
                if (ReverseLevel < maxLevel)
                {
                    ReverseLevel = maxLevel;
                }

                // Debug.WriteLine("ReverseLevel = " + dataItem.ReverseLevel);

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"XElement_processing_error {xElement.Name.LocalName} Err:{ex.Message}");
            }
        }
        

        private void AddProp(XElement element, Field schema)
        {
            if (element == null)
            {
                logger.Error($"XElement is null");
                return;
            }

            var nodeName = element.Name.LocalName;

            var schemaNode = schema;
            if (schemaNode.Name != nodeName)
            {
                schemaNode = schema.ChildNodes.FirstOrDefault(x => x.Name == nodeName);
            }


            if (schemaNode == null)
            {
                logger.Error($"Schema node for XElement {nodeName} is null");
                return;
            }

            var targetType = schemaNode.DataType;
            targetType = targetType.Replace("xs:", "");


            string nodeValue = element.Value;

            if (element.HasElements)
                nodeValue = "Array";

            if (nodeValue != "Array")
                ValidateNodeDataType(nodeName, targetType, nodeValue);

            var propItem = new DataProperty(nodeName, targetType, nodeValue);
            var lineInfo = element as IXmlLineInfo;
            if (lineInfo != null)
            {
                propItem.LineNo = lineInfo.LineNumber;
                propItem.Position = lineInfo.LinePosition;
            }
            AddProp(propItem);
        }


        
        public void AddProp(DataProperty prop)
        {
            if (_props == null)
            {
                _props = new ObservableCollection<DataProperty>();
            }
            prop.Level = this.Level + 1;
            Props.Add(prop);
        }


        */


    }
}
