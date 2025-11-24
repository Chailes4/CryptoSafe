using CommunityToolkit.Maui.Views;

namespace CryptoSafe.Views;

public partial class PaymentSuccessPage : Popup
{
    private decimal _usdtAmount;

    public PaymentSuccessPage(decimal usdtAmount)
    {
        InitializeComponent();
        _usdtAmount = usdtAmount;

        // Display the USDT amount on the success page
        USDTAmountLabel.Text = $"You have purchased {_usdtAmount:N2} USDT.";
    }

    private async void OnContinueButtonClicked(object sender, EventArgs e)
    {
        // First, navigate to HomePage
        await Shell.Current.GoToAsync("//HomePage");
        Close();

        // After navigation, retrieve HomePage and update the USDT balance
        if (Shell.Current.CurrentPage is HomePage homePage)
        {
            homePage.UpdateUSDTBalance(_usdtAmount);
        }
    }
}