using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace SecureStorageDemo
{
    class Program
    {
        // Key và IV cứng (Chỉ dùng cho demo, thực tế phải sinh ngẫu nhiên và bảo mật)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"); // 32 bytes = 256 bit
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("ABCDEF0123456789"); // 16 bytes = 128 bit

        static void Main(string[] args)
        {
            string originalFile = "sensitive_data.txt";
            string securedFile = "data.enc.gz";
            string restoredFile = "restored_data.txt";

            // 1. Tạo file dữ liệu mẫu
            string secretContent = "Mat khau e-banking la 123456. " + string.Concat(System.Linq.Enumerable.Repeat("Lap lai noi dung de test nen. ", 20));
            File.WriteAllText(originalFile, secretContent);
            Console.WriteLine($"[ORIGINAL] Tao file goc: {new FileInfo(originalFile).Length} bytes");

            // 2. Quy trình xuôi: Đọc -> Mã hóa -> Nén -> Lưu
            EncryptAndCompress(originalFile, securedFile);
            Console.WriteLine($"[SECURED] File sau khi Ma hoa + Nen: {new FileInfo(securedFile).Length} bytes");

            // 3. Quy trình ngược: Đọc -> Giải nén -> Giải mã -> Lưu
            DecompressAndDecrypt(securedFile, restoredFile);
            Console.WriteLine($"[RESTORED] File khoi phuc: {new FileInfo(restoredFile).Length} bytes");

            // 4. Kiểm tra nội dung
            string restoredContent = File.ReadAllText(restoredFile);
            Console.WriteLine("\nNoi dung khoi phuc:");
            Console.WriteLine(restoredContent.Substring(0, 50) + "...");
        }

        static void EncryptAndCompress(string inputFile, string outputFile)
        {
            using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
            {
                // Tầng 1: Nén (GZip) bao bên ngoài cùng
                using (GZipStream gzipStream = new GZipStream(fsOut, CompressionMode.Compress))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = Key;
                        aes.IV = IV;

                        // Tầng 2: Mã hóa (Crypto) nằm bên trong
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                        using (CryptoStream cryptoStream = new CryptoStream(gzipStream, encryptor, CryptoStreamMode.Write))
                        {
                            // Tầng 3: Đọc dữ liệu gốc và ghi vào chuỗi stream trên
                            byte[] inputBytes = File.ReadAllBytes(inputFile);
                            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                        }
                    }
                }
            }
        }

        static void DecompressAndDecrypt(string inputFile, string outputFile)
        {
            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
            {
                // Tầng 1: Giải nén trước
                using (GZipStream gzipStream = new GZipStream(fsIn, CompressionMode.Decompress))
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = Key;
                        aes.IV = IV;

                        // Tầng 2: Giải mã sau
                        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                        using (CryptoStream cryptoStream = new CryptoStream(gzipStream, decryptor, CryptoStreamMode.Read))
                        using (StreamReader reader = new StreamReader(cryptoStream))
                        {
                            // Ghi kết quả ra file
                            string decryptedContent = reader.ReadToEnd();
                            File.WriteAllText(outputFile, decryptedContent);
                        }
                    }
                }
            }
        }
    }
}