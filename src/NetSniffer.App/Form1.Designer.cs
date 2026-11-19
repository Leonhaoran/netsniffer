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
    private Button followTcpStreamButton = null!;
    private Button clearTcpStreamButton = null!;
    private FlowLayoutPanel filterPanel = null!;
    private Label protocolFilterLabel = null!;
    private ComboBox protocolFilterComboBox = null!;
    private Label ipFilterLabel = null!;
    private TextBox ipFilterTextBox = null!;
    private Label portFilterLabel = null!;
    private TextBox portFilterTextBox = null!;
    private Button applyFilterButton = null!;
    private Button resetFilterButton = null!;
    private Label filterResultLabel = null!;
    private SplitContainer mainSplitContainer = null!;
    private ListView packetListView = null!;
    private ColumnHeader sequenceColumn = null!;
    private ColumnHeader timeColumn = null!;
    private ColumnHeader sourceColumn = null!;
    private ColumnHeader destinationColumn = null!;
    private ColumnHeader protocolColumn = null!;
    private ColumnHeader lengthColumn = null!;
    private ColumnHeader summaryColumn = null!;
    private SplitContainer detailSplitContainer = null!;
    private GroupBox protocolDetailsGroupBox = null!;
    private TreeView packetDetailTreeView = null!;
    private GroupBox hexDumpGroupBox = null!;
    private RichTextBox hexDumpTextBox = null!;
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
        followTcpStreamButton = new Button();
        clearTcpStreamButton = new Button();
        filterPanel = new FlowLayoutPanel();
        protocolFilterLabel = new Label();
        protocolFilterComboBox = new ComboBox();
        ipFilterLabel = new Label();
        ipFilterTextBox = new TextBox();
        portFilterLabel = new Label();
        portFilterTextBox = new TextBox();
        applyFilterButton = new Button();
        resetFilterButton = new Button();
        filterResultLabel = new Label();
        mainSplitContainer = new SplitContainer();
        packetListView = new ListView();
        sequenceColumn = new ColumnHeader();
        timeColumn = new ColumnHeader();
        sourceColumn = new ColumnHeader();
        destinationColumn = new ColumnHeader();
        protocolColumn = new ColumnHeader();
        lengthColumn = new ColumnHeader();
        summaryColumn = new ColumnHeader();
        detailSplitContainer = new SplitContainer();
        protocolDetailsGroupBox = new GroupBox();
        packetDetailTreeView = new TreeView();
        hexDumpGroupBox = new GroupBox();
        hexDumpTextBox = new RichTextBox();
        statusStrip = new StatusStrip();
        statusTextLabel = new ToolStripStatusLabel();
        statusSpringLabel = new ToolStripStatusLabel();
        packetCountLabel = new ToolStripStatusLabel();
        uiTimer = new System.Windows.Forms.Timer(components);
        rootLayout.SuspendLayout();
        commandPanel.SuspendLayout();
        filterPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
        mainSplitContainer.Panel1.SuspendLayout();
        mainSplitContainer.Panel2.SuspendLayout();
        mainSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)detailSplitContainer).BeginInit();
        detailSplitContainer.Panel1.SuspendLayout();
        detailSplitContainer.Panel2.SuspendLayout();
        detailSplitContainer.SuspendLayout();
        protocolDetailsGroupBox.SuspendLayout();
        hexDumpGroupBox.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(commandPanel, 0, 0);
        rootLayout.Controls.Add(filterPanel, 0, 1);
        rootLayout.Controls.Add(mainSplitContainer, 0, 2);
        rootLayout.Controls.Add(statusStrip, 0, 3);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.RowCount = 4;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        commandPanel.Controls.Add(deviceLabel);
        commandPanel.Controls.Add(deviceComboBox);
        commandPanel.Controls.Add(refreshButton);
        commandPanel.Controls.Add(startButton);
        commandPanel.Controls.Add(stopButton);
        commandPanel.Controls.Add(clearButton);
        commandPanel.Controls.Add(followTcpStreamButton);
        commandPanel.Controls.Add(clearTcpStreamButton);
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
        clearButton.Margin = new Padding(0, 0, 8, 0);
        clearButton.Text = "清空";
        clearButton.UseVisualStyleBackColor = true;

        followTcpStreamButton.AutoSize = true;
        followTcpStreamButton.Enabled = false;
        followTcpStreamButton.Margin = new Padding(0, 0, 8, 0);
        followTcpStreamButton.Text = "追踪 TCP 流";
        followTcpStreamButton.UseVisualStyleBackColor = true;

        clearTcpStreamButton.AutoSize = true;
        clearTcpStreamButton.Enabled = false;
        clearTcpStreamButton.Margin = new Padding(0);
        clearTcpStreamButton.Text = "退出追踪";
        clearTcpStreamButton.UseVisualStyleBackColor = true;

        filterPanel.Controls.Add(protocolFilterLabel);
        filterPanel.Controls.Add(protocolFilterComboBox);
        filterPanel.Controls.Add(ipFilterLabel);
        filterPanel.Controls.Add(ipFilterTextBox);
        filterPanel.Controls.Add(portFilterLabel);
        filterPanel.Controls.Add(portFilterTextBox);
        filterPanel.Controls.Add(applyFilterButton);
        filterPanel.Controls.Add(resetFilterButton);
        filterPanel.Controls.Add(filterResultLabel);
        filterPanel.Dock = DockStyle.Fill;
        filterPanel.Padding = new Padding(10, 7, 10, 6);
        filterPanel.WrapContents = false;

        protocolFilterLabel.AutoSize = true;
        protocolFilterLabel.Margin = new Padding(0, 6, 6, 0);
        protocolFilterLabel.Text = "协议：";

        protocolFilterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        protocolFilterComboBox.Margin = new Padding(0, 1, 12, 0);
        protocolFilterComboBox.Size = new Size(105, 25);

        ipFilterLabel.AutoSize = true;
        ipFilterLabel.Margin = new Padding(0, 6, 6, 0);
        ipFilterLabel.Text = "IP：";

        ipFilterTextBox.Margin = new Padding(0, 1, 12, 0);
        ipFilterTextBox.PlaceholderText = "源或目的 IP";
        ipFilterTextBox.Size = new Size(190, 23);

        portFilterLabel.AutoSize = true;
        portFilterLabel.Margin = new Padding(0, 6, 6, 0);
        portFilterLabel.Text = "端口：";

        portFilterTextBox.Margin = new Padding(0, 1, 12, 0);
        portFilterTextBox.PlaceholderText = "0-65535";
        portFilterTextBox.Size = new Size(90, 23);

        applyFilterButton.AutoSize = true;
        applyFilterButton.Margin = new Padding(0, 0, 8, 0);
        applyFilterButton.Text = "应用过滤";
        applyFilterButton.UseVisualStyleBackColor = true;

        resetFilterButton.AutoSize = true;
        resetFilterButton.Margin = new Padding(0, 0, 16, 0);
        resetFilterButton.Text = "重置";
        resetFilterButton.UseVisualStyleBackColor = true;

        filterResultLabel.AutoSize = true;
        filterResultLabel.Margin = new Padding(0, 6, 0, 0);
        filterResultLabel.Text = "保留：0";

        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.Location = new Point(0, 106);
        mainSplitContainer.Name = "mainSplitContainer";
        mainSplitContainer.Orientation = Orientation.Horizontal;
        mainSplitContainer.Panel1.Controls.Add(packetListView);
        mainSplitContainer.Panel1MinSize = 160;
        mainSplitContainer.Panel2.Controls.Add(detailSplitContainer);
        mainSplitContainer.Panel2MinSize = 170;
        mainSplitContainer.Size = new Size(1_180, 626);
        mainSplitContainer.SplitterDistance = 330;
        mainSplitContainer.SplitterWidth = 6;

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

        detailSplitContainer.Dock = DockStyle.Fill;
        detailSplitContainer.Name = "detailSplitContainer";
        detailSplitContainer.Panel1.Controls.Add(protocolDetailsGroupBox);
        detailSplitContainer.Panel1MinSize = 300;
        detailSplitContainer.Panel2.Controls.Add(hexDumpGroupBox);
        detailSplitContainer.Panel2MinSize = 300;
        detailSplitContainer.Size = new Size(1_180, 290);
        detailSplitContainer.SplitterDistance = 500;
        detailSplitContainer.SplitterWidth = 6;

        protocolDetailsGroupBox.Controls.Add(packetDetailTreeView);
        protocolDetailsGroupBox.Dock = DockStyle.Fill;
        protocolDetailsGroupBox.Padding = new Padding(8);
        protocolDetailsGroupBox.Text = "协议详情";

        packetDetailTreeView.Dock = DockStyle.Fill;
        packetDetailTreeView.HideSelection = false;
        packetDetailTreeView.Name = "packetDetailTreeView";

        hexDumpGroupBox.Controls.Add(hexDumpTextBox);
        hexDumpGroupBox.Dock = DockStyle.Fill;
        hexDumpGroupBox.Padding = new Padding(8);
        hexDumpGroupBox.Text = "原始数据（Hex / ASCII）";

        hexDumpTextBox.BackColor = SystemColors.Window;
        hexDumpTextBox.DetectUrls = false;
        hexDumpTextBox.Dock = DockStyle.Fill;
        hexDumpTextBox.Font = new Font("Consolas", 9F);
        hexDumpTextBox.ReadOnly = true;
        hexDumpTextBox.WordWrap = false;

        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange([statusTextLabel, statusSpringLabel, packetCountLabel]);
        statusTextLabel.Text = "准备就绪";
        statusSpringLabel.Spring = true;
        packetCountLabel.Text = "已捕获：0";

        uiTimer.Interval = 200;

        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1_180, 760);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1_000, 620);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "NetSniffer - 网络嗅探器";
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        commandPanel.ResumeLayout(false);
        commandPanel.PerformLayout();
        filterPanel.ResumeLayout(false);
        filterPanel.PerformLayout();
        mainSplitContainer.Panel1.ResumeLayout(false);
        mainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
        mainSplitContainer.ResumeLayout(false);
        detailSplitContainer.Panel1.ResumeLayout(false);
        detailSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)detailSplitContainer).EndInit();
        detailSplitContainer.ResumeLayout(false);
        protocolDetailsGroupBox.ResumeLayout(false);
        hexDumpGroupBox.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
