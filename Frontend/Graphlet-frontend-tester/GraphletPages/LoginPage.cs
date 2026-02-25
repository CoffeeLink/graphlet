using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Graphlet_frontend_tester.GraphletPages
{
    public class LoginPage:BasePage
    {
        public static readonly string URL = DefaultValues.base_url+"login";

        IWebElement emailField => driver.FindElement(By.ClassName("emailInput"));
        IWebElement passwordField => driver.FindElement(By.ClassName("passwordInput"));
        IWebElement loginButton => driver.FindElement(By.Id("loginButton"));
        IWebElement registerLink => driver.FindElement(By.Id("registerLink"));


        public LoginPage(IWebDriver driver) : base(driver)
        {
        }

        public void Login(string email, string password)
        {
            emailField.SendKeys(email);
            passwordField.SendKeys(password);
            loginButton.Click();
        }

        public void GoToRegisterPage()
        {
            registerLink.Click();
        }


    }
}
