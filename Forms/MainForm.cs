using CalendarApp.Models;
using CalendarApp.Services;
using CalendarApp.Database;

namespace CalendarApp.Forms
{
    public partial class MainForm : Form
    {
        private CalendarUI _calendarUI;
        private Services.Calendar _calendar;
        private User _currentUser;
        private DatabaseHelper _db;

        public MainForm()
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            _currentUser = new User("u001", "Nguyen Van A", "a@email.com");
            _calendar = _currentUser.GetCalendar();
            _calendarUI = new CalendarUI();
            _calendarUI.ActiveDate = DateTime.Today;
            _calendarUI.ActiveView = "month";

            LoadAppointments();
        }

        private void LoadAppointments()
        {
            var appointments = _db.GetAppointmentsByUserId(_currentUser.UserId);
            foreach (var a in appointments)
                _calendar.AddAppointment(a);

            // THÊM ĐOẠN NÀY — load GroupMeetings từ DB vào Calendar
            var groupMeetings = _db.GetGroupMeetings();
            foreach (var gm in groupMeetings)
                _calendar.AddGroupMeeting(gm);

            RefreshList();
        }

        private void RefreshList()
        {
            listViewAppointments.Items.Clear();
            foreach (var a in _calendar.Appointments)
            {
                var item = new ListViewItem(a.Name);
                item.SubItems.Add(a.Location);
                item.SubItems.Add(a.StartTime.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(a.EndTime.ToString("dd/MM/yyyy HH:mm"));
                item.Tag = a;
                listViewAppointments.Items.Add(item);
            }
        }

        private void btnAddAppointment_Click(object sender, EventArgs e)
        {
            var addForm = new AddAppointmentForm(_calendar, _currentUser, _db, _calendarUI.ActiveDate);
            addForm.ShowDialog();
            RefreshList();
        }

        public void ShowAddAppointment()
        {
            _calendarUI.ShowAddAppointment();
        }

        public void ShowWarning(string msg)
        {
            _calendarUI.ShowWarning(msg);
        }

        public void ShowGroupDialog()
        {
            _calendarUI.ShowGroupDialog();
        }

        private void InitializeComponent()
        {
            this.Text = "Calendar App";
            this.Size = new Size(800, 600);

            var btnAdd = new Button
            {
                Text = "Add Appointment",
                Location = new Point(10, 10),
                Size = new Size(150, 35)
            };
            btnAdd.Click += btnAddAppointment_Click;

            listViewAppointments = new ListView
            {
                Location = new Point(10, 60),
                Size = new Size(760, 480),
                View = View.Details,
                FullRowSelect = true
            };
            listViewAppointments.Columns.Add("Name", 180);
            listViewAppointments.Columns.Add("Location", 150);
            listViewAppointments.Columns.Add("Start", 160);
            listViewAppointments.Columns.Add("End", 160);

            this.Controls.Add(btnAdd);
            this.Controls.Add(listViewAppointments);
        }

        private ListView listViewAppointments;
    }
}
