# 07 — Rate Limiting & Request Validation

**What to build:**
`ChatAgentSend` security rate limiting policy (60 requests/min IP, 20 requests/min per user) and FluentValidation rules for request DTOs.

**Blocked by:** 03 — RAG Pipeline & Message Exchange (`SendMessage`)

**Status:** completed

- [x] Add `ChatAgentSend` policy constant to `RateLimitPolicyNames`
- [x] Add `ChatAgentSend` rate limit policy (60 IP / 20 User per minute) to `SecurityRateLimitPolicies`
- [x] Apply `[SecurityRateLimit(RateLimitPolicyNames.ChatAgentSend)]` to `SendMessageAsync` on `ChatAgentController`
- [x] Write unit tests in `ChatAgentValidatorTests` for `CreateAgentConversationRequestValidator` and `SendAgentMessageRequestValidator`
