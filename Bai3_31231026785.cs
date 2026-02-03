using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FileLoggingDemo
{
    class Program
    {
        // Đường dẫn file log
        static string filePath = "app_log.txt";
        
        // Đối tượng khóa (dùng cho cách an toàn)
        static readonly object fileLock = new object();

        static void Main(string[] args)
        {
            // Xóa file cũ nếu có để log sạch sẽ
            if (File.Exists(filePath)) File.Delete(filePath);

            Console.WriteLine("=== DEMO GHI FILE DA LUONG ===");
            Console.WriteLine("1. Chay che do KHONG AN TOAN (Se gay loi)");
            Console.WriteLine("2. Chay che do AN TOAN (Dung lock)");
            Console.Write("Chon (1 hoac 2): ");
            
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                RunUnsafeLogging();
            }
            else
            {
                RunSafeLogging();
            }

            Console.WriteLine("\nNhan Enter de ket thuc...");
            Console.ReadLine();
        }

        // --- TRƯỜNG HỢP 1: KHÔNG ĐỒNG BỘ (SẼ LỖI) ---
        static void RunUnsafeLogging()
        {
            Console.WriteLine("\n--- DANG CHAY KHONG AN TOAN ---");
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        // Giả lập nhiều luồng cùng mở file một lúc
                        string logMessage = $"Thread {threadId}: Ghi log luc {DateTime.Now.Ticks}\n";
                        
                        // Lệnh này mở file, ghi, rồi đóng. 
                        // Nếu 2 luồng cùng gọi lệnh này -> XUNG ĐỘT
                        File.AppendAllText(filePath, logMessage);
                        
                        Console.WriteLine($"Thread {threadId}: Ghi thanh cong.");
                    }
                    catch (IOException ex)
                    {
                        // Bắt lỗi để chương trình không bị tắt đột ngột, giúp bạn đọc được lỗi
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Thread {threadId} LOI: {ex.Message}");
                        Console.ResetColor();
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        // --- TRƯỜNG HỢP 2: AN TOAN (DÙNG LOCK) ---
        static void RunSafeLogging()
        {
            Console.WriteLine("\n--- DANG CHAY AN TOAN (LOCK) ---");
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                int threadId = i;
                tasks.Add(Task.Run(() =>
                {
                    string logMessage = $"Thread {threadId}: Ghi log luc {DateTime.Now.Ticks}\n";

                    // Cơ chế khóa: Chỉ 1 luồng được vào đây tại 1 thời điểm
                    lock (fileLock)
                    {
                        File.AppendAllText(filePath, logMessage);
                        Console.WriteLine($"Thread {threadId}: Ghi thanh cong (Safe).");
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("-> Kiem tra file 'app_log.txt' de xem ket qua.");
        }
    }
}