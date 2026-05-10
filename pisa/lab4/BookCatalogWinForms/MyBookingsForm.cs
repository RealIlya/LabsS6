using System;
using System.Windows.Forms;

namespace BookCatalogWinForms
{
    public partial class MyBookingsForm : Form
    {
        public MyBookingsForm()
        {
            InitializeComponent();
            Theme.Apply(this);
            LoadBookings();
        }

        private void LoadBookings()
        {
            dgvBookings.DataSource = null;

            if (DataStore.CurrentUser == null)
            {
                lblInfo.Text = "Вы не авторизованы";
                btnClose.BackColor = Theme.PanelBack;
                return;
            }

            var bookings = DataStore.GetUserBookings(DataStore.CurrentUser.UserID);
            dgvBookings.DataSource = bookings;

            if (bookings.Count == 0)
            {
                lblInfo.Text = "У вас нет забронированных книг";
                btnCancel.Enabled = false;
            }
            else
            {
                lblInfo.Text = $"Забронировано: {bookings.Count} книг";
                btnCancel.Enabled = false;
            }
        }

        private void dgvBookings_SelectionChanged(object sender, EventArgs e)
        {
            btnCancel.Enabled = dgvBookings.CurrentRow != null;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (dgvBookings.CurrentRow == null)
            {
                MessageBox.Show("Выберите бронирование для отмены", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var booking = (Booking)dgvBookings.CurrentRow.DataBoundItem;

            var result = MessageBox.Show(
                $"Отменить бронирование книги «{booking.BookTitle}»?",
                "Подтверждение отмены",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            if (DataStore.CancelBooking(booking.BookingID))
            {
                MessageBox.Show("Бронирование отменено", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadBookings();
            }
            else
            {
                MessageBox.Show("Не удалось отменить бронирование", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
