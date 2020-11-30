using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// Include models for CarAdvert model
using web_scraper.Models;

using AngleSharp;
using AngleSharp.Html.Parser;

namespace web_scraper.Controllers
{
 [Route("[controller]")]
    [ApiController]
    public class WebScraperController : ControllerBase
    {
        /**
         * The website I'm scraping has data where the paths are relative
         * so I need a base url set somewhere to build full url's
         */
        private readonly String websiteUrl = "https://kolesa.kz/cars/audi/";
        private readonly ILogger<WebScraperController> _logger;

        // Constructor
        public WebScraperController(ILogger<WebScraperController> logger)
        {
            _logger = logger;
        }

        private async Task<List<dynamic>> GetPageData(string url, List<dynamic> results)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(url);

            // Debug
            //_logger.LogInformation(document.DocumentElement.OuterHtml);

            var advertrows = document.QuerySelectorAll("tr.result-row");

            foreach (var row in advertrows)
            {
                // Create a container object
                CarAdvert advert = new CarAdvert();

                // Use regex to get all the numbers from this string
                MatchCollection regxMatches = Regex.Matches(row.QuerySelector(".price").TextContent, @"\d+\.*\d+");
                uint.TryParse(string.Join("", regxMatches), out uint price);
                advert.Price = price;

                regxMatches = Regex.Matches(row.QuerySelector(".year").TextContent, @"\d+");
                uint.TryParse(string.Join("", regxMatches), out uint year);
                advert.Year = year;

                // Get the fuel type from the ad
                advert.Fuel = row.QuerySelector(".fuel").TextContent[0];

                // Make and model
                advert.MakeAndModel = row.QuerySelector(".make_and_model > a").TextContent;
                
                // Link to the advert
                advert.AdvertUrl = websiteUrl + row.QuerySelector(".make_and_model > a").GetAttribute("Href");

                results.Add(advert);
            }

            // Check if a next page link is present
            string nextPageUrl = "";
            var nextPageLink = document.QuerySelector(".next-page > .item");
            if (nextPageLink != null)
            {
                nextPageUrl = websiteUrl + nextPageLink.GetAttribute("Href");
            }

            // If next page link is present recursively call the function again with the new url
            if (!String.IsNullOrEmpty(nextPageUrl))
            {
                return await GetPageData(nextPageUrl, results);
            }

            return results;
        }

        private async void CheckForUpdates(string url, string mailTitle)
        {
            // We create the container for the data we want
            List<dynamic> adverts = new List<dynamic>();

            /**
             * GetPageData will recursively fill the container with data
             * and the await keyword guarantees that nothing else is done
             * before that operation is complete.
             */
            await GetPageData(url, adverts);

            // TODO: Diff the data
        }

        [HttpGet]
        public string Get()
        {
            CheckForUpdates("<url to website>", "Web-Scraper updates");
            return "Hello";
        }
    }
}