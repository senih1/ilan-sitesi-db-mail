using Dapper;
using ilan_sitesi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace ilan_sitesi.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "Server=X;Initial Catalog=X;User Id =X; Password =X";

        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "SELECT * FROM ilan_sitesi WHERE isApproved = 1 ORDER BY id DESC;";
            var ads = connection.Query<Ad>(sql).ToList();
            return View(ads);
        }
    }
}
