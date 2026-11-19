namespace PlcTest.View.Controls
{
    partial class MonitorRegionBlock
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
        /// 设计器生成：标题栏 + 空矩阵。矩阵的行列在运行时按条目长度生成，
        /// 设计时只保留一个空壳，方便在设计器里看到结构。
        /// </summary>
        private void InitializeComponent()
        {
            this._bar = new System.Windows.Forms.Panel();
            this._labTitle = new System.Windows.Forms.Label();
            this._btnToggle = new System.Windows.Forms.Button();
            this._matrix = new System.Windows.Forms.TableLayoutPanel();
            this._bar.SuspendLayout();
            this.SuspendLayout();
            // 
            // _bar
            // 
            this._bar.Controls.Add(this._labTitle);
            this._bar.Controls.Add(this._btnToggle);
            this._bar.Dock = System.Windows.Forms.DockStyle.Top;
            this._bar.Location = new System.Drawing.Point(0, 0);
            this._bar.Name = "_bar";
            this._bar.Size = new System.Drawing.Size(760, 26);
            this._bar.TabIndex = 0;
            // 
            // _labTitle
            // 
            this._labTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labTitle.Location = new System.Drawing.Point(0, 0);
            this._labTitle.Name = "_labTitle";
            this._labTitle.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this._labTitle.Size = new System.Drawing.Size(700, 26);
            this._labTitle.TabIndex = 0;
            this._labTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _btnToggle
            // 
            this._btnToggle.Dock = System.Windows.Forms.DockStyle.Right;
            this._btnToggle.Location = new System.Drawing.Point(700, 0);
            this._btnToggle.Name = "_btnToggle";
            this._btnToggle.Size = new System.Drawing.Size(60, 26);
            this._btnToggle.TabIndex = 1;
            this._btnToggle.Text = "收起";
            this._btnToggle.UseVisualStyleBackColor = true;
            // 
            // _matrix
            // 
            this._matrix.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this._matrix.ColumnCount = 1;
            this._matrix.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._matrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this._matrix.Location = new System.Drawing.Point(0, 26);
            this._matrix.Name = "_matrix";
            this._matrix.RowCount = 1;
            this._matrix.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this._matrix.Size = new System.Drawing.Size(760, 88);
            this._matrix.TabIndex = 1;
            // 
            // MonitorRegionBlock
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._matrix);
            this.Controls.Add(this._bar);
            this.Name = "MonitorRegionBlock";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.Size = new System.Drawing.Size(760, 120);
            this._bar.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel _bar;
        private System.Windows.Forms.Label _labTitle;
        private System.Windows.Forms.Button _btnToggle;
        private System.Windows.Forms.TableLayoutPanel _matrix;
    }
}
