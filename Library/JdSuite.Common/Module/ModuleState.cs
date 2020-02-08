using JdSuite.Common.FileProcessing;
using JdSuite.Common.Internal;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace JdSuite.Common.Module
{

    /// <summary>
    /// ModuleState class is used to transfer module state from one module to other
    /// </summary>
    [Serializable]
    public class ModuleState
    {

        public const string VerifySchemaTagOrderVar = "VerifySchemaTagOrderVar";

        public ModuleState() { }


        [XmlAttribute]
        public bool InputIsSchema { get; set; } = false;


        [XmlAttribute]
        public string TextEncoding { get; set; } = String.Empty;



        /// <summary>
        /// Usually shows input path of data file
        /// </summary>
        [XmlAttribute]
        public string DataFilePath { get; set; } = String.Empty;

        /// <summary>
        /// Input file to work with
        /// </summary>
        [XmlIgnore]
        public WorkflowFile DataFile { get; set; }


        [XmlElement(ElementName = "StateVar")]
        public SerializableDictionary<string, string> StateVar { get; set; }= new SerializableDictionary<string, string>();



        [XmlElement(ElementName ="Schema")]
        /// <summary>
        /// Usually shows XML schema in Field form
        /// </summary>
        public Field Schema { get; set; }


        public void SetStateVar(string VarName,string VarValue)
        {
            StateVar[VarName] = VarValue;
        }


        /// <summary>
        /// Returns null if VarName is not found
        /// </summary>
        /// <param name="VarName"></param>
        /// <returns></returns>
        public string GetStateVar(string VarName)
        {
            if(StateVar.ContainsKey(VarName))
            {
               return StateVar[VarName];
            }

            return null;
        }

        /// <summary>
        /// Checks if VarName exists in State
        /// </summary>
        /// <param name="VarName"></param>
        /// <returns></returns>
        public bool HasStateVar(string VarName)
        {
            return StateVar.ContainsKey(VarName);
        }

        /// <summary>
        /// Matches given VarName and VarValue in internal store
        /// </summary>
        /// <param name="VarName"></param>
        /// <param name="VarValue"></param>
        /// <returns></returns>
        public bool MatchState(string VarName,string VarValue)
        {
            if (!StateVar.ContainsKey(VarName))
                return false;
            return StateVar[VarName] == VarValue;
        }

    }

}
