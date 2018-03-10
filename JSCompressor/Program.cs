using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yahoo.Yui.Compressor;

namespace JSCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("====== 压缩工具，可以对JS文件进行压缩，以缩小数据大小 ======\n");

            string directory = ".\\";
            if (!File.Exists(directory + "main.js"))
                directory = "..\\";

            bool isV2 = Directory.Exists(directory + "project");

            string main="";
            try
            {
                main = File.ReadAllText(directory+"main.js");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadKey();
                return;
            }
            JavaScriptCompressor compressor=new JavaScriptCompressor();
            main = compressor.Compress(main);

            // find load list
            int index = main.IndexOf("this.loadList=[")+15;
            int index2 = main.IndexOf("]", index);
            string[] loadlist = main.Substring(index, index2 - index).Replace("\"","").Replace("'","").Split(',');

            Console.WriteLine("正在压缩核心文件...");
            foreach (string one in loadlist)
            {
                try
                {
                    string data = File.ReadAllText(directory + "libs\\" + one + ".js");
                    File.WriteAllText(directory + "libs\\" + one + ".min.js", compressor.Compress(data));
                    Console.WriteLine("压缩中：libs/" + one + ".js ===> libs/" + one + ".min.js");
                }
                catch (Exception e)
                {
                    Console.WriteLine(one+".js 压缩出错：" + e.Message);
                }
            }
            Console.WriteLine("------ 核心文件压缩完毕 ------\n\n");

            string[] mapList = null;
            if (isV2)
            {
                // find load list
                index = main.IndexOf("this.pureData=[") + 15;
                index2 = main.IndexOf("]", index);
                loadlist = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');

                Console.WriteLine("正在压缩项目文件...");
                foreach (string one in loadlist)
                {
                    try
                    {
                        string data = File.ReadAllText(directory + "project\\" + one + ".js");
                        data = compressor.Compress(data);
                        File.WriteAllText(directory + "project\\" + one + ".min.js", data);
                        Console.WriteLine("压缩中：project/" + one + ".js ===> project/" + one + ".min.js");

                        if (one.Equals("data"))
                        {
                            index = data.IndexOf("floorIds:[")+10;
                            index2 = data.IndexOf("]", index);
                            mapList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(one + ".js 压缩出错：" + e.Message);
                    }
                }
                Console.WriteLine("------ 项目文件压缩完毕 ------\n\n");
            }

            Console.WriteLine("正在压缩地图文件...");
            if (mapList == null)
            {
                index = main.IndexOf("this.floorIds=[") + 15;
                index2 = main.IndexOf("]", index);
                mapList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }

            string maps = "";
            var floorDir = isV2 ? "project" : "libs";
            foreach (string map in mapList)
            {
                try
                {
                    maps += compressor.Compress(File.ReadAllText(directory + floorDir + "\\floors\\" + map + ".js"));
                    Console.WriteLine(map + ".js已压缩");
                }
                catch (Exception e)
                {
                    Console.WriteLine("！！！！！！" + map + ".js 压缩出错：" + e.Message + "  请检查是否存在语法错误。");
                }
            }
            File.WriteAllText(directory + floorDir + "\\floors.min.js", maps);
            Console.WriteLine("======> 所有地图文件已压缩到" + floorDir + "/floors.min.js。");
            Console.WriteLine("------ 地图文件压缩完毕 ------\n\n");

            Console.WriteLine("=====================================================");
            Console.WriteLine("警告：请将main.js中的 this.useCompress 从 false 改成 true，才能真正加载压缩后的代码。");
            Console.WriteLine("=====================================================\n");

            Console.WriteLine("按任意键退出...");

            Console.ReadKey();
        }
    }
}
