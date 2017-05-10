using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public class GZip
    {
        private string sourceFile;
        private string resultFile;
        static int x = 0;
        static object locker = new object();
        public GZip(string _sourceFile, string _compressedFile)
        {
            this.sourceFile = _sourceFile;
            this.resultFile = _compressedFile;
        }

        
        public void Compress()
        {
            long lengthFile;
            WriteFileInfo(sourceFile);
            // поток для чтения исходного файла
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                lengthFile = sourceStream.Length;
            }
            long memory = GetMaxMemory();
            Console.WriteLine("Доступный объем памяти: {0}", memory);
            long k = lengthFile / memory + 1;
            Console.WriteLine("Количество частей файла: {0}", k);
            for (long i = 0; i < k; i++)
            {
                Thread myThread = new Thread(CompressPathStream);
                myThread.Start();
            }

        }



        public void Decompress()
        {
            long lengthFile;
            WriteFileInfo(sourceFile);
            // поток для чтения из сжатого файла
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                lengthFile = sourceStream.Length;
            }
            long memory = GetMaxMemory();
            Console.WriteLine("Доступный объем памяти: {0}", memory);
            long k = lengthFile / memory + 1;
            Console.WriteLine("Количество частей файла: {0}", k);
            for (long i = 0; i < k; i++)
            {
                Thread myThread = new Thread(DecompressPathStream);
                myThread.Start();
            }
        }

        private void WriteFileInfo(string path)
        {
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                Console.WriteLine("Имя файла: {0}", fileInf.Name);
                Console.WriteLine("Время создания: {0}", fileInf.CreationTime);
                Console.WriteLine("Размер: {0}", fileInf.Length);
            }
        }

        private long GetMaxMemory()
        {
            PerformanceCounter _ramCounter = new PerformanceCounter("Memory", "Available Bytes");
            return (long)_ramCounter.NextValue();
        }
        private void CompressPathStream()
        {
            lock (locker)
            {
                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
                {
                    // считываем данные
                    long length = sourceStream.Length - x;
                    byte[] array = new byte[length < GetMaxMemory() ? length : GetMaxMemory()];
                    sourceStream.Read(array, (int)x, array.Length);
                    using (FileStream resultStream = new FileStream(resultFile, FileMode.Append, FileAccess.Write))
                    {

                        using (GZipStream compressionStream = new GZipStream(resultStream, CompressionMode.Compress))
                        {
                            compressionStream.Write(array, 0, array.Length); // копируем байты из одного потока в другой
                            x += array.Length;
                        }
                    }
                    if(x >= sourceStream.Length)
                    {
                        using (FileStream resultStream = File.OpenRead(resultFile))
                        {
                            Console.WriteLine("Сжатие файла {0} завершено. Исходный размер: {1}  сжатый размер: {2}.",
                                sourceFile, sourceStream.Length.ToString(), resultStream.Length.ToString());
                        }
                    }
                }
            }
        }

        private void DecompressPathStream()
        {
            lock (locker)
            {
                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
                {
                    // считываем данные
                    long length = sourceStream.Length - x;
                    byte[] array = new byte[length < GetMaxMemory() ? length : GetMaxMemory()];
                    sourceStream.Read(array, (int)x, array.Length);

                    using (FileStream resultStream = new FileStream(resultFile, FileMode.Append, FileAccess.Write))
                    {
                        using (FileStream pathStream = new FileStream("temporary.txt", FileMode.Create))
                        {
                            pathStream.Write(array, 0, array.Length);
                            pathStream.Position = 0;
                            using (GZipStream decompressionStream = new GZipStream(pathStream, CompressionMode.Decompress))
                            {
                                decompressionStream.CopyTo(resultStream); // копируем байты из одного потока в другой
                                x += array.Length;
                            }
                            
                        }
                        File.Delete("temporary.txt");
                    }
                    if (x >= sourceStream.Length)
                    {
                        using (FileStream resultStream = File.OpenRead(resultFile))
                        {
                            Console.WriteLine("Восстановлен файл: {0}", resultFile);
                        }
                    }
                }
            }
        }

    }
}
