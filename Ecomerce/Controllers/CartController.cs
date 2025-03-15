using Ecomerce.Data;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Helpers;

namespace Ecomerce.Controllers
{
    public class CartController : Controller
    {
        private readonly Hshop2023Context db;

        public CartController(Hshop2023Context context) 
        {
            db = context;
        }
 
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem 
                {
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity

                };
                gioHang.Add(item);
            }
            else 
            {
                item.SoLuong += quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            return RedirectToAction("Index");
        }
        public IActionResult RemoveCart(int id) 
            {
            foreach (var c in Cart) 
            {
                Console.WriteLine($"Id của từng item : {c.MaHh}");
            }
            var gioHang = Cart;
            Console.WriteLine("Giỏ hàng: " + string.Join(", ", gioHang.Select(item => item.MaHh)));
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null) 
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            else
            {
                Console.WriteLine($"Không tìm thấy item có mã {id} trong giỏ hàng.");
            }
            return RedirectToAction("Index");
        }
    }
}
