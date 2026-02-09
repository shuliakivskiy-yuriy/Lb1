using Kursova.Models;
using Kursova.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kursova
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = $"Користувач: {UserSession.FullName} | Посада: {UserSession.Position}";

            ApplyUserPermissions();
        }
        private void ApplyUserPermissions()
        {
            customerToolStripMenuItem.Enabled = false;
            createOrdersToolStripMenuItem.Enabled = false;
            reportToolStripMenuItem.Enabled = false;
            shipmentToolStripMenuItem.Enabled = false;
            productToolStripMenuItem.Enabled = false;
            employeeslToolStripMenuItem.Enabled = false;

            string role = UserSession.Position?.Trim();

            switch (role)
            {
                case "Менеджер з продажу":
                    customerToolStripMenuItem.Enabled = true;
                    productToolStripMenuItem.Enabled = true;
                    createOrdersToolStripMenuItem.Enabled = true;
                    reportToolStripMenuItem.Enabled = true;
                    break;

                case "Комірник":
                    shipmentToolStripMenuItem.Enabled = true;
                    break;

                case "Адміністратор":
                    employeeslToolStripMenuItem.Enabled = true;
                    break;

                default:
                    MessageBox.Show("Увага: Для вашої посади права доступу не налаштовані.");
                    break;
            }
        }
        private void OpenChildForm(Form childForm)
        {
            this.Hide();
            childForm.ShowDialog();
            this.Show();
        }

        private void createOrdersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateOrderForm form = new CreateOrderForm();
            OpenChildForm(form);
        }

        private void reportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReportForm form = new ReportForm();
            OpenChildForm(form);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void shipmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShipmentForm form = new ShipmentForm();
            OpenChildForm(form);
        }

        private void customerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CustomersForm form = new CustomersForm();
            OpenChildForm(form);
        }

        private void productToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProductsForm form = new ProductsForm();
            OpenChildForm(form);
        }

        private void employeeslToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EmployeesForm form = new EmployeesForm();
            OpenChildForm(form);
        }
    }
}
