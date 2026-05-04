using CalendarApp.Models;

namespace CalendarApp.Forms;

public class AddApptDialog : Form
{
    private readonly TextBox txtName;
    private readonly TextBox txtLocation;
    private readonly DateTimePicker dtpStart;
    private readonly DateTimePicker dtpEnd;
    private readonly DateTimePicker dtpAlert;
    private readonly CheckBox chkReminder;
    private readonly ComboBox cmbReminderType;
    private readonly Panel pnlReminder;

    public string ApptName => txtName.Text.Trim();
    public string ApptLocation => txtLocation.Text.Trim();
    public new string Location => ApptLocation;
    public DateTime StartTime => dtpStart.Value;
    public DateTime EndTime => dtpEnd.Value;
    public bool HasReminder => chkReminder.Checked;
    public DateTime AlertTime => dtpAlert.Value;
    public string ReminderType => cmbReminderType.Text;

    public AddApptDialog(DateTime activeDate)
    {
        Text = "Add Appointment";
        Size = new Size(400, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var y = 10;

        Controls.Add(new Label { Text = "Ten:", Location = new Point(10, y), Size = new Size(80, 25) });
        txtName = new TextBox { Location = new Point(100, y), Size = new Size(270, 25) };
        Controls.Add(txtName);
        y += 35;

        Controls.Add(new Label { Text = "Dia diem:", Location = new Point(10, y), Size = new Size(80, 25) });
        txtLocation = new TextBox { Location = new Point(100, y), Size = new Size(270, 25) };
        Controls.Add(txtLocation);
        y += 35;

        Controls.Add(new Label { Text = "Bat dau:", Location = new Point(10, y), Size = new Size(80, 25) });
        dtpStart = new DateTimePicker
        {
            Location = new Point(100, y),
            Size = new Size(270, 25),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            Value = activeDate.Date.AddHours(9)
        };
        Controls.Add(dtpStart);
        y += 35;

        Controls.Add(new Label { Text = "Ket thuc:", Location = new Point(10, y), Size = new Size(80, 25) });
        dtpEnd = new DateTimePicker
        {
            Location = new Point(100, y),
            Size = new Size(270, 25),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            Value = activeDate.Date.AddHours(9).AddMinutes(30)
        };
        Controls.Add(dtpEnd);
        y += 35;

        chkReminder = new CheckBox { Text = "Them Reminder", Location = new Point(10, y), Size = new Size(150, 25) };
        Controls.Add(chkReminder);
        y += 30;

        pnlReminder = new Panel { Location = new Point(10, y), Size = new Size(360, 65), Visible = false };
        chkReminder.CheckedChanged += (_, _) => pnlReminder.Visible = chkReminder.Checked;
        pnlReminder.Controls.Add(new Label { Text = "Alert luc:", Location = new Point(0, 5), Size = new Size(80, 25) });
        dtpAlert = new DateTimePicker
        {
            Location = new Point(85, 2),
            Size = new Size(180, 25),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy HH:mm",
            Value = activeDate.Date.AddHours(8).AddMinutes(45)
        };
        pnlReminder.Controls.Add(dtpAlert);

        pnlReminder.Controls.Add(new Label { Text = "Loai:", Location = new Point(0, 35), Size = new Size(80, 25) });
        cmbReminderType = new ComboBox
        {
            Location = new Point(85, 32),
            Size = new Size(130, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbReminderType.Items.AddRange(new object[] { "Popup", "Email", "SMS" });
        cmbReminderType.SelectedIndex = 0;
        pnlReminder.Controls.Add(cmbReminderType);
        Controls.Add(pnlReminder);
        y += 75;

        var btnOk = new Button
        {
            Text = "OK",
            Location = new Point(220, y),
            Size = new Size(70, 30),
            DialogResult = DialogResult.OK,
            BackColor = Color.SteelBlue,
            ForeColor = Color.White,
            UseVisualStyleBackColor = false
        };
        var btnCancel = new Button
        {
            Text = "Huy",
            Location = new Point(300, y),
            Size = new Size(70, 30),
            DialogResult = DialogResult.Cancel
        };

        Controls.Add(btnOk);
        Controls.Add(btnCancel);
        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }

    public void LoadAppointment(Appointment a)
    {
        Text = "Update Appointment";
        txtName.Text = a.Name;
        txtLocation.Text = a.Location;
        dtpStart.Value = a.StartTime;
        dtpEnd.Value = a.EndTime;
    }
}