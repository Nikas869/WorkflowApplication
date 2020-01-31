using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using JdSuite.Common.Internal;

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
