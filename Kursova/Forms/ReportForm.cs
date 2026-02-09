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
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
            InitStatusComboBox();
        }
        private void InitStatusComboBox()
        {
            cmbNewStatus.Items.Clear();
            cmbNewStatus.Items.AddRange(new object[] {
                "Новий",
                "Оплачено",
                "Відправлено",
                "Отримано"
            });
            cmbNewStatus.SelectedIndex = 0;
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            LoadReportData();
        }
        private void LoadReportData()
        {
            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                StringBuilder query = new StringBuilder(@"
                    SELECT o.OrderId, o.OrderDate, o.Status, c.Name AS Client, e.FullName AS Manager, o.TotalAmount 
                    FROM Orders o
                    JOIN Customers c ON o.CustomerId = c.CustomerId
                    JOIN Employees e ON o.EmployeeId = e.EmployeeId
                    WHERE 1=1 ");

                query.Append(" AND o.OrderDate >= @From");
                query.Append(" AND o.OrderDate <= @To");

                SqlCommand cmd = new SqlCommand(query.ToString(), conn);
                cmd.Parameters.AddWithValue("@From", dtpFrom.Value.Date);
                cmd.Parameters.AddWithValue("@To", dtpTo.Value.Date.AddDays(1));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dgvReport.DataSource = dt;

                decimal sum = 0;
                foreach (DataRow row in dt.Rows)
                {
                    sum += Convert.ToDecimal(row["TotalAmount"]);
                }
                lblTotalReport.Text = $"Загальна виручка: {sum} грн";
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (dgvReport.SelectedRows.Count == 0)
            {
                MessageBox.Show("Оберіть замовлення зі списку!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbNewStatus.SelectedItem == null)
            {
                MessageBox.Show("Оберіть новий статус!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int orderId = Convert.ToInt32(dgvReport.SelectedRows[0].Cells["OrderId"].Value);
            string oldStatus = dgvReport.SelectedRows[0].Cells["Status"].Value.ToString();
            string newStatus = cmbNewStatus.SelectedItem.ToString();

            if (oldStatus == newStatus) return;
            var result = MessageBox.Show(
                $"Змінити статус замовлення №{orderId}\nз '{oldStatus}' на '{newStatus}'?",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                UpdateOrderStatusInDb(orderId, newStatus, oldStatus);
                LoadReportData();
            }
        }
        private void UpdateOrderStatusInDb(int orderId, string newStatus, string oldStatus)
        {
            try
            {
                using (SqlConnection conn = DatabaseContext.GetConnection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    SqlTransaction transaction = conn.BeginTransaction();
                    string updateSql = "UPDATE Orders SET Status = @Status WHERE OrderId = @Id";

                    SqlCommand cmdUpdate = new SqlCommand(updateSql, conn, transaction);
                    cmdUpdate.Parameters.AddWithValue("@Status", newStatus);
                    cmdUpdate.Parameters.AddWithValue("@Id", orderId);
                    cmdUpdate.ExecuteNonQuery();

                    if (oldStatus == "Відправлено" && newStatus == "Оплачено")
                    {
                        string deleteShipmentSql = "DELETE FROM Shipments WHERE OrderId = @Id";
                        SqlCommand cmdDelete = new SqlCommand(deleteShipmentSql, conn, transaction);
                        cmdDelete.Parameters.AddWithValue("@Id", orderId);
                        cmdDelete.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Статус успішно оновлено!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при оновленні статусу: " + ex.Message);
            }
        }
    }
}
