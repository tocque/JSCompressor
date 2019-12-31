using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EcmaScript.NET;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
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
            index = main.IndexOf("this.materials=[") + 16;
            index2 = main.IndexOf("]", index);
            string[] materialsList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');

            Console.WriteLine("正在压缩核心文件...");
            string libs = "";
            foreach (string one in loadlist)
            {
                try
                {
                    string data = File.ReadAllText(directory + "libs\\" + one + ".js");
                    string compressed = compressor.Compress(data);
                    libs += compressed;
                    File.WriteAllText(directory + "libs\\" + one + ".min.js", compressed);
                    Console.WriteLine("压缩中：libs/" + one + ".js ===> libs/" + one + ".min.js");
                }
                catch (EcmaScriptException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(one + ".js 压缩出错：" + e.Message);
                    Console.WriteLine("[Line {0} Col {1}]: {2}", e.LineNumber, e.ColumnNumber, e.LineSource);
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(one + ".js 压缩出错：" + e.Message);
                    Console.ResetColor();
                }
            }
            if (isV2)
            {
                File.WriteAllText(directory + "libs\\libs.min.js", libs);
                Console.WriteLine("======> 所有核心文件已压缩到 libs/libs.min.js。");
            }
            Console.WriteLine("------ 核心文件压缩完毕 ------\n\n");

            string[] mapList = null;
            string[] animationList = null;
            string[] imageList = null;
            string[] soundList = null;
            string[] bgmList = null;
            string[] tilesetsList = null;
            string[] autotilesList = null;
            if (isV2)
            {
                // find load list
                index = main.IndexOf("this.pureData=[") + 15;
                index2 = main.IndexOf("]", index);
                // project文件夹下的js文件列表
                loadlist = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');

                Console.WriteLine("正在压缩项目文件...");
                string project = "";
                foreach (string one in loadlist)
                {
                    try
                    {
                        string data = File.ReadAllText(directory + "project\\" + one + ".js");
                        data = compressor.Compress(data);
                        project += data;
                        File.WriteAllText(directory + "project\\" + one + ".min.js", data);
                        Console.WriteLine("压缩中：project/" + one + ".js ===> project/" + one + ".min.js");

                        // 读到data.js
                        if (one.Equals("data"))
                        {
                            index = data.IndexOf("floorIds:[")+10;
                            index2 = data.IndexOf("]", index);
                            mapList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                            index = data.IndexOf("images:[") + 8;
                            index2 = data.IndexOf("]", index);
                            imageList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                            index = data.IndexOf("tilesets:[") + 10;
                            index2 = data.IndexOf("]", index);
                            tilesetsList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                            index = data.IndexOf("animates:[") + 10;
                            index2 = data.IndexOf("]", index);
                            animationList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                            index = data.IndexOf("sounds:[") + 8;
                            index2 = data.IndexOf("]", index);
                            soundList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                            index = data.IndexOf("bgms:[") + 6;
                            index2 = data.IndexOf("]", index);
                            bgmList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                        }

                        // 读到data.js
                        if (one.Equals("icons"))
                        {
                            index = data.IndexOf("autotile:{") + 10;
                            index2 = data.IndexOf("}", index);
                            autotilesList = data.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
                        }

                    }
                    catch (EcmaScriptException e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(one + ".js 压缩出错：" + e.Message);
                        Console.WriteLine("[Line {0} Col {1}]: {2}", e.LineNumber, e.ColumnNumber, e.LineSource);
                        Console.ResetColor();
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(one + ".js 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }
                }
                File.WriteAllText(directory + "project\\project.min.js", project);
                Console.WriteLine("======> 所有核心文件已压缩到 project/project.min.js。");
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
                catch (EcmaScriptException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(map + ".js 压缩出错：" + e.Message);
                    Console.WriteLine("[Line {0} Col {1}]: {2}", e.LineNumber, e.ColumnNumber, e.LineSource);
                    Console.ResetColor();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(map + ".js 压缩出错：" + e.Message);
                    Console.ResetColor();
                }
            }
            File.WriteAllText(directory + floorDir + "\\floors.min.js", maps);
            Console.WriteLine("======> 所有地图文件已压缩到" + floorDir + "/floors.min.js。");
            Console.WriteLine("------ 地图文件压缩完毕 ------\n\n");

            Console.WriteLine("正在压缩图片文件...");
            if (imageList == null)
            {
                index = main.IndexOf("this.images=[") + 13;
                index2 = main.IndexOf("]", index);
                imageList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }
            Directory.SetCurrentDirectory(directory + floorDir + "\\images\\");

            bool check = false;
            List<string> ml = imageList.ToList();
            foreach (var one in ml)
            {
                if (one == "hero.png")
                {
                    check = true;
                    break;
                }
            }
            if (!check)
            {
                ml.Add("hero.png");
            }

            if (File.Exists("images.zip"))
            {
                File.Delete("images.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("images.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string file in ml)
                {
                    try
                    {
                        ZipEntry entry = new ZipEntry(file);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + " 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + "已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有图片文件已压缩到" + floorDir + "/images/images.zip。");
            Console.WriteLine("------ 图片文件压缩完毕 ------\n\n");

            Console.WriteLine("正在压缩材质图片文件...");
            if (materialsList == null)
            {
                index = main.IndexOf("this.materials=[") + 16;
                index2 = main.IndexOf("]", index);
                materialsList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }

            if (File.Exists("materials.zip"))
            {
                File.Delete("materials.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("materials.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string file in materialsList.ToList())
                {
                    try
                    {
                        ZipEntry entry = new ZipEntry(file + ".png");
                        entry.DateTime = DateTime.Now; 
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file + ".png"))
                        {
                            int sourceBytes; 
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + ".png 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + ".png 已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有材质图片文件已压缩到" + floorDir + "/images/materials.zip。");
            Console.WriteLine("------ 材质图片文件压缩完毕 ------\n\n");

            Console.WriteLine("正在压缩瓦片图片文件...");
            if (tilesetsList == null)
            {
                index = main.IndexOf("this.tilesets=[") + 16;
                index2 = main.IndexOf("]", index);
                tilesetsList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }

            if (File.Exists("tilesets.zip"))
            {
                File.Delete("tilesets.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("tilesets.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string file in tilesetsList.ToList())
                {
                    try
                    {
                        ZipEntry entry = new ZipEntry(file);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + " 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + "已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有瓦片图片文件已压缩到" + floorDir + "/images/tilesets.zip。");
            Console.WriteLine("------ 瓦片图片文件压缩完毕 ------\n\n");


            Console.WriteLine("正在压缩自动元件图片文件...");
            if (autotilesList == null)
            {
                index = main.IndexOf("this.autotiles=[") + 16;
                index2 = main.IndexOf("]", index);
                autotilesList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }

            if (File.Exists("autotiles.zip"))
            {
                File.Delete("autotiles.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("autotiles.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string one in autotilesList.ToList())
                {
                    string file = "";
                    try
                    {
                        int i = one.IndexOf(":");
                        file = one.Substring(0,i);
                        ZipEntry entry = new ZipEntry(file + ".png");
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file + ".png"))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + ".png 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + ".png 已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有自动元件图片文件已压缩到" + floorDir + "/images/autotiles.zip。");
            Console.WriteLine("------ 自动元件图片文件压缩完毕 ------\n\n");

            Console.WriteLine("正在压缩动画文件...");
            if (animationList == null)
            {
                index = main.IndexOf("this.animates=[") + 15;
                index2 = main.IndexOf("]", index);
                animationList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }
            Directory.SetCurrentDirectory(directory + "\\animates\\");
            if (File.Exists("animates.zip"))
            {
                File.Delete("animates.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("animates.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string file in animationList.ToList())
                {
                    try
                    {
                        ZipEntry entry = new ZipEntry(file + ".animate");
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file + ".animate"))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + ".animate 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + ".animate 已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有动画文件已压缩到" + floorDir + "/animates/animates.zip。");
            Console.WriteLine("------ 动画文件压缩完毕 ------\n\n");

            Console.WriteLine("正在压缩音效文件...");
            if (soundList == null)
            {
                index = main.IndexOf("this.sounds=[") + 13;
                index2 = main.IndexOf("]", index);
                soundList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }
            Directory.SetCurrentDirectory(directory + "\\sounds\\");
            if (File.Exists("sounds.zip"))
            {
                File.Delete("sounds.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("sounds.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string file in soundList.ToList())
                {
                    try
                    {
                        ZipEntry entry = new ZipEntry(file);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + " 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + "已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有音效文件已压缩到" + floorDir + "/sounds/sounds.zip。");
            Console.WriteLine("------ 音效文件压缩完毕 ------\n\n");

            Console.WriteLine("正在压缩音乐文件...");
            if (bgmList == null)
            {
                index = main.IndexOf("this.bgms=[") + 11;
                index2 = main.IndexOf("]", index);
                bgmList = main.Substring(index, index2 - index).Replace("\"", "").Replace("'", "").Split(',');
            }

            if (File.Exists("bgms.zip"))
            {
                File.Delete("bgms.zip");
            }

            using (ZipOutputStream s = new ZipOutputStream(File.Create("bgms.zip")))
            {
                s.SetLevel(9);
                s.UseZip64 = UseZip64.Off;
                byte[] buffer = new byte[2048];
                foreach (string file in bgmList.ToList())
                {
                    try
                    {
                        ZipEntry entry = new ZipEntry(file);
                        entry.DateTime = DateTime.Now;
                        s.PutNextEntry(entry);
                        using (FileStream fs = File.OpenRead(file))
                        {
                            int sourceBytes;
                            do
                            {
                                sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                s.Write(buffer, 0, sourceBytes);
                            } while (sourceBytes > 0);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(file + " 压缩出错：" + e.Message);
                        Console.ResetColor();
                    }

                    Console.WriteLine(file + "已压缩");
                }
                s.Finish();
                s.Close();
            }
            Console.WriteLine("======> 所有音乐文件已压缩到" + floorDir + "/sounds/bgms.zip。");
            Console.WriteLine("------ 音乐文件压缩完毕 ------\n\n");

            Console.WriteLine("=====================================================");
            Console.WriteLine("警告：请将main.js中的 this.useCompress 从 false 改成 true，才能真正加载压缩后的代码。");
            Console.WriteLine("=====================================================\n");

            Console.WriteLine("按任意键退出...");

            Console.ReadKey();
        }
    }
}
