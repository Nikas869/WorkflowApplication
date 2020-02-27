using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace WinFormCodeCompile
{
    #region
    public class Input_0 { public Input_0_catalog catalog { get; set; }
 } 
public class Input_0_catalog { public List<Input_0_catalog_book> book { get; set; } = new List<Input_0_catalog_book>();
 } 
public class Input_0_catalog_book { public String id { get; set; }
public String author { get; set; }
public String title { get; set; }
public String genre { get; set; }
public String price { get; set; }
public String publish_date { get; set; }
public String description { get; set; }
 } 

    public class Output_0 { public Output_0_catalog catalog { get; set; }
 } 
public class Output_0_catalog { public List<Output_0_catalog_book> book { get; set; } = new List<Output_0_catalog_book>();
 } 
public class Output_0_catalog_book { public String id { get; set; }
public String author { get; set; }
public String title { get; set; }
public String genre { get; set; }
public String price { get; set; }
public String publish_date { get; set; }
public String description { get; set; }
 } 

    #endregion

    public class Transform
    {
        Input_0 Input_0 =  new Input_0();
Output_0 Output_0 =  new Output_0();


        public Transform()
        {

        }

        public static void Log(object message)
{
    try
    {
        using (var sw = new StreamWriter(@"D:\Projects\WorkflowApplication\Pugin\Scripting App\bin\Debug\Log.txt", true))
        {
            sw.WriteLine(message);
        }
    }
    catch { }
}

        public void  UpdateText()
        {
            
        }
    }
}