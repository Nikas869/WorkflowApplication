using JdSuite.Common.FileProcessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace JdSuite.DataSorting
{
    public class XMLSorter
    {
        NLog.ILogger logger = NLog.LogManager.GetLogger(nameof(XMLSorter));

        private WorkflowFile workflowFile;
        public List<SortingField> SortingFields { get; private set; } = new List<SortingField>();

        public XMLSorter(WorkflowFile dataFile, IEnumerable<SortingField> sortingFields)
        {
            workflowFile = dataFile;
            SortingFields.AddRange(sortingFields);
        }

        public void Sort()
        {
            logger.Info("Entered");
            try
            {
                if (SortingFields.Count == 0)
                {
                    logger.Warn("No sorting field is found to sort data, please set a sorting field first");
                    throw new InvalidOperationException("No sorting field is found to sort data, please set a sorting field first");
                }

                SortingField rootSF = SortingFields[0];//xpath is null form grid

                var endElement = workflowFile.RootNode.Descendants(rootSF.GetLeafName()).First();
                var parentElement = endElement.Parent;
                var grandParentElement = parentElement.Parent;


                if (grandParentElement == null)
                {
                    TwoLevelSort(rootSF);
                }
                else
                {
                    MultiLevelSort(rootSF, grandParentElement, parentElement);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "XML Sorting Error, DataFile {0}", workflowFile.FilePath);
                throw ex;
            }
            logger.Info("Leaving");
        }

        /// <summary>
        /// It is used to sort root/child/grand_child or a more deeper hieararchy
        /// </summary>
        /// <param name="rootSF"></param>
        /// <param name="grandParentElement"></param>
        /// <param name="parentElement"></param>
        private void MultiLevelSort(SortingField rootSF, XElement grandParentElement, XElement parentElement)
        {
            logger.Info("Sorting using xpath {0}", rootSF.XPath);

            if (workflowFile.RootNode == grandParentElement)
            {
                logger.Info("RootNode is GrandParent element case, RootSF XPath {0}", rootSF.XPath);

                // IEnumerable<XElement> grandParentNodes = null;
                IEnumerable<XElement> parentNodes = grandParentElement.Descendants(parentElement.Name);
                IOrderedEnumerable<XElement> orderNodeTree = Process(parentNodes, rootSF);


                for (int i = 1; i < SortingFields.Count; i++)
                {
                    var sf = SortingFields[i];
                    //logger.Info("Sorting using SF {0}", sf.XPath);
                    orderNodeTree = Process(orderNodeTree, sf);
                }

                grandParentElement.ReplaceAll(orderNodeTree);

            }
            else
            {
                logger.Info("RootNode <> GrandParent element case, RootSF XPath {0}", rootSF.XPath);

                var grandParentNodes = workflowFile.RootNode.Descendants(grandParentElement.Name);

                foreach (var gparent in grandParentNodes)
                {

                    IOrderedEnumerable<XElement> orderNodeTree = null;

                    orderNodeTree = Process(gparent.Descendants(parentElement.Name), rootSF);


                    for (int i = 1; i < SortingFields.Count; i++)
                    {
                        var sf = SortingFields[i];
                       // logger.Info("Sorting using SF {0}", sf.XPath);
                        orderNodeTree = Process(orderNodeTree, sf);
                    }

                    gparent.ReplaceAll(orderNodeTree);
                }
            }
            logger.Info("Multi-level sorting completed");

        }

        /// <summary>
        /// It is used to sort root/child only case
        /// </summary>
        /// <param name="rootSF"></param>
        private void TwoLevelSort(SortingField rootSF)
        {

            logger.Info("Sorting using xpath {0}", rootSF.XPath);

            IOrderedEnumerable<XElement> orderNodeTree = null;

            orderNodeTree = rootSF.Process(workflowFile.RootNode);

            var lastElement = ApplyOrder(workflowFile.RootNode, orderNodeTree);

            for (int i = 1; i < SortingFields.Count; i++)
            {
                var sf = SortingFields[i];
                //logger.Info("Sorting using SF {0}", sf.XPath);

                orderNodeTree = sf.Process(workflowFile.RootNode);
                lastElement = ApplyOrderAfter(lastElement, orderNodeTree);
            }

            logger.Info("2-level sorting operation completed");
        }

        /// <summary>
        /// Adds ordered XElements starting as first child of ParentNode and returns last XElement added to tree
        /// </summary>
        /// <param name="ParentNode"></param>
        /// <param name="orderNodeTree"></param>
        /// <returns></returns>
        private XElement ApplyOrder(XElement ParentNode, IOrderedEnumerable<XElement> orderNodeTree)
        {
            bool first = true;
            XElement last = null;
            foreach (var item in orderNodeTree)
            {
                item.Remove();
                if (first)
                {
                    ParentNode.AddFirst(item);
                    last = item;
                    first = false;
                }
                else
                {
                    last.AddAfterSelf(item);
                    last = item;
                }
            }

            return last;
        }

        /// <summary>
        /// Adds ordered XElements after PreviousSibling and returns last XElement added to tree
        /// </summary>
        /// <param name="PreviousSibling"></param>
        /// <param name="orderNodeTree"></param>
        /// <returns></returns>
        private XElement ApplyOrderAfter(XElement PreviousSibling, IOrderedEnumerable<XElement> orderNodeTree)
        {

            XElement last = PreviousSibling;
            foreach (var item in orderNodeTree)
            {
                item.Remove();
                last.AddAfterSelf(item);
                last = item;
            }

            return last;
        }

        /// <summary>
        /// Called by multi level grand_parent/root_node for first level ordering
        /// </summary>
        /// <param name="NodeTree"></param>
        /// <param name="sf"></param>
        /// <returns></returns>
        public IOrderedEnumerable<XElement> Process(IEnumerable<XElement> NodeTree, SortingField sf)
        {

            IOrderedEnumerable<XElement> orderNodeTree;

            string elementName = sf.GetLeafName();

            if (sf.SortingType == SortingType.Ascending)
            {
                orderNodeTree = NodeTree.OrderBy(m => m.Element(elementName).Value, sf.GetComparer());
            }
            else
            {
                orderNodeTree = NodeTree.OrderByDescending(m => m.Element(elementName).Value, sf.GetComparer());
            }

            return orderNodeTree;
        }

        /// <summary>
        /// Called by multi level child nodes for second level ordering
        /// </summary>
        /// <param name="orderNodeTree"></param>
        /// <param name="sf"></param>
        /// <returns></returns>
        public IOrderedEnumerable<XElement> Process(IOrderedEnumerable<XElement> orderNodeTree, SortingField sf)
        {

            string elementName = sf.GetLeafName();


            if (sf.SortingType == SortingType.Ascending)
            {
                orderNodeTree = orderNodeTree.ThenBy(m => m.Element(elementName).Value, sf.GetComparer());
            }
            else
            {
                orderNodeTree = orderNodeTree.ThenByDescending(m => m.Element(elementName).Value, sf.GetComparer());
            }

            return orderNodeTree;
        }
    }
}

