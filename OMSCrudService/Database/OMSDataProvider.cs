using Microsoft.Data.Sqlite;
using SimpleOMS.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMSCrudService.Database
{
    public class OMSDataProvider : IOMSDataProvider
    {
        private readonly string _connectionString;

        public OMSDataProvider(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                
                CREATE TABLE IF NOT EXISTS Customers (
                    CustomerID INTEGER PRIMARY KEY,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    MiddleInitial TEXT NOT NULL,
                    DeleteDate DATE NULL,
                    UNIQUE(FirstName, LastName, MiddleInitial)
                );

                CREATE TABLE IF NOT EXISTS Employees (
                    EmployeeID INTEGER PRIMARY KEY,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    MiddleInitial TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Products (
                    ProductID INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Price REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Orders (
                    OrderID INTEGER PRIMARY KEY,
                    SalesPersonID INTEGER NOT NULL,
                    CustomerID INTEGER NOT NULL,
                    ProductID INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    OrderDate DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    DeleteDate DATE NULL,
                    FOREIGN KEY(CustomerID) REFERENCES Customers(CustomerID),
                    FOREIGN KEY(SalesPersonID) REFERENCES Employees(EmployeeID),
                    FOREIGN KEY(ProductID) REFERENCES Products(ProductID)
                );

                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (1, 'John', 'Doe', 'D');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (2, 'Jane', 'Smith', 'S');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (3, 'Bob', 'Johnson', 'J');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (4, 'Alice', 'Williams', 'W');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (5, 'Charlie', 'Brown', 'B');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (6, 'David', 'Davis', 'D');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (7, 'Eve', 'Evans', 'E');
                INSERT OR IGNORE INTO Employees (EmployeeID, FirstName, LastName, MiddleInitial) VALUES (8, 'Frank', 'Franklin', 'F');

                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (1, 'Widget', 10.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (2, 'Gadget', 20.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (3, 'Thing', 30.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (4, 'Pen', 40.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (5, 'Pencil', 50.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (6, 'Eraser', 60.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (7, 'Sharpener', 70.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (8, 'Ruler', 80.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (9, 'Calculator', 90.00);
                INSERT OR IGNORE INTO Products (ProductID, Name, Price) VALUES (10, 'Notebook', 100.00);

                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (1, 'Arun', 'Anna', 'D');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (2, 'Bala', 'Balu', 'S');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (3, 'Chitra', 'Chandran', 'J');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (4, 'Dinesh', 'Dhawan', 'W');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (5, 'Esha', 'Eshwar', 'B');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (6, 'Feroz', 'Fernandes', 'D');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (7, 'Gita', 'Gopal', 'E');
                INSERT OR IGNORE INTO Customers (CustomerID, FirstName, LastName, MiddleInitial) VALUES (8, 'Hari', 'Harish', 'F');
                    
            ";
            command.ExecuteNonQuery();
        }

        public async Task<IEnumerable<Customer>> GetCustomers()
        {
            var customers = new List<Customer>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT CustomerID,FirstName,LastName,MiddleInitial,DeleteDate FROM Customers where DeleteDate is NULL";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    customers.Add(new Customer
                    {
                        CustomerID = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        MiddleInitial = reader.GetString(3)[0],
                        DeleteDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
                    });
                }
            }

            return customers;
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            var orders = new List<Order>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT OrderID, CustomerID,SalesPersonID, ProductID,Quantity,OrderDate,DeleteDate FROM Orders and DeleteDate is NULL";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new Order
                    {
                        OrderID = reader.GetInt32(0),
                        CustomerID = reader.GetInt32(1),
                        SalesPersonID = reader.GetInt32(2),
                        ProductID = reader.GetInt32(3),
                        Quantity = reader.GetInt32(4),
                        OrderDate = reader.GetDateTime(5),
                        DeleteDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                    });
                }
            }

            return orders;
        }

        public async Task<Customer> GetCustomer(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT CustomerID, FirstName, LastName, MiddleInitial FROM Customers WHERE CustomerID = $id";
                command.Parameters.AddWithValue("$id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Customer
                        {
                            CustomerID = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            MiddleInitial = reader.GetString(3)[0]
                        };
                    }
                }
            }        

            return null;
        }

        public async Task<Customer> CreateCustomer(Customer customer)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = 
                    @"INSERT INTO Customers (FirstName, LastName, MiddleInitial) VALUES ($firstName, $lastName, $middleInitial);
                      SELECT last_insert_rowid();";

                command.Parameters.AddWithValue("$firstName", customer.FirstName);
                command.Parameters.AddWithValue("$lastName", customer.LastName);
                command.Parameters.AddWithValue("$middleInitial", customer.MiddleInitial);

                customer.CustomerID = (int)(long)await command.ExecuteScalarAsync();
            }
            return customer;
        }

        public async Task<Customer> UpdateCustomer(int id, Customer customer)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE Customers SET FirstName = $firstName, LastName = $lastName, MiddleInitial = $middleInitial WHERE CustomerID = $id";

                command.Parameters.AddWithValue("$firstName", customer.FirstName);
                command.Parameters.AddWithValue("$lastName", customer.LastName);
                command.Parameters.AddWithValue("$middleInitial", customer.MiddleInitial);
                command.Parameters.AddWithValue("$id", id);

                await command.ExecuteNonQueryAsync();
            }

            return customer;
        }

        public async Task<Customer> DeleteCustomer(int id)
        {
            var customer = await GetCustomer(id);
            if (customer == null) return null;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Customers WHERE CustomerID = $id";
            command.Parameters.AddWithValue("$id", id);

            await command.ExecuteNonQueryAsync();
            return customer;
        }

        public async Task<Order> GetOrder(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT OrderID,SalesPersonID,CustomerID,Quantity,ProductID,OrderDate,DeleteDate FROM Orders WHERE OrderID = $id";
            command.Parameters.AddWithValue("$id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Order
                {
                    OrderID = reader.GetInt32(0),
                    SalesPersonID = reader.GetInt32(1),
                    CustomerID = reader.GetInt32(2),
                    Quantity = reader.GetInt32(3),
                    ProductID = reader.GetInt32(4),
                    OrderDate = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                    DeleteDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                };
            }

            return null;
        }

        public async Task<IEnumerable<Order>> GetCustomerOrders(int id)
        {
            var orders = new List<Order>();
            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT OrderID, CustomerID,SalesPersonID, ProductID,Quantity,OrderDate,DeleteDate FROM Orders WHERE CustomerID = $id and DeleteDate is NULL";
                command.Parameters.AddWithValue("$id", id);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new Order
                    {
                        OrderID = reader.GetInt32(0),
                        CustomerID = reader.GetInt32(1),
                        SalesPersonID = reader.GetInt32(2),
                        ProductID = reader.GetInt32(3),
                        Quantity = reader.GetInt32(4),
                        OrderDate = reader.GetDateTime(5),
                        DeleteDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                    });
                }
            }

            return orders;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = 
                @"INSERT INTO Orders (OrderID, SalesPersonID, CustomerID, ProductID, Quantity, OrderDate) VALUES ($orderID, $salesPersonID, $customerID, $productID, $quantity, $orderDate);
                SELECT last_insert_rowid();";
            command.Parameters.AddWithValue("$orderID", order.OrderID);
            command.Parameters.AddWithValue("$salesPersonID", order.SalesPersonID);
            command.Parameters.AddWithValue("$customerID", order.CustomerID);
            command.Parameters.AddWithValue("$productID", order.ProductID);
            command.Parameters.AddWithValue("$quantity", order.Quantity);
            command.Parameters.AddWithValue("$orderDate", order.OrderDate);

            order.OrderID = (int)(long)await command.ExecuteScalarAsync();
            return order;
        }

        public async Task<Order> UpdateOrder(int id, Order order)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Orders SET SalesPersonID = $salesPersonID, CustomerID = $customerID, ProductID = $productID, Quantity = $quantity, OrderDate = $orderDate WHERE OrderID = $id";
            command.Parameters.AddWithValue("$salesPersonID", order.SalesPersonID);
            command.Parameters.AddWithValue("$customerID", order.CustomerID);
            command.Parameters.AddWithValue("$productID", order.ProductID);
            command.Parameters.AddWithValue("$quantity", order.Quantity);
            command.Parameters.AddWithValue("$orderDate", order.OrderDate);
            command.Parameters.AddWithValue("$id", id);

            await command.ExecuteNonQueryAsync();
            return order;
        }

        public async Task<Order> DeleteOrder(int id)
        {
            var order = await GetOrder(id);
            if (order == null) return null;

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var deleteDate = DateTime.Now;
            var command = connection.CreateCommand();
            command.CommandText = @"UPDATE Orders SET DeleteDate = $deleteDate WHERE OrderID = $id;
                                    select OrderID from Orders where OrderId = $id;";

            command.Parameters.AddWithValue("$deleteDate", deleteDate);
            command.Parameters.AddWithValue("$id", id);
            await command.ExecuteNonQueryAsync();
            
            order.DeleteDate = deleteDate;

            return order;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT ProductID, Name, Price FROM Products";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    ProductID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetFloat(2)
                });
            }

            return products;
        }

        public async Task<IEnumerable<Employee>> GetEmployees()
        {
            var employees = new List<Employee>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT EmployeeID, FirstName, LastName, MiddleInitial FROM Employees";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    EmployeeID = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    MiddleInitial = reader.GetString(3)[0]
                });
            }

            return employees;
        }
    }
}
