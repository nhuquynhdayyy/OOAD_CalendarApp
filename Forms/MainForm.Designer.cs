namespace CalendarApp.Forms;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;

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
        components = new System.ComponentModel.Container();
        monthCalendar1 = new MonthCalendar();
        btnAddAppointment = new Button();
        btnUpdateAppointment = new Button();
        btnDeleteAppointment = new Button();
        lblActiveDate = new Label();
        dgvAppointments = new DataGridView();
        lblStatus = new Label();
        ((System.ComponentModel.ISupportInitialize)dgvAppointments).BeginInit();
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1080, 620);
        MinimumSize = new Size(960, 560);
        Text = "Calendar App";

        monthCalendar1.Location = new Point(12, 55);
        monthCalendar1.MaxSelectionCount = 1;
        monthCalendar1.DateSelected += MonthCalendar1_DateSelected;

        btnAddAppointment.BackColor = Color.SteelBlue;
        btnAddAppointment.ForeColor = Color.White;
        btnAddAppointment.Location = new Point(12, 12);
        btnAddAppointment.Size = new Size(130, 32);
        btnAddAppointment.Text = "+ Add";
        btnAddAppointment.UseVisualStyleBackColor = false;
        btnAddAppointment.Click += BtnAddAppointment_Click;

        btnUpdateAppointment.BackColor = Color.DarkOrange;
        btnUpdateAppointment.ForeColor = Color.White;
        btnUpdateAppointment.Location = new Point(150, 12);
        btnUpdateAppointment.Size = new Size(90, 32);
        btnUpdateAppointment.Text = "Update";
        btnUpdateAppointment.UseVisualStyleBackColor = false;
        btnUpdateAppointment.Click += BtnUpdateAppointment_Click;

        btnDeleteAppointment.BackColor = Color.Crimson;
        btnDeleteAppointment.ForeColor = Color.White;
        btnDeleteAppointment.Location = new Point(250, 12);
        btnDeleteAppointment.Size = new Size(90, 32);
        btnDeleteAppointment.Text = "Delete";
        btnDeleteAppointment.UseVisualStyleBackColor = false;
        btnDeleteAppointment.Click += BtnDeleteAppointment_Click;

        lblActiveDate.Font = new Font("Arial", 11F, FontStyle.Bold);
        lblActiveDate.Location = new Point(350, 16);
        lblActiveDate.Size = new Size(690, 28);
        lblActiveDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        dgvAppointments.AllowUserToAddRows = false;
        dgvAppointments.AllowUserToDeleteRows = false;
        dgvAppointments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvAppointments.Location = new Point(280, 55);
        dgvAppointments.ReadOnly = true;
        dgvAppointments.RowHeadersVisible = false;
        dgvAppointments.Size = new Size(780, 505);
        dgvAppointments.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        lblStatus.ForeColor = Color.DarkGreen;
        lblStatus.Location = new Point(12, 580);
        lblStatus.Size = new Size(1048, 25);
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        Controls.AddRange(new Control[]
        {
            monthCalendar1,
            btnAddAppointment,
            btnUpdateAppointment,
            btnDeleteAppointment,
            lblActiveDate,
            dgvAppointments,
            lblStatus
        });
        ((System.ComponentModel.ISupportInitialize)dgvAppointments).EndInit();
    }

    #endregion

    private MonthCalendar monthCalendar1;
    private Button btnAddAppointment;
    private Button btnUpdateAppointment;
    private Button btnDeleteAppointment;
    private Label lblActiveDate;
    private DataGridView dgvAppointments;
    private Label lblStatus;
}