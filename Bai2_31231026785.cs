using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskWaitDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== BAT DAU CHUONG TRINH ===");
            
            // Danh sách chứa các Task đang chạy
            List<Task> taskList = new List<Task>();
            
            // Tạo 3 tác vụ độc lập
            for (int i = 1; i <= 3; i++)
            {
                int taskId = i; // Biến cục bộ để tránh lỗi closure
                
                // Khởi động Task
                Task myTask = Task.Run(async () => 
                {
                    await DoWork(taskId);
                });

                taskList.Add(myTask);
            }

            Console.WriteLine("-> Main Thread: Dang cho 3 tac vu hoan thanh...");

            // --- CƠ CHẾ ĐỒNG BỘ: Task.WhenAll ---
            // Dòng này sẽ tạm dừng Main lại (không chặn luồng) cho đến khi 
            // TẤT CẢ các task trong danh sách đều chạy xong.
            await Task.WhenAll(taskList);

            Console.WriteLine("\n=== TAT CA TAC VU DA XONG. CHUONG TRINH KET THUC! ===");
        }

        // Hàm giả lập công việc (Ngủ ngẫu nhiên từ 1-3 giây)
        static async Task DoWork(int id)
        {
            Random rnd = new Random();
            int sleepTime = rnd.Next(1000, 3000); // Random 1s - 3s
            
            Console.WriteLine($"[Task {id}] Bat dau lam viec (mat {sleepTime}ms)...");
            
            await Task.Delay(sleepTime); // Giả lập việc đang xử lý
            
            Console.WriteLine($"[Task {id}] --> DA XONG.");
        }
    }
}