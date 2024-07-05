using Dapper;
using ilan_sitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.SqlServer.Server;
using static System.Formats.Asn1.AsnWriter;
using System.Reflection;

namespace ilan_sitesi.Controllers
{
    public class AdminController : Controller
    {
        string connectionString = "Server=X;Initial Catalog=X;User Id =X; Password =X";
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM ilan_sitesi WHERE isApproved = 1 ORDER BY id DESC";
            var ads = connection.Query<Ad>(sql).ToList();
            return View(ads);
        }

        [HttpPost]
        public IActionResult Add(Ad model)
        {
            if (!ModelState.IsValid)
            {
                //TempData["Message"] = $"<div class=\"alert alert-warning\" role=\"alert\">\r\n   Hatalı veya eksik ilan girişi!\r\n</div>\r\n";
                //return View();
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın.";
                return View("Message");

            }

            model.CreateDate = DateTime.Now;
            model.UpdateDate = DateTime.Now;

            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);
            var path = Path.Combine(/*Directory.GetCurrentDirectory(), */"wwwroot", "uploads", imageName);

            using var stream = new FileStream(path, FileMode.Create);
            model.Image.CopyTo(stream);

            ViewBag.Image = $"/uploads/{imageName}";
            string imageUrl = ViewBag.Image;

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO ilan_sitesi (name, price, detail, imageUrl, createDate, updateDate, email, sellerName) VALUES (@Name, @Price, @Detail, @ImageUrl, @CreateDate, @UpdateDate, @Email, @SellerName)";

            var data = new
            {
                Name = model.Name,
                Price = model.Price,
                Detail = model.Detail,
                CreateDate = model.CreateDate,
                UpdateDate = model.UpdateDate,
                ImageUrl = imageUrl,
                Email = model.Email,
                SellerName = model.sellerName,
            };

            var rowsAffected = connection.Execute(sql, data);

            var client = new SmtpClient("X", 587)
            {
                Credentials = new NetworkCredential("X","X"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("X", "İlan Sitesi - İlanınız Oluşturuldu"),
                Subject = $"Sevgili {model.sellerName}, {model.Name} isimli ilanınız sıraya alındı.",
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Ürün İlanı Aldık</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                line-height: 1.6;
                            }}
                            .bold {{
                                font-weight: bold;
                            }}
                        </style>
                    </head>
                    <body>
                        <p><span class='bold'>Sevgili {model.sellerName},</span></p>

                        <p><span class='bold'>{model.Name}</span> adlı ürün ilanınızı aldık. İlanınız, moderasyon sürecinden geçtikten sonra sitemizde yayımlanacaktır.</p>
                        <p>Onay süreci tamamlandığında size bildirim yapılacaktır.</p>
                        
                        <hr>    
                        <p><span class='bold'>İlan Detayları:</span></p>
                        <ul>
                            <li><strong>Ürün İsmi:</strong> {model.Name}</li>
                            <li><strong>Fiyat:</strong> {model.Price} TL</li>
                            <li><strong>Detaylar:</strong> {model.Detail}</li>
                        </ul>
                        <hr>

                        <p>Teşekkür ederiz.</p>

                        <p><span class='bold'>Saygılarımla,</span><br>
                        Senih</p>
                    </body>
                    </html>
                    ",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(new MailAddress(model.Email,model.sellerName));

            client.Send(mailMessage);

            //TempData["Message"] = $"<div class=\"alert alert-success\" role=\"alert\">\r\n  İlanınız başarı ile eklendi!\r\n</div>\r\n";
            //return View();
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "İlanınız başarı ile oluşturuldu. Onaylandıktan sonra listelemeye eklenicektir.";
            return View("Message");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM ilan_sitesi WHERE id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var post = connection.QuerySingleOrDefault<Ad>("SELECT * FROM ilan_sitesi WHERE id = @Id", new { Id = id });

            return View(post);
        }

        [HttpPost]
        public IActionResult Edit(Ad model)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "UPDATE ilan_sitesi SET name=@Name, price=@Price, detail=@Detail imageUrl = @ImageUrl WHERE id = @Id";

            var param = new
            {
                Name = model.Name,
                Price = model.Price,
                Detail = model.Detail,
                ImageUrl = model.ImageUrl,
                Id = model.Id,
            };

            var affectedRows = connection.Execute(sql, param);

            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        [HttpGet]
        public IActionResult Approve()
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM ilan_sitesi WHERE isApproved = 0 ORDER BY createDate DESC;";
            var ads = connection.Query<Ad>(sql).ToList();
            return View(ads);
        }

        [HttpGet]
        public IActionResult Approved(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT Id, Name, Price, Detail, Email, sellerName FROM ilan_sitesi WHERE Id = @Id";
            var model = connection.QueryFirstOrDefault(sql, new { Id = id });


            var sql1 = "UPDATE ilan_sitesi SET isApproved = 1, approveDate = GETDATE() WHERE id = @id;";
            var affectedRows = connection.Execute(sql1, new { Id = id });

            var client = new SmtpClient("X", 587)
            {
                Credentials = new NetworkCredential("X","X"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("X", "İlan Sitesi - İlanınız Onaylandı"),
                Subject = $"Sevgili {model.sellerName}, {model.Name} isimli ilanınız başarı ile onaylandı.",
                Body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <title>Ürün İlanı Onaylandı</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                line-height: 1.6;
                            }}
                            .bold {{
                                font-weight: bold;
                            }}
                        </style>
                    </head>
                    <body>
                        <p><span class='bold'>Sevgili {model.sellerName},</span></p>
                        <p><span class='bold'>{model.Name}</span> adlı ürün ilanınızı onaylandı.</p>
                        
                        <hr>    
                        <p><span class='bold'>İlan Detayları:</span></p>
                        <ul>
                            <li><strong>Ürün İsmi:</strong> {model.Name}</li>
                            <li><strong>Fiyat:</strong> {model.Price} TL</li>
                            <li><strong>Detaylar:</strong> {model.Detail}</li>
                        </ul>
                        <hr>

                        <p>Teşekkür ederiz.</p>

                        <p><span class='bold'>Saygılarımla,</span><br>
                        Senih</p>
                    </body>
                    </html>
                    ",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(new MailAddress(model.Email,model.sellerName));
            client.Send(mailMessage);

            ViewBag.Message = "Onaylandı.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        [HttpGet]
        public IActionResult UnApproved(int id)
        {
            using var connection = new SqlConnection(connectionString);
           
            var sql1 = "DELETE FROM ilan_sitesi WHERE id = @id;";
            var affectedRows = connection.Execute(sql1, new { Id = id });

            ViewBag.Message = "Silindi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }
    }
}
