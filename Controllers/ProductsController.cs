using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Bai9._1.Models;

namespace Bai9._1.Controllers
{
    public class ProductsController : Controller
    {
        private Model1 db = new Model1();

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,Description,PurchasePrice,Price,Quantity,Vintage,CategoryID,Region")] Product product, HttpPostedFileBase imageFile)
        {
            string imageName;
            if (ModelState.IsValid && TrySaveImage(imageFile, out imageName))
            {
                product.Image = imageName;
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Description,PurchasePrice,Price,Quantity,Vintage,CategoryID,Region")] Product product, HttpPostedFileBase imageFile)
        {
            var existingProduct = db.Products.Find(product.ProductID);
            if (existingProduct == null)
            {
                return HttpNotFound();
            }

            product.Image = existingProduct.Image;
            string imageName;
            if (ModelState.IsValid && TrySaveImage(imageFile, out imageName))
            {
                if (imageName != null)
                {
                    product.Image = imageName;
                }
                db.Entry(existingProduct).CurrentValues.SetValues(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool TrySaveImage(HttpPostedFileBase imageFile, out string imageName)
        {
            imageName = null;
            if (imageFile == null || imageFile.ContentLength == 0)
            {
                return true;
            }

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("imageFile", "Chỉ chấp nhận ảnh JPG, JPEG, PNG, GIF hoặc BMP.");
                return false;
            }

            if (imageFile.ContentLength > 3 * 1024 * 1024)
            {
                ModelState.AddModelError("imageFile", "Ảnh phải có dung lượng tối đa 3 MB.");
                return false;
            }

            try
            {
                using (var image = System.Drawing.Image.FromStream(imageFile.InputStream, false, true))
                {
                    // Decode the file to verify that it contains an image.
                }
                imageFile.InputStream.Position = 0;
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError("imageFile", "File đã chọn không phải là ảnh hợp lệ.");
                return false;
            }

            try
            {
                var directory = Server.MapPath("~/Content/WineImages");
                Directory.CreateDirectory(directory);
                var fileName = Guid.NewGuid().ToString("N") + extension;
                imageFile.SaveAs(Path.Combine(directory, fileName));
                imageName = fileName;
                return true;
            }
            catch (IOException)
            {
                ModelState.AddModelError("imageFile", "Không thể lưu ảnh. Vui lòng thử lại.");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                ModelState.AddModelError("imageFile", "Ứng dụng chưa có quyền ghi vào thư mục Content/WineImages.");
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
