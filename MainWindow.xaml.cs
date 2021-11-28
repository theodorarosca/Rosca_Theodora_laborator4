using AutoLotModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Roșca_Theodora_labor4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
     enum ActionState
    {
      New,
      Edit,
      Delete,
      Nothing
    }

    public partial class MainWindow : Window
    {
        ActionState action = ActionState.Nothing;
        AutoLotModel.AutoLotEntitiesModel ctx = new AutoLotModel.AutoLotEntitiesModel();
        CollectionViewSource customerOrdersVSource;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            customerOrdersVSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("customerOrdersViewSource")));
            //customerOrdersVSource.Source = ctx.Customers.Local;
            ctx.Orders.Load();
            ctx.Inventories.Load();

            cmbCustomers.ItemsSource = ctx.Customers.Local;
           // cmbCustomers.DisplayMemberPath = "FirstName";
            cmbCustomers.SelectedValuePath = "CustId";

            cmbInventory.ItemsSource = ctx.Inventories.Local;
            //cmbInventory.DisplayMemberPath = "Make";
            cmbInventory.SelectedValuePath = "CarId";
            BindDataGrid();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.New;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.Edit;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            action = ActionState.Delete;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            customerOrdersVSource.View.MoveCurrentToPrevious();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            customerOrdersVSource.View.MoveCurrentToNext();
        }

        private void SaveCustomers()
        {
            AutoLotModel.Customer customer = null;
            if(action==ActionState.New)
            {
                try
                {
                    customer = new Costumer();
                    {
                        string FirstName = firstNameTextBox.Text.Trim();
                        string LastName = lastNameTextBox.Text.Trim();
                    }
                    ctx.Customers.Add(customer);
                    customerOrdersVSource.View.Refresh();
                    ctx.SaveChanges();
                }
                catch (DataMisalignedException ex )
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                if(action==ActionState.Edit)
            {
                try
                {
                    customer = (Customer)customerDataGrid.SelectedItem;
                    customer.FirstName = firstNameTextBox.Text.Trim();
                    customer.LastName = lastNameTextBox.Text.Trim();
                    ctx.SaveChanges();
                }
                catch (DataMisalignedException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (action==ActionState.Delete)
            {
                try
                {
                    customer = (Customer)customerDataGrid.SelectedItem;
                    ctx.Customers.Remove(customer);
                    ctx.SaveChanges();
                }
                catch (DataMisalignedException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                customerOrdersVSource.View.Refresh();
            }

        }
        private void gbOperations_Click(object sender, RoutedEventArgs e)
        {
            Button SelectedButton = (Button)e.OriginalSource;
            Panel panel = (Panel)SelectedButton.Parent;
            foreach (Button B in panel.Children.OfType<Button>())
            {
                if (B != SelectedButton)
                    B.IsEnabled = false;

            }
            gbActions.IsEnabled = true;
        }
        private void ReInitialize()
        {
            Panel panel = gbOperations.Content as Panel;
            foreach (Button B in panel.Children.OfType<Button>())
            {
                B.IsEnabled = true;

            }
            gbActions.IsEnabled = false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ReInitialize();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            TabItem ti = tbCtrlAutoLot.SelectedItem as TabItem;
            switch (ti.Header)
            {
                case "Customers":
                    SaveCustomers();
                    break;
                case "Inventory":
                    SaveInventory();
                    break;
                case "Orders":
                    break;
             }
            ReInitialize();
         }

        private void SaveInventory()
        {
            throw new NotImplementedException();
        }

        private void SaveOrders()
        {
            Order order = null;
            if(action==ActionState.New)
            {
                try
                {
                    Customer customer = (Customer)cmbCustomers.SelectedItem;
                    Inventory inventory = (Inventory)cmbInventory.SelectedItem;
                    order = new Order()
                    {
                        CustId = customer.CustId,
                        CarId = inventory.CarId};
                    ctx.Orders.Add(order);
                    ctx.SaveChanges();
                    BindDataGrid();

                }
                catch (DataMisalignedException ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }
            else
                if(action==ActionState.Edit)
            {
                dynamic selectedOrder = ordersDataGrid.SelectedItem;
                try
                {
                    int curr_id = selectedOrder.OrderId;
                    var editedOrder = ctx.Orders.FirstOrDefault(s => s.OrderId == curr_id);
                    if(editedOrder!=null)
                    {
                        editedOrder.CustId = Int32.Parse(cmbCustomers.SelectedValue.ToString());
                        editedOrder.CarId=Convert.ToInt32(cmbInventory.SelectedValue.ToString());
                        ctx.SaveChanges();

                    }
                }
                catch (DataMisalignedException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                BindDataGrid();
                customerOrdersVSource.View.MoveCurrentTo(selectedOrder);
            }
              else if(action==ActionState.Delete)
            {
                try
                {
                    dynamic selectedOrder = ordersDataGrid.SelectedItem;
                    int curr_id = selectedOrder.OrderId;
                    var deletedOrder = ctx.Orders.FirstOrDefault(s => s.OrderId == curr_id);
                    if(deletedOrder!=null)
                    {
                        ctx.Orders.Remove(deletedOrder);
                        ctx.SaveChanges();
                        MessageBox.Show("Order Deleted Successfully", "Message");
                        BindDataGrid();
                    }

                }
                catch (DataMisalignedException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void BindDataGrid()
        {
            var queryOrder = from ord in ctx.Orders
                             join cust in ctx.Customers on ord.CustId equals
                             cust.CustId join inv in ctx.Inventories on ord.CarId
                             equals inv.CarId select new { ord.OrderId, ord.CarId, ord.CustId,
                                 cust.FirstName, inv.Make, inv.Color };
            customerOrdersVSource.Source = queryOrder.ToList();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

            System.Windows.Data.CollectionViewSource inventoryViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("inventoryViewSource")));
            // Load data by setting the CollectionViewSource.Source property:
            // inventoryViewSource.Source = [generic data source]
        }

       
    }

}
