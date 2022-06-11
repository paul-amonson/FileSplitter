///////////////////////////////////////////////////////////////////////
//        Copyright©2003-2022 Paul Amonson, All Rights Reserved      //
///////////////////////////////////////////////////////////////////////
#region Using Section
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Amonson.Utils;
#endregion

namespace FileSplitter
{
	public class MainForm : System.Windows.Forms.Form
	{
        #region Constants
        const int BUFFERSIZE = 2097152;
        #endregion

        #region Private Fields and Properties
        private Configuration _config = null;
        private bool _doCombine = false;
        private int _combineLength = 5;
        private bool _isProcessing = false;
        private int _lastIndex;
        private bool _inEvent = false;
        private byte[] _buffer = null;
        private ManualResetEvent _cancel = null;
        private string _title = "";
        private bool IsProcessing
        {
            get
            {
                bool local;
                lock(this)
                {
                    local = _isProcessing;
                }
                return local;
            }
            set
            {
                lock(this)
                {
                    _isProcessing = value;
                }
            }
        }
        #endregion

        #region Windows Form Designer Fields
        private System.Windows.Forms.OpenFileDialog _openFileDialog;
        private System.Windows.Forms.TextBox _filenameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _filenameButton;
        private System.Windows.Forms.ImageList _imageList;
        private System.Windows.Forms.RadioButton _floppyRadioButton;
        private System.Windows.Forms.RadioButton _zip100RadioButton;
        private System.Windows.Forms.RadioButton _zip250RadioButton;
        private System.Windows.Forms.RadioButton _cdr650RadioButton;
        private System.Windows.Forms.RadioButton _cdr700RadioButton;
        private System.Windows.Forms.RadioButton _2GBRadioButton;
        private System.Windows.Forms.RadioButton _customRadioButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label _sizeLabel;
        private System.Windows.Forms.ComboBox _customSizeTypeComboBox;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Button _processButton;
        private System.Windows.Forms.Button _exitButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _maskTextBox;
        private System.Windows.Forms.ErrorProvider _errorProvider;
        private System.Windows.Forms.RichTextBox _helpRichTextBox;
        private System.Windows.Forms.TextBox _customSizeTextBox;
        private System.Windows.Forms.Label _percentLabel;
        private System.Windows.Forms.CheckBox _hashCheckBox;
        private System.ComponentModel.IContainer components;
        #endregion

        #region Construction and Destruction
		public MainForm()
		{
            _buffer = new byte[BUFFERSIZE];
            _config = Configuration.Load();

            _cancel = new ManualResetEvent(false);

            InitializeComponent();

            _inEvent = true;
            IndexToRadioButton(_config.SelectedRadioButton);
            _maskTextBox.Text = _config.OutputMask;
            _customSizeTypeComboBox.SelectedIndex = _config.CustomType;
            _lastIndex = _config.CustomType;
            ValueToUI(_lastIndex, _config[6]);
            //_helpRichTextBox.Text = _config.Help;
            _inEvent = false;
            _hashCheckBox.Checked = _config.GenerateHash;
            _title = Text;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
                _cancel.Close();
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
        #endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this._filenameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._filenameButton = new System.Windows.Forms.Button();
            this._imageList = new System.Windows.Forms.ImageList(this.components);
            this._floppyRadioButton = new System.Windows.Forms.RadioButton();
            this._zip100RadioButton = new System.Windows.Forms.RadioButton();
            this._zip250RadioButton = new System.Windows.Forms.RadioButton();
            this._cdr650RadioButton = new System.Windows.Forms.RadioButton();
            this._cdr700RadioButton = new System.Windows.Forms.RadioButton();
            this._2GBRadioButton = new System.Windows.Forms.RadioButton();
            this._customRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this._sizeLabel = new System.Windows.Forms.Label();
            this._customSizeTypeComboBox = new System.Windows.Forms.ComboBox();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._processButton = new System.Windows.Forms.Button();
            this._exitButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._maskTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this._hashCheckBox = new System.Windows.Forms.CheckBox();
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this._helpRichTextBox = new System.Windows.Forms.RichTextBox();
            this._customSizeTextBox = new System.Windows.Forms.TextBox();
            this._percentLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // _openFileDialog
            // 
            this._openFileDialog.Filter = "All Files (*.*)|*.*";
            this._openFileDialog.Title = "Select the File to Split or Combine";
            // 
            // _filenameTextBox
            // 
            this._filenameTextBox.AllowDrop = true;
            this._filenameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._filenameTextBox.BackColor = System.Drawing.Color.GhostWhite;
            this._filenameTextBox.Enabled = false;
            this._filenameTextBox.Location = new System.Drawing.Point(80, 12);
            this._filenameTextBox.Name = "_filenameTextBox";
            this._filenameTextBox.Size = new System.Drawing.Size(388, 22);
            this._filenameTextBox.TabIndex = 0;
            this._filenameTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.DropFile);
            this._filenameTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnterSink);
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Filename:";
            this.label1.DragDrop += new System.Windows.Forms.DragEventHandler(this.DropFile);
            this.label1.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnterSink);
            // 
            // _filenameButton
            // 
            this._filenameButton.AllowDrop = true;
            this._filenameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._filenameButton.BackColor = System.Drawing.SystemColors.Control;
            this._filenameButton.ImageIndex = 1;
            this._filenameButton.ImageList = this._imageList;
            this._filenameButton.Location = new System.Drawing.Point(472, 8);
            this._filenameButton.Name = "_filenameButton";
            this._filenameButton.Size = new System.Drawing.Size(28, 28);
            this._filenameButton.TabIndex = 1;
            this._filenameButton.UseVisualStyleBackColor = false;
            this._filenameButton.Click += new System.EventHandler(this._filenameButton_Click);
            this._filenameButton.DragDrop += new System.Windows.Forms.DragEventHandler(this.DropFile);
            this._filenameButton.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnterSink);
            // 
            // _imageList
            // 
            this._imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imageList.ImageStream")));
            this._imageList.TransparentColor = System.Drawing.Color.Transparent;
            this._imageList.Images.SetKeyName(0, "");
            this._imageList.Images.SetKeyName(1, "");
            // 
            // _floppyRadioButton
            // 
            this._floppyRadioButton.Location = new System.Drawing.Point(12, 48);
            this._floppyRadioButton.Name = "_floppyRadioButton";
            this._floppyRadioButton.Size = new System.Drawing.Size(148, 24);
            this._floppyRadioButton.TabIndex = 3;
            this._floppyRadioButton.TabStop = true;
            this._floppyRadioButton.Text = "Floppy Size Pieces";
            this._floppyRadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // _zip100RadioButton
            // 
            this._zip100RadioButton.Location = new System.Drawing.Point(12, 72);
            this._zip100RadioButton.Name = "_zip100RadioButton";
            this._zip100RadioButton.Size = new System.Drawing.Size(172, 24);
            this._zip100RadioButton.TabIndex = 4;
            this._zip100RadioButton.Text = "100MB Zip Drive Pieces";
            this._zip100RadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // _zip250RadioButton
            // 
            this._zip250RadioButton.Location = new System.Drawing.Point(12, 96);
            this._zip250RadioButton.Name = "_zip250RadioButton";
            this._zip250RadioButton.Size = new System.Drawing.Size(172, 24);
            this._zip250RadioButton.TabIndex = 5;
            this._zip250RadioButton.Text = "250MB Zip Drive Pieces";
            this._zip250RadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // _cdr650RadioButton
            // 
            this._cdr650RadioButton.Location = new System.Drawing.Point(12, 120);
            this._cdr650RadioButton.Name = "_cdr650RadioButton";
            this._cdr650RadioButton.Size = new System.Drawing.Size(180, 24);
            this._cdr650RadioButton.TabIndex = 6;
            this._cdr650RadioButton.Text = "650MB CD-R/RW Pieces";
            this._cdr650RadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // _cdr700RadioButton
            // 
            this._cdr700RadioButton.Location = new System.Drawing.Point(12, 144);
            this._cdr700RadioButton.Name = "_cdr700RadioButton";
            this._cdr700RadioButton.Size = new System.Drawing.Size(180, 24);
            this._cdr700RadioButton.TabIndex = 7;
            this._cdr700RadioButton.Text = "700MB CD-R/RW Pieces";
            this._cdr700RadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // _2GBRadioButton
            // 
            this._2GBRadioButton.Location = new System.Drawing.Point(12, 168);
            this._2GBRadioButton.Name = "_2GBRadioButton";
            this._2GBRadioButton.Size = new System.Drawing.Size(104, 24);
            this._2GBRadioButton.TabIndex = 8;
            this._2GBRadioButton.Text = "2GB Pieces";
            this._2GBRadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // _customRadioButton
            // 
            this._customRadioButton.Location = new System.Drawing.Point(12, 192);
            this._customRadioButton.Name = "_customRadioButton";
            this._customRadioButton.Size = new System.Drawing.Size(152, 24);
            this._customRadioButton.TabIndex = 9;
            this._customRadioButton.Text = "Custom Size Pieces";
            this._customRadioButton.Click += new System.EventHandler(this.RadioButton_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(40, 224);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 20);
            this.label2.TabIndex = 10;
            this.label2.Text = "Size:";
            // 
            // _sizeLabel
            // 
            this._sizeLabel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._sizeLabel.Location = new System.Drawing.Point(12, 248);
            this._sizeLabel.Name = "_sizeLabel";
            this._sizeLabel.Size = new System.Drawing.Size(276, 20);
            this._sizeLabel.TabIndex = 12;
            this._sizeLabel.Text = "Size in Bytes:  0 bytes";
            // 
            // _customSizeTypeComboBox
            // 
            this._customSizeTypeComboBox.BackColor = System.Drawing.Color.GhostWhite;
            this._customSizeTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._customSizeTypeComboBox.Enabled = false;
            this._customSizeTypeComboBox.Items.AddRange(new object[] {
            "bytes",
            "KB",
            "MB",
            "GB"});
            this._customSizeTypeComboBox.Location = new System.Drawing.Point(208, 220);
            this._customSizeTypeComboBox.Name = "_customSizeTypeComboBox";
            this._customSizeTypeComboBox.Size = new System.Drawing.Size(80, 24);
            this._customSizeTypeComboBox.TabIndex = 13;
            this._customSizeTypeComboBox.SelectedIndexChanged += new System.EventHandler(this._customSizeTypeComboBox_SelectedIndexChanged);
            // 
            // _progressBar
            // 
            this._progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._progressBar.Location = new System.Drawing.Point(64, 272);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(432, 16);
            this._progressBar.Step = 1;
            this._progressBar.TabIndex = 14;
            // 
            // _processButton
            // 
            this._processButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._processButton.BackColor = System.Drawing.SystemColors.Control;
            this._processButton.Enabled = false;
            this._processButton.Location = new System.Drawing.Point(8, 304);
            this._processButton.Name = "_processButton";
            this._processButton.Size = new System.Drawing.Size(108, 28);
            this._processButton.TabIndex = 15;
            this._processButton.Text = "Process";
            this._processButton.UseVisualStyleBackColor = false;
            this._processButton.Click += new System.EventHandler(this._processButton_Click);
            // 
            // _exitButton
            // 
            this._exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._exitButton.BackColor = System.Drawing.SystemColors.Control;
            this._exitButton.Location = new System.Drawing.Point(388, 304);
            this._exitButton.Name = "_exitButton";
            this._exitButton.Size = new System.Drawing.Size(108, 28);
            this._exitButton.TabIndex = 16;
            this._exitButton.Text = "Exit";
            this._exitButton.UseVisualStyleBackColor = false;
            this._exitButton.Click += new System.EventHandler(this._exitButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.groupBox1.Controls.Add(this._maskTextBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this._hashCheckBox);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(296, 188);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 80);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output Configuration";
            // 
            // _maskTextBox
            // 
            this._maskTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._maskTextBox.BackColor = System.Drawing.Color.GhostWhite;
            this._maskTextBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._maskTextBox.Location = new System.Drawing.Point(128, 28);
            this._maskTextBox.Name = "_maskTextBox";
            this._maskTextBox.Size = new System.Drawing.Size(48, 22);
            this._maskTextBox.TabIndex = 1;
            this._maskTextBox.Text = "nnnn";
            this._maskTextBox.TextChanged += new System.EventHandler(this._maskTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(8, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 20);
            this.label4.TabIndex = 0;
            this.label4.Text = "Output Ext. Mask:";
            // 
            // _hashCheckBox
            // 
            this._hashCheckBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._hashCheckBox.Location = new System.Drawing.Point(8, 52);
            this._hashCheckBox.Name = "_hashCheckBox";
            this._hashCheckBox.Size = new System.Drawing.Size(184, 24);
            this._hashCheckBox.TabIndex = 20;
            this._hashCheckBox.Text = "Generate a \'.md5\' hashfile";
            this._hashCheckBox.CheckedChanged += new System.EventHandler(this._hashCheckBox_CheckedChanged);
            // 
            // _errorProvider
            // 
            this._errorProvider.ContainerControl = this;
            // 
            // _helpRichTextBox
            // 
            this._helpRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._helpRichTextBox.BackColor = System.Drawing.Color.WhiteSmoke;
            this._helpRichTextBox.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._helpRichTextBox.Location = new System.Drawing.Point(208, 48);
            this._helpRichTextBox.Name = "_helpRichTextBox";
            this._helpRichTextBox.ReadOnly = true;
            this._helpRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this._helpRichTextBox.Size = new System.Drawing.Size(288, 136);
            this._helpRichTextBox.TabIndex = 18;
            this._helpRichTextBox.Text = resources.GetString("_helpRichTextBox.Text");
            this._helpRichTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this._helpRichTextBox_LinkClicked);
            // 
            // _customSizeTextBox
            // 
            this._customSizeTextBox.Location = new System.Drawing.Point(84, 220);
            this._customSizeTextBox.Name = "_customSizeTextBox";
            this._customSizeTextBox.Size = new System.Drawing.Size(120, 22);
            this._customSizeTextBox.TabIndex = 11;
            this._customSizeTextBox.Text = "0";
            this._customSizeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this._customSizeTextBox.TextChanged += new System.EventHandler(this._customSizeTextBox_TextChanged);
            // 
            // _percentLabel
            // 
            this._percentLabel.Font = new System.Drawing.Font("Arial", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._percentLabel.Location = new System.Drawing.Point(12, 272);
            this._percentLabel.Name = "_percentLabel";
            this._percentLabel.Size = new System.Drawing.Size(48, 16);
            this._percentLabel.TabIndex = 19;
            this._percentLabel.Text = "0%";
            this._percentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(504, 334);
            this.Controls.Add(this._percentLabel);
            this.Controls.Add(this._customSizeTextBox);
            this.Controls.Add(this._filenameTextBox);
            this.Controls.Add(this._helpRichTextBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._exitButton);
            this.Controls.Add(this._processButton);
            this.Controls.Add(this._progressBar);
            this.Controls.Add(this._customSizeTypeComboBox);
            this.Controls.Add(this._sizeLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._customRadioButton);
            this.Controls.Add(this._2GBRadioButton);
            this.Controls.Add(this._cdr700RadioButton);
            this.Controls.Add(this._cdr650RadioButton);
            this.Controls.Add(this._zip250RadioButton);
            this.Controls.Add(this._zip100RadioButton);
            this.Controls.Add(this._floppyRadioButton);
            this.Controls.Add(this._filenameButton);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1280, 368);
            this.MinimumSize = new System.Drawing.Size(512, 368);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "File Splitter and Combiner";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Closed += new System.EventHandler(this.MainForm_Closed);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DropFile);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DragEnterSink);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

        #region Application Entry Point
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}
        #endregion

        #region Application Event Handlers
        private void OnIdle(object obj, EventArgs args)
        {
            UpdateUI();
        }

        private void _exitButton_Click(object sender, System.EventArgs e)
        {
            if(IsProcessing == true)
            {
                _cancel.Set();
            }
            else
            {
                Close();
            }
        }

        private void _filenameButton_Click(object sender, System.EventArgs e)
        {
            if(_openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string filename = _openFileDialog.FileName;
                if(IsFilePart(Path.GetExtension(filename)) == true)
                {
                    _doCombine = true;
                    _combineLength = Path.GetExtension(filename).Length - 1;
                    if(Path.GetDirectoryName(filename).EndsWith("\\") == true)
                    {
                        filename = Path.GetDirectoryName(filename) + Path.GetFileNameWithoutExtension(filename);
                    }
                    else
                    {
                        filename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);
                    }
                }
                else
                {
                    _doCombine = false;
                }
                _filenameTextBox.Text = filename;
                UpdateUI();
            }
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(IsProcessing == true)
            {
                e.Cancel = true;
            }
            _config.WindowLocation = this.Location;
            _config.WindowSize = this.Size;
            Application.Idle -= new EventHandler(OnIdle);
        }

        private void MainForm_Closed(object sender, System.EventArgs e)
        {
            _config.OutputMask = _maskTextBox.Text;
            _config.SelectedRadioButton = RadioButtonToIndex();
            _config.CustomType = _customSizeTypeComboBox.SelectedIndex;
            _config[6] = UIToValue(_config.CustomType);

            _config.Save();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {
            Point zero = new Point(0, 0);

            if(_config.WindowLocation != zero)
            {
                this.Location = _config.WindowLocation;
            }
            this.Size = _config.WindowSize;

            Application.Idle += new EventHandler(OnIdle);
        }

        private void RadioButton_Click(object sender, System.EventArgs e)
        {
            UpdateUI();
        }

        private void _customSizeTypeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if(_inEvent == false)
            {
                _inEvent = true;
                long value = UIToValue(_lastIndex);
                _lastIndex = _customSizeTypeComboBox.SelectedIndex;
                ValueToUI(_lastIndex, value);
                _config[6] = value;
                UpdateUI();
                _inEvent = false;
            }
            UpdateUI();
        }


        private void _customSizeTextBox_TextChanged(object sender, System.EventArgs e)
        {
            if(_inEvent == false)
            {
                _inEvent = true;
                _config[6] = UIToValue(_lastIndex);
                UpdateUI();
                _inEvent = false;
            }
        }

        private void _maskTextBox_TextChanged(object sender, System.EventArgs e)
        {
            if(_maskTextBox.Text != "n" && _maskTextBox.Text != "nn" &&
                _maskTextBox.Text != "nnn" && _maskTextBox.Text != "nnnn" &&
                _maskTextBox.Text != "nnnnn")
            {
                _errorProvider.SetError(_maskTextBox, "The value can only be n-nnnnn!");
            }
            else
            {
                _errorProvider.SetError(_maskTextBox, "");
                _config.OutputMask = _maskTextBox.Text;
            }
        }

        private void _processButton_Click(object sender, System.EventArgs e)
        {
            IsProcessing = true;
            UpdateUI();
            _progressBar.Value = 0;
            Thread thread = new Thread(new ThreadStart(ThreadProc));
            thread.Start();
        }

        private void _helpRichTextBox_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            Shell.Open(e.LinkText);
        }

        private void _hashCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            _config.GenerateHash = _hashCheckBox.Checked;
        }
        #endregion

        #region Internal Implementation
        private bool IsFilePart(string extension)
        {
            foreach(char c in extension)
            {
                if(c == '.')
                {
                    continue;
                }
                if(c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateUI()
        {
            if(IsProcessing == true)
            {
                _filenameButton.Enabled = false;
                _floppyRadioButton.Enabled = false;
                _zip100RadioButton.Enabled = false;
                _zip250RadioButton.Enabled = false;
                _cdr650RadioButton.Enabled = false;
                _cdr700RadioButton.Enabled = false;
                _2GBRadioButton.Enabled = false;
                _customRadioButton.Enabled = false;
                _customSizeTextBox.Enabled = false;
                _customSizeTypeComboBox.Enabled = false;
                _maskTextBox.Enabled = false;
                _processButton.Enabled = false;
                _exitButton.Enabled = true;
                _exitButton.Text = "Cancel";
            }
            else
            {
                _filenameButton.Enabled = true;
                if(_doCombine == true)
                {
                    _floppyRadioButton.Enabled = false;
                    _zip100RadioButton.Enabled = false;
                    _zip250RadioButton.Enabled = false;
                    _cdr650RadioButton.Enabled = false;
                    _cdr700RadioButton.Enabled = false;
                    _2GBRadioButton.Enabled = false;
                    _customRadioButton.Enabled = false;
                    _customSizeTextBox.Enabled = false;
                    _customSizeTypeComboBox.Enabled = false;
                }
                else
                {
                    _floppyRadioButton.Enabled = true;
                    _zip100RadioButton.Enabled = true;
                    _zip250RadioButton.Enabled = true;
                    _cdr650RadioButton.Enabled = true;
                    _cdr700RadioButton.Enabled = true;
                    _2GBRadioButton.Enabled = true;
                    _customRadioButton.Enabled = true;
                    if(_customRadioButton.Checked == true)
                    {
                        _customSizeTextBox.Enabled = true;
                        _customSizeTypeComboBox.Enabled = true;
                    }
                    else
                    {
                        _customSizeTextBox.Enabled = false;
                        _customSizeTypeComboBox.Enabled = false;
                    }
                }
                _maskTextBox.Enabled = true;
                if(_filenameTextBox.Text == null || _filenameTextBox.Text == "")
                {
                    _processButton.Enabled = false;
                    _processButton.Text = "Process";
                }
                else
                {
                    _processButton.Enabled = true;
                    if(_doCombine == true)
                    {
                        _processButton.Text = "Combine";
                    }
                    else
                    {
                        _processButton.Text = "Split";
                    }
                }
                _exitButton.Enabled = true;
                _exitButton.Text = "Exit";
            }
            NumberFormatInfo nfi = new CultureInfo( "en-US", false ).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            _sizeLabel.Text = "Size: " + _config[RadioButtonToIndex()].ToString("n", nfi) + " bytes.";
            _percentLabel.Text = _progressBar.Value.ToString() + "%";
        }

        private void IndexToRadioButton(int index)
        {
            switch(index)
            {
                case 0:
                    _floppyRadioButton.Checked = true;
                    break;
                case 1:
                    _zip100RadioButton.Checked = true;
                    break;
                case 2:
                    _zip250RadioButton.Checked = true;
                    break;
                case 3:
                    _cdr650RadioButton.Checked = true;
                    break;
                case 4:
                    _cdr700RadioButton.Checked = true;
                    break;
                case 5:
                    _2GBRadioButton.Checked = true;
                    break;
                case 6:
                    _customRadioButton.Checked = true;
                    break;
            }
        }

        private int RadioButtonToIndex()
        {
            if(_floppyRadioButton.Checked == true)
            {
                return 0;
            }
            if(_zip100RadioButton.Checked == true)
            {
                return 1;
            }
            if(_zip250RadioButton.Checked == true)
            {
                return 2;
            }
            if(_cdr650RadioButton.Checked == true)
            {
                return 3;
            }
            if(_cdr700RadioButton.Checked == true)
            {
                return 4;
            }
            if(_2GBRadioButton.Checked == true)
            {
                return 5;
            }
            if(_customRadioButton.Checked == true)
            {
                return 6;
            }
            return -1;
        }

        private void ValueToUI(int index, long value)
        {
            long newValue = 0;
            switch(index)
            {
                case 0:
                    newValue = value;
                    break;
                case 1:
                    newValue = value / 1024;
                    break;
                case 2:
                    newValue = value / 1024 / 1024;
                    break;
                case 3:
                    newValue = value / 1024 / 1024 / 1024;
                    break;
            }
            _customSizeTextBox.Text = newValue.ToString();
            UpdateUI();
        }

        private long UIToValue(int index)
        {
            long value = Convert.ToInt64(_customSizeTextBox.Text);
            switch(index)
            {
                case 0:
                    return value;
                case 1:
                    return value * 1024;
                case 2:
                    return value * 1024 * 1024;
                case 3:
                    return value * 1024 * 1024 * 1024;
            }
            return -1L;
        }
        #endregion

        #region Processing Implementation
        private delegate void UpdateBarHandler(long current, long total);
        private void UpdateBar(long current, long total)
        {
            if(InvokeRequired)
            {
                BeginInvoke(new UpdateBarHandler(UpdateBar), new object[] { current, total });
            }
            else
            {
                int percent = (int)((current * 100L) / total);
                Text = percent.ToString() + "% Complete - " + _title;
                _progressBar.Value = percent;
            }
        }

        private delegate void SetTitleHandler(string title);
        private void SetTitle(string title)
        {
            if(InvokeRequired == true)
            {
                BeginInvoke(new SetTitleHandler(SetTitle), new object[] { title });
            }
            else
            {
                Text = title;
            }
        }

        private void ThreadProc()
        {
            if(_doCombine == false)
            {
                Split(_filenameTextBox.Text, _config[RadioButtonToIndex()], _maskTextBox.Text.Length);
            }
            else
            {
                long size;
                int parts = FindPartCount(_filenameTextBox.Text, out size);
                if(parts > 1)
                {
                    Combine(_filenameTextBox.Text, parts, size);
                }
            }

            IsProcessing = false;
        }

        private int FindPartCount(string fileBase, out long totalSize)
        {
            int count = 0;
            totalSize = 0;
            for(int i = 1; i <= 99999; i++)
            {
                string ext = i.ToString();
                if(ext.Length < _combineLength)
                {
                    string pad = "00000";
                    ext = pad.Substring(0, _combineLength - ext.Length) + ext;
                }
                string fn = fileBase + "." + ext;
                FileAttributes attr;
                Int32 iattr;
                try
                {
                    attr = File.GetAttributes(fn);
                    iattr = (Int32)Convert.ChangeType(attr, typeof(Int32));
                    FileStream stream = File.OpenRead(fn);
                    totalSize += stream.Length;
                    stream.Close();
                }
                catch
                {
                    iattr = -1;
                }
                if(iattr == -1)
                {
                    count = i - 1;
                    break;
                }

            }
            return count;
        }

        private void Combine(string filename, int parts, long totalBytes)
        {
            long current = 0;
            string targetfn = filename;
            int counter = 1;
            while(File.Exists(targetfn) == true)
            {
                if(Path.GetDirectoryName(filename).EndsWith("\\") == true)
                {
                    targetfn = Path.GetDirectoryName(filename) + Path.GetFileNameWithoutExtension(filename);
                }
                else
                {
                    targetfn = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);
                }
                targetfn += "~" + counter.ToString();
                targetfn += Path.GetExtension(filename);
                counter++;
            }
            SetTitle("0% Complete - " + _title);
            FileStream targetStream = File.OpenWrite(targetfn);
            for(int i = 1; i <= parts; i++)
            {
                string ext = i.ToString();
                if(ext.Length < _combineLength)
                {
                    string pad = "00000";
                    ext = pad.Substring(0, _combineLength - ext.Length) + ext;
                }
                string partfn = filename + "." + ext;
                FileStream partStream = File.OpenRead(partfn);
                long partSize = partStream.Length;

                while(partSize > 0)
                {
                    if(_cancel.WaitOne(1, false) == true)
                    {
                        _cancel.Reset();
                        partStream.Close();
                        targetStream.Close();
                        File.Delete(targetfn);
                        UpdateBar(0, 100);
                        return;
                    }
                    long read = (long)partStream.Read(_buffer, 0, (int)Math.Min((long)BUFFERSIZE, partSize));
                    targetStream.Write(_buffer, 0, (int)read);
                    partSize -= read;
                    current += read;
                    UpdateBar(current, totalBytes);
                }

                partStream.Close();
            }
            targetStream.Close();

            if(_hashCheckBox.Checked == true && File.Exists(filename + ".hash") == true)
            {
                SetTitle("Checking Hash - " + _title);
                if(VerifyHash(targetfn, filename) == false)
                {
                    MessageBox.Show(null, "The resulting combined file failed to match the original file hash!  It may be corrupted!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            _filenameTextBox.Text = "";
            UpdateBar(0, 100);
            _doCombine = false;
            SetTitle(_title);
        }

        private void Split(string filename, long partSize, int maskLen)
        {

            if(_hashCheckBox.Checked == true)
            {
                SetTitle("Computing Hash - " + _title);
                SaveHash(filename);
            }
            SetTitle("0% Complete - " + _title);
            long current = 0;
            FileStream fileStream = File.OpenRead(filename);
            long totalSize = fileStream.Length;
            int partCount = (int)(totalSize / partSize);
            if((totalSize % partSize) > 0)
            {
                partCount++;
            }
            for(int i = 1; i <= partCount; i++)
            {
                string ext = i.ToString();
                if(ext.Length < maskLen)
                {
                    string pad = "00000";
                    ext = pad.Substring(0, maskLen - ext.Length) + ext;
                }
                string partfn = filename + "." + ext;
                FileStream partStream = File.OpenWrite(partfn);
                long left = Math.Min(partSize, totalSize - current);
                while(left > 0)
                {
                    if(_cancel.WaitOne(1, false) == true)
                    {
                        _cancel.Reset();
                        partStream.Close();
                        fileStream.Close();
                        CleanupFiles(filename, i, maskLen);
                        UpdateBar(0, 100);
                        return;
                    }
                    int toRead = (int)Math.Min((long)BUFFERSIZE, Math.Min(left, partSize));
                    long read = (int)fileStream.Read(_buffer, 0, toRead);
                    partStream.Write(_buffer, 0, (int)read);
                    current += read;
                    left -= read;
                    UpdateBar(current, totalSize);
                }

                partStream.Close();
            }

            fileStream.Close();

            _filenameTextBox.Text = "";
            UpdateBar(0, 100);

            SetTitle(_title);
        }

        private void CleanupFiles(string baseName, int lastPart, int maskLen)
        {
            for(int i = 1; i <= lastPart; i++)
            {
                string ext = i.ToString();
                if(ext.Length < maskLen)
                {
                    string pad = "00000";
                    ext = pad.Substring(0, maskLen - ext.Length) + ext;
                }
                string partfn = baseName + "." + ext;
                File.Delete(partfn);
            }
        }
        #endregion

        #region Drag Drop Operations
        private void DropFile(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string filename = "";
            string[] filenames = e.Data.GetData("FileDrop", false) as string[];
            if(filenames != null && filenames.Length > 0)
            {
                if(filenames.Length != 1)
                {
                    MessageBox.Show(this, "Please only drag one file at a time please.", "File Splitter and Combiner", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                filename = filenames[0];
                if(IsFilePart(Path.GetExtension(filename)) == true)
                {
                    _doCombine = true;
                    _combineLength = Path.GetExtension(filename).Length - 1;
                    if(Path.GetDirectoryName(filename).EndsWith("\\") == true)
                    {
                        filename = Path.GetDirectoryName(filename) + Path.GetFileNameWithoutExtension(filename);
                    }
                    else
                    {
                        filename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);
                    }
                }
                else
                {
                    _doCombine = false;
                }
                _filenameTextBox.Text = filename;
                UpdateUI();
            }
        }

        private void DragEnterSink(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        #endregion

        #region Hashing Functions
        private byte[] ComputeHash(string filename)
        {
            HashAlgorithm hasher = MD5.Create();
            FileStream stream = File.OpenRead(filename);
            byte[] hash = hasher.ComputeHash(stream);
            stream.Close();
            return hash;
        }

        private void SaveHash(string filename)
        {
            string fn = filename + ".md5";
            byte[] hash = ComputeHash(filename);
            FileStream stream = File.OpenWrite(fn);
            stream.Write(hash, 0, hash.Length);
            stream.Close();
        }

        private bool VerifyHash(string actualFilename, string baseFilename)
        {
            Encoding encoder = new ASCIIEncoding();
            byte[] oldHash = null;
            byte[] newHash = ComputeHash(actualFilename);
            string fn = baseFilename + ".md5";
            FileStream stream = File.OpenRead(fn);
            oldHash = new byte[(int)stream.Length];
            stream.Read(oldHash, 0, (int)stream.Length);
            stream.Close();

            string h1 = encoder.GetString(oldHash, 0, oldHash.Length);
            string h2 = encoder.GetString(newHash, 0, newHash.Length);

            return (h1 == h2);
        }
        #endregion
    }
}
