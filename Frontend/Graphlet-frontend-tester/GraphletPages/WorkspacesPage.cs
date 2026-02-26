using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace Graphlet_frontend_tester.GraphletPages
{
    public class WorkspacesPage : BasePage
    {
        public static readonly string URL = DefaultValues.base_url + "workspaces";

        IWebElement createNewButton => driver.FindElement(By.Id("create-new-button"));
        IWebElement searchInput => driver.FindElement(By.Id("search-workspaces-input"));
        IWebElement otherOptionsButton => driver.FindElement(By.Id("other-options-button"));

        // Elements inside the "Create new workspace" dialog
        IWebElement workspaceNameInput => driver.FindElement(By.Id("nameInput"));
        IWebElement createWorkspaceButton => driver.FindElement(By.Id("create-new-workspace-button"));

        // Other options panel
        IWebElement logoutButton => driver.FindElement(By.Id("logout-button"));
        IWebElement settingsButton => driver.FindElement(By.Id("settings-button"));

        public WorkspacesPage(IWebDriver driver) : base(driver)
        {
        }

        public void OpenCreateNewDialog()
        {
            createNewButton.Click();
        }

        public void CreateWorkspace(string name)
        {
            OpenCreateNewDialog();
            Thread.Sleep(200);
            workspaceNameInput.Clear();
            workspaceNameInput.SendKeys(name);
            createWorkspaceButton.Click();
        }

        public void SearchWorkspace(string term)
        {
            searchInput.Clear();
            searchInput.SendKeys(term);
        }

        public void ClearSearch()
        {
            // Clearing the input programmatically doesn't always trigger the frontend
            // listeners. Try to dispatch an input event via JS; fall back to sending
            // a keystroke (space + backspace) which usually triggers change handlers.
            
                searchInput.Clear();
                // send a keystroke to ensure frontend reacts to the change
                searchInput.SendKeys(" ");
                searchInput.SendKeys(Keys.Backspace);
                Thread.Sleep(50);
        }

        public void OpenOtherOptions()
        {
            otherOptionsButton.Click();
        }

        public void Logout()
        {
            OpenOtherOptions();
            Thread.Sleep(200);
            logoutButton.Click();
        }

        public void GoToSettings()
        {
            OpenOtherOptions();
            Thread.Sleep(200);
            settingsButton.Click();
        }

        public void OpenWorkspaceMenu(string workspaceId)
        {
            driver.FindElement(By.Id($"workspace-menu-button-{workspaceId}")).Click();
        }

        public void ClickRenameWorkspace(string workspaceId)
        {
            OpenWorkspaceMenu(workspaceId);
            Thread.Sleep(100);
            driver.FindElement(By.Id($"workspace-rename-button-{workspaceId}")).Click();
        }

        public void ClickDeleteWorkspace(string workspaceId)
        {
            OpenWorkspaceMenu(workspaceId);
            Thread.Sleep(100);
            driver.FindElement(By.Id($"workspace-delete-button-{workspaceId}")).Click();
        }

        public void ConfirmDialog()
        {
            driver.FindElement(By.Id("confirm-dialog-confirm-button")).Click();
        }

        public void CancelDialog()
        {
            driver.FindElement(By.Id("confirm-dialog-cancel-button")).Click();
        }

        public void RenameWorkspace(string workspaceId, string newName)
        {
            ClickRenameWorkspace(workspaceId);
            Thread.Sleep(200);
            IWebElement renameInput = wait.Until(d => d.FindElement(By.CssSelector(".confirm-dialog input[type='text']")));
            renameInput.Clear();
            renameInput.SendKeys(newName);
            ConfirmDialog();
        }

        public void DeleteWorkspace(string workspaceId)
        {
            ClickDeleteWorkspace(workspaceId);
            Thread.Sleep(200);
            ConfirmDialog();
        }

        /// <summary>
        /// Finds a workspace card by its displayed name and deletes it via the UI.
        /// Returns true if found and deleted, false if not found.
        /// </summary>
        public bool DeleteWorkspaceByName(string name)
        {
            // Clear search first to make sure the card is visible
            try { searchInput.Clear(); } catch { /* ignore */ }
            Thread.Sleep(150);

            var cards = driver.FindElements(By.CssSelector(".workspacePreview"));
            IWebElement? target = cards.FirstOrDefault(c => c.Text.Contains(name));
            if (target == null) return false;

            IWebElement menuBtn = target.FindElement(By.CssSelector("[id^='workspace-menu-button-']"));
            string wsId = menuBtn.GetAttribute("id")!.Replace("workspace-menu-button-", "");
            DeleteWorkspace(wsId);
            Thread.Sleep(600);
            return true;
        }

        /// <summary>
        /// Returns the list of visible workspace card elements.
        /// </summary>
        public IReadOnlyCollection<IWebElement> GetWorkspaceCards()
        {
            return driver.FindElements(By.CssSelector(".workspacePreview"));
        }

        /// <summary>
        /// Waits until at least one workspace card is present and returns them all.
        /// </summary>
        public IReadOnlyCollection<IWebElement> WaitForWorkspaceCards()
        {
            wait.Until(d => d.FindElements(By.CssSelector(".workspacePreview")).Count > 0);
            return GetWorkspaceCards();
        }

        public bool IsCreateNewDialogVisible()
        {
            try
            {
                var el = driver.FindElement(By.Id("nameInput"));
                return el.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public bool IsOtherOptionsVisible()
        {
            try
            {
                var el = driver.FindElement(By.Id("logout-button"));
                return el.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}

