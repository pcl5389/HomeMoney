using System;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Scaler.UI
{
    public partial class MyComboBox: ComboBox
    {
        #region fields and properties
        private readonly DropdownControl m_dropDown;
        public MyListView m_list;
        private bool m_fromKeyboard;
        #endregion
        private DataTable _data;
        public string[] ColumnsWidth { get; set; } = new string[] { };
        public DataTable dtSource {
            get { return _data; }
            set {
                _data = value;
                RefreshList();
                OnTextChanged(null);
            }
        }
        public event EventHandler AfterChange;
        private void RefreshList()
        {
            if (m_list.SelectedItems.Count > 0)
            {
                foreach (ListViewItem li in m_list.SelectedItems)
                {
                    li.Selected = false;
                }
            }
            m_list.iOld = -1;
            m_list.Clear();
            if (dtSource != null)
            {
                int icol_count = dtSource.Columns.Count, itotalWidth = 0;
                for (int i = 0; i < dtSource.Columns.Count; i++)
                {
                    int.TryParse(ColumnsWidth[i], out int w);
                    w = w < 1 ? 100 : w;
                    m_list.Columns.Add(dtSource.Columns[i].ColumnName, w);
                    itotalWidth += w;
                }
                
                foreach (DataRow dr in dtSource.Rows)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < icol_count; i++)
                    {
                        sb.Append(dr[i].ToString()).Append(",");
                    }
                    m_list.Items.Add(new ListViewItem(sb.Remove(sb.Length - 1, 1).ToString().Split(',')));
                }
                this.DropDownWidth = itotalWidth + 22;
            }
        }

        public MyComboBox()
        {
            m_list = new MyListView
            {
                TabStop = false,
                Dock = DockStyle.Fill,
                View = View.Details,
                GridLines=true,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = true,
               
            };
            m_list.Click += onSuggestionListClick;
            FontChanged += new EventHandler(NewFont);
            DropDownStyle = ComboBoxStyle.DropDown;

            m_dropDown = new DropdownControl(m_list);
            DropDownHeight = 1;
            hideDropDown();
        }

        protected override void OnClick(EventArgs e)
        {
            if (!m_dropDown.Visible)
            {
                showDropDown();
            }
            //base.OnClick(e);
        }

        private void NewFont(object sender, EventArgs e)
        {
            m_list.Font = this.Font;
        }
        protected override void OnLocationChanged(EventArgs e)
        {
#if DEBUG
            Console.WriteLine("OnLocationChanged");
#endif
            base.OnLocationChanged(e);
            hideDropDown();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            m_dropDown.Width = Width;
        }
        public void showDropDown()
        {
            if (DesignMode)
            {
                return;
            }
            hideDropDown();
            m_dropDown.Show(this, new Size(DropDownWidth, DropDownHeight));
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr GetFocus();

        ///获取 当前拥有焦点的控件
        private Control GetFocusedControl()
        {
            Control focusedControl = null;

            // To get hold of the focused control:

            IntPtr focusedHandle = GetFocus();

            if (focusedHandle != IntPtr.Zero)

                //focusedControl = Control.FromHandle(focusedHandle);

                focusedControl = Control.FromChildHandle(focusedHandle);

            return focusedControl;
        }

        bool InOwnerWindow(Control ctl)
        {
            IntPtr focusedHandle = GetFocus();
            if (focusedHandle != IntPtr.Zero)
            {
                foreach (Control ct in ctl.Controls)
                {
                    if (ct.Handle == focusedHandle)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// <see cref="ComboBox.OnLostFocus(EventArgs)"/>
        /// </summary>
        protected override void OnLostFocus(EventArgs e)
        {
            if (!m_dropDown.Focused && !m_list.Focused && GetFocusedControl() != this)
            {
#if DEBUG
                Console.WriteLine("OnLostFocus, "+ (GetFocusedControl()==this).ToString() +  "当前焦点"+ GetFocus());
#endif
                hideDropDown();
            }
            base.OnLostFocus(e);
        }
        private void onSuggestionListClick(object sender, EventArgs e)
        {
            Text = m_list.SelectedItems[0].Text;
            m_fromKeyboard = false;

            Focus();
            Select(0, Text.Length);
            hideDropDown();
            if(AfterChange!=null)
                AfterChange.Invoke(this, e);
        }

        /// <summary>
        /// Process command keys
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == Keys.Tab) && (m_dropDown.Visible))
            {
                // we change the selection but will also allow the navigation to the next control
                if (m_list.Text.Length != 0)
                {
                    Text = m_list.Text;
                }
                Select(0, Text.Length);
#if DEBUG
                Console.WriteLine("ProcessCmdKey");
#endif
                hideDropDown();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Hides the drop down.
        /// </summary>
        public void hideDropDown()
        {
            if (m_dropDown.Visible)
            {
                m_dropDown.Visible = false;
                m_dropDown.Close();
            }
        }

        /// <summary>
        /// if the dropdown is visible some keystrokes
        /// should behave in a custom way
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            m_fromKeyboard = true;
            if (!m_dropDown.Visible || m_list.Items.Count == 0)
            {
                base.OnKeyDown(e);
                return;
            }
            switch (e.KeyCode)
            {
                case Keys.Down:
                    if (m_list.SelectedItems.Count == 0)
                    {
                        m_list.Items[0].Selected = true;
                        m_list.Items[0].EnsureVisible();
                    }
                    else if (m_list.SelectedIndices[0] < m_list.Items.Count - 1)
                    {
                        m_list.Items[m_list.SelectedIndices[0] + 1].Selected = true;
                        m_list.Items[m_list.SelectedIndices[0]].EnsureVisible();
                    }
                    else
                    {
                        m_list.Items[0].Selected = true;
                        m_list.Items[0].EnsureVisible();
                    }
                    break;
                case Keys.Up:
                    if (m_list.SelectedIndices[0] > 0)
                    {
                        m_list.Items[m_list.SelectedIndices[0] - 1].Selected = true;
                        m_list.Items[m_list.SelectedIndices[0]].EnsureVisible();
                    }
                    else if (m_list.SelectedIndices[0] <= 0)
                    {
                        m_list.Items[m_list.Items.Count - 1].Selected = true;
                        m_list.Items[m_list.Items.Count - 1].EnsureVisible();
                    }

                    break;
                case Keys.Enter:
                    if (m_list.SelectedItems.Count > 0)
                    {
                        Text = m_list.SelectedItems[0].Text;
                        if (AfterChange != null)
                            AfterChange.Invoke(this, e);
                    }
                    //Select(0, Text.Length);
                    hideDropDown();
                    break;
                case Keys.Escape:
                    hideDropDown();
                    break;
                default:
                    base.OnKeyDown(e);
                    return;
            }
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        /// <summary>
        /// We need to know if the last text changed event was due to one of the dropdowns 
        /// or to the keyboard
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownClosed(EventArgs e)
        {
            m_fromKeyboard = false;
            base.OnDropDownClosed(e);
        }

        /// <summary>
        /// this were we can make suggestions
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (!m_fromKeyboard || !Focused)
            {
                return;
            }
            if(!m_dropDown.Visible)
                showDropDown();
            bool bFind = false;
            foreach (ListViewItem li in m_list.Items)
            {
                if (li.Text.IndexOf(this.Text) > -1)
                {
                    li.Checked = true;
                    li.Selected = true;
                    li.EnsureVisible();
                    bFind = true;
                    break;
                }
            }

            if (!bFind)
            {
                m_list.Items[0].EnsureVisible();
                if(m_list.SelectedItems.Count>0)
                {
                    m_list.SelectedItems[0].Selected = false;
                }
                
            }
            m_fromKeyboard = false;
        }

        #region misc
        [Category("Behavior"), DefaultValue(false), Description("Specifies whether items in the list portion of the combobo are sorted.")]
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool DroppedDown
        {
            get { return base.DroppedDown || m_dropDown.Visible; }
            set
            {
                m_dropDown.Visible = false;
                base.DroppedDown = value;
            }
        }
        #endregion

        private const int WM_USER = 0x0400,
                          WM_REFLECT = WM_USER + 0x1C00,
                          WM_COMMAND = 0x0111,
                          CBN_DROPDOWN = 7;

        public static int HIWORD(int n)
        {
            return (n >> 16) & 0xffff;
        }
        protected override void WndProc(ref Message m)
        {
            switch (HIWORD((int)m.WParam))
            {
                case CBN_DROPDOWN:
                    if(!m_dropDown.Visible)
                    {
                        showDropDown();
                    }
                    IntPtr hwnd = this.Handle;
                    SendMessage(hwnd, BM_CLICK, 0, 0);     //发送点击按钮的消息
                    return;
            }
            base.WndProc(ref m);
        }
        /// <summary>
        /// <see cref="ComboBox.Dispose(bool)"/>
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_dropDown.Dispose();
            }
            base.Dispose(disposing);
        }
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        const int BM_CLICK = 0x202;
    }
}
