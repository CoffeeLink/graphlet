using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Graphlet_frontend_tester.GraphletPages;

namespace Graphlet_frontend_tester.Tests
{
    /// <summary>
    /// Tests for the fullscreen workspace view (workspace.tsx + NoteCard).
    /// Every test opens the workspace in the same tab by navigating directly
    /// to the fullscreen URL so we can control the driver without tab switching.
    /// A helper workspace is created once per test and cleaned up in TearDown.
    /// </summary>
    [TestFixture]
    internal class WorkspaceTests : BaseTests
    {
        private string? _helperWorkspaceId;
        private string? _helperWorkspaceName;

        // ── Lifecycle ────────────────────────────────────────────────────────

        [SetUp]
        public void WorkspaceSetUp()
        {
            // Log in
            driver.Url = LoginPage.URL;
            Thread.Sleep(200);
            loginPage.Login("demo@example.com", "demo");
            var loginWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            loginWait.Until(d => d.Url.Contains("workspaces"));
            Thread.Sleep(100);

            // Create a dedicated workspace for this test
            _helperWorkspaceName = "WSTest_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
            workspacesPage.CreateWorkspace(_helperWorkspaceName);
            Thread.Sleep(1500);

            // Find its id
            var cards = driver.FindElements(By.CssSelector(".workspacePreview"));
            IWebElement? card = cards.FirstOrDefault(c => c.Text.Contains(_helperWorkspaceName));
            Assert.That(card, Is.Not.Null, "Helper workspace card not found");
            IWebElement menuBtn = card!.FindElement(By.CssSelector("[id^='workspace-menu-button-']"));
            _helperWorkspaceId = menuBtn.GetAttribute("id")!.Replace("workspace-menu-button-", "");

            // Navigate directly to the fullscreen workspace URL in the same tab
            string wsUrl = DefaultValues.base_url + $"workspaces?workspaceId={Uri.EscapeDataString(_helperWorkspaceId)}&fullscreen=1";
            driver.Url = wsUrl;
            Thread.Sleep(400);
        }

        [TearDown]
        public void WorkspaceTearDown()
        {
            if (_helperWorkspaceName == null) return;

            // Navigate back to workspaces list and delete the helper workspace
            driver.Url = WorkspacesPage.URL;
            Thread.Sleep(600);
            workspacesPage.DeleteWorkspaceByName(_helperWorkspaceName);
            _helperWorkspaceId = null;
            _helperWorkspaceName = null;
        }

        // ── Toolbar ──────────────────────────────────────────────────────────

        [Test]
        public void ToolbarShouldBeVisible()
        {
            Assert.That(workspacePage.IsToolbarVisible(), Is.True);
        }

        [Test]
        public void AddNewNoteButtonShouldBeVisible()
        {
            IWebElement btn = driver.FindElement(By.Id("add-new-note-btn"));
            Assert.That(btn.Displayed, Is.True);
        }

        [Test]
        public void CloseButtonShouldBeVisible()
        {
            IWebElement btn = driver.FindElement(By.Id("workspace-close-btn"));
            Assert.That(btn.Displayed, Is.True);
        }

        [Test]
        public void ErrorDivShouldBePresent()
        {
            IWebElement err = driver.FindElement(By.Id("workspace-error"));
            Assert.That(err, Is.Not.Null);
        }

        // ── Canvas ───────────────────────────────────────────────────────────

        [Test]
        public void CanvasShouldBeVisible()
        {
            Assert.That(workspacePage.IsCanvasVisible(), Is.True);
        }

        // ── Add note ─────────────────────────────────────────────────────────

        [Test]
        public void AddNewNoteShouldIncreaseNoteCount()
        {
            int before = workspacePage.GetNoteCards().Count;
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            int after = workspacePage.GetNoteCards().Count;
            Assert.That(after, Is.GreaterThan(before));
        }

        [Test]
        public void AddedNoteShouldHaveDefaultTitle()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            var cards = workspacePage.GetNoteCards();
            Assert.That(cards.Count, Is.GreaterThan(0));
            string noteId = workspacePage.GetNoteId(cards.Last());
            Assert.That(workspacePage.GetNoteTitle(noteId), Is.EqualTo("New note"));
        }

        [Test]
        public void AddedNoteShouldHaveDefaultContent()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(800);
            var cards = workspacePage.GetNoteCards();
            string noteId = workspacePage.GetNoteId(cards.Last());
            Assert.That(workspacePage.GetNoteContent(noteId), Is.EqualTo("New note text"));
        }

        [Test]
        public void AddingMultipleNotesShouldAllAppear()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            Assert.That(workspacePage.GetNoteCards().Count, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void MoveNoteShouldChangePosition()
        {
            // Add a note and get its id
            workspacePage.ClickAddNote();
            Thread.Sleep(200);
            var cards = workspacePage.GetNoteCards();
            string noteId = workspacePage.GetNoteId(cards.Last());

            // Read initial position
            var before = workspacePage.GetNotePosition(noteId);

            // Move the note by an offset
            workspacePage.MoveNote(noteId, 120, 60);

            // Read final position
            var after = workspacePage.GetNotePosition(noteId);

            // Assert that position changed by at least some pixels
            Assert.That(Math.Abs(after.left - before.left) >= 5 || Math.Abs(after.top - before.top) >= 5,
                "Note position should change after dragging");
        }

        // ── Note card view mode ──────────────────────────────────────────────

        [Test]
        public void NoteCardShouldShowTitle()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            var cards = workspacePage.GetNoteCards();
            string noteId = workspacePage.GetNoteId(cards.Last());
            IWebElement titleEl = driver.FindElement(By.CssSelector($"#note-card-{noteId} .note-title"));
            Assert.That(titleEl.Displayed, Is.True);
        }

        [Test]
        public void NoteCardShouldShowContent()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            var cards = workspacePage.GetNoteCards();
            string noteId = workspacePage.GetNoteId(cards.Last());
            IWebElement contentEl = driver.FindElement(By.CssSelector($"#note-card-{noteId} .note-content"));
            Assert.That(contentEl.Displayed, Is.True);
        }

        [Test]
        public void NoteCardShouldShowDeleteButton()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            var cards = workspacePage.GetNoteCards();
            string noteId = workspacePage.GetNoteId(cards.Last());
            IWebElement deleteBtn = driver.FindElement(By.Id($"note-delete-btn-{noteId}"));
            Assert.That(deleteBtn.Displayed, Is.True);
        }

        // ── Delete note ──────────────────────────────────────────────────────

        [Test]
        public void DeleteNoteShouldRemoveItFromCanvas()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            var cards = workspacePage.GetNoteCards();
            int before = cards.Count;
            string noteId = workspacePage.GetNoteId(cards.Last());

            workspacePage.DeleteNote(noteId);
            Thread.Sleep(200);

            int after = workspacePage.GetNoteCards().Count;
            Assert.That(after, Is.LessThan(before));
        }

        [Test]
        public void DeletedNoteCardShouldNoLongerExistInDom()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            workspacePage.DeleteNote(noteId);
            Thread.Sleep(600);

            var remaining = driver.FindElements(By.Id($"note-card-{noteId}"));
            Assert.That(remaining.Count, Is.EqualTo(0));
        }

        [Test]
        public void DeletingAllNotesShouldLeaveEmptyCanvas()
        {
            // Add two notes then delete them
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            workspacePage.ClickAddNote();
            Thread.Sleep(100);

            // Move one of the notes after creating to ensure movement doesn't interfere with deletion
            var initialCards = workspacePage.GetNoteCards().ToList();
            Console.WriteLine(initialCards);
            if (initialCards.Count > 0)
            {
                string moveNoteId = workspacePage.GetNoteId(initialCards.First());
                // move by a small offset; MoveNote will wait for the position to change
                workspacePage.MoveNote(moveNoteId, 100, 50);
                Thread.Sleep(100);
            }

            var cards = workspacePage.GetNoteCards().ToList();
            Console.WriteLine(cards.Count);
            foreach (IWebElement card in cards)
            {
                string noteId = workspacePage.GetNoteId(card);
                workspacePage.DeleteNote(noteId);
                Thread.Sleep(400);
            }

            Assert.That(workspacePage.GetNoteCards().Count, Is.EqualTo(0));
        }

        // ── Edit mode — enter / exit ─────────────────────────────────────────

        [Test]
        public void DoubleClickNoteShouldEnterEditMode()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            Assert.That(workspacePage.IsNoteInEditMode(noteId), Is.True);
        }

        [Test]
        public void EditModeShouldShowTitleInput()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            IWebElement input = driver.FindElement(By.Id($"note-title-input-{noteId}"));
            Assert.That(input.Displayed, Is.True);
        }

        [Test]
        public void EditModeShouldShowContentTextarea()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            IWebElement area = driver.FindElement(By.Id($"note-content-input-{noteId}"));
            Assert.That(area.Displayed, Is.True);
        }

        [Test]
        public void EditModeShouldShowSaveButton()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            IWebElement saveBtn = driver.FindElement(By.Id($"note-save-btn-{noteId}"));
            Assert.That(saveBtn.Displayed, Is.True);
        }

        [Test]
        public void EditModeShouldShowCancelButton()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            IWebElement cancelBtn = driver.FindElement(By.Id($"note-cancel-btn-{noteId}"));
            Assert.That(cancelBtn.Displayed, Is.True);
        }

        [Test]
        public void EditModeShouldPrePopulateTitleInput()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            string originalTitle = workspacePage.GetNoteTitle(noteId);

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            Assert.That(workspacePage.GetEditTitleValue(noteId), Is.EqualTo(originalTitle));
        }

        [Test]
        public void EditModeShouldPrePopulateContentTextarea()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            string originalContent = workspacePage.GetNoteContent(noteId);

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);

            Assert.That(workspacePage.GetEditContentValue(noteId), Is.EqualTo(originalContent));
        }

        // ── Edit mode — save ─────────────────────────────────────────────────

        [Test]
        public void SaveNoteShouldUpdateTitleInViewMode()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            string newTitle = "UpdatedTitle_" + DateTime.Now.ToString("HHmmssfff");
            workspacePage.EditNote(noteId, newTitle, "some content");
            Thread.Sleep(200);

            Assert.That(workspacePage.GetNoteTitle(noteId), Is.EqualTo(newTitle));
        }

        [Test]
        public void SaveNoteShouldUpdateContentInViewMode()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            string newContent = "UpdatedContent_" + DateTime.Now.ToString("HHmmssfff");
            workspacePage.EditNote(noteId, "some title", newContent);
            Thread.Sleep(200);

            Assert.That(workspacePage.GetNoteContent(noteId), Is.EqualTo(newContent));
        }

        [Test]
        public void SaveNoteShouldExitEditMode()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);
            workspacePage.SaveNote(noteId);
            Thread.Sleep(200);

            Assert.That(workspacePage.IsNoteInEditMode(noteId), Is.False);
        }

        // ── Edit mode — cancel ───────────────────────────────────────────────

        [Test]
        public void CancelEditShouldExitEditMode()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);
            workspacePage.CancelNoteEdit(noteId);
            Thread.Sleep(200);

            Assert.That(workspacePage.IsNoteInEditMode(noteId), Is.False);
        }

        [Test]
        public void CancelEditShouldKeepOriginalTitle()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            string originalTitle = workspacePage.GetNoteTitle(noteId);

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);
            workspacePage.SetNoteTitle(noteId, "ShouldNotBeSaved_" + DateTime.Now.Ticks);
            workspacePage.CancelNoteEdit(noteId);
            Thread.Sleep(100);

            Assert.That(workspacePage.GetNoteTitle(noteId), Is.EqualTo(originalTitle));
        }

        [Test]
        public void CancelEditShouldKeepOriginalContent()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(100);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());
            string originalContent = workspacePage.GetNoteContent(noteId);

            workspacePage.DoubleClickNote(noteId);
            Thread.Sleep(100);
            workspacePage.SetNoteContent(noteId, "ShouldNotBeSaved_" + DateTime.Now.Ticks);
            workspacePage.CancelNoteEdit(noteId);
            Thread.Sleep(100);

            Assert.That(workspacePage.GetNoteContent(noteId), Is.EqualTo(originalContent));
        }

        // ── Note position persistence ────────────────────────────────────────

        [Test]
        public void MovedNoteShouldKeepPositionAfterReload()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(200);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            workspacePage.MoveNote(noteId, 150, 80);
            Thread.Sleep(600); // allow backend to persist the new position

            var before = workspacePage.GetNoteStylePosition(noteId);

            // Reload the workspace page
            string wsUrl = DefaultValues.base_url + $"workspaces?workspaceId={Uri.EscapeDataString(_helperWorkspaceId!)}&fullscreen=1";
            driver.Url = wsUrl;
            Thread.Sleep(600);

            var after = workspacePage.GetNoteStylePosition(noteId);

            Assert.That(Math.Abs(after.left - before.left), Is.LessThanOrEqualTo(2),
                "Note left position should be preserved after reload");
            Assert.That(Math.Abs(after.top - before.top), Is.LessThanOrEqualTo(2),
                "Note top position should be preserved after reload");
        }

        [Test]
        public void MovedNoteShouldNotRevertToDefaultPositionAfterReload()
        {
            workspacePage.ClickAddNote();
            Thread.Sleep(200);
            string noteId = workspacePage.GetNoteId(workspacePage.GetNoteCards().Last());

            // Record the default (pre-move) position
            var defaultPos = workspacePage.GetNoteStylePosition(noteId);

            workspacePage.MoveNote(noteId, 150, 80);
            Thread.Sleep(600); // allow backend to persist the new position

            // Reload the workspace page
            string wsUrl = DefaultValues.base_url + $"workspaces?workspaceId={Uri.EscapeDataString(_helperWorkspaceId!)}&fullscreen=1";
            driver.Url = wsUrl;
            Thread.Sleep(600);

            var afterReload = workspacePage.GetNoteStylePosition(noteId);

            Assert.That(
                Math.Abs(afterReload.left - defaultPos.left) > 2 ||
                Math.Abs(afterReload.top - defaultPos.top) > 2,
                "Note should not revert to its default position after reload"
            );
        }

        // ── Close button ─────────────────────────────────────────────────────

        [Test]
        public void CloseButtonShouldCloseTheTab()
        {
            // Open the workspace in a real new tab so window.close() works
            string wsUrl = DefaultValues.base_url + $"workspaces?workspaceId={Uri.EscapeDataString(_helperWorkspaceId!)}&fullscreen=1";
            string originalHandle = driver.CurrentWindowHandle;

            // Open new tab via JavaScript
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.open('{wsUrl}', '_blank');");
            Thread.Sleep(100);

            var handles = driver.WindowHandles;
            string newHandle = handles.Last();
            driver.SwitchTo().Window(newHandle);
            Thread.Sleep(500);

            int beforeCount = driver.WindowHandles.Count;
            workspacePage.ClickClose();
            Thread.Sleep(200);

            int afterCount = driver.WindowHandles.Count;
            Assert.That(afterCount, Is.LessThan(beforeCount), "Tab should have been closed");

            // Switch back to the original handle
            driver.SwitchTo().Window(originalHandle);
        }
    }
}

