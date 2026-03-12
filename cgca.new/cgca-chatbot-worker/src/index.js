import { SYSTEM_PROMPT } from "./systemPrompt.js";
import { checkSessionLimit, incrementSessionCount, checkIpRate } from "./rateLimit.js";
import { getCachedResponse, maybeCacheResponse } from "./cache.js";

const ALLOWED_ORIGINS = [
  "https://cedargrovechristianacademy.org",
  "https://www.cedargrovechristianacademy.org",
  "http://localhost:5297",
  "https://localhost:7183",
];

const MODEL = "claude-haiku-4-5-20251001";
const MAX_TOKENS = 350;
const MAX_HISTORY = 6;

function getCorsHeaders(request) {
  const origin = request.headers.get("Origin") || "";
  const allowed = ALLOWED_ORIGINS.includes(origin) ? origin : "";
  return {
    "Access-Control-Allow-Origin": allowed,
    "Access-Control-Allow-Methods": "POST, OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type",
    "Access-Control-Max-Age": "86400",
  };
}

function jsonResponse(data, status, corsHeaders) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { "Content-Type": "application/json", ...corsHeaders },
  });
}

/**
 * Handle POST /api/chat — proxy to Anthropic Claude API.
 */
async function handleChat(request, env) {
  const cors = getCorsHeaders(request);
  const ip = request.headers.get("CF-Connecting-IP") || "unknown";

  // Rate limit by IP
  const ipCheck = await checkIpRate(env, ip);
  if (!ipCheck.allowed) {
    return jsonResponse(
      { reply: "You're sending messages too quickly. Please wait a moment and try again." },
      429,
      cors
    );
  }

  let body;
  try {
    body = await request.json();
  } catch {
    return jsonResponse({ reply: "Invalid request." }, 400, cors);
  }

  const { messages, sessionId } = body;
  if (!sessionId || !Array.isArray(messages) || messages.length === 0) {
    return jsonResponse({ reply: "Invalid request format." }, 400, cors);
  }

  // Session message cap
  const sessionCheck = await checkSessionLimit(env, sessionId);
  if (!sessionCheck.allowed) {
    return jsonResponse(
      {
        reply: "We've reached the limit for this chat session. Please contact our admissions office directly at contactus@cedargrovechristianacademy.org for further help!",
        sessionId,
      },
      200,
      cors
    );
  }

  // Check cache for the latest user message
  const lastUserMessage = messages[messages.length - 1]?.content || "";
  const cached = await getCachedResponse(env, lastUserMessage);
  if (cached) {
    await incrementSessionCount(env, sessionId);
    return jsonResponse({ reply: cached, sessionId }, 200, cors);
  }

  // Trim history to last N messages
  const trimmedMessages = messages.slice(-MAX_HISTORY);

  // Call Anthropic API
  let apiResponse;
  try {
    apiResponse = await fetch("https://api.anthropic.com/v1/messages", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "x-api-key": env.ANTHROPIC_API_KEY,
        "anthropic-version": "2023-06-01",
      },
      body: JSON.stringify({
        model: MODEL,
        max_tokens: MAX_TOKENS,
        system: SYSTEM_PROMPT,
        messages: trimmedMessages,
      }),
    });
  } catch {
    return jsonResponse(
      { reply: "Sorry, I'm having trouble right now. Please contact us directly at contactus@cedargrovechristianacademy.org." },
      502,
      cors
    );
  }

  if (!apiResponse.ok) {
    return jsonResponse(
      { reply: "Sorry, I'm having trouble right now. Please contact us directly at contactus@cedargrovechristianacademy.org." },
      502,
      cors
    );
  }

  const result = await apiResponse.json();
  const reply = result.content?.[0]?.text || "I'm sorry, I couldn't generate a response.";

  // Cache the response if it matches a common question
  await maybeCacheResponse(env, lastUserMessage, reply);
  await incrementSessionCount(env, sessionId);

  return jsonResponse({ reply, sessionId }, 200, cors);
}

/**
 * Handle POST /api/leads — forward lead info to Google Sheets via Apps Script.
 * Falls back to KV storage if the Google Sheets webhook is not configured or fails.
 */
async function handleLeads(request, env) {
  const cors = getCorsHeaders(request);

  let body;
  try {
    body = await request.json();
  } catch {
    return jsonResponse({ success: false }, 400, cors);
  }

  const { name, email, sessionId } = body;
  if (!name || !email) {
    return jsonResponse({ success: false, error: "Name and email are required." }, 400, cors);
  }

  const timestamp = new Date().toISOString();
  const leadData = { name, email, sessionId, timestamp };

  // Forward to Google Sheets via Apps Script webhook
  if (env.GOOGLE_SHEETS_WEBHOOK_URL) {
    try {
      const sheetResponse = await fetch(env.GOOGLE_SHEETS_WEBHOOK_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(leadData),
      });
      if (sheetResponse.ok) {
        return jsonResponse({ success: true }, 200, cors);
      }
    } catch {
      // Fall through to KV backup
    }
  }

  // Fallback: store in KV if Google Sheets is unavailable or not configured
  const leadKey = `lead:${sessionId || "unknown"}:${timestamp}`;
  await env.CHAT_CACHE.put(leadKey, JSON.stringify(leadData), { expirationTtl: 7776000 });

  return jsonResponse({ success: true }, 200, cors);
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    const cors = getCorsHeaders(request);

    // Handle CORS preflight
    if (request.method === "OPTIONS") {
      return new Response(null, { status: 204, headers: cors });
    }

    try {
      if (request.method !== "POST") {
        return jsonResponse({ error: "Method not allowed" }, 405, cors);
      }

      if (url.pathname === "/api/chat") {
        return await handleChat(request, env);
      }

      if (url.pathname === "/api/leads") {
        return await handleLeads(request, env);
      }

      return jsonResponse({ error: "Not found" }, 404, cors);
    } catch (err) {
      return jsonResponse(
        { reply: "Sorry, I'm having trouble right now. Please contact us directly at contactus@cedargrovechristianacademy.org." },
        500,
        cors
      );
    }
  },
};
