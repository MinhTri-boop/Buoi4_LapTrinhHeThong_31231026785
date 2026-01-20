using System;
using System.Drawing; // Cần package System.Drawing.Common
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace MembershipCardPrinter
{
    class Program
    {
        // Import thư viện DLL của Windows để đăng ký Font
        [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

        static void Main(string[] args)
        {
            // Cấu hình thông tin thẻ
            string fontFileName = "MyCustomFont.ttf"; // Tên file font cần cài
            string fontName = "Arial"; // Tên Font Family (Sẽ dùng font này nếu chưa cài được font custom)
            
            // 1. Kiểm tra và Cài đặt Font
            InstallFontIfMissing(fontFileName);

            // 2. In thẻ thành viên
            CreateMembershipCard("Nguyen Van A", "ID: VN-2026-9999", "DIAMOND MEMBER", fontName);
            
            Console.WriteLine("Done! Kiem tra file 'MemberCard.png'.");
        }

        static void InstallFontIfMissing(string fontFileName)
        {
            string fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string destPath = Path.Combine(fontsFolder, fontFileName);
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fontFileName);

            // Kiểm tra xem font đã có trong C:\Windows\Fonts chưa
            if (!File.Exists(destPath))
            {
                Console.WriteLine($"[INFO] Font {fontFileName} chua duoc cai dat. Dang tien hanh cai dat...");

                try
                {
                    if (File.Exists(sourcePath))
                    {
                        // Copy file vào thư mục Fonts (Cần quyền Admin)
                        File.Copy(sourcePath, destPath);
                        
                        // Đăng ký font với hệ thống Windows ngay lập tức mà không cần khởi động lại
                        AddFontResource(destPath);
                        
                        Console.WriteLine("[SUCCESS] Da cai dat font thanh cong!");
                    }
                    else
                    {
                        Console.WriteLine($"[WARNING] Khong tim thay file font tai: {sourcePath}. Se su dung font mac dinh.");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("[ERROR] Khong du quyen Admin de cai font! Hay chay chuong trinh duoi quyen Administrator.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Loi cai dat font: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("[INFO] Font da duoc cai dat tren he thong.");
            }
        }

        static void CreateMembershipCard(string name, string id, string level, string fontName)
        {
            int width = 600;
            int height = 350;

            // Tạo một ảnh Bitmap mới
            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Tô màu nền
                g.Clear(Color.FromArgb(30, 30, 30)); // Màu đen xám sang trọng

                // Vẽ viền
                Pen borderPen = new Pen(Color.Gold, 5);
                g.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);

                // Cấu hình Font (Sử dụng Font vừa cài hoặc Font mặc định)
                Font titleFont = new Font("Arial", 20, FontStyle.Bold);
                Font nameFont = new Font(fontName, 28, FontStyle.Bold); // Font custom
                Font infoFont = new Font("Consolas", 14, FontStyle.Regular);

                // Vẽ Text
                Brush goldBrush = Brushes.Gold;
                Brush whiteBrush = Brushes.White;

                // Tiêu đề
                g.DrawString("VIP MEMBERSHIP", titleFont, goldBrush, new PointF(180, 30));

                // Tên thành viên
                g.DrawString(name, nameFont, whiteBrush, new PointF(50, 120));

                // ID và Level
                g.DrawString(id, infoFont, Brushes.LightGray, new PointF(50, 200));
                g.DrawString(level, infoFont, goldBrush, new PointF(50, 240));

                // Lưu ảnh (Giả lập việc in)
                string outputPath = "MemberCard.png";
                bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"[PRINT] The da duoc in ra file: {outputPath}");
            }
        }
    }
}