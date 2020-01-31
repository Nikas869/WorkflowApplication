using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JdSuite.Common.Module;



namespace JdSuite.Common
{
    /// <summary>
    /// This class is used to validate xml data nodes, using Field schema
    /// </summary>
    public class XMLNodeValidator
    {
               
        const string TypeAttribute = "Attribute";

        public List<string> ErrorList { get; private set; } = new List<string>();

        NLog.ILogger logger = NLog.LogManager.GetLogger("XMLNodeValidator");

        //XElement RootDataNode = null;
        //Field RootSchemaNode = null;


        public bool CheckOptionality(Field schemaNode, XElement parentDataNode)
        {

            bool isValid = true;
            var dataNodeCount = parentDataNode.Elements().Where(x => string.Compare(x.Name.LocalName, schemaNode.Name, StringComparison.OrdinalIgnoreCase) == 0).Count();

            int lineNo = (parentDataNode as IXmlLineInfo)?.LineNumber ?? -1;

            string parent = "";
            if (schemaNode.Parent != null)
            {
                parent = schemaNode.Parent.Name;
                if (schemaNode.Parent.Parent != null)
                    parent = schemaNode.Parent.Parent.Name + "/" + parent;
            }

            if (string.Compare(schemaNode.Optionality, JdSuite.Common.Optionality.One, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (dataNodeCount == 1)
                {
                    isValid = true;
                    goto lblReturn;
                }

                ErrorList.Add($"Node {parent + "/" + schemaNode.Name} violates optionality rule {schemaNode.Optionality} LineNo [{lineNo}]");
                isValid = false;
                goto lblReturn;
            }

            if (string.Compare(schemaNode.Optionality, JdSuite.Common.Optionality.One_Or_More, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (dataNodeCount >= 1)
                {
                    isValid = true;
                    goto lblReturn;
                }

                ErrorList.Add($"Node {parent + "/" + schemaNode.Name} violates optionality rule {schemaNode.Optionality} LineNo [{lineNo}]");
                isValid = false;
                goto lblReturn;
            }

            if (string.Compare(schemaNode.Optionality, JdSuite.Common.Optionality.Zero_OR_One, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (dataNodeCount <= 1)
                {
                    isValid = true;
                    goto lblReturn;
                }
                ErrorList.Add($"Node {parent + "/" + schemaNode.Name} violates optionality rule {schemaNode.Optionality} LineNo [{lineNo}]");
                isValid = false;
                goto lblReturn;

            }

        //No need to check this
        /*
        if (schemaNode.Use == ZeroOrMore && dataNodeCount > 1)
        {
            ErrorList.Add($"Node {currentXNode.Name} violates optionality rule {schemaNode.Use}");
            return false;
        }
        */

        lblReturn:

            logger.Trace($"{schemaNode.Name}.[{schemaNode.Optionality}] TargetCount {dataNodeCount}=>{isValid}");

            return isValid;
        }

        public bool Validate(Field rootSchemaNode, XElement rootDataNode)
        {
            bool IsValid = false;

            if(rootDataNode==null)
            {
                logger.Warn("XMLData node is null");
            }

            logger.Trace("Validating nodes schemaNode [{0}] DataNode [{1}]", rootSchemaNode.Name, rootDataNode.Name.LocalName);

            if (string.Compare(rootSchemaNode.Name, rootDataNode.Name.LocalName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                int lineNo = (rootDataNode as IXmlLineInfo)?.LineNumber ?? -1;
                ErrorList.Add(string.Format("SchemaNode.Name [{0}] != DataNode.Name [{1}] LineNo [{2}]", rootSchemaNode.Name, rootDataNode.Name,lineNo));
                return false;
            }

            List<string> extraDataNodes = new List<string>();

            var schemaNameList = rootSchemaNode.ChildNodes.Select(x => x.Name.ToLower()).ToList();
            var dataNameList = rootDataNode.Elements().Select(x => x.Name.LocalName.ToLower()).ToList();

            var difference = dataNameList.Except(schemaNameList).ToList();
            var csvlist = string.Join(",", difference);

            if (difference.Count > 0)
            {
                int lineNo = (rootDataNode as IXmlLineInfo)?.LineNumber ?? -1;
                ErrorList.Add($"Node:[{rootDataNode.Name}] has extra child nodes [{csvlist}] which are not found in schema LineNo [{lineNo}]");
                return false;
            }

            foreach (var schemaNode in rootSchemaNode.ChildNodes)
            {
                if (schemaNode.Type == TypeAttribute)
                    continue;

                logger.Trace("Validating ChildSchemaNode schemaNode [{0}] DataNode [{1}]", schemaNode.Name, rootDataNode.Name.LocalName);


                IsValid = CheckOptionality(schemaNode, rootDataNode);
                if (IsValid == false)
                {
                    return false;
                }


                foreach (var childDataNode in rootDataNode.Elements().Where(x => string.Compare(x.Name.LocalName.ToLower(), schemaNode.Name.ToLower(), StringComparison.OrdinalIgnoreCase) == 0))
                {
                    foreach (var attrSchema in schemaNode.ChildNodes.Where(x => x.Type == TypeAttribute))
                    {
                        IsValid = ValidateAttr(attrSchema, childDataNode);
                        if (IsValid == false)
                            return false;
                    }

                    if (schemaNode.ChildNodes.Count == 0)
                    {
                        string svalue = childDataNode.Value;
                        IsValid = DataTypeChecker.IsValid(schemaNode, svalue);
                        if (IsValid == false)
                        {
                            string parentName = "";
                            if (schemaNode.Parent != null)
                            {
                                parentName = schemaNode.Parent.Name;
                            }

                            int lineNo = (childDataNode as IXmlLineInfo)?.LineNumber ?? -1;

                            ErrorList.Add($"Node [{parentName}/{schemaNode.Name}] value [{svalue}] DataType is not correct Expected DataType {schemaNode.DataType} LineNo [{lineNo}]");
                            return false;
                        }
                    }

                    IsValid = Validate(schemaNode, childDataNode);
                    if (IsValid == false)
                        return false;
                }
            }

            return true;
        }

        public bool ValidateAttr(Field attrSchemaNode, XElement dataNode)
        {
            bool isValid = true;

            var attrList = dataNode.Attributes().Where(x => string.Compare(x.Name.LocalName, attrSchemaNode.Name, StringComparison.OrdinalIgnoreCase) == 0).ToList();

            var itemCount = attrList.Count();

            int lineNo = (dataNode as IXmlLineInfo)?.LineNumber ?? -1;

            string parent = "";
            if (attrSchemaNode.Parent != null)
            {
                parent = attrSchemaNode.Parent.Name;
                if (attrSchemaNode.Parent.Parent != null)
                    parent = attrSchemaNode.Parent.Parent.Name + "/" + parent;
            }

            if (string.Compare(attrSchemaNode.Optionality, JdSuite.Common.Optionality.One, StringComparison.OrdinalIgnoreCase) == 0)
            {
                

                if (itemCount == 1)
                {
                    isValid = true;

                    string attrValue = attrList.FirstOrDefault().Value;

                    if (!DataTypeChecker.IsValid(attrSchemaNode, attrValue))
                    {
                       

                        ErrorList.Add($"Node attribute <{parent}[{attrSchemaNode.Name}]> value [{attrValue}] DataType is not correct Expected DataType {attrSchemaNode.DataType} LineNo [{lineNo}]");
                        isValid = false;
                    }

                    goto lblReturn;
                }

                 lineNo = (dataNode as IXmlLineInfo)?.LineNumber ?? -1;
                ErrorList.Add($"Node {parent} violates attribute [{attrSchemaNode.Name}] optionality rule {attrSchemaNode.Optionality} LineNo [{lineNo}]");
                isValid = false;
                goto lblReturn;
            }

            if (string.Compare(attrSchemaNode.Optionality, JdSuite.Common.Optionality.One_Or_More, StringComparison.OrdinalIgnoreCase) == 0)
            {
                
                if (itemCount >= 1)
                {
                    isValid = true;

                    string attrValue = attrList.FirstOrDefault().Value;
                    if (!DataTypeChecker.IsValid(attrSchemaNode, attrValue))
                    {
                        ErrorList.Add($"Node attribute <{parent}[{attrSchemaNode.Name}]> value [{attrValue}] DataType is not correct Expected DataType {attrSchemaNode.DataType} LineNo [{lineNo}]");
                        isValid = false;
                    }

                    goto lblReturn;
                }

                ErrorList.Add($"Node {parent} violates attribute [{attrSchemaNode.Name}] optionality rule {attrSchemaNode.Optionality} LineNo [{lineNo}]");
                isValid = false;
                goto lblReturn;
            }

            if (string.Compare(attrSchemaNode.Optionality, JdSuite.Common.Optionality.Zero_OR_One, StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (itemCount == 0)
                {
                    isValid = true;

                    goto lblReturn;
                }

                if (itemCount == 1)
                {

                    string attrValue = attrList.FirstOrDefault().Value;
                    if (!DataTypeChecker.IsValid(attrSchemaNode, attrValue))
                    {
                        ErrorList.Add($"Node attribute <{parent}[{attrSchemaNode.Name}]> value [{attrValue}] DataType is not correct Expected DataType {attrSchemaNode.DataType} LineNo [{lineNo}]");
                        isValid = false;
                    }
                    else
                    {
                        isValid = true;
                    }

                    goto lblReturn;
                }


                ErrorList.Add($"Node {parent} violates attribute [{attrSchemaNode.Name}] optionality rule {attrSchemaNode.Optionality} LineNo [{lineNo}]");
                isValid = false;
                goto lblReturn;

            }

        //No need to check this
        /*
        if (schemaNode.Use == ZeroOrMore && dataNodeCount > 1)
        {
            ErrorList.Add($"Node {currentXNode.Name} violates optionality rule {schemaNode.Use}");
            return false;
        }
        */

        lblReturn:

            logger.Trace($"{parent}[{attrSchemaNode.Name}].<{attrSchemaNode.Optionality}> TargetCount {itemCount}=>{isValid}");

            return isValid;


        }

        public bool ValidateInternal(Field schemaNode, XElement dataNode)
        {
            bool IsValid = false;

            try
            {


                IsValid = CheckOptionality(schemaNode, dataNode);
                if (!IsValid)
                {
                    return false;
                }

                foreach (var childSchemaNode in schemaNode.ChildNodes)
                {

                    if (CheckOptionality(childSchemaNode, dataNode) == false)
                        return false;

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return true;
        }



        private bool ValidateInternal(DataItem dataNode, Field schemaNode)
        {
            bool isValid = true;
            Field ff_current = schemaNode;
            DataItem data_current = dataNode;

            if (!ff_current.Name.Equals(data_current.Name, StringComparison.OrdinalIgnoreCase))
            {
                ErrorList.Add($"Expected XMLNode {ff_current.Name} found {data_current.Name} Line:{dataNode.LineNo}");
                isValid = false;
            }

            //logger.Info($"Validating {data_current.Name} properties");

            foreach (var ff_prop in ff_current.GetProps())
            {
                var data_prop = data_current.Props.FirstOrDefault(x => x.Name.Equals(ff_prop.Name, StringComparison.OrdinalIgnoreCase));
                if (data_prop == null)
                {
                    ErrorList.Add($"Expected XMLNode {ff_current.Name}/{ff_prop.Name} is missing in {data_current.Name} Line:{data_current.LineNo}");
                    isValid = false;
                }
                else
                {
                    if (!DataTypeChecker.IsValid(ff_prop, data_prop.Value))
                    {
                        ErrorList.Add($"Expected XMLNode {ff_current.Name}/{ff_prop.Name} value type mismatch Type:{ff_prop.Type}[{data_prop.Value}] Line:{data_prop.LineNo}");
                    }
                }
            }



            foreach (var ff_child in ff_current.GetContainerChild())
            {
                // logger.Info($"Validating child {ff_child.Name}");

                try
                {
                    if (data_current.ChildCount == 0)
                    {
                        ErrorList.Add($"Expected XMLNode <{ff_child.Name}> is missing in <{data_current.Name}> Line:{data_current.LineNo}");
                        isValid = false;
                        var item = data_current.AddChild(new DataItem(ff_child.Name, ff_child.Optionality, ff_child.Type,ff_child.DataType, ""));
                        item.Props = new System.Collections.ObjectModel.ObservableCollection<DataProperty>();
                        item.Children = new System.Collections.ObjectModel.ObservableCollection<DataItem>();

                        break;
                    }

                lblCheckAgain:
                    var data_child = data_current.Children.FirstOrDefault(x => x.Name.Equals(ff_child.Name, StringComparison.OrdinalIgnoreCase));
                    if (data_child == null)
                    {
                        ErrorList.Add($"Expected XMLNode <{ff_child.Name}> is missing in <{data_current.Name}> Line:{data_current.LineNo}");
                        var item = data_current.AddChild(new DataItem(ff_child.Name, ff_child.Optionality, ff_child.Type,ff_child.DataType,""));
                        item.IsDataNodeMissing = true;
                        item.Props = new System.Collections.ObjectModel.ObservableCollection<DataProperty>();
                        item.Children = new System.Collections.ObjectModel.ObservableCollection<DataItem>();

                        isValid = false;
                        goto lblCheckAgain;
                    }
                    else
                    {
                        if (ff_child.ChildNodes != null)
                        {
                            ValidateInternal(data_child, ff_child);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorList.Add($"Child Node <{ff_child.Name}> validation error <{data_current.Name}> Line:{data_current.LineNo} {ex.Message}");
                }
            }

            return isValid;


        }

        public void LogError()
        {
            logger.Error($"XML validation errors: {string.Join(Environment.NewLine, ErrorList)} ");
        }

        public void LogEachError()
        {
            foreach (var err in ErrorList)
            {
                logger.Error(err);
            }
        }
    }
}
