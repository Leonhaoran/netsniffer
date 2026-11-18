#nullable enable

namespace NetSniffer.App;

partial class Form1
{
    private System.ComponentModel.IContainer? components = null;
    private TableLayoutPanel rootLayout = null!;
    private FlowLayoutPanel commandPanel = null!;
    private Label deviceLabel = null!;
    private ComboBox deviceComboBox = null!;
    private Button refreshButton = null!;
    private Button startButton = null!;
    private Button stopButton = null!;
    private Button clearButton = null!;
    private ListView packetListView = null!;
    private ColumnHeader sequenceColumn = null!;
    private ColumnHeader timeColumn = null!;
    private ColumnHeader sourceColumn = null!;
    private ColumnHeader destinationColumn = null!;
    private ColumnHeader protocolColumn = null!;
    private ColumnHeader lengthColumn = null!;
    private ColumnHeader summaryColumn = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel statusTextLabel = null!;
    private ToolStripStatusLabel statusSpringLabel = null!;
    private ToolStripStatusLabel packetCountLabel = null!;
    private System.Windows.Forms.Timer uiTimer = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        rootLayout = new TableLayoutPanel();
        commandPanel = new FlowLayoutPanel();
        deviceLabel = new Label();
        deviceComboBox = new ComboBox();
        refreshButton = new Button();
        startButton = new Button();
        stopButton = new Button();
        clearButton = new Button();
        packetListView = new ListView();
        sequenceColumn = new ColumnHeader();
        timeColumn = new ColumnHeader();
        sourceColumn = new ColumnHeader();
        destinationColumn = new ColumnHeader();
        protocolColumn = new ColumnHeader();
        lengthColumn = new ColumnHeader();
        summaryColumn = new ColumnHeader();
        statusStrip = new StatusStrip();
        statusTextLabel = new ToolStripStatusLabel();
        statusSpringLabel = new ToolStripStatusLabel();
        packetCountLabel = new ToolStripStatusLabel();
        uiTimer = new System.Windows.Forms.Timer(components);
        rootLayout.SuspendLayout();
        commandPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(commandPanel, 0, 0);
        rootLayout.Controls.Add(packetListView, 0, 1);
        rootLayout.Controls.Add(statusStrip, 0, 2);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        commandPanel.Controls.Add(deviceLabel);
        commandPanel.Controls.Add(deviceComboBox);
        commandPanel.Controls.Add(refreshButton);
        commandPanel.Controls.Add(startButton);
        commandPanel.Controls.Add(stopButton);
        commandPanel.Controls.Add(clearButton);
        commandPanel.Dock = DockStyle.Fill;
        commandPanel.FlowDirection = FlowDirection.LeftToRight;
        commandPanel.Padding = new Padding(10, 11, 10, 8);
        commandPanel.WrapContents = false;

        deviceLabel.AutoSize = true;
        deviceLabel.Margin = new Padding(0, 7, 8, 0);
        deviceLabel.Text = "网卡：";

        deviceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        deviceComboBox.Margin = new Padding(0, 2, 8, 0);
        deviceComboBox.Size = new Size(500, 25);

        refreshButton.AutoSize = true;
        refreshButton.Margin = new Padding(0, 0, 8, 0);
        refreshButton.Text = "刷新";
        refreshButton.UseVisualStyleBackColor = true;

        startButton.AutoSize = true;
        startButton.Enabled = false;
        startButton.Margin = new Padding(0, 0, 8, 0);
        startButton.Text = "开始抓包";
        startButton.UseVisualStyleBackColor = true;

        stopButton.AutoSize = true;
        stopButton.Enabled = false;
        stopButton.Margin = new Padding(0, 0, 8, 0);
        stopButton.Text = "停止";
        stopButton.UseVisualStyleBackColor = true;

        clearButton.AutoSize = true;
        clearButton.Margin = new Padding(0);
        clearButton.Text = "清空";
        clearButton.UseVisualStyleBackColor = true;

        packetListView.Columns.AddRange([
            sequenceColumn,
            timeColumn,
            sourceColumn,
            destinationColumn,
            protocolColumn,
            lengthColumn,
            summaryColumn,
        ]);
        packetListView.Dock = DockStyle.Fill;
        packetListView.FullRowSelect = true;
        packetListView.GridLines = true;
        packetListView.HideSelection = false;
        packetListView.View = View.Details;

        sequenceColumn.Text = "序号";
        sequenceColumn.Width = 90;
        timeColumn.Text = "时间";
        timeColumn.Width = 140;
        sourceColumn.Text = "源地址";
        sourceColumn.Width = 180;
        destinationColumn.Text = "目的地址";
        destinationColumn.Width = 180;
        protocolColumn.Text = "协议";
        protocolColumn.Width = 90;
        lengthColumn.Text = "长度";
        lengthColumn.Width = 80;
        summaryColumn.Text = "信息";
        summaryColumn.Width = 420;

        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange([statusTextLabel, statusSpringLabel, packetCountLabel]);
        statusTextLabel.Text = "准备就绪";
        statusSpringLabel.Spring = true;
        packetCountLabel.Text = "已捕获：0";

        uiTimer.Interval = 200;

        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1_100, 650);
        Controls.Add(rootLayout);
        MinimumSize = new Size(850, 450);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "NetSniffer - 网络嗅探器";
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        commandPanel.ResumeLayout(false);
        commandPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
