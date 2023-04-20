using HtmlAgilityPack;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json.Linq;

namespace TradeIndia
{
    public class Program
    {
        static void Main(string[] args)
        {
            var parentCategories = new List<ParentCategories>();
            ChromeDriver driver = new ChromeDriver(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
            driver.Navigate().GoToUrl("https://www.tradeindia.com/seller/");
            var htmlsource = driver.PageSource;

            var html = GetHtmlDocument(htmlsource);

            //string AuctionPageSource = GetFullyLoadedWebPageContent(driver);
            //var node = html.DocumentNode.SelectNodes("//div[contains(@class, \"cat-det-wrp\")]");
            var node2 = html.DocumentNode.SelectSingleNode("//script[@id = \"__NEXT_DATA__\"]").InnerText;

            var json = JObject.Parse(node2);

            var s = json["props"]["pageProps"]["serverData"]["seller_main_cate_list_data"].ToList();

            foreach(var node in s)
            {
                var parentCategory = new ParentCategories();
                parentCategory.Id = node["category_id"].ToString();
                parentCategory.Name = node["cat_name"].ToString();
                parentCategory.Url = $"https://www.tradeindia.com{node["cat_url"]}";

                parentCategories.Add(parentCategory);

                LoadChildCategories(parentCategory, driver);
            }
        }

        private static void LoadChildCategories(ParentCategories parentCategory, ChromeDriver driver)
        {
            var childCategories = new List<ChildCategories>();
            driver.Navigate().GoToUrl(parentCategory.Url);
            var html = GetHtmlDocument(driver.PageSource);

            var categoryhtml = html.DocumentNode.SelectNodes("//div[contains(@class,\"row cat-row\")]/div");

            foreach(var category in categoryhtml)
            {
                var childCategory = new ChildCategories();
                childCategory.Name = category.SelectSingleNode("//h3[contains(@class,\"Typography__Title1\")]").InnerText;
                childCategory.ParentCategory = parentCategory.Name;
                childCategories.Add(childCategory);

                LoadProducts(category, childCategory.Name, parentCategory.Name);

            }
        }

        private static void LoadProducts(HtmlNode category, string childCategory, string parentCategory)
        {
            var productList = new List<Product>();
            var productsNode = category.SelectNodes("//p[contains(@class,\"Typography__Body\")]/a");

            foreach(var node in productsNode)
            {
                var product = new Product();
                product.Name = node.InnerText;
                product.ChildCategory = childCategory;
                product.ParentCategory = parentCategory;

                productList.Add(product);
            }

        }

        private static HtmlDocument GetHtmlDocument(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc;
        }

        private static HtmlDocument GetHTML(string url)
        {
            try
            {
                HtmlWeb doc = new HtmlWeb();
                WebProxy wc = new WebProxy("127.0.0.1", 24005);

                var html = doc.Load(url, "GET", wc, null);
                return html;
            }
            catch (Exception ex)
            {
                ex.ToString();

                throw;
            }
        }
    }
}
