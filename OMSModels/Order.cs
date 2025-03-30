using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOMS.Model
{
    public class Order : INotifyPropertyChanged
    {
        private int _orderID;
        private int _salesPersonID;
        private int _customerID;
        private int _productID;
        private int _quantity;
        private DateTime _orderDate;
        public required int OrderID
        {
            get => _orderID;
            set => SetProperty(ref _orderID, value);
        }
        public int SalesPersonID
        {
            get => _salesPersonID;
            set => SetProperty(ref _salesPersonID, value);
        }
        public int CustomerID
        {
            get => _customerID;
            set => SetProperty(ref _customerID, value);
        }
        public int ProductID
        {
            get => _productID;
            set => SetProperty(ref _productID, value);
        }
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
        public DateTime OrderDate
        {
            get => _orderDate;
            set => SetProperty(ref _orderDate, value);
        }
        public DateTime? DeleteDate { get; set; }

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
    }
}
