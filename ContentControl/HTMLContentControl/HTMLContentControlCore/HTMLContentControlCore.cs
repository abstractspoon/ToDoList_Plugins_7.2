﻿
using System;
using System.Runtime.InteropServices;

using Abstractspoon.Tdl.PluginHelpers;

// PLS DON'T ADD OTHER 'USING' STATEMENTS WHILE I AM STILL LEARNING!

namespace HTMLContentControl
{
    [System.ComponentModel.DesignerCategory("")]
    public class HTMLContentControlCore : ZetaHtmlEditControl.HtmlEditUserControl, IContentControlWnd
    {
        private IntPtr m_hwndParent;
        private UIThemeToolbarRenderer m_toolbarRenderer;
        private System.Drawing.Font m_ControlsFont;

        // --------------------------------------------------------------------------------------

        private const string FontName = "Tahoma";

        // --------------------------------------------------------------------------------------

        public HTMLContentControlCore(IntPtr hwndParent)
        {
            m_hwndParent = hwndParent;
            m_toolbarRenderer = new UIThemeToolbarRenderer();
            m_ControlsFont = new System.Drawing.Font(FontName, 8);

            InitializeComponent();

//             inputTextBox.TextChanged += new System.EventHandler(OnInputTextChanged);
//             inputTextBox.LostFocus += new System.EventHandler(OnInputTextLostFocus);
        }

        // ITDLContentControl ------------------------------------------------------------------

        public Byte[] GetContent()
        {
            return System.Text.Encoding.Unicode.GetBytes(""/*InputText*/);
        }

        public bool SetContent(Byte[] content, bool bResetSelection)
        {
//             InputText = System.Text.Encoding.Unicode.GetString(content);
            return true;
        }

        // text content if supported. return false if not supported
        public String GetTextContent()
        {
            return "";// OutputText;
        }

        public bool SetTextContent(String content, bool bResetSelection)
        {
//             InputText = content;
            return true;
        }

        public bool ProcessMessage(IntPtr hwnd, UInt32 message, UInt32 wParam, UInt32 lParam, UInt32 time, Int32 xPos, Int32 yPos)
        {
            // TODO
            return false;
        }

        public bool Undo()
        {
            // TODO
            return false;
        }

        public bool Redo()
        {
            // TODO
            return false;
        }

        public void SetUITheme(UITheme theme)
        {
            m_toolbarRenderer.SetUITheme(theme);

            BackColor = theme.GetAppDrawingColor(UITheme.AppColor.AppBackLight);
            ToolBar.BackColor = theme.GetAppDrawingColor(UITheme.AppColor.AppBackLight);
        }

        public void SetReadOnly(bool bReadOnly)
        {
            // TODO

        }

        public void SavePreferences(Preferences prefs, String key)
        {
            // TODO

        }

        public void LoadPreferences(Preferences prefs, String key, bool appOnly)
        {
            if (!appOnly)
            {
                // private settings
                // TODO
            }

            String fontName = @"Tahoma";
            float fontSize = 8.25F;
            
            if (prefs.GetProfileInt("Preferences", "SpecifyCommentsFont", 0) != 0)
            {
                fontName = prefs.GetProfileString("Preferences", "CommentsFont", "Tahoma");
                fontSize = prefs.GetProfileInt("Preferences", "CommentsFontSize", 8);
            }
            else if ((prefs.GetProfileInt("Preferences", "SpecifyTreeFont", 0) != 0) &&
                    (prefs.GetProfileInt("Preferences", "CommentsUseTreeFont", 0) != 0))
            {
                fontName = prefs.GetProfileString("Preferences", "TreeFont", "Tahoma");
                fontSize = prefs.GetProfileInt("Preferences", "FontSize", 8);
            }

            SetCssFontNameAndSize(fontName, fontSize);
        }

        // --------------------------------------------------------------------

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
 
            HtmlEditControl.SetDocumentText(@"", @"C:\", true);
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);

            ToolBar.Width = ClientSize.Width;

            Win32.RemoveClientEdge(Handle);
        }

        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        {
            // Do nothing
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.ToolBar.Renderer = m_toolbarRenderer;
            this.ToolBar.Font = m_ControlsFont;

            // 
            // HTMLContentControlCore
            // 
            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(603, 716);
            this.Name = "HTMLContentControlCore";
            this.ResizeRedraw = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Font = m_ControlsFont;

            this.HtmlEditControl.AllowWebBrowserDrop = false;
            this.HtmlEditControl.CssFontSize = "8pt";
            this.HtmlEditControl.CssFontName = "Tahoma";
            this.HtmlEditControl.IsWebBrowserContextMenuEnabled = false;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void OnInputTextChanged(object sender, EventArgs e)
        {
			ContentControlWnd.ParentNotify notify = new ContentControlWnd.ParentNotify(m_hwndParent);

            notify.NotifyChange();
        }

        private void OnInputTextLostFocus(object sender, EventArgs e)
        {
			ContentControlWnd.ParentNotify notify = new ContentControlWnd.ParentNotify(m_hwndParent);

            notify.NotifyKillFocus();
        }

    }
}