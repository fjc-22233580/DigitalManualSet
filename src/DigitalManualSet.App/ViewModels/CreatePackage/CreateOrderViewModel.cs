using DigitalManualSet.App.Common;
using DigitalManualSet.App.ViewModels.CreatePackage.Interfaces;
using DigitalManualSet.Core.Orders;
using DigitalManualSet.Core.PackageCreation.Workflow;
using DigitalManualSet.Core.Workflow;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DigitalManualSet.App.ViewModels.CreatePackage;

/// <summary>
/// View model for the order selection stage of the package creation workflow.
/// Allows an existing order to be selected and filtered, or an order to be
/// entered manually when the required order is not available.
/// </summary>
public class CreateOrderViewModel : ViewModel, IPackageWorkflowStepViewModel
{
    private readonly CreateOrderStep _step;

    /// <summary>
    /// Initialises the view model for the supplied package workflow step.
    /// </summary>
    /// <param name="step">The workflow step responsible for storing the selected or manually created order.</param>
    /// <exception cref="System.ArgumentException">Expected a {nameof(CreateOrderStep)}. - step</exception>
    /// <exception cref="ArgumentException">Thrown when the supplied workflow step is not a <see cref="CreateOrderStep" />.</exception>
    public CreateOrderViewModel(CreateOrderStep step)
    {
        _step = step ?? throw new ArgumentException($"Expected a {nameof(CreateOrderStep)}.", nameof(step));

        OrdersView = CollectionViewSource.GetDefaultView(_step.AvailableOrders);
        OrdersView.Filter = FilterOrder;

        ClearCommand = new RelayCommand(ClearSelection, CanClearSelection);
        CreateOrderCommand = new RelayCommand(CreateOrder);
    }

    /// <summary>
    /// Gets the command used to clear the current filters and order selection.
    /// </summary>
    public RelayCommand ClearCommand { get; }

    /// <summary>
    /// Gets the command used to toggle manual order entry mode.
    /// </summary>
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

            // Clear any existing selection because the visible order set has changed.
            SelectedOrder = null;

            OrdersView.Refresh();
            OnPropertyChanged(nameof(HasNoResults));

            ClearCommand.RaiseCanExecuteChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Gets the System ID prefixes that can be used to narrow down the available orders.
    /// </summary>
    public IReadOnlyList<string> SystemIdPrefixOptions => SystemIdPrefixes.ValidPrefixes;

    /// <summary>
    /// Gets or sets the System ID prefix used to filter the available orders.
    /// When <see langword="null"/>, orders are not filtered by System ID.
    /// </summary>
    public string? SelectedSystemIdPrefix
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            // Clear any existing selection because the visible order set has changed.
            SelectedOrder = null;

            OrdersView.Refresh();
            OnPropertyChanged(nameof(HasNoResults));

            ClearCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Gets or sets the order currently selected from the available order list.
    /// Updating the selection also updates the underlying workflow step.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the order number entered when creating an order manually.
    /// </summary>
    public string OrderNumber
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            TryCompleteManualEntry();
        }
    }

    /// <summary>
    /// Gets or sets the customer name entered when creating an order manually.
    /// </summary>
    public string CustomerName
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            TryCompleteManualEntry();
        }
    }

    /// <summary>
    /// Gets or sets the System ID entered when creating an order manually.
    /// </summary>
    public string SystemId
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            TryCompleteManualEntry();
        }
    }

    /// <summary>
    /// Gets the filtered view of the available orders.
    /// </summary>
    public ICollectionView OrdersView { get; }

    /// <summary>
    /// Gets a value indicating whether the current order filters return no results.
    /// </summary>
    public bool HasNoResults => OrdersView.IsEmpty;

    /// <summary>
    /// Gets or sets whether the user is currently entering an order manually.
    /// </summary>
    public bool IsManualEntry
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the current validation message for manual order entry.
    /// An empty value indicates that no validation error is currently present.
    /// </summary>
    public string ValidationMessage
    {
        get;
        set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the display title for this workflow step.
    /// </summary>
    public string Title => "Placeholder: select order.";

    /// <summary>
    /// Toggles between selecting an existing order and entering an order manually.
    /// </summary>
    private void CreateOrder()
    {
        IsManualEntry = !IsManualEntry;
    }

    /// <summary>
    /// Updates the order stored by the underlying workflow step.
    /// Passing <see langword="null"/> clears the current workflow order.
    /// </summary>
    /// <param name="value">The order to assign, or <see langword="null"/> to clear it.</param>
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

    /// <summary>
    /// Attempts to construct a manual order whenever one of the manual-entry
    /// fields changes. The workflow order is only set when all validation passes.
    /// </summary>
    private void TryCompleteManualEntry()
    {
        Order? order = null;

        if (ValidateManualOrder() && ValidateOrderNumber())
        {
            order = new Order(OrderNumber, CustomerName, SystemId);
        }

        SetOrder(order);
    }

    /// <summary>
    /// Validates that all fields required for manual order creation have been completed.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when all required fields contain values;
    /// otherwise <see langword="false"/>.
    /// </returns>
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

    /// <summary>
    /// Validates that the manually entered order number uses the expected
    /// format of one letter followed by six digits, for example A123456.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the order number is valid;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private bool ValidateOrderNumber()
    {
        ValidationMessage = string.Empty;

        if (!Regex.IsMatch(OrderNumber, @"^[A-Za-z]\d{6}$"))
        {
            ValidationMessage = "Order number must be in the format A123456.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether there is currently a filter or selection that can be cleared.
    /// </summary>
    private bool CanClearSelection()
    {
        return !string.IsNullOrWhiteSpace(FilterText)
               || SelectedSystemIdPrefix is not null
               || SelectedOrder is not null;
    }

    /// <summary>
    /// Clears the current order selection and resets all order-list filters.
    /// </summary>
    private void ClearSelection()
    {
        FilterText = string.Empty;
        SelectedSystemIdPrefix = null;
        SelectedOrder = null;

        OrdersView.Refresh();
    }

    /// <summary>
    /// Determines whether an order should be included in the filtered order view.
    /// Filtering can be performed by System ID prefix and free-text matching against
    /// the order number, customer name, or System ID.
    /// </summary>
    /// <param name="item">The item being evaluated by the collection view.</param>
    /// <returns>
    /// <see langword="true"/> when the order matches the active filters;
    /// otherwise <see langword="false"/>.
    /// </returns>
    private bool FilterOrder(object item)
    {
        if (item is not Order order)
        {
            return false;
        }

        if (SelectedSystemIdPrefix is not null && !string.Equals(SystemIdPrefixes.GetPrefix(order.SystemId), SelectedSystemIdPrefix, StringComparison.OrdinalIgnoreCase))
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