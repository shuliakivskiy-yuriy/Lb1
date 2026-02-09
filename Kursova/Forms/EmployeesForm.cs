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
    public partial class EmployeesForm : Form
    {
        private int selectedId = 0;
        public EmployeesForm()
        {
            InitializeComponent();
        }

        private void EmployeesForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            string query = "SELECT EmployeeId, FullName, Position, Phone, Email, HireDate, Password FROM Employees";
            dgvEmployees.DataSource = DatabaseContext.ExecuteQuery(query);

            if (dgvEmployees.Columns.Contains("EmployeeId"))
                dgvEmployees.Columns["EmployeeId"].Visible = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введіть ПІБ співробітника!");
                return;
            }

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string sql = @"INSERT INTO Employees (FullName, Position, Phone, Email, HireDate, Password) 
                               VALUES (@Name, @Pos, @Phone, @Email, @Date, @Pass)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", txtFullName.Text);
                cmd.Parameters.AddWithValue("@Pos", txtPosition.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                cmd.Parameters.AddWithValue("@Pass", txtPassword.Text);

                cmd.ExecuteNonQuery();
            }

            LoadData();
            ClearFields();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) return;

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string sql = @"UPDATE Employees 
                               SET FullName=@Name, Position=@Pos, Phone=@Phone, Email=@Email, Password=@Pass 
                               WHERE EmployeeId=@Id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", txtFullName.Text);
                cmd.Parameters.AddWithValue("@Pos", txtPosition.Text);
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@Pass", txtPassword.Text);
                cmd.Parameters.AddWithValue("@Id", selectedId);

                cmd.ExecuteNonQuery();
            }

            LoadData();
            ClearFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) return;

            if (MessageBox.Show("Видалити запис про співробітника?", "Увага", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DatabaseContext.GetConnection())
                    {
                        if (conn.State == ConnectionState.Closed) conn.Open();

                        string sql = "DELETE FROM Employees WHERE EmployeeId=@Id";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                    ClearFields();
                }
                catch (Exception)
                {
                    MessageBox.Show("Помилка видалення. Можливо, цей менеджер прив'язаний до замовлень.");
                }
            }
        }

        private void dgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["EmployeeId"].Value);

                txtFullName.Text = row.Cells["FullName"].Value.ToString();
                txtPosition.Text = row.Cells["Position"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPassword.Text = row.Cells["Password"].Value.ToString();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        private void ClearFields()
        {
            txtFullName.Clear();
            txtPosition.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            selectedId = 0;
        }
    }
}
