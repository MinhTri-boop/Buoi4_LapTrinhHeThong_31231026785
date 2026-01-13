using System;

namespace SystemInfoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== THONG TIN HE THONG ===");

            // 1. Lấy thông tin phiên bản Hệ điều hành
            var osVersion = Environment.OSVersion;
            Console.WriteLine($"1. He dieu hanh: {osVersion}");

            // 2. Lấy đường dẫn thư mục hiện tại
            var currentDir = Environment.CurrentDirectory;
            Console.WriteLine($"2. Thu muc hien tai: {currentDir}");

            // 3. Lấy thời gian hệ thống hiện tại
            var sysTime = DateTime.Now;
            Console.WriteLine($"3. Thoi gian he thong: {sysTime}");

            Console.WriteLine("==========================");
        }
    }
}