using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Booyco_HMI_Utility
{
    public class MernokAsset
    {
        public byte Type;
        public byte Group;
        public bool IsLicensable;
        public string TypeName;
        public string GroupName;
    }

    public class MernokAssetFile
    {
        public List<MernokAsset> mernokAssetList;
        public UInt16 version;
        public DateTime dateCreated;
        public string createdBy;            //Name of file creator
    }

    public static class MernokAssetManager
    {
        //Change this to accept a path and name for the file
        public static string CreateMernokAssetFile(MernokAssetFile f)
        {
            string result = "File created succesfully";
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MernokAssetFile));
                using (TextWriter writer = new StreamWriter(@"C:\MernokAssets\MernokAssetList.xml"))
                {
                    serializer.Serialize(writer, f);
                }
            }
            catch (Exception e)
            {
                result = e.ToString();
            }

            return result;
        }

        public static string MernokAssetContent { get; set; }
        //todo: Change this to accept a path for the file
        //       public static MernokAssetFile ReadMernokAssetFile(string filename)
        public static MernokAssetFile ReadMernokAssetFile()
        {
            //todo: add exception handling
            //Try Read the XML file
            XmlSerializer deserializer = new XmlSerializer(typeof(MernokAssetFile));
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            //TextReader reader = new StreamReader(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/MernokAssetList.xml");
            TextReader reader = new StreamReader(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/MernokAssetList.xml");
            MernokAssetContent = reader.ReadToEnd();
            reader = new StringReader((string)MernokAssetContent.Clone());
            object obj = deserializer.Deserialize(reader);
            MernokAssetFile f = (MernokAssetFile)obj;
            reader.Close();
            return f;
        }


    }

}
