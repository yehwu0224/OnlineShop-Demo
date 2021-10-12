using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Helpers;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private readonly OnlineShopContext _context;

        public CartController(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<CartItem> CartItems = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");

            if (CartItems != null)
            {
                ViewBag.Total = CartItems.Sum(m => m.SubTotal); // 計算商品總額
            }
            else
            {
                ViewBag.Total = 0;
            }

            return View(CartItems);
        }


        public IActionResult AddtoCart(int id)
        {
            var product = _context.Product.Single(x => x.Id.Equals(id));
            CartItem item = new CartItem()
            {
                ProductId = product.Id,
                Product = product,
                Amount = 1,
                SubTotal = product.Price,
                imageSrc = ViewImage(product.Image)
            };


            if (SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart") == null)
            {
                //如果沒有已存在購物車: 建立新的購物車
                List<CartItem> cart = new List<CartItem>();
                cart.Add(item);
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            else
            {
                //如果已存在購物車: 檢查有無相同的商品，有的話只調整數量
                List<CartItem> cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");

                int index = cart.FindIndex(m => m.Product.Id.Equals(id)); // FindIndex查詢位置
                
                if (index != -1)
                {
                    cart[index].Amount += item.Amount;
                    cart[index].SubTotal += item.SubTotal;
                }
                else
                {
                    cart.Add(item);
                }
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }

            return NoContent(); // HttpStatus 204: 請求成功但不更新畫面
        }

        public IActionResult RemoveItem(int id)
        {
            List<CartItem> cart = SessionHelper.GetObjectFromJson<List<CartItem>>(HttpContext.Session, "cart");
            int index = cart.FindIndex(m => m.Product.Id.Equals(id)); //FindIndex查詢位置
            cart.RemoveAt(index);

            if(cart.Count < 1)
            {
                SessionHelper.Remove(HttpContext.Session, "cart");
            }
            else
            {
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }

            return RedirectToAction("Index");
        }

        private string ViewImage(byte[] arrayImage)
        {
            string base64String = Convert.ToBase64String(arrayImage, 0, arrayImage.Length);
            return "data:image/png;base64," + base64String;
        }
    }
}
