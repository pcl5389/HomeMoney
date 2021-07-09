using Newtonsoft.Json.Linq;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HomeMoney
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        HotKeys.KeyModifiers hotKeyModify = HotKeys.KeyModifiers.None;
        Keys hotKey = Keys.Pause;
        bool bRegKey = true;
        private void RegKey()
        {
            if (bRegKey)
                bRegKey = HotKeys.RegHotKey(Handle, 0x80F2, hotKeyModify, hotKey);
        }
        private void UnRegKey()
        {
            if (bRegKey)
                HotKeys.UnRegHotKey(Handle, 0x80F2);
        }
        private const int WM_HOTKEY = 0x312; //窗口消息：热键
        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case WM_HOTKEY: //窗口消息：热键
                    int tmpWParam = msg.WParam.ToInt32();
                    if (tmpWParam == 0x80F2)
                    {
                        Program.frmMain.show();
                    }
                    break;
            }
            base.WndProc(ref msg);
        }
        public void show()
        {
            if (!Program.frmMain.Visible)
                Program.frmMain.Visible = true;
            if (Program.frmMain.WindowState != FormWindowState.Normal)
                Program.frmMain.WindowState = FormWindowState.Normal;

            Program.frmMain.Update();
            Form2.SetForegroundWindow(Program.frmMain.Handle);
        }
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private void button1_Click(object sender, EventArgs e)
        {
            if (!(button1.Enabled = Clipboard.ContainsText()))
                return;
            string data = Clipboard.GetText();
            if (data.IndexOf("base64,") == -1)
            {
                MessageBox.Show("数据格式错误！非Base64格式", "解析错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Clipboard.Clear();
                button1.Enabled = false;
                return;
            }
            try
            {
                data = Encoding.UTF8.GetString(Convert.FromBase64String(data.Substring(data.IndexOf("base64,") + 7)));
            }
            catch(Exception err)
            {
                MessageBox.Show("Base64格式解析失败！"+ err.Message, "解析错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Clipboard.Clear();
                button1.Enabled = false;
                return;
            }
            
            //richTextBox1.Text= data;
            string m_id = "", m_name="", nick_name="", goods_name="" ;
            if (Scaler.Common.TryParse(data) is JObject obj)
            {
                JToken token;
                if (obj.TryGetValue("entrances", out token)
                    && token is JArray entrances)
                {
                    foreach (JObject item in entrances)
                    {
                        if (item.TryGetValue("name", out token) && token.ToString() == "在此商户的交易账单" && item.TryGetValue("url", out token) && token is JObject url && url.TryGetValue("query", out token) && token is JArray query && query[0] is JObject sub_mcht && sub_mcht.TryGetValue("key", out token) && token.ToString() == "sub_mch_id")
                        {
                            m_id = sub_mcht.TryGetValue("value", out token) ? token.ToString() : "";
                            break;
                        }
                    }
                }
                if (obj.TryGetValue("header", out token)
                    && token is JObject header)
                {
                    nick_name = header.TryGetValue("nickname", out token) ? token.ToString() : "";
                }

                if (obj.TryGetValue("preview", out token) && token is JArray preview)
                {
                    foreach (JObject item in preview)
                    {
                        if (item.TryGetValue("label", out token) && token is JObject label && label.TryGetValue("name", out token) && token.ToString() == "商品"
                    && item.TryGetValue("value", out token) && token is JArray items && items[0] is JObject g_name)
                        {
                            goods_name = g_name.TryGetValue("name", out token) ? token.ToString() : "";
                        }
                        else if (item.TryGetValue("label", out token) && token is JObject label2 && label2.TryGetValue("name", out token) && token.ToString() == "商户全称"
                    && item.TryGetValue("value", out token) && token is JArray items2 && items2[0] is JObject _m_name)
                        {
                            m_name = _m_name.TryGetValue("name", out token) ? token.ToString() : "";
                        }
                    }
                }

                if (string.IsNullOrEmpty(m_name) && string.IsNullOrEmpty(nick_name) && string.IsNullOrEmpty(m_id) && string.IsNullOrEmpty(goods_name))
                {
                    MessageBox.Show("抱歉！未解析到微信商户号。", "获取失败！", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Clipboard.Clear();
                    button1.Enabled = false;
                    return;
                }
                string line = string.Format("{0}\t{1}\t{2}\t{3}", m_name, nick_name, m_id, goods_name);
                Scaler.Win.WriteLog(line);
                richTextBox1.Text += string.Format("{0}\n", line);
                richTextBox1.Select(richTextBox1.Text.Length, 0);
                richTextBox1.ScrollToCaret();
                Clipboard.Clear();
                button1.Enabled = false;
            }
            else
            {
                MessageBox.Show("数据格式错误！非Json格式", "解析错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Clipboard.Clear();
                button1.Enabled = false;
            }
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            RegKey();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            }
            else
                UnRegKey();
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            button1.Enabled = Clipboard.ContainsText();
        }

        private void Form2_Activated(object sender, EventArgs e)
        {
            button1.Enabled = Clipboard.ContainsText();
        }
    }
}
