using Kursova.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursova.Forms
{
    public partial class ShipmentForm : Form
    {
        public ShipmentForm()
        {
            InitializeComponent();
        }

        private void ShipmentForm_Load(object sender, EventArgs e)
        {
            LoadPaidOrders();
            InitCarriers();
        }
        private void LoadPaidOrders()
        {
            try
            {
                string query = @"SELECT OrderId, OrderDate, TotalAmount, CustomerId, Status 
                                 FROM Orders 
                                 WHERE Status = N'Оплачено'";

                DataTable dt = DatabaseContext.ExecuteQuery(query);
                dgvOrders.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження замовлень: " + ex.Message);
            }
        }

        private void InitCarriers()
        {
            cmbCarrier.Items.Clear();
            cmbCarrier.Items.Add("Нова Пошта");
            cmbCarrier.Items.Add("Укрпошта");
            cmbCarrier.Items.Add("Meest");
            cmbCarrier.Items.Add("Delivery");
            cmbCarrier.SelectedIndex = 0;
        }

        private void btnShip_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count == 0)
            {
                MessageBox.Show("Будь ласка, оберіть замовлення зі списку!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTracking.Text))
            {
                MessageBox.Show("Введіть трек-номер (Tracking Number)!");
                return;
            }

            if (cmbCarrier.SelectedItem == null)
            {
                MessageBox.Show("Оберіть перевізника!");
                return;
            }

            int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderId"].Value);
            decimal totalAmount = Convert.ToDecimal(dgvOrders.SelectedRows[0].Cells["TotalAmount"].Value);
            string carrier = cmbCarrier.SelectedItem.ToString();
            string trackingNumber = txtTracking.Text.Trim();

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string sqlInsertShipment = @"INSERT INTO Shipments (OrderId, ShipmentDate, TrackingNumber, Carrier, ShippedAmount) 
                                                 VALUES (@OId, @Date, @Track, @Carr, @Amount)";

                    SqlCommand cmdShip = new SqlCommand(sqlInsertShipment, conn, transaction);
                    cmdShip.Parameters.AddWithValue("@OId", orderId);
                    cmdShip.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmdShip.Parameters.AddWithValue("@Track", trackingNumber);
                    cmdShip.Parameters.AddWithValue("@Carr", carrier);
                    cmdShip.Parameters.AddWithValue("@Amount", totalAmount);

                    cmdShip.ExecuteNonQuery();

                    string sqlUpdateOrder = @"UPDATE Orders 
                                              SET Status = N'Відправлено' 
                                              WHERE OrderId = @OId";

                    SqlCommand cmdUpdate = new SqlCommand(sqlUpdateOrder, conn, transaction);
                    cmdUpdate.Parameters.AddWithValue("@OId", orderId);

                    cmdUpdate.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show($"Замовлення №{orderId} успішно відправлено!\nТТН: {trackingNumber}", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtTracking.Clear();
                    LoadPaidOrders();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Сталася помилка при обробці: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
