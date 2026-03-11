# CGCA Chatbot — Cloudflare Worker Setup Instructions

## Prerequisites

- A [Cloudflare account](https://dash.cloudflare.com/sign-up) (free tier is fine)
- An [Anthropic API key](https://console.anthropic.com/settings/keys)
- Node.js 18+ installed on your machine

---

## Step 1 — Install Dependencies

From the project root, navigate to the worker directory and install packages:

```bash
cd cgca.new/cgca-chatbot-worker
npm install
```

This installs `wrangler`, the Cloudflare CLI tool.

---

## Step 2 — Authenticate with Cloudflare

```bash
npx wrangler login
```

This opens a browser window. Sign in to your Cloudflare account and authorize Wrangler. Once complete, the CLI will confirm you're logged in.

---

## Step 3 — Create KV Namespaces

The worker uses two KV (Key-Value) namespaces for rate limiting and response caching. Create them:

```bash
npx wrangler kv:namespace create CHAT_CACHE
npx wrangler kv:namespace create RATE_LIMIT
```

Each command will output something like:

```
Add the following to your configuration file in your kv_namespaces array:
{ binding = "CHAT_CACHE", id = "abc123def456..." }
```

**Copy the `id` values** and update `wrangler.toml`:

```toml
[[kv_namespaces]]
binding = "CHAT_CACHE"
id = "PASTE_CHAT_CACHE_ID_HERE"

[[kv_namespaces]]
binding = "RATE_LIMIT"
id = "PASTE_RATE_LIMIT_ID_HERE"
```

---

## Step 4 — Set Up Google Sheets for Lead Capture

Leads collected by the chatbot are sent to a Google Sheet via a Google Apps Script webhook. Follow these steps to set it up.

### 4a. Create the Google Sheet

1. Go to [Google Sheets](https://sheets.google.com) and create a new spreadsheet
2. Name it something like **CGCA Chatbot Leads**
3. In **Row 1**, add these column headers:
   - A1: `Timestamp`
   - B1: `Name`
   - C1: `Email`
   - D1: `Session ID`

### 4b. Create the Apps Script Webhook

1. In your spreadsheet, go to **Extensions** > **Apps Script**
2. Delete any code in the editor and paste the following:

```javascript
function doPost(e) {
  var sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  var data = JSON.parse(e.postData.contents);

  sheet.appendRow([
    data.timestamp || new Date().toISOString(),
    data.name || "",
    data.email || "",
    data.sessionId || ""
  ]);

  return ContentService
    .createTextOutput(JSON.stringify({ success: true }))
    .setMimeType(ContentService.MimeType.JSON);
}
```

3. Click **Save** (name the project something like "CGCA Lead Webhook")
4. Click **Deploy** > **New deployment**
5. Click the gear icon next to **Select type** and choose **Web app**
6. Set:
   - **Description**: CGCA Chatbot Lead Capture
   - **Execute as**: Me
   - **Who has access**: Anyone
7. Click **Deploy**
8. Authorize the app when prompted (review permissions and allow)
9. **Copy the Web app URL** — it will look like:
   ```
   https://script.google.com/macros/s/AKfycb.../exec
   ```

### 4c. Store the Webhook URL in Cloudflare

```bash
npx wrangler secret put GOOGLE_SHEETS_WEBHOOK_URL
```

Paste the Apps Script Web app URL when prompted.

> **Note**: If the Google Sheets webhook is not configured or temporarily unavailable, the worker automatically falls back to storing leads in Cloudflare KV so no data is lost.

---

## Step 5 — Store the Anthropic API Key

Never hardcode the API key. Store it as an encrypted secret:

```bash
npx wrangler secret put ANTHROPIC_API_KEY
```

You'll be prompted to paste your key. It will not be echoed to the terminal.

---

## Step 6 — Deploy the Worker

```bash
npx wrangler deploy
```

Wrangler will output the live URL, something like:

```
Published cgca-chatbot (1.0.0)
  https://cgca-chatbot.<your-account>.workers.dev
```

**Save this URL** — you'll need it for the next step.

---

## Step 7 — Configure the Blazor App

Open `cgca.new/cgca.web/wwwroot/appsettings.json` and replace the placeholder URL with your Worker URL:

```json
{
  "ChatApiBaseUrl": "https://cgca-chatbot.<your-account>.workers.dev"
}
```

---

## Step 8 — Set the Anthropic Spend Cap

Go to the Anthropic usage limits page and set a monthly spend cap of $30 (or your preferred limit):

https://console.anthropic.com/settings/limits

This prevents unexpected charges if the chatbot sees high traffic.

---

## Step 9 — (Optional) Custom Domain

Instead of using the `workers.dev` subdomain, you can route the worker through a custom subdomain like `chat-api.cedargrovechristianacademy.org`.

1. In the [Cloudflare dashboard](https://dash.cloudflare.com), go to **Workers & Pages** > **cgca-chatbot** > **Settings** > **Domains & Routes**
2. Click **Add** > **Custom Domain**
3. Enter `chat-api.cedargrovechristianacademy.org`
4. Cloudflare will automatically create the DNS record

If you do this, update `appsettings.json` accordingly:

```json
{
  "ChatApiBaseUrl": "https://chat-api.cedargrovechristianacademy.org"
}
```

**Note**: The domain `cedargrovechristianacademy.org` must be managed by Cloudflare DNS for custom domains to work. If your DNS is elsewhere, stick with the `workers.dev` URL and ensure the CORS origins in `src/index.js` include it.

---

## Verifying the Deployment

Test the worker with a quick curl command:

```bash
curl -X POST https://cgca-chatbot.<your-account>.workers.dev/api/chat \
  -H "Content-Type: application/json" \
  -d '{"messages":[{"role":"user","content":"What grades do you offer?"}],"sessionId":"test-123"}'
```

You should get a JSON response with a `reply` field from the chatbot.

---

## Ongoing Management

### View logs in real time
```bash
npx wrangler tail
```

### Update the worker after code changes
```bash
npx wrangler deploy
```

### Rotate the API key
```bash
npx wrangler secret put ANTHROPIC_API_KEY
```

### View leads
Leads go directly to your Google Sheet. Open the spreadsheet to see them.

If any leads fell back to KV storage (Google Sheets was temporarily unavailable), retrieve them with:
```bash
npx wrangler kv:key list --binding CHAT_CACHE --prefix "lead:"
```

To read a specific fallback lead:
```bash
npx wrangler kv:key get --binding CHAT_CACHE "lead:<key-name>"
```

### Update the Google Sheets webhook URL
If you redeploy the Apps Script (new URL), update the secret:
```bash
npx wrangler secret put GOOGLE_SHEETS_WEBHOOK_URL
```

### Monitor costs
- **Anthropic usage**: https://console.anthropic.com/settings/usage
- **Cloudflare Workers usage**: https://dash.cloudflare.com > Workers & Pages > Overview (free tier includes 100,000 requests/day)
