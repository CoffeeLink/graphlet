using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Graphlet_frontend_tester.GraphletPages
{
    /// <summary>
    /// Page model for the fullscreen workspace view
    /// (opened via ?workspaceId=...&amp;fullscreen=1).
    /// </summary>
    public class WorkspacePage : BasePage
    {
        // ── Toolbar ──────────────────────────────────────────────────────────
        IWebElement toolbar        => driver.FindElement(By.Id("workspace-toolbar"));
        IWebElement errorDiv       => driver.FindElement(By.Id("workspace-error"));
        IWebElement addNoteButton  => driver.FindElement(By.Id("add-new-note-btn"));
        IWebElement closeButton    => driver.FindElement(By.Id("workspace-close-btn"));

        // ── Canvas ───────────────────────────────────────────────────────────
        IWebElement canvas         => driver.FindElement(By.Id("cv"));

        public WorkspacePage(IWebDriver driver) : base(driver) { }

        // ── Toolbar helpers ──────────────────────────────────────────────────

        public bool IsToolbarVisible()      => toolbar.Displayed;
        public bool IsErrorVisible()        => !string.IsNullOrWhiteSpace(errorDiv.Text);
        public string GetErrorText()        => errorDiv.Text;
        public bool IsCanvasVisible()       => canvas.Displayed;

        public void ClickAddNote()          => addNoteButton.Click();
        public void ClickClose()            => closeButton.Click();

        // ── Note helpers ─────────────────────────────────────────────────────

        /// <summary>All note card root elements currently on the canvas.</summary>
        public IReadOnlyCollection<IWebElement> GetNoteCards()
            => driver.FindElements(By.CssSelector("[id^='note-card-']"));

        /// <summary>Wait until the expected count of note cards is reached.</summary>
        public IReadOnlyCollection<IWebElement> WaitForNoteCount(int expected)
        {
            wait.Until(d => d.FindElements(By.CssSelector("[id^='note-card-']")).Count >= expected);
            return GetNoteCards();
        }

        /// <summary>Extract note id from a note-card element.</summary>
        public string GetNoteId(IWebElement noteCard)
            => noteCard.GetAttribute("id")!.Replace("note-card-", "");

        /// <summary>Get title text shown on a note card (view mode).</summary>
        public string GetNoteTitle(string noteId)
            => driver.FindElement(By.CssSelector($"#note-card-{noteId} .note-title")).Text;

        /// <summary>Get content text shown on a note card (view mode).</summary>
        public string GetNoteContent(string noteId)
            => driver.FindElement(By.CssSelector($"#note-card-{noteId} .note-content")).Text;

        /// <summary>Double-click a note card to enter edit mode.</summary>
        public void DoubleClickNote(string noteId)
        {
            IWebElement card = driver.FindElement(By.Id($"note-card-{noteId}"));
            new Actions(driver).DoubleClick(card).Perform();
        }

        /// <summary>Click the delete (✕) button on a note card.</summary>
        public void DeleteNote(string noteId)
            => driver.FindElement(By.Id($"note-delete-btn-{noteId}")).Click();

        // ── Edit mode helpers ────────────────────────────────────────────────

        public bool IsNoteInEditMode(string noteId)
        {
            try { return driver.FindElement(By.Id($"note-title-input-{noteId}")).Displayed; }
            catch (NoSuchElementException) { return false; }
        }

        public void SetNoteTitle(string noteId, string title)
        {
            IWebElement input = driver.FindElement(By.Id($"note-title-input-{noteId}"));
            input.Clear();
            input.SendKeys(title);
        }

        public void SetNoteContent(string noteId, string content)
        {
            IWebElement area = driver.FindElement(By.Id($"note-content-input-{noteId}"));
            area.Clear();
            area.SendKeys(content);
        }

        public string GetEditTitleValue(string noteId)
            => driver.FindElement(By.Id($"note-title-input-{noteId}")).GetAttribute("value")!;

        public string GetEditContentValue(string noteId)
            => driver.FindElement(By.Id($"note-content-input-{noteId}")).GetAttribute("value")!;

        public void SaveNote(string noteId)
            => driver.FindElement(By.Id($"note-save-btn-{noteId}")).Click();

        public void CancelNoteEdit(string noteId)
            => driver.FindElement(By.Id($"note-cancel-btn-{noteId}")).Click();

        /// <summary>
        /// Full edit: enter edit mode, change title+content, save.
        /// </summary>
        public void EditNote(string noteId, string newTitle, string newContent)
        {
            DoubleClickNote(noteId);
            Thread.Sleep(100);
            SetNoteTitle(noteId, newTitle);
            SetNoteContent(noteId, newContent);
            SaveNote(noteId);
        }

        /// <summary>
        /// Get the on-screen bounding rect (left, top) of a note card using JS.
        /// Returns a tuple (left, top) as doubles.
        /// </summary>
        public (double left, double top) GetNotePosition(string noteId)
        {
            IWebElement card = driver.FindElement(By.Id($"note-card-{noteId}"));
            var js = (IJavaScriptExecutor)driver;
            var dict = (Dictionary<string, object>)js.ExecuteScript(
                "var r = arguments[0].getBoundingClientRect(); return {left: r.left, top: r.top};",
                card
            );
            double left = Convert.ToDouble(dict["left"]);
            double top = Convert.ToDouble(dict["top"]);
            return (left, top);
        }

        /// <summary>
        /// Move a note card by an offset using Actions (drag) and wait until its position changes.
        /// </summary>
        public void MoveNote(string noteId, int offsetX, int offsetY)
        {
            IWebElement card = driver.FindElement(By.Id($"note-card-{noteId}"));

            var before = GetNotePosition(noteId);

            var actions = new Actions(driver);
            actions.MoveToElement(card).ClickAndHold().MoveByOffset(offsetX, offsetY).Release().Perform();

            
            
        }
    }
}

