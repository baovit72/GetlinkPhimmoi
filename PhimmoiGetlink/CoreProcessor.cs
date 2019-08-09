using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support;
using System.Threading;
using System.Diagnostics;
using System.Net;
namespace PhimmoiGetlink
{
    class CoreProcessor
    {
        private ChromeDriver driver;
        private static CoreProcessor __Instance;
        public static CoreProcessor GetInstance()
        {
            if (__Instance == null)
                __Instance = new CoreProcessor();
            return __Instance;
        }

        private CoreProcessor()
        {
            ChromeOptions opt = new ChromeOptions();
            opt.AddExtension("uBlock.crx");
            //opt.AddArgument("--headless");
            driver = new ChromeDriver(opt);
        }

        public string GetHLSURL(string url)
        {
             
            driver.Navigate().GoToUrl(url);
            //Lấy link hls
            object data = ((IJavaScriptExecutor)driver).ExecuteScript("var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntries() || {}; var urls = network.filter((item)=>{return item.name.includes(\"playlist.m3u8\");}).map((item)=>item.name) || {}; if(urls.length>0) return urls[0]; else return \"Nope\"; ");
            driver.Navigate().GoToUrl("https://google.com");
            Debug.WriteLine(data.ToString());

            return data.ToString();
        }

        public void Dispose()
        {
            driver.Quit();
        }
    }
}
