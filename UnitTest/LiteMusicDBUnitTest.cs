using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using CSCore;
using CSCore.Codecs;
using LiteDB;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MusicPLayerV2.Utils;

namespace UnitTest
{
    public class LiteMusicDBUnitTest
    {
        static LiteDatabase ldb = new LiteDatabase(@"DB.db");
        static LiteCollection<A> aColle;
        static LiteCollection<B> bColle;
        static int Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            aColle = ldb.GetCollection<A>();
            aColle.EnsureIndex(x => x.AA,true);
            bColle = ldb.GetCollection<B>();
            bColle.EnsureIndex(x => x.BB,true);
            ScanDirectory(@"D:\Music\ACG Music\Year_2017");
            foreach (var s in aColle.FindAll())
            {
                Console.WriteLine(
                    $"{s}\n" +
                    $"\t{s.AA}\n" +
                    $"\t{s.BB.BB}\n");
            }
            Console.ReadKey();
            return 0;
        }
        static void ScanDirectory(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException();
            foreach (var file in Directory.EnumerateFiles(path))
            {
                if (!MusicDatabase.CheckFileSupported(file))
                    continue;
                Console.WriteLine(file);
                var nb = new B() { BB = file.GetHashCode().ToString() };
                bColle.Insert(nb);
                aColle.Insert(new A() { AA = file,BB=nb });
                //MusicDatabase.CreateSongEntity(file);
            }
        }
        class A
        {
            public int Id { get; set; }
            [BsonRef("B")]
            public B BB { get; set; }
            public string AA { get; set; }
        }
        class B
        {
            public int Id { get; set; }
            public string BB { get; set; }
        }
    }
}
