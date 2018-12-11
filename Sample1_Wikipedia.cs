using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using Reportium.test;
using Reportium.test.Result;
using Reportium.client;
using Reportium.model;

namespace Wiki2Sample
{
	/// <summary>
	/// This template is for users that use DigitalZoom Reporting (ReportiumClient).
	/// For any other use cases please see the basic template at https://github.com/PerfectoCode/Templates.
	/// For more programming samples and updated templates refer to the Perfecto Documentation at: http://developers.perfectomobile.com/
	/// </summary>
	[TestClass]
	public class RemoteWebDriverTest
	{
		private RemoteWebDriver driver;
		private ReportiumClient reportiumClient;

		[TestInitialize]
		public void PerfectoOpenConnection()
		{
			var browserName = "mobileOS";
			var host = "[ENTER YOUR CQ LAB NAME HERE]";

			DesiredCapabilities capabilities = new DesiredCapabilities(browserName, string.Empty, new Platform(PlatformType.Any));
			capabilities.SetCapability("user", "[ENTER YOUR USER NAME HERE]");

			//TODO: Provide your password or security token
			capabilities.SetCapability("password", "[ENTER YOUR PASSWORD HERE]");
            //capabilities.SetCapability("securityToken", "[ENTER YOUR SECURITY TOKEN HERE]");

			//TODO: Provide your device ID
			capabilities.SetCapability("platformName", "Android");

			capabilities.SetPerfectoLabExecutionId(host);

			// Name your script
			capabilities.SetCapability("scriptName", "Wikipedia sample");

			var url = new Uri(string.Format("http://{0}/nexperience/perfectomobile/wd/hub", host));
			driver = new RemoteWebDriver(url, capabilities);
			driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(15));

			// Reporting client. For more details, see http://developers.perfectomobile.com/display/PD/Reporting
			PerfectoExecutionContext perfectoExecutionContext = new PerfectoExecutionContext.PerfectoExecutionContextBuilder()
					.withProject(new Project("My Project", "1.0"))
					.withJob(new Job("My Job", 45))
					.withContextTags(new[] { "sampleTag1", "Wikipedia", "c#" })
					.withWebDriver(driver)
					.build();
			reportiumClient = PerfectoClientFactory.createPerfectoReportiumClient(perfectoExecutionContext);
		}

		[TestCleanup]
		public void PerfectoCloseConnection()
		{
			driver.Quit();

			// Retrieve the URL of the Single Test Report, can be saved to your execution summary and used to download the report at a later point
			String reportURL = reportiumClient.getReportUrl();
			Trace.WriteLine("Find your execution report at: " + reportURL);

			// For documentation on how to export reporting PDF, see https://github.com/perfectocode/samples/wiki/reporting
			//String reportPdfUrl = (String)(driver.Capabilities.GetCapability("reportPdfUrl"));

			// For detailed documentation on how to export the Execution Summary PDF Report, the Single Test report and other attachments such as
			// video, images, device logs, vitals and network files - see http://developers.perfectomobile.com/display/PD/Exporting+the+Reports
		}

		[TestMethod]
		public void WebDriverTestMethod()
		{
			Dictionary<String, Object> pars = new Dictionary<String, Object>();

			try
			{
				reportiumClient.testStart("Wikipedia sample", new TestContextTags("sample1", "apples"));

				// write your code here
                reportiumClient.stepStart("Open Wikipedia");
                driver.Navigate().GoToUrl("www.wikipedia.org");
                pars.Clear();
                pars.Add("content", "The Free Encyclopedia");
                pars.Add("timeout", 20);
                String res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);

                if (!res.ToLower().Equals("true"))
                {
                    reportiumClient.reportiumAssert("homepage loaded", false);
                }
                reportiumClient.stepEnd();

                reportiumClient.stepStart("Looking for term");
                driver.FindElementByXPath("//*[@id='searchInput']").SendKeys("apple");
                System.Threading.Thread.Sleep(2000);

                driver.FindElementByXPath("//*[@class=\"pure-button pure-button-primary-progressive\"]").Click();
                reportiumClient.stepEnd();

                reportiumClient.stepStart("Visual Analysis to verify on right page");
                pars.Clear();
                pars.Add("content", "fruit");
                pars.Add("timeout", 20);
                res = (String)driver.ExecuteScript("mobile:checkpoint:text", pars);
                if (!res.ToLower().Equals("true"))
                {
                    reportiumClient.reportiumAssert("search term loaded", false);
                }
                reportiumClient.stepEnd();

				reportiumClient.testStop(TestResultFactory.createSuccess());
			}
			catch (Exception e)
			{
				reportiumClient.testStop(TestResultFactory.createFailure(e.Message, e));
				Trace.Write(e.StackTrace);
				Trace.WriteLine(" ");
			}
		}
	}
}
