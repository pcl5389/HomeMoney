using System.Drawing;
using System.Windows.Forms;

namespace Scaler.UI
{
    public partial class MyListView : ListView
    {
        public MyListView()
        {
            // 开启双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            // Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            ListViewItem _OldItem = SelectedItems.Count > 0 ? SelectedItems[0] : null;
            ListViewItem _Item = GetItemAt(e.X, e.Y);
            if (_Item != null && !_Item.Equals(_OldItem))
            {
                Tag = _Item;
                _Item.Selected = true;
            }
        }
        public int iOld = -1;
        protected override void OnItemSelectionChanged(ListViewItemSelectionChangedEventArgs e)
        {
            if (this.SelectedIndices.Count > 0) //若有选中项 
            {
                if (iOld == -1)
                {
                    this.Items[this.SelectedIndices[0]].BackColor = Color.Black; //设置选中项的背景颜色 
                    this.Items[this.SelectedIndices[0]].ForeColor = Color.White; //设置选中项的背景颜色 

                    iOld = this.SelectedIndices[0]; //设置当前选中项索引 
                    this.Update();
                }
                else
                {
                    if (this.SelectedIndices[0] != iOld)
                    {
                        if (iOld < this.Items.Count)
                        {
                            this.Items[iOld].BackColor = BackColor; //恢复默认背景色 
                            this.Items[iOld].ForeColor = ForeColor; //恢复默认背景色 
                        }

                        this.Items[this.SelectedIndices[0]].BackColor = Color.Black; //设置选中项的背景颜色 
                        this.Items[this.SelectedIndices[0]].ForeColor = Color.White; //设置选中项的背景颜色 

                        
                        iOld = this.SelectedIndices[0]; //设置当前选中项索引 
                        this.Update();
                    }
                }
            }
            else //若无选中项 
            {
                this.Items[iOld].BackColor = BackColor; //恢复默认背景色 
                this.Items[iOld].ForeColor = ForeColor; //恢复默认背景色 
                iOld = -1; //设置当前处于无选中项状态 
            }

            base.OnItemSelectionChanged(e);
        }
        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }


        /*
        Color _headColor = Color.Yellow;
        Color _selectedColor = Color.Black;
        Color _rowBackColor1 = Color.Red;
        Color _rowBackColor2 = Color.Brown;
        

        protected override void OnDrawColumnHeader(
            DrawListViewColumnHeaderEventArgs e)
        {
            base.OnDrawColumnHeader(e);
            Graphics g = e.Graphics;
            Rectangle bounds = e.Bounds;
            Color baseColor = _headColor;
            Color borderColor = _headColor;
            Color innerBorderColor = Color.FromArgb(200, 255, 255);
            RenderBackgroundInternal(
                g,
                bounds,
                baseColor,
                borderColor,
                innerBorderColor,
                0.35f,
                true,
                LinearGradientMode.Vertical);
            TextFormatFlags flags = GetFormatFlags(e.Header.TextAlign);
            Rectangle textRect = new Rectangle(
                       bounds.X + 3,
                       bounds.Y,
                       bounds.Width - 6,
                       bounds.Height); ;
            if (e.Header.ImageList != null)
            {
                Image image = e.Header.ImageIndex == -1 ?
                    null : e.Header.ImageList.Images[e.Header.ImageIndex];
                if (image != null)
                {
                    Rectangle imageRect = new Rectangle(
                        bounds.X + 3,
                        bounds.Y + 2,
                        bounds.Height - 4,
                        bounds.Height - 4);
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(image, imageRect);
                    textRect.X = imageRect.Right + 3;
                    textRect.Width -= imageRect.Width;
                }
            }
            TextRenderer.DrawText(
                   g,
                   e.Header.Text,
                   e.Font,
                   textRect,
                   e.ForeColor,
                   flags);
        }
        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            base.OnDrawItem(e);
            if (View != View.Details)
            {
                e.DrawDefault = true;
            }
        }
        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            base.OnDrawSubItem(e);
            if (View != View.Details)
            {
                return;
            }
            if (e.ItemIndex == -1)
            {
                return;
            }
            Rectangle bounds = e.Bounds;
            ListViewItemStates itemState = e.ItemState;
            Graphics g = e.Graphics;
            if ((itemState & ListViewItemStates.Selected)
                == ListViewItemStates.Selected)
            {
                bounds.Height--;
                Color baseColor = _selectedColor;
                Color borderColor = _selectedColor;
                Color innerBorderColor = Color.FromArgb(200, 255, 255);
                RenderBackgroundInternal(
                    g,
                    bounds,
                    baseColor,
                    borderColor,
                    innerBorderColor,
                    0.35f,
                    true,
                    LinearGradientMode.Vertical);
                bounds.Height++;
            }
            else
            {
                Color backColor = e.ItemIndex % 2 == 0 ?
                _rowBackColor1 : _rowBackColor2;
                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, bounds);
                }
            }
            TextFormatFlags flags = GetFormatFlags(e.Header.TextAlign);
            if (e.ColumnIndex == 0)
            {
                if (e.Item.ImageList == null)
                {
                    e.DrawText(flags);
                    return;
                }
                Image image = e.Item.ImageIndex == -1 ?
                    null : e.Item.ImageList.Images[e.Item.ImageIndex];
                if (image == null)
                {
                    e.DrawText(flags);
                    return;
                }
                Rectangle imageRect = new Rectangle(
                    bounds.X + 4,
                    bounds.Y + 2,
                    bounds.Height - 4,
                    bounds.Height - 4);
                g.DrawImage(image, imageRect);
                Rectangle textRect = new Rectangle(
                    imageRect.Right + 3,
                    bounds.Y,
                    bounds.Width - imageRect.Right - 3,
                    bounds.Height);
                TextRenderer.DrawText(
                    g,
                    e.Item.Text,
                    e.Item.Font,
                    textRect,
                    e.Item.ForeColor,
                    flags);
                return;
            }
            e.DrawText(flags);
        }*/
    }
}
