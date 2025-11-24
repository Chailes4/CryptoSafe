using CryptoSafe.Services;
using CryptoSafe.Models;
using System.ComponentModel;
using CommunityToolkit.Maui.Views;
namespace CryptoSafe.Views;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    private readonly UserService _userService;
    public List<CryptoToken> Tokens { get; set; }
    public decimal USDTBalance { get; private set; }

    private decimal _totalUSDTValue;
    public decimal TotalUSDTValue
    {
        get => _totalUSDTValue;
        set
        {
            _totalUSDTValue = value;
            OnPropertyChanged(nameof(TotalUSDTValue));
        }
    }
    private decimal _totalPesoValue;
    public decimal TotalPesoValue
    {
        get => _totalPesoValue;
        set
        {
            _totalPesoValue = value;
            OnPropertyChanged(nameof(TotalPesoValue));
        }
    }

    public HomePage()
    {
        InitializeComponent();
        _userService = new UserService();
        BindingContext = this;
        USDTBalance = 0;
        TotalUSDTValue = 0;

        // Load tokens automatically when the page is initialized
        LoadTokensAsync(Guid.Parse(App.CurrentUserId));
    }

    private async Task LoadTokensAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            Console.WriteLine("Invalid user ID");
            return;
        }

        // Fetch tokens for the current user from the database
        Tokens = await _userService.GetUserTokensAsync(userId);

        // Check if no tokens exist and add default tokens if necessary
        if (Tokens == null || Tokens.Count == 0)
        {
            await _userService.AddDefaultTokensToUserAsync(userId);
            Tokens = await _userService.GetUserTokensAsync(userId);
        }

        // Calculate the total USDT equivalent of all tokens
        CalculateTotalUSDTValue();

        // Bind the tokens to the CollectionView
        TokenCollectionView.ItemsSource = Tokens;
    }

    private async void OnTokenSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CryptoToken selectedToken)
        {
            // Navigate to TokenDetailPage, passing the selected token as a parameter
            await Navigation.PushAsync(new TokenDetailPage(selectedToken));

            // Clear selection
            TokenCollectionView.SelectedItem = null;
        }
    }

    private void CalculateTotalUSDTValue()
    {
        if (Tokens == null || Tokens.Count == 0)
        {
            TotalUSDTValue = 0;
            return;
        }

        TotalUSDTValue = 0;
        decimal conversionRateUSDTToPHP = 59.1m;

        foreach (var token in Tokens)
        {
            decimal conversionRateToUSDT = _userService.GetConversionRateToUSDT(token.TokenName);
            decimal usdtValue = token.Amount * conversionRateToUSDT;

            token.USDTValue = usdtValue;
            TotalUSDTValue += usdtValue;

            TotalPesoValue = TotalUSDTValue * conversionRateUSDTToPHP;

        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Refresh token data when refresh button is clicked
    private async void OnRefreshButtonClicked(object sender, EventArgs e)
    {
        await LoadTokensAsync(Guid.Parse(App.CurrentUserId));
    }

    private async void OnSendReceiveButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SendTokenPage());

    }

    private async void OnTransactionClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TransactionPage(_userService));
    }
    private async void OnReceiveButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ReceiveTokenPage());
    }

    public async void UpdateUSDTBalance(decimal addedAmount)
    {
        var userId = Guid.Parse(App.CurrentUserId);
        var userTokens = await _userService.GetUserTokensAsync(userId);
        var usdtToken = userTokens.FirstOrDefault(t => t.TokenName == "USDT");

        if (usdtToken != null)
        {
            usdtToken.Amount += addedAmount;

            // Update the token in the database
            await _userService.UpdateTokenBalanceAsync(usdtToken);

            // Refresh UI and recalculate totals
            TokenCollectionView.ItemsSource = null;
            TokenCollectionView.ItemsSource = userTokens;
            Tokens = userTokens;

            // Recalculate totals after updating the token amount
            CalculateTotalUSDTValue();
        }
    }

    // Command for navigating to Home
    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage(_userService));
    }

}