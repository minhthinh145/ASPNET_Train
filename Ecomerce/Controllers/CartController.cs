using Ecomerce.Data;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Helpers;
using Microsoft.AspNetCore.Authorization;

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
        [HttpPost]
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY);
            var item = cart?.FirstOrDefault(p => p.MaHh == productId);
            if (item != null)
            {
                item.SoLuong = quantity;
            }
            HttpContext.Session.Set(MySetting.CART_KEY, cart);

            // Tính toán lại
            var total = cart.Sum(x => x.SoLuong * x.DonGia);
            var totalQuantity = cart.Sum(x => x.SoLuong);
            var itemTotal = item.SoLuong * item.DonGia;

            return Json(new
            {
                success = true,
                total,
                totalQuantity,
                itemTotal
            });
        }
        [Authorize]

        [HttpGet]
        public IActionResult Checkout() 
        {
            if (Cart.Count == 0) 
            {
                Redirect("/");
            }
            return View(Cart);
        }

        [Authorize]

        [HttpPost]
        public IActionResult Checkout(CheckoutVM model)
        {
            if (ModelState.IsValid)
            {
                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                var khachHang = new KhachHang();
                if (model.GiongKhachHang)
                {
                    khachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                }

                var hoadon = new HoaDon
                {
                    MaKh = customerId,
                    HoTen = model.HoTen ?? khachHang.HoTen,
                    DiaChi = model.DiaChi ?? khachHang.DiaChi,
                    SoDienThoai = model.DienThoai ?? khachHang.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };

                using var transaction = db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges(); 

                    var cthd = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthd.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            MaHh = item.MaHh,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(cthd);
                    db.SaveChanges(); 

                    transaction.Commit(); 

                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>()); // reset giỏ hàng

                    return View("Success"); 
                }
                catch
                {
                    transaction.Rollback(); 
                    throw; 
                }
            }
            return View(Cart); 
        }

    }
}
