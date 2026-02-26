using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Graphlet_frontend_tester.GraphletPages;

namespace Graphlet_frontend_tester.Tests
{
    [TestFixture]
    internal class WorkspacesTests : BaseTests
    {
        // Tracks workspace names created by each test so TearDown can clean them up
        private readonly List<string> _createdWorkspaceNames = new();

        // Helper: log in with the demo account and navigate to workspaces
        private void LoginAndGoToWorkspaces()
        {
            driver.Url = LoginPage.URL;
            Thread.Sleep(200);
            loginPage.Login("demo@example.com", "demo");
            // Wait until redirect to workspaces completes
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Url.Contains("workspaces"));
            Thread.Sleep(300);
        }

        // Helper: register a workspace name for cleanup
        private string TrackWorkspace(string name)
        {
            _createdWorkspaceNames.Add(name);
            return name;
        }

        [SetUp]
        public void WorkspacesSetUp()
        {
            _createdWorkspaceNames.Clear();
            LoginAndGoToWorkspaces();
        }

        [TearDown]
        public void WorkspacesTearDown()
        {
            if (_createdWorkspaceNames.Count == 0) return;

            // Navigate to workspaces page if not already there
            if (!driver.Url.Contains("workspaces"))
            {
                driver.Url = WorkspacesPage.URL;
                Thread.Sleep(500);
            }

            foreach (string name in _createdWorkspaceNames.ToList())
            {
                workspacesPage.DeleteWorkspaceByName(name);
            }

            _createdWorkspaceNames.Clear();
        }

        // ── Create workspace ─────────────────────────────────────────────────

        [Test]
        public void OpenCreateNewDialogShouldShowDialog()
        {
            workspacesPage.OpenCreateNewDialog();
            Thread.Sleep(200);
            Assert.That(workspacesPage.IsCreateNewDialogVisible(), Is.True);
        }

        [Test]
        public void CreateNewWorkspaceShouldAppearInList()
        {
            string name = TrackWorkspace("TestWS_" + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            int countBefore = workspacesPage.GetWorkspaceCards().Count;

            workspacesPage.CreateWorkspace(name);
            Thread.Sleep(1500);

            int countAfter = workspacesPage.GetWorkspaceCards().Count;
            Assert.That(countAfter, Is.GreaterThan(countBefore));
        }

        [Test]
        public void CreateWorkspaceWithEmptyNameShouldKeepButtonDisabled()
        {
            workspacesPage.OpenCreateNewDialog();
            Thread.Sleep(200);
            IWebElement createBtn = driver.FindElement(By.Id("create-new-workspace-button"));
            Assert.That(createBtn.Enabled, Is.False);
        }

        [Test]
        public void CreateWorkspaceShouldShowCorrectNameInList()
        {
            string name = TrackWorkspace("Named_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(name);
            Thread.Sleep(1500);

            var cards = workspacesPage.GetWorkspaceCards();
            bool found = cards.Any(c => c.Text.Contains(name));
            Assert.That(found, Is.True);
        }

        // ── Search ───────────────────────────────────────────────────────────

        [Test]
        public void SearchWithExistingTermShouldShowMatchingWorkspaces()
        {
            // Create a workspace with a unique name so we can search for it
            string unique = TrackWorkspace("SrchWS_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(unique);
            Thread.Sleep(1500);

            workspacesPage.SearchWorkspace(unique);
            Thread.Sleep(300);

            var cards = workspacesPage.GetWorkspaceCards();
            Assert.That(cards.Count, Is.GreaterThan(0));
            Assert.That(cards.All(c => c.Text.Contains(unique, StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public void SearchWithNonExistingTermShouldShowNoWorkspaces()
        {
            workspacesPage.SearchWorkspace("ZZZNOTEXIST_XYZ_99999");
            Thread.Sleep(300);

            var cards = workspacesPage.GetWorkspaceCards();
            Assert.That(cards.Count, Is.EqualTo(0));
        }

        [Test]
        public void ClearingSearchShouldRestoreAllWorkspaces()
        {
            // first ensure at least one workspace exists
            string name = TrackWorkspace("ClearSrch_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(name);
            Thread.Sleep(1500);

            int totalCount = workspacesPage.GetWorkspaceCards().Count;

            workspacesPage.SearchWorkspace("ZZZNOTEXIST_XYZ_99999");
            Thread.Sleep(200);
            workspacesPage.ClearSearch();
            Thread.Sleep(200);

            int afterClear = workspacesPage.GetWorkspaceCards().Count;
            Assert.That(afterClear, Is.EqualTo(totalCount));
        }

        // ── Other options panel ──────────────────────────────────────────────

        [Test]
        public void OpenOtherOptionsShouldShowPanel()
        {
            workspacesPage.OpenOtherOptions();
            Thread.Sleep(200);
            Assert.That(workspacesPage.IsOtherOptionsVisible(), Is.True);
        }

        [Test]
        public void LogoutShouldRedirectToLoginPage()
        {
            workspacesPage.Logout();
            Thread.Sleep(500);
            Assert.That(driver.Url, Is.EqualTo(LoginPage.URL));
        }

        [Test]
        public void GoToSettingsShouldOpenSettingsPage()
        {
            workspacesPage.GoToSettings();
            Thread.Sleep(500);
            Assert.That(driver.Url, Does.Contain("settings"));
        }

        // ── Rename workspace ─────────────────────────────────────────────────

        [Test]
        public void RenameWorkspaceShouldUpdateName()
        {
            // Create a workspace to rename
            string original = TrackWorkspace("RenameMe_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(original);
            Thread.Sleep(1500);

            // Find the id of the just-created workspace card
            var cards = driver.FindElements(By.CssSelector(".workspacePreview"));
            IWebElement? targetCard = cards.FirstOrDefault(c => c.Text.Contains(original));
            Assert.That(targetCard, Is.Not.Null, "Created workspace not found in list");

            // Extract the workspace id from the card child element
            IWebElement menuBtn = targetCard!.FindElement(By.CssSelector("[id^='workspace-menu-button-']"));
            string wsId = menuBtn.GetAttribute("id")!.Replace("workspace-menu-button-", "");

            string newName = TrackWorkspace("Renamed_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.RenameWorkspace(wsId, newName);
            Thread.Sleep(1000);

            var updatedCards = workspacesPage.GetWorkspaceCards();
            bool found = updatedCards.Any(c => c.Text.Contains(newName));
            Assert.That(found, Is.True);
        }

        [Test]
        public void CancelRenameShouldKeepOriginalName()
        {
            string original = TrackWorkspace("KeepName_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(original);
            Thread.Sleep(1500);

            var cards = driver.FindElements(By.CssSelector(".workspacePreview"));
            IWebElement? targetCard = cards.FirstOrDefault(c => c.Text.Contains(original));
            Assert.That(targetCard, Is.Not.Null);

            IWebElement menuBtn = targetCard!.FindElement(By.CssSelector("[id^='workspace-menu-button-']"));
            string wsId = menuBtn.GetAttribute("id")!.Replace("workspace-menu-button-", "");

            workspacesPage.ClickRenameWorkspace(wsId);
            Thread.Sleep(200);
            workspacesPage.CancelDialog();
            Thread.Sleep(300);

            var updatedCards = workspacesPage.GetWorkspaceCards();
            bool found = updatedCards.Any(c => c.Text.Contains(original));
            Assert.That(found, Is.True);
        }

        // ── Delete workspace ─────────────────────────────────────────────────

        [Test]
        public void DeleteWorkspaceShouldRemoveItFromList()
        {
            string name = TrackWorkspace("DeleteMe_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(name);
            Thread.Sleep(1500);

            var cards = driver.FindElements(By.CssSelector(".workspacePreview"));
            IWebElement? targetCard = cards.FirstOrDefault(c => c.Text.Contains(name));
            Assert.That(targetCard, Is.Not.Null);

            IWebElement menuBtn = targetCard!.FindElement(By.CssSelector("[id^='workspace-menu-button-']"));
            string wsId = menuBtn.GetAttribute("id")!.Replace("workspace-menu-button-", "");

            int countBefore = workspacesPage.GetWorkspaceCards().Count;
            workspacesPage.DeleteWorkspace(wsId);
            Thread.Sleep(800);

            int countAfter = workspacesPage.GetWorkspaceCards().Count;
            Assert.That(countAfter, Is.LessThan(countBefore));

            // Already deleted by the test itself — no need for TearDown to retry
            _createdWorkspaceNames.Remove(name);
        }

        [Test]
        public void CancelDeleteShouldKeepWorkspaceInList()
        {
            string name = TrackWorkspace("DontDeleteMe_" + DateTime.Now.ToString("HHmmssfff"));
            workspacesPage.CreateWorkspace(name);
            Thread.Sleep(1500);

            var cards = driver.FindElements(By.CssSelector(".workspacePreview"));
            IWebElement? targetCard = cards.FirstOrDefault(c => c.Text.Contains(name));
            Assert.That(targetCard, Is.Not.Null);

            IWebElement menuBtn = targetCard!.FindElement(By.CssSelector("[id^='workspace-menu-button-']"));
            string wsId = menuBtn.GetAttribute("id")!.Replace("workspace-menu-button-", "");

            int countBefore = workspacesPage.GetWorkspaceCards().Count;
            workspacesPage.ClickDeleteWorkspace(wsId);
            Thread.Sleep(200);
            workspacesPage.CancelDialog();
            Thread.Sleep(300);

            int countAfter = workspacesPage.GetWorkspaceCards().Count;
            Assert.That(countAfter, Is.EqualTo(countBefore));
        }
    }
}

