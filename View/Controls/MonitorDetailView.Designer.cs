namespace PlcTest.View.Controls
{
    partial class MonitorDetailView
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>清理正在使用的资源。</summary>
        /// <param name="disposing">true 表示释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 设计器生成：顶部选项条 + 纵向排列区块的容器。
        /// </summary>
        private void InitializeComponent()
        {
            this._bar = new System.Windows.Forms.Panel();
            this._chkOnlySelected = new System.Windows.Forms.CheckBox();
            this._labHint = new System.Windows.Forms.Label();
            this._flow = new System.Windows.Forms.FlowLayoutPanel();
            this._bar.SuspendLayout();
            this.SuspendLayout();
            // 
            // _bar
            // 
            this._bar.Controls.Add(this._labHint);
            this._bar.Controls.Add(this._chkOnlySelected);
            this._bar.Dock = System.Windows.Forms.DockStyle.Top;
            this._bar.Location = new System.Drawing.Point(0, 0);
            this._bar.Name = "_bar";
            this._bar.Size = new System.Drawing.Size(800, 30);
            this._bar.TabIndex = 0;
            // 
            // _chkOnlySelected
            // 
            this._chkOnlySelected.AutoSize = true;
            this._chkOnlySelected.Location = new System.Drawing.Point(6, 6);
            this._chkOnlySelected.Name = "_chkOnlySelected";
            this._chkOnlySelected.Size = new System.Drawing.Size(130, 18);
            this._chkOnlySelected.TabIndex = 0;
            this._chkOnlySelected.Text = "仅显示选中区域";
            this._chkOnlySelected.UseVisualStyleBackColor = true;
            // 
            // _labHint
            // 
            this._labHint.AutoSize = true;
            this._labHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this._labHint.Location = new System.Drawing.Point(160, 8);
            this._labHint.Name = "_labHint";
            this._labHint.Size = new System.Drawing.Size(0, 14);
            this._labHint.TabIndex = 1;
            this._labHint.Text = "在「监视」页双击某行可定位到这里";
            // 
            // _flow
            // 
            this._flow.AutoScroll = true;
            this._flow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._flow.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._flow.Location = new System.Drawing.Point(0, 30);
            this._flow.Name = "_flow";
            this._flow.Padding = new System.Windows.Forms.Padding(4);
            this._flow.Size = new System.Drawing.Size(800, 370);
            this._flow.TabIndex = 1;
            this._flow.WrapContents = false;
            // 
            // MonitorDetailView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._flow);
            this.Controls.Add(this._bar);
            this.Name = "MonitorDetailView";
            this.Size = new System.Drawing.Size(800, 400);
            this._bar.ResumeLayout(false);
            this._bar.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel _bar;
        private System.Windows.Forms.CheckBox _chkOnlySelected;
        private System.Windows.Forms.Label _labHint;
        private System.Windows.Forms.FlowLayoutPanel _flow;
    }
}
