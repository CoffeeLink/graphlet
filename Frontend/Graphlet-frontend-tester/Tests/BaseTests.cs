using OpenQA.Selenium;
using NUnit.Framework;
using OpenQA.Selenium.Chrome;
using System;
using Graphlet_frontend_tester.GraphletPages;

namespace Graphlet_frontend_tester.Tests
{
    [TestFixture]
    public class BaseTests
    {
        protected IWebDriver driver;
        protected LoginPage loginPage;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Url = "http://localhost:5173/";

            loginPage = new LoginPage(driver);
        }

        [Test]
        public void Open()
        {
            
        }


        [TearDown]
        public void Teardown()
        {
            driver.Dispose();
        }
    }
}
