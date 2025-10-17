using System.Text.Json.Serialization;

namespace Trick.Models;

public class DexScreenerResponse
{
    [JsonPropertyName("pairs")]
    public List<TokenPair> Pairs { get; set; } = new();
}

// For direct array responses
public class DexScreenerArrayResponse : List<TokenPair>
{
}

public class TokenPair
{
    [JsonPropertyName("chainId")]
    public string ChainId { get; set; } = string.Empty;

    [JsonPropertyName("dexId")]
    public string DexId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("pairAddress")]
    public string PairAddress { get; set; } = string.Empty;

    [JsonPropertyName("baseToken")]
    public Token BaseToken { get; set; } = new();

    [JsonPropertyName("quoteToken")]
    public Token QuoteToken { get; set; } = new();

    [JsonPropertyName("priceNative")]
    public string PriceNative { get; set; } = string.Empty;

    [JsonPropertyName("priceUsd")]
    public string PriceUsd { get; set; } = string.Empty;

    [JsonPropertyName("volume")]
    public VolumeData Volume { get; set; } = new();

    [JsonPropertyName("priceChange")]
    public PriceChangeData PriceChange { get; set; } = new();

    [JsonPropertyName("liquidity")]
    public LiquidityData Liquidity { get; set; } = new();

    [JsonPropertyName("fdv")]
    public decimal Fdv { get; set; }

    [JsonPropertyName("marketCap")]
    public decimal MarketCap { get; set; }

    [JsonPropertyName("pairCreatedAt")]
    public long PairCreatedAt { get; set; }

    [JsonPropertyName("info")]
    public TokenInfo Info { get; set; } = new();
}

public class Token
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
}

public class VolumeData
{
    [JsonPropertyName("h24")]
    public decimal H24 { get; set; }

    [JsonPropertyName("h6")]
    public decimal H6 { get; set; }

    [JsonPropertyName("h1")]
    public decimal H1 { get; set; }

    [JsonPropertyName("m5")]
    public decimal M5 { get; set; }
}

public class PriceChangeData
{
    [JsonPropertyName("h24")]
    public decimal H24 { get; set; }

    [JsonPropertyName("h6")]
    public decimal H6 { get; set; }

    [JsonPropertyName("h1")]
    public decimal H1 { get; set; }

    [JsonPropertyName("m5")]
    public decimal M5 { get; set; }
}

public class LiquidityData
{
    [JsonPropertyName("usd")]
    public decimal Usd { get; set; }

    [JsonPropertyName("base")]
    public decimal Base { get; set; }

    [JsonPropertyName("quote")]
    public decimal Quote { get; set; }
}

public class TokenInfo
{
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("header")]
    public string Header { get; set; } = string.Empty;

    [JsonPropertyName("openGraph")]
    public string OpenGraph { get; set; } = string.Empty;
}

public class DexPaidOrder
{
    [JsonPropertyName("chainId")]
    public string ChainId { get; set; } = string.Empty;

    [JsonPropertyName("tokenAddress")]
    public string TokenAddress { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("paymentTimestamp")]
    public long PaymentTimestamp { get; set; }
}