using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using Trick.Models;

namespace Trick.Services;

public class DexScreenerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DexScreenerService> _logger;
    
    private readonly Regex _solanaTokenRegex = new Regex(@"\$([A-Za-z0-9]{32,44})", RegexOptions.IgnoreCase);
    private readonly Regex _ethBscTokenRegex = new Regex(@"\$([0x][A-Za-z0-9]{40})", RegexOptions.IgnoreCase);
    private readonly Regex _symbolTokenRegex = new Regex(@"\$([A-Z]{2,10})|#([A-Z]{2,10})", RegexOptions.IgnoreCase);

    public DexScreenerService(HttpClient httpClient, ILogger<DexScreenerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal?> GetMarketCapAsync(string tokenAddress)
    {
        try
        {
            var solanaData = await GetTokenDataAsync("solana", tokenAddress);
            if (solanaData?.Pairs?.Any() == true)
            {
                var bestPair = GetBestPair(solanaData.Pairs);
                if (bestPair != null)
                {
                    _logger.LogInformation($"Found Solana token {tokenAddress} with market cap: ${bestPair.MarketCap:N2}");
                    return bestPair.MarketCap;
                }
            }

            var ethData = await GetTokenDataAsync("ethereum", tokenAddress);
            if (ethData?.Pairs?.Any() == true)
            {
                var bestPair = GetBestPair(ethData.Pairs);
                if (bestPair != null)
                {
                    _logger.LogInformation($"Found Ethereum token {tokenAddress} with market cap: ${bestPair.MarketCap:N2}");
                    return bestPair.MarketCap;
                }
            }

            var bscData = await GetTokenDataAsync("bsc", tokenAddress);
            if (bscData?.Pairs?.Any() == true)
            {
                var bestPair = GetBestPair(bscData.Pairs);
                if (bestPair != null)
                {
                    _logger.LogInformation($"Found BSC token {tokenAddress} with market cap: ${bestPair.MarketCap:N2}");
                    return bestPair.MarketCap;
                }
            }

            _logger.LogWarning($"No market cap data found for token {tokenAddress}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching market cap for token {tokenAddress}");
            return null;
        }
    }

    private async Task<DexScreenerResponse?> GetTokenDataAsync(string chainId, string tokenAddress)
    {
        try
        {
            var url = $"https://api.dexscreener.com/token-pairs/v1/{chainId}/{tokenAddress}";
            _logger.LogDebug($"Fetching data from: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"API Response: {jsonContent}");
            
            try
            {
                var arrayData = JsonSerializer.Deserialize<List<TokenPair>>(jsonContent);
                return new DexScreenerResponse { Pairs = arrayData ?? new List<TokenPair>() };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to deserialize as array, trying object format");
                var data = JsonSerializer.Deserialize<DexScreenerResponse>(jsonContent);
                return data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to fetch data for {chainId}/{tokenAddress}");
            return null;
        }
    }

    private TokenPair? GetBestPair(List<TokenPair> pairs)
    {
        if (!pairs.Any()) return null;

        return pairs
            .Where(p => p.MarketCap > 0)
            .OrderByDescending(p => p.Liquidity.Usd)
            .ThenByDescending(p => p.Volume.H24)
            .FirstOrDefault();
    }

    public async Task<TokenPair?> GetTokenInfoAsync(string tokenAddress)
    {
        try
        {
            var solanaData = await GetTokenDataAsync("solana", tokenAddress);
            if (solanaData?.Pairs?.Any() == true)
            {
                return GetBestPair(solanaData.Pairs);
            }

            var ethData = await GetTokenDataAsync("ethereum", tokenAddress);
            if (ethData?.Pairs?.Any() == true)
            {
                return GetBestPair(ethData.Pairs);
            }

            var bscData = await GetTokenDataAsync("bsc", tokenAddress);
            if (bscData?.Pairs?.Any() == true)
            {
                return GetBestPair(bscData.Pairs);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching token info for {tokenAddress}");
            return null;
        }
    }

    public async Task<bool> DexPaid(string token)
    {
        try
        {
            string chainId = DetermineChainId(token);
            if (string.IsNullOrEmpty(chainId))
            {
                _logger.LogWarning($"Unable to determine chain for token: {token}");
                return false;
            }

            var url = $"https://api.dexscreener.com/orders/v1/{chainId}/{token}";
            _logger.LogDebug($"Checking payment status from: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Payment API Response: {jsonContent}");

            var orders = JsonSerializer.Deserialize<List<DexPaidOrder>>(jsonContent);
            if (orders == null || !orders.Any())
            {
                _logger.LogInformation($"No payment orders found for token: {token}");
                return false;
            }

            var paidOrder = orders.FirstOrDefault(o => 
                o.Type.Equals("tokenProfile", StringComparison.OrdinalIgnoreCase) && 
                o.Status.Equals("approved", StringComparison.OrdinalIgnoreCase));

            bool isPaid = paidOrder != null;
            _logger.LogInformation($"Token {token} payment status: {(isPaid ? "PAID" : "NOT PAID")}");
            
            return isPaid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking payment status for token {token}");
            return false;
        }
    }

    public async Task<TokenPair?> SearchTokenAsync(string query)
    {
        try
        {
            var url = $"https://api.dexscreener.com/latest/dex/search?q={Uri.EscapeDataString(query)}";
            _logger.LogDebug($"Searching token from: {url}");

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Search API Response: {jsonContent}");

            var searchResponse = JsonSerializer.Deserialize<DexScreenerResponse>(jsonContent);
            if (searchResponse?.Pairs?.Any() == true)
            {
                // Return the first (best) result
                var bestPair = GetBestPair(searchResponse.Pairs);
                _logger.LogInformation($"Found token search result for '{query}': {bestPair?.BaseToken.Symbol}");
                return bestPair;
            }

            _logger.LogWarning($"No search results found for query: {query}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error searching for token: {query}");
            return null;
        }
    }

    private string DetermineChainId(string token)
    {
        if (_solanaTokenRegex.IsMatch($"${token}"))
        {
            return "solana";
        }

        if (_ethBscTokenRegex.IsMatch($"${token}"))
        {
            return "ethereum";
        }

        return string.Empty;
    }
}
