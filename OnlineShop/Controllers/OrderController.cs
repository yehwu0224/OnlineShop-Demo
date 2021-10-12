using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Areas.Identity.Data;
using OnlineShop.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OnlineShopContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(OnlineShopContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region 訂單列表

        // 訂單列表
        public async Task<IActionResult> Index()
        {
            
            List<OrderViewModel> orderVM = new List<OrderViewModel>();

            var userId = _userManager.GetUserId(User);
            var orders = await _context.Order.
                OrderByDescending(k => k.OrderDate).            //用日期排序
                Where(m => m.UserId == userId).ToListAsync();   //取得屬於當前登入者的訂單

            foreach(var item in orders)
            {
                item.OrderItem = await _context.OrderItem.
                    Where(p => p.OrderId == item.Id).ToListAsync(); //取得訂單內的商品項目

                var ovm = new OrderViewModel()
                {
                    Order = item,
                    CartItems = GetOrderItems(item.Id)
                };

                orderVM.Add(ovm);
            }

            return View(orderVM);
        }

        // 訂單資訊
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FirstOrDefaultAsync(m => m.Id == Id);
            if (order.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            else
            {
                order.OrderItem = await _context.OrderItem.Where(p => p.OrderId == Id).ToListAsync();
                ViewBag.orderItems = GetOrderItems(order.Id);
            }

            return View(order);
        }

        #endregion

        #region 結帳流程

        // 結帳
        public IActionResult Checkout()
        {
            //確認 Session 內存在購物車
            if (SessionHelper.GetObjectFromJson<List<OrderItem>>(HttpContext.Session, "cart") == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            //建立新的訂單
            var myOrder = new Order()
            {
                UserId = _userManager.GetUserId(User),
                UserName = _userManager.GetUserName(User),
                OrderItem = SessionHelper.GetObjectFromJson<List<OrderItem>>(HttpContext.Session, "cart")
            };
            myOrder.Total = myOrder.OrderItem.Sum(m => m.SubTotal);
            ViewBag.CartItems = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");

            return View(myOrder);
        }

        // 新增訂單到資料庫
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            //新增訂單到資料庫
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.isPaid = false;
                order.OrderItem = SessionHelper.GetObjectFromJson<List<OrderItem>>(HttpContext.Session, "cart");

                _context.Add(order);
                await _context.SaveChangesAsync();
                SessionHelper.Remove(HttpContext.Session, "cart");

                return RedirectToAction("ReviewOrder", new {  Id = order.Id } );
            }
            return View();
        }

        // 取得當前訂單
        public async Task<IActionResult> ReviewOrder(int? Id)
        {
            if(Id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FirstOrDefaultAsync(m => m.Id == Id); 
            if(order.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            else
            {
                order.OrderItem = await _context.OrderItem.Where(p => p.OrderId == Id).ToListAsync();
                ViewBag.orderItems = GetOrderItems(order.Id);
            }

            return View(order);
        }

        // 付款
        public async Task<IActionResult> Payment(int? Id, bool isSuccess)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FirstOrDefaultAsync(p => p.Id == Id);
            if(order == null)
            {
                return NotFound();
            }
            else
            {
                if (isSuccess)
                {
                    order.isPaid = true;
                    _context.Update(order);
                    await _context.SaveChangesAsync();  //更新訂單狀態
                }
                return RedirectToAction("ReviewOrder", new { Id = order.Id });
            }
        }

        #endregion

        // 取得商品詳細資料
        private List<CartItem> GetOrderItems(int orderId)
        {
            
            var OrderItems = _context.OrderItem.Where(p => p.OrderId == orderId).ToList();

            List<CartItem> orderItems = new List<CartItem>();
            foreach (var orderitem in OrderItems)
            {
                CartItem item = new CartItem(orderitem);
                item.Product = _context.Product.Single(x => x.Id == orderitem.ProductId);
                orderItems.Add(item);
            }

            return orderItems;
        }

    }
}
