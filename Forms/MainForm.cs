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
                Ten = a.Name,
                DiaDiem = a.Location,
                BatDau = a.StartTime.ToString("HH:mm"),
                KetThuc = a.EndTime.ToString("HH:mm"),
                Loai = a is GroupMeeting ? "Group" : "Personal"
            })
            .ToList();

        dgvAppointments.DataSource = dailyAppointments;
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
                "Chon Yes de thay the, No/Cancel de chon gio khac.";

            var result = MessageBox.Show(
                conflictMessage,
                "Conflict Warning",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);

            if (result != DialogResult.Yes)
            {
                lblStatus.ForeColor = Color.DarkGreen;
                lblStatus.Text = "Da huy. Vui long chon gio khac.";
                return;
            }

            shouldReplace = true;
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