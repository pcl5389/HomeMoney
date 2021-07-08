using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HomeMoney
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            init_kemu();
            monitor();
        }
        delegate void dele_init();
        public void init_kemu()
        {
            if (this.InvokeRequired)
            {
                dele_init _init = new dele_init(init_kemu);
                this.Invoke(_init);
            }
            else
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("科目拼音");
                dt.Columns.Add("科目名称");

                using (FileStream fs = new FileStream("科目.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                    {
                        while (!sr.EndOfStream)
                        {
                            string[] line = sr.ReadLine().Trim().Trim(new char[] { '|', '\r', '\n', '\t', ' ' }).Split('|');
                            if (line.Length < 2 || dt.Select("科目名称='" + line[0].Trim() + "'").Length > 0)
                                continue;

                            DataRow dr = dt.NewRow();
                            dr["科目拼音"] = PingYinHelper.GetFirstSpell(line[0].Trim()).ToLowerInvariant();
                            dr["科目名称"] = line[0].Trim();
                            dt.Rows.Add(dr);
                        }
                    }
                }
                

                myComboBox1.ColumnsWidth = new string[] { "1", "120" };
                myComboBox1.dtSource = dt;
            }
        }

        public void init_kemu2(string kemu)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("科目拼音");
            dt.Columns.Add("科目名称");

            using (StreamReader sr = new StreamReader("科目.txt", Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Trim().Trim(new char[] { '|', '\r', '\n', '\t', ' ' }).Split('|');
                    if (line.Length < 2 || line[0].Trim() != kemu || dt.Select("科目名称='" + line[0].Trim() + "'").Length > 0)
                        continue;

                    DataRow dr = dt.NewRow();
                    dr["科目拼音"] = PingYinHelper.GetFirstSpell(line[1].Trim()).ToLowerInvariant();
                    dr["科目名称"] = line[1].Trim();
                    dt.Rows.Add(dr);
                }
            }

            myComboBox2.ColumnsWidth = new string[] { "1", "120" };
            myComboBox2.dtSource = dt;
        }
        private void myComboBox1_AfterChange(object sender, EventArgs e)
        {
            myComboBox1.Text = myComboBox1.dtSource.Rows[myComboBox1.m_list.SelectedIndices[0]][1].ToString();
            init_kemu2(myComboBox1.Text);
            myComboBox2.Focus();
        }

        private void myComboBox1_Enter(object sender, EventArgs e)
        {
            if(!myComboBox1.DroppedDown)
            {
                myComboBox1.DroppedDown = true;
            }
        }

        private void myComboBox2_Enter(object sender, EventArgs e)
        {
            if (!myComboBox2.DroppedDown)
            {
                myComboBox2.DroppedDown = true;
            }
        }

        private void myComboBox2_AfterChange(object sender, EventArgs e)
        {
            myComboBox2.Text = myComboBox2.dtSource.Rows[myComboBox2.m_list.SelectedIndices[0]][1].ToString();
            textBox1.Focus();
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern long SendMessage(IntPtr hWnd, uint Msg, uint wParam, long lParam);

        const int WM_SYSKEYDOWN = 0x0104, VK_DOWN = 0x28;

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            myComboBox1.Focus();
        }

        private void dateTimePicker1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                myComboBox1.Focus();
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox1.Text, out decimal jine))
            {
                textBox1.Text = jine.ToString("0.00");
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox2.Focus();
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Enter)
            {
                if (string.IsNullOrEmpty(myComboBox1.Text))
                {
                    myComboBox1.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(myComboBox2.Text))
                {
                    myComboBox2.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(textBox1.Text))
                {
                    textBox1.Focus();
                    return;
                }
                listView1.Items.Add(new ListViewItem(new string[] { dateTimePicker1.Value.ToString("yyyy-MM-dd"), myComboBox1.Text, myComboBox2.Text, textBox1.Text, textBox2.Text }));
                myComboBox1.Text = "";
                myComboBox2.Text = "";

                textBox1.Text = "";
                textBox2.Text = "";
                dateTimePicker1.Focus();
                listView1.Items[listView1.Items.Count - 1].EnsureVisible();
            }
        }

        FileSystemWatcher watcher = new FileSystemWatcher();
        public void monitor()
        {
            Console.WriteLine("文件开始监控");

            watcher.Path = AppDomain.CurrentDomain.BaseDirectory;
            watcher.Filter = "科目.txt";
            watcher.NotifyFilter =//被监控的方面
               NotifyFilters.LastWrite;

            // 订阅一些事件，当它被触发时（.net(windows)底层触发它，我们不用管），执行我们的方法
            watcher.Changed += (object source, FileSystemEventArgs e) =>
            {
                init_kemu();
#if DEBUG
                Console.WriteLine("文件{0}已经被修改,修改类型{1}", e.FullPath, e.ChangeType.ToString());
#endif
            };
            // 为true表示开启FileSystemWatcher组件，反之我们的监控将不启作用
            watcher.EnableRaisingEvents = true;
        }

        private void Form1_Leave(object sender, EventArgs e1)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("日期,科目一,科目二,金额,备注");
            foreach (ListViewItem li in listView1.Items)
            {
                sb.AppendFormat("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"", li.Text, li.SubItems[1].Text, li.SubItems[2].Text, li.SubItems[3].Text, li.SubItems[4].Text).AppendLine();
            }
            StreamWriter sw = new StreamWriter("账单.csv", false, Encoding.Default);
            sw.Write(sb.ToString());
            sw.Close();
            Process.Start("账单.csv");
            //Process.Start(AppDomain.CurrentDomain.BaseDirectory + "..\\BeyondCompare4\\BCompare.exe", string.Format("\"{0}\" \"{1}\"", new_file_name, file_name));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //sender.GetType
                //ListViewItem 
                ContextMenu cm = new ContextMenu();
                MenuItem mi = new MenuItem();

                mi.Text = "删除当前行";
                mi.Shortcut = Shortcut.CtrlD;
                mi.ShowShortcut = true;
                mi.Click += new EventHandler(Item_Delete);
                cm.MenuItems.Add(mi);

                cm.Show((Control)sender, new Point(e.X, e.Y));
            }
        }

        private void Item_Delete(object sender, EventArgs e)
        {
            foreach (ListViewItem li in listView1.SelectedItems)
            {
                listView1.Items.Remove(li);
            }
            listView1.Refresh();
        }

        bool bSend = false;

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Focus();
        }

        private void dateTimePicker1_Enter(object sender, EventArgs e)
        {
            if(!bSend && (bSend=!bSend))
            {
                SendKeys.Send("{RIGHT}{RIGHT}");
            }
            
            //SendKeys.Send("%+{DOWN}");
            //SendMessage(dateTimePicker1.Handle, WM_SYSKEYDOWN, VK_DOWN, 0);
        }
    }
}
