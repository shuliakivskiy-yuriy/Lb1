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

namespace Kursova
{
    public partial class CustomersForm : Form
    {
        private int selectedId = 0;
        public CustomersForm()
        {
            InitializeComponent();
        }

        private void CustomersForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            string query = "SELECT CustomerId, Name, Address, Phone, Email, RegistrationDate FROM Customers";
            dgvCustomers.DataSource = DatabaseContext.ExecuteQuery(query);

            if (dgvCustomers.Columns.Contains("CustomerId"))
                dgvCustomers.Columns["CustomerId"].Visible = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введіть назву клієнта!");
                return;
            }

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string sql = @"INSERT INTO Customers (Name, Address, Phone, Email, RegistrationDate) 
                               VALUES (@Name, @Address, @Phone, @Email, @Date)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@Date", DateTime.Now);

                cmd.ExecuteNonQuery();
            }

            LoadData();
            ClearFields();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Оберіть клієнта для редагування!");
                return;
            }

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string sql = @"UPDATE Customers 
                               SET Name=@Name, Address=@Address, Phone=@Phone, Email=@Email 
                               WHERE CustomerId=@Id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@Id", selectedId);

                cmd.ExecuteNonQuery();
            }

            LoadData();
            ClearFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) return;

            if (MessageBox.Show("Видалити клієнта?", "Увага", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DatabaseContext.GetConnection())
                    {
                        if (conn.State == ConnectionState.Closed) conn.Open();

                        string sql = "DELETE FROM Customers WHERE CustomerId=@Id";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                    ClearFields();
                }
                catch (Exception)
                {
                    MessageBox.Show("Не можна видалити клієнта, оскільки у нього є історія замовлень.");
                }
            }
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["CustomerId"].Value);

                txtName.Text = row.Cells["Name"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        private void ClearFields()
        {
            txtName.Clear();
            txtAddress.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            selectedId = 0;
        }
    }
}
