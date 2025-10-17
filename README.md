# Trick Twitch Bot

like Rick, but on Twitch.

## Features

- **Token Detection**: Automatically detects Solana, Ethereum/BSC, and symbol-based tokens
- **Real-time Data**: Fetches live market data from DexScreener API
- **Rich Formatting**: Clean, emoji-rich token information display

## Supported Token Formats

- **Solana**: `$GKbUiHTjJ4DxzL4LX9P4evhtsPsBHgUxtr4Da8HMpump`
- **Ethereum/BSC**: `$0x8577cf66262d8d7bbc713f8c96fdf5871baf4444`

## Commands
- `$dexpaid <token>` - Checks if a token profile was paid on DexScreener
- `$help` - Shows a list of available commands and their descriptions

### DexPaid Command Examples

```
$dexpaid 4QUUhKUnG9jDdACyVQgmYmATfH2Eo6cTwK4rMQXApump
$dexpaid 0xA0b86a33E6441b8C4C8C0C4C0C4C0C4C0C4C0C4C0
```

**Response:**
```
🔍 DexScreener Payment Status for 4QUUhKUnG9jDdACyVQgmYmATfH2Eo6cTwK4rMQXApump: ✅ PAID
```

## Setup

1. **Get your Twitch OAuth Token**:
   - Visit [TwitchTokenGenerator.com](https://twitchtokengenerator.com/)
   - Select "Bot Chat Token" 
   - Choose the required scopes: `chat:read` and `chat:edit`
   - Click "Generate Token!" and authorize with Twitch
   - Copy the generated OAuth token

2. **Configure Twitch credentials** in `appsettings.json`:
   ```json
   {
     "TwitchBot": {
       "BotUsername": "your bot",
       "OAuthToken": "your oauth",
       "ChannelName": "channel(s) you want trick to join [channel1, channel2]",
       "CommandPrefix": "your prefix (! by default)",
       "LogLevel": "Information"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     }
   }
   ```

3. **Set up bot permissions**:
   - Make sure your bot account is **moderated** in the channels you want it to join
   - The bot needs moderator permissions to read and send messages in chat
   - Add your bot as a moderator in each channel: `/mod your_bot_username`

4. **Run the bot**:
   ```bash
   dotnet restore
   dotnet run
   ```

## Example Output

**Solana Token:**
```
🚀 Friday Night [$78.5K/+2.8%] FRIDAY/SOL 📈 SOLANA @ pumpswap 
💰 USD: $0.00007852 💎 FDV: $78.5K 📊 Vol: $77.5K ⏰ Age: 2m 
📈 24H: +2.8% 📈 1H: +2.8% 🔗 Contract: 14q7Wz9gNoezKn8Gwj5GwgNiZqeYdg3AySYgz8uqpump 
🗓️ Updated: 09:02 UTC 
axiom: https://axiom.trade/meme/[pairAddress] | gmgn: https://gmgn.ai/sol/token/[token]
```

**Ethereum Token:**
```
🚀 TokenName [$1.2M/+5.2%] TOKEN/ETH 📈 ETHEREUM @ uniswap 
💰 USD: $0.001234 💎 FDV: $1.2M 📊 Vol: $500K ⏰ Age: 1d 
📈 24H: +5.2% 📈 1H: +1.5% 🔗 Contract: 0x1234567890abcdef1234567890abcdef12345678 
🗓️ Updated: 09:02 UTC 
gmgn: https://gmgn.ai/eth/token/[token]
```

**BSC Token:**
```
🚀 TokenName [$500K/-3.1%] TOKEN/BNB 📉 BSC @ pancakeswap 
💰 USD: $0.000567 💎 FDV: $500K 📊 Vol: $200K ⏰ Age: 3d 
📈 24H: -3.1% 📈 1H: -0.8% 🔗 Contract: 0xabcdef1234567890abcdef1234567890abcdef12 
🗓️ Updated: 09:02 UTC 
gmgn: https://gmgn.ai/bsc/token/[token]
```

## Tech Stack

- **.NET 8**
- **TwitchLib.Client**
- **DexScreener API**




