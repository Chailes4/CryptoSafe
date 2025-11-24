using CommunityToolkit.Maui.Views;
using CryptoSafe.Models;

namespace CryptoSafe.Views;

public partial class TransactionDetailPopup : Popup
{
    public TransactionDetailPopup(Transaction transaction)
    {
        InitializeComponent();

        BindingContext = transaction; 
    }

}
