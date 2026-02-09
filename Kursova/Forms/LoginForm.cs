using Kursova.Data;
using Kursova.Models;
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
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '*';
            this.Text = "Авторизація";
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введіть пошту та пароль!");
                return;
            }

            string query = "SELECT EmployeeId, FullName, Position FROM Employees WHERE Email = @Email AND Password = @Password";

            SqlParameter[] parameters = {
                new SqlParameter("@Email", email),
                new SqlParameter("@Password", password)
            };

            DataTable result = DatabaseContext.ExecuteQuery(query, parameters);

            if (result.Rows.Count > 0)
            {
                DataRow row = result.Rows[0];
                UserSession.EmployeeId = Convert.ToInt32(row["EmployeeId"]);
                UserSession.FullName = row["FullName"].ToString();
                UserSession.Position = row["Position"].ToString();

                MessageBox.Show($"Вітаємо, {UserSession.FullName}!");

                Form1 main = new Form1();
                this.Hide();
                main.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Невірний логін або пароль.");
            }
        }
    }
}
