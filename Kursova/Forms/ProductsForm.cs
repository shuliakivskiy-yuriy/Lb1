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
    public partial class ProductsForm : Form
    {
        private int selectedId = 0;
        public ProductsForm()
        {
            InitializeComponent();
        }

        private void ProductsForm_Load(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            string query = "SELECT ProductId, Name, Category, Unit, Price, StockQuantity, Code FROM Products";
            dgvProducts.DataSource = DatabaseContext.ExecuteQuery(query);

            if (dgvProducts.Columns.Contains("ProductId"))
                dgvProducts.Columns["ProductId"].Visible = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCode.Text))
            {
                MessageBox.Show("Будь ласка, заповніть Назву та Код товару!");
                return;
            }

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string sql = @"INSERT INTO Products (Name, Category, Unit, Price, StockQuantity, Code) 
                               VALUES (@Name, @Cat, @Unit, @Price, @Qty, @Code)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Cat", txtCategory.Text);
                cmd.Parameters.AddWithValue("@Unit", txtUnit.Text);
                cmd.Parameters.AddWithValue("@Price", numPrice.Value);
                cmd.Parameters.AddWithValue("@Qty", (int)numStock.Value);
                cmd.Parameters.AddWithValue("@Code", txtCode.Text);

                cmd.ExecuteNonQuery();
            }

            LoadData();
            ClearFields();
            MessageBox.Show("Товар успішно додано!");
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (selectedId == 0)
            {
                MessageBox.Show("Оберіть товар для редагування!");
                return;
            }

            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                string sql = @"UPDATE Products 
                               SET Name=@Name, Category=@Cat, Unit=@Unit, Price=@Price, StockQuantity=@Qty, Code=@Code
                               WHERE ProductId=@Id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", txtName.Text);
                cmd.Parameters.AddWithValue("@Cat", txtCategory.Text);
                cmd.Parameters.AddWithValue("@Unit", txtUnit.Text);
                cmd.Parameters.AddWithValue("@Price", numPrice.Value);
                cmd.Parameters.AddWithValue("@Qty", (int)numStock.Value);
                cmd.Parameters.AddWithValue("@Code", txtCode.Text);
                cmd.Parameters.AddWithValue("@Id", selectedId);

                cmd.ExecuteNonQuery();
            }

            LoadData();
            ClearFields();
            MessageBox.Show("Дані товару оновлено!");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == 0) return;

            if (MessageBox.Show($"Видалити товар '{txtName.Text}'?", "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = DatabaseContext.GetConnection())
                    {
                        if (conn.State == ConnectionState.Closed) conn.Open();

                        string sql = "DELETE FROM Products WHERE ProductId=@Id";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@Id", selectedId);
                        cmd.ExecuteNonQuery();
                    }
                    LoadData();
                    ClearFields();
                }
                catch (SqlException)
                {
                    MessageBox.Show("Не можна видалити цей товар, оскільки він вже є в замовленнях або накладних.", "Помилка цілісності", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
                selectedId = Convert.ToInt32(row.Cells["ProductId"].Value);

                txtName.Text = row.Cells["Name"].Value.ToString();
                txtCategory.Text = row.Cells["Category"].Value.ToString();
                txtUnit.Text = row.Cells["Unit"].Value.ToString();
                txtCode.Text = row.Cells["Code"].Value.ToString();

                if (decimal.TryParse(row.Cells["Price"].Value.ToString(), out decimal price))
                    numPrice.Value = price;

                if (int.TryParse(row.Cells["StockQuantity"].Value.ToString(), out int qty))
                    numStock.Value = qty;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        private void ClearFields()
        {
            txtName.Clear();
            txtCategory.Clear();
            txtUnit.Clear();
            txtCode.Clear();
            numPrice.Value = 0;
            numStock.Value = 0;
            selectedId = 0;
        }
    }
}
