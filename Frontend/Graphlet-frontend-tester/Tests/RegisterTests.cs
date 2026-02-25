using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Graphlet_frontend_tester.GraphletPages;
using OpenQA.Selenium;
using System.Threading;

namespace Graphlet_frontend_tester.Tests
{
    [TestFixture]
    internal class RegisterTests : BaseTests
    {

[SetUp]
        public void SetUp()
        {
            driver.Url = RegisterPage.URL;
            Thread.Sleep(20);
        }
        [Test]
        public void RegisterWithValidCredentials()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string email = $"user{now}@example.com";
            string password = "P@" + now;
            string username = "user" + now;
            registerPage.Register(email, password, username);
            Thread.Sleep(1200);
            Assert.That(driver.Url, Is.EqualTo(DefaultValues.base_url+"login"));
        }

        [Test]
        public void SuccessfullRegisterShouldShowMeaasge()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string email = $"user{now}@example.com";
            string password = "P@" + now;
            string username = "user" + now;
            registerPage.Register(email, password, username);
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.Id("successful-register-message"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void RegisterWithEmptyCredentials()
        {
            registerPage.Register("", "", "");
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void RegisterWithEmptyEmail()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            registerPage.Register("", "P@" + now, "user" + now);
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void RegisterWithEmptyPassword()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            registerPage.Register($"user{now}@example.com", "", "user" + now);
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void RegisterWithInvalidCredentials()
        {
            registerPage.Register("wrong one", "weri vrong", "wrongname");
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void RegisterWithInvalidPassword()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            registerPage.Register($"user{now}@example.com", "wrong pswd", "user" + now);
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));
        }

        [Test]
        public void RegisterWithInvalidEmail()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            registerPage.Register("wrong email", "P@" + now, "user" + now);
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void GoToLoginPageShouldWork()
        {
            // ensure we're on the register page before looking for the login link
            driver.Url = RegisterPage.URL;
            Thread.Sleep(200);
            registerPage.GoToLoginPage();
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));

        }
    }
}
