using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace FileMonitorApp
{
    class Program
    {
        // Định nghĩa đường dẫn
        static string watchPath = Path.Combine(Directory.GetCurrentDirectory(), "InputFolder");
        static string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "OutputFolder");

        static void Main(string[] args)
        {
            // 1. Tạo thư mục nếu chưa có
            Directory.CreateDirectory(watchPath);
            Directory.CreateDirectory(outputPath);

            Console.WriteLine("=== FILE MONITOR SYSTEM ===");
            Console.WriteLine($"Dang theo doi thu muc: {watchPath}");
            Console.WriteLine($"Ket qua se luu tai:   {outputPath}");
            Console.WriteLine("-----------------------------------");

            // 2. Cấu hình FileSystemWatcher
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = watchPath;
                
                // Chỉ theo dõi file văn bản (hoặc để *.* nếu muốn tất cả)
                watcher.Filter = "*.txt"; 

                // Các loại thay đổi cần theo dõi (Tên file, nội dung, v.v.)
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                // Đăng ký sự kiện: Khi file được TẠO MỚI
                watcher.Created += OnFileCreated;

                // Bắt đầu theo dõi
                watcher.EnableRaisingEvents = true;

                Console.WriteLine("He thong dang chay. Hay copy file .txt vao 'InputFolder' de test.");
                Console.WriteLine("Nhan 'q' de thoat.");
                while (Console.Read() != 'q') ;
            }
        }

        // Sự kiện khi phát hiện file mới
        private static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"[DETECTED] Phat hien file moi: {e.Name}");

            // Xử lý Concurrent: Đẩy việc xử lý sang luồng khác (Task.Run)
            // để Watcher tiếp tục bắt các file khác ngay lập tức.
            Task.Run(() => ProcessFile(e.FullPath, e.Name));
        }

        // Logic xử lý file (Đọc -> Nén -> Lưu)
        private static async Task ProcessFile(string inputFilePath, string fileName)
        {
            // BƯỚC QUAN TRỌNG: Chờ file sẵn sàng (tránh lỗi file đang bị lock bởi OS)
            if (!WaitForFile(inputFilePath))
            {
                Console.WriteLine($"[ERROR] Khong the truy cap file {fileName} sau nhieu lan thu.");
                return;
            }

            try
            {
                string compressedFileName = fileName + ".gz";
                string outputFilePath = Path.Combine(outputPath, compressedFileName);

                // Đọc và Nén file
                using (FileStream sourceStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                using (FileStream targetStream = File.Create(outputFilePath))
                using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                {
                    await sourceStream.CopyToAsync(compressionStream);
                }

                Console.WriteLine($"[SUCCESS] Da nen xong: {compressedFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] Loi xu ly file {fileName}: {ex.Message}");
            }
        }

        // Hàm helper: Thử mở file liên tục cho đến khi OS nhả file ra
        private static bool WaitForFile(string fullPath)
        {
            for (int i = 0; i < 10; i++) // Thử 10 lần
            {
                try
                {
                    // Thử mở file chế độ Open
                    using (FileStream fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        if (fs.Length > 0) return true; // File ok
                    }
                }
                catch (IOException)
                {
                    // File vẫn đang bị lock, chờ 500ms rồi thử lại
                    Thread.Sleep(500);
                }
            }
            return false;
        }
    }
}