using Ecomerce.Data;
using Ecomerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ecomerce.Helpers;
using Microsoft.AspNetCore.Authorization;
using Ecomerce.Services;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Ecomerce.Models;

namespace Ecomerce.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly Hshop2023Context db;
        private readonly IVnPayService _vnPayService;

        public CartController(Hshop2023Context context, PaypalClient paypal , IVnPayService vnPayservice) 
        {
            _paypalClient = paypal;
            db = context;
            _vnPayService = vnPayservice;
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
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }

        [Authorize]

        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 10000)
                    };
                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));   
                }

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
        [Authorize]
        public IActionResult PaymentSuccess() 
        {
            return View("Success");
        }
        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken) 
        {
            // Thông tin đơn hàng gửi qua Paypal
            var tongTien = Cart.Sum(p => p.ThanhTien).ToString();

            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();
            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);
                Console.WriteLine("Create Order Response: " + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex) 
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            // Kiểm tra orderID hợp lệ
            if (string.IsNullOrEmpty(orderID))
            {
                return BadRequest(new { error = "orderID không hợp lệ hoặc bị thiếu" });
            }

            Console.WriteLine($"Debug: orderID nhận được -> {orderID}");

            try
            {
                // Gửi yêu cầu đến PayPal để xác nhận thanh toán
                var response = await _paypalClient.CaptureOrder(orderID);
                Console.WriteLine($"PayPal Capture Response: {JsonConvert.SerializeObject(response)}");

                // Kiểm tra trạng thái giao dịch từ PayPal
                if (response.status != "COMPLETED")
                {
                    return BadRequest(new { error = "Giao dịch chưa hoàn tất. Trạng thái: " + response.status });
                }

                // Lấy số tiền và thông tin người thanh toán
                var amountPaid = response.purchase_units.First().payments.captures.First().amount.value;
                var currency = response.purchase_units.First().payments.captures.First().amount.currency_code;
                var payerId = response.payer.payer_id;
                var payerEmail = response.payer.email_address;

                // Log thông tin giao dịch
                Console.WriteLine($"Thanh toán thành công: {amountPaid} {currency}, PayerID: {payerId}, Email: {payerEmail}");

                return Ok(new
                {
                    message = "Thanh toán thành công",
                    amount = amountPaid,
                    currency,
                    payerId,
                    payerEmail
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi Capture Order: {ex.GetBaseException().Message}");
                return BadRequest(new { error = ex.GetBaseException().Message });
            }
        }

        #endregion
        [Authorize]
        public IActionResult PaymentFail() 
        {
            return View();
        }
        [Authorize]
        public IActionResult PaymentCallBack() 
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if(response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VNPay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            //Lưu đơn hàng vào database

            TempData["Message"] = $"Thanh toán VNPay thành công";
            return RedirectToAction("PaymentSuccess");
        }
    }
}
