const SESSION_MESSAGE_LIMIT = 15;
const IP_COOLDOWN_SECONDS = 5;

/**
 * Check if a session has exceeded the message limit.
 * Returns { allowed: boolean, count: number }
 */
export async function checkSessionLimit(env, sessionId) {
  const key = `session:${sessionId}`;
  const raw = await env.RATE_LIMIT.get(key);
  const count = raw ? parseInt(raw, 10) : 0;
  return { allowed: count < SESSION_MESSAGE_LIMIT, count };
}

/**
 * Increment the session message counter.
 */
export async function incrementSessionCount(env, sessionId) {
  const key = `session:${sessionId}`;
  const raw = await env.RATE_LIMIT.get(key);
  const count = raw ? parseInt(raw, 10) : 0;
  // Expire after 24 hours so stale sessions don't persist forever
  await env.RATE_LIMIT.put(key, String(count + 1), { expirationTtl: 86400 });
}

/**
 * Check whether a session has ever sent a message through /api/chat.
 * Used, alongside the leads-scoped IP cooldown, to reject lead submissions
 * for session IDs /api/chat never saw. Workers KV is eventually consistent,
 * so a lead submitted immediately after a session's first chat message could
 * see a stale read here; in practice the time a user spends typing their
 * name/email is enough for the write to propagate.
 */
export async function sessionExists(env, sessionId) {
  const key = `session:${sessionId}`;
  const raw = await env.RATE_LIMIT.get(key);
  return raw !== null;
}

/**
 * Check whether a session has already submitted a lead. Prevents a single
 * harvested sessionId from being replayed to submit unlimited leads across
 * different IPs, since each IP gets its own independent cooldown.
 */
export async function hasSubmittedLead(env, sessionId) {
  const key = `lead-submitted:${sessionId}`;
  const raw = await env.RATE_LIMIT.get(key);
  return raw !== null;
}

/**
 * Mark a session as having submitted a lead.
 */
export async function markLeadSubmitted(env, sessionId) {
  const key = `lead-submitted:${sessionId}`;
  // Same 24-hour window as the session's own message-count TTL.
  await env.RATE_LIMIT.put(key, "1", { expirationTtl: 86400 });
}

/**
 * Check if an IP is sending requests too quickly (1 req per 2 seconds).
 * `scope` namespaces the cooldown per endpoint so a chat request and a
 * lead submission from the same IP don't contend for the same window.
 * Returns { allowed: boolean }
 */
export async function checkIpRate(env, ip, scope = "chat") {
  const key = `ip:${scope}:${ip}`;
  const lastRequest = await env.RATE_LIMIT.get(key);
  if (lastRequest) {
    const elapsed = Date.now() - parseInt(lastRequest, 10);
    if (elapsed < IP_COOLDOWN_SECONDS * 1000) {
      return { allowed: false };
    }
  }
  await env.RATE_LIMIT.put(key, String(Date.now()), { expirationTtl: 60 });
  return { allowed: true };
}
