using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GZipTest
{
    class Program
    {
        // Объявление делегата
        delegate void GetGZipType(); 
        static void Main(string[] args)
        {
            try
            {
                GetGZipType zip;
                GZip gZip;
                // Проверка на количество аргументов
                if (args.Count() != 3) throw new Exception("[Ошибка] Неправильное количество аргументов.");
              
                string sourceFileName = args[1];

                if(!File.Exists(sourceFileName)) throw new Exception("[Ошибка] Заданного файла не существует.");

                string resultFileName = args[2];

                gZip = new GZip(sourceFileName, resultFileName);
                // Проверка на правильность написания команды
                // присвоение переменной адреса метода
                string command = args[0];
                switch (command)
                {
                    case ("compress"):
                        zip = gZip.Compress;
                        break;
                    case ("decompress"):
                        zip = gZip.Decompress;
                        break;
                    default:
                        throw new Exception("[Ошибка] Неправильное название команды.");

                }
                
                // Вызов функции
                
                zip.Invoke();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
