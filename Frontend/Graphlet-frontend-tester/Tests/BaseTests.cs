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
        protected RegisterPage registerPage;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Url = DefaultValues.base_url;

            loginPage = new LoginPage(driver);
            registerPage = new RegisterPage(driver);
        }


        [TearDown]
        public void Teardown()
        {
            driver.Dispose();
        }
    }
}
