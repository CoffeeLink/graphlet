using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using Graphlet_frontend_tester.GraphletPages;

namespace Graphlet_frontend_tester.GraphletPages
{
    public class RegisterPage: BasePage
    {
        public static readonly string URL = DefaultValues.base_url + "register";

        IWebElement emailField => driver.FindElement(By.CssSelector(".emailInput input"));
        IWebElement passwordField => driver.FindElement(By.CssSelector(".passwordInput input"));
        IWebElement usernameInput => driver.FindElement(By.CssSelector(".usernameInput input"));
        IWebElement registerButton => driver.FindElement(By.Id("registerButton"));
        IWebElement login_Link => driver.FindElement(By.Id("loginLink"));


        public RegisterPage(IWebDriver driver) : base(driver)
        {
        }

        public void Register(string email, string password, string username)
        {
            emailField.SendKeys(email);
            passwordField.SendKeys(password);
            usernameInput.SendKeys(username);
            registerButton.Click();
        }

        public void GoToLoginPage()
        {
            login_Link.Click();
        }
    }
}
