# CGCA Chatbot — Cloudflare Worker

Backend API proxy for the CGCA AI chatbot. Handles chat requests via the Anthropic Claude API and stores lead capture data.

## Prerequisites

- Cloudflare account (free tier is fine)
- Anthropic API key
- Node.js 18+ installed

## Setup

```bash
npm install

# Authenticate with Cloudflare
npx wrangler login

# Create KV namespaces and update wrangler.toml with the returned IDs
npx wrangler kv:namespace create CHAT_CACHE
npx wrangler kv:namespace create RATE_LIMIT

# Store the Anthropic API key as a secret (never hardcode it)
npx wrangler secret put ANTHROPIC_API_KEY
```

After creating KV namespaces, update the `id` fields in `wrangler.toml` with the IDs printed by wrangler.

## Deploy

```bash
npx wrangler deploy
```

## Local Development

```bash
npx wrangler dev
```

## Cost Controls

- **Model**: Always uses `claude-haiku-4-5-20251001` (cheapest option)
- **Token cap**: 350 max tokens per response
- **History trimming**: Only the last 6 messages are sent to the API
- **Session cap**: 15 messages per session ID
- **Response caching**: Common questions are cached in KV for 24 hours

**IMPORTANT**: Set a monthly spend cap of $30 in the Anthropic console:
https://console.anthropic.com/settings/limits

## Endpoints

- `POST /api/chat` — Send chat messages, returns AI response
- `POST /api/leads` — Submit lead contact info (name, email)

## Retrieving Leads from KV

```bash
npx wrangler kv:key list --binding CHAT_CACHE --prefix "lead:"
```
