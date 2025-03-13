using Ecomerce.Data;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecomerce.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly Hshop2023Context db;
    
        public HangHoaController(Hshop2023Context context)
        {
            db = context;
        }
        public IActionResult Index(int? loai)
        {
            var hanghoas = db.HangHoas.AsQueryable();
            if (loai.HasValue) 
            {
                hanghoas = hanghoas.Where(p => p.MaLoai == loai.Value);
            }
            var result = hanghoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                tenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "", 
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }

        public IActionResult Search(string? query) 
        {
            var hanghoas = db.HangHoas.AsQueryable();
            if (query != null)
            {
                hanghoas = hanghoas.Where(p => p.TenHh.Contains(query));
            }
            var result = hanghoas.Select(p => new HangHoaVM
            {
                MaHh = p.MaHh,
                tenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai
            });
            return View(result);
        }

        public IActionResult Detail(int id) 
        {
            var data = db.HangHoas
                .Include(p=>p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);
            if (data == null) 
            {
                TempData["Message"] = $"Không thấy sản phẩm có mã {id}";
                return Redirect("/404");
            }
            var result = new ChiTietHangHoaVM
            {
                //ko chuyển cái empty model qua
                MaHh = data.MaHh,
                tenHH = data.TenHh,
                DonGia = data.DonGia ??0,
                ChiTiet = data.MoTa ?? string.Empty,
                Hinh = data.Hinh,
                MoTaNgan = data.MoTaDonVi,
                TenLoai = data.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10,// tính sau
                DiemDanhGia =5, // check sau
            };
            return View(result);
        }
    }
}
