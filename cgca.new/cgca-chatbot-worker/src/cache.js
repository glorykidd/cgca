const CACHE_TTL = 86400; // 24 hours

// Common questions mapped to normalized keys for cache lookup
const COMMON_QUESTIONS = [
  "what are school hours",
  "how do i apply",
  "how do i enroll",
  "what grades do you offer",
  "what is tuition",
  "how much does it cost",
  "where is the school located",
  "what is the address",
  "how do i contact the school",
  "what is the phone number",
];

/**
 * Normalize a user message for cache key matching.
 * Strips punctuation, lowercases, and trims.
 */
function normalize(text) {
  return text
    .toLowerCase()
    .replace(/[^a-z0-9\s]/g, "")
    .replace(/\s+/g, " ")
    .trim();
}

/**
 * Check if a message matches a common question and return a cached response.
 * Returns the cached reply string, or null if not cached.
 */
export async function getCachedResponse(env, userMessage) {
  const normalized = normalize(userMessage);
  const match = COMMON_QUESTIONS.find(
    (q) => normalized.includes(q) || q.includes(normalized)
  );
  if (!match) return null;

  const key = `cache:${match}`;
  return await env.CHAT_CACHE.get(key);
}

/**
 * Store a response in the cache if the user message matches a common question.
 */
export async function maybeCacheResponse(env, userMessage, reply) {
  const normalized = normalize(userMessage);
  const match = COMMON_QUESTIONS.find(
    (q) => normalized.includes(q) || q.includes(normalized)
  );
  if (!match) return;

  const key = `cache:${match}`;
  await env.CHAT_CACHE.put(key, reply, { expirationTtl: CACHE_TTL });
}
