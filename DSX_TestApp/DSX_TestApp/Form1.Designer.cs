namespace DSX_TestApp
{
  partial class Form1
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.button1 = new System.Windows.Forms.Button();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(100, 88);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Location = new System.Drawing.Point(100, 148);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Size = new System.Drawing.Size(150, 100);
      this.splitContainer1.TabIndex = 1;
      // 
      // numericUpDown1
      // 
      this.numericUpDown1.AccessibleName = "";
      this.numericUpDown1.Location = new System.Drawing.Point(363, 181);
      this.numericUpDown1.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
      this.numericUpDown1.Name = "numericUpDown1";
      this.numericUpDown1.Size = new System.Drawing.Size(120, 23);
      this.numericUpDown1.TabIndex = 2;
      this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.numericUpDown1);
      this.Controls.Add(this.splitContainer1);
      this.Controls.Add(this.button1);
      this.Name = "Form1";
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.Form1_Load);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
    private Button button1;
    private SplitContainer splitContainer1;
    private NumericUpDown numericUpDown1;
  }
}