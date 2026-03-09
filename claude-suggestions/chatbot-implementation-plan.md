# CGCA AI Chatbot — Adjusted Implementation Plan for Blazor WASM

> Adapted from the original plan to fit within the existing Blazor WebAssembly application.

---

## Key Differences from Original Plan

The original plan assumed a vanilla JS/HTML site with a drop-in `<script>` widget. This project is a **Blazor WebAssembly** client-side app (.NET 10.0). The adjustments are:

| Original Plan | Adjusted Plan |
|---|---|
| Vanilla JS widget (`cgca-chatbot.js`) | Blazor Razor component (`ChatWidget.razor`) |
| Inline styles injected via JS | Scoped CSS (`ChatWidget.razor.css`) using existing CGCA design tokens |
| Script tag in HTML pages | Component added to `MainLayout.razor` (appears on all pages) |
| UUID v4 via JS `crypto.randomUUID()` | `Guid.NewGuid()` in C# |
| `fetch()` to Worker API | Injected `HttpClient` service via Blazor DI |
| Separate `cgca-chatbot/` project folder | Worker code lives in `cgca.new/cgca-chatbot-worker/`; frontend is part of `cgca.web` |
| Standalone `.gitignore` | Use existing repo `.gitignore` |

---

## Phase 1 — Cloudflare Worker (Backend Proxy)

**No major changes from the original plan.** The Worker is a separate deployment from the Blazor app and remains a standalone Cloudflare Worker project.

Create the Worker project at `cgca.new/cgca-chatbot-worker/`.

### Adjustments:
- **CORS origins**: Allow `https://cedargrovechristianacademy.org`, `https://www.cedargrovechristianacademy.org`, and `http://localhost:5297` / `https://localhost:7183` (dev server ports from `launchSettings.json`)
- **Endpoint base URL**: The Worker will be deployed to a Cloudflare Workers subdomain (e.g., `cgca-chatbot.<account>.workers.dev`) or a custom subdomain like `chat-api.cedargrovechristianacademy.org`. The Blazor app will call this external URL, not a relative `/api/chat` path.
- **No changes** to rate limiting, session caps, history trimming, or Anthropic API settings.

### File structure:
```
cgca.new/cgca-chatbot-worker/
  ├── src/
  │   ├── index.js          # Main Worker (chat + leads handlers)
  │   ├── systemPrompt.js   # Exports the CGCA system prompt string
  │   ├── rateLimit.js      # Rate limiting helpers using Cloudflare KV
  │   └── cache.js          # Response caching helpers
  ├── wrangler.toml          # Cloudflare Worker config
  ├── package.json
  └── README.md              # Worker-specific setup instructions
```

### wrangler.toml notes:
```toml
name = "cgca-chatbot"
main = "src/index.js"
compatibility_date = "2024-01-01"

[[kv_namespaces]]
binding = "CHAT_CACHE"
id = "<created-via-wrangler>"

[[kv_namespaces]]
binding = "RATE_LIMIT"
id = "<created-via-wrangler>"
```

---

## Phase 2 — Frontend Chat Widget (Blazor Component)

**This is the biggest change.** Replace the vanilla JS widget with a Razor component.

### New files:
```
cgca.new/cgca.web/
  ├── Components/
  │   ├── ChatWidget.razor        # Chat UI component
  │   └── ChatWidget.razor.css    # Scoped styles
  ├── Services/
  │   ├── IChatService.cs         # Interface for chat API calls
  │   └── ChatService.cs          # HttpClient-based implementation
  └── Models/
      ├── ChatMessage.cs          # { Role, Content } record
      ├── ChatRequest.cs          # { Messages, SessionId }
      └── ChatResponse.cs         # { Reply, SessionId }
```

### ChatWidget.razor — Component Design:

1. **Placement**: Add `<ChatWidget />` to `Layout/MainLayout.razor` just before the closing `</footer>` or after it, so it renders on every page.

2. **State management**: All conversation state held in component fields (no localStorage, no static state):
   - `List<ChatMessage> messages` — conversation history
   - `string sessionId` — generated via `Guid.NewGuid().ToString()` on component init
   - `int messageCount` — tracks messages for lead capture trigger and session cap
   - `bool isOpen` — controls panel visibility
   - `bool isLoading` — shows typing indicator
   - `bool leadCaptured` — prevents re-asking for contact info

3. **Chat flow**:
   - Floating button (bottom-right, fixed position) toggles `isOpen`
   - Welcome message shown on open (not counted as a message)
   - User types message → appends to UI → calls `ChatService.SendAsync()` → appends reply
   - After user's 2nd message, if `!leadCaptured`, the assistant response will include the lead capture prompt (driven by system prompt, not frontend logic — see Phase 3 adjustment)

4. **Lead capture**: When the assistant's response contains contact info (name + email pattern detected), POST to the leads endpoint via `ChatService.SubmitLeadAsync()`. Alternatively, keep it simpler: the system prompt handles asking, and we add a small "Share your info" form that appears in the chat after message 2 — a Blazor `<EditForm>` inline in the chat thread.

5. **Error handling**: On `HttpRequestException` or non-success status, display the fallback message with a link to `contactus@cedargrovechristianacademy.org`.

6. **Accessibility**: `aria-label` on the toggle button, chat panel, input field. `Enter` to send, `Escape` to close (via `@onkeydown`).

### Service Registration:

In `Program.cs`, add:
```csharp
builder.Services.AddScoped<IChatService, ChatService>();
```

The `ChatService` will need the Worker URL configured. Options:
- **Option A (simple)**: Hardcode the Worker URL in `ChatService.cs` or `appsettings.json` loaded via `IConfiguration`
- **Option B**: Add a `wwwroot/appsettings.json` with `{ "ChatApiBaseUrl": "https://cgca-chatbot.<account>.workers.dev" }` and inject `IConfiguration`

### Styling approach:

Use `ChatWidget.razor.css` (scoped CSS) with the existing CGCA design tokens:
- `--cgca-primary-blue: #0441f7` for the chat button and header
- `--cgca-accent-green: #06ce17b0` for the send button
- `--cgca-footer-bg: #333333` as a dark background option
- Bootstrap 5 utility classes for layout (already loaded via CDN)
- Mobile-responsive: full-width panel on screens < 576px, fixed 380px panel on desktop

---

## Phase 3 — System Prompt

**Minor adjustments.** The system prompt content is good as-is, but:

1. **Store in the Worker**, not in the Blazor app (the frontend should never see the system prompt).
2. **Adjust lead capture instructions**: Instead of relying on the frontend to detect the 2nd message, include in the system prompt:

```
LEAD COLLECTION:
- After the user has sent 2 messages, gently offer to connect them with the admissions team.
- If they're interested, ask for their first name, last name, and email.
- Format collected info as: [LEAD: firstName, lastName, email]
- Confirm their info was received and tell them someone will be in touch shortly.
```

The Worker can parse `[LEAD: ...]` from responses and auto-forward to the leads endpoint before stripping the tag from the response sent to the client. This keeps lead capture server-side and reliable.

**Alternative (simpler)**: Keep lead capture entirely in the frontend Blazor component — show a small inline form after message 2. This avoids complex parsing and is more predictable.

---

## Phase 4 — Lead Capture Endpoint

**No major changes.** The `/api/leads` handler in the Worker stays the same.

### Recommendation for CGCA's setup:
Given the existing infrastructure (self-hosted Windows runner, SesMailer for deploy notifications), **Option B (email notification)** is the natural fit:
- The Worker POSTs lead data to an email service (Amazon SES is already in use for deploy notifications — could reuse that, or use Resend/SendGrid free tier from the Worker directly)
- Alternatively, **Option C (Cloudflare KV storage)** as a simple first pass — leads stored in KV, retrievable via a Wrangler CLI command or a simple admin endpoint

---

## Phase 5 — Cost Controls

**No changes needed.** All cost controls are Worker-side and remain as specified:

1. Model: `claude-haiku-4-5-20251001`
2. History trimming: last 6 messages
3. Token cap: `max_tokens: 350`
4. Session cap: 15 messages per sessionId (KV counter)
5. Response caching: common questions cached in KV with 24h TTL
6. Anthropic spend cap: $30/month (set in Anthropic console)

---

## Phase 6 — Adjusted File Structure

```
cgca.new/
├── cgca.web/                          # Existing Blazor WASM app
│   ├── Components/
│   │   ├── ChatWidget.razor           # Chat UI component
│   │   └── ChatWidget.razor.css       # Scoped styles
│   ├── Services/
│   │   ├── IChatService.cs            # Chat service interface
│   │   └── ChatService.cs             # HTTP implementation
│   ├── Models/
│   │   ├── ChatMessage.cs             # Message model
│   │   ├── ChatRequest.cs             # API request model
│   │   └── ChatResponse.cs            # API response model
│   ├── Layout/
│   │   └── MainLayout.razor           # (modified) Add <ChatWidget />
│   ├── Program.cs                     # (modified) Register ChatService
│   └── wwwroot/
│       └── appsettings.json           # (new) ChatApiBaseUrl config
│
├── cgca-chatbot-worker/               # New Cloudflare Worker project
│   ├── src/
│   │   ├── index.js
│   │   ├── systemPrompt.js
│   │   ├── rateLimit.js
│   │   └── cache.js
│   ├── wrangler.toml
│   ├── package.json
│   └── README.md
│
└── cgca.sln                           # Existing solution (no changes needed)
```

---

## Phase 7 — Deployment & CI/CD

### Worker deployment (manual, separate from the Blazor CI/CD):
```bash
cd cgca.new/cgca-chatbot-worker
npm install
npx wrangler login
npx wrangler secret put ANTHROPIC_API_KEY
npx wrangler kv:namespace create CHAT_CACHE
npx wrangler kv:namespace create RATE_LIMIT
npx wrangler deploy
```

### Blazor app deployment:
- No changes to the existing `.github/workflows/cgca.yml` pipeline
- The `ChatWidget` component is part of the Blazor app build — `dotnet publish` includes it automatically
- The only new config is `wwwroot/appsettings.json` with the Worker URL

### DNS (optional but recommended):
Set up a CNAME record `chat-api.cedargrovechristianacademy.org` pointing to the Cloudflare Worker's `workers.dev` subdomain. This avoids CORS issues with a third-party domain and looks more professional.

---

## Phase 8 — README Updates

Add a `## Chatbot` section to the existing project README (or the Worker's own README) covering:

1. Architecture overview (Blazor component + Cloudflare Worker proxy)
2. Worker deployment steps
3. How to update the system prompt
4. How to update school info / FAQ cache entries
5. Monitoring usage in the Anthropic console
6. Setting the $30/month spend cap
7. How to retrieve leads from KV (if using Option C)

---

## Acceptance Criteria (Adjusted)

- [ ] `ChatWidget` renders on all pages via `MainLayout.razor`
- [ ] Floating button appears bottom-right, opens/closes chat panel
- [ ] Messages send and receive correctly via the Cloudflare Worker
- [ ] System prompt is applied server-side to every conversation
- [ ] Rate limiting blocks after 15 messages per session
- [ ] Conversation history is trimmed to last 6 messages (server-side)
- [ ] Lead capture triggers after 2nd message
- [ ] Error states display a friendly fallback with contact email
- [ ] No API key exposed in frontend code (key is in Worker secrets only)
- [ ] Widget is mobile-responsive (full-width on small screens)
- [ ] CORS restricted to cedargrovechristianacademy.org + localhost dev ports
- [ ] Scoped CSS uses existing CGCA design tokens
- [ ] Component integrates with Blazor DI (`IChatService`)
- [ ] `dotnet build` succeeds with no warnings from new code
- [ ] Existing CI/CD pipeline deploys the chatbot with no changes

---

## Implementation Order

1. **Worker** — Build and deploy the Cloudflare Worker first (chat + leads endpoints)
2. **Models & Service** — Add C# models and `ChatService` to the Blazor app
3. **Component** — Build `ChatWidget.razor` with scoped CSS
4. **Integration** — Add to `MainLayout.razor` and register service in `Program.cs`
5. **Lead capture** — Wire up the inline form or server-side parsing
6. **Testing** — Manual end-to-end test with dev server + deployed Worker
7. **Deploy** — Push to `develop`, PR to `main`, existing CI/CD handles the rest
