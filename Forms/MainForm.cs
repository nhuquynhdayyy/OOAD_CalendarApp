namespace CalendarApp.Forms;

using CalendarApp.Database;
using CalendarApp.Models;
using CalendarApp.Services;
using CalendarService = CalendarApp.Services.Calendar;

public partial class MainForm : Form
{
    private readonly DatabaseHelper _db;
    private CalendarService _calendar = null!;
    private User _currentUser = null!;
    private DateTime _activeDate;

    public MainForm()
    {
        InitializeComponent();
        _db = new DatabaseHelper();
        LoadCurrentUser();
        LoadCalendarData();
        _activeDate = DateTime.Today;
        monthCalendar1.SetDate(_activeDate);
        lblActiveDate.Text = _activeDate.ToString("dddd, dd/MM/yyyy");
        RefreshGrid();
    }

    private void LoadCurrentUser()
    {
        _currentUser = new User("u001", "Nguyen Van A", "a@email.com");
        _calendar = new CalendarService(_currentUser.UserId);
    }

    private void LoadCalendarData()
    {
        try
        {
            var appointmentCount = 0;
            var groupMeetingCount = 0;
            var joinedGroupMeetingCount = 0;

            foreach (var appointment in _db.GetAppointmentsByUserId(_currentUser.UserId))
            {
                _calendar.AddAppointment(appointment);
                appointmentCount++;
            }

            foreach (var groupMeeting in _db.GetGroupMeetings())
            {
                _calendar.AddGroupMeeting(groupMeeting);
                groupMeetingCount++;
            }
            foreach (var joinedGroupMeeting in _db.GetGroupMeetingsByUserId(_currentUser.UserId))
            {
                _calendar.AddJoinedGroupMeeting(joinedGroupMeeting, _currentUser);
                joinedGroupMeetingCount++;
            }

            lblStatus.ForeColor = groupMeetingCount > 0 ? Color.DarkGreen : Color.DarkOrange;
            lblStatus.Text = groupMeetingCount > 0
                ? $"Da tai {appointmentCount} appointment, {groupMeetingCount} group meeting an va {joinedGroupMeetingCount} group da tham gia."
                : "Chua tai duoc group meeting nao. Hay kiem tra bang GroupMeetings trong database.";
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Text = $"Khong the tai du lieu tu database: {ex.Message}";
        }
    }

    private void RefreshGrid()
    {
        var dailyAppointments = _calendar.Appointments
            .Where(a => a.StartTime.Date == _activeDate.Date)
            .OrderBy(a => a.StartTime)
            .Select(a => new
            {
                Id = a.Id,
                Ten = a.Name,
                DiaDiem = a.Location,
                BatDau = a.StartTime.ToString("HH:mm"),
                KetThuc = a.EndTime.ToString("HH:mm"),
                Loai = a is GroupMeeting ? "Group" : "Personal"
            })
            .ToList();

        dgvAppointments.DataSource = dailyAppointments;
        if (dgvAppointments.Columns.Contains("Id"))
        {
            dgvAppointments.Columns["Id"].Visible = false;
        }
    }

    private void MonthCalendar1_DateSelected(object? sender, DateRangeEventArgs e)
    {
        _activeDate = e.Start;
        lblActiveDate.Text = _activeDate.ToString("dddd, dd/MM/yyyy");
        RefreshGrid();
    }

    private void BtnAddAppointment_Click(object? sender, EventArgs e)
    {
        using var dialog = new AddApptDialog(_activeDate);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        var appointment = new Appointment(
            Guid.NewGuid().ToString(),
            dialog.ApptName,
            dialog.ApptLocation,
            dialog.StartTime,
            dialog.EndTime);

        var validator = new Validator();
        if (!validator.Validate(appointment))
        {
            MessageBox.Show(
                "Thong tin khong hop le. Ten khong duoc de trong va thoi gian ket thuc phai sau thoi gian bat dau.",
                "Loi nhap lieu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        var conflicting = _calendar.GetConflictingAppointment(appointment);
        var shouldReplace = false;
        if (conflicting != null)
        {
            var conflictMessage = $"Trung lich voi: '{conflicting.Name}'\n" +
                $"({conflicting.StartTime:HH:mm} - {conflicting.EndTime:HH:mm})\n\n" +
                "Chon Yes de chon gio khac.\n" +
                "Chon No de thay the.";

            var result = MessageBox.Show(
                conflictMessage,
                "Conflict Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using var retryDialog = new AddApptDialog(_activeDate);
                retryDialog.LoadAppointment(appointment);
                lblStatus.ForeColor = Color.DarkOrange;
                lblStatus.Text = "Vui long chon gio khac.";
                
                if (retryDialog.ShowDialog(this) != DialogResult.OK)
                    return;
                
                appointment = new Appointment(
                    Guid.NewGuid().ToString(),
                    retryDialog.ApptName,
                    retryDialog.ApptLocation,
                    retryDialog.StartTime,
                    retryDialog.EndTime);
                
                var stillConflicting = _calendar.GetConflictingAppointment(appointment);
                if (stillConflicting != null)
                {
                    MessageBox.Show("Van con trung lich. Vui long chon gio khac.", 
                        "Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                shouldReplace = true;
            }
        }

        var groupMatch = _calendar.FindGroupMatch(appointment);
        if (groupMatch != null)
        {
            var joinResult = MessageBox.Show(
                $"Da ton tai Group Meeting '{groupMatch.Name}'\n" +
                $"({groupMatch.StartTime:HH:mm} - {groupMatch.EndTime:HH:mm})\n\n" +
                "Ban co muon tham gia group meeting nay khong?",
                "Group Meeting",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (joinResult == DialogResult.Yes)
            {
                if (shouldReplace && conflicting != null)
                {
                    _calendar.RemoveAppointment(conflicting);
                    TryDeleteAppointment(conflicting.Id);
                }

                _calendar.AddParticipantToGroup(groupMatch, _currentUser);
                TryAddParticipantToGroup(groupMatch.Id, _currentUser.UserId);
                lblStatus.ForeColor = Color.DarkGreen;
                lblStatus.Text = $"Da tham gia group meeting '{groupMatch.Name}' thanh cong.";
                RefreshGrid();
                return;
            }
        }

        if (shouldReplace && conflicting != null)
        {
            _calendar.ReplaceAppointment(conflicting, appointment);
            TryDeleteAppointment(conflicting.Id);
        }
        else
        {
            _calendar.AddAppointment(appointment);
        }

        if (!TryInsertAppointment(appointment))
        {
            return;
        }

        if (dialog.HasReminder)
        {
            var reminder = new Reminder(appointment.Id, dialog.AlertTime, dialog.ReminderType);
            _calendar.AddReminder(reminder);
            TryInsertReminder(reminder);
        }

        lblStatus.ForeColor = Color.DarkGreen;
        lblStatus.Text = "Da luu appointment thanh cong.";
        MessageBox.Show(
            "Appointment da duoc luu thanh cong.",
            "Thanh cong",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        RefreshGrid();
    }

    private void BtnUpdateAppointment_Click(object? sender, EventArgs e)
    {
        if (dgvAppointments.CurrentRow == null)
        {
            MessageBox.Show("Vui long chon mot appointment de cap nhat.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string id = dgvAppointments.CurrentRow.Cells["Id"].Value.ToString()!;
        var appointment = _calendar.Appointments.FirstOrDefault(a => a.Id == id);
        
        if (appointment == null) return;
        if (appointment is GroupMeeting)
        {
            MessageBox.Show("Khong the cap nhat Group Meeting tu day.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new AddApptDialog(_activeDate);
        dialog.LoadAppointment(appointment);

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            appointment.Name = dialog.ApptName;
            appointment.Location = dialog.ApptLocation;
            appointment.StartTime = dialog.StartTime;
            appointment.EndTime = dialog.EndTime;

            var validator = new Validator();
            if (!validator.Validate(appointment))
            {
                MessageBox.Show("Thong tin khong hop le.", "Loi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _db.UpdateAppointment(appointment);
                lblStatus.Text = "Da cap nhat appointment thanh cong.";
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi cap nhat: {ex.Message}", "Loi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnDeleteAppointment_Click(object? sender, EventArgs e)
    {
        if (dgvAppointments.CurrentRow == null)
        {
            MessageBox.Show("Vui long chon mot appointment de xoa.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string id = dgvAppointments.CurrentRow.Cells["Id"].Value.ToString()!;
        string name = dgvAppointments.CurrentRow.Cells["Ten"].Value.ToString()!;

        var result = MessageBox.Show($"Ban co chac chan muon xoa appointment '{name}'?", "Xac nhan xoa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        
        if (result == DialogResult.Yes)
        {
            try
            {
                _db.DeleteAppointment(id);
                var appt = _calendar.Appointments.FirstOrDefault(a => a.Id == id);
                if (appt != null) _calendar.RemoveAppointment(appt);
                
                lblStatus.Text = "Da xoa appointment thanh cong.";
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loi khi xoa: {ex.Message}", "Loi Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private bool TryInsertAppointment(Appointment appointment)
    {
        try
        {
            _db.InsertAppointment(_currentUser.UserId, appointment);
            return true;
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Text = $"Khong the luu appointment vao database: {ex.Message}";
            MessageBox.Show(lblStatus.Text, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void TryDeleteAppointment(string appointmentId)
    {
        try
        {
            _db.DeleteAppointment(appointmentId);
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Text = $"Khong the xoa appointment cu trong database: {ex.Message}";
        }
    }

    private void TryInsertReminder(Reminder reminder)
    {
        try
        {
            _db.InsertReminder(reminder);
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Text = $"Appointment da luu, nhung reminder chua duoc luu: {ex.Message}";
        }
    }

    private void TryAddParticipantToGroup(string meetingId, string userId)
    {
        try
        {
            _db.AddParticipantToGroup(meetingId, userId);
        }
        catch (Exception ex)
        {
            lblStatus.ForeColor = Color.DarkRed;
            lblStatus.Text = $"Da them vao lich tam thoi, nhung chua luu participant vao database: {ex.Message}";
        }
    }
}