using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Graphlet_frontend_tester.GraphletPages;

namespace Graphlet_frontend_tester.Tests
{
    [TestFixture]
    internal class SettingsTests : BaseTests
    {
        private void LoginAndGoToSettings()
        {
            driver.Url = LoginPage.URL;
            Thread.Sleep(200);
            loginPage.Login("demo@example.com", "demo");
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("workspaces"));
            Thread.Sleep(100);
            driver.Url = SettingsPage.AppearanceURL;
            Thread.Sleep(400);
        }

        [SetUp]
        public void SettingsSetUp()
        {
            LoginAndGoToSettings();
        }

        // ── Navigation ───────────────────────────────────────────────────────

        [Test]
        public void SettingsPageShouldBeAccessible()
        {
            Assert.That(driver.Url, Does.Contain("settings"));
        }

        [Test]
        public void GoBackButtonShouldReturnToWorkspaces()
        {
            settingsPage.GoBack();
            Thread.Sleep(500);
            Assert.That(driver.Url, Does.Contain("workspaces"));
        }

        [Test]
        public void ClickAppearanceNavShouldNavigateToAppearance()
        {
            // navigate away first
            settingsPage.GoToProfile();
            Thread.Sleep(100);
            settingsPage.GoToAppearance();
            Thread.Sleep(100);
            Assert.That(driver.Url, Does.Contain("appearance"));
        }

        [Test]
        public void ClickProfileNavShouldNavigateToProfile()
        {
            settingsPage.GoToProfile();
            Thread.Sleep(100);
            Assert.That(driver.Url, Does.Contain("profile"));
        }

        [Test]
        public void ClickSharedNavShouldNavigateToShared()
        {
            settingsPage.GoToShared();
            Thread.Sleep(100);
            Assert.That(driver.Url, Does.Contain("shared"));
        }

        [Test]
        public void ClickSubscriptionNavShouldNavigateToSubscription()
        {
            settingsPage.GoToSubscription();
            Thread.Sleep(100);
            Assert.That(driver.Url, Does.Contain("subscription"));
        }

        // ── Appearance page ──────────────────────────────────────────────────

        [Test]
        public void AppearancePageShouldShowDarkModeCheckbox()
        {
            settingsPage.GoToAppearance();
            Thread.Sleep(100);
            Assert.That(settingsPage.IsDarkModeCheckboxPresent(), Is.True);
        }

        [Test]
        public void AppearancePageShouldHaveCorrectHeading()
        {
            settingsPage.GoToAppearance();
            Thread.Sleep(300);
            IWebElement heading = driver.FindElement(By.CssSelector(".settings-right h1"));
            Assert.That(heading.Text, Is.EqualTo("Appearance"));
        }

        // ── Profile page ─────────────────────────────────────────────────────

        [Test]
        public void ProfilePageShouldHaveCorrectHeading()
        {
            settingsPage.GoToProfile();
            Thread.Sleep(100);
            IWebElement heading = driver.FindElement(By.CssSelector(".settings-right h1"));
            Assert.That(heading.Text, Is.EqualTo("Profile Settings"));
        }

        // ── Shared Items page ────────────────────────────────────────────────

        [Test]
        public void SharedItemsPageShouldHaveCorrectHeading()
        {
            settingsPage.GoToShared();
            Thread.Sleep(100);
            IWebElement heading = driver.FindElement(By.CssSelector(".settings-right h1"));
            Assert.That(heading.Text, Is.EqualTo("Shared Items"));
        }

        // ── Subscription page ────────────────────────────────────────────────

        [Test]
        public void SubscriptionPageShouldHaveCorrectHeading()
        {
            settingsPage.GoToSubscription();
            Thread.Sleep(100);
            IWebElement heading = driver.FindElement(By.CssSelector(".settings-right h1"));
            Assert.That(heading.Text, Is.EqualTo("Subscription"));
        }

        // ── Default redirect ─────────────────────────────────────────────────

        [Test]
        public void NavigatingToSettingsRootShouldRedirectToAppearance()
        {
            driver.Url = SettingsPage.URL;
            Thread.Sleep(600);
            Assert.That(driver.Url, Does.Contain("appearance"));
        }
    }
}

