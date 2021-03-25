using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightConductor.Main
{
    static class JsonNewtonsoft
    {

        /// <summary>
        /// 把对象转换为JSON字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object o)
        {
            if (o == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(o);
        }
        /// <summary>
        /// 把Json文本转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T FromJSON<T>(this string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        /// <summary>
        ///     添加json
        /// </summary>
        /// <param name="desktopPath"></param>
        public static void WritingJson<T>(string filePath, List<T> list)
        {
            //转成json
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            //保存到桌面的文件
            SaveMyJson(filePath, json);

        }

        /// <summary>
        ///     将我们的json保存到本地
        /// </summary>
        /// <param name="desktopPath"></param>
        /// <param name="json"></param>
        public static void SaveMyJson(string filePath, string json)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                //写入
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(json);
                }

            }
        }


        /// <summary>
        ///     读取json
        /// </summary>
        /// <param name="desktopPath"></param>
        public static List<T> ReadingJson<T>(string filePath)
        {
            string myStr = "";
            //IO读取
            myStr = GetMyJson(filePath);
            if (string.IsNullOrWhiteSpace(myStr))
            {
                return new List<T>();
            }
            //转换
            List<T> lists = JsonConvert.DeserializeObject<List<T>>(myStr);
            //进一步的转换我就不写啦
            return lists;
        }


        /// <summary>
        ///     IO读取本地json
        /// </summary>
        /// <param name="desktopPath"></param>
        /// <returns></returns>
        public static string GetMyJson(string filePath)
        {
            bool e = File.Exists(filePath);
            if (!e)
            {
                return "";
            }
            using (FileStream fsRead = new FileStream(filePath, FileMode.Open))
            {
                //读取加转换
                int fsLen = (int)fsRead.Length;
                byte[] heByte = new byte[fsLen];
                int r = fsRead.Read(heByte, 0, heByte.Length);
                return System.Text.Encoding.UTF8.GetString(heByte);
            }
        }


    }
}
