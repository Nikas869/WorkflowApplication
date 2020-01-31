using System.Collections.ObjectModel;
using System.ComponentModel;
using JdSuite.DataFilter.Models.Filters;
using JdSuite.Common.Module;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;
using JdSuite.Common;

namespace JdSuite.DataFilter.ViewModels
{
    /// <summary>
    /// Very important class used in UI, Filter grid on UI bindes controls with this object.
    /// This same object is serialized to .flo file
    /// Objects of this class are used in actual xml filtering process
    /// </summary>
    public class FilterField : ViewModelBase
    {
        [XmlIgnore]
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(FilterField));

        [XmlIgnore]
        private int _id;
        [XmlIgnore]
        private string _fieldName;
        [XmlIgnore]
        private string _fieldType;
        [XmlIgnore]
        private string _condition;
        [XmlIgnore]
        private string _conditionValue;

        [XmlIgnore]
        private string _joins;

        [XmlIgnore]
        private string _join;

        /// <summary>
        /// (1,and);(2,or)
        /// </summary>
        [XmlAttribute]
        public string Joins
        {
            get { return _joins; }
            set
            {
                 
                    SetPropertry(ref _joins, value);
                
            }
        }


        /// <summary>
        /// Either and or or
        /// </summary>
        [XmlAttribute]
        public string JoinType
        {
            get { return _join; }
            set { SetPropertry(ref _join, value); }
        }

        [XmlAttribute]
        public int Id { get { return _id; } set { SetPropertry(ref _id, value); } }

        [XmlAttribute]
        public string FieldName { get { return _fieldName; } set { SetPropertry(ref _fieldName, value); } }

        [XmlAttribute]
        public string FieldType { get { return _fieldType; } set { SetPropertry(ref _fieldType, value); } }

        [XmlAttribute]
        public string Condition { get { return _condition; } set { SetPropertry(ref _condition, value); } }

        [XmlAttribute]
        public string ConditionValue { get { return _conditionValue; } set { SetPropertry(ref _conditionValue, value); } }

        [XmlAttribute]
        public string XPath { get; set; }

        [XmlIgnore]
        public string ParentXPath
        {
            get { return XPath.Substring(0, XPath.LastIndexOf('/')); }
        }

        [XmlAttribute]
        public string XPathCondition { get; set; }

        public void SetXPathCondition()
        {
            logger.Trace("Setting XPathCondition for {0}", FieldName);

            if (FieldType.ToLower() == "string")
            {
                //"=","!=", "Contains", "Starts With", "Ends With" ,"Matches"
                //  "/Invoices/Invoice/items/item[./Description[contains(text(),'23')]]";

                string strcond = "";

                if (Condition == "Contains")
                {
                    strcond = "[//" + FieldName + $"[{ConditionExpr()}]]";
                }
                else if (Condition == "Starts With")
                {
                    strcond = "[//" + FieldName + $"[{ConditionExpr()}]]";
                }
                else if (Condition == "Ends With")
                {
                    strcond = "[//" + FieldName + $"[{ConditionExpr()}]]";
                }
                else if (Condition == "Matches")
                {
                    strcond = "[//" + FieldName + $"[{ConditionExpr()}]]";
                }
                else if (Condition == "=" || Condition == "!=")
                {
                    strcond = $"[{ConditionExpr()}]";
                }

                var temp =ParentXPath;

                string filter = temp + strcond;

                XPathCondition = filter;
            }
            else
            {
                //"/Invoices/Invoice/items/item[ItemNumber>=100]";

                var temp = ParentXPath;
                string filter = temp + $"[{ConditionExpr()}]";
                XPathCondition = filter;
            }

            logger.Trace("FilterField {0} XPathCondition={1}", FieldName, XPathCondition);
        }

        public string ConditionExpr()
        {
            //category[@name='Sport' and author[starts-with(.,'James Small')]]

            string strcond = "";

            if (FieldType.ToLower() == "string")
            {
                if (Condition == "Contains")
                {
                    strcond = $"contains(text(),'{ConditionValue}')";
                }
                else if (Condition == "Starts With")
                {
                    strcond = $"starts-with(text(),'{ConditionValue}')";
                }
                else if (Condition == "Ends With")
                {
                    strcond = $"ends-with(text(),'{ConditionValue}')";
                }
                else if (Condition == "Matches")
                {
                    strcond = $"matches(text(),'{ConditionValue}')";
                }
                else if (Condition == "=" || Condition == "!=")
                {
                    strcond = $"{FieldName}{Condition}'{ConditionValue}'";
                }

            }
            else
            {
                strcond = $"{FieldName}{Condition}{ConditionValue}";
            }

            return strcond;
        }

        public string ConditionExpr2()
        {
            //category[@name='Sport' and author[starts-with(.,'James Small')]]

            string strcond = "";

            if (FieldType.ToLower() == "string")
            {
                if (Condition == "Contains")
                {
                    strcond = $"{FieldName}[contains(text(),'{ConditionValue}')]";
                }
                else if (Condition == "Starts With")
                {
                    strcond = $"{FieldName}[starts-with(text(),'{ConditionValue}')]";
                }
                else if (Condition == "Ends With")
                {
                    strcond = $"{FieldName}[ends-with(text(),'{ConditionValue}')]";
                }
                else if (Condition == "Matches")
                {
                    strcond = $"{FieldName}[matches(text(),'{ConditionValue}')]";
                }
                else if (Condition == "=" || Condition == "!=")
                {
                    strcond = $"{FieldName}{Condition}'{ConditionValue}'";
                }

            }
            else
            {
                strcond = $"{FieldName}{Condition}{ConditionValue}";
            }

            return strcond;
        }

        /// <summary>
        /// Parses Joins property //(1,and);(2,or) and returns Field Id, Join Type list
        /// </summary>
        /// <returns></returns>
        public List<Tuple<int,string>> GetJoins()
        {
            //(1,and);(2,or)
            List<Tuple<int, string>> lst = new List<Tuple<int, string>>();
            var parts = Joins.Split(';');
            foreach (var part in parts)
            {
                var pp = part.Split(',');
                pp[0] = pp[0].Replace("(", "").Trim();
                pp[1] = pp[1].Replace(")", "").Trim();

                lst.Add(Tuple.Create(int.Parse(pp[0]), pp[1]));
            }

            return lst;
        }
    }
}
