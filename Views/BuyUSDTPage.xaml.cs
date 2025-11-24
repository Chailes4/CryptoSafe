using CommunityToolkit.Maui.Views;
using CryptoSafe.Services;
namespace CryptoSafe.Views;

public partial class BuyUSDTPage : ContentPage
{
        private const decimal PesoToUSDTConversionRate = 59.1m;
        private bool isPesoChanging = false;
        private bool isUSDTChanging = false;
        private UserService _userService;

        public BuyUSDTPage()
        {
            InitializeComponent();
            _userService = new UserService();

            // Add event handlers for both the Peso and USDT Entry fields
            PesoAmountEntry.TextChanged += OnPesoAmountChanged;
            USDTEntry.TextChanged += OnUSDTAmountChanged;
        }

        // Event handler for Peso Entry change
        private void OnPesoAmountChanged(object sender, TextChangedEventArgs e)
        {
            if (isUSDTChanging) return; // Prevent recursion if USDT is changing

            if (decimal.TryParse(PesoAmountEntry.Text, out decimal pesoAmount))
            {
                isPesoChanging = true;
                // Calculate USDT based on Pesos entered
                decimal usdtAmount = pesoAmount / PesoToUSDTConversionRate;
                USDTEntry.Text = usdtAmount.ToString("N3");
                isPesoChanging = false;
            }
        }

        // Event handler for USDT Entry change
        private void OnUSDTAmountChanged(object sender, TextChangedEventArgs e)
        {
            if (isPesoChanging) return; // Prevent recursion if Peso is changing

            if (decimal.TryParse(USDTEntry.Text, out decimal usdtAmount))
            {
                isUSDTChanging = true;
                // Calculate Pesos based on USDT entered
                decimal pesoAmount = usdtAmount * PesoToUSDTConversionRate;
                PesoAmountEntry.Text = pesoAmount.ToString("N3");
                isUSDTChanging = false;
            }
        }

        // When the user clicks "Buy USDT"
        private async void OnBuyUSDTButtonClicked(object sender, EventArgs e)
        {
            if (decimal.TryParse(PesoAmountEntry.Text, out decimal pesoAmount))
            {
                decimal usdtAmount = pesoAmount / PesoToUSDTConversionRate;

                // Ensure the user is informed of the USDT they'll receive
                USDTEntry.Text = usdtAmount.ToString("N3");

                if (usdtAmount < 1)
                {
                    MinAmountLabel.IsVisible = true;
                    return;
                }


                // Show confirmation dialog before proceeding
                bool confirmPurchase = await DisplayAlert("Confirm Purchase",
                    $"You are about to buy {usdtAmount:N3} USDT for {pesoAmount:N3} Pesos.\nDo you want to proceed?",
                    "Proceed", "Cancel");

                // If the user confirms the purchase
                if (confirmPurchase)
                {
                    // Process the purchase via the UserService
                    var userId = Guid.Parse(App.CurrentUserId); // Assuming CurrentUserId holds the logged-in user's ID
                    bool success = await _userService.BuyUSDTAsync(userId, pesoAmount);

                    if (success)
                    {
                        var popup = new PaymentSuccessPage(usdtAmount);
                        await Application.Current.MainPage.ShowPopupAsync(popup);

                        USDTEntry.Text = string.Empty;
                        PesoAmountEntry.Text = string.Empty;

                }
                else
                    {
                        await DisplayAlert("Error", "Failed to process your purchase.", "OK");
                    }
                }
                else
                {
                    // User canceled the purchase
                    await DisplayAlert("Purchase Canceled", "Your USDT purchase was canceled.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please enter a valid amount in Pesos.", "OK");
            }
        }

}