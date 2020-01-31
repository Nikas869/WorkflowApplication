using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JdSuite.Common.Module
{

    /// <summary>
    /// This class should be used by all modules which need to load/save and process xml schema nodes
    /// 
    /// </summary>
    public class Field : ViewModelBase
    {


        [XmlIgnore]
        private bool _isSelected = false;

        [XmlIgnore]
        private string _type;

        [XmlIgnore]
        private string _name;

        [XmlIgnore]
        private string _dataType;

        [XmlIgnore]
        private string _optionality;

        [XmlIgnore]
        private string _change;

        [XmlIgnore]
        private string _alias;

        [XmlIgnore]
        public bool IsValidated { get; set; }

        [XmlIgnore]
        public Field Parent { get; set; }


        [XmlIgnore]
        public bool IsSelected { get { return _isSelected; } set { SetPropertry(ref _isSelected, value); } }


        [XmlAttribute]
        public String Type
        {
            get { return _type; }
            set { SetPropertry(ref _type, value); }
        }

        [XmlAttribute]
        public String Name { get { return _name; } set { SetPropertry(ref _name, value); } }

        [XmlAttribute]
        public String DataType { get { return _dataType; } set { SetPropertry(ref _dataType, value); } }

        [XmlAttribute]
        public String Optionality { get { return _optionality; } set { SetPropertry(ref _optionality, value); } }

        [XmlAttribute]
        public String Change { get { return _change; } set { SetPropertry(ref _change, value); } }

        [XmlAttribute]
        public String Alias { get { return _alias; } set { SetPropertry(ref _alias, value); } }

        [XmlIgnore]
        public string XPath { get; private set; }

        [XmlElement("Field")]
        public List<Field> ChildNodes { get; set; } = new List<Field>();

        public bool HasChildNodes()
        {
            return ChildNodes.Any(x => x.ChildNodes != null || x.ChildNodes.Count > 0);
        }

        public IEnumerable<Field> GetProps()
        {
            return ChildNodes.Where(x => x.ChildNodes == null || x.ChildNodes.Count == 0);
        }

        public IEnumerable<Field> GetContainerChild()
        {
            return ChildNodes.Where(x => x.ChildNodes == null || x.ChildNodes.Count > 0);
        }

        public void Save(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (StreamWriter writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static void SetParent(Field field)
        {
            foreach (var child in field.ChildNodes)
            {
                child.Parent = field;
                SetParent(child);
            }
        }

        /// <summary>
        /// This method computes XPath only for current node
        /// </summary>
        public void ComputeXPath()
        {

            string xpath = "";
            Field node = this;
            while(node !=null)
            {
                xpath = node.Name + "/" + xpath;
                node = node.Parent;
            }

            XPath = xpath;
        }

        /// <summary>
        /// This method computes XPath for full graph
        /// </summary>
        /// <param name="field"></param>
        public static void ComputeXPath(Field field)
        {
            field.ComputeXPath();
            foreach(var child in field.ChildNodes)
            {
                ComputeXPath(child);
            }
        }

        public string GetXPath(Field root)
        {
            Field passField = this;
            bool bFound = false;
            List<Field> chain = new List<Field>();

            Field.FindChain(root, ref passField, ref bFound, ref chain);

            chain.Reverse();
            string xpath = "";
            if (bFound)
            {
                xpath = "/" + string.Join("/", chain.Select(x => x.Name));
            }

            return xpath;
        }


        public static void FindChain(Field Parent, ref Field Child, ref bool Found, ref List<Field> Chain)
        {
            if (Found == true)
            {
                if (Parent.ChildNodes.Contains(Chain.Last()))
                    Chain.Add(Parent);
                return;
            }

            if (Parent.ChildNodes == null || Parent.ChildNodes.Count == 0)
                return;

            if (Parent.ChildNodes.Contains(Child))
            {
                Chain.Add(Child);
                Chain.Add(Parent);
                Found = true;
                return;
            }

            for (int i = 0; i < Parent.ChildNodes.Count; i++)
            {
                FindChain(Parent.ChildNodes[i], ref Child, ref Found, ref Chain);
                if (Found)
                {
                    if (Parent.ChildNodes.Contains(Chain.Last()))
                        Chain.Add(Parent);
                    break;
                }

            }


        }

        /// <summary>
        /// Loads xml schema fields into Field data structure
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Field Parse(String filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Field));
            Field root = null;

            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                root = (Field)serializer.Deserialize(reader);
            }

            return root;

        }

        public override string ToString()
        {
            return $"Field [Name:{Name}, Type:{Type}, DataType:{DataType}, Use:{Optionality}, Change:{Change}, Alias:{Alias}]";
        }


    }
}
