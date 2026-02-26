using OpenQA.Selenium;

namespace Graphlet_frontend_tester.GraphletPages
{
    public class SettingsPage : BasePage
    {
        public static readonly string URL = DefaultValues.base_url + "settings";
        public static readonly string AppearanceURL = DefaultValues.base_url + "settings/appearance";
        public static readonly string ProfileURL = DefaultValues.base_url + "settings/profile";
        public static readonly string SharedURL = DefaultValues.base_url + "settings/shared";
        public static readonly string SubscriptionURL = DefaultValues.base_url + "settings/subscription";

        IWebElement backButton => driver.FindElement(By.Id("back-to-dashboard"));
        IWebElement appearanceNavButton => driver.FindElement(By.Id("settings-nav-appearance"));
        IWebElement profileNavButton => driver.FindElement(By.Id("settings-nav-profile"));
        IWebElement sharedNavButton => driver.FindElement(By.Id("settings-nav-shared"));
        IWebElement subscriptionNavButton => driver.FindElement(By.Id("settings-nav-subscription"));

        public SettingsPage(IWebDriver driver) : base(driver)
        {
        }

        public void GoBack()
        {
            backButton.Click();
        }

        public void GoToAppearance()
        {
            appearanceNavButton.Click();
        }

        public void GoToProfile()
        {
            profileNavButton.Click();
        }

        public void GoToShared()
        {
            sharedNavButton.Click();
        }

        public void GoToSubscription()
        {
            subscriptionNavButton.Click();
        }

        public bool IsDarkModeCheckboxPresent()
        {
            try
            {
                var el = driver.FindElement(By.Id("dark-mode-checkbox"));
                return el.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}

