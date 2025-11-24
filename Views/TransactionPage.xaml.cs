using CryptoSafe.Services;
using CryptoSafe.Models;
using CommunityToolkit.Maui.Views;

namespace CryptoSafe.Views;

public partial class TransactionPage : ContentPage
{
    private readonly UserService _userService;
    public List<Transaction> Transactions { get; set; }

    public TransactionPage(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
        LoadTransactionsAsync(Guid.Parse(App.CurrentUserId));
    }

    private async Task LoadTransactionsAsync(Guid userId)
    {
        Transactions = await _userService.GetTransactionsAsync(userId);
        TransactionCollectionView.ItemsSource = Transactions;
    }
    private async void OnTransactionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            var selectedTransaction = e.CurrentSelection[0] as Transaction;

            if (selectedTransaction != null)
            {
                // Show the TransactionDetailPopup and pass the selected transaction as a parameter
                var transactionPopup = new TransactionDetailPopup(selectedTransaction);
                await this.ShowPopupAsync(transactionPopup);


                // Deselect the item after showing the popup
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
