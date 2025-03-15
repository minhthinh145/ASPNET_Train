using System.Text;

namespace Ecomerce.Helpers
{
    public class MyUtil
    {
        public static string UploadHinh(IFormFile Hinh, string folder)
        {
            try
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(), "wwwroot", "Hinh", Hinh.FileName);
                using (var myfile = new FileStream(fullPath, FileMode.Create)) // Ghi đè nếu tồn tại
                {
                    Hinh.CopyTo(myfile);
                }

                return Hinh.FileName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string GenerateRandomKey(int length = 5)
        {
            var pattern = @"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!";
            var sb = new StringBuilder();
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }
            return sb.ToString();
        }
    }
}
