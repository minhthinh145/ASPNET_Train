using Ecomerce.Data;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace Ecomerce.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly Hshop2023Context db;

        //read datbase
        public MenuLoaiViewComponent(Hshop2023Context context) => db = context;

        public IViewComponentResult Invoke() 
        {
            //cần hiển thị danh sách các loại , lấy tnê loại + số lg sản phẩm thuộc loại đó
            var data = db.Loais.Select(lo => new MenuLoaiVM
            {
                MaLoai = lo.MaLoai , TenLoai = lo.TenLoai , SoLuong  = lo.HangHoas.Count
            }).OrderBy(p => p.TenLoai);
            return View(data); // Default.cshtml
            //or đặt tên :))
            // return View("Default",data);
        }
    }
}
