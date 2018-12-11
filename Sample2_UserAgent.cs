using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
//Reportium required using statements
using Reportium.test;
using Reportium.test.Result;
using Reportium.client;
using Reportium.model;

namespace UserAgent
{
    /// <summary>
    /// Summary description for RemoteWebDriverTest
    /// 
    /// For programming samples and updated templates refer to the Perfecto GitHub at: https://github.com/PerfectoCode
    /// </summary>
    [TestClass]
    public class RemoteWebDriverTest
    {
        private RemoteWebDriverExtended driver;
        private ReportiumClient reportClient;

        [TestInitialize]
        public void PerfectoOpenConnection()
        {
            var browserName = "mobileOS";
            var host = "YOUR CQ LAB";

            DesiredCapabilities capabilities = new DesiredCapabilities(browserName, string.Empty, new Platform(PlatformType.Any));
            capabilities.SetCapability("user", "[ENTER YOUR USER NAME HERE]");

            //TODO: Provide your password or security token
            capabilities.SetCapability("password", "[ENTER YOUR PASSWORD HERE]");
            //capabilities.SetCapability("securityToken", "[ENTER YOUR SECURITY TOKEN HERE]");

            //TODO: Provide your device selection criteria
            capabilities.SetCapability("platformName", "Android");

            capabilities.SetPerfectoLabExecutionId(host);

            // Name your script
            capabilities.SetCapability("scriptName", "Test user agent");

            var url = new Uri(string.Format("https://{0}/nexperience/perfectomobile/wd/hub", host));
            driver = new RemoteWebDriverExtended(new HttpAuthenticatedCommandExecutor(url), capabilities);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

            // Create the Reporting Client agent : for more information see https://developers.perfectomobile.com/pages/viewpage.action?pageId=12419423
            // Create the PerfectoExecutionContext instance to provide the Execution Test Report metadata
            PerfectoExecutionContext peContext = new PerfectoExecutionContext.PerfectoExecutionContextBuilder()
              .withContextTags(new[] { "sample tag1", "userAgent sample", "c#" })
              .withWebDriver(driver)
              .build();
            reportClient = PerfectoClientFactory.createPerfectoReportiumClient(peContext);
        }

        [TestCleanup]
        public void PerfectoCloseConnection()
        {
            driver.Close();

            driver.Quit();
            // retrieve the STR URL
            String reportLoc = reportClient.getReportUrl();
            Trace.WriteLine("Find your execution report at: " + reportLoc);
        }

        [TestMethod]
        public void WebDriverTestMethod()
        {
            //Write your test here
            reportClient.testStart("User Agent test", new TestContextTags("assignment2", "training"));

            Dictionary<String, Object> pars = new Dictionary<String, Object>();

            try
            {
                reportClient.stepStart("Open the website");
                driver.Navigate().GoToUrl("https://training.perfecto.io");
                pars.Clear();
                pars.Add("content", "Training Website");
                pars.Add("timeout", 20);
                String res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);

                if (!res.ToLower().Equals("true"))
                {
                    reportClient.reportiumAssert("homepage loaded", false);
                }
                reportClient.stepEnd();

                reportClient.stepStart("navigate to agent page");
                driver.FindElementByXPath("//*[@class=\"mobile-menu\"]").Click();

                pars.Clear();
                pars.Add("label", "Agent");
                pars.Add("timeout", 20);
                driver.ExecuteScript("mobile:button-text:click", pars);

                String result = (String)driver.FindElementByXPath("//*[text()=\"The User Agent\"]").Text;
                Trace.WriteLine(result);

                reportClient.testStop(TestResultFactory.createSuccess());
            }
            catch (Exception e)
            {
                reportClient.testStop(TestResultFactory.createFailure(e.Message, e));
                Trace.Write(e.StackTrace);
                Trace.WriteLine(" ");
            }

        }
    }
}
