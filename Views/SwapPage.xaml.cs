using CryptoSafe.Services;
using CryptoSafe.Models;

namespace CryptoSafe.Views
{
    public partial class SwapPage : ContentPage
    {
        private UserService _userService;
        private List<CryptoToken> _userTokens;
        private const decimal GasFeeInUSDT = 0.01m;
        private const decimal MinSwapAmount = 0.01m; // Minimum swap amount

        public SwapPage()
        {
            InitializeComponent();
            _userService = new UserService();

            // Populate the Picker with available token options
            TokenFromPicker.ItemsSource = new List<string> { "USDT", "TON", "DOGS", "CATI", "BTC", "USDC", "SOL", "DOGE", "ETH", "BNB", "TRX" };
            TokenToPicker.ItemsSource = new List<string> { "USDT", "TON", "DOGS", "CATI", "BTC", "USDC", "SOL", "DOGE", "ETH", "BNB", "TRX" };
        }

        private async void OnSwapButtonClicked(object sender, EventArgs e)
        {
            var userId = Guid.Parse(App.CurrentUserId);

            // Fetch selected tokens
            var tokenFrom = TokenFromPicker.SelectedItem?.ToString();
            var tokenTo = TokenToPicker.SelectedItem?.ToString();

            // Ensure different tokens are selected for the swap
            if (tokenFrom == tokenTo)
            {
                await DisplayAlert("Error", "Cannot convert the same token.", "OK");
                return;
            }

            // Validate the amount input
            if (string.IsNullOrWhiteSpace(SwapAmountEntry.Text) || !decimal.TryParse(SwapAmountEntry.Text, out decimal amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }

            // Check if the amount meets the minimum swap requirement
            if (amount < MinSwapAmount)
            {
                MinAmountLabel.IsVisible = true;
                return;
            }

            // Retrieve user tokens, including USDT balance for gas fee
            _userTokens = await _userService.GetUserTokensAsync(userId);
            var tokenFromBalance = _userTokens.FirstOrDefault(t => t.TokenName == tokenFrom);
            var usdtBalance = _userTokens.FirstOrDefault(t => t.TokenName == "USDT");

            // Check if the user has enough USDT for the gas fee
            if (usdtBalance == null || usdtBalance.Amount < GasFeeInUSDT)
            {
                await DisplayAlert("Error", "Insufficient USDT balance to cover the gas fee.", "OK");
                return;
            }

            // Check if user has enough of the selected "from" token
            if (tokenFromBalance == null || tokenFromBalance.Amount < amount)
            {
                await DisplayAlert("Error", $"Insufficient {tokenFrom} balance for this transaction.", "OK");
                return;
            }

            // Conversion rates in USDT
            var tokenFromPrice = tokenFromBalance.TokenPriceInUSDT;
            var tokenToPrice = _userTokens.FirstOrDefault(t => t.TokenName == tokenTo)?.TokenPriceInUSDT ?? 1.0m;

            // Calculate the equivalent token amount in the "to" token
            decimal amountInUSDT = amount * tokenFromPrice;
            decimal convertedAmount = amountInUSDT / tokenToPrice;

            // Display confirmation alert before proceeding
            bool confirmSwap = await DisplayAlert("Confirm Swap",
                $"You are about to swap {amount:N2} {tokenFrom} for {convertedAmount:N2} {tokenTo}.\n" +
                $"Gas Fee: {GasFeeInUSDT} USDT \n\nDo you want to proceed?", "Proceed", "Cancel");

            if (!confirmSwap)
            {
                await DisplayAlert("Transaction Canceled", "You have successfully canceled the transaction.", "OK");
                return;
            }

            // Deduct gas fee from USDT balance
            usdtBalance.Amount -= GasFeeInUSDT;

            // Deduct the swap amount from the "from" token
            tokenFromBalance.Amount -= amount;

            // Add the swapped amount to the "to" token balance
            var tokenToBalance = _userTokens.FirstOrDefault(t => t.TokenName == tokenTo);
            if (tokenToBalance != null)
            {
                tokenToBalance.Amount += convertedAmount;
                await _userService.UpdateTokenBalanceAsync(tokenToBalance); // Update token balance
            }
            else
            {
                // Add the new token if the user doesn't have it
                await _userService.AddCryptoTokenToUserAsync(userId, tokenTo, convertedAmount);
            }

            // Update the "from" token and USDT balances in the database
            await _userService.UpdateTokenBalanceAsync(tokenFromBalance);
            await _userService.UpdateTokenBalanceAsync(usdtBalance);

            await DisplayAlert("Success", $"You swapped {amount:N2} {tokenFrom} for {convertedAmount:N2} {tokenTo}. Gas fee of {GasFeeInUSDT} USDT has been deducted.", "OK");

            // Record the swap as a transaction
            await _userService.AddTransactionAsync(
                userId,
                tokenFrom,    // Token name being swapped from
                -amount,      // Negative amount indicates outgoing transaction
                tokenFromPrice,
                "Swap From"
            );

            await _userService.AddTransactionAsync(
                userId,
                tokenTo,      // Token name being swapped to
                convertedAmount, // Positive amount indicates incoming transaction
                tokenToPrice,
                "Swap To"
            );


            // Reset the form
            TokenFromPicker.SelectedItem = null;
            TokenToPicker.SelectedItem = null;
            SwapAmountEntry.Text = string.Empty;
            MinAmountLabel.IsVisible = false;
        }

        // Navigations
        private async void OnHomeTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage());
        }

        private async void OnTransactionTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransactionPage(_userService));
        }

        private async void OnSettingsTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage(_userService));
        }
    }
}
