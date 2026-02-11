using Kursova.Data;
using Kursova.Models;
using Kursova.Repositories;
using Kursova.Services;
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
    public partial class CreateOrderForm : Form
    {
        private OrderService _orderService = new OrderService();
        private OrderRepository _orderRepository = new OrderRepository();
        private DataTable orderItemsTable;
        private decimal totalAmount = 0;
        public CreateOrderForm()
        {
            InitializeComponent();
            InitCartTable();
        }
        private void InitCartTable()
        {
            orderItemsTable = new DataTable();
            orderItemsTable.Columns.Add("ProductId", typeof(int));
            orderItemsTable.Columns.Add("ProductName", typeof(string));
            orderItemsTable.Columns.Add("Price", typeof(decimal));
            orderItemsTable.Columns.Add("Quantity", typeof(int));
            orderItemsTable.Columns.Add("Sum", typeof(decimal));

            dgvItems.DataSource = orderItemsTable;
            dgvItems.Columns["ProductId"].Visible = false;
            dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void CreateOrderForm_Load(object sender, EventArgs e)
        {
            cmbCustomers.DataSource = DatabaseContext.ExecuteQuery("SELECT CustomerId, Name FROM Customers");
            cmbCustomers.DisplayMember = "Name";
            cmbCustomers.ValueMember = "CustomerId";

            cmbProducts.DataSource = DatabaseContext.ExecuteQuery("SELECT ProductId, Name, Price, StockQuantity FROM Products");
            cmbProducts.DisplayMember = "Name";
            cmbProducts.ValueMember = "ProductId";
        }
        private void UpdateProductInfo()
        {
            if (cmbProducts.SelectedItem is DataRowView row)
            {
                int pId = (int)row["ProductId"];
                decimal price = (decimal)row["Price"];
                int realStock = (int)row["StockQuantity"];

                int reservedInCart = GetCurrentStockInCart(pId);
                int available = realStock - reservedInCart;

                lblInfo.Text = $"Ціна: {price} грн | На складі: {realStock} шт. | Доступно: {available} шт.";

                if (available <= 0) lblInfo.ForeColor = System.Drawing.Color.Red;
                else lblInfo.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void cmbProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProductInfo();
        }
        private int GetCurrentStockInCart(int productId)
        {
            int total = 0;
            foreach (DataRow row in orderItemsTable.Rows)
            {
                if ((int)row["ProductId"] == productId)
                {
                    total += (int)row["Quantity"];
                }
            }
            return total;
        }

        private void btnAddLine_Click(object sender, EventArgs e)
        {
            if (cmbProducts.SelectedItem is DataRowView row)
            {
                int pId = (int)row["ProductId"];
                string pName = row["Name"].ToString();
                decimal price = (decimal)row["Price"];
                int realStock = (int)row["StockQuantity"];
                int qtyToAdd = (int)numQuantity.Value;

                if (qtyToAdd <= 0)
                {
                    MessageBox.Show("Кількість має бути більше 0");
                    return;
                }

                int alreadyInCart = GetCurrentStockInCart(pId);

                if (alreadyInCart + qtyToAdd > realStock)
                {
                    MessageBox.Show($"Недостатньо товару!\n" +
                                    $"На складі: {realStock}\n" +
                                    $"У кошику вже: {alreadyInCart}\n" +
                                    $"Ви намагаєтесь додати ще: {qtyToAdd}",
                                    "Помилка складу", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal sum = price * qtyToAdd;

                orderItemsTable.Rows.Add(pId, pName, price, qtyToAdd, sum);

                totalAmount += sum;
                UpdateTotalLabel();
                UpdateProductInfo();
            }
            else
            {
                MessageBox.Show("Оберіть товар!");
            }
        }
        private void UpdateTotalLabel()
        {
            decimal discount = _orderService.CalculateDiscount(totalAmount);
            decimal finalSum = _orderService.CalculateFinalSum(totalAmount);

            lblTotal.Text = $"Сума: {totalAmount} грн\nЗнижка: {discount}%\nДО СПЛАТИ: {finalSum} грн";
        }

        private decimal CalculateDiscount(decimal currentTotal)
        {
            if (currentTotal >= 50000) return 10.0m;
            if (currentTotal >= 10000) return 5.0m;
            return 0.0m;
        }

        private void btnSaveOrder_Click(object sender, EventArgs e)
        {
            if (orderItemsTable.Rows.Count == 0) { MessageBox.Show("Кошик порожній!"); return; }

            try
            {
                decimal discount = _orderService.CalculateDiscount(totalAmount);
                int customerId = (int)cmbCustomers.SelectedValue;

                _orderRepository.CreateOrder(customerId, UserSession.EmployeeId, totalAmount, discount, orderItemsTable);

                MessageBox.Show("Замовлення успішно створено!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при створенні замовлення: " + ex.Message);
            }
        }

        private void btnRemoveLine_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count > 0)
            {
                DataGridViewRow dgvRow = dgvItems.SelectedRows[0];
                DataRowView dataRowView = (DataRowView)dgvRow.DataBoundItem;
                DataRow row = dataRowView.Row;

                decimal rowSum = (decimal)row["Sum"];
                totalAmount -= rowSum;

                row.Delete();

                UpdateTotalLabel();
                UpdateProductInfo();
            }
            else
            {
                MessageBox.Show("Оберіть рядок у таблиці, який треба видалити.");
            }
        }
    }
}
