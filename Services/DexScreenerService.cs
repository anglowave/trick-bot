using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trick.Models;

namespace Trick.Services;

public class DexScreenerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DexScreenerService> _logger;

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
}
