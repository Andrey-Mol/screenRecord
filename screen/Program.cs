using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        string executablePath = @"./prog/svchost.exe"; // Укажите путь к вашему исполняемому файлу

        while (true)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = executablePath;
                process.Start();

                // Ожидание завершения дочернего процесса
                process.WaitForExit();
                Thread.Sleep(5000); // Небольшая задержка перед перезапуском
            }
        }
    }
}