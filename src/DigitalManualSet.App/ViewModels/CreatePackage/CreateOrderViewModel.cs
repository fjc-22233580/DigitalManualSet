using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the "Create Order" step of the create-package workflow.
/// This is a minimal placeholder that exposes a display <see cref="Title"/>.
/// </summary>
public class CreateOrderViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    private readonly CreateOrderStep _step;

    public CreateOrderViewModel(IWorkflowStep<PackageWorkflowStepId> step)
    {
        _step = step as CreateOrderStep ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));

        OrdersView = CollectionViewSource.GetDefaultView(_step.AvailableOrders);
        OrdersView.Filter = FilterOrder;

        ClearCommand = new RelayCommand(ClearSelection, CanClearSelection);
        CreateOrderCommand = new RelayCommand(CreateOrder);
    }

    private void CreateOrder()
    {
        IsManualEntry = !IsManualEntry;
    }


    public RelayCommand ClearCommand { get; }
    public RelayCommand CreateOrderCommand { get; }

    /// <summary>
    /// Gets or sets the text used to filter the available orders.
    /// </summary>
    public string FilterText
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            SelectedOrder = null;
            OrdersView.Refresh();
            OnPropertyChanged(nameof(HasNoResults));

            ClearCommand.RaiseCanExecuteChanged();
        }
    } = string.Empty;



    public Order? SelectedOrder
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();

            SetOrder(value);

            ClearCommand.RaiseCanExecuteChanged();

        }
    }

    private void SetOrder(Order? value)
    {
        if (value is null)
        {
            _step.ClearOrder();
        }
        else
        {
            _step.SetOrder(value);
        }

        Debug.WriteLine(value is null ? "*********Selected order cleared" : $"*********Selected order: {value.CustomerName}");

    }


    public string OrderNumber
    {
        get;
        set
        {
            if (value == field) return;
            field = value;

            TryCompleteManualEntry();
        }
    }

    public string CustomerName
    {
        get;
        set
        {
            if (value == field) return;
            field = value;

            TryCompleteManualEntry();
        }
    }

    public string SystemId
    {
        get;
        set
        {
            if (value == field) return;
            field = value;

            TryCompleteManualEntry();
        }
    }

    private void TryCompleteManualEntry()
    {
        Order order = null;

        if (ValidateManualOrder() && ValidateOrderNumber())
        {
            order = new Order(OrderNumber, CustomerName, SystemId);
        }
        
        SetOrder(order);
    }

    private bool ValidateManualOrder()
    {
        if (string.IsNullOrWhiteSpace(CustomerName) ||
            string.IsNullOrWhiteSpace(OrderNumber) ||
            string.IsNullOrWhiteSpace(SystemId))
        {
            ValidationMessage = "Please complete all manual order fields.";
            return false;
        }

        ValidationMessage = string.Empty;
        return true;
    }

    private bool ValidateOrderNumber()
    {
        ValidationMessage = string.Empty;
        
        if (Regex.IsMatch(OrderNumber, @"^[A-Za-z]\d{6}$") == false)
        {
            ValidationMessage = "Order number must be in the format A123456.";
            return false;
        }

        return true;
    }

    public string ValidationMessage
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// Gets the filtered view of the available orders.
    /// </summary>
    public ICollectionView OrdersView { get; }

    public bool HasNoResults => OrdersView.IsEmpty;

    public bool IsManualEntry
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
        }
    }


    /// <summary>
    /// Gets the display title for the workflow step.
    /// </summary>
    public string Title => "Placeholder: select order.";


    private bool CanClearSelection()
    {
        return string.IsNullOrWhiteSpace(FilterText) == false || SelectedOrder is not null;
    }

    private void ClearSelection()
    {
        FilterText = string.Empty;
        SelectedOrder = null;
        OrdersView.Refresh();
    }

    private bool FilterOrder(object item)
    {
        if (item is not Order order)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(FilterText))
        {
            return true;
        }

        return order.OrderNumber.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
               || order.CustomerName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
               || order.SystemId.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
    }
}
