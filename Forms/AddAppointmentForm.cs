using CalendarApp.Models;
using CalendarApp.Services;
using CalendarApp.Database;

namespace CalendarApp.Forms
{
    public partial class AddAppointmentForm : Form
    {
        private string _name;
        private string _location;
        private DateTime _startTime;
        private DateTime _endTime;

        private Services.Calendar _calendar;
        private User _currentUser;
        private DatabaseHelper _db;
        private Validator _validator;

        private TextBox txtName, txtLocation;
        private DateTimePicker dtpStart, dtpEnd;
        private CheckBox chkReminder;
        private DateTimePicker dtpReminderTime;
        private ComboBox cmbReminderType;
        private Button btnSubmit, btnCancel;

        public AddAppointmentForm(Services.Calendar calendar, User user, DatabaseHelper db, DateTime activeDate)
        {
            _calendar = calendar;
            _currentUser = user;
            _db = db;
            _validator = new Validator();

            InitializeComponent(activeDate);
        }

        public bool ValidateInput()
        {
            var window = new AddAppointmentWindow(
                txtName.Text, txtLocation.Text,
                dtpStart.Value, dtpEnd.Value);
            return window.ValidateInput();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                MessageBox.Show(
                    "Invalid input: Name cannot be empty and end time must be after start time.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var window = new AddAppointmentWindow(
                txtName.Text, txtLocation.Text,
                dtpStart.Value, dtpEnd.Value);
            var newAppt = window.Submit();

            bool hasConflict = _calendar.CheckConflict(newAppt);

            if (hasConflict)
            {
                var existingAppt = _calendar.GetConflictingAppointment(newAppt);

                var conflictResult = MessageBox.Show(
                    $"Trung lich voi: '{existingAppt?.Name}'\n" +
                    $"({existingAppt?.StartTime:HH:mm} - {existingAppt?.EndTime:HH:mm})\n\n" +
                    "Chon Yes de chon gio khac.\n" +
                    "Chon No de thay the.",
                    "Conflict Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (conflictResult == DialogResult.Yes)
                {
                    dtpStart.Focus();
                    return;
                }
                else
                {
                    if (existingAppt != null)
                    {
                        _calendar.ReplaceAppointment(existingAppt, newAppt);
                        _db.DeleteAppointment(existingAppt.Id);
                        _db.InsertAppointment(_currentUser.UserId, newAppt);
                    }
                    ShowConfirmation();
                    this.Close();
                    return;
                }
            }

            GroupMeeting? groupMatch = _calendar.FindGroupMatch(newAppt);

            if (groupMatch != null)
            {
                var joinResult = MessageBox.Show(
                    $"An existing group meeting '{groupMatch.Name}' matches your appointment.\n" +
                    "Do you want to join that group meeting instead?",
                    "Join Group Meeting?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (joinResult == DialogResult.Yes)
                {
                    var updatedGroup = _calendar.AddParticipantToGroup(groupMatch, _currentUser);
                    _db.AddParticipantToGroup(updatedGroup.Id, _currentUser.UserId);

                    MessageBox.Show("You have joined the group meeting!", "Confirmed",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                    return;
                }
            }

            _calendar.AddAppointment(newAppt);
            _db.InsertAppointment(_currentUser.UserId, newAppt);

            if (chkReminder.Checked)
            {
                var reminder = new Reminder(
                    newAppt.Id,
                    dtpReminderTime.Value,
                    cmbReminderType.SelectedItem?.ToString() ?? "popup"
                );
                _calendar.AddReminder(reminder);
                _db.InsertReminder(reminder);
            }

            ShowConfirmation();
            this.Close();
        }

        private void ShowConfirmation()
        {
            MessageBox.Show("Appointment saved successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkReminder_CheckedChanged(object sender, EventArgs e)
        {
            dtpReminderTime.Enabled = chkReminder.Checked;
            cmbReminderType.Enabled = chkReminder.Checked;
        }

        private void InitializeComponent(DateTime activeDate)
        {
            this.Text = "Add Appointment";
            this.Size = new Size(420, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 15;

            this.Controls.Add(new Label { Text = "Name *", Location = new Point(15, y), Size = new Size(100, 20) });
            txtName = new TextBox { Location = new Point(120, y), Size = new Size(260, 25) };
            this.Controls.Add(txtName);
            y += 35;

            this.Controls.Add(new Label { Text = "Location", Location = new Point(15, y), Size = new Size(100, 20) });
            txtLocation = new TextBox { Location = new Point(120, y), Size = new Size(260, 25) };
            this.Controls.Add(txtLocation);
            y += 35;

            this.Controls.Add(new Label { Text = "Start Time *", Location = new Point(15, y), Size = new Size(100, 20) });
            dtpStart = new DateTimePicker { Location = new Point(120, y), Size = new Size(260, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Value = activeDate };
            this.Controls.Add(dtpStart);
            y += 35;

            this.Controls.Add(new Label { Text = "End Time *", Location = new Point(15, y), Size = new Size(100, 20) });
            dtpEnd = new DateTimePicker { Location = new Point(120, y), Size = new Size(260, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Value = activeDate.AddHours(1) };
            this.Controls.Add(dtpEnd);
            y += 35;

            chkReminder = new CheckBox { Text = "Set Reminder", Location = new Point(15, y), Size = new Size(150, 25) };
            chkReminder.CheckedChanged += chkReminder_CheckedChanged;
            this.Controls.Add(chkReminder);
            y += 35;

            this.Controls.Add(new Label { Text = "Remind At", Location = new Point(15, y), Size = new Size(100, 20) });
            dtpReminderTime = new DateTimePicker { Location = new Point(120, y), Size = new Size(260, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Enabled = false };
            this.Controls.Add(dtpReminderTime);
            y += 35;

            this.Controls.Add(new Label { Text = "Type", Location = new Point(15, y), Size = new Size(100, 20) });
            cmbReminderType = new ComboBox { Location = new Point(120, y), Size = new Size(260, 25), Enabled = false };
            cmbReminderType.Items.AddRange(new[] { "popup", "email" });
            cmbReminderType.SelectedIndex = 0;
            this.Controls.Add(cmbReminderType);
            y += 45;

            btnSubmit = new Button { Text = "Save", Location = new Point(200, y), Size = new Size(80, 32) };
            btnSubmit.Click += btnSubmit_Click;
            btnCancel = new Button { Text = "Cancel", Location = new Point(295, y), Size = new Size(80, 32) };
            btnCancel.Click += btnCancel_Click;
            this.Controls.Add(btnSubmit);
            this.Controls.Add(btnCancel);
        }
    }
}
