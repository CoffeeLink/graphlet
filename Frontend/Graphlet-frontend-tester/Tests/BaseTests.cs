﻿using OpenQA.Selenium;
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
        protected WorkspacesPage workspacesPage;
        protected SettingsPage settingsPage;
        protected WorkspacePage workspacePage;

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            // Disable Chrome password manager / save password prompts which interfere with tests
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("password_manager_enabled", false);
            // Disable notifications and other UI that can interfere
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-save-password-bubble");
            // Remove automation flags and extension which sometimes trigger UI changes
            options.AddExcludedArgument("enable-automation");
            options.AddArgument("--headless=new");
            try
            {
                options.AddAdditionalOption("useAutomationExtension", false);
                options.AddAdditionalOption("excludeSwitches", new[] { "enable-automation" });
            }
            catch { /* older Selenium versions may not support AddAdditionalOption */ }

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Url = DefaultValues.base_url;

            loginPage = new LoginPage(driver);
            registerPage = new RegisterPage(driver);
            workspacesPage = new WorkspacesPage(driver);
            settingsPage = new SettingsPage(driver);
            workspacePage = new WorkspacePage(driver);
        }


        [TearDown]
        public void Teardown()
        {
            driver.Dispose();
        }
    }
}
