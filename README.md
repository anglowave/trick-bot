# Trick Twitch Bot

like Rick, but on Twitch.

## Features

- **Token Detection**: Automatically detects Solana, Ethereum/BSC, and symbol-based tokens
- **Real-time Data**: Fetches live market data from DexScreener API
- **Call Tracking**: Stores token calls in MongoDB for PnL tracking
- **Rich Formatting**: Clean, emoji-rich token information display

## Supported Token Formats

- **Solana**: `$GKbUiHTjJ4DxzL4LX9P4evhtsPsBHgUxtr4Da8HMpump`
- **Ethereum/BSC**: `$0x8577cf66262d8d7bbc713f8c96fdf5871baf4444`
- **Symbols**: `$BTC`, `#ETH`

## Commands

- `$hello` - Greeting command
- `$ping` - Ping command
- `$pnl <token>` - Shows PnL for your latest call

## Setup

1. **Configure Twitch credentials** in `appsettings.json`:
   ```json
   {
     "TwitchBot": {
       "BotUsername": "your_bot_username",
       "OAuthToken": "oauth:your_oauth_token",
       "ChannelName": "your_channel_name"
     }
   }
   ```

2. **Set up MongoDB** (local or Atlas)

3. **Run the bot**:
   ```bash
   dotnet restore
   dotnet run
   ```

## Example Output

```
🚀 Friday Night [$78.5K/+2.8%]
FRIDAY/SOL 📈

🔗 SOLANA @ pumpswap
💰 USD: $0.00007852
💎 FDV: $78.5K
💧 Liq: $31.3K
📊 Vol: $77.5K
⏰ Age: 2m
📈 24H: +2.8%
📈 1H: +2.8%

🔗 Contract: 14q7Wz9gNoezKn8Gwj5GwgNiZqeYdg3AySYgz8uqpump
🗓️ Updated: 09:02 UTC
```

## Tech Stack

- **.NET 8**
- **TwitchLib.Client**
- **MongoDB.Driver**
- **DexScreener API**
- **Dependency Injection**

