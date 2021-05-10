﻿using System;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

namespace SendGet
{
    class Program
    {
        static void Main(string[] args)
        {
            callGet("INSERT_SECRET","INSERT_SECRET","INSERT_SECRET");
        }

         public static string CreateHMAC(
            string sharedKey,
            string secretKey,
            string date,
            string httpMethod,
            string requestURL,
            string contentType = null,
            string contentMD5 = null,
            string nepApplicationKey = null,
            string nepCorrelationID = null,
            string nepOrganization = null,
            string nepServiceVersion = null)
        {
            Uri url = new Uri(requestURL);

            string pathAndQuery = url.PathAndQuery;

            Console.WriteLine(date);
            string secretDate = date + ".000Z";

            string oneTimeSecret = secretKey + secretDate;
            string toSign = httpMethod + "\n" + pathAndQuery;

            if (!String.IsNullOrEmpty(contentType))
            {
                toSign += "\n" + contentType;
            }
            if (!String.IsNullOrEmpty(contentMD5))
            {
                toSign += "\n" + contentMD5;
            }
            if (!String.IsNullOrEmpty(nepApplicationKey))
            {
                toSign += "\n" + nepApplicationKey;
            }
            if (!String.IsNullOrEmpty(nepCorrelationID))
            {
                toSign += "\n" + nepCorrelationID;
            }
            if (!String.IsNullOrEmpty(nepOrganization))
            {
                toSign += "\n" + nepOrganization;
            }
            if (!String.IsNullOrEmpty(nepServiceVersion))
            {
                toSign += "\n" + nepServiceVersion;
            }


            var data = Encoding.UTF8.GetBytes(toSign);
            var key = Encoding.UTF8.GetBytes(oneTimeSecret);
            byte[] hash = null;

            using (HMACSHA512 shaM = new HMACSHA512(key))
            {
                hash = shaM.ComputeHash(data);
            }

            Console.WriteLine(sharedKey + ":" + System.Convert.ToBase64String(hash));
            string accessKey = sharedKey + ":" + System.Convert.ToBase64String(hash);
            return accessKey;
        }

        public static void callGet(String secretKey, String sharedKey, String nepOrganization){
            String url = "https://gateway-staging.ncrcloud.com/site/sites/find-nearby/88.05,46.25?radius=10000";
            String httpMethod = "GET";
            String contentType = "application/json";
            DateTime utcDate = DateTime.UtcNow;

            String hmacAccessKey = CreateHMAC(sharedKey, secretKey, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"), httpMethod, url, contentType, "", "", "", nepOrganization, "");

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var gmtDate = utcDate.DayOfWeek.ToString().Substring(0,3) + ", " + utcDate.ToString("dd MMM yyyy HH:mm:ss") + " GMT";

            request.AddHeader("nep-organization", nepOrganization);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("date", gmtDate);
            request.AddHeader("authorization", "AccessKey " + hmacAccessKey);

            IRestResponse response = client.Execute(request);

            Console.WriteLine(response.Content);
        }
    }
}
