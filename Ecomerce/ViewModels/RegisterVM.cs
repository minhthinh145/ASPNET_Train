using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
namespace Ecomerce.ViewModels
{
    public class RegisterVM
    {
        [Display(Name = "Tên đăng nhập")]
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "*")]
        [MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
        public string MaKh { get; set; }

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "*")]
        public string MatKhau { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "*")]
        [Display(Name = "Họ tên")]
        [MaxLength(50, ErrorMessage = "Tối đa 50 ký tự")]
        public string HoTen { get; set; }

        public bool GioiTinh { get; set; } = true;
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }


        [MaxLength(60, ErrorMessage = "Tối đa 60 ký tự")]
        [Display(Name = "Địa chỉ")]

        public string DiaChi { get; set; }
        [MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
        [RegularExpression(@"0[9875]\d{8}", ErrorMessage = "Chưa đúng định dạng di động Việt Nam")]
        [Display(Name = "Số điện thoại")]

        public string DienThoai { get; set; }
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]

        public string Email { get; set; }
        [Display(Name = "Avatar")]

        public string? Hinh { get; set; }
    }
}
