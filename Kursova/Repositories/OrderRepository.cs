using Kursova.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kursova.Repositories
{
    public class OrderRepository
    {
        public void CreateOrder(int customerId, int employeeId, decimal totalAmount, decimal discountPercent, DataTable items)
        {
            using (SqlConnection conn = DatabaseContext.GetConnection())
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    decimal finalSum = totalAmount - (totalAmount * discountPercent / 100);

                    string sqlOrder = "INSERT INTO Orders (CustomerId, EmployeeId, OrderDate, RequiredDate, Status, TotalAmount) VALUES (@C, @E, @D, @ReqD, 'Новий', @T); SELECT SCOPE_IDENTITY();";
                    SqlCommand cmdOrder = new SqlCommand(sqlOrder, conn, transaction);
                    cmdOrder.Parameters.AddWithValue("@C", customerId);
                    cmdOrder.Parameters.AddWithValue("@E", employeeId);
                    cmdOrder.Parameters.AddWithValue("@D", DateTime.Now);
                    cmdOrder.Parameters.AddWithValue("@ReqD", DateTime.Now.AddDays(3));
                    cmdOrder.Parameters.AddWithValue("@T", finalSum);

                    int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                    foreach (DataRow row in items.Rows)
                    {
                        string sqlItem = "INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, Discount) VALUES (@O, @P, @Q, @Price, @Disc)";
                        SqlCommand cmdItem = new SqlCommand(sqlItem, conn, transaction);
                        cmdItem.Parameters.AddWithValue("@O", orderId);
                        cmdItem.Parameters.AddWithValue("@P", row["ProductId"]);
                        cmdItem.Parameters.AddWithValue("@Q", row["Quantity"]);
                        cmdItem.Parameters.AddWithValue("@Price", row["Price"]);
                        cmdItem.Parameters.AddWithValue("@Disc", discountPercent);
                        cmdItem.ExecuteNonQuery();

                        string sqlStock = "UPDATE Products SET StockQuantity = StockQuantity - @Q WHERE ProductId = @P";
                        SqlCommand cmdStock = new SqlCommand(sqlStock, conn, transaction);
                        cmdStock.Parameters.AddWithValue("@Q", row["Quantity"]);
                        cmdStock.Parameters.AddWithValue("@P", row["ProductId"]);
                        cmdStock.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
