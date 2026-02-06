using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.Helpers
{
    public static class Utils
    {
        public static string GetCourierSrc(string courier)
        {
            if (string.IsNullOrWhiteSpace(courier))
                return string.Empty;

            // Remove invalid characters
            courier = courier.Replace(":", "").Trim();

            string basePath = "images/couriers/";
            string pngPath = $"{basePath}{courier}.png";
            string jpgPath = $"{basePath}{courier}.jpg";

            // Map to physical paths on server to check existence
            string wwwRoot = Path.Combine(Environment.CurrentDirectory, "wwwroot");
            string physicalPng = Path.Combine(wwwRoot, pngPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            string physicalJpg = Path.Combine(wwwRoot, jpgPath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(physicalPng))
                return pngPath;

            if (File.Exists(physicalJpg))
                return jpgPath;

            // fallback if no file exists
            return string.Empty;
        }
    }
}