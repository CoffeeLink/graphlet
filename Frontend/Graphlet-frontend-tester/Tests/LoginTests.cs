using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Graphlet_frontend_tester.GraphletPages;
using OpenQA.Selenium;

namespace Graphlet_frontend_tester.Tests
{
    [TestFixture]
    internal class LoginTests : BaseTests
    {

        [Test]
        public void LoginWithValidCredentials()
        {
            loginPage.Login("demo@example.com", "demo");
            Thread.Sleep(200);
            IWebElement message = driver.FindElement(By.Id("successful-login-message"));
            Assert.That(message.Displayed, Is.True);
            Thread.Sleep(1200);
            Assert.That(driver.Url, Is.EqualTo(DefaultValues.base_url+"workspaces"));
        }

        [Test]
        public void LoginWithEmptyCredentials()
        {
            loginPage.Login("", "");
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void LoginWithEmptyEmail()
        {
            loginPage.Login("", "demo");
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void LoginWithEmptyPassword()
        {
            loginPage.Login("demo@example.com", "");
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void LoginWithInvalidCredentials()
        {
            loginPage.Login("wrong one", "weri vrong");
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void LoginWithInvalidPassword()
        {
            loginPage.Login("demo@example.com", "wrong pswd");
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
        }

        [Test]
        public void LoginWithInvalidEmail()
        {
            loginPage.Login("wrong email", "demo");
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
            Thread.Sleep(120);
            IWebElement message = driver.FindElement(By.ClassName("error-text"));
            Assert.That(message.Displayed, Is.True);
        }

        [Test]
        public void GoToRegisterPageShouldWork()
        {
            loginPage.GoToRegisterPage();
            Assert.That(driver.Url, Is.EqualTo(RegisterPage.URL));

        }
    }
}
