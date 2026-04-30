namespace CalendarApp.Models
{
    public class CalendarUI
    {
        public DateTime ActiveDate { get; set; }
        public string ActiveView { get; set; }

        public CalendarUI()
        {
            ActiveDate = DateTime.Today;
            ActiveView = "month";
        }

        public void ShowAddAppointment()
        {
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowGroupDialog()
        {
        }
    }
}
