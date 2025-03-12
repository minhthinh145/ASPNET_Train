using Ecomerce.Data;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
    }
}
