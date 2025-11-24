using CommunityToolkit.Maui.Views;
using CryptoSafe.Services;

namespace CryptoSafe.Views;

public partial class SendTokenPage : ContentPage
{
    private UserService _userService;
    private List<string> _availableTokens = new List<string> { "USDT", "TON", "DOGS", "CATI", "BTC", "USDC", "SOL", "DOGE", "ETH", "BNB", "TRX" };
    private const decimal GasFeeInUSDT = 0.001m;
    private const decimal MinSendAmount = 0.01m;

    public SendTokenPage()
    {
        InitializeComponent();
        _userService = new UserService();

        // Populate the Picker with token options
        TokenPicker.ItemsSource = _availableTokens;
    }

    // Send tokens to another user
    private async void OnSendTokensClicked(object sender, EventArgs e)
    {
        var senderId = Guid.Parse(App.CurrentUserId);

        // Validate if the receiver ID is empty or invalid
        if (string.IsNullOrWhiteSpace(ReceiverIdEntry.Text) || !Guid.TryParse(ReceiverIdEntry.Text, out Guid receiverId))
        {
            await DisplayAlert("Error", "Please enter a valid receiver ID", "OK");
            return;
        }

        // Validate if a token is selected
        var tokenName = TokenPicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(tokenName))
        {
            await DisplayAlert("Error", "Please select a token", "OK");
            return;
        }

        // Validate the amount
        if (string.IsNullOrWhiteSpace(AmountEntry.Text) || !decimal.TryParse(AmountEntry.Text, out decimal amount))
        {
            await DisplayAlert("Error", "Please enter a valid amount", "OK");
            return;
        }

        // Check if the amount meets the minimum send requirement
        if (amount < MinSendAmount)
        {
            MinAmountLabel.IsVisible = true;
            return;
        }

        // Retrieve user balance for the selected token and USDT
        var userTokens = await _userService.GetUserTokensAsync(senderId);
        var userTokenBalance = userTokens.FirstOrDefault(t => t.TokenName == tokenName);
        var usdtBalance = userTokens.FirstOrDefault(t => t.TokenName == "USDT");

        // Check if the user has enough USDT for the gas fee
        if (usdtBalance == null || usdtBalance.Amount < GasFeeInUSDT)
        {
            await DisplayAlert("Error", "Insufficient USDT balance to cover the gas fee.", "OK");
            return;
        }

        // Check if the user has enough of the selected token for the transfer
        if (userTokenBalance == null || userTokenBalance.Amount < amount)
        {
            await DisplayAlert("Error", $"Insufficient {tokenName} balance for this transaction.", "OK");
            return;
        }

        // Display confirmation alert before proceeding
        bool isConfirmed = await DisplayAlert("Confirm Transaction",
                                $"You are about to send {amount:N2} {tokenName} to {receiverId}.\n" +
                                $"Gas Fee: {GasFeeInUSDT:N3} USDT \n\nDo you want to proceed?",
                                "Proceed",
                                "Cancel");

        if (!isConfirmed)
        {
            await DisplayAlert("Transaction Canceled", "You have successfully canceled the transaction.", "OK");
            return; // User canceled the transaction
        }

        // Deduct the gas fee from the USDT balance
        usdtBalance.Amount -= GasFeeInUSDT;

        // Deduct the amount from the token balance
        userTokenBalance.Amount -= amount;

        // Transfer tokens using the validated senderId and receiverId
        var success = await _userService.TransferTokensAsync(senderId, receiverId, tokenName, amount);

        if (success)
        {
            // Update the user's token balance and USDT balance in the database
            await _userService.UpdateTokenBalanceAsync(userTokenBalance);
            await _userService.UpdateTokenBalanceAsync(usdtBalance);

            await DisplayAlert("Success", "Tokens sent successfully!", "OK");

            // Clear the fields after success
            ReceiverIdEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
            TokenPicker.SelectedItem = null;
            MinAmountLabel.IsVisible = false; // Hide the label after success
        }
        else
        {
            await DisplayAlert("Error", "Failed to send tokens. Please try again.", "OK");
        }
    }
}