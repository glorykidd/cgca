const SESSION_MESSAGE_LIMIT = 15;
const IP_COOLDOWN_SECONDS = 2;

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
 * Check if an IP is sending requests too quickly (1 req per 2 seconds).
 * Returns { allowed: boolean }
 */
export async function checkIpRate(env, ip) {
  const key = `ip:${ip}`;
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
