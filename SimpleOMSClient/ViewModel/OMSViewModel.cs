using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SimpleOMS.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using SimpleOMSClient.Clients;
using SimpleOMSClient.Helpers;
using System.Windows;
using System.Windows.Threading;


namespace SimpleOMS.ViewModel
{
    public class OMSViewModel : INotifyPropertyChanged, IDisposable
    {
        private ObservableCollection<Order> _orders;
        private Order _selectedOrder;
        private Product _selectedProduct;
        private ObservableCollection<Product> _products;
        private ObservableCollection<string> _messages;
        private string _statusText;
        private ObservableCollection<Customer> _customers;
        private bool _isBusy;
        private bool _isNotBusy = true;
        private string _messageToSend; 
        private Customer _selectedCustomer;
        private BroadcastServiceClient _broadcastServiceClient;
        private bool _editModeOn;
        private int _salesPersonID;
        private int _tabIndex;

        public bool EditModeOn
        {
            get => _editModeOn;
            set => SetProperty(ref _editModeOn, value);
        }
        private bool _editingOrder;
        public bool EditingOrder
        {
            get => _editingOrder;
            set => SetProperty(ref _editingOrder, value);
        }
        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                SetProperty(ref _tabIndex, value);
                if (value == 0)
                {
                    EditingOrder = true;
                }
                else if (value == 1)
                {
                    EditingOrder = false;
                }
            }
        }

        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if(value == null)
                {
                    return;
                }
                SetProperty(ref _selectedCustomer, value);
                LoadOrdersForCustomer();
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if(value == null)
                {
                    value = new Order { OrderID = -1, OrderDate = DateTime.MinValue, ProductID=-1, CustomerID=-1, Quantity=-1, SalesPersonID=-1 };
                }
                SetProperty(ref _selectedOrder, value);
                SelectedProduct = Products.FirstOrDefault(p => p.ProductID == value.ProductID);
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {                
                SetProperty(ref _selectedProduct, value);
            }
        }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public string MessageToSend
        {
            get => _messageToSend;
            set => SetProperty(ref _messageToSend, value);
        }


        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool IsNotBusy
        {
            get => _isNotBusy;
            set => SetProperty(ref _isNotBusy, value);
        }

        

        public ICommand EditCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SendMessageCommand { get; }

        private ObservableCollection<Employee> Employees = new ObservableCollection<Employee>();

        private readonly CrudServiceClient.CrudServiceClient _crudService = null;

        public OMSViewModel(CrudServiceClient.CrudServiceClient crudServiceClient, BroadcastServiceClient broadcastServiceClient)
        {
            _crudService = crudServiceClient;            
            _broadcastServiceClient = broadcastServiceClient;

            _broadcastServiceClient.OnMessageReceived += OnMessageReceived;


            Orders = new ObservableCollection<Order>();
            Products = new ObservableCollection<Product>();
            Messages = new ObservableCollection<string>();
            Customers = new ObservableCollection<Customer>();

            EditCommand = new RelayCommand(Edit);
            DeleteCommand = new RelayCommand(Delete);
            NewCommand = new RelayCommand(New);
            SaveCommand = new RelayCommand(Save);
            SendMessageCommand = new RelayCommand(SendMessage);

            Task.Factory.StartNew(async () => await LoadDataAndConnectServices());

        }

        private void OnMessageReceived(object? sender, string message)
        {
            Application.Current.Dispatcher.BeginInvoke(() => Messages.Add(message));
            Task.Factory.StartNew(() => MarkUserLoggedIn(message));
        }

        private async void LoginEmployeeAsRandomEmployee()
        {
            // fake login
            if (Employees.Any(e => !e.IsLoggedIn))
            {
                var employee = Employees.Where(e => !e.IsLoggedIn).PickRandom();
                employee.IsLoggedIn = true;
                StatusText = $"Connected as {employee.FirstName} {employee.LastName}";
                _salesPersonID = employee.EmployeeID;

                // connect to the broadcast service
                await _broadcastServiceClient.ConnectAsync(employee.FullName);

                // Tell Everyone I'm online
                //await _broadcastServiceClient.SendMessageAsync($"Employee ID:{employee.EmployeeID}: {employee.FirstName} {employee.LastName} is online.");
            }
        }

        private void MarkUserLoggedIn(string message)
        {
            if(message.EndsWith("logged-in"))
            {
                var employeeId = message.Replace(" logged-in","").Trim();
                var employee = Employees.FirstOrDefault(e => e.FullName == employeeId);
                if (employee != null)
                {
                    employee.IsLoggedIn = true;
                }
            }
        }

        private async Task LoadDataAndConnectServices()
        {
            IsBusy = true;
            try
            {
                var employees = await _crudService.EmployeesAsync();
                foreach (var employee in employees)
                {
                    Employees.Add(new Employee { EmployeeID = employee.EmployeeID, FirstName = employee.FirstName, LastName = employee.LastName, MiddleInitial = employee.MiddleInitial[0] });
                }

                LoginEmployeeAsRandomEmployee();

                Orders.Clear();

                var products = await _crudService.ProductsAsync();
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(new Product { Name = product.Name, ProductID = product.ProductID, Price = (float)product.Price});
                }
                var customers = await _crudService.CustomersAllAsync();
                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(new Customer { CustomerID = customer.CustomerID, FirstName = customer.FirstName, LastName =customer.LastName, MiddleInitial = customer.MiddleInitial[0] });
                }

                await Application.Current.Dispatcher.BeginInvoke(() => SelectedCustomer = Customers.FirstOrDefault());

            }
            finally
            {
                IsBusy = false;
            }
            EditingOrder = true;
        }
        private async void DeleteCustomer()
        {
            if(SelectedCustomer == null)
            {
                return;
            }
            await _crudService.CustomersDELETEAsync(SelectedCustomer.CustomerID);
            SelectedCustomer = Customers.FirstOrDefault();
        }

        private void Edit()
        {            
            if ((!EditingOrder && SelectedCustomer == null || (EditingOrder && SelectedOrder == null)))
            {
                MessageBox.Show(String.Format("Please select a {0} to edit.", (EditingOrder ? "order" : "customer")));
                return;
            }
            EditModeOn = true;
        }

        private void Delete()
        {
            if (EditingOrder)
            {
                if (SelectedOrder == null)
                {
                    MessageBox.Show("Please select an order to delete.");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this order?", "Delete Order", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    Orders.Remove(SelectedOrder);
                    _crudService.OrdersDELETEAsync(SelectedOrder.OrderID);
                }
            }
            else
            {
                if (SelectedCustomer == null)
                {                    
                    return;
                }
                if (Orders.Any(o => o.CustomerID == SelectedCustomer.CustomerID))
                {
                    MessageBox.Show("Cannot delete customer with orders.");
                    return;
                }

                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this customer?", "Delete Customer", MessageBoxButton.YesNo);
                if(result == MessageBoxResult.Yes)
                {
                    _crudService.CustomersDELETEAsync(SelectedCustomer.CustomerID);
                    Customers.Remove(SelectedCustomer);
                }
            }
        }

        private void New()
        {
            EditModeOn = true;
            if(EditingOrder)
            {
                var order = new Model.Order { OrderID = RandomHelper.GetRandomInt(1000, 99999), SalesPersonID = _salesPersonID, OrderDate = DateTime.Now };
                SelectedOrder = order;
            }
            else
            {
                var customer = new Customer { CustomerID = RandomHelper.GetRandomInt(1000, 99999), FirstName = "", LastName = "", MiddleInitial = 'X' };
                SelectedCustomer = customer;
            }
        }

        private async void Save()
        {            
            if (EditingOrder)
            {                
                if(SelectedOrder == null)
                {
                    return;
                }
                MessageBoxResult result = MessageBox.Show("Are you sure you want to save this order?", "Save Order", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
                if (!Orders.Contains(SelectedOrder))
                {
                   
                    await _crudService.OrdersPOSTAsync(
                    new CrudServiceClient.Order
                    {
                        OrderID = SelectedOrder.OrderID,
                        CustomerID = SelectedCustomer.CustomerID,
                        ProductID = SelectedProduct.ProductID,
                        Quantity = SelectedOrder.Quantity,
                        SalesPersonID = SelectedOrder.SalesPersonID,
                        OrderDate = SelectedOrder.OrderDate
                    });
                    await Application.Current.Dispatcher.BeginInvoke(() => Orders.Add(SelectedOrder));
                }
                else
                {
                    await _crudService.OrdersPUTAsync (
                        SelectedOrder.OrderID,
                    new CrudServiceClient.Order
                    {
                        OrderID = SelectedOrder.OrderID,
                        CustomerID = SelectedCustomer.CustomerID,
                        ProductID = SelectedProduct.ProductID,
                        Quantity = SelectedOrder.Quantity,
                        SalesPersonID = SelectedOrder.SalesPersonID,
                        OrderDate = SelectedOrder.OrderDate
                    });
                }
            }
            else
            {
                if (SelectedCustomer == null)
                {
                    return;
                }
                MessageBoxResult result = MessageBox.Show("Are you sure you want to save this customer?", "Save Customer", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                if (!Customers.Contains(SelectedCustomer))
                {
                    await _crudService.CustomersPOSTAsync(
                    new CrudServiceClient.Customer
                    {
                        CustomerID = SelectedCustomer.CustomerID,
                        FirstName = SelectedCustomer.FirstName,
                        LastName = SelectedCustomer.LastName,
                        MiddleInitial = SelectedCustomer.MiddleInitial.ToString()
                    });
                    await Application.Current.Dispatcher.BeginInvoke(() => Customers.Add(SelectedCustomer));
                }
                {
                    await _crudService.CustomersPUTAsync(
                        SelectedCustomer.CustomerID,
                        new CrudServiceClient.Customer
                        {
                            CustomerID = SelectedCustomer.CustomerID,
                            FirstName = SelectedCustomer.FirstName,
                            LastName = SelectedCustomer.LastName,
                            MiddleInitial = SelectedCustomer.MiddleInitial.ToString()
                        });
                }
            }
            EditModeOn = false;
        }

        private async void SendMessage()
        {
           await _broadcastServiceClient.SendMessageAsync(MessageToSend);
            MessageToSend = string.Empty;
        }

        private async void LoadOrdersForCustomer()
        {
            var orders = await _crudService.CustomerOrdersAsync(SelectedCustomer.CustomerID);

            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(new Order { OrderID = order.OrderID, CustomerID = order.CustomerID, Quantity = order.Quantity, ProductID = order.ProductID, SalesPersonID = order.SalesPersonID, OrderDate = order.OrderDate });
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null, Func<T, T, bool>? validateValue = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
            {
                return false;
            }

            if (validateValue != null && !validateValue(backingStore, value))
            {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
             Task.Factory.StartNew(async () => await _broadcastServiceClient.DisconnectAsync());
            _broadcastServiceClient.Dispose();
        }
    }
}
