using CSCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using MusicPLayer.Utils;

namespace MusicPLayer.Models
{
    public partial class MusicList
    {
        ObservableCollection<MusicItem> _list = new ObservableCollection<MusicItem>();
        public ObservableCollection<MusicItem> List { get => _list; set => _list = value; }

        public void SaveListAsXml(string fileName)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(typeof(MusicList));
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, this);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(MusicList)}.SaveSettingAsXml: {ex}");
            }
        }
        public static MusicList LoadListFromXml(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { throw new FileNotFoundException("NotFound"); }

            MusicList db = new MusicList();
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using (StringReader read = new StringReader(xmlString))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MusicList));
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        db = (MusicList)serializer.Deserialize(reader);
                        reader.Close();
                    }
                    read.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{typeof(MusicList)}.LoadSettingFromXml: {ex}");
            }
            return db;
        }
    }
}
